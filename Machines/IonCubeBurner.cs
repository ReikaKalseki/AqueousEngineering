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
	
	[Obsolete]
	public class IonCubeBurner : CustomMachine<IonCubeBurnerLogic> {
		
		internal static readonly float POWER_RATE = 40F; //PPS
		internal static readonly float CUBE_VALUE = 300000; //total
		
		public IonCubeBurner(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "??") {
			addIngredient(TechType.AdvancedWiringKit, 2);
			addIngredient(TechType.Polyaniline, 1);
			addIngredient(TechType.EnameledGlass, 3);
			
			glowIntensity = 1;
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
			ObjectUtil.removeComponent<Trashcan>(go);
						
			IonCubeBurnerLogic lgc = go.GetComponent<IonCubeBurnerLogic>();
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 2, 2);
						
			GameObject mdl = RenderUtil.setModel(go, "bedc40fb-bd97-4b4d-a943-d39360c9c7bd", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("??"), "??"));
			mdl.transform.localRotation = Quaternion.Euler(-90, 180, 0);
			mdl.transform.localPosition = new Vector3(0, -0.05F, 0);
			mdl.transform.localScale = new Vector3(1.5F, 0.5F, 0.5F);
			
			Renderer r = mdl.GetComponentInChildren<Renderer>();
		}
		
	}
		
	public class IonCubeBurnerLogic : CustomMachineLogic {
		
		void Start() {
			SNUtil.log("Reinitializing base ion cube burner");
			//AqueousEngineeringMod.ionCubeBlock.initializeMachine(gameObject);
		}

		protected override float getTickRate() {
			return 0.1F;
		}
		
		public override float getBaseEnergyStorageCapacityBonus() {
			return 400;
		}
		
		protected override void updateEntity(float seconds) {
			SubRoot sub = getSub();
			if (sub) {
				float space = sub.powerRelay.GetMaxPower()-sub.powerRelay.GetPower();
				if (space > 0) {
					float add = Mathf.Min(space, IonCubeBurner.POWER_RATE);
					StorageContainer sc = getStorage();
					if (sc) {
						sc.container.GetItems(TechType.PrecursorIonCrystal);
					}
				}
			}
		}
	}
}
