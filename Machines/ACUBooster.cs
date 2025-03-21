﻿using System;
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
	
	public class ACUBooster : CustomMachine<ACUBoosterLogic> {
		
		internal static readonly float POWER_COST = 0.0625F;
		internal static readonly float CONSUMPTION_RATE = 15*60; //s
		
		internal static WorldCollectedItem fuel;
		
		public ACUBooster(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5fc7744b-5a2c-4572-8e53-eebf990de434") {
			addIngredient(TechType.Titanium, 1);
			addIngredient(TechType.Pipe, 4);
			addIngredient(TechType.FiberMesh, 2);
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
			ObjectUtil.removeChildObject(go, "Label");
			
			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("cdade216-3d4d-4adf-901c-3a91fb3b88c4"), "model/submarine_centrifuge_base"));
			mdl.transform.localScale = Vector3.one*50;
			mdl.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 6, 5);
						
			ACUBoosterLogic lgc = go.GetComponent<ACUBoosterLogic>();
			
			//GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
			//GameObject mdl = RenderUtil.setModel(go, "discovery_trashcan_01_d", ObjectUtil.getChildObject(air, "model"));
			//lgc.rotator = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(ObjectUtil.getChildObject(air, "model"), "_pipes_floating_air_intake_turbine_geo"));
			//lgc.rotator.transform.parent = go.transform;
			
			Renderer[] r = go.GetComponentsInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			Constructable c = go.GetComponent<Constructable>();
			c.model = mdl;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
			
			c.allowedOnCeiling = false;
			c.allowedOnGround = false;
			c.allowedOnWall = true;
			c.allowedOnConstructables = true;
			c.allowedOutside = false;
		}
		
	}
		
	public class ACUBoosterLogic : CustomMachineLogic {
		
		private WaterPark connectedACU;
		
		private float lastFeedTime;
		
		//internal GameObject rotator;
				
		void Start() {
			SNUtil.log("Reinitializing acu booster");
			AqueousEngineeringMod.acuBoosterBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 2;
		}
		
		protected override void load(System.Xml.XmlElement data) {
			lastFeedTime = (float)data.getFloat("last", float.NaN);
		}
		
		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("last", lastFeedTime);
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
		
		protected override void updateEntity(float seconds) {
			if (!connectedACU) {
				connectedACU = tryFindACU();
			}
			if (connectedACU && consumePower(ACUBooster.POWER_COST*seconds) && getStorage().container.GetCount(ACUBooster.fuel.TechType) > 0) {
				//rotator.transform.position = connectedACU.transform.position+Vector3.down*1.45F;
				//rotator.transform.localScale = new Vector3(13.8F, 1, 13.8F);
				/*
				foreach (WaterParkItem wp in connectedACU.items) {
					if (wp && wp is WaterParkCreature) {
						
					}
				}*/
				ACUCallbackSystem.ACUCallback hook = connectedACU.GetComponent<ACUCallbackSystem.ACUCallback>();
				if (hook) {
					hook.boost();
					float time = DayNightCycle.main.timePassedAsFloat;
					if (time-lastFeedTime >= ACUBooster.CONSUMPTION_RATE) {
						lastFeedTime = time;
						getStorage().container.DestroyItem(ACUBooster.fuel.TechType);
					}
				}
			}
		}	
	}
}
