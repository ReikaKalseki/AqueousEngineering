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
	
	public class BaseBattery : CustomMachine<BaseBatteryLogic> {
		
		internal static readonly float CAPACITY = 300F;
		
		public BaseBattery(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "??") {
			addIngredient(TechType.PowerCell, 2);
			addIngredient(TechType.WiringKit, 1);
			addIngredient(TechType.Titanium, 4);
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
						
			BaseBatteryLogic lgc = go.GetComponent<BaseBatteryLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class BaseBatteryLogic : CustomMachineLogic { //TODO set color based on storage and flow direction, like in space engineers
		
		void Start() {
			SNUtil.log("Reinitializing base battery");
			AqueousEngineeringMod.batteryBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 1;
		}
		
		protected override float getBaseEnergyStorageCapacityBonus() {
			return BaseBattery.CAPACITY;
		}
		
		protected override void updateEntity(float seconds) {
			SubRoot sub = getSub();
			
		}	
	}
}
