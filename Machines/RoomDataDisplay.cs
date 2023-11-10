using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class RoomDataDisplay : CustomMachine<RoomDataDisplayLogic> {
		
		public RoomDataDisplay(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "b343166e-3a17-4a1c-85d1-05dee8ec1575") {
			addIngredient(TechType.Titanium, 1);
			addIngredient(TechType.Quartz, 1);
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
			ObjectUtil.removeComponent<Sign>(go);
			ObjectUtil.removeComponent<uGUI_SignInput>(go);
			//ObjectUtil.removeChildObject(go, "UI/Base/InputField");
			//ObjectUtil.removeChildObject(go, "SignMesh"); //used for the constructable!!!
			//ObjectUtil.removeChildObject(go, "Trigger");
			ObjectUtil.removeChildObject(go, "UI/Base/Up");
			ObjectUtil.removeChildObject(go, "UI/Base/Down");
			ObjectUtil.removeChildObject(go, "UI/Base/Left");
			ObjectUtil.removeChildObject(go, "UI/Base/Right");
			ObjectUtil.removeChildObject(go, "UI/Base/Minus");
			ObjectUtil.removeChildObject(go, "UI/Base/Plus");
			ObjectUtil.getChildObject(go, "UI").SetActive(true);
			
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
				text = GetComponentsInChildren<Text>();
			if (!field)
				field = GetComponentInChildren<uGUI_InputField>();
			float deco;
			float decoThresh;
			BaseRoomSpecializationSystem.RoomTypes type = BaseRoomSpecializationSystem.instance.getSavedType(this, out deco, out decoThresh);
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
