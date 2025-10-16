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

	public class AmpeelAntenna : CustomMachine<AmpeelAntennaLogic> {

		public static readonly float POWER_GEN = 3F; //max, per s per ampeel
		public static readonly float POWER_FALLOFF = 0.12F; //per meter
		public static float ACU_COEFFICIENT = 0.4F;
		public static readonly float RANGE = POWER_GEN/POWER_FALLOFF;
		public static readonly float INTERVAL = 0.25F;
		public static readonly float AMPEEL_CAP = 25;

		public AmpeelAntenna(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
			this.addIngredient(TechType.CopperWire, 6);
			this.addIngredient(TechType.WiringKit, 2);
			this.addIngredient(TechType.Gold, 1);
			this.addIngredient(TechType.Titanium, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		public override bool isOutdoors() {
			return true;
		}

		protected override bool isPowerGenerator() {
			return true;
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<Bench>();
			go.removeChildObject("Bubbles");

			go.transform.localScale = new Vector3(0.4F, 0.2F, 1);

			go.GetComponent<Constructable>().model = go.getChildObject("bench");

			string name = "CoilHolder";
			GameObject child = go.getChildObject(name);
			if (child == null) {
				child = new GameObject(name);
				child.transform.SetParent(go.transform);
			}
			PrefabIdentifier[] pi = child.GetComponentsInChildren<PrefabIdentifier>();
			int n = 15;
			for (int i = pi.Length; i < n; i++) {
				GameObject fin = ObjectUtil.createWorldObject("cf522a95-3038-4759-a53c-8dad1242c8ed");
				fin.convertToModel();
				fin.EnsureComponent<AmpeelCoil>();
				fin.transform.SetParent(child.transform);
				fin.transform.localScale = new Vector3(0.09F, 0.13F, 0.05F);
				fin.transform.localRotation = Quaternion.Euler(0, 0, 0);
				fin.transform.localPosition = new Vector3(-0.015F, 0, -0.75F + (1.5F * i / n));//Vector3.zero+i*go.transform.right*0.25F;
				RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, fin.GetComponentInChildren<Renderer>(), "Textures/Machines/AmpeelCoil");
			}

			AmpeelAntennaLogic lgc = go.GetComponent<AmpeelAntennaLogic>();

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

		public static float computeACUValue(WaterPark acu) {
			float ampeels = 0;
			float exponent = 1;
			foreach (WaterParkItem wp in acu.items) {
				if (wp is WaterParkCreature wpc) {
					Shocker s = wpc.GetComponent<Shocker>();
					AmpeelAntennaCreature aac = wpc.GetComponent<AmpeelAntennaCreature>();
					if (s && s.liveMixin.IsAlive()) {
						ampeels += s.liveMixin.GetHealthFraction();
					}
					else if (aac != null && aac.live && aac.live.IsAlive()) {
						float f = aac.live.GetHealthFraction();
						ampeels += aac.ampeelValue * f;
						exponent += aac.powerExponentAddition * f;
					}
				}
			}
			return Mathf.Min(AMPEEL_CAP, Mathf.Pow(ampeels, exponent)) * ACU_COEFFICIENT * POWER_GEN;
		}

	}

	class AmpeelCoil : MonoBehaviour {

	}

	public interface AmpeelAntennaCreature {

		LiveMixin live { get; }

		float ampeelValue { get; }

		float powerExponentAddition { get; }

	}

	public class AmpeelAntennaLogic : CustomMachineLogic {

		private WaterPark connectedACU;
		private WaterParkPiece connectedACUPart;

		private float lastACUCheckTime = -1;

		void Start() {
			SNUtil.log("Reinitializing base ampeel antenna");
			AqueousEngineeringMod.ampeelAntennaBlock.initializeMachine(gameObject);

			connectedACU = this.tryFindACU();
		}

		private WaterPark tryFindACU() {
			if (!sub)
				return null;
			foreach (WaterPark wp in sub.GetComponentsInChildren<WaterPark>()) {
				if (Mathf.Abs(wp.transform.position.y - transform.position.y) <= 0.5F && Vector3.Distance(wp.transform.position, transform.position) <= 1) {
					return wp;
				}
			}
			return null;
		}

		protected override float getTickRate() {
			return AmpeelAntenna.INTERVAL;
		}

		public override float getBaseEnergyStorageCapacityBonus() {
			return 50;
		}

		protected override bool isOutdoors() {
			return !connectedACU;
		}

		protected override void updateEntity(float seconds) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (sub && connectedACU && !connectedACUPart) {
				IEnumerable<WaterParkPiece> li = sub.GetComponentsInChildren<WaterParkPiece>().Where(wp => wp.GetWaterParkModule() == connectedACU);
				connectedACUPart = ACUCallbackSystem.instance.getACUCeiling(li).transform.parent.GetComponent<WaterParkPiece>();
				transform.position = connectedACUPart.transform.position + (Vector3.up * 1.95F);
				transform.rotation = Quaternion.Euler(180, 0, 0);
			}
			if (!connectedACU && time - lastACUCheckTime >= 0.5) {
				connectedACUPart = null;
				lastACUCheckTime = time;
				bool hasACU = connectedACU;
				connectedACU = this.tryFindACU();
				if ((bool)connectedACU != hasACU)
					this.setupSky();
			}
			if (sub && sub.powerRelay.GetPower() < sub.powerRelay.GetMaxPower()) {
				float toAdd = 0;
				if (connectedACU) {
					toAdd = AmpeelAntenna.computeACUValue(connectedACU);
				}
				else {
					HashSet<Shocker> set = WorldUtil.getObjectsNearWithComponent<Shocker>(gameObject.transform.position, AmpeelAntenna.RANGE);
					foreach (Shocker c in set) {
						if (c && c.liveMixin.IsAlive()) {
							float dd = Vector3.Distance(c.transform.position, transform.position);
							if (dd >= AmpeelAntenna.RANGE)
								continue;
							toAdd += c.liveMixin.GetHealthFraction() * (AmpeelAntenna.POWER_GEN - dd*AmpeelAntenna.POWER_FALLOFF);
						}
					}
				}
				if (toAdd > 0) {
					sub.powerRelay.AddEnergy(seconds * toAdd, out float trash);
				}
			}
		}
	}
}
