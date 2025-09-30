using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {

	public class FloatingPowerRelay : CustomMachine<FloatingPowerRelayLogic> {

		public FloatingPowerRelay(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			this.addIngredient(TechType.Titanium, 1);
			this.addIngredient(TechType.Floater, 1);
			this.addIngredient(TechType.Gold, 1);
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

			FloatingPowerRelayLogic lgc = go.GetComponent<FloatingPowerRelayLogic>();

			GameObject mdl = go.setModel("Power_Transmitter", ObjectUtil.lookupPrefab(TechType.Gravsphere).getChildObject("gravSphere_anim"));
			GameObject inner = mdl.getChildObject("Gravsphere");
			foreach (Renderer rr in inner.GetComponentsInChildren<Renderer>()) {
				if (rr.gameObject != inner)
					rr.gameObject.SetActive(false);
			}
			mdl.transform.localScale = new Vector3(0.33F, 0.33F, 0.33F);
			mdl.transform.localRotation = Quaternion.identity;
			//mdl.GetComponent<Animator>().StopPlayback();

			Renderer r = inner.GetComponent<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 1);

			go.EnsureComponent<Rigidbody>().mass = 20;

			go.removeComponent<Collider>();
			SphereCollider sc = go.EnsureComponent<SphereCollider>();
			sc.radius = 0.2F;

			go.EnsureComponent<EcoTarget>().SetTargetType(EcoTargetType.Shiny);
			go.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
			ObjectUtil.makeMapRoomScannable(go, TechType, true);

			go.GetComponent<PowerFX>().attachPoint.transform.localPosition = Vector3.zero;

			Constructable c = go.GetComponent<Constructable>();
			c.allowedOnCeiling = false;
			c.allowedOnConstructables = false;
			c.allowedOnGround = false;
			c.allowedOnWall = false;
			c.model = mdl;//go.GetComponentInChildren<Renderer>().gameObject.clone();//mdl;
			c.model.EnsureComponent<PowerRelay>();
			c.model.removeComponent<Collider>();
		}

	}

	public class FloatingPowerRelayLogic : CustomMachineLogic {

		private static readonly float MAX_ROTATE_SPEED = 18F;

		private Vector3 rotationSpeed;

		private Renderer mainRenderer;

		void Start() {
			SNUtil.log("Reinitializing base floating power relay");
			AqueousEngineeringMod.powerRelayBlock.initializeMachine(gameObject);
		}

		protected override bool needsAttachedBase() {
			return false;
		}

		protected override float getTickRate() {
			return 0F;
		}

		protected override void load(XmlElement data) {

		}

		protected override void save(XmlElement data) {

		}

		protected override void updateEntity(float seconds) {
			if (!mainRenderer)
				mainRenderer = this.GetComponentInChildren<Renderer>();

			RenderUtil.setEmissivity(mainRenderer, sub ? 1 : 0.2F);

			transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
			rotationSpeed += new Vector3(UnityEngine.Random.Range(-0.5F, 0.5F), UnityEngine.Random.Range(-0.5F, 0.5F), UnityEngine.Random.Range(-0.5F, 0.5F));
			rotationSpeed.x = Mathf.Clamp(rotationSpeed.x, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
			rotationSpeed.y = Mathf.Clamp(rotationSpeed.y, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
			rotationSpeed.z = Mathf.Clamp(rotationSpeed.z, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
		}
	}
}
