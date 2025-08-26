using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {

	public class BaseDomeLight : CustomMachine<BaseDomeLightLogic> {

		public BaseDomeLight(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5c8cb04b-9f30-49e7-8687-0cbb338fc7fa") {
			this.addIngredient(TechType.Titanium, 1);
			this.addIngredient(TechType.Quartz, 1);
			this.addIngredient(TechType.Silver, 1);
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
			go.removeComponent<Planter>();
			go.removeComponent<StorageContainer>();
			go.GetComponentInChildren<ChildObjectIdentifier>().gameObject.destroy();
			go.removeChildObject("slots");
			go.removeChildObject("grownPlant");
			GameObject mdl = go.getChildObject("model");
			mdl.removeChildObject("Wall");
			mdl.removeChildObject("Soil");
			mdl.removeChildObject("Tropical_Plant_10a");
			mdl.removeChildObject("Base_interior_Planter_Pot_02/Base_interior_Planter_Tray_ground");
			mdl.removeChildObject("Base_interior_Planter_Pot_02/pot_generic_plant_02");
			mdl.transform.localEulerAngles = new Vector3(180, 0, 0);
			mdl.transform.localPosition = new Vector3(0, 0.11F, 0);
			mdl.transform.localScale = new Vector3(0.33F, 0.25F, 0.33F);
			go.GetComponentInChildren<Collider>().transform.localScale = mdl.transform.localScale;
			Renderer r = mdl.GetComponentInChildren<Renderer>();
			r.materials[0].SetColor("_GlowColor", Color.white);
			RenderUtil.setEmissivity(r, 0);
			Light l = go.GetComponentInChildren<Light>();
			if (!l)
				l = go.addLight(0.8F, 21);
			l.type = LightType.Point;
			l.color = Color.white;
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
				render = this.GetComponentInChildren<Renderer>();
			if (!light)
				light = this.GetComponentInChildren<Light>();
			this.setState(this.consumePower(0.25F * seconds));
		}

		private void setState(bool on) {
			RenderUtil.setEmissivity(render, on ? 3 : 0);
			light.enabled = on;
		}
	}
}
