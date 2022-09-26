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
	
	public class BaseBeacon : CustomMachine<BaseBeaconLogic> {
		
		public BaseBeacon(XMLLocale.LocaleEntry e) : base("basebeacon", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.MapRoomUpgradeScanRange, 1);
			addIngredient(TechType.Beacon, 1);
			addIngredient(TechType.LEDLight, 1);
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
			ObjectUtil.removeComponent<PowerRelay>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
						
			BaseBeaconLogic lgc = go.GetComponent<BaseBeaconLogic>();
			
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
		
	public class BaseBeaconLogic : CustomMachineLogic {
		
		void Start() {
			SNUtil.log("Reinitializing base beacon");
			AqueousEngineeringMod.beaconBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			
		}	
	}
}
