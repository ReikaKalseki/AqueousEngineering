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
		
		internal static readonly float POWER_GEN = 12F; //max, per s per ampeel
		internal static readonly float POWER_FALLOFF = 0.08F; //per meter
		internal static readonly float RANGE = POWER_GEN/POWER_FALLOFF;
		internal static readonly float INTERVAL = 0.25F;
		
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
		
		float lastTime = -1;
		
		void Start() {
			SNUtil.log("Reinitializing base ampeel antenna");
			AqueousEngineeringMod.ampeelAntennaBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			if (seconds > 0) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastTime < AmpeelAntenna.INTERVAL)
					return;
				lastTime = time;
				seconds = time-lastTime;
				SubRoot sub = getSub();
				if (sub && sub.powerRelay.GetPower() < sub.powerRelay.GetMaxPower()) {
					RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, AmpeelAntenna.RANGE, new Vector3(1, 1, 1), AmpeelAntenna.RANGE);
					foreach (RaycastHit rh in hit) {
						if (rh.transform != null && rh.transform.gameObject) {
							Shocker c = rh.transform.gameObject.GetComponent<Shocker>();
							if (c && c.liveMixin.IsAlive() && !c.gameObject.GetComponent<WaterParkCreature>()) {
								float dd = Vector3.Distance(c.transform.position, transform.position);
								if (dd >= AmpeelAntenna.RANGE)
									continue;
								float trash;
								sub.powerRelay.AddEnergy(seconds*(AmpeelAntenna.POWER_GEN-AmpeelAntenna.POWER_FALLOFF*dd), out trash);
							}
						}
					}
				}
			}
		}	
	}
}
