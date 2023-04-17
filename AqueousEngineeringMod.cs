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
    public static ACUBooster acuBoosterBlock;
    public static PlanktonFeeder planktonFeederBlock;
    public static BaseBattery batteryBlock;
    //public static IonCubeBurner ionCubeBlock;
    public static ItemDisplay displayBlock;
    public static BaseDomeLight domeLightBlock;
    
    public static OutdoorPot outdoorBasicPot;
    public static OutdoorPot outdoorChicPot;
    public static OutdoorPot outdoorCompositePot;
    
    public static MiniPoo poo;
    
    public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();
    
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
        
        ModVersionCheck.getFromGitVsInstall("Aqueous Engineering", modDLL, "AqueousEngineering").register();
        
        CustomPrefab.addPrefabNamespace("ReikaKalseki.AqueousEngineering");
        
        locale.load();
        
        poo = new MiniPoo(locale.getEntry("MiniPoop"));
	    poo.Patch();
	    
	    sonarBlock = createMachine<BaseSonarPinger, BaseSonarPingerLogic>("BaseSonar");
	    repellentBlock = createMachine<BaseCreatureRepellent, BaseCreatureRepellentLogic>("BaseRepellent");
	    beaconBlock = createMachine<BaseBeacon, BaseBeaconLogic>("BaseBeacon");
	    ampeelAntennaBlock = createMachine<AmpeelAntenna, AmpeelAntennaLogic>("BaseAmpeelAntenna");
	    farmerBlock = createMachine<Autofarmer, AutofarmerLogic>("BaseFarmer");
	    acuCleanerBlock = createMachine<ACUCleaner, ACUCleanerLogic>("BaseACUCleaner");
	    cameraAntennaBlock = createMachine<RemoteCameraAntenna, RemoteCameraAntennaLogic>("BaseRemoteCamera");
	    batteryBlock = createMachine<BaseBattery, BaseBatteryLogic>("BaseBattery");
	    //ionCubeBlock = createMachine<IonCubeBurner, IonCubeBurnerLogic>("IonCubeBurner");
	    displayBlock = createMachine<ItemDisplay, ItemDisplayLogic>("BaseItemDisplay");
	    domeLightBlock = createMachine<BaseDomeLight, BaseDomeLightLogic>("BaseDomeLight");
        
        outdoorBasicPot = new OutdoorPot(TechType.PlanterPot);
        outdoorCompositePot = new OutdoorPot(TechType.PlanterPot2);
        outdoorChicPot = new OutdoorPot(TechType.PlanterPot3);
        outdoorBasicPot.register();
        outdoorCompositePot.register();
        outdoorChicPot.register();
                 
       	worldgen.load();
       	
       	ACUCallbackSystem.instance.register();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(AEHooks).TypeHandle);
        
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Shocker, ampeelAntennaBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Beacon, beaconBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(poo.TechType, acuCleanerBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseMapRoom, cameraAntennaBlock.TechType);
        
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("debugACU", ACUCallbackSystem.instance.debugACU);
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
		if (luminol != null) { //c2c is loaded
			RecipeUtil.addIngredient(sonarBlock.TechType, luminol.TechType, 2);
			RecipeUtil.removeIngredient(sonarBlock.TechType, TechType.CyclopsSonarModule); //cyclops gates it too late
			RecipeUtil.addIngredient(sonarBlock.TechType, TechType.SeamothSonarModule, 1);
		}
		else {
			RecipeUtil.addIngredient(sonarBlock.TechType, TechType.WiringKit, 1);
		}
		
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
		
		Spawnable bgl = ItemRegistry.instance.getItem("BaseGlass");
		if (bgl != null)
			RecipeUtil.addIngredient(displayBlock.TechType, bgl.TechType, 1);
		else
			RecipeUtil.addIngredient(displayBlock.TechType, TechType.Glass, 1);
		/*
		Spawnable hull = ItemRegistry.instance.getItem("HullPlating");
		if (hull != null)
			RecipeUtil.addIngredient(ionCubeBlock.TechType, sealf.TechType, 4);
		else
			RecipeUtil.addIngredient(ionCubeBlock.TechType, TechType.TitaniumIngot, 3);
		*/
		Spawnable plankton = ItemRegistry.instance.getItem("planktonItem");
		if (plankton != null) {
			SNUtil.log("Found plankton item. Adding compat machinery.");
		    PlanktonFeeder.fuel = (BasicCraftingItem)plankton;
		    ACUBooster.fuel = PlanktonFeeder.fuel;
	    	acuBoosterBlock = createMachine<ACUBooster, ACUBoosterLogic>("BaseACUBooster");
		    planktonFeederBlock = createMachine<PlanktonFeeder, PlanktonFeederLogic>("BasePlanktonFeeder");
		    if (motor != null)
				RecipeUtil.addIngredient(planktonFeederBlock.TechType, motor.TechType, 1);
	        TechType tt = TechType.None;
	        if (TechTypeHandler.TryGetModdedTechType("plankton", out tt))
	        	TechnologyUnlockSystem.instance.addDirectUnlock(tt, planktonFeederBlock.TechType);
        	TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseWaterPark, acuBoosterBlock.TechType);
	        TechnologyUnlockSystem.instance.addDirectUnlock(plankton.TechType, planktonFeederBlock.TechType);
		}
		else {
			SNUtil.log("Plankton item not found.");
		}
		
		Spawnable glowShroom = ItemRegistry.instance.getItem("GLOWSHROOM");
		if (glowShroom != null) {
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(glowShroom, 0.25F, BiomeRegions.RegionType.Other));
		}
		Spawnable lavaShroom = ItemRegistry.instance.getItem("LAVASHROOM");
		if (lavaShroom != null) {
			ACUEcosystems.addFood(new ACUEcosystems.PlantFood(lavaShroom, 0.25F, BiomeRegions.RegionType.LavaZone));
		}
    }

  }
}
