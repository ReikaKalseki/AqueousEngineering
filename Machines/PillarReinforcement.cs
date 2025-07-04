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
	
	public class BasePillar : CustomMachine<BasePillarLogic> {
		
		public BasePillar(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
			addIngredient(TechType.Titanium, 2);
		}

		public override bool UnlockedAtStart {
			get {
				return true;
			}
		}
		
		public override bool isOutdoors() {
			return false;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Bench>(go);
						
			BasePillarLogic lgc = go.GetComponent<BasePillarLogic>();
			
			GameObject mdl = ObjectUtil.getChildObject(go, "bench");
			go.GetComponent<Constructable>().model = mdl;
			
			mdl.transform.localScale = new Vector3(0.8F, 1, 1.83F);
			mdl.transform.localEulerAngles = new Vector3(90, 0, 0);
			mdl.transform.localPosition = new Vector3(0, 1.45F, -0.15F);
			
			GameObject mdl2 = ObjectUtil.getChildObject(go, "bench2");
			if (!mdl2) {
				mdl2 = UnityEngine.Object.Instantiate(mdl);
				mdl2.transform.SetParent(mdl.transform.parent);
				mdl2.name = "bench2";
			}
			mdl2.transform.localScale = new Vector3(0.8F, 1, 1.83F);
			mdl2.transform.localEulerAngles = new Vector3(90, 180, 0);
			mdl2.transform.localPosition = new Vector3(0, 1.45F, 0.15F);
			
			RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, mdl.GetComponentsInChildren<Renderer>(), "Textures/Machines/BasePillar");
			RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, mdl2.GetComponentsInChildren<Renderer>(), "Textures/Machines/BasePillar");
			
			BoxCollider box = go.GetComponentInChildren<BoxCollider>();
			box.size = new Vector3(0.5F, 3F, 0.5F);
			box.transform.localPosition = new Vector3(0, 1.25F, 0);
		}
		
	}
		
	public class BasePillarLogic : CustomMachineLogic {
		
		void Start() {
			SNUtil.log("Reinitializing base pillar");
			AqueousEngineeringMod.pillarBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 999999999;
		}
		
		protected override void updateEntity(float seconds) {
			
		}
	}
}
