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
using UnityEngine.UI;

namespace ReikaKalseki.AqueousEngineering {

	public class RoomDataDisplay : CustomMachine<RoomDataDisplayLogic> {

		public RoomDataDisplay(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "b343166e-3a17-4a1c-85d1-05dee8ec1575") {
			this.addIngredient(TechType.Titanium, 1);
			this.addIngredient(TechType.Quartz, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return true;
			}
		}

		public override bool isOutdoors() {
			return false;
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Miscellaneous;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Misc;
			}
		}

		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			go.removeComponent<Sign>();
			go.removeComponent<uGUI_SignInput>();
			//go.removeChildObject("UI/Base/InputField");
			//go.removeChildObject("SignMesh"); //used for the constructable!!!
			//go.removeChildObject("Trigger");
			go.removeChildObject("UI/Base/Up");
			go.removeChildObject("UI/Base/Down");
			go.removeChildObject("UI/Base/Left");
			go.removeChildObject("UI/Base/Right");
			go.removeChildObject("UI/Base/Minus");
			go.removeChildObject("UI/Base/Plus");
			go.getChildObject("UI").SetActive(true);

			Constructable ctr = go.EnsureComponent<Constructable>();/*
			ctr.CopyFields(ObjectUtil.lookupPrefab(baseTemplate.prefab).GetComponent<Constructable>());
			ctr.model = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(baseTemplate.prefab).GetComponent<Constructable>().model);
			ctr.model.transform.SetParent(go.transform);
			ctr.model.transform.localPosition = Vector3.zero;
			ctr.techType = TechType;*/
			ctr.allowedInBase = true;
			ctr.allowedInSub = false;
			ctr.allowedOnConstructables = true;
			ctr.allowedOnGround = false;
			ctr.allowedOnWall = true;
			ctr.allowedOutside = false;
			ctr.allowedOnCeiling = false;
			ctr.forceUpright = true;
			//ctr.model = UnityEngine.Object.Instantiate(ObjectUtil.lookupPrefab(baseTemplate.prefab).GetComponent<Constructable>().model);
			//ctr.model.SetActive(true);
		}

	}

	public class RoomDataDisplayLogic : CustomMachineLogic {

		private uGUI_InputField field;
		private Text[] text = null;

		void Start() {
			SNUtil.log("Reinitializing base room data display");
			AqueousEngineeringMod.roomDataBlock.initializeMachine(gameObject);
		}

		protected override float getTickRate() {
			return 0.5F;
		}

		protected override void updateEntity(float seconds) {
			if (text == null)
				text = this.GetComponentsInChildren<Text>();
			if (!field)
				field = this.GetComponentInChildren<uGUI_InputField>();
			BaseRoomSpecializationSystem.RoomTypes type = BaseRoomSpecializationSystem.instance.getSavedType(this, out float deco, out float decoThresh);
			string name = AqueousEngineeringMod.roomLocale.getEntry(Enum.GetName(typeof(BaseRoomSpecializationSystem.RoomTypes), type)).name;
			string put = name+" ("+deco.ToString("0.00")+")";
			field.text = put;
			foreach (Text t in text)
				t.text = put;
			field.enabled = false;
			field.enabled = true; //trigger refresh
		}
	}
}
