using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using JetBrains.Annotations;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {

	public static class AEHooks {

		private static readonly Vector3 mountainWreckLaserable = new Vector3(684.46F, -359.33F, 1218.44F);
		private static readonly Vector3 mountainWreckBlock = new Vector3(686.81F, -364.29F, 1223.04F);

		private static BaseCell currentPlayerRoom;
		private static float lastPlayerRoomCheckTime;

		static AEHooks() {
			SNUtil.log("Initializing AEHooks");
			DIHooks.onWorldLoadedEvent += onWorldLoaded;
			DIHooks.onConstructedEvent += onConstructionComplete;
			DIHooks.onItemPickedUpEvent += onPickup;
			DIHooks.onDamageEvent += onTakeDamage;
			DIHooks.knifeHarvestEvent += interceptItemHarvest;
			DIHooks.inventoryClosedEvent += onInvClosed;
			DIHooks.onBaseLoadedEvent += onBaseLoaded;
			DIHooks.constructabilityEvent += enforceBuildability;
			DIHooks.gravTrapAttemptEvent += gravTryAttract;
			DIHooks.onSkyApplierSpawnEvent += onSkyApplierSpawn;
			DIHooks.onPlayerTickEvent += tickPlayer;
			DIHooks.craftTimeEvent += affectCraftTime;
			DIHooks.onSleepEvent += onSleep;
			DIHooks.baseRebuildEvent += onBaseRebuild;
			DIHooks.baseStrengthComputeEvent += onBaseHullCompute;

			//DIHooks.onRedundantScanEvent += ch => ch.preventNormalDrop = onRedundantScan();
			CustomMachineLogic.getMachinePowerCostFactorEvent += getCustomMachinePowerCostMultiplier;
		}

		public static void tickPlayer(Player ep) {
			if (ep.currentSub && ep.currentSub.isBase && ep.currentSub is BaseRoot) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastPlayerRoomCheckTime >= 0.5F) {
					currentPlayerRoom = ObjectUtil.getBaseRoom((BaseRoot)ep.currentSub, ep.transform.position);
					lastPlayerRoomCheckTime = time;
				}
			}
		}

		public static BaseCell getCurrentPlayerRoom() {
			return currentPlayerRoom;
		}

		public static void onWorldLoaded() {
			OutdoorPot.updateLocale();

			string s = AqueousEngineeringMod.machineLocale.getEntry("BaseRepairBeacon").getField<string>("frag");
			foreach (TechnologyFragment f in AqueousEngineeringMod.repairBeaconFragments)
				LanguageHandler.Main.SetLanguageLine(f.fragmentPrefab.TechType.AsString(), s);
		}

		public static void onSkyApplierSpawn(SkyApplier sk) {
			PrefabIdentifier pi = sk.GetComponent<PrefabIdentifier>();
			MoonpoolRotationSystem.instance.processObject(sk.gameObject);
			if (pi && pi.name.StartsWith("Seamoth", StringComparison.InvariantCultureIgnoreCase) && pi.name.EndsWith("Arm(Clone)", StringComparison.InvariantCultureIgnoreCase))
				return;
			if (sk.GetComponent<StarshipDoor>() && Vector3.Distance(mountainWreckLaserable, sk.transform.position) <= 0.5)
				new WreckDoorSwaps.DoorSwap(sk.transform.position, "Laser").applyTo(sk.gameObject);
			else if (pi && pi.ClassId == "055b3160-f57b-46ba-80f5-b708d0c8180e" && Vector3.Distance(mountainWreckBlock, sk.transform.position) <= 0.5)
				new WreckDoorSwaps.DoorSwap(sk.transform.position, "Blocked").applyTo(sk.gameObject);
		}

		public static void onNuclearReactorSpawn(BaseNuclearReactor reactor) {
			reactor.gameObject.EnsureComponent<NuclearReactorFuelSystem.ReactorManager>();
		}

		public static void tickACU(WaterPark acu) {
			ACUCallbackSystem.instance.tick(acu);
		}

		public static void tryBreedACU(WaterPark acu, WaterParkCreature creature) {
			if (!acu.items.Contains(creature))
				return;
			TechType tt = creature.pickupable.GetTechType();
			ACUCallbackSystem.ACUCallback call = acu.gameObject.GetComponent<ACUCallbackSystem.ACUCallback>();
			BaseBioReactor bio = call ? call.isAboveBioreactor : null;
			bool full = !acu.HasFreeSpace();
			if (full && !(bio && bio.IsAllowedToAdd(creature.pickupable, false) && bio.container.HasRoomCached(CraftData.GetItemSize(tt))))
				return;
			WaterParkCreature mate = acu.items.Find(item => item && item != creature && item is WaterParkCreature && item.pickupable && ((WaterParkCreature)item).GetCanBreed() && item.pickupable.GetTechType() == tt) as WaterParkCreature;
			if (!mate)
				return;
			bool flag = true;
			if (full) {
				GameObject go = ObjectUtil.createWorldObject(tt);
				go.SetActive(false);
				flag = bio.container.AddItem(go.GetComponent<Pickupable>()) != null;
			}
			else {
				WaterParkCreature.Born(WaterParkCreature.creatureEggs.GetOrDefault(tt, tt), acu, creature.transform.position + Vector3.down);
			}
			if (flag)
				mate.ResetBreedTime();
		}

		public static bool canAddItemToACU(Pickupable item) {
			if (!item)
				return false;
			TechType tt = item.GetTechType();
			if (ACUCallbackSystem.isStalkerToy(tt))
				return true;
			GameObject go = item.gameObject;
			if (go.GetComponent<Creature>() == null && go.GetComponent<CreatureEgg>() == null)
				return false;
			LiveMixin lv = go.GetComponent<LiveMixin>();
			return !lv || lv.IsAlive();
		}

		public static void onChunkGenGrass(IVoxelandChunk2 chunk) {
			foreach (Renderer r in chunk.grassRenders) {
				ACUTheming.cacheGrassMaterial(r.materials[0]);
			}
		}

		public static float getCameraDistanceForRenderFX(MapRoomCamera cam, MapRoomScreen scr) {
			SubRoot sub = cam.dockingPoint ? cam.dockingPoint.gameObject.GetComponentInParent<SubRoot>() : null;
			if (!sub) {
				sub = WorldUtil.getClosest<SubRoot>(cam.gameObject);
			}
			if (sub) {
				float dist = Vector3.Distance(sub.transform.position, cam.transform.position);
				if (dist <= 400) {
					RemoteCameraAntennaLogic lgc = sub.GetComponentInChildren<RemoteCameraAntennaLogic>();
					if (lgc && lgc.isReady()) {
						return dist <= 350 ? 0 : (float)MathUtil.linterpolate(dist, 350, 400, 0, 400, true);
					}
				}
			}
			return cam.GetScreenDistance(scr);
		}

		private static bool isBuildingACUBuiltBlock() {
			return (AqueousEngineeringMod.acuBoosterBlock != null && Builder.constructableTechType == AqueousEngineeringMod.acuBoosterBlock.TechType) || (AqueousEngineeringMod.acuCleanerBlock != null && Builder.constructableTechType == AqueousEngineeringMod.acuCleanerBlock.TechType) || (AqueousEngineeringMod.acuMonitorBlock != null && Builder.constructableTechType == AqueousEngineeringMod.acuMonitorBlock.TechType);
		}
		/*
		private static bool isOnACU(Collider c) {
			if (!c)
				return false;
			BaseExplicitFace face = c.gameObject.FindAncestor<BaseExplicitFace>();
			if (!face)
				return false;
			SNUtil.writeToChat(face+" > "+face.gameObject.GetFullHierarchyPath()+" > "+face.gameObject.name.Contains("WaterPark"));
			return face && face.gameObject.name.Contains("WaterPark");
		}
	    */
		public static void enforceBuildability(DIHooks.BuildabilityCheck check) {
			if (isBuildingACUBuiltBlock()) {
				check.placeable = check.placeOn && check.placeOn.gameObject.FindAncestor<WaterParkPiece>();//isOnACU(check.placeOn && chec);
				check.ignoreSpaceRequirements = true;
			}
			else if (Builder.constructableTechType == AqueousEngineeringMod.ampeelAntennaBlock.TechType && check.placeOn && Player.main.currentWaterPark && check.placeOn.gameObject.FindAncestor<WaterParkPiece>().GetWaterParkModule() == Player.main.currentWaterPark) {
				check.placeable = true;
				check.ignoreSpaceRequirements = true;
			}
			else if (Builder.constructableTechType == AqueousEngineeringMod.batteryBlock.TechType) {
				//SNUtil.writeToChat(check.placeOn ? check.placeOn.gameObject.GetFullHierarchyPath() : "null");
				check.placeable &= check.placeOn && (check.placeOn.gameObject.isRoom(false) || check.placeOn.gameObject.isMoonpool(false, false));
				check.ignoreSpaceRequirements = false;
			}
			else if (Builder.constructableTechType == AqueousEngineeringMod.pillarBlock.TechType) {
				//SNUtil.writeToChat(check.placeOn ? check.placeOn.gameObject.GetFullHierarchyPath() : "null");
				check.placeable &= check.placeOn && check.placeOn.gameObject.isRoom(false);
				check.ignoreSpaceRequirements = true;
			}
			else if (Builder.constructableTechType == AqueousEngineeringMod.powerRelayBlock.TechType) {
				check.placeable = !check.placeOn;
				check.ignoreSpaceRequirements = true;
			}
			else if (Builder.constructableTechType == AqueousEngineeringMod.atpTapBlock.TechType) {
				check.placeable = check.placeOn && ATPTapLogic.isValidSourceObject(check.placeOn.gameObject) && WorldUtil.getObjectsNearMatching(check.placeOn.transform.position, 100, go => go.GetComponent<ATPTapLogic>() && go.GetComponent<Constructable>().constructed).Count == 0;
				check.ignoreSpaceRequirements = true;
			}
		}
		/*
	   public static bool onRedundantScan() {
	   	PDAScanner.ScanTarget tgt = PDAScanner.scanTarget;
	   	if (tgt.gameObject) {
	   		PrefabIdentifier pi = tgt.gameObject.GetComponent<PrefabIdentifier>();
	   		if (pi && AqueousEngineeringMod.repa
	   	}
	   }*/

		public static void interceptItemHarvest(DIHooks.KnifeHarvest h) {
			if (h.hit && h.drops.Count > 0) {
				Planter p = h.hit.FindAncestor<Planter>();
				if (p && BaseRoomSpecializationSystem.instance.getSavedType(p) == BaseRoomSpecializationSystem.RoomTypes.AGRICULTURAL) {
					if (UnityEngine.Random.Range(0F, 1F) < 0.33F)
						h.drops[h.defaultDrop] = h.drops[h.defaultDrop] + 1;
				}
			}
		}

		public static void onPickup(DIHooks.ItemPickup ip) {
			Pickupable pp = ip.item;
			if (BaseRoomSpecializationSystem.instance.getPlayerRoomType(Player.main) == BaseRoomSpecializationSystem.RoomTypes.AGRICULTURAL) {
				Eatable ea = pp.GetComponent<Eatable>();
				if (ea) {
					//SNUtil.writeToChat(pp+" is edible, +25% to values since agri room");
					ea.waterValue *= 1.25F;
					ea.foodValue *= 1.25F;
				}
			}
		}

		public static float getReactorGeneration(float orig, MonoBehaviour reactor) { //either bio or nuclear
																					  //SNUtil.writeToChat("Reactor gen "+orig+" in "+BaseRoomSpecializationSystem.instance.getSavedType(reactor));
			return BaseRoomSpecializationSystem.instance.getSavedType(reactor) == BaseRoomSpecializationSystem.RoomTypes.POWER ? orig * 1.25F : orig;
		}

		public static void onSleep(Bed bed) {
			//SNUtil.writeToChat("Slept in "+BaseRoomSpecializationSystem.instance.getSavedType(bed));
			if (BaseRoomSpecializationSystem.instance.getSavedType(bed, out float deco, out float thresh) == BaseRoomSpecializationSystem.RoomTypes.LEISURE)
				Player.main.gameObject.AddComponent<HealingOverTime>().setValues(Mathf.Min(20, 15 + deco - thresh), bed.kSleepRealTimeDuration).activate();
		}

		public static void affectFoodRate(DIHooks.FoodRateCalculation calc) {
			BaseRoomSpecializationSystem.RoomTypes type = BaseRoomSpecializationSystem.instance.getPlayerRoomType(Player.main, out float deco, out float thresh);
			//SNUtil.writeToChat("Current player room type: "+type);
			if (type == BaseRoomSpecializationSystem.RoomTypes.LEISURE)
				calc.rate *= Mathf.Max(0.2F, 0.33F - (0.02F * (deco - thresh)));
			else if (type == BaseRoomSpecializationSystem.RoomTypes.WORK)
				calc.rate *= 0.8F - (0.01F * Mathf.Min(5, deco));
		}

		private static void affectCraftTime(DIHooks.CraftTimeCalculation calc) {
			if (BaseRoomSpecializationSystem.instance.getSavedType(calc.crafter) == BaseRoomSpecializationSystem.RoomTypes.WORK)
				calc.craftingDuration /= 1.5F;
		}

		public static void onConstructionComplete(Constructable c, bool complete) {
			if (DIHooks.getWorldAge() < 1F)
				return;
			if (Player.main.currentSub && Player.main.currentSub.isBase) {
				BaseRoomSpecializationSystem.instance.updateRoom(c.gameObject);
			}
		}

		public static void onInvClosed(StorageContainer sc) {
			if (Player.main.currentSub && Player.main.currentSub.isBase && BaseRoomSpecializationSystem.instance.storageHasDecoValue(sc))
				BaseRoomSpecializationSystem.instance.updateRoom(sc.gameObject);
		}

		public static float getWaterFilterPowerCost(float cost, FiltrationMachine c) {
			//SNUtil.writeToChat("Waterfilter power cost "+cost+" in "+BaseRoomSpecializationSystem.instance.getSavedType(c));
			if (BaseRoomSpecializationSystem.instance.getSavedType(c) == BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
				cost *= 0.8F;
			return cost;
		}

		public static float getChargerSpeed(float speed, Charger c) {
			//SNUtil.writeToChat("Charger speed "+speed+" in "+BaseRoomSpecializationSystem.instance.getSavedType(c));
			if (BaseRoomSpecializationSystem.instance.getSavedType(c) == BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
				speed *= 1.5F;
			return speed;
		}

		public static void getCustomMachinePowerCostMultiplier(CustomMachinePowerCostFactorCheck ch) {
			if (BaseRoomSpecializationSystem.instance.getSavedType(ch.machine) == BaseRoomSpecializationSystem.RoomTypes.MECHANICAL)
				ch.value *= 0.8F;
		}

		public static void onBaseLoaded(BaseRoot root) {
			BaseRoomSpecializationSystem.instance.recomputeBaseRooms(root, 1F);
		}
		/*
	   public static void onPDAClosed() {
			XMLLocale.LocaleEntry e = AqueousEngineeringMod.acuMonitorBlock.locale;
			PDAManager.PDAPage pp = PDAManager.getPage(e.key+"PDA");
			pp.relock();
	   }*/

		public static void gravTryAttract(DIHooks.GravTrapGrabAttempt h) {
			if (h.gravtrap.GetComponent<ItemCollector.ItemCollectorLogic>()) {
				h.allowGrab &= ItemCollector.ItemCollectorLogic.canGrab(h.target);
			}
		}

		public static void onTakeDamage(DIHooks.DamageToDeal dmg) {
			if (dmg.type == DamageType.Heat || dmg.type == DamageType.Fire) {
				PrefabIdentifier pi = dmg.target.FindAncestor<PrefabIdentifier>();
				if (pi && pi.ClassId == AqueousEngineeringMod.collector.ClassID)
					dmg.setValue(0);
			}
		}

		public static void onEquipmentSlotActivated(uGUI_EquipmentSlot slot, bool active) {
			if (active && !slot.active && slot.slot.StartsWith("NuclearReactor", StringComparison.InvariantCultureIgnoreCase)) {
				slot.gameObject.EnsureComponent<NuclearReactorFuelSystem.ReactorFuelDisplay>();
			}
		}

		public static void onPlacedItem(PlaceTool pt) {
			if (Player.main.currentSub && Player.main.currentSub.isBase)
				BaseRoomSpecializationSystem.instance.updateRoom(pt.gameObject);
		}

		public static void onBaseRebuild(Base b) {
			MoonpoolRotationSystem.instance.rebuildBase(b);
		}

		public static void onBaseHullCompute(DIHooks.BaseStrengthCalculation calc) {
			BasePillarLogic[] arr = calc.component.baseComp.GetComponentsInChildren<BasePillarLogic>();
			Dictionary<BaseCell, RoomPillarTracker> pillarsByRoom = new Dictionary<BaseCell, RoomPillarTracker>();
			BaseRoot bb = calc.component.baseComp.GetComponent<BaseRoot>();
			for (int i = 0; i < arr.Length; i++) {
				if (!arr[i] || !arr[i].buildable || !arr[i].buildable.constructed)
					continue;
				BaseCell bc = ObjectUtil.getBaseRoom(bb, arr[i].gameObject);
				if (!bc)
					continue;
				RoomPillarTracker tr = pillarsByRoom.ContainsKey(bc) ? pillarsByRoom[bc] : null;
				if (tr == null) {
					tr = new RoomPillarTracker(bc);
					pillarsByRoom[bc] = tr;
				}
				tr.pillars.Add(arr[i]);
			}
			foreach (RoomPillarTracker tr in pillarsByRoom.Values) {
				float eff = 1;
				int n = 0;
				foreach (BasePillarLogic lgc in tr.pillars) {
					n++;
					calc.addBonusStrength(lgc.gameObject, eff * AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.PILLARHULL));
					if (n >= AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.PILLARLIM))
						eff *= 0.5F;
				}
			}
		}

		class RoomPillarTracker {

			internal readonly BaseCell room;
			internal readonly List<BasePillarLogic> pillars = new List<BasePillarLogic>();

			internal RoomPillarTracker(BaseCell bc) {
				room = bc;
			}

		}
	}
}
