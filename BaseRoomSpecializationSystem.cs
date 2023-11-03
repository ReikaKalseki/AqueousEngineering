﻿using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using Story;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;

namespace ReikaKalseki.AqueousEngineering {
	
	public class BaseRoomSpecializationSystem {
		
		private static readonly HashSet<string> lockers = new HashSet<string>();
		private static readonly Dictionary<string, float> decoRatings = new Dictionary<string, float>();
		private static readonly Dictionary<TechType, float> itemDecoRatings = new Dictionary<TechType, float>();
		private static readonly Dictionary<string, RoomTypes[]> objectTypeMappings = new Dictionary<string, RoomTypes[]>();
		
		public static readonly BaseRoomSpecializationSystem instance = new BaseRoomSpecializationSystem();
		
		private BaseRoomSpecializationSystem() {
			lockers.Add("367656d6-87d9-42a1-926c-3cf959ea1c85");
			lockers.Add("5fc7744b-5a2c-4572-8e53-eebf990de434");
			lockers.Add("775feb4c-dab9-4322-b4a5-a4289ca1cf6a");			
			lockers.Add("cd34fecd-794c-4a0c-8012-dd81b77f2840");
			
			foreach (string s in lockers)
				objectTypeMappings[s] = new RoomTypes[]{RoomTypes.STORAGE};
			
			objectTypeMappings["d3bf649e-1eaa-4790-a113-8f73286af611"] = new RoomTypes[]{RoomTypes.AGRICULTURAL}; //indoor growbed
			objectTypeMappings["769f9f44-30f6-46ed-aaf6-fbba358e1676"] = new RoomTypes[]{RoomTypes.POWER}; //bioreactor
			objectTypeMappings["864f7780-a4c3-4bf2-b9c7-f4296388b70f"] = new RoomTypes[]{RoomTypes.POWER}; //nuclear reactor
			objectTypeMappings["51eba507-317c-46bf-adde-4459dc8e002e"] = new RoomTypes[]{RoomTypes.LEISURE}; //vending machine
			objectTypeMappings["b09a156d-d3cf-455a-848d-a9a8cad2b811"] = new RoomTypes[]{RoomTypes.LEISURE}; //coffee
			objectTypeMappings["5d63470b-2705-4963-be4b-2e11e8dc6f2e"] = new RoomTypes[]{RoomTypes.LEISURE}; //bench
			objectTypeMappings["4cb154ef-bdb6-4ff4-9107-f378ce21a9b7"] = new RoomTypes[]{RoomTypes.LEISURE}; //bench
			objectTypeMappings["26cdb865-efbd-403c-8873-92453bcfc935"] = new RoomTypes[]{RoomTypes.LEISURE}; //chair
			objectTypeMappings["4f0e304b-0d25-4f9c-a6ba-1b1a6bf029b0"] = new RoomTypes[]{RoomTypes.LEISURE, RoomTypes.WORK}; //chair
			objectTypeMappings["cbeca4bd-cba4-4905-89fd-2470aaa204b1"] = new RoomTypes[]{RoomTypes.LEISURE, RoomTypes.WORK}; //chair
			objectTypeMappings["cf522a95-3038-4759-a53c-8dad1242c8ed"] = new RoomTypes[]{RoomTypes.LEISURE, RoomTypes.WORK}; //desk
			objectTypeMappings["7370e7a0-ebc0-4c33-9997-7084c11a55b0"] = new RoomTypes[]{RoomTypes.LEISURE, RoomTypes.WORK}; //counter
			objectTypeMappings["cdb374fd-4f38-4bef-86a3-100cc87155b6"] = new RoomTypes[]{RoomTypes.LEISURE}; //bed
			objectTypeMappings["c3994649-d0da-4f8c-bb77-1590f50838b9"] = new RoomTypes[]{RoomTypes.LEISURE}; //bed
			objectTypeMappings["dffaf40f-9fbc-4553-9b35-3f939c76c283"] = new RoomTypes[]{RoomTypes.LEISURE}; //bed
			objectTypeMappings["2f2d8419-c55b-49ac-9698-ecb431fffed2"] = new RoomTypes[]{RoomTypes.MECHANICAL}; //water filter			
			objectTypeMappings["cdade216-3d4d-4adf-901c-3a91fb3b88c4"] = new RoomTypes[]{RoomTypes.WORK}; //centrifuge		
			objectTypeMappings["bef7bc0b-149d-4342-bbb4-329047685578"] = new RoomTypes[]{RoomTypes.WORK}; //fragment analyzer
			objectTypeMappings["c0175cf7-0b6a-4a1d-938f-dad0dbb6fa06"] = new RoomTypes[]{RoomTypes.WORK}; //medical cabinet
			objectTypeMappings["c9bdcc4d-a8c6-43c0-8f7a-f86841cd4493"] = new RoomTypes[]{RoomTypes.WORK}; //specimen analyzer
			objectTypeMappings["5c06baec-0539-4f26-817d-78443548cc52"] = new RoomTypes[]{RoomTypes.LEISURE, RoomTypes.WORK}; //radio			
			
			decoRatings["26cdb865-efbd-403c-8873-92453bcfc935"] = 0.15F; //best chair	
			decoRatings["cbeca4bd-cba4-4905-89fd-2470aaa204b1"] = 0.1F; //chair		
			
			decoRatings["5c06baec-0539-4f26-817d-78443548cc52"] = -0.25F; //radio		

			decoRatings["cdb374fd-4f38-4bef-86a3-100cc87155b6"] = 0.25F; //double bed + extra sheet
			decoRatings["c3994649-d0da-4f8c-bb77-1590f50838b9"] = 0.1F; //bed
			decoRatings["dffaf40f-9fbc-4553-9b35-3f939c76c283"] = 0.2F; //double bed
			
			decoRatings["336f276f-9546-40d0-98cb-974994dee3bf"] = 0.15F; //prawn poster
			decoRatings["d76dd251-492d-4bf9-8adb-25e59d709df2"] = 0.15F; //prawn poster 2
			decoRatings["876cbea4-b4bf-4311-8264-5118bfef291c"] = 0.4F; //aurora poster
			decoRatings["72da21f9-f3e2-4183-ac57-d3679fb09122"] = 0.05F; //shooter poster
			decoRatings["d809cb15-6784-4f7c-bf5d-f7d0c5bf8546"] = 0.15F; //cat poster
			decoRatings["c0d320d2-537e-4128-90ec-ab1466cfbbc3"] = 1F; //toy aurora
			
			decoRatings["1d1898ca-8436-4fe4-aaf4-a1d9fa6d58cb"] = 1; //bar table
			decoRatings["51eba507-317c-46bf-adde-4459dc8e002e"] = -2; //vending machine
			decoRatings["f1cde32e-101a-4dd5-8084-8c950b9c2432"] = -5; //trashcan
			decoRatings["bedc40fb-bd97-4b4d-a943-d39360c9c7bd"] = -3; //lab trashcan
			decoRatings["cf1df719-905c-4385-98da-b638fdfd53f7"] = 0.25F; //shelf
			decoRatings["c5ae1472-0bdc-4203-8418-fb1f74c8edf5"] = 0.5F; //shelf
			decoRatings["cf522a95-3038-4759-a53c-8dad1242c8ed"] = 0.5F; //desk
			decoRatings["7370e7a0-ebc0-4c33-9997-7084c11a55b0"] = 0.25F; //counter
			decoRatings["274bd60f-16c4-4810-911b-c5562fe7c2d8"] = 1.5F; //plant panel
			decoRatings["b343166e-3a17-4a1c-85d1-05dee8ec1575"] = 0.25F; //sign
			decoRatings["07a05a2f-de55-4c60-bfda-cedb3ab72b88"] = 0.25F; //jack eye
			decoRatings["4b8cd269-6646-42d0-b8a0-9a40ef0c07d0"] = 0.5F; //toy car
			decoRatings["7cdcbed0-7d20-43c4-beb4-f1ad539b2a76"] = 0.5F; //toy car
			decoRatings["ad5e149b-d35c-4b46-bb4e-b4c0a9c6e668"] = 0.25F; //markiplier
			decoRatings["cb89366d-eac0-4011-8665-fafde75b215c"] = 0.25F; //markiplier
			decoRatings["f7e26c44-bb28-4979-8f83-76ed529979fc"] = 0.25F; //markiplier
			decoRatings["c96baff4-0993-4893-8345-adb8709901a7"] = 0.33F; //toy cat
			
			//foreach (string s in decoRatings.Keys)
			//	objectTypeMappings[s] = RoomTypes.LEISURE;
			
			itemDecoRatings[TechType.Peeper] = 0.05F;
			itemDecoRatings[TechType.Boomerang] = 0.05F;
			itemDecoRatings[TechType.Spinefish] = 0.05F;
			itemDecoRatings[TechType.HoleFish] = 0.05F;
			itemDecoRatings[TechType.Oculus] = 0.1F;
			itemDecoRatings[TechType.Hoopfish] = 0.2F;
			itemDecoRatings[TechType.Hoverfish] = 0.2F;
			itemDecoRatings[TechType.LavaBoomerang] = 0.15F;
			itemDecoRatings[TechType.Eyeye] = -0.05F;
			itemDecoRatings[TechType.LavaEyeye] = -0.05F;
			itemDecoRatings[TechType.GarryFish] = -0.1F;
			itemDecoRatings[TechType.Spadefish] = -0.1F;
			itemDecoRatings[TechType.Reginald] = 0;
			
			itemDecoRatings[TechType.CreepvineSeedCluster] = 0.1F;
			itemDecoRatings[TechType.CreepvinePiece] = 0;
			itemDecoRatings[TechType.BloodOil] = -0.25F;
			itemDecoRatings[TechType.SnakeMushroomSpore] = 0.25F;
			itemDecoRatings[TechType.EyesPlantSeed] = 0F;
			itemDecoRatings[TechType.GabeSFeatherSeed] = 0.1F;
			itemDecoRatings[TechType.RedGreenTentacleSeed] = 0.1F;
			itemDecoRatings[TechType.SpikePlantSeed] = -0.2F;
			itemDecoRatings[TechType.MembrainTreeSeed] = 0.2F;
			itemDecoRatings[TechType.RedConePlantSeed] = 0.1F;
			itemDecoRatings[TechType.KooshChunk] = 0F;
			itemDecoRatings[TechType.SpottedLeavesPlantSeed] = 0F;
			itemDecoRatings[TechType.PurpleFanSeed] = 0F;
			itemDecoRatings[TechType.PurpleBranchesSeed] = 0F;
			itemDecoRatings[TechType.PurpleStalkSeed] = 0.25F;
			itemDecoRatings[TechType.AcidMushroomSpore] = 0F;
			itemDecoRatings[TechType.WhiteMushroomSpore] = 0F;
			itemDecoRatings[TechType.RedBasketPlantSeed] = 0.33F;
			itemDecoRatings[TechType.ShellGrassSeed] = 0F;
			itemDecoRatings[TechType.RedRollPlantSeed] = 0.2F;
			itemDecoRatings[TechType.RedBushSeed] = 0.05F;
			itemDecoRatings[TechType.SeaCrownSeed] = 0.15F;
			itemDecoRatings[TechType.PurpleTentacleSeed] = 0.1F;
			itemDecoRatings[TechType.BluePalmSeed] = 0.05F;
			itemDecoRatings[TechType.SmallFanSeed] = 0.05F;
			
			itemDecoRatings[TechType.PinkFlowerSeed] = 0.1F;
			itemDecoRatings[TechType.PurpleRattleSpore] = -0.05F;
			itemDecoRatings[TechType.PurpleVegetablePlant] = -0.1F;
			itemDecoRatings[TechType.PurpleVegetable] = -0.1F;
			itemDecoRatings[TechType.PurpleVasePlantSeed] = 0.4F;
			itemDecoRatings[TechType.HangingFruit] = 0.5F;
			itemDecoRatings[TechType.OrangeMushroomSpore] = -0.25F;
			itemDecoRatings[TechType.OrangePetalsPlantSeed] = 0.05F;
			itemDecoRatings[TechType.MelonSeed] = 0F;
			itemDecoRatings[TechType.SmallMelon] = 0F;
			itemDecoRatings[TechType.Melon] = 0F;
			itemDecoRatings[TechType.MelonPlant] = 0F;
			itemDecoRatings[TechType.BulboTreePiece] = 0F;
			itemDecoRatings[TechType.JellyPlantSeed] = -0.2F;
			itemDecoRatings[TechType.FernPalmSeed] = 0.1F;
		}
		
		public void registerModdedObject(ModPrefab pfb, float deco, params RoomTypes[] types) {
			if (pfb == null)
				return;
			registerModdedObject(pfb.ClassID, deco, types);
			if (pfb is BasicCustomPlant) {
				BasicCustomPlantSeed seed = ((BasicCustomPlant)pfb).seed;
				if (seed != null)
					registerModdedObject(seed.ClassID, deco, types);
			}
		}
		
		public void registerModdedObject(string pfb, float deco, params RoomTypes[] types) {
			if (types.Length > 0)
				objectTypeMappings[pfb] = types;
			if (!Mathf.Approximately(deco, 0))
				decoRatings[pfb] = deco;
		}
		
		internal RoomTypes getType(BaseCell bc, List<PrefabIdentifier> li) {
			HashSet<RoomTypes> options = new HashSet<RoomTypes>();
			//if (bc.GetComponentInChildren<BaseNuclearReactor>() || bc.GetComponentInChildren<BaseBioReactor>())
			//	options.Add(RoomTypes.POWER);
			int lockerCount = 0;
			float decoRating = 0;
			foreach (PrefabIdentifier pi in li) {
				options.AddRange(getObjectType(pi));
				if (lockers.Contains(pi.ClassId))
				    lockerCount++;
				if (decoRatings.ContainsKey(pi.ClassId)) {
					decoRating += decoRatings[pi.ClassId];
				}
			}
			//SNUtil.writeToChat("Room at "+bc.transform.position+" has options "+options.toDebugString()+" & deco value "+decoRating);
			if (lockerCount <= 2 && options.Count > 1)
				options.Remove(RoomTypes.STORAGE);
			if (decoRating < 8)
				options.Remove(RoomTypes.LEISURE);
			if (options.Count == 2 && options.Contains(RoomTypes.UNSPECIALIZED)) //if unspecialized + one thing, choose that one thing
				options.Remove(RoomTypes.UNSPECIALIZED);
			return options.Count == 1 ? options.First() : RoomTypes.UNSPECIALIZED;
		}
		
		private float getDecoRating(PrefabIdentifier pi) {
			PictureFrame pf = pi.GetComponent<PictureFrame>();
			if (pf)
				return pf.current == PictureFrame.State.Full ? 4 : -1;
			ItemDisplayLogic disp = pi.GetComponent<ItemDisplayLogic>();
			if (disp)
				return disp.getDecorativeBonus();
			Planter p = pi.GetComponent<Planter>();
			if (p)
				return getInventoryDecoValue(p.GetComponentInChildren<StorageContainer>());
			Aquarium a = pi.GetComponent<Aquarium>();
			if (a)
				return 1+getInventoryDecoValue(a.GetComponentInChildren<StorageContainer>()); //even empty has some value
			return decoRatings.ContainsKey(pi.ClassId) ? decoRatings[pi.ClassId] : 0;
		}
		
		private float getInventoryDecoValue(StorageContainer sc) {
			float ret = 0;
			foreach (Pickupable pp in sc.storageRoot.GetComponentsInChildren<Pickupable>()) {
				TechType tt = pp.GetTechType();
				float val = itemDecoRatings.ContainsKey(tt) ? itemDecoRatings[tt] : 0;
				if (tt == TechType.Peeper && pp.GetComponent<Peeper>().isHero)
					val = 1.5F;
				ret += val;
			}
			return ret;
		}
		
		private RoomTypes[] getObjectType(PrefabIdentifier pi) {
			if (pi.GetComponent<Crafter>() || pi.GetComponent<Charger>())
				return new RoomTypes[]{RoomTypes.WORK};
			if (objectTypeMappings.ContainsKey(pi.ClassId))
				return objectTypeMappings[pi.ClassId];
			return new RoomTypes[]{RoomTypes.UNSPECIALIZED};
		}
		
		internal RoomTypes getSavedType(Component go) {
			RoomTypeTracker rt = go.gameObject.FindAncestor<RoomTypeTracker>(); //will find the one on the main GO, else BaseCell if possible
			if (!rt) {
				BaseRoot bb = go.GetComponentInParent<BaseRoot>();
				if (bb) {
					BaseCell bc = ObjectUtil.getBaseRoom(bb, go.gameObject);
					if (bc)
						rt = bc.GetComponent<RoomTypeTracker>();
				}
			}
			return rt ? rt.getType() : RoomTypes.UNSPECIALIZED;
		}
		
		internal RoomTypes getPlayerRoomType(Player ep) {
			BaseCell bc = AEHooks.getCurrentPlayerRoom();
			if (!bc)
				return RoomTypes.UNSPECIALIZED;
			RoomTypeTracker rt = bc.GetComponent<RoomTypeTracker>();
			return rt ? rt.getType() : RoomTypes.UNSPECIALIZED;
		}
		
		public void updateRoom(GameObject go) {
			BaseRoot bb = go.FindAncestor<BaseRoot>();
			if (!bb)
				return;
			BaseCell cell = ObjectUtil.getBaseRoom(bb, go);
			if (!cell)
				return;
			List<PrefabIdentifier> li = ObjectUtil.getBaseObjectsInRoom(bb, cell);
			RoomTypes type = getType(cell, li);
			SNUtil.writeToChat("Room at "+cell.transform.position+" is now type "+type);
			cell.gameObject.EnsureComponent<RoomTypeTracker>().setType(type, null);
			foreach (PrefabIdentifier pi in li) {
				pi.gameObject.EnsureComponent<RoomTypeTracker>().setType(type, pi);
			}
		}
		
		public enum RoomTypes {
			UNSPECIALIZED,
			STORAGE, //storage +1 row and col
			POWER, //generators +25%
			MECHANICAL, //machine (AE, C2C, vanilla [eg medkit fab, battery charger]) speed +20% or power cost -20%
			AGRICULTURAL, //crop growth speed +20%
			WORK, //food and water rate -20%, fab speed +25%
			LEISURE, //food and water rate -67%, sleeping in regenerates 15 health
			ACU, //breeding rate +10%
		}
		
		internal class RoomTypeTracker : MonoBehaviour {
			
			private RoomTypes roomType;
			
			internal void setType(RoomTypes type, PrefabIdentifier pi) {
				if (roomType == type)
					return;
				roomType = type;
				applyTypeBonusesToObject(pi);
			}
			
			internal RoomTypes getType() {
				return roomType;
			}
		
			private void applyTypeBonusesToObject(PrefabIdentifier pi) {
				if (!pi) //is basecell itself
					return;
				switch(roomType) {
					case RoomTypes.STORAGE:
						if (pi.ClassId == "367656d6-87d9-42a1-926c-3cf959ea1c85" || pi.ClassId == "5fc7744b-5a2c-4572-8e53-eebf990de434" || pi.ClassId == "775feb4c-dab9-4322-b4a5-a4289ca1cf6a") {
							StorageContainer sc = pi.GetComponentInChildren<StorageContainer>();
							sc.Resize(sc.width+1, sc.height+1); //how to reset it? and does it need refresh on reload?
						}
						break;
				}
			}
			
		}
	}
	
}