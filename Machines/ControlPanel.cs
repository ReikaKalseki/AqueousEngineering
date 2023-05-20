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
		
		private static float[] offsets = new float[]{0, -0.125F, 0.33F, 0.5F};
		
		private HolographicControl.HolographicControlTag[] buttons = null;
		
		private float lastButtonValidityCheck = -1;
		
		private readonly HashSet<string> activeButtons = new HashSet<string>();
		
		void Start() {
			SNUtil.log("Reinitializing base control panel");
			AqueousEngineeringMod.controlsBlock.initializeMachine(gameObject);
		}
		
		protected override void load(System.Xml.XmlElement data) {
			activeButtons.Clear();
			foreach (System.Xml.XmlElement e in data.getDirectElementsByTagName("activeButton")) {
				activeButtons.Add(e.InnerText);
			}
		}
		
		protected override void save(System.Xml.XmlElement data) {
			foreach (string s in activeButtons) {
				System.Xml.XmlElement e = data.OwnerDocument.CreateElement("activeButton");
				e.InnerText = s;
				data.AppendChild(e);
			}
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
			HolographicControl.HolographicControlTag com = btn.GetComponentInChildren<HolographicControl.HolographicControlTag>();
			if (activeButtons.Contains(control.ClassID)) {
				com.setState(true);
				//activeButtons.Remove(control.ClassID);
			}
			btn.transform.SetParent(box.transform);
			updateButtons();
		}
		
		void updateButtons() {
			buttons = GetComponentsInChildren<HolographicControl.HolographicControlTag>();
			float offset = -0.4F+buttons.Length*0.3125F;//0.33F; //-0.125 for 1, 0.33 for 2, 0.5 for 3;
			if (buttons.Length < offsets.Length)
				offset = offsets[buttons.Length];
			for (int i = 0; i < buttons.Length; i++) {
				HolographicControl.HolographicControlTag tag = buttons[i];
				float f = (2F/buttons.Length)*i-offset;
				tag.transform.parent.localPosition = new Vector3(f, 0, 0.1F);
				tag.transform.parent.localScale = new Vector3(2, 2, 1F);
				tag.transform.localRotation = Quaternion.identity;
				tag.transform.parent.localRotation = Quaternion.identity;
			}
		}
		
		void SetHolographicControlState(HolographicControl.HolographicControlTag tag) {
			string id = tag.GetComponentInParent<PrefabIdentifier>().ClassId;
			if (tag.getState())
				activeButtons.Add(id);
			else
				activeButtons.Remove(id);
		}
		
		protected override void updateEntity(float seconds) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastButtonValidityCheck >= 1) {
				lastButtonValidityCheck = time;
				bool changed = false;
				foreach (HolographicControl.HolographicControlTag tag in buttons) {
					if (tag && !tag.isStillValid()) {
						tag.destroy();
						activeButtons.Remove(tag.GetComponentInParent<PrefabIdentifier>().ClassId);
						changed = true;
					}
				}
				if (changed)
					updateButtons();
			}
		}	
	}
}
