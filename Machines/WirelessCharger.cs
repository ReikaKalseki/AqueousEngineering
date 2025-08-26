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

	public class WirelessCharger : CustomMachine<WirelessChargerLogic> {

		public static TechType unlockTrigger = TechType.BatteryCharger;

		//vanilla charger is 0.5%/s per tool, so 0.5/s for batteries and 2.5/s for ion, 3.75/s for azurite
		//but that is just how it computes the total and then splits it evenly, so one basic + 3 ions gives 2/s each; this machine will preserve the fractions
		public static readonly float MAX_CHARGE_FRAC_PER_SECOND = 0.0075F;

		public WirelessCharger(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			this.addIngredient(TechType.AdvancedWiringKit, 1);
			this.addIngredient(TechType.CopperWire, 6);
			this.addIngredient(TechType.SeamothElectricalDefense, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<PowerRelay>();
			go.removeComponent<PowerFX>();
			go.removeComponent<PowerSystemPreview>();

			GameObject mdl = RenderUtil.setModel(go, "Power_Transmitter", ObjectUtil.lookupPrefab("67744b32-93c2-4aba-8a18-ffb87204a8eb").getChildObject("model/LED_light"));
			mdl.removeChildObject("*");
			mdl.transform.localScale = new Vector3(2.5F, 2.5F, 1.5F);
			mdl.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			mdl.transform.localPosition = new Vector3(0, -0.05F, 0);
			/*
			GameObject mdl2 = ObjectUtil.lookupPrefab("07a05a2f-de55-4c60-bfda-cedb3ab72b88").getChildObject("jacksepticeye/Geo/container_geo");
			mdl2.name = "BaseModel";
			mdl2.transform.SetParent(mdl.transform.parent);
			Utils.ZeroTransform(mdl2.transform);
			mdl2.transform.localScale = new Vector3(1, 1, 1);
			mdl2.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			Renderer r2 = mdl2.GetComponentInChildren<Renderer>();
			r2.materials[1].SetColor("_Color", Color.clear);
			r2.materials[2].SetColor("_Color", Color.clear);
			*/
			WirelessChargerLogic lgc = go.GetComponent<WirelessChargerLogic>();

			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			RenderUtil.setEmissivity(r, 5);

			Constructable c = go.GetComponent<Constructable>();
			c.allowedInBase = true;
			c.allowedInSub = true;
			c.allowedOnCeiling = false;
			c.allowedOnWall = false;
			c.allowedOnGround = true;
			c.allowedOnConstructables = false;
			c.allowedOutside = false;
			c.model = mdl;

		}

	}

	public class WirelessChargerLogic : CustomMachineLogic {

		public static readonly float MAX_EFFICIENCY = 0.67F;
		public static readonly float MIN_EFFICIENCY = 0.1F;
		public static readonly float FALLOFF_START = 25F;
		public static readonly float MAX_RANGE = 60F;

		private static readonly Color offlineColor = new Color(0.1F, 0.1F, 0.1F);
		private static readonly Color noPlayerInRangeColor = new Color(1, 0, 0);
		private static readonly Color workingColor = new Color(0, 1, 0);
		private static readonly Color everythingChargedColor = new Color(0, 0.5F, 1);
		private static readonly Color noToolsColor = new Color(1, 0, 1);

		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "wcharger", "Sounds/wcharger.ogg", SoundManager.soundMode3D);

		private float lastSound = -1;

		void Start() {
			SNUtil.log("Reinitializing base wireless charger");
			AqueousEngineeringMod.wirelessChargerBlock.initializeMachine(gameObject);
		}
		/*
		protected override Renderer findRenderer() {
			return gameObject.getChildObject("Power_Transmitter").GetComponent<Renderer>();
		}*/

		protected override float getTickRate() {
			return 1;
		}

		protected override void updateEntity(float seconds) {
			Color c = noPlayerInRangeColor;
			if (sub) {
				Player ep = Player.main;
				if (ep) {
					float dist = (ep.transform.position-transform.position).magnitude;
					if (dist <= MAX_RANGE) {

						float eff = this.getEfficiency(dist);

						c = Color.Lerp(noPlayerInRangeColor, workingColor, eff);

						float wanted = 0;
						Dictionary<EnergyMixin, float> dict = new Dictionary<EnergyMixin, float>();

						foreach (EnergyMixin e in InventoryUtil.getAllHeldChargeables()) {
							IBattery ib = e.battery;
							float max = ib != null ? ib.capacity : e.maxEnergy;
							float space = max-(ib != null ? ib.charge : e.energy);
							space = Mathf.Min(space, seconds * WirelessCharger.MAX_CHARGE_FRAC_PER_SECOND * max);
							wanted += space;
							dict[e] = space;
						}

						if (dict.Count == 0) {
							c = noToolsColor;
						}
						else if (wanted <= 0) {
							c = everythingChargedColor;
						}
						else if (this.consumePower(wanted)) {
							float frac = powerConsumedLastAttempt/wanted;
							foreach (KeyValuePair<EnergyMixin, float> kvp in dict) {
								kvp.Key.AddEnergy(frac * eff * kvp.Value);
							}
							c = Color.Lerp(offlineColor, c, frac);

							BatteryChargeIndicatorHandler.resyncChargeIndicators();
						}
						else {
							c = offlineColor;
						}

						//SNUtil.writeToChat(dist.ToString("0.0")+" > "+dict.Count+" > T="+wanted.ToString("0.0")+" & C="+ powerConsumedLastAttempt.ToString("0.0"));
					}
				}
			}
			this.setEmissiveColor(c);
		}

		public float getEfficiency(float dist) {
			return (float)MathUtil.linterpolate(dist, FALLOFF_START, MAX_RANGE, MAX_EFFICIENCY, MIN_EFFICIENCY, true);
		}
	}
}
