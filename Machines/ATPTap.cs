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
	
	public class ATPTap : CustomMachine<ATPTapLogic> {
		
		public ATPTap(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "a620b5d5-b413-4627-84b0-1e3a7c6bf5b6") {
			addIngredient(TechType.PrecursorIonCrystal, 4);
			addIngredient(TechType.AdvancedWiringKit, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		protected override bool isPowerGenerator() {
			return true;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
						
			ATPTapLogic lgc = go.GetComponent<ATPTapLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 2);
			//r.materials[0].SetFloat("_Shininess", 2F);
			//r.materials[0].SetFloat("_Fresnel", 0.6F);
			//r.materials[0].SetFloat("_SpecInt", 8F);
			
			Constructable c = go.GetComponent<Constructable>();
			c.allowedOnWall = true;
			c.allowedOutside = true;
			c.allowedOnCeiling = true;
			c.allowedOnGround = true;
			c.allowedOnConstructables = true;
			c.forceUpright = false;
			
			ObjectUtil.removeChildObject(go, "model/root/head");
			ObjectUtil.removeChildObject(go, "UI/Canvas/temperatureBar");
		}
		
	}
		
	public class ATPTapLogic : CustomMachineLogic {
		
		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "atptap", "Sounds/atptap.ogg", SoundManager.soundMode3D);
		
		private GameObject powerSource;
		
		private ThermalPlant thermalComponent;
		
		private Renderer render;
		
		private float lastSound = -1;
		
		private static readonly HashSet<string> validObjects = new HashSet<string>(){
			//cables
			"31f84eba-d435-438c-a58e-f3f7bae8bfbd",
			"69cd7462-7cd2-456c-bfff-50903c391737",
			"94933bb3-0587-4e8d-a38d-b7ec4c859b1a",
			"37f07c77-ac44-4246-9f53-1d186fb99921",
			"2334eec8-0968-4e0f-8441-25e0f76fc6b6",
			
			//sanctuaries
			"640f57a6-6436-4132-a9bb-d914f3e19ef5", //pillars with light column, used as spotlights
			
		};
		
		public static bool isValidSourceObject(GameObject go) {
			PrefabIdentifier pi = go.FindAncestor<PrefabIdentifier>();
			return pi && validObjects.Contains(pi.ClassId);
		}
		
		private static readonly Vector3 drfLocation = new Vector3(-248, -800, 281);
		
		void Start() {
			SNUtil.log("Reinitializing ATP tap");
			AqueousEngineeringMod.atpTapBlock.initializeMachine(gameObject);
			base.InvokeRepeating("tryFindCable", UnityEngine.Random.value, 4f);
			base.InvokeRepeating("AddPower", UnityEngine.Random.value, 1f);
		}
		
		protected override float getTickRate() {
			return 0.5F;
		}
		
		public override float getBaseEnergyStorageCapacityBonus() {
			return 0;//100;
		}
		
		protected override void updateEntity(float seconds) {
			if (!render) {
				render = gameObject.GetComponentInChildren<Renderer>();
			}
			if (!thermalComponent)
				thermalComponent = GetComponent<ThermalPlant>();
			thermalComponent.enabled = false;
			thermalComponent.CancelInvoke();
					
			if (powerSource && getBuildable().constructed && DayNightCycle.main.timePassedAsFloat-lastSound >= 6.2F) {
				lastSound = DayNightCycle.main.timePassedAsFloat;
				SoundManager.playSoundAt(workingSound, transform.position);
			}
			/*
			if (!cableObject) {
				cableObject = tryFindCable();
			}*/
		}
		
		private void tryFindCable() {
			powerSource = null;
			if (Vector3.Distance(transform.position, drfLocation) <= 200) {
				return; //those cables are dead
			}
			powerSource = WorldUtil.areAnyObjectsNear(transform.position, 4, isValidCable);
			setEmissiveStates((bool)powerSource);
		}
		
		private bool isValidCable(GameObject go) {
			PrecursorTeleporter pt = go.GetComponent<PrecursorTeleporter>();
			if (pt)
				return pt.isOpen;
			PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
			return pi && validObjects.Contains(pi.classId);
		}
		
		private void AddPower() {
			if (powerSource && this.getBuildable().constructed) {
				//float trash = 0f;
				//thermalComponent.powerSource.AddEnergy(25, out trash);
				SubRoot sub = getSub();
				if (sub) {
					float trash = 0f;
					sub.powerRelay.AddEnergy(AqueousEngineeringMod.config.getInt(AEConfig.ConfigEntries.ATPTAPRATE), out trash);
				}
			}
		}
		
		private void setEmissiveStates(bool working) {
			if (!render)
				return;
			Color c = working ? Color.green : Color.red;
			render.materials[0].SetColor("_GlowColor", c);
			thermalComponent.temperatureText.text = working ? "\u2713" : "\u26A0";
			thermalComponent.temperatureText.color = c;
			thermalComponent.temperatureText.transform.localScale = Vector3.one*2.5F;
			
		}
	}
}
