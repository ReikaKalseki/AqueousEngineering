using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.AqueousEngineering
{
  [QModCore]
  public static class AqueousEngineeringMod {
  	
    public const string MOD_KEY = "ReikaKalseki.AqueousEngineering";
    
    //public static readonly ModLogger logger = new ModLogger();
	public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    public static readonly Config<AEConfig.ConfigEntries> config = new Config<AEConfig.ConfigEntries>();
    
    public static BaseSonarPinger sonarBlock;
    public static BaseCreatureRepellent repellentBlock;
    public static BaseBeacon beaconBlock;
    public static AmpeelAntenna ampeelAntennaBlock;
    public static ACUCleaner acuCleanerBlock;
    public static Autofarmer farmerBlock;
    public static RemoteCameraAntenna cameraAntennaBlock;
    
    public static OutdoorPot outdoorBasicPot;
    public static OutdoorPot outdoorChicPot;
    public static OutdoorPot outdoorCompositePot;
    
    public static MiniPoo poo;
    
    public static readonly XMLLocale locale = new XMLLocale("XML/locale.xml");

    [QModPrePatch]
    public static void PreLoad() {
        config.load();
    }

    [QModPatch]
    public static void Load() {        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(modDLL);
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        CustomPrefab.addPrefabNamespace("ReikaKalseki.AqueousEngineering");
        
        locale.load();
        
        createEgg(TechType.SpineEel, TechType.BonesharkEgg, 1, "SpineEelDesc", true, 0.5F, BiomeType.BonesField_Ground, BiomeType.LostRiverJunction_Ground);
        createEgg(TechType.GhostRayBlue, TechType.JumperEgg, 1.75F, "GhostRayDesc", true, 1, BiomeType.TreeCove_LakeFloor);
        createEgg(TechType.GhostRayRed, TechType.CrabsnakeEgg, 1.25F, "CrimsonRayDesc", true, 1, BiomeType.InactiveLavaZone_Chamber_Floor_Far);
        createEgg(TechType.Biter, TechType.RabbitrayEgg, 1F, "BiterDesc", false, 1, BiomeType.GrassyPlateaus_CaveFloor, BiomeType.Mountains_CaveFloor);
        createEgg(TechType.Blighter, TechType.RabbitrayEgg, 1F, "BlighterDesc", false, 1, BiomeType.BloodKelp_CaveFloor);
        
        poo = new MiniPoo(locale.getEntry("MiniPoop"));
	    poo.Patch();
	    
	    sonarBlock = createMachine<BaseSonarPinger, BaseSonarPingerLogic>("SeabaseSonar");
	    repellentBlock = createMachine<BaseCreatureRepellent, BaseCreatureRepellentLogic>("SeabaseRepellent");
	    beaconBlock = createMachine<BaseBeacon, BaseBeaconLogic>("SeabaseBeacon");
	    ampeelAntennaBlock = createMachine<AmpeelAntenna, AmpeelAntennaLogic>("SeabaseAmpeelAntenna");
	    farmerBlock = createMachine<Autofarmer, AutofarmerLogic>("SeabaseFarmer");
	    acuCleanerBlock = createMachine<ACUCleaner, ACUCleanerLogic>("SeabaseACUCleaner");
	    cameraAntennaBlock = createMachine<RemoteCameraAntenna, RemoteCameraAntennaLogic>("SeabaseRemoteCamera");
        
        outdoorBasicPot = new OutdoorPot(TechType.PlanterPot);
        outdoorCompositePot = new OutdoorPot(TechType.PlanterPot2);
        outdoorChicPot = new OutdoorPot(TechType.PlanterPot3);
        outdoorBasicPot.register();
        outdoorCompositePot.register();
        outdoorChicPot.register();
                 
        new WorldgenDatabase().load();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(AEHooks).TypeHandle);
        
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Shocker, ampeelAntennaBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Beacon, beaconBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(poo.TechType, acuCleanerBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseMapRoom, cameraAntennaBlock.TechType);
    }
    
    private static M createMachine<M, N>(string lck, TechnologyFragment[] frags = null) where N : CustomMachineLogic where M : CustomMachine<N> {
    	M m = (M)Activator.CreateInstance(typeof(M), locale.getEntry(lck));
        m.Patch();
        if (frags != null)
        	m.addFragments(frags.Length, 4F, frags);
        SNUtil.log("Registered custom machine "+m);
        return m;
    }
    
    [QModPostPatch]
    public static void PostLoad() {
		Spawnable azurite = ItemRegistry.instance.getItem("VENT_CRYSTAL");
		if (azurite != null)
			RecipeUtil.addIngredient(repellentBlock.TechType, azurite.TechType, 8);
		else
			RecipeUtil.addIngredient(repellentBlock.TechType, TechType.SeamothElectricalDefense, 1);
		
		Spawnable luminol = ItemRegistry.instance.getItem("Luminol");
		if (luminol != null)
			RecipeUtil.addIngredient(sonarBlock.TechType, luminol.TechType, 2);
		else
			RecipeUtil.addIngredient(sonarBlock.TechType, TechType.WiringKit, 1);
		
		Spawnable motor = ItemRegistry.instance.getItem("Motor");
		if (motor != null) {
			RecipeUtil.addIngredient(acuCleanerBlock.TechType, motor.TechType, 1);
			RecipeUtil.addIngredient(farmerBlock.TechType, motor.TechType, 2);
		}
		else {
			RecipeUtil.addIngredient(acuCleanerBlock.TechType, TechType.Lubricant, 2);
			RecipeUtil.addIngredient(farmerBlock.TechType, TechType.Lubricant, 5);
		}
		
		Spawnable sealf = ItemRegistry.instance.getItem("SealFabric");
		if (sealf != null)
			RecipeUtil.addIngredient(ampeelAntennaBlock.TechType, sealf.TechType, 2);
		else
			RecipeUtil.addIngredient(ampeelAntennaBlock.TechType, TechType.Silicone, 3);
    }
    
    private static void createEgg(TechType creature, TechType basis, float scale, string locKey, bool isBig, float rate, params BiomeType[] spawn) {
    	CustomEgg.createAndRegisterEgg(creature, basis, scale, locale.getEntry(locKey).desc, isBig, rate, spawn);
    }

  }
}
