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
		
		public Autofarmer(XMLLocale.LocaleEntry e) : base("basefarmer", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
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
			ObjectUtil.removeComponent<PowerRelay>(go);
						
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
		
		void Start() {
			SNUtil.log("Reinitializing base farmer");
			AqueousEngineeringMod.farmerBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			
		}	
	}
}
