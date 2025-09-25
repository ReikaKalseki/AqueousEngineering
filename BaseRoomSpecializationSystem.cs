using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;

using RootMotion.FinalIK;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.AqueousEngineering {

	public class BaseRoomSpecializationSystem { //TODO 2.0 handle large rooms

		private static readonly string ACU_PREFAB = "31662630-7cba-4583-8456-2fa1c4cc31aa";
		private static readonly string STANDING_LOCKER_PREFAB = "775feb4c-dab9-4322-b4a5-a4289ca1cf6a";
		private static readonly string BASIC_LOCKER_PREFAB = "5fc7744b-5a2c-4572-8e53-eebf990de434";

		private static readonly HashSet<string> lockers = new HashSet<string>();
		private static readonly Dictionary<string, float> decoRatings = new Dictionary<string, float>();
		private static readonly Dictionary<TechType, float> itemDecoRatings = new Dictionary<TechType, float>();
		private static readonly Dictionary<string, RoomTypes[]> objectTypeMappings = new Dictionary<string, RoomTypes[]>();

		public static bool debugRoomCompute = false;

		public static readonly BaseRoomSpecializationSystem instance = new BaseRoomSpecializationSystem();

		//public static readonly float LEISURE_DECO_THRESHOLD = computeDecoThreshold();

		private BaseRoomSpecializationSystem() {
			lockers.Add("367656d6-87d9-42a1-926c-3cf959ea1c85");
			lockers.Add(BASIC_LOCKER_PREFAB);
			lockers.Add(STANDING_LOCKER_PREFAB);
			lockers.Add("cd34fecd-794c-4a0c-8012-dd81b77f2840");
			lockers.Add("CabinetWide");
			lockers.Add("CabinetMediumTall");
			lockers.Add("CabinetTall");
			lockers.Add("FCSCrewLocker");


			//foreach (string s in lockers)
			//	objectTypeMappings[s] = new RoomTypes[]{RoomTypes.STORAGE};

			objectTypeMappings[ACU_PREFAB] = new RoomTypes[] { RoomTypes.ACU };
			objectTypeMappings["87f5d3e6-e00b-4cf3-be39-0a9c7e951b84"] = new RoomTypes[] { RoomTypes.AGRICULTURAL }; //indoor growbed			
			objectTypeMappings["769f9f44-30f6-46ed-aaf6-fbba358e1676"] = new RoomTypes[] { RoomTypes.POWER }; //bioreactor
			objectTypeMappings["864f7780-a4c3-4bf2-b9c7-f4296388b70f"] = new RoomTypes[] { RoomTypes.POWER }; //nuclear reactor
			objectTypeMappings["51eba507-317c-46bf-adde-4459dc8e002e"] = new RoomTypes[] { RoomTypes.LEISURE }; //vending machine
			objectTypeMappings["b09a156d-d3cf-455a-848d-a9a8cad2b811"] = new RoomTypes[] { RoomTypes.LEISURE }; //coffee
			objectTypeMappings["5d63470b-2705-4963-be4b-2e11e8dc6f2e"] = new RoomTypes[] { RoomTypes.LEISURE }; //bench
			objectTypeMappings["4cb154ef-bdb6-4ff4-9107-f378ce21a9b7"] = new RoomTypes[] { RoomTypes.LEISURE }; //bench
			objectTypeMappings["26cdb865-efbd-403c-8873-92453bcfc935"] = new RoomTypes[] { RoomTypes.LEISURE }; //chair
			objectTypeMappings["4f0e304b-0d25-4f9c-a6ba-1b1a6bf029b0"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK }; //chair
			objectTypeMappings["cbeca4bd-cba4-4905-89fd-2470aaa204b1"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK }; //chair
			objectTypeMappings["cf522a95-3038-4759-a53c-8dad1242c8ed"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK }; //desk
			objectTypeMappings["7370e7a0-ebc0-4c33-9997-7084c11a55b0"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK }; //counter
			objectTypeMappings["cdb374fd-4f38-4bef-86a3-100cc87155b6"] = new RoomTypes[] { RoomTypes.LEISURE }; //bed
			objectTypeMappings["c3994649-d0da-4f8c-bb77-1590f50838b9"] = new RoomTypes[] { RoomTypes.LEISURE }; //bed
			objectTypeMappings["dffaf40f-9fbc-4553-9b35-3f939c76c283"] = new RoomTypes[] { RoomTypes.LEISURE }; //bed
			objectTypeMappings["2f2d8419-c55b-49ac-9698-ecb431fffed2"] = new RoomTypes[] { RoomTypes.MECHANICAL }; //water filter			
			objectTypeMappings["cdade216-3d4d-4adf-901c-3a91fb3b88c4"] = new RoomTypes[] { RoomTypes.WORK }; //centrifuge		
			objectTypeMappings["bef7bc0b-149d-4342-bbb4-329047685578"] = new RoomTypes[] { RoomTypes.WORK }; //fragment analyzer
			objectTypeMappings["c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06"] = new RoomTypes[] { RoomTypes.WORK, RoomTypes.MECHANICAL, RoomTypes.LEISURE }; //medical cabinet
			objectTypeMappings["c9bdcc4d-a8c6-43c0-8f7a-f86841cd4493"] = new RoomTypes[] { RoomTypes.WORK }; //specimen analyzer
			objectTypeMappings["5c06baec-0539-4f26-817d-78443548cc52"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK, RoomTypes.MECHANICAL }; //radio	
																																					  //no need to map chargers; that is handled directly via instanceof Charger in the computation code

			//FCS
			objectTypeMappings["CabinetTVStand"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSToilet"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["fcsmicrowave"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSCrewBunkBed"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["UniversalCharger"] = new RoomTypes[] { RoomTypes.MECHANICAL };
			objectTypeMappings["MiniMedBay"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["MiniFountainFilter"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["Seabreeze"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.STORAGE };
			objectTypeMappings["EnergyPillVendingMachine"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.MECHANICAL };
			//objectTypeMappings["neonShelf01"] = new RoomTypes[]{RoomTypes.LEISURE};	
			//objectTypeMappings["neonShelf02"] = new RoomTypes[]{RoomTypes.LEISURE};	
			//objectTypeMappings["neonShelf03"] = new RoomTypes[]{RoomTypes.LEISURE};	
			objectTypeMappings["fcsCuringCabinet"] = new RoomTypes[] { RoomTypes.WORK };
			objectTypeMappings["AlterraGen"] = new RoomTypes[] { RoomTypes.POWER };
			objectTypeMappings["AlterraStorage"] = new RoomTypes[] { RoomTypes.STORAGE };
			objectTypeMappings["HydroponicHarvester"] = new RoomTypes[] { RoomTypes.AGRICULTURAL };
			objectTypeMappings["Replicator"] = new RoomTypes[] { RoomTypes.MECHANICAL };
			objectTypeMappings["MatterAnalyzer"] = new RoomTypes[] { RoomTypes.WORK };
			objectTypeMappings["AutoCrafter"] = new RoomTypes[] { RoomTypes.MECHANICAL };
			objectTypeMappings["PeeperLoungeBar"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FireExtinguisherRefueler"] = new RoomTypes[] { RoomTypes.MECHANICAL };
			objectTypeMappings["DSSFloorServerRack"] = new RoomTypes[] { RoomTypes.MECHANICAL };
			//objectTypeMappings["QuantumPowerBankCharger"] = new RoomTypes[]{RoomTypes.MECHANICAL};	
			//objectTypeMappings["QuantumTeleporter"] = new RoomTypes[]{RoomTypes.MECHANICAL};	
			objectTypeMappings["FCSShower"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSSink"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSJukebox"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSJukeboxSpeaker"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSJukeBoxSubWoofer"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSStove"] = new RoomTypes[] { RoomTypes.LEISURE, RoomTypes.WORK };
			objectTypeMappings["tableSmartTV"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["mountSmartTV"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["Sofa1"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["Sofa2"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["Sofa3"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["NeonBarStool"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["FCSCrewBunkBed"] = new RoomTypes[] { RoomTypes.LEISURE };
			objectTypeMappings["PatreonStatue"] = new RoomTypes[] { RoomTypes.LEISURE };
			//

			decoRatings["26cdb865-efbd-403c-8873-92453bcfc935"] = 0.15F; //best chair	
			decoRatings["cbeca4bd-cba4-4905-89fd-2470aaa204b1"] = 0.1F; //chair		

			decoRatings["5c06baec-0539-4f26-817d-78443548cc52"] = -0.25F; //radio	

			decoRatings[STANDING_LOCKER_PREFAB] = 0.1F;

			decoRatings["cdb374fd-4f38-4bef-86a3-100cc87155b6"] = 0.33F; //double bed + extra sheet
			decoRatings["c3994649-d0da-4f8c-bb77-1590f50838b9"] = 0.15F; //bed
			decoRatings["dffaf40f-9fbc-4553-9b35-3f939c76c283"] = 0.25F; //double bed

			decoRatings["336f276f-9546-40d0-98cb-974994dee3bf"] = 0.15F; //prawn poster
			decoRatings["d76dd251-492d-4bf9-8adb-25e59d709df2"] = 0.15F; //prawn poster 2
			decoRatings["876cbea4-b4bf-4311-8264-5118bfef291c"] = 0.75F; //aurora poster
			decoRatings["72da21f9-f3e2-4183-ac57-d3679fb09122"] = 0.05F; //shooter poster
			decoRatings["d809cb15-6784-4f7c-bf5d-f7d0c5bf8546"] = 0.15F; //cat poster
			decoRatings["c0d320d2-537e-4128-90ec-ab1466cfbbc3"] = 1.25F; //toy aurora

			decoRatings["1d1898ca-8436-4fe4-aaf4-a1d9fa6d58cb"] = 1; //bar table
			decoRatings["51eba507-317c-46bf-adde-4459dc8e002e"] = -2; //vending machine
			decoRatings["f1cde32e-101a-4dd5-8084-8c950b9c2432"] = -5; //trashcan
			decoRatings["bedc40fb-bd97-4b4d-a943-d39360c9c7bd"] = -3; //lab trashcan
			decoRatings["cf1df719-905c-4385-98da-b638fdfd53f7"] = 0.25F; //shelf
			decoRatings["c5ae1472-0bdc-4203-8418-fb1f74c8edf5"] = 0.5F; //shelf
			decoRatings["cf522a95-3038-4759-a53c-8dad1242c8ed"] = 0.5F; //desk
			decoRatings["7370e7a0-ebc0-4c33-9997-7084c11a55b0"] = 0.25F; //counter
			decoRatings["274bd60f-16c4-4810-911b-c5562fe7c2d8"] = 0.5F; //wall mounted plant shelf
																		//decoRatings["b343166e-3a17-4a1c-85d1-05dee8ec1575"] = 0.25F; //sign //made content sensitive
			decoRatings["07a05a2f-de55-4c60-bfda-cedb3ab72b88"] = 0.25F; //jack eye
			decoRatings["4b8cd269-6646-42d0-b8a0-9a40ef0c07d0"] = 0.75F; //toy car
			decoRatings["7cdcbed0-7d20-43c4-beb4-f1ad539b2a76"] = 0.75F; //toy car
			decoRatings["ad5e149b-d35c-4b46-bb4e-b4c0a9c6e668"] = -0.25F; //markiplier
			decoRatings["cb89366d-eac0-4011-8665-fafde75b215c"] = -0.25F; //markiplier
			decoRatings["f7e26c44-bb28-4979-8f83-76ed529979fc"] = -0.25F; //markiplier
			decoRatings["c96baff4-0993-4893-8345-adb8709901a7"] = 0.33F; //toy cat
			decoRatings["7ea4a91e-80fc-43aa-8ce3-5d52bd19e278"] = 0.33F; //"gorge toy"

			decoRatings["0fbf203a-a940-4b6e-ac63-0fe2737d84c2"] = 0.25F; //chic pot

			decoRatings["2cee55bc-6136-47c5-a1ed-14c8f3203856"] = 0.6F; //lab parts
			decoRatings["9c5f22de-5049-48bb-ad1e-0d78c894210e"] = 0.6F; //lab parts
			decoRatings["3fd9050b-4baf-4a78-a883-e774c648887c"] = 0.8F; //lab parts

			decoRatings["1faf2b57-ff4f-4ea5-a715-7cc5ff6aae60"] = 0.05F; //lab containers
			decoRatings["1b0b7f6d-9793-469c-9872-dfe690834fee"] = 0.05F; //lab containers
			decoRatings["7f601dd4-0645-414d-bb62-5b0b62985836"] = 0.05F; //lab containers
			decoRatings["a227d6b6-d64c-4bf0-b919-2db02d67d037"] = 0.05F; //lab containers
			decoRatings["d6389e01-f2cd-4f9d-a495-0867753e44f0"] = 0.05F; //lab containers
			decoRatings["e7f9c5e7-3906-4efd-b239-28783bce17a5"] = 0.05F; //lab containers

			decoRatings["a36047b0-1533-4718-8879-d6ba9229c978"] = 2.0F; //large lab glass tube

			decoRatings["ResourceMonitorBuildableSmall"] = 0.05F;
			decoRatings["ResourceMonitorBuildableLarge"] = 0.1F;

			//FCS
			decoRatings["NeonPlanter"] = 0.75F;
			decoRatings["FireExtinguisherRefueler"] = -0.25F;
			decoRatings["DSSFloorServerRack"] = 0.125F;
			decoRatings["TrashReceptacle"] = -1;
			decoRatings["Curtain"] = 0.33F;
			decoRatings["QuantumPowerBankCharger"] = 0.75F;
			decoRatings["QuantumTeleporter"] = 2.0F;
			decoRatings["FCSStove"] = 0.1F;
			decoRatings["FCSShower"] = 0.33F;
			decoRatings["FCSSink"] = 0.1F;
			decoRatings["FCSJukebox"] = 1.5F;
			decoRatings["FCSJukeboxSpeaker"] = 0.25F;
			decoRatings["FCSJukeBoxSubWoofer"] = 0.5F;
			decoRatings["HologramPoster"] = 0.25F;
			decoRatings["tableSmartTV"] = 0.75F;
			decoRatings["mountSmartTV"] = 0.75F;
			decoRatings["neonTable01"] = 1;
			decoRatings["neonTable02"] = 1;
			decoRatings["FCSRug"] = 0.4F;
			decoRatings["FCSCrewLocker"] = 0.25F;
			decoRatings["Sofa1"] = 0.25F;
			decoRatings["Sofa2"] = 0.25F;
			decoRatings["Sofa3"] = 0.25F;
			decoRatings["NeonBarStool"] = 0.15F;
			decoRatings["FCSCrewBunkBed"] = 0.1F;
			decoRatings["DisplayBoard"] = 0.5F;
			decoRatings["PatreonStatue"] = 0.25F;
			decoRatings["ahsLeftCornerwGlassRailing"] = 0.1F;
			decoRatings["ahsRightCornerwGlassRailing"] = 0.1F;
			decoRatings["ahsrailingglass"] = 0.05F;
			decoRatings["LedLightStickShort"] = 0.125F;
			decoRatings["LedLightStickLong"] = 0.25F;

			//misc other mods
			decoRatings["ActualTimeAnalogueClock"] = 0.25F;
			decoRatings["ActualTimeDigitalClock"] = 0.25F;
			//

			//decoRatings["****"] = 1.5F; //plant panel

			//foreach (string s in decoRatings.Keys)
			//	objectTypeMappings[s] = RoomTypes.LEISURE;

			itemDecoRatings[TechType.PrecursorKey_Blue] = 2F;
			itemDecoRatings[TechType.PrecursorKey_Red] = 2F;
			itemDecoRatings[TechType.PrecursorKey_White] = 1.5F;
			itemDecoRatings[TechType.PrecursorKey_Orange] = 1.5F;
			itemDecoRatings[TechType.PrecursorKey_Purple] = 1.5F;
			itemDecoRatings[TechType.PrecursorIonPowerCell] = 1.25F;
			itemDecoRatings[TechType.PrecursorIonCrystal] = 2F;
			itemDecoRatings[TechType.Kyanite] = 1.5F;
			itemDecoRatings[TechType.Diamond] = 0.67F;
			itemDecoRatings[TechType.AluminumOxide] = 0.67F;

			itemDecoRatings[TechType.Peeper] = 0.05F;
			itemDecoRatings[TechType.Boomerang] = 0.05F;
			itemDecoRatings[TechType.Spinefish] = 0.05F;
			itemDecoRatings[TechType.HoleFish] = 0.05F;
			itemDecoRatings[TechType.Oculus] = 0.1F;
			itemDecoRatings[TechType.Hoopfish] = 0.2F;
			itemDecoRatings[TechType.Hoverfish] = 0.2F;
			itemDecoRatings[TechType.LavaBoomerang] = 0.25F;
			itemDecoRatings[TechType.Eyeye] = -0.05F;
			itemDecoRatings[TechType.LavaEyeye] = -0.05F;
			itemDecoRatings[TechType.GarryFish] = -0.1F;
			itemDecoRatings[TechType.Spadefish] = -0.1F;
			itemDecoRatings[TechType.Reginald] = 0.02F;

			itemDecoRatings[TechType.CreepvineSeedCluster] = 0.1F;
			itemDecoRatings[TechType.CreepvinePiece] = 0.05F;
			itemDecoRatings[TechType.Creepvine] = 0.05F;
			itemDecoRatings[TechType.BloodOil] = -0.25F;
			itemDecoRatings[TechType.BloodVine] = -0.25F;
			itemDecoRatings[TechType.SnakeMushroomSpore] = 0.4F;
			itemDecoRatings[TechType.SnakeMushroom] = 0.4F;
			itemDecoRatings[TechType.EyesPlantSeed] = 0F;
			itemDecoRatings[TechType.EyesPlant] = 0F;
			itemDecoRatings[TechType.GabeSFeatherSeed] = 0.1F;
			itemDecoRatings[TechType.GabeSFeather] = 0.1F;
			itemDecoRatings[TechType.RedGreenTentacleSeed] = 0.1F;
			itemDecoRatings[TechType.RedGreenTentacle] = 0.1F;
			itemDecoRatings[TechType.SpikePlantSeed] = -0.2F;
			itemDecoRatings[TechType.SpikePlant] = -0.2F;
			itemDecoRatings[TechType.MembrainTreeSeed] = 0.25F;
			itemDecoRatings[TechType.MembrainTree] = 0.25F;
			itemDecoRatings[TechType.RedConePlantSeed] = 0.1F;
			itemDecoRatings[TechType.RedConePlant] = 0.1F;
			itemDecoRatings[TechType.KooshChunk] = 0F;
			itemDecoRatings[TechType.SpottedLeavesPlantSeed] = 0.05F;
			itemDecoRatings[TechType.SpottedLeavesPlant] = 0.05F;
			itemDecoRatings[TechType.PurpleFanSeed] = 0.02F;
			itemDecoRatings[TechType.PurpleFan] = 0.02F;
			itemDecoRatings[TechType.PurpleBranchesSeed] = 0F;
			itemDecoRatings[TechType.PurpleBranches] = 0F;
			itemDecoRatings[TechType.PurpleStalkSeed] = 0.33F;
			itemDecoRatings[TechType.PurpleStalk] = 0.33F;
			itemDecoRatings[TechType.AcidMushroomSpore] = 0F;
			itemDecoRatings[TechType.WhiteMushroomSpore] = 0F;
			itemDecoRatings[TechType.AcidMushroom] = 0F;
			itemDecoRatings[TechType.WhiteMushroom] = 0F;
			itemDecoRatings[TechType.RedBasketPlantSeed] = 0.75F;
			itemDecoRatings[TechType.RedBasketPlant] = 0.75F;
			itemDecoRatings[TechType.ShellGrassSeed] = 0.05F;
			itemDecoRatings[TechType.ShellGrass] = 0.05F;
			itemDecoRatings[TechType.RedRollPlantSeed] = 0.25F;
			itemDecoRatings[TechType.RedRollPlant] = 0.25F;
			itemDecoRatings[TechType.RedBushSeed] = 0.02F;
			itemDecoRatings[TechType.RedBush] = 0.02F;
			itemDecoRatings[TechType.SeaCrownSeed] = 0.2F;
			itemDecoRatings[TechType.SeaCrown] = 0.2F;
			itemDecoRatings[TechType.PurpleTentacleSeed] = 0.1F;
			itemDecoRatings[TechType.PurpleTentacle] = 0.1F;
			itemDecoRatings[TechType.BluePalmSeed] = 0.05F;
			itemDecoRatings[TechType.BluePalm] = 0.05F;
			itemDecoRatings[TechType.SmallFanSeed] = 0.02F;
			itemDecoRatings[TechType.SmallFan] = 0.02F;

			itemDecoRatings[TechType.PinkFlowerSeed] = 0.2F;
			itemDecoRatings[TechType.PinkFlower] = 0.2F;
			itemDecoRatings[TechType.PurpleRattleSpore] = -0.05F;
			itemDecoRatings[TechType.PurpleRattle] = -0.05F;
			itemDecoRatings[TechType.PurpleVegetablePlant] = -0.1F;
			itemDecoRatings[TechType.PurpleVegetable] = -0.1F;
			itemDecoRatings[TechType.PurpleVasePlantSeed] = 0.5F;
			itemDecoRatings[TechType.PurpleVasePlant] = 0.5F;
			itemDecoRatings[TechType.HangingFruit] = 0.5F;
			itemDecoRatings[TechType.HangingFruitTree] = 0.5F;
			itemDecoRatings[TechType.OrangeMushroomSpore] = -0.25F;
			itemDecoRatings[TechType.OrangeMushroom] = -0.25F;
			itemDecoRatings[TechType.OrangePetalsPlantSeed] = 0.05F;
			itemDecoRatings[TechType.OrangePetalsPlant] = 0.05F;
			itemDecoRatings[TechType.MelonSeed] = 0F;
			itemDecoRatings[TechType.SmallMelon] = 0F;
			itemDecoRatings[TechType.Melon] = 0F;
			itemDecoRatings[TechType.MelonPlant] = 0F;
			itemDecoRatings[TechType.BulboTreePiece] = 0.05F;
			itemDecoRatings[TechType.BulboTree] = 0.05F;
			itemDecoRatings[TechType.JellyPlantSeed] = -0.2F;
			itemDecoRatings[TechType.JellyPlant] = -0.2F;
			itemDecoRatings[TechType.FernPalmSeed] = 0.1F;
			itemDecoRatings[TechType.FernPalm] = 0.1F;
		}

		private float getLeisureDecoThreshold(BaseCell cell) {
			float val = AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.LEISUREDECO);
			if (QModManager.API.QModServices.Main.ModPresent("FCSAlterraHub"))
				val = val * 1.25F + 5;
			if (this.isLargeRoom(cell))
				val *= 1.5F;
			return val;
		}

		private bool isLargeRoom(BaseCell cell) {
			if (!cell)
				return false;
			return false; //TODO 2.0
		}

		public void setDisplayValue(TechType tt, float value) {
			itemDecoRatings[tt] = value;
		}

		public void registerModdedObject(ModPrefab pfb, float deco, params RoomTypes[] types) {
			if (pfb == null)
				return;
			this.registerModdedObject(pfb.ClassID, deco, types);
			this.setDisplayValue(pfb.TechType, deco);
			if (pfb is BasicCustomPlant) {
				BasicCustomPlantSeed seed = ((BasicCustomPlant)pfb).seed;
				if (seed != null) {
					this.registerModdedObject(seed.ClassID, deco, types);
					this.setDisplayValue(seed.TechType, deco);
				}
			}
		}

		public void registerModdedObject(string pfb, float deco, params RoomTypes[] types) {
			if (types.Length > 0)
				objectTypeMappings[pfb] = types;
			if (!Mathf.Approximately(deco, 0))
				decoRatings[pfb] = deco;
		}

		public static void debugRoomValues() {
			BaseCell bc = AEHooks.getCurrentPlayerRoom();
			BaseRoot bb = bc.gameObject.FindAncestor<BaseRoot>();
			instance.getType(bb, bc, ObjectUtil.getBaseObjectsInRoom(bb, bc), out float trash, true);
		}

		internal RoomTypes getType(BaseRoot bb, BaseCell bc, List<PrefabIdentifier> li, out float decoRating, bool debug = false) {
			debug |= debugRoomCompute;
			HashSet<RoomTypes> options = new HashSet<RoomTypes>((IEnumerable<RoomTypes>)Enum.GetValues(typeof(RoomTypes)));
			//if (bc.GetComponentInChildren<BaseNuclearReactor>() || bc.GetComponentInChildren<BaseBioReactor>())
			//	options.Add(RoomTypes.POWER);
			int lockerCount = 0;
			int agriCount = 0;
			DecoRatingTracker deco = new DecoRatingTracker();
			bool large = this.isLargeRoom(bc);
			foreach (PrefabIdentifier pi in li) {
				Constructable cc = pi.GetComponent<Constructable>();
				if (cc && !cc.constructed) {
					deco.addDeco(-2F);
					continue;
				}
				RoomTypes[] obj = this.getObjectType(pi);
				//if (obj.Length == 1)
				//	options.AddRange(obj);
				if (obj.Length > 1 || (obj.Length == 1 && obj[0] != RoomTypes.UNSPECIALIZED)) //do not rule out all other room types just because of an unspecialized item
					options.IntersectWith(obj);
				if (lockers.Contains(pi.ClassId))
					lockerCount++;
				if (Array.IndexOf(obj, RoomTypes.AGRICULTURAL) >= 0)
					agriCount++;
				this.getDecoRating(pi, deco, large, debug);
				//SNUtil.writeToChat("Cell "+bc.transform.position+": Object "+pi.name+" > "+getObjectType(pi).toDebugString()+" #"+getDecoRating(pi));
			}
			bool hasGlassRoof = bc.gameObject.getChildObject("BaseRoomInteriorTopGlass") != null;
			int plantPanels = bc.gameObject.getChildObjects("BaseRoomPlanterSide").Count;
			int windows = bc.gameObject.getChildObjects("BaseRoomWindowSide").Count;
			bool sideWindows = windows > 0;
			if (hasGlassRoof)
				windows += 3; //counts as 3 windows
			deco.addDeco(plantPanels * 1.5F); //plant panels, 1.5 each
			if (!AqueousEngineeringMod.config.getBoolean(AEConfig.ConfigEntries.ROOMENVIRODECO))
				windows = 0;
			if (windows > 0) {
				deco.addDeco(windows * this.getWindowDecoValue(bb, bc, hasGlassRoof, debug)); //windows, rating is base location dependent
				if (sideWindows) {
					HashSet<int> seen = new HashSet<int>();
					WorldUtil.getObjectsNear<GameObject>(bc.transform.position, 25, go => {
						if (go.activeSelf && !seen.Contains(go.GetInstanceID()) && Mathf.Abs(go.transform.position.y - bc.transform.position.y) <= 10) {
							seen.Add(go.GetInstanceID());
							Planter p = go.GetComponent<Planter>();
							if (p && !p.isIndoor) {
								this.getInventoryDecoValue(p.GetComponent<StorageContainer>(), deco, debug/*, 0.8F*/);
								//if (debug)
								//	SNUtil.log("Found nearby exterior planter " + p.name + ": +" + add + " (" + p.GetComponent<StorageContainer>().container.GetItemTypes().Select<TechType, string>(tt => Language.main.Get(tt.AsString())).toDebugString());

							}
						}
					});
				}
			}
			decoRating = deco.totalRating;
			if (debug)
				SNUtil.writeToChat("Room at " + bc.transform.position + " has options " + options.toDebugString() + " & deco value " + decoRating + " (" + plantPanels + "/" + windows + "*" + this.getWindowDecoValue(bb, bc, hasGlassRoof, debug) + ")");
			int lockerThresh = large ? 8 : 5;
			if (lockerCount >= lockerThresh) { //do before leisure/agri are removed
				options.Add(RoomTypes.STORAGE);
				if (options.Count == Enum.GetValues(typeof(RoomTypes)).Length) { //if only lockers + other generics, will not have any filtering
					options.Clear();
					options.Add(RoomTypes.STORAGE);
				}
			}
			if (decoRating < this.getLeisureDecoThreshold(bc))
				options.Remove(RoomTypes.LEISURE);
			if (agriCount < (large ? 5 : 3))
				options.Remove(RoomTypes.AGRICULTURAL);
			if (options.Count == 2 && options.Contains(RoomTypes.UNSPECIALIZED)) //if unspecialized + one thing, choose that one thing
				options.Remove(RoomTypes.UNSPECIALIZED);
			if (debug) {
				string msg = "Net options "+options.toDebugString()+" from "+lockerCount+"/"+agriCount;
				SNUtil.writeToChat(msg);
				SNUtil.log(msg);
			}
			return options.Count == 1 ? options.First() : RoomTypes.UNSPECIALIZED;
		}

		private float getWindowDecoValue(BaseRoot bb, BaseCell bc, bool hasGlassRoof, bool debug) {
			Vector3 pos = bc.transform.position;
			if (pos.y >= -1)
				return 0.1F;
			float depth = -pos.y;
			BiomeBase b = BiomeBase.getBiome(pos);
			float scenery = Mathf.Clamp(b.sceneryValue, -1, 2.5F);
			if (scenery > 0 && scenery < 0.1F)
				scenery = 0.1F;
			if ((b == VanillaBiomes.REDGRASS || b == VanillaBiomes.MUSHROOM) && depth < 50) {
				scenery = 0;
			}
			else if ((b == VanillaBiomes.KOOSH) && depth < 100) {
				scenery = 0;
			}
			else if ((b == VanillaBiomes.CRAG || b == VanillaBiomes.SPARSE || b == VanillaBiomes.GRANDREEF) && depth < 150) {
				scenery = 0;
			}
			else if ((b == VanillaBiomes.DUNES || b == VanillaBiomes.BLOODKELPNORTH) && depth < 200) {
				scenery = 0;
			}
			else if ((b == VanillaBiomes.MOUNTAINS || b == VanillaBiomes.TREADER) && depth < 250) {
				scenery = 0;
			}
			else if (b == VanillaBiomes.LOSTRIVER) {
				string biome = WaterBiomeManager.main.GetBiome(pos, false).ToLowerInvariant();
				if (biome.Contains("bonesfield"))
					scenery = 1.5F;
				else if (biome.Contains("ghosttree") || biome.Contains("junction"))
					scenery = 1.0F;
			}
			if (debug)
				SNUtil.log("Biome " + b.displayName + " scenery value is " + scenery);
			if (scenery > 0) {
				int objectsFound = 0;
				int totalFound = 0;

				WorldUtil.getObjectsNear<GameObject>(pos, 100, go => {
					if (go.activeSelf && go.transform.position.y >= pos.y - 50 && (hasGlassRoof || go.transform.position.y <= pos.y + 50) && this.isExternalPropObject(bb, go.transform)) {
						totalFound++;
						if (go.FindAncestor<PrefabIdentifier>())
							objectsFound++;
					}
				});
				if (totalFound <= 50) //basically nothing found, must be open water (will never be zero beacuse of ocean, occasional fish, etc)
					scenery = 0;
				else if (objectsFound <= 200) //terrain only, or terrain plus only a handful of things, still many because of things like grass
					scenery *= 0.33F;
				if (debug)
					SNUtil.log("Found near-room outdoor objects: " + objectsFound + "/" + totalFound);
			}
			float ret = 0.75F*scenery;
			IEcoTarget tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, pos, null, 8);
			if (debug)
				SNUtil.log("Nearby leviathan: " + (tgt != null ? tgt.GetGameObject().name : "None"));
			if (tgt != null) {
				float dist = Vector3.Distance(tgt.GetPosition(), pos);
				if (dist <= 250) {
					Creature c = tgt.GetGameObject().GetComponent<Creature>();
					float val = (c is ReaperLeviathan || c is GhostLeviatanVoid || c is GhostLeviathan || c is SeaDragon) ? 1.5F : 1;
					ret += val * (float)MathUtil.linterpolate(dist, 80, 250, 1, 0, true);
				}
			}
			tgt = EcoRegionManager.main.FindNearestTarget(EcoTargetType.HeatArea, pos, null, 4);
			if (debug)
				SNUtil.log("Nearby heat area: " + (tgt != null ? tgt.GetGameObject().name : "None"));
			if (tgt != null) {
				float dist = Vector3.Distance(tgt.GetPosition(), pos);
				if (dist <= 80) {
					ret += tgt.GetGameObject().GetComponent<Geyser>() ? 1.5F : 0.5F;
				}
			}
			return ret;
		}

		private bool isExternalPropObject(BaseRoot bb, Component c) {
			Transform t = c.transform;
			while (t != null) {
				if (t == bb.transform || t == Player.main.transform)
					return false;
				Planter p = t.GetComponent<Planter>();
				if (p)
					return !p.isIndoor;
				t = t.parent;
			}
			return true;
		}

		private void getDecoRating(PrefabIdentifier pi, DecoRatingTracker deco, bool largeRoom, bool debug) {
			PictureFrame pf = pi.GetComponent<PictureFrame>();
			if (pf) {
				deco.addObject(pi, pf.current == PictureFrame.State.None ? -1 : 3, debug, 9999);
				return;
			}
			Sign sg = pi.GetComponent<Sign>();
			if (sg) {
				string text = sg.GetComponentInChildren<uGUI_SignInput>().text;
				float amt = string.IsNullOrEmpty(text) || text.Equals("sign", StringComparison.InvariantCultureIgnoreCase) ? 0 : 0.25F;
				if (amt > 0) {
					deco.addObject(pi, amt, debug);
				}
				return;
			}
			if (pi.ClassId == STANDING_LOCKER_PREFAB && QModManager.API.QModServices.Main.ModPresent("lockerMod")) { //locker content display
				this.getInventoryDecoValue(pi.GetComponent<StorageContainer>(), deco, debug, 0.2F); //20% value since it contains many many items, and they are small
			}
			ItemDisplayLogic disp = pi.GetComponent<ItemDisplayLogic>();
			if (disp) {
				TechType tt = disp.displayType;
				Pickupable pp = disp.currentItem;
				float amt = 0;
				if (tt == TechType.None || !pp)
					amt = -0.25F;
				else
					amt = this.getItemDecoValue(disp.displayType);
				if (amt > 0)
					deco.addObject(tt, pp.gameObject, amt, debug, 1);
				else
					deco.addObject(pi, amt, debug);
				return;
			}
			Planter p = pi.GetComponent<Planter>();
			if (p) {
				deco.addObject(pi, this.getDecoRating(pi.ClassId), debug);
				this.getInventoryDecoValue(p.GetComponent<StorageContainer>(), deco, debug, repLimit:3);
				return;
			}
			Aquarium a = pi.GetComponent<Aquarium>();
			if (a) {
				deco.addObject(pi, 2, debug); //even empty has some value
				this.getInventoryDecoValue(a.GetComponent<StorageContainer>(), deco, debug, repLimit: 3);
				return;
			}
			ACUCallbackSystem.ACUCallback wp = pi.GetComponent<ACUCallbackSystem.ACUCallback>();
			if (wp) {
				this.getACUDecoValue(wp, largeRoom, deco, debug);
				return;
			}
			deco.addObject(pi, this.getDecoRating(pi.ClassId), debug);
		}

		private void getInventoryDecoValue(StorageContainer sc, DecoRatingTracker deco, bool debug, float factor = 1, int repLimit = 2) {
			getInventoryDecoValue(sc.storageRoot.GetComponentsInChildren<Pickupable>(true), deco, debug, factor, repLimit);
		}

		private void getInventoryDecoValue(IEnumerable<Pickupable> li, DecoRatingTracker deco, bool debug, float factor = 1, int repLimit = 2) {
			foreach (Pickupable pp in li) {
				deco.addObject(pp.GetTechType(), pp.gameObject, this.getItemDecoValue(pp) * factor, debug, repLimit);
			}
		}

		private void getACUDecoValue(ACUCallbackSystem.ACUCallback wp, bool largeRoom, DecoRatingTracker deco, bool debug) {
			float baseVal = wp.consistentBiome ? BiomeBase.getBiome(wp.currentTheme.baseBiome).sceneryValue * (largeRoom ? 3 : 2) : 1;
			deco.addDeco(baseVal); //even empty has some value; can do directly since never more than one ACU per room
			getInventoryDecoValue(wp.acu.items.Select(wpi => wpi.GetComponent<Pickupable>()), deco, debug, 0.5F);
			getInventoryDecoValue(wp.acu.planter.GetComponent<StorageContainer>(), deco, debug);
		}

		private float getDecoRating(string classID) {
			return decoRatings.ContainsKey(classID) ? decoRatings[classID] : 0;
		}

		private float getItemDecoValue(Pickupable pp) {
			TechType tt = pp.GetTechType();
			float ret = this.getItemDecoValue(tt);
			if (tt == TechType.Peeper && pp.GetComponent<Peeper>().isHero)
				ret = 1.0F;
			//SNUtil.writeToChat("Deco value of inv item "+pp+" ("+tt+"): "+ret);
			return ret;
		}

		public float getItemDecoValue(TechType tt) {
			return itemDecoRatings.ContainsKey(tt) ? itemDecoRatings[tt] : 0;
		}

		internal RoomTypes[] getObjectType(PrefabIdentifier pi) {
			return getObjectType(pi.gameObject, pi.ClassId);
		}

		internal RoomTypes[] getObjectType(GameObject go, string id) {
			return go.GetComponent<Charger>() ? (new RoomTypes[] { RoomTypes.MECHANICAL }) : go.GetComponent<Crafter>() ? (new RoomTypes[] { RoomTypes.WORK }) : objectTypeMappings.ContainsKey(id) ? objectTypeMappings[id] : (new RoomTypes[] { RoomTypes.UNSPECIALIZED });
		}

		internal RoomTypes getSavedType(Component go) {
			return this.getSavedType(go, out float deco, out float decoThresh);
		}

		internal RoomTypes getSavedType(Component go, out float deco, out float decoThresh) {
			RoomTypeTracker rt = go.gameObject.FindAncestor<RoomTypeTracker>(); //will find the one on the main GO, else BaseCell if possible
			if (!rt) {
				BaseRoot bb = go.GetComponentInParent<BaseRoot>();
				if (bb) {
					BaseCell bc = ObjectUtil.getBaseRoom(bb, go.gameObject);
					if (bc)
						rt = bc.GetComponent<RoomTypeTracker>();
				}
			}
			decoThresh = rt ? this.getLeisureDecoThreshold(rt.getRoom()) : 9999;
			deco = rt ? rt.getDecorationValue() : 0;
			return rt ? rt.getType() : RoomTypes.UNSPECIALIZED;
		}

		public RoomTypes getPlayerRoomType(Player ep) {
			return this.getPlayerRoomType(ep, out float deco, out float decoThresh);
		}

		public RoomTypes getPlayerRoomType(Player ep, out float deco, out float decoThresh) {
			BaseCell bc = AEHooks.getCurrentPlayerRoom();
			decoThresh = this.getLeisureDecoThreshold(bc);
			if (!bc) {
				deco = 0;
				return RoomTypes.UNSPECIALIZED;
			}
			RoomTypeTracker rt = bc.GetComponent<RoomTypeTracker>();
			deco = rt ? rt.getDecorationValue() : 0;
			return rt ? rt.getType() : RoomTypes.UNSPECIALIZED;
		}

		public bool storageHasDecoValue(StorageContainer sc) {
			PrefabIdentifier pi = sc.GetComponent<PrefabIdentifier>();
			return (pi && pi.ClassId == AqueousEngineeringMod.displayBlock.ClassID) || (pi && pi.ClassId == STANDING_LOCKER_PREFAB && QModManager.API.QModServices.Main.ModPresent("lockerMod")) || sc.GetComponent<Aquarium>() || sc.GetComponent<Planter>();
		}

		public void updateRoom(GameObject go, bool notify = true) {
			if (debugRoomCompute)
				SNUtil.log("Recomputing room " + go.transform.GetChild(0).name + " via " + SNUtil.getStacktrace());
			BaseRoot bb = go.FindAncestor<BaseRoot>();
			if (!bb) {
				//SNUtil.writeToChat("No base for "+go+", not attempting room type update");
				return;
			}
			BaseCell cell = ObjectUtil.getBaseRoom(bb, go);
			if (!cell) {
				if (debugRoomCompute)
					SNUtil.log("No room for " + go.transform.GetChild(0).name + ", queuing update for later");
				this.queueRoomUpdate(go);
				return;
			}
			this.recomputeBaseRoom(bb, cell, notify);
		}

		public void recomputeBaseRoom(BaseRoot bb, BaseCell cell, bool notify) {
			RoomTypeTracker tr = cell.GetComponent<RoomTypeTracker>();
			if (tr && tr.justComputed)
				return;
			RoomTypes prev = tr ? tr.getType() : RoomTypes.UNSPECIALIZED;
			float prevD = tr ? tr.getDecorationValue() : 0;
			List<PrefabIdentifier> li = ObjectUtil.getBaseObjectsInRoom(bb, cell);
			//SNUtil.writeToChat("Checking room type for "+go);
			RoomTypes type = this.getType(bb, cell, li, out float deco);
			//SNUtil.writeToChat("Room at "+cell.transform.position+" is now type "+type+"; decoration rating = "+deco.ToString("0.00"));
			string name = AqueousEngineeringMod.roomLocale.getEntry(Enum.GetName(typeof(RoomTypes), type)).desc;
			if (debugRoomCompute || (notify && AqueousEngineeringMod.config.getBoolean(AEConfig.ConfigEntries.ROOMCHAT) && (prev != type || !Mathf.Approximately(prevD, deco)))) {
				string msg = "This room is now "+name+", with a decoration rating of "+deco.ToString("0.00");
				if (deco > 0)
					msg += " (" + (deco * 100F / this.getLeisureDecoThreshold(cell)).ToString("0.00") + "%)";
				SNUtil.writeToChat(msg);
				if (debugRoomCompute)
					SNUtil.log(cell.transform.GetChild(0).gameObject.name + ": " + msg);
			}
			cell.gameObject.EnsureComponent<RoomTypeTracker>().setType(type, cell, null, deco);
			foreach (PrefabIdentifier pi in li) {
				pi.gameObject.EnsureComponent<RoomTypeTracker>().setType(type, cell, pi, deco);
			}
		}

		private void queueRoomUpdate(GameObject go) {
			go.AddComponent<RoomUpdateQueue>().Invoke("recompute", 0.5F);
		}

		public void recomputeBaseRooms(BaseRoot root, float delay = 0) {
			if (delay > 0) {
				BaseUpdateQueue q = root.gameObject.AddComponent<BaseUpdateQueue>();
				q.delay = delay;
				q.Invoke("recompute", delay);
			}
			else {
				BaseCell[] bc = root.GetComponentsInChildren<BaseCell>();
				if (debugRoomCompute)
					SNUtil.log("Recomputing deco in base @ " + WorldUtil.getRegionalDescription(root.transform.position, true) + " (" + root.transform.position + ") with " + bc.Length + " rooms");
				foreach (BaseCell cell in bc) {
					this.recomputeBaseRoom(root, cell, debugRoomCompute);
				}
			}
		}

		public enum RoomTypes {
			UNSPECIALIZED,
			STORAGE, //storage +1 row and col
			POWER, //generators +25%
			MECHANICAL, //machine (AE, C2C, vanilla [water filter] etc) power cost -20%, charger speed +50%
			AGRICULTURAL, //+33% harvests per plant; eatables obtained this way have +25% to food and water
			WORK, //food and water rate -20% to -25% (-1% per deco), fab speed +50%
			LEISURE, //food and water rate -67% to -80% (-2% per surplus deco), sleeping in regenerates 15-20 health (15 + surplus deco up to +5)
			ACU, //creature capacity +5, ecosystems slightly more lenient, poo rate +50%
		}

		class BaseUpdateQueue : MonoBehaviour {

			internal float delay;

			void recompute() {
				if (!DIHooks.isWorldLoaded()) {
					this.Invoke("recompute", delay);
					return;
				}
				instance.recomputeBaseRooms(this.GetComponent<BaseRoot>());
				this.destroy(false);
			}

		}

		class RoomUpdateQueue : MonoBehaviour {

			void recompute() {
				if (!DIHooks.isWorldLoaded()) {
					this.Invoke("recompute", 0.5F);
					return;
				}
				instance.updateRoom(gameObject);
				this.destroy(false);
			}

		}

		class DecoRatingTracker {

			internal float totalRating { get; private set; }
			private readonly Dictionary<string, float> totalContribution = new Dictionary<string, float>();
			private readonly CountMap<string> objectCount = new CountMap<string>();

			internal void addDeco(float amt) {
				totalRating += amt;
			}

			internal void addObject(TechType tt, GameObject go, float deco, bool debug, int repLimit = 2) {
				addObject(CraftData.GetClassIdForTechType(tt), go, deco, debug, repLimit);
			}

			internal void addObject(PrefabIdentifier pi, float deco, bool debug, int repLimit = 2) {
				addObject(pi.ClassId, pi.gameObject, deco, debug, repLimit);
			}

			internal void addObject(string id, GameObject go, float deco, bool debug, int repLimit = 2) { //start penalizing on the third by default
				if (string.IsNullOrEmpty(id))
					return;
				int hasc = objectCount[id];
				if (deco > 0 && hasc > 0) {
					deco *= getRepeatedDecoPenaltyFactor(hasc+1, repLimit);
				}
				totalRating += deco;
				float has = totalContribution.ContainsKey(id) ? totalContribution[id] : 0;
				totalContribution[id] = has+deco;
				objectCount.add(id);
				if (debug)
					SNUtil.log("Processed room data for "+go.name + ": " + instance.getObjectType(go, id).toDebugString() + ", deco value = " + deco);
			}

			private float getRepeatedDecoPenaltyFactor(int idx, int safe) { //idx being "this is the Nth" (up to 'safe' is unpenalized)
				return idx <= safe ? 1 : 1F / (1F + (0.5F * (idx - safe))); //67% for 3rd, 50% for 4th, 40% for 5th, 33% for 6th, etc
			}

		}

		internal class RoomTypeTracker : MonoBehaviour {

			private RoomTypes roomType;
			private PrefabIdentifier prefab;
			private BaseCell room;
			private float decoRating;

			private float lastComputeTime = -1;

			public bool justComputed {
				get {
					return DayNightCycle.main.timePassedAsFloat - lastComputeTime <= 0.5F;
				}
			}


			internal void setType(RoomTypes type, BaseCell bc, PrefabIdentifier pi, float deco) {
				lastComputeTime = DayNightCycle.main.timePassedAsFloat;
				decoRating = deco;
				if (roomType == type && room == bc && prefab == pi)
					return;
				this.unApplyBonuses();
				//SNUtil.writeToChat("Initializing "+(pi ? pi.name : "room")+" in room "+bc.transform+" to "+type);
				roomType = type;
				room = bc;
				prefab = pi;
				this.applyTypeBonusesToObject();
			}

			internal RoomTypes getType() {
				return roomType;
			}

			internal float getDecorationValue() {
				return decoRating;
			}

			internal BaseCell getRoom() {
				return room;
			}

			private void applyTypeBonusesToObject() {
				if (!prefab) //is basecell itself
					return;
				switch (roomType) {
					case RoomTypes.STORAGE:
						if (lockers.Contains(prefab.ClassId)) {
							StorageContainer sc = prefab.GetComponent<StorageContainer>();
							StorageContainer refSc = ObjectUtil.lookupPrefab(prefab.ClassId).GetComponent<StorageContainer>();
							sc.Resize(refSc.width + 1, refSc.height + (prefab.ClassId == BASIC_LOCKER_PREFAB ? 2 : 1));
						}
						break;
					case RoomTypes.ACU:
						if (prefab.ClassId == ACU_PREFAB) {
							WaterPark wp = prefab.GetComponent<WaterPark>();
							WaterPark refWp = ObjectUtil.lookupPrefab(prefab.ClassId).GetComponent<WaterPark>();
							wp.wpPieceCapacity = refWp.wpPieceCapacity + 5;
						}
						break;
				}
			}

			private void unApplyBonuses() {
				if (!prefab) //is basecell itself
					return;
				switch (roomType) {
					case RoomTypes.STORAGE:
						if (lockers.Contains(prefab.ClassId)) {
							StorageContainer sc = prefab.GetComponent<StorageContainer>();
							StorageContainer refSc = ObjectUtil.lookupPrefab(prefab.ClassId).GetComponent<StorageContainer>();
							sc.Resize(refSc.width, refSc.height);
						}
						break;
					case RoomTypes.ACU:
						if (prefab.ClassId == ACU_PREFAB) {
							WaterPark wp = prefab.GetComponent<WaterPark>();
							WaterPark refWp = ObjectUtil.lookupPrefab(prefab.ClassId).GetComponent<WaterPark>();
							wp.wpPieceCapacity = refWp.wpPieceCapacity;
						}
						break;
				}
			}

		}
	}

}
