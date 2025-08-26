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

	public class RepairBeacon : CustomMachine<RepairBeaconLogic> {

		internal static readonly float POWER_COST = 0.2F; //per second
		internal static readonly float POWER_COST_ACTIVE = 10.0F; //per second

		public RepairBeacon(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			this.addIngredient(TechType.AdvancedWiringKit, 1);
			this.addIngredient(TechType.TitaniumIngot, 1);
			this.addIngredient(TechType.CyclopsSeamothRepairModule, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		protected override bool shouldDeleteFragments() {
			return false;
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<PowerRelay>();
			go.removeComponent<PowerFX>();
			go.removeComponent<PowerSystemPreview>();

			RepairBeaconLogic lgc = go.GetComponent<RepairBeaconLogic>();

			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/

			Constructable c = go.GetComponent<Constructable>();
			c.allowedInBase = true;
			c.allowedInSub = true;
			c.allowedOnCeiling = true;
			c.allowedOnWall = true;
			c.allowedOnConstructables = false;
			c.allowedOutside = true;

			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}

	}

	public class RepairBeaconLogic : CustomMachineLogic {

		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "nanite", "Sounds/nanite.ogg", SoundManager.soundMode3D);

		private LiveMixin[] live = null;

		private float lastSound = -1;

		void Start() {
			SNUtil.log("Reinitializing base repair beacon");
			AqueousEngineeringMod.repairBlock.initializeMachine(gameObject);
		}

		protected override void updateEntity(float seconds) {
			if (sub && live == null)
				live = sub.GetComponentsInChildren<LiveMixin>().Where(lv => !lv.GetComponent<Vehicle>()).ToArray();
			if (sub && GameModeUtils.RequiresReinforcements() && this.canHeal(sub) && live != null && live.Length > 0 && this.consumePower(RepairBeacon.POWER_COST * seconds)) {
				LiveMixin lv = live.GetRandom<LiveMixin>();
				if (lv && lv.health < lv.maxHealth && this.consumePower((RepairBeacon.POWER_COST_ACTIVE - RepairBeacon.POWER_COST) * seconds)) {
					lv.AddHealth(seconds * 12);

					if (DayNightCycle.main.timePassedAsFloat - lastSound >= 1.25F) {
						lastSound = DayNightCycle.main.timePassedAsFloat;
						SoundManager.playSoundAt(workingSound, transform.position);
					}
				}
			}
		}

		private bool canHeal(SubRoot sub) {
			return !(sub is BaseRoot) || ((BaseRoot)sub).GetComponentInChildren<BaseHullStrength>().totalStrength > 0;
		}
	}
}
