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
	
	public class BaseSonarPinger : CustomMachine<BaseSonarPingerLogic> {
		
		internal static readonly float POWER_COST = 10F; //per ping
		internal static readonly float FIRE_RATE = 4F; //interval in seconds
		internal static readonly float MAX_RANGE = 300F; //m
		
		public static event Action<GameObject> onBaseSonarPingedEvent;
		
		public BaseSonarPinger(XMLLocale.LocaleEntry e) : base("basesonarping", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.Magnetite, 3);
			addIngredient(TechType.Gold, 2);
			addIngredient(TechType.CyclopsSonarModule, 1);
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
						
			BaseSonarPingerLogic lgc = go.GetComponent<BaseSonarPingerLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);/*
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
		internal static void pingEvent(GameObject go) {
			if (onBaseSonarPingedEvent != null)
				onBaseSonarPingedEvent.Invoke(go);
		}
		
	}
		
	public class BaseSonarPingerLogic : CustomMachineLogic {
		
		private float lastPing;
		
		private GameObject rotator;
		
		void Start() {
			SNUtil.log("Reinitializing base sonar");
			AqueousEngineeringMod.sonarBlock.initializeMachine(gameObject);
		}
		
		private void ping(float time) {
			if (consumePower(BaseSonarPinger.POWER_COST, 1)) {
				lastPing = time;
				BaseSonarPinger.pingEvent(gameObject);
				if (Inventory.main.equipment.GetCount(TechType.MapRoomHUDChip) > 0)
					SNCameraRoot.main.SonarPing();
				SoundManager.playSoundAt(SoundManager.buildSound("event:/sub/cyclops/sonar"), Player.main.transform.position, false, BaseSonarPinger.MAX_RANGE, 4);
			}
		}
		
		private bool isInAppropriateLocation() {
			Vector3 p1 = Player.main.transform.position;
			Vector3 p2 = gameObject.transform.position;
			return p1.y >= p2.y-100 && Vector3.Distance(p1, p2) <= BaseSonarPinger.MAX_RANGE;
		}
		
		protected override void updateEntity(float seconds) {
			//if (mainRenderer == null)
			//	mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (!rotator)
				rotator = ObjectUtil.getChildObject(gameObject, "Power_Transmitter");
			float time = DayNightCycle.main.timePassedAsFloat;
			if (rotator && getSub() && getSub().powerRelay.GetPower() > 0.1F) {
				Vector3 angs = rotator.transform.localEulerAngles;
				angs.y += 90*seconds;
				rotator.transform.localEulerAngles = angs;
			}
			if (time-lastPing >= BaseSonarPinger.FIRE_RATE && isInAppropriateLocation()) {
				ping(time);
			}
		}	
	}
}
