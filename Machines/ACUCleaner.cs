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
		
		public ACUCleaner(XMLLocale.LocaleEntry e) : base("baseacucleaner", e.name, e.desc, "bedc40fb-bd97-4b4d-a943-d39360c9c7bd") {
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
			ObjectUtil.removeComponent<Trashcan>(go);
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 3, 5);
						
			ACUCleanerLogic lgc = go.GetComponent<ACUCleanerLogic>();
			
			//GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
			//GameObject mdl = RenderUtil.setModel(go, "discovery_trashcan_01_d", ObjectUtil.getChildObject(air, "model"));
			//lgc.rotator = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.getChildObject(air, "model"), "_pipes_floating_air_intake_turbine_geo"));
			//lgc.rotator.transform.parent = go.transform;
			
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
		
		//internal GameObject rotator;
				
		void Start() {
			SNUtil.log("Reinitializing acu cleaner");
			AqueousEngineeringMod.acuCleanerBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 2;
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
			}
			if (connectedACU && consumePower(ACUCleaner.POWER_COST, seconds)) {
				//rotator.transform.position = connectedACU.transform.position+Vector3.down*1.45F;
				//rotator.transform.localScale = new Vector3(13.8F, 1, 13.8F);
				foreach (WaterParkItem wp in connectedACU.items) {
					if (wp) {
						Pickupable pp = wp.GetComponent<Pickupable>();
						TechType tt = pp.GetTechType();
						if (tt == TechType.SeaTreaderPoop || tt == AqueousEngineeringMod.poo.TechType) {
							InventoryItem ii = getStorage().container.AddItem(pp);
							if (ii != null) {
								connectedACU.RemoveItem(pp);
								pp.gameObject.SetActive(false);
								break;
							}
						}
					}
				}
			}
		}	
	}
}
