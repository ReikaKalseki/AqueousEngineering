using System;
using System.IO;
using System.Reflection;
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
		
		public Autofarmer(XMLLocale.LocaleEntry e) : base("basefarmer", e.name, e.desc, "f1cde32e-101a-4dd5-8084-8c950b9c2432") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(TechType.VehicleStorageModule, 1);
			addIngredient(TechType.Knife, 1);
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
						
			AutofarmerLogic lgc = go.GetComponent<AutofarmerLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class AutofarmerLogic : CustomMachineLogic {
		
		private List<Planter> growbeds = new List<Planter>();
		
		void Start() {
			SNUtil.log("Reinitializing base farmer");
			AqueousEngineeringMod.farmerBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 5;
		}
		
		protected override void updateEntity(float seconds) {
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
			if (growbeds.Count > 0 && consumePower(Autofarmer.POWER_COST, seconds)) {
				Planter p = growbeds[UnityEngine.Random.Range(0, growbeds.Count)];
				if (p) {
					tryHarvestFrom(p);
				}
			}
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
			//SNUtil.writeToChat("Try harvest "+p+" : "+tt);
			if (tt != TechType.None) {
				FruitPlant fp = p.GetComponent<FruitPlant>();
				GameObject drop = getHarvest(p, tt, fp);
				//SNUtil.writeToChat("drops "+drop);
				if (drop) {
					drop = UnityEngine.Object.Instantiate(drop);
					TechType td = CraftData.GetTechType(drop);
					if (fp) {
						PickPrefab pp = drop.GetComponent<PickPrefab>();
						td = pp.pickTech;
						drop = CraftData.GetPrefabForTechType(td);
					}
					else if (td == TechType.JellyPlantSeed || td == TechType.WhiteMushroomSpore || td == TechType.AcidMushroomSpore) {
						td = tt;
						drop = UnityEngine.Object.Instantiate(CraftData.GetPrefabForTechType(tt));
					}
					SNUtil.writeToChat("DT "+td);
					drop.SetActive(false);
					if (getStorage().container.AddItem(drop.GetComponent<Pickupable>()) != null) {
						FMODAsset ass = SNUtil.getSound(CraftData.pickupSoundList.ContainsKey(td) ? CraftData.pickupSoundList[td] : CraftData.defaultPickupSound);
						if (ass != null) {
							SNUtil.playSoundAt(ass, gameObject.transform.position);
						}
						if (fp) {
							PickPrefab pp = drop.GetComponent<PickPrefab>();
							pp.SetPickedUp();
						}
						else if (td == TechType.JellyPlant || td == TechType.WhiteMushroom || td == TechType.AcidMushroom) {
							pl.ReplaceItem(pt, drop.GetComponent<Plantable>());
						}
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
					return CraftData.harvestOutputList.ContainsKey(tt) ? CraftData.GetPrefabForTechType(CraftData.harvestOutputList[tt]) : null;
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
