using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class ItemCollectorCyclopsTether : CustomMachine<ItemCollectorCyclopsTetherLogic> {
		
		public ItemCollectorCyclopsTether(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.Titanium, 2);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.Magnetite, 4);
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
			ObjectUtil.removeComponent<PowerSystemPreview>(go);
						
			ItemCollectorCyclopsTetherLogic lgc = go.GetComponent<ItemCollectorCyclopsTetherLogic>();
			
			Constructable c = go.GetComponent<Constructable>();
			c.allowedOnWall = false;
			c.allowedOutside = false;
			c.allowedInBase = false;
			c.allowedOnGround = true;
			c.allowedOnConstructables = false;
			c.allowedOnCeiling = false;
			c.allowedInSub = true;
			c.forceUpright = true;
			
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
		
	public class ItemCollectorCyclopsTetherLogic : CustomMachineLogic {
		
		internal PowerFX lineRenderer;
		
		public GameObject itemCollector;
		
		void Start() {
			SNUtil.log("Reinitializing cyclops item collector tether");
			AqueousEngineeringMod.collectorTetherBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			if (!lineRenderer) {
				lineRenderer = GetComponent<PowerFX>();
			}
			lineRenderer.SetTarget(itemCollector ? itemCollector.gameObject : null);
		}
	}
}
