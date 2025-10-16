using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;

using HarmonyLib;
using System.Reflection.Emit;

using QModManager.API.ModLoading;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.AqueousEngineering {
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
		public static ACUMonitor acuMonitorBlock;
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
		public static RoomDataDisplay roomDataBlock;
		public static FloatingPowerRelay powerRelayBlock;
		public static ItemCollectorCyclopsTether collectorTetherBlock;
		public static ItemDistributor distributorBlock;
		public static BasePillar pillarBlock;
		public static WirelessCharger wirelessChargerBlock;

		public static OutdoorPot outdoorBasicPot;
		public static OutdoorPot outdoorChicPot;
		public static OutdoorPot outdoorCompositePot;

		public static MiniPoo poo;
		public static StalkerToy toy;
		public static ItemCollector collector;
		public static NuclearFuelItem fulguriteRod;
		public static NuclearFuelItem cooledRod;
		public static NuclearFuelItem shieldedRod;
		public static NuclearFuelItem ionRod;
		//public static NuclearFuelItem reprocessedRod;

		public static TechCategory nuclearCategory;

		public static HolographicControl seabaseStasisControl;
		public static HolographicControl seabaseSonarControl;
		public static HolographicControl seabaseRepellentControl;

		public static HolographicControl rotateMoonpool;
		/*
        public static HolographicControl moonPoolRotateP90;
        public static HolographicControl moonPoolRotateM90;
        public static HolographicControl moonPoolRotateP15;
        public static HolographicControl moonPoolRotateM15;
        */

		public static TechnologyFragment[] repairBeaconFragments;

		public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();

		public static readonly XMLLocale itemLocale = new XMLLocale(modDLL, "XML/items.xml");
		public static readonly XMLLocale machineLocale = new XMLLocale(modDLL, "XML/machines.xml");
		public static readonly XMLLocale roomLocale = new XMLLocale(modDLL, "XML/rooms.xml");
		public static readonly XMLLocale acuLocale = new XMLLocale(modDLL, "XML/acu.xml");

		public static HarmonySystem harmony;

		[QModPrePatch]
		public static void PreLoad() {
			config.load();
		}

		[QModPatch]
		public static void Load() {
			harmony = new HarmonySystem(MOD_KEY, modDLL, typeof(AEPatches));
			harmony.apply();

			ModVersionCheck.getFromGitVsInstall("Aqueous Engineering", modDLL, "AqueousEngineering").register();
			SNUtil.checkModHash(modDLL);

			CustomPrefab.addPrefabNamespace("ReikaKalseki.AqueousEngineering");

			itemLocale.load();
			machineLocale.load();
			roomLocale.load();
			acuLocale.load();

			XMLLocale.LocaleEntry e = roomLocale.getEntry("ROOMTYPESPDAPAGE");
			PDAManager.PDAPage page = PDAManager.createPage(e);
			page.format(false, false, BaseRoomSpecializationSystem.instance.getLeisureDecoThreshold(false), BaseRoomSpecializationSystem.instance.getLeisureDecoThreshold(true), "%");
			page.register();

			e = itemLocale.getEntry("CraftingNodes");
			nuclearCategory = TechCategoryHandler.Main.AddTechCategory("Nuclear", e.getField<string>("nuclear"));
			TechCategoryHandler.Main.TryRegisterTechCategoryToTechGroup(TechGroup.Resources, nuclearCategory);
			CraftTreeHandler.Main.AddTabNode(CraftTree.Type.Fabricator, "Nuclear", e.getField<string>("nuclear"), TextureManager.getSprite(modDLL, "Textures/NuclearTab"), "Resources");

			poo = new MiniPoo(itemLocale.getEntry("MiniPoop"));
			poo.Patch();
			BioReactorHandler.Main.SetBioReactorCharge(poo.TechType, BaseBioReactor.GetCharge(TechType.SeaTreaderPoop) / 4);

			toy = new StalkerToy(itemLocale.getEntry("StalkerToy"));
			toy.addIngredient(TechType.Hoopfish, 1);
			toy.addIngredient(TechType.Titanium, 2);
			toy.Patch();

			setNuclear(TechType.ReactorRod);
			//setNuclear(TechType.DepletedReactorRod);

			fulguriteRod = new NuclearFuelItem("FulguriteRod");
			fulguriteRod.addIngredient(TechType.ReactorRod, 1);
			fulguriteRod.addIngredient(TechType.Sulphur, 3);
			fulguriteRod.addIngredient(TechType.Benzene, 1);
			fulguriteRod.addIngredient(TechType.Lithium, 1);
			fulguriteRod.Patch();/*
	    reprocessedRod = new NuclearFuelItem("ReprocessedRod");
	    reprocessedRod.addIngredient(TechType.DepletedReactorRod, 1);
	    reprocessedRod.addIngredient(TechType.Sulphur, 3);
	    reprocessedRod.addIngredient(TechType.Benzene, 1);
	    reprocessedRod.addIngredient(TechType.Lithium, 1);
	    reprocessedRod.Patch();*/
			cooledRod = new NuclearFuelItem("WaterCooledRod");
			cooledRod.addIngredient(TechType.ReactorRod, 1);
			cooledRod.addIngredient(TechType.FilteredWater, 2);
			cooledRod.addIngredient(TechType.Aerogel, 1);
			cooledRod.Patch();

			shieldedRod = new NuclearFuelItem("ShieldedRod");
			shieldedRod.addIngredient(TechType.ReactorRod, 1);
			shieldedRod.addIngredient(TechType.Lead, 4);
			shieldedRod.Patch();

			ionRod = new NuclearFuelItem("IonRod");
			ionRod.addIngredient(fulguriteRod.TechType, 1);
			ionRod.addIngredient(TechType.UraniniteCrystal, 10);
			ionRod.addIngredient(TechType.PrecursorIonCrystal, 3);
			ionRod.Patch();

			collector = new ItemCollector(machineLocale.getEntry("ItemCollector")); //deliberately under machine locale for site
			collector.Patch();

			seabaseStasisControl = new HolographicControl("SeabaseStasis", "Fire stasis pulse", true, fireStasisPulses, btn => machineExists<BaseStasisTurretLogic>(btn));
			seabaseStasisControl.setIcons("Textures/HoloButtons/StasisButton", 200).Patch();
			seabaseSonarControl = new HolographicControl("SeabaseSonar", "Toggle sonar", true, btn => toggleMachines<BaseSonarPingerLogic>(btn), btn => machineExists<BaseSonarPingerLogic>(btn));
			seabaseSonarControl.setIcons("Textures/HoloButtons/SonarButton", 200).Patch();
			seabaseRepellentControl = new HolographicControl("SeabaseRepellent", "Toggle repellant pylon", true, btn => toggleMachines<BaseCreatureRepellentLogic>(btn), btn => machineExists<BaseCreatureRepellentLogic>(btn));
			seabaseRepellentControl.setIcons("Textures/HoloButtons/RepellentButton", 200).Patch();
			/*
            moonPoolRotateP90 = new HolographicControl("MoonPoolP90", "Rotate 90 Degrees Clockwise", false, MoonpoolRotationSystem.instance.rotateMoonpool, btn => true);
            moonPoolRotateP90.setIcons("Textures/HoloButtons/MoonPoolP90", 200).Patch();
            moonPoolRotateM90 = new HolographicControl("MoonPoolM90", "Rotate 90 Degrees AntiClockwise", false, MoonpoolRotationSystem.instance.rotateMoonpool, btn => true);
            moonPoolRotateM90.setIcons("Textures/HoloButtons/MoonPoolM90", 200).Patch();
            moonPoolRotateP15 = new HolographicControl("MoonPoolP15", "Rotate 15 Degrees Clockwise", false, MoonpoolRotationSystem.instance.rotateMoonpool, btn => true);
            moonPoolRotateP15.setIcons("Textures/HoloButtons/MoonPoolP15", 200).Patch();
            moonPoolRotateM15 = new HolographicControl("MoonPoolM15", "Rotate 15 Degrees AntiClockwise", false, MoonpoolRotationSystem.instance.rotateMoonpool, btn => true);
            moonPoolRotateM15.setIcons("Textures/HoloButtons/MoonPoolM15", 200).Patch();
            */
			rotateMoonpool = new HolographicControl("MoonPoolRotate", "Rotate Docking Bay", false, MoonpoolRotationSystem.instance.rotateMoonpool, btn => true);
			rotateMoonpool.setIcons("Textures/HoloButtons/MoonPoolRotate", 200).Patch();

			sonarBlock = createMachine<BaseSonarPinger, BaseSonarPingerLogic>("BaseSonar");
			repellentBlock = createMachine<BaseCreatureRepellent, BaseCreatureRepellentLogic>("BaseRepellent");
			beaconBlock = createMachine<BaseBeacon, BaseBeaconLogic>("BaseBeacon");
			ampeelAntennaBlock = createMachine<AmpeelAntenna, AmpeelAntennaLogic>("BaseAmpeelAntenna");
			farmerBlock = createMachine<Autofarmer, AutofarmerLogic>("BaseFarmer");
			acuCleanerBlock = createMachine<ACUCleaner, ACUCleanerLogic>("BaseACUCleaner");
			acuMonitorBlock = createMachine<ACUMonitor, ACUMonitorLogic>("BaseACUMonitor");
			cameraAntennaBlock = createMachine<RemoteCameraAntenna, RemoteCameraAntennaLogic>("BaseRemoteCamera");
			batteryBlock = createMachine<BaseBattery, BaseBatteryLogic>("BaseBattery");
			//ionCubeBlock = createMachine<IonCubeBurner, IonCubeBurnerLogic>("IonCubeBurner");
			displayBlock = createMachine<ItemDisplay, ItemDisplayLogic>("BaseItemDisplay");
			domeLightBlock = createMachine<BaseDomeLight, BaseDomeLightLogic>("BaseDomeLight");
			atpTapBlock = createMachine<ATPTap, ATPTapLogic>("BaseATPTap");
			stasisBlock = createMachine<BaseStasisTurret, BaseStasisTurretLogic>("BaseStasisTurret");
			controlsBlock = createMachine<BaseControlPanel, BaseControlPanelLogic>("BaseControlPanel");
			grinderBlock = createMachine<BaseDrillableGrinder, BaseDrillableGrinderLogic>("BaseDrillableGrinder");
			roomDataBlock = createMachine<RoomDataDisplay, RoomDataDisplayLogic>("BaseRoomDataDisplay");
			powerRelayBlock = createMachine<FloatingPowerRelay, FloatingPowerRelayLogic>("BaseFloatingPowerRelay");
			collectorTetherBlock = createMachine<ItemCollectorCyclopsTether, ItemCollectorCyclopsTetherLogic>("CyclopsCollectorTether");
			distributorBlock = createMachine<ItemDistributor, ItemDistributorLogic>("ItemDistributor");
			pillarBlock = createMachine<BasePillar, BasePillarLogic>("BasePillar");
			wirelessChargerBlock = createMachine<WirelessCharger, WirelessChargerLogic>("WirelessCharger");
			string[] li = VanillaFlora.MUSHROOM_BUMP.getPrefabs(true, true).ToArray();
			repairBeaconFragments = new TechnologyFragment[li.Length - 2];
			for (int i = 1; i < li.Length - 1; i++) { //only idx 1,2,3 since 0 is rotated and tall and 4 has a light and is just 3 anyway
				repairBeaconFragments[i - 1] = new TechnologyFragment(li[i], go => {
					go.removeComponent<CoralBlendWhite>();
					go.removeComponent<PlantBehaviour>();
					go.removeComponent<LiveMixin>();
					go.removeComponent<FMOD_StudioEventEmitter>();
					go.removeComponent<Pickupable>();
					go.EnsureComponent<NaniteFragment>();
					foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
						RenderUtil.swapTextures(modDLL, r, "Textures/RepairFragment");
						RenderUtil.setGlossiness(r, 1.5F, 0, 0.85F);
						RenderUtil.setEmissivity(r, 10);
					}
					foreach (Collider c in go.GetComponentsInChildren<Collider>()) {
						if (c is BoxCollider)
							((BoxCollider)c).size *= 1.5F;
						if (c is SphereCollider s) {
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
				f.fragmentPrefab.setDisplayName(machineLocale.getEntry("BaseRepairBeacon").getField<string>("frag"));
			}
			SNUtil.log("Found " + count + " " + repairBlock.ClassID + " fragments to use", modDLL);
			PDAHandler.EditFragmentsToScan(GenUtil.getFragment(repairBlock.TechType, 0).TechType, count);

			outdoorBasicPot = new OutdoorPot(TechType.PlanterPot);
			outdoorCompositePot = new OutdoorPot(TechType.PlanterPot2);
			outdoorChicPot = new OutdoorPot(TechType.PlanterPot3);
			outdoorBasicPot.register();
			outdoorCompositePot.register();
			outdoorChicPot.register();

			ACUCallbackSystem.instance.register();
			NuclearReactorFuelSystem.instance.register();

			NuclearReactorFuelSystem.instance.registerReactorFuelRelative(cooledRod.TechType, 2F, 0.8F, 0.75F, TechType.DepletedReactorRod);
			NuclearReactorFuelSystem.instance.registerReactorFuelRelative(fulguriteRod.TechType, 0.25F, 2.5F, 1.5F, TechType.DepletedReactorRod);
			NuclearReactorFuelSystem.instance.registerReactorFuelRelative(shieldedRod.TechType, 1F, 1F, 0.1F, TechType.DepletedReactorRod);
			NuclearReactorFuelSystem.instance.registerReactorFuelRelative(ionRod.TechType, 5F, 2F, 1.0F, TechType.DepletedReactorRod);
			//NuclearReactorFuelSystem.instance.registerReactorFuelRelative(reprocessedRod.TechType, 0.4F, 0.8F, TechType.None);

			StoryHandler.instance.registerTrigger(new ScanTrigger(TechType.PrecursorThermalPlant), new TechUnlockEffect(atpTapBlock.TechType, 1F, 5));
			StoryHandler.instance.registerTrigger(new ScanTrigger(TechType.PrecursorPrisonIonGenerator), new TechUnlockEffect(ionRod.TechType, 1F, 1));

			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(AEHooks).TypeHandle);

			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Shocker, ampeelAntennaBlock.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Beacon, beaconBlock.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(poo.TechType, acuCleanerBlock.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseWaterPark, acuMonitorBlock.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseWaterPark, toy.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.BaseMapRoom, cameraAntennaBlock.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.PowerTransmitter, powerRelayBlock.TechType);
			//TechnologyUnlockSystem.instance.addDirectUnlock(TechType.StasisRifle, stasisBlock.TechType);
			//TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Gravsphere, collector.TechType);

			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.ReactorRod, cooledRod.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.ReactorRod, fulguriteRod.TechType);
			TechnologyUnlockSystem.instance.addDirectUnlock(TechType.ReactorRod, shieldedRod.TechType);

			StoryHandler.instance.registerTrigger(new TechTrigger(TechType.Gravsphere), new TechUnlockEffect(collector.TechType, 1, 10));
			StoryHandler.instance.registerTrigger(new MultiTechTrigger(collector.TechType, TechType.Cyclops), new TechUnlockEffect(collectorTetherBlock.TechType, 1, 10));

			e = machineLocale.getEntry("BaseACUMonitor");
			page = PDAManager.createPage(e.key+"PDA", e.getField<string>("pdatitle"), e.pda, e.getField<string>("category"));
			page.register();

			BaseRoomSpecializationSystem.instance.registerModdedObject(toy, 0.25F, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(poo, -0.25F, BaseRoomSpecializationSystem.RoomTypes.ACU);

			BaseRoomSpecializationSystem.instance.registerModdedObject(acuBoosterBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(acuCleanerBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(acuMonitorBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(batteryBlock, 0, BaseRoomSpecializationSystem.RoomTypes.POWER);

			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("debugACU", ACUCallbackSystem.instance.debugACU);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("sunbeamModel", createSunbeamModel);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<bool>>("debugRooms", arg => BaseRoomSpecializationSystem.debugRoomCompute = arg);
			ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("debugRoom", BaseRoomSpecializationSystem.debugRoomValues);

			if (config.getBoolean(AEConfig.ConfigEntries.ACUSOUND))
				WaterParkCreature.behavioursToDisableInside[0] = typeof(AqueousEngineeringMod); //replace with a non-MB class that will never be present
		}

		internal static void setNuclear(TechType item) {
			RecipeUtil.changeRecipePath(item, "Resources", "Nuclear");
			RecipeUtil.setItemCategory(item, TechGroup.Resources, nuclearCategory);
		}

		private static void createSunbeamModel() {
			GameObject pfb = ObjectUtil.createWorldObject("c0d320d2-537e-4128-90ec-ab1466cfbbc3");
			pfb.transform.position = Player.main.transform.position + (Camera.main.transform.forward * 5);
			ParticleSystemRenderer r = VFXSunbeam.main.shipPrefab.getChildObject("xShip").GetComponent<ParticleSystemRenderer>();
			Renderer r2 = r.clone();
			Mesh m = r.mesh;
			MeshRenderer[] r0 = pfb.GetComponentsInChildren<MeshRenderer>();
			Transform mdl = r0[0].transform.parent;
			foreach (MeshRenderer rr in r0) {
				rr.gameObject.destroy(false);
			}
			r2.transform.SetParent(mdl);
			r2.transform.localPosition = Vector3.zero;
			ParticleSystem pp = r2.GetComponent<ParticleSystem>();
			ParticleSystem.MainModule mm = pp.main;
			mm.startSizeMultiplier = 0.03F;
			pp.Play();
			r2.transform.localScale = Vector3.one * 0.01F;
			pfb.removeComponent<VFXKeepAtDistance>();
			pfb.removeComponent<PrecursorGunTarget>();
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
			XMLLocale.LocaleEntry e = machineLocale.getEntry(lck);
			M m = (M)Activator.CreateInstance(typeof(M), e);
			m.Patch();
			if (!string.IsNullOrEmpty(e.pda))
				m.addPDAPage(e.pda, lck);
			SNUtil.log("Registered custom machine " + m);
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

			Spawnable platinum = ItemRegistry.instance.getItem("PLATINUM");
			if (platinum != null) {
				RecipeUtil.addIngredient(toy.TechType, platinum.TechType, 1);
			}
			else {
				RecipeUtil.addIngredient(toy.TechType, TechType.Silver, 1);
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
				RecipeUtil.addIngredient(collector.TechType, motor.TechType, 1);
				RecipeUtil.addIngredient(distributorBlock.TechType, motor.TechType, 2);
			}
			else {
				RecipeUtil.addIngredient(acuCleanerBlock.TechType, TechType.Lubricant, 2);
				RecipeUtil.addIngredient(farmerBlock.TechType, TechType.Lubricant, 5);
				RecipeUtil.addIngredient(collector.TechType, TechType.AdvancedWiringKit, 1);
				RecipeUtil.addIngredient(distributorBlock.TechType, TechType.Lubricant, 3);
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
				PlanktonFeeder.fuel = (WorldCollectedItem)plankton;
				ACUBooster.fuels[PlanktonFeeder.fuel.ClassID] = new ACUFuel(PlanktonFeeder.fuel, 1, 1);
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

			Spawnable mushdisk = ItemRegistry.instance.getItem("treeMushroomSpores");
			if (mushdisk != null) {
				SNUtil.log("Found mushroom disk spore item. Adding compat.");
				ACUBooster.fuels[mushdisk.ClassID] = new ACUFuel((WorldCollectedItem)mushdisk, 5, 2);
			}
			else {
				SNUtil.log("Plankton item not found.");
			}

			ACUEcosystems.addPost();

			BaseRoomSpecializationSystem.instance.registerModdedObject(acuBoosterBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(acuCleanerBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(acuMonitorBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			BaseRoomSpecializationSystem.instance.registerModdedObject(ampeelAntennaBlock, 0, BaseRoomSpecializationSystem.RoomTypes.ACU);
			//BaseRoomSpecializationSystem.instance.registerModdedObject(roomDataBlock, 0, BaseRoomSpecializationSystem.RoomTypes.UNSPECIALIZED);
			BaseRoomSpecializationSystem.instance.registerModdedObject(repairBlock, -0.1F, BaseRoomSpecializationSystem.RoomTypes.MECHANICAL);
			BaseRoomSpecializationSystem.instance.registerModdedObject(batteryBlock, 0, BaseRoomSpecializationSystem.RoomTypes.POWER);
			BaseRoomSpecializationSystem.instance.registerModdedObject(wirelessChargerBlock, 0.5F, BaseRoomSpecializationSystem.RoomTypes.POWER);

			if (WirelessCharger.unlockTrigger != TechType.None)
				TechnologyUnlockSystem.instance.addDirectUnlock(WirelessCharger.unlockTrigger, wirelessChargerBlock.TechType);

			TechType refuel = SNUtil.getTechType("ReplenishReactorRod");
			if (refuel != TechType.None) {
				SNUtil.log("Moving reactor rod recharge to nuclear tab");
				CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, "Resources", "Electronics", "ReplenishReactorRod");
				refuel.preventCraftNodeAddition();
				CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, refuel, "Resources", "Nuclear");
				RecipeUtil.setItemCategory(refuel, TechGroup.Resources, nuclearCategory);
				//KnownTechHandler.SetCompoundUnlock(refuel, TechType.Unobtanium);
				KnownTechHandler.SetAnalysisTechEntry(TechType.Unobtanium, new TechType[]{refuel});
				refuel.removeUnlockTrigger(new TechTrigger(TechType.BaseNuclearReactor));
				TechnologyUnlockSystem.instance.addDirectUnlock(TechType.ReactorRod, refuel);
			}

			TechType baseglass = SNUtil.getTechType("BaseGlass");
			if (baseglass != TechType.None) {
				RecipeUtil.addIngredient(shieldedRod.TechType, baseglass, 1);
			}
		}

	}
}
