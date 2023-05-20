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
	
	public class BaseStasisTurret : CustomMachine<BaseStasisTurretLogic> {
		
		internal static readonly float POWER_COST = 250F; //per shot
		internal static readonly float POWER_LEVEL = 0.75F;
		internal static readonly float RADIUS = 18F;
		internal static readonly float COOLDOWN = 20F;
		
		public BaseStasisTurret(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			//addIngredient(TechType.StasisRifle, 1);
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(TechType.Polyaniline, 2);
			addIngredient(TechType.TitaniumIngot, 1);
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
		
		void Start() {
			SNUtil.log("Reinitializing base stasis turret");
			AqueousEngineeringMod.stasisBlock.initializeMachine(gameObject);
		}
		
		private void addButton() {/*
			foreach (BaseFoundationPiece bf in getSub().GetComponentsInChildren<BaseFoundationPiece>()) {
				if (bf.name.Contains("Moonpool")) {
					GameObject box = ObjectUtil.getChildObject(bf.gameObject, "ButtonHolder");
					if (!box) {
						box = new GameObject("ButtonHolder");
						box.transform.SetParent(bf.transform);
						box.transform.localPosition = Vector3.zero;
						GameObject btn = ObjectUtil.createWorldObject(AqueousEngineeringMod.seabaseStasisControl.ClassID);
						btn.transform.SetParent(box.transform);
						btn.transform.localPosition = new Vector3(0, 0.75F, 3F);
					}
				}
			}*/
			foreach (BaseControlPanelLogic panel in getSub().GetComponentsInChildren<BaseControlPanelLogic>()) {
				panel.addButton(AqueousEngineeringMod.seabaseStasisControl);
			}
		}
		
		public void fire() {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastFire > BaseStasisTurret.COOLDOWN && consumePower(BaseStasisTurret.POWER_COST)) {
				lastFire = time;
				GameObject sph = ObjectUtil.lookupPrefab(TechType.StasisRifle).GetComponent<StasisRifle>().effectSpherePrefab;
				sph = UnityEngine.Object.Instantiate(sph);
				sph.SetActive(true);
				ObjectUtil.fullyEnable(sph);
				sph.transform.position = transform.position;
				StasisSphere ss = sph.GetComponent<StasisSphere>();
				ss.fieldEnergy = BaseStasisTurret.POWER_LEVEL;
				ss.time = Mathf.Lerp(ss.minTime, ss.maxTime, ss.fieldEnergy);
				ss.radius = BaseStasisTurret.RADIUS;
				ss.EnableField();
			}
		}
		
		protected override void updateEntity(float seconds) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastButtonCheck >= 1) {
				lastButtonCheck = time;
				addButton();
			}
		}	
	}
}
