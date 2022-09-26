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
	
	public class AmpeelAntenna : CustomMachine<AmpeelAntennaLogic> {
		
		internal static readonly float POWER_COST = 10F; //per ping
		internal static readonly float FIRE_RATE = 4F; //interval in seconds
		internal static readonly float MAX_RANGE = 300F; //m
		
		public AmpeelAntenna(XMLLocale.LocaleEntry e) : base("baseampeelantenna", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.CopperWire, 6);
			addIngredient(TechType.WiringKit, 2);
			addIngredient(TechType.PowerTransmitter, 1);
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
						
			AmpeelAntennaLogic lgc = go.GetComponent<AmpeelAntennaLogic>();
			
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
		
	public class AmpeelAntennaLogic : CustomMachineLogic {
		
		void Start() {
			SNUtil.log("Reinitializing base ampeel antenna");
			AqueousEngineeringMod.ampeelAntennaBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			
		}	
	}
}
