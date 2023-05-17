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
	
	public class BaseControlPanel : CustomMachine<BaseControlPanelLogic> {
		
		public BaseControlPanel(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, /*"cf522a95-3038-4759-a53c-8dad1242c8ed"*/"c5ae1472-0bdc-4203-8418-fb1f74c8edf5") {
			addIngredient(TechType.WiringKit, 1);
			addIngredient(TechType.Titanium, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return true;
			}
		}
		
		public override bool isOutdoors() {
			return false;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
						
			ObjectUtil.removeComponent<StorageContainer>(go);
			BaseControlPanelLogic lgc = go.GetComponent<BaseControlPanelLogic>();
			
			GameObject mdl = RenderUtil.setModel(go, "shelve_02", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("9460942c-2347-4b58-b9ff-0f7f693dc9ff"), "Starship_work_desk_01"));
			mdl.transform.localPosition = new Vector3(0, 0, 0);
			mdl.transform.localScale = Vector3.one;
			mdl.transform.SetParent(go.transform);
			mdl.transform.localRotation = Quaternion.Euler(0, 0, 0);
			
			ObjectUtil.removeChildObject(go, "collisions");
			BoxCollider b = go.EnsureComponent<BoxCollider>();
			b.size = new Vector3(2.75F, 1, 1);
			b.center = new Vector3(0.2F, 0, 0);
			
			Renderer r = mdl.GetComponentInChildren<Renderer>();
			r.transform.localScale = new Vector3(1, 1, 0.1F);
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			Constructable c = go.GetComponent<Constructable>();
			c.allowedInBase = true;
			c.allowedInSub = false;
			c.allowedOnCeiling = false;
			c.allowedOnGround = false;
			c.allowedOnConstructables = false;
			c.allowedOnWall = true;
			c.allowedOutside = false;
			c.rotationEnabled = true;
			c.model = mdl;
		}
		
	}
		
	public class BaseControlPanelLogic : CustomMachineLogic {
		
		private HolographicControl.HolographicControlTag[] buttons = null;
		
		void Start() {
			SNUtil.log("Reinitializing base control panel");
			AqueousEngineeringMod.controlsBlock.initializeMachine(gameObject);
		}
		
		public void addButton(HolographicControl control) {
			GameObject box = ObjectUtil.getChildObject(gameObject, "ButtonHolder");
			if (!box) {
				box = new GameObject("ButtonHolder");
				box.transform.SetParent(transform);
			}
			box.transform.localPosition = Vector3.zero;
			box.transform.localRotation = Quaternion.identity;
			foreach (PrefabIdentifier pi in box.transform.GetComponentsInChildren<PrefabIdentifier>()) {
				if (pi && pi.classId == control.ClassID)
					return;
			}
			GameObject btn = ObjectUtil.createWorldObject(control.ClassID);
			btn.transform.SetParent(box.transform);
			buttons = box.GetComponentsInChildren<HolographicControl.HolographicControlTag>();
			foreach (HolographicControl.HolographicControlTag tag in buttons) {
				float f = 2F/buttons.Length-1;
				tag.transform.parent.localPosition = new Vector3(f, 0, 0.1F);
				tag.transform.parent.localScale = new Vector3(2, 2, 1F);
				tag.transform.localRotation = Quaternion.identity;
				tag.transform.parent.localRotation = Quaternion.identity;
			}
		}
		
		protected override void updateEntity(float seconds) {
			
		}	
	}
}
