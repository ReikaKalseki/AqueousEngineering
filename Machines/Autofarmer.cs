using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class Autofarmer : CustomMachine<AutofarmerLogic> {
		
		internal static readonly float POWER_COST = 1F;
		
		public Autofarmer(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "f1cde32e-101a-4dd5-8084-8c950b9c2432") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(TechType.VehicleStorageModule, 1);
			addIngredient(TechType.Knife, 1);
			
			glowIntensity = 2;
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Trashcan>(go);
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 8, 8);
			con.errorSound = null;
			
			ObjectUtil.removeChildObject(go, "descent_trashcan_01/descent_trash_01");
			ObjectUtil.removeChildObject(go, "descent_trashcan_01/descent_trash_02");
			ObjectUtil.removeChildObject(go, "descent_trashcan_01/descent_trashcan_interior_01");
			ObjectUtil.removeChildObject(go, "descent_trashcan_01/descent_trashcan_interior_02");
						
			AutofarmerLogic lgc = go.GetComponent<AutofarmerLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetColor("_Color", Color.white);
			r.materials[0].SetFloat("_Fresnel", 0.8F);
			r.materials[0].SetFloat("_SpecInt", 12F);
			/*
			//SNUtil.dumpTextures(r);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class AutofarmerLogic : CustomMachineLogic {
		
		private List<Planter> growbeds = new List<Planter>();
		
		private VFXElectricLine effect;
		private float harvestTime;
		
		void Start() {
			SNUtil.log("Reinitializing base farmer");
			AqueousEngineeringMod.farmerBlock.initializeMachine(gameObject);
		}
		
		private void OnDisable() {
			UnityEngine.Object.DestroyImmediate(effect);
		}
		
		protected override float getTickRate() {
			return 5;
		}
		
		protected override void updateEntity(float seconds) {
			if (!effect) {
				GameObject go = ObjectUtil.createWorldObject("d11dfcc3-bce7-4870-a112-65a5dab5141b", true, false);
				go.SetActive(false);
				go = go.GetComponent<Gravsphere>().vfxPrefab;
				go = UnityEngine.Object.Instantiate(go);
				effect = go.GetComponent<VFXElectricLine>();
				effect.transform.parent = transform;
			}
			if (growbeds.Count == 0) {
				SubRoot sub = getSub();
				if (sub) {
					Planter[] all = sub.GetComponentsInChildren<Planter>();
					foreach (Planter p in all) {
						if (p && p.GetContainerType() == ItemsContainerType.WaterPlants && Vector3.Distance(p.transform.position, transform.position) <= 8) {
							growbeds.Add(p);
						}
					}
				}
			}
			if (growbeds.Count > 0 && consumePower(Autofarmer.POWER_COST*seconds)) {
				Planter p = growbeds[UnityEngine.Random.Range(0, growbeds.Count)];
				if (p) {
					tryHarvestFrom(p);
				}
			}
			tickFX();
		}
		
		private void tickFX() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-harvestTime > 5) {
				effect.gameObject.SetActive(false);
			}
		}
		
		private void tryAllocateFX(GameObject go) {
			effect.gameObject.SetActive(true);
			effect.enabled = true;
			effect.origin = transform.position+Vector3.up*0.75F;
			effect.target = go.transform.position+Vector3.up*0.125F;
			harvestTime = DayNightCycle.main.timePassedAsFloat;
		}
		
		private void tryHarvestFrom(Planter p) {
			Planter.PlantSlot[] arr = UnityEngine.Random.Range(0, 2) == 0 ? p.bigPlantSlots : p.smallPlantSlots;
			Planter.PlantSlot slot = arr[UnityEngine.Random.Range(0, arr.Length)];
			if (slot != null && slot.isOccupied) {
				Plantable pt = slot.plantable;
				if (pt && pt.linkedGrownPlant) {
					tryHarvestPlant(p, pt);
				}
			}
		}
		
		private void tryHarvestPlant(Planter pl, Plantable pt) {
			GrownPlant p = pt.linkedGrownPlant;
			TechType tt = CraftData.GetTechType(p.gameObject);
			SNUtil.log("Try harvest "+p+" : "+tt);
			if (tt != TechType.None) {
				FruitPlant fp = p.GetComponent<FruitPlant>();
				GameObject drop = getHarvest(p, tt, fp);
				SNUtil.log("drops "+drop);
				if (drop) {
					drop = UnityEngine.Object.Instantiate(drop);
					TechType td = CraftData.GetTechType(drop);
					if (fp) {
						PickPrefab pp = drop.GetComponent<PickPrefab>();
						td = pp.pickTech;
						drop = ObjectUtil.lookupPrefab(td);
					}
					else if (td == TechType.JellyPlantSeed || td == TechType.WhiteMushroomSpore || td == TechType.AcidMushroomSpore) {
						td = tt;
						drop = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(tt));
					}
					SNUtil.log("DT "+td+" > "+drop);
					drop.SetActive(false);
					Pickupable ppb = drop.GetComponent<Pickupable>();
					if (!ppb) {
						ppb = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(td)).GetComponent<Pickupable>();
					}
					SNUtil.log(""+ppb);
					if (ppb && getStorage().container.AddItem(ppb) != null) {
						FMODAsset ass = SoundManager.buildSound(CraftData.pickupSoundList.ContainsKey(td) ? CraftData.pickupSoundList[td] : CraftData.defaultPickupSound);
						if (ass != null) {
							SoundManager.playSoundAt(ass, gameObject.transform.position);
						}
						if (fp) {
							PickPrefab pp = drop.GetComponent<PickPrefab>();
							SNUtil.log("fp pp "+pp);
							if (pp)
								pp.SetPickedUp();
						}
						else if (td == TechType.JellyPlant || td == TechType.WhiteMushroom || td == TechType.AcidMushroom) {
							//pl.ReplaceItem(pt, drop.GetComponent<Plantable>());
						}
						SNUtil.log("fx "+p);
						if (p)
							tryAllocateFX(p.gameObject);
						else
							tryAllocateFX(fp.fruits[0].gameObject);
					}
				}
			}
		}
		
		private GameObject getHarvest(GrownPlant p, TechType tt, FruitPlant fp) {
			if (fp) {
				PickPrefab pp = fp.fruits[UnityEngine.Random.Range(0, fp.fruits.Length)];
				if (pp && pp.isActiveAndEnabled) {
					return pp.gameObject;
				}
				else {
					return null;
				}
			}
			switch (tt) {/*
				case TechType.BloodVine:
					return TechType.BloodOil;
				case TechType.Creepvine:
					return  ? TechType.CreepvineSeedCluster : TechType.CreepvinePiece;*/
				default:
					return CraftData.harvestOutputList.ContainsKey(tt) ? ObjectUtil.lookupPrefab(CraftData.harvestOutputList[tt]) : null;
			}
		}
	}
	/*
	class HarvestData {
		
		private readonly TechType plant;
		private readonly TechType drop;
		
		private GameObject createDroppedItem(GrownPlant p, TechType tt) {
			
		}
		
	}*/
}
