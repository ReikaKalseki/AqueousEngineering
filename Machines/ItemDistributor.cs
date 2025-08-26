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

	public class ItemDistributor : CustomMachine<ItemDistributorLogic> {

		public ItemDistributor(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5fc7744b-5a2c-4572-8e53-eebf990de434") {
			this.addIngredient(TechType.ComputerChip, 2);
			this.addIngredient(TechType.Titanium, 2);
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

			ItemDistributorLogic lgc = go.GetComponent<ItemDistributorLogic>();

			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			this.initializeStorageContainer(con, 5, 5);

			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.lookupPrefab("c5ae1472-0bdc-4203-8418-fb1f74c8edf5"));
			mdl.transform.localScale = new Vector3(1, 2, 1);

			Constructable c = go.GetComponent<Constructable>();
			c.model = mdl;
			c.allowedOnCeiling = false;
			c.allowedOnGround = false;
			c.allowedOnWall = true;
			c.allowedOnConstructables = false;
			c.allowedOutside = true;

			Renderer r = mdl.GetComponentInChildren<Renderer>();
		}

	}

	public class ItemDistributorLogic : CustomMachineLogic {

		private readonly Dictionary<TechType, List<StorageContainer>> targets = new Dictionary<TechType, List<StorageContainer>>();

		void Start() {
			SNUtil.log("Reinitializing base item distributor");
			//AqueousEngineeringMod.ionCubeBlock.initializeMachine(gameObject);
		}

		protected override float getTickRate() {
			return 0.5F;
		}

		protected override void updateEntity(float seconds) {
			if (sub && storage) {

			}
		}

		public void rebuildStorages() {
			targets.Clear();
			if (!sub)
				return;
			foreach (StorageContainer sc in sub.GetComponentsInChildren<StorageContainer>()) {
				this.addStorage(sc);
			}
		}

		public void removeStorage(StorageContainer sc) {
			foreach (List<StorageContainer> li in targets.Values) {
				li.Remove(sc);
			}
		}

		public void addStorage(StorageContainer sc) {
			IEnumerable<TechType> ie = this.getRelevantTypes(sc);
			if (ie == null)
				return;
			foreach (TechType tt in ie) {
				if (!targets.ContainsKey(tt)) {
					targets[tt] = new List<StorageContainer>();
				}
				targets[tt].Add(sc);
			}
		}

		private IEnumerable<TechType> getRelevantTypes(StorageContainer sc) {
			if (SNUtil.match(sc.GetComponent<PrefabIdentifier>(), "5fc7744b-5a2c-4572-8e53-eebf990de434")) { //small locker
				GameObject lbl = sc.gameObject.getChildObject("Label");
				string text = lbl.GetComponent<uGUI_SignInput>().inputField.text;
			}
			return sc.GetComponent<CyclopsLocker>() || sc.GetComponent<RocketLocker>() ? sc.container.GetItemTypes() : (IEnumerable<TechType>)null;
		}
	}
}
