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
		
		internal static readonly float POWER_GEN = 3F; //max, per s per ampeel
		internal static readonly float POWER_FALLOFF = 0.12F; //per meter
		internal static readonly float RANGE = POWER_GEN/POWER_FALLOFF;
		internal static readonly float INTERVAL = 0.25F;
		
		public AmpeelAntenna(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
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
			ObjectUtil.removeComponent<Bench>(go);
			ObjectUtil.removeChildObject(go, "Bubbles");
			
			go.transform.localScale = new Vector3(0.4F, 0.2F, 1);
			
			go.GetComponent<Constructable>().model = ObjectUtil.getChildObject(go, "bench");
			
			string name = "CoilHolder";
			GameObject child = ObjectUtil.getChildObject(go, name);
			if (child == null) {
				child = new GameObject(name);
				child.transform.SetParent(go.transform);
			}
			PrefabIdentifier[] pi = child.GetComponentsInChildren<PrefabIdentifier>();
			int n = 15;
			for (int i = pi.Length; i < n; i++) {
				GameObject fin = ObjectUtil.createWorldObject("cf522a95-3038-4759-a53c-8dad1242c8ed");
				RenderUtil.convertToModel(fin);
				fin.EnsureComponent<AmpeelCoil>();
				fin.transform.SetParent(child.transform);
				fin.transform.localScale = new Vector3(0.09F, 0.13F, 0.05F);
				fin.transform.localRotation = Quaternion.Euler(0, 0, 0);
				fin.transform.localPosition = new Vector3(-0.015F, 0, -0.75F+1.5F*i/n);//Vector3.zero+i*go.transform.right*0.25F;
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
		
	}
	
	class AmpeelCoil : MonoBehaviour {
		
	}
		
	public class AmpeelAntennaLogic : CustomMachineLogic {
		
		private GameObject wireObject;
		
		void Start() {
			SNUtil.log("Reinitializing base ampeel antenna");
			AqueousEngineeringMod.ampeelAntennaBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return AmpeelAntenna.INTERVAL;
		}
		
		public override float getBaseEnergyStorageCapacityBonus() {
			return 50;
		}
		
		protected override void updateEntity(float seconds) {
			SubRoot sub = getSub();
			if (sub && sub.powerRelay.GetPower() < sub.powerRelay.GetMaxPower()) {
				HashSet<Shocker> set = WorldUtil.getObjectsNearWithComponent<Shocker>(gameObject.transform.position, AmpeelAntenna.RANGE);
				foreach (Shocker c in set) {
					if (c.liveMixin.IsAlive() && !c.gameObject.GetComponent<WaterParkCreature>()) {
						float dd = Vector3.Distance(c.transform.position, transform.position);
						if (dd >= AmpeelAntenna.RANGE)
							continue;
						float trash;
						sub.powerRelay.AddEnergy(seconds*c.liveMixin.GetHealthFraction()*(AmpeelAntenna.POWER_GEN-AmpeelAntenna.POWER_FALLOFF*dd), out trash);
					}
				}
			}
		}	
	}
}
