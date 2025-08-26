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

	public class ACUBooster : CustomMachine<ACUBoosterLogic> {

		internal static readonly float POWER_COST = 0.0625F;
		internal static readonly float CONSUMPTION_RATE = 15*60; //s

		internal static readonly Dictionary<string, ACUFuel> fuels = new Dictionary<string, ACUFuel>();

		public ACUBooster(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5fc7744b-5a2c-4572-8e53-eebf990de434") {
			this.addIngredient(TechType.Titanium, 1);
			this.addIngredient(TechType.Pipe, 4);
			this.addIngredient(TechType.FiberMesh, 2);
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
			go.removeChildObject("Label");

			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.lookupPrefab("cdade216-3d4d-4adf-901c-3a91fb3b88c4").getChildObject("model/submarine_centrifuge_base"));
			mdl.transform.localScale = Vector3.one * 50;
			mdl.transform.localRotation = Quaternion.Euler(-90, 0, 0);

			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			this.initializeStorageContainer(con, 6, 5);

			ACUBoosterLogic lgc = go.GetComponent<ACUBoosterLogic>();

			//GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
			//GameObject mdl = RenderUtil.setModel(go, "discovery_trashcan_01_d", air.getChildObject("model"));
			//lgc.rotator = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(air, "model").getChildObject("_pipes_floating_air_intake_turbine_geo"));
			//lgc.rotator.transform.parent = go.transform;

			Renderer[] r = go.GetComponentsInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/

			Constructable c = go.GetComponent<Constructable>();
			c.model = mdl;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);

			c.allowedOnCeiling = false;
			c.allowedOnGround = false;
			c.allowedOnWall = true;
			c.allowedOnConstructables = true;
			c.allowedOutside = false;
		}

	}

	public class ACUFuel {

		public readonly WorldCollectedItem item;
		public readonly float lifetimeModifier;
		public readonly float effectStrength;

		public ACUFuel(WorldCollectedItem item, float s, float l) {
			this.item = item;
			lifetimeModifier = l;
			effectStrength = s;
		}

	}

	public class ACUBoosterLogic : CustomMachineLogic {

		private WaterPark connectedACU;

		private float lastFeedTime;

		//internal GameObject rotator;

		void Start() {
			SNUtil.log("Reinitializing acu booster");
			AqueousEngineeringMod.acuBoosterBlock.initializeMachine(gameObject);
		}

		protected override float getTickRate() {
			return 2;
		}

		protected override void load(System.Xml.XmlElement data) {
			lastFeedTime = (float)data.getFloat("last", float.NaN);
		}

		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("last", lastFeedTime);
		}

		private WaterPark tryFindACU() {
			if (!sub)
				return null;
			foreach (WaterPark wp in sub.GetComponentsInChildren<WaterPark>()) {
				if (Vector3.Distance(wp.transform.position, transform.position) <= 6) {
					return wp;
				}
			}
			return null;
		}

		protected override void updateEntity(float seconds) {
			if (!connectedACU) {
				connectedACU = this.tryFindACU();
			}
			if (connectedACU && this.consumePower(ACUBooster.POWER_COST * seconds)) {
				ACUFuel fuel = this.tryFindFuel();
				if (fuel != null) {
					//rotator.transform.position = connectedACU.transform.position+Vector3.down*1.45F;
					//rotator.transform.localScale = new Vector3(13.8F, 1, 13.8F);
					/*
					foreach (WaterParkItem wp in connectedACU.items) {
						if (wp && wp is WaterParkCreature) {
							
						}
					}*/
					ACUCallbackSystem.ACUCallback hook = connectedACU.GetComponent<ACUCallbackSystem.ACUCallback>();
					if (hook) {
						hook.boost(fuel);
						float time = DayNightCycle.main.timePassedAsFloat;
						if (time - lastFeedTime >= ACUBooster.CONSUMPTION_RATE * fuel.lifetimeModifier) {
							lastFeedTime = time;
							storage.container.DestroyItem(fuel.item.TechType);
						}
					}
				}
			}
		}

		private ACUFuel tryFindFuel() {
			foreach (ACUFuel f in ACUBooster.fuels.Values) {
				if (storage.container.GetCount(f.item.TechType) > 0)
					return f;
			}
			return null;
		}
	}
}
