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
	
	public class ACUMonitor : CustomMachine<ACUMonitorLogic>, MultiTexturePrefab {
		
		internal XMLLocale.LocaleEntry locale;
		
		public ACUMonitor(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "5c06baec-0539-4f26-817d-78443548cc52") {
			locale = e;
			addIngredient(TechType.Titanium, 2);
			addIngredient(TechType.Quartz, 2);
			addIngredient(TechType.CopperWire, 1);
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
			ObjectUtil.removeComponent<Radio>(go);
			ObjectUtil.removeChildObject(go, "xFlare");
			
			GameObject mdl = RenderUtil.setModel(go, "Mesh", ObjectUtil.lookupPrefab("b460a6a6-2a05-472c-b4bf-c76ae49d9a29"));
						
			ACUMonitorLogic lgc = go.GetComponent<ACUMonitorLogic>();
			
			Constructable c = go.GetComponent<Constructable>();
			c.model = mdl;
			c.allowedOnCeiling = false;
			c.allowedOnGround = false;
			c.allowedOnWall = true;
			c.allowedOnConstructables = true;
			c.allowedOutside = false;
			
			foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
				RenderUtil.swapToModdedTextures(r, this);
				r.materials[0].SetColor("_Color", new Color(0.5F, 0.5F, 0.5F, 1));
				//RenderUtil.setGlossiness(r, );
			}
			
			go.GetComponent<Constructable>().model = mdl;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
		public Dictionary<int, string> getTextureLayers(Renderer r) {
			return new Dictionary<int, string>{{0, "Base"}, {1, "Screen"}};
		}
		
	}
		
	public class ACUMonitorLogic : CustomMachineLogic, IHandTarget {
		
		private WaterPark connectedACU;
		
		//internal GameObject rotator;
				
		void Start() {
			SNUtil.log("Reinitializing acu cleaner");
			AqueousEngineeringMod.acuMonitorBlock.initializeMachine(gameObject);
		}
		
		protected override float getTickRate() {
			return 0;
		}
		
		private WaterPark tryFindACU() {
			SubRoot sub = getSub();
			if (!sub) {
				return null;
			}
			foreach (WaterPark wp in sub.GetComponentsInChildren<WaterPark>()) {
				if (Vector3.Distance(wp.transform.position, transform.position) <= 6) {
					return wp;
				}
			}
			return null;
		}
		
		protected override void updateEntity(float seconds) {
			if (!connectedACU) {
				connectedACU = tryFindACU();
			}
		}
		
		public void OnHandHover(GUIHand hand) {
			if (connectedACU) {
				HandReticle.main.SetInteractText(AqueousEngineeringMod.acuMonitorBlock.locale.getField<string>("tooltip"), false);
				HandReticle.main.SetIcon(HandReticle.IconType.Interact);
			}
			else {
				HandReticle.main.SetInteractText(AqueousEngineeringMod.acuMonitorBlock.locale.getField<string>("noacu"), false);
				HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
			}
		}
	
		public void OnHandClick(GUIHand hand) {
			if (connectedACU) {
				ACUCallbackSystem.ACUCallback call = connectedACU.GetComponent<ACUCallbackSystem.ACUCallback>();
				if (call)
					call.printTerminalInfo();
				else
					SNUtil.writeToChat("ACU is in an invalid state.");
			}
		}
	}
}
