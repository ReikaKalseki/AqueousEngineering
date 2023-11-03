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
    
    public static readonly Config<AEConfig.ConfigEntries> config = new Config<AEConfig.ConfigEntries>(modDLL);
    
    internal static readonly SoundManager.SoundData beepSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "aebeep", "Sounds/beep.ogg", SoundManager.soundMode3D);
		
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
    public static ATPTap atpTapBlock;
    public static BaseStasisTurret stasisBlock;
    public static BaseControlPanel controlsBlock;
    public static BaseDrillableGrinder grinderBlock;
    public static RepairBeacon repairBlock;
    
    public static OutdoorPot outdoorBasicPot;
    public static OutdoorPot outdoorChicPot;
    public static OutdoorPot outdoorCompositePot;
    
    public static MiniPoo poo;
    
    public static HolographicControl seabaseStasisControl;
    public static HolographicControl seabaseSonarControl;
    public static HolographicControl seabaseRepellentControl;
    
    public static TechnologyFragment[] repairBeaconFragments;
    
    public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();
    
    public static readonly XMLLocale locale = new XMLLocale(modDLL, "XML/locale.xml");

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
        SNUtil.checkModHash(modDLL);
        
        CustomPrefab.addPrefabNamespace("ReikaKalseki.AqueousEngineering");
        
        locale.load();
        
        poo = new MiniPoo(locale.getEntry("MiniPoop"));
	    poo.Patch();
		BioReactorHandler.Main.SetBioReactorCharge(poo.TechType, BaseBioReactor.GetCharge(TechType.SeaTreaderPoop)/4);
	    
	    seabaseStasisControl = new HolographicControl("SeabaseStasis", "Fire stasis pulse", fireStasisPulses, btn => machineExists<BaseStasisTurretLogic>(btn));
	    seabaseStasisControl.setIcons("Textures/HoloButtons/StasisButton", 200).Patch();
	    seabaseSonarControl = new HolographicControl("SeabaseSonar", "Toggle sonar", btn => toggleMachines<BaseSonarPingerLogic>(btn), btn => machineExists<BaseSonarPingerLogic>(btn));
	    seabaseSonarControl.setIcons("Textures/HoloButtons/SonarButton", 200).Patch();
	    seabaseRepellentControl = new HolographicControl("SeabaseRepellent", "Toggle repellant pylon", btn => toggleMachines<BaseCreatureRepellentLogic>(btn), btn => machineExists<BaseCreatureRepellentLogic>(btn));
	    seabaseRepellentControl.setIcons("Textures/HoloButtons/RepellentButton", 200).Patch();
	    
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
	    atpTapBlock = createMachine<ATPTap, ATPTapLogic>("BaseATPTap");
	    stasisBlock = createMachine<BaseStasisTurret, BaseStasisTurretLogic>("BaseStasisTurret");
	    controlsBlock = createMachine<BaseControlPanel, BaseControlPanelLogic>("BaseControlPanel");
	    grinderBlock = createMachine<BaseDrillableGrinder, BaseDrillableGrinderLogic>("BaseDrillableGrinder");
	    string[] li = VanillaFlora.MUSHROOM_BUMP.getPrefabs(true, true).ToArray();
	    repairBeaconFragments = new TechnologyFragment[li.Length-2];
	    for (int i = 1; i < li.Length-1; i++) { //only idx 1,2,3 since 0 is rotated and tall and 4 has a light and is just 3 anyway
	    	repairBeaconFragments[i-1] = new TechnologyFragment(li[i], go => {
				ObjectUtil.removeComponent<CoralBlendWhite>(go);
				ObjectUtil.removeComponent<PlantBehaviour>(go);
				ObjectUtil.removeComponent<LiveMixin>(go);
				ObjectUtil.removeComponent<FMOD_StudioEventEmitter>(go);
				ObjectUtil.removeComponent<Pickupable>(go);
				go.EnsureComponent<NaniteFragment>();
				foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
		    		RenderUtil.swapTextures(modDLL, r, "Textures/RepairFragment");
		    		RenderUtil.setGlossiness(r, 1.5F, 0, 0.85F);
		    		RenderUtil.setEmissivity(r, 10);
				}
	    		foreach (Collider c in go.GetComponentsInChildren<Collider>()) {
	    			if (c is BoxCollider)
	    				((BoxCollider)c).size *= 1.5F;
	    			if (c is SphereCollider) {
	    				SphereCollider s = (SphereCollider)c;
	    				s.radius *= 1.5F;
	    				s.radius = Mathf.Max(0.225F, s.radius);
	    			}
	    			if (c is CapsuleCollider)
	    				((CapsuleCollider)c).radius *= 1.5F;
	    		}
			});
	    }
       	
	    repairBlock = createMachine<RepairBeacon, RepairBeaconLogic>("BaseRepairBeacon");
	    repairBlock.addFragments(1, 6F, repairBeaconFragments);
                 
       	worldgen.load();
       	
       	int count = 0;
       	foreach (TechnologyFragment f in repairBeaconFragments) {
       		count += worldgen.getCount(f.fragmentPrefab.ClassID);
	    	f.fragmentPrefab.setDisplayName(locale.getEntry("BaseRepairBeacon").getField<string>("frag"));
       	}
		SNUtil.log("Found "+count+" "+repairBlock.ClassID+" fragments to use", modDLL);
       	PDAHandler.EditFragmentsToScan(GenUtil.getFragment(repairBlock.TechType, 0).TechType, count);
        
        outdoorBasicPot = new OutdoorPot(TechType.PlanterPot);
        outdoorCompositePot = new OutdoorPot(TechType.PlanterPot2);
        outdoorChicPot = new OutdoorPot(TechType.PlanterPot3);
        outdoorBasicPot.register();
        outdoorCompositePot.register();
        outdoorChicPot.register();
       	
       	ACUCallbackSystem.instance.register();
       	
       	StoryHandler.instance.registerTrigger(new StoryTrigger("Precursor_LavaCastleBase_ThermalPlant2"), new TechUnlockEffect(atpTapBlock.TechType, 0.05F));
			
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(AEHooks).TypeHandle);
        
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Shocker, ampeelAntennaBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Beacon, beaconBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(poo.TechType, acuCleanerBlock.TechType);
        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseMapRoom, cameraAntennaBlock.TechType);
        //TechnologyUnlockSystem.instance.addDirectUnlock(TechType.StasisRifle, stasisBlock.TechType);
        
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("debugACU", ACUCallbackSystem.instance.debugACU);
    }
    
    class NaniteFragment : MonoBehaviour {
    	
		void OnScanned() {
    		SNUtil.addBlueprintNotification(repairBlock.TechType);
		}
    	
    }
    
    private static void fireStasisPulses(HolographicControl.HolographicControlTag btn) {
    	SubRoot sub = btn.gameObject.FindAncestor<SubRoot>();
    	if (sub && sub.isBase) {
    		foreach (BaseStasisTurretLogic lgc in sub.GetComponentsInChildren<BaseStasisTurretLogic>()) {
    			lgc.fire();
    		}
    	}
    	btn.enableForDuration(BaseStasisTurret.COOLDOWN);
    }
    
    private static void toggleMachines<M>(HolographicControl.HolographicControlTag btn) where M : ToggleableMachineBase {
    	btn.setState(!btn.getState());
    	SubRoot sub = btn.gameObject.FindAncestor<SubRoot>();
    	if (sub && sub.isBase) {
    		foreach (M lgc in sub.GetComponentsInChildren<M>()) {
    			lgc.isEnabled = btn.getState();
    		}
    	}
    }
    
    private static bool machineExists<M>(HolographicControl.HolographicControlTag btn) where M : CustomMachineLogic {
    	SubRoot sub = btn.gameObject.FindAncestor<SubRoot>();
    	return sub && sub.isBase && sub.GetComponentsInChildren<M>().Length > 0;
    }
    
    private static M createMachine<M, N>(string lck) where N : CustomMachineLogic where M : CustomMachine<N> {
    	XMLLocale.LocaleEntry e = locale.getEntry(lck);
    	M m = (M)Activator.CreateInstance(typeof(M), e);
        m.Patch();
        if (!string.IsNullOrEmpty(e.pda))
        	m.addPDAPage(e.pda, lck);
        SNUtil.log("Registered custom machine "+m);
        return m;
    }
    
    [QModPostPatch]
    public static void PostLoad() {    	
		Spawnable azurite = ItemRegistry.instance.getItem("VENT_CRYSTAL");
		if (azurite != null) {
			RecipeUtil.addIngredient(repellentBlock.TechType, azurite.TechType, 2);
			RecipeUtil.addIngredient(stasisBlock.TechType, azurite.TechType, 6);
		}
		else {
			RecipeUtil.addIngredient(repellentBlock.TechType, TechType.SeamothElectricalDefense, 1);
			RecipeUtil.addIngredient(stasisBlock.TechType, TechType.StasisRifle, 1);
		}
		
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
			RecipeUtil.addIngredient(grinderBlock.TechType, motor.TechType, 5);
		}
		else {
			RecipeUtil.addIngredient(acuCleanerBlock.TechType, TechType.Lubricant, 2);
			RecipeUtil.addIngredient(farmerBlock.TechType, TechType.Lubricant, 5);
		}
		
		Spawnable plating = ItemRegistry.instance.getItem("HullPlating");
		if (plating != null) { //c2c is loaded
			RecipeUtil.addIngredient(atpTapBlock.TechType, plating.TechType, 4);
		}
		else {
			RecipeUtil.addIngredient(atpTapBlock.TechType, TechType.PlasteelIngot, 2);
		}
		
		Spawnable drone = ItemRegistry.instance.getItem("LathingDrone");
		if (drone != null) { //c2c is loaded
			RecipeUtil.addIngredient(repairBlock.TechType, drone.TechType, 1);
		}
		else {
			RecipeUtil.addIngredient(repairBlock.TechType, TechType.Welder, 2);
		}
		
		Spawnable glowOil = ItemRegistry.instance.getItem("GlowOil");
		if (glowOil != null) {
			RecipeUtil.addIngredient(atpTapBlock.TechType, glowOil.TechType, 3);
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
		
    	BaseRoomSpecializationSystem.instance.registerModdedObject(acuBoosterBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
    	BaseRoomSpecializationSystem.instance.registerModdedObject(acuCleanerBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
    	BaseRoomSpecializationSystem.instance.registerModdedObject(ampeelAntennaBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
    	BaseRoomSpecializationSystem.instance.registerModdedObject(repairBlock, -0.1F, BaseRoomSpecializationSystem.RoomTypes.WORK);
    	BaseRoomSpecializationSystem.instance.registerModdedObject(batteryBlock, 0, BaseRoomSpecializationSystem.RoomTypes.POWER);
    }

  }
}
