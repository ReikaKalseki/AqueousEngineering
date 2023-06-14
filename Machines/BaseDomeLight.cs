using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class BaseDomeLight : CustomMachine<BaseDomeLightLogic> {
		
		public BaseDomeLight(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5c8cb04b-9f30-49e7-8687-0cbb338fc7fa") {
			addIngredient(TechType.Titanium, 1);
			addIngredient(TechType.Quartz, 1);
			addIngredient(TechType.Silver, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return true;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Planter>(go);
			ObjectUtil.removeComponent<StorageContainer>(go);
			UnityEngine.Object.DestroyImmediate(go.GetComponentInChildren<ChildObjectIdentifier>().gameObject);
			ObjectUtil.removeChildObject(go, "slots");
			ObjectUtil.removeChildObject(go, "grownPlant");
			GameObject mdl = ObjectUtil.getChildObject(go, "model");
			ObjectUtil.removeChildObject(mdl, "Wall");
			ObjectUtil.removeChildObject(mdl, "Soil");
			ObjectUtil.removeChildObject(mdl, "Tropical_Plant_10a");
			ObjectUtil.removeChildObject(mdl, "Base_interior_Planter_Pot_02/Base_interior_Planter_Tray_ground");
			ObjectUtil.removeChildObject(mdl, "Base_interior_Planter_Pot_02/pot_generic_plant_02");
			mdl.transform.localEulerAngles = new Vector3(180, 0, 0);
			mdl.transform.localPosition = new Vector3(0, 0.11F, 0);
			mdl.transform.localScale = new Vector3(0.33F, 0.25F, 0.33F);
			go.GetComponentInChildren<Collider>().transform.localScale = mdl.transform.localScale;
			Renderer r = mdl.GetComponentInChildren<Renderer>();
			r.materials[0].SetColor("_GlowColor", Color.white);
			RenderUtil.setEmissivity(r, 0);
			Light l = go.GetComponentInChildren<Light>();
			if (!l)
				l = ObjectUtil.addLight(go);
			l.range = 21;
			l.type = LightType.Point;
			l.color = Color.white;
			l.intensity = 0.8F;
			l.gameObject.transform.localPosition = new Vector3(0, 0.25F, 0);
		}
		
	}
		
	public class BaseDomeLightLogic : CustomMachineLogic {
		
		private Renderer render;
		private Light light;
		
		void Start() {
			SNUtil.log("Reinitializing base dome light");
			AqueousEngineeringMod.domeLightBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 1;
		}
		
		protected override void updateEntity(float seconds) {
			if (!render)
				render = GetComponentInChildren<Renderer>();
			if (!light)
				light = GetComponentInChildren<Light>();
			setState(consumePower(0.25F*seconds));
		}
		
		private void setState(bool on) {
			RenderUtil.setEmissivity(render, on ? 3 : 0);
			light.enabled = on;
		}
	}
}
