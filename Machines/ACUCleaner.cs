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
	
	public class ACUCleaner : CustomMachine<ACUCleanerLogic> {
		
		internal static readonly float POWER_COST = 0.15F;
		
		public ACUCleaner(XMLLocale.LocaleEntry e) : base("baseacucleaner", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.Titanium, 5);
			addIngredient(TechType.ExosuitPropulsionArmModule, 1);
			addIngredient(TechType.MapRoomCamera, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return false;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<PowerRelay>(go);
						
			ACUCleanerLogic lgc = go.GetComponent<ACUCleanerLogic>();
			
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
		
	public class ACUCleanerLogic : CustomMachineLogic {
		
		private WaterPark connectedACU;
		private StorageContainer closestLocker;
		
		private float lastRunTime;
				
		void Start() {
			SNUtil.log("Reinitializing acu cleaner");
			AqueousEngineeringMod.acuCleanerBlock.initializeMachine(gameObject);
		}
		
		private WaterPark tryFindACU() {
			SubRoot sub = getSub();
			if (!sub) {
				return null;
			}
			foreach (WaterPark wp in sub.GetComponentsInChildren<WaterPark>()) {
				if (Vector3.Distance(wp.transform.position, transform.position) <= 6) {
					return wp;
				}
			}
			return null;
		}
		
		private StorageContainer tryFindStorage() {
			SubRoot sub = getSub();
			if (!sub) {
				return null;
			}
			foreach (StorageContainer wp in sub.GetComponentsInChildren<StorageContainer>()) {
				if (Vector3.Distance(wp.transform.position, transform.position) <= 3 && !wp.GetComponent<WaterPark>() && wp.container.GetContainerType() == ItemsContainerType.Default) {
					return wp;
				}
			}
			return null;
		}
		
		protected override void updateEntity(float seconds) {
			if (!connectedACU) {
				connectedACU = tryFindACU();
				closestLocker = tryFindStorage();
			}
			if (connectedACU && closestLocker && consumePower(ACUCleaner.POWER_COST, seconds)) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastRunTime >= 2) {
					lastRunTime = time;
					foreach (WaterParkItem wp in connectedACU.items) {
						if (wp) {
							Pickupable pp = wp.GetComponent<Pickupable>();
							TechType tt = pp.GetTechType();
							if (tt == TechType.SeaTreaderPoop || tt == AqueousEngineeringMod.poo.TechType) {
								InventoryItem ii = closestLocker.container.AddItem(pp);
								if (ii != null) {
									connectedACU.RemoveItem(pp);
									pp.gameObject.SetActive(false);
								}
							}
						}
					}
				}
			}
		}	
	}
}
