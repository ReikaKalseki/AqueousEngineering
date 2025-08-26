using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {

	public class BaseStasisTurret : CustomMachine<BaseStasisTurretLogic> {

		internal static readonly float POWER_COST = 250F; //per shot
		internal static readonly float POWER_LEVEL = 0.75F;
		internal static readonly float RADIUS = 18F;
		internal static readonly float COOLDOWN = 20F;

		public BaseStasisTurret(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			//addIngredient(TechType.StasisRifle, 1);
			this.addIngredient(TechType.AdvancedWiringKit, 1);
			this.addIngredient(TechType.Polyaniline, 2);
			this.addIngredient(TechType.TitaniumIngot, 1);
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
			go.removeComponent<PowerRelay>();
			go.removeComponent<PowerFX>();
			go.removeComponent<PowerSystemPreview>();

			BaseStasisTurretLogic lgc = go.GetComponent<BaseStasisTurretLogic>();

			Renderer r = go.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/

			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}

	}

	public class BaseStasisTurretLogic : CustomMachineLogic {

		private float lastFire;

		private float lastButtonCheck = -1;

		private GameObject sparker;

		private ParticleSystem[] particles;

		private Renderer mainRenderer;

		void Start() {
			SNUtil.log("Reinitializing base stasis turret");
			AqueousEngineeringMod.stasisBlock.initializeMachine(gameObject);
		}

		private void addButton() {
			if (!sub) {
				SNUtil.log("Could not add button for stasis turret, no sub");
				return;
			}
			foreach (BaseControlPanelLogic panel in sub.GetComponentsInChildren<BaseControlPanelLogic>()) {
				panel.addButton(AqueousEngineeringMod.seabaseStasisControl);
			}
		}

		public void fire() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastFire > BaseStasisTurret.COOLDOWN && this.consumePower(BaseStasisTurret.POWER_COST)) {
				lastFire = time;
				WorldUtil.createStasisSphere(transform.position, BaseStasisTurret.RADIUS, BaseStasisTurret.POWER_LEVEL);
			}
		}

		protected override void updateEntity(float seconds) {
			if (mainRenderer == null)
				mainRenderer = this.GetComponentInChildren<Renderer>();
			if (!sparker) {
				sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
				sparker.transform.localScale = new Vector3(0.4F, 0.4F, 0.4F);
				sparker.transform.parent = transform;
				//sparker.transform.eulerAngles = new Vector3(325, 180, 0);
				sparker.removeComponent<DamagePlayerInRadius>();
				sparker.removeComponent<PlayerDistanceTracker>();
				//sparker.removeChildObject("ElecLight");
				sparker.transform.localPosition = new Vector3(0, 0.95F, 0);
				foreach (ParticleSystem p in particles) {
					ParticleSystem.MainModule pm = p.main;
					pm.startSize = 0.4F;
				}
			}
			if (particles == null) {
				particles = sparker.GetComponentsInChildren<ParticleSystem>();
			}
			bool active = !GameModeUtils.RequiresPower() || (sub && sub.powerRelay.GetPower() > 0.1F);
			sparker.SetActive(active);
			if (mainRenderer)
				RenderUtil.setEmissivity(mainRenderer, active ? 200 : 0);
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time - lastButtonCheck >= 1) {
				lastButtonCheck = time;
				this.addButton();

				sparker.transform.localPosition = new Vector3(0, 0.95F, 0);
				foreach (ParticleSystem p in particles) {
					ParticleSystem.MainModule pm = p.main;
					pm.startSize = 0.4F;
				}
			}
		}
	}
}
