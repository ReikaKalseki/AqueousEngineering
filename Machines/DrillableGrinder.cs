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
	
	public class BaseDrillableGrinder : CustomMachine<BaseDrillableGrinderLogic> {
		
		internal static readonly float POWER_COST = 0.1F; //per second
		internal static readonly float POWER_COST_ACTIVE = 25F; //per second
		
		public static event Action<DrillableGrindingResult> onDrillableGrindEvent;
		
		static BaseDrillableGrinder() {
			
		}
		
		public BaseDrillableGrinder(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "51eba507-317c-46bf-adde-4459dc8e002e") {
			addIngredient(TechType.PlasteelIngot, 1);
			addIngredient(TechType.WiringKit, 1);
			addIngredient(TechType.Diamond, 4);
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
			ObjectUtil.removeComponent<VendingMachine>(go);
						
			BaseDrillableGrinderLogic lgc = go.GetComponent<BaseDrillableGrinderLogic>();
			ObjectUtil.removeChildObject(go, "collisions/handTarget");
			ObjectUtil.removeChildObject(go, "Vending_machine/vending_machine_glass");
			ObjectUtil.removeChildObject(go, "Vending_machine/vending_machine_snacks");
			
			go.transform.localScale = new Vector3(1F, 1F, 1F);
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			r.transform.localScale = new Vector3(1, 2, 1);
			RenderUtil.swapToModdedTextures(r, this);
		}
		
		internal static void pingEvent(DrillableGrindingResult e) {
			if (onDrillableGrindEvent != null)
				onDrillableGrindEvent.Invoke(e);
		}
		
	}
		
	public class BaseDrillableGrinderLogic : CustomMachineLogic {
		
		private bool isReady;
		private bool isGrinding;
		
		private BoxCollider aoe;
		
		private float lastSound = -1;
		
		private readonly List<GameObject> drills = new List<GameObject>();
		
		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "crusher", "Sounds/crusher.ogg", SoundManager.soundMode3D);
		internal static readonly SoundManager.SoundData jammedSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "crusherjam", "Sounds/crusherjam.ogg", SoundManager.soundMode3D);
		
		void Start() {
			SNUtil.log("Reinitializing base drillable grinder");
			AqueousEngineeringMod.grinderBlock.initializeMachine(gameObject);
		}
		
		protected override void save(System.Xml.XmlElement data) {
			
		}
		
		protected override void updateEntity(float seconds) {
			isReady = !GameModeUtils.RequiresPower() || consumePower((isGrinding ? BaseDrillableGrinder.POWER_COST_ACTIVE : BaseDrillableGrinder.POWER_COST)*seconds);
			isGrinding = false;
			
			if (!aoe) {
				GameObject go = new GameObject("AoEHolder");
				go.transform.SetParent(transform);
				aoe = go.EnsureComponent<BoxCollider>();
				go.EnsureComponent<BaseDrillableGrinderColliderTag>().machine = this;
			}
			aoe.transform.localPosition = new Vector3(0, 1.5F, 0.5F);
			aoe.transform.localRotation = Quaternion.identity;
			aoe.isTrigger = true;
			aoe.center = Vector3.zero;
			aoe.size = new Vector3(1.05F, 1.125F, 1.4F);
			
			if (drills.Count != 3) {
				drills.Clear();
				for (int i = 0; i < 3; i++) {
					GameObject air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
					GameObject turbine = UnityEngine.Object.Instantiate(ObjectUtil.getChildObject(air, "model/_pipes_floating_air_intake_turbine_geo"));
					turbine.transform.SetParent(transform);
					turbine.transform.localScale = new Vector3(0.7F, 10F, 0.7F);
					turbine.transform.localPosition = new Vector3(1.35F, i*0.31F+1.23F, 0.77F);
					turbine.transform.localRotation = Quaternion.Euler(0, 0, 90);
					drills.Add(turbine);
				}
			}
		}
		
	    internal void OnTriggerStay(Collider other) {
			if (!other.isTrigger && isReady) {
				Drillable d = other.gameObject.FindAncestor<Drillable>();
				//SNUtil.writeToChat("Drillable: "+d);
				if (d && d.resources.Length > 0) {
					SpecialDrillable s = other.gameObject.FindAncestor<SpecialDrillable>();
					if (s && !s.allowAutomatedGrinding())
						return;
					isGrinding = true;
					TechType tt = d.resources[0].techType;
					SoundManager.SoundData sound = workingSound;
					float loopTime = 3.25F;
					if (tt == TechType.Kyanite) {
						//too hard : make grinding noises and sparks and such
						sound = jammedSound;
						loopTime = 1.6805F;
					}
					else {/*
						GameObject hit;
						d.OnDrill(d.transform.position, null, out hit);
						foreach (GameObject drop in d.lootPinataObjects) {
							SNUtil.writeToChat("Drop: "+drop);
							if (drop) {
								drop.transform.position = Vector3.Lerp(d.transform.position, transform.position+aoe.center, 0.5F);
								DrillableGrindingResult res = new DrillableGrindingResult(this, tt, d, drop);
								BaseDrillableGrinder.pingEvent(res);
							}
						}
						d.lootPinataObjects.Clear();*/
						drill(d, tt);
					}
					if (DayNightCycle.main.timePassedAsFloat-lastSound >= loopTime-0.1F) {
						lastSound = DayNightCycle.main.timePassedAsFloat;
						SoundManager.playSoundAt(sound, transform.position);
					}
					float dT = Time.deltaTime;
					foreach (GameObject dd in drills) {
						dd.transform.Rotate(new Vector3(0, dT*100, 0), Space.Self);
					}
					if (d.GetComponent<ReactsOnDrilled>())
						d.gameObject.SendMessage("onDrilled");
				}
			}
	    }
		
		private void drill(Drillable d, TechType tt) {
			float num = 0f;
			for (int i = 0; i < d.health.Length; i++)
				num += d.health[i];
			Vector3 zero = Vector3.zero;
			int num2 = d.FindClosestMesh(aoe.transform.position, out zero);
			d.timeLastDrilled = Time.time;
			if (num > 0) {
				float num3 = d.health[num2];
				d.health[num2] = Mathf.Max(0f, d.health[num2] - 5f);
				num -= num3 - d.health[num2];
				if (num3 > 0f && d.health[num2] <= 0f) {
					d.renderers[num2].gameObject.SetActive(false);
					d.SpawnFX(d.breakFX, zero);
					if (UnityEngine.Random.value < d.kChanceToSpawnResources) {
						int amt = UnityEngine.Random.Range(d.minResourcesToSpawn, d.maxResourcesToSpawn);
						for (int i = 0; i < amt; i++) {
							GameObject drop = d.ChooseRandomResource();
							if (drop) {
								DrillableGrindingResult res = new DrillableGrindingResult(this, tt, d, drop);
								BaseDrillableGrinder.pingEvent(res);
								if (res.drop) {
									for (int a = 0; a < res.dropCount; a++) {
										GameObject use = UnityEngine.Object.Instantiate(res.drop);
										use.transform.position = aoe.transform.position+Vector3.down*0.7F+transform.forward*0.25F;
										use.transform.rotation = UnityEngine.Random.rotationUniform;
										use.GetComponent<Rigidbody>().isKinematic = false;
										use.GetComponent<Rigidbody>().AddForce(transform.forward.normalized*15);
									}
								}
							}
						}
					}
				}
				if (num <= 0f) {
					d.SpawnFX(d.breakAllFX, zero);
					if (d.deleteWhenDrilled) {
						float time = d.lootPinataOnSpawn ? 6f : 0f;
						d.Invoke("DestroySelf", time);
					}
				}
			}
		}
	}
	
	public class BaseDrillableGrinderColliderTag : MonoBehaviour {
		
		internal BaseDrillableGrinderLogic machine;
		
		void OnTriggerStay(Collider other) {
			machine.OnTriggerStay(other);
		}
		
	}
	
	public class DrillableGrindingResult {
		
		public readonly BaseDrillableGrinderLogic grinder;	
		public readonly TechType materialTech;			
		public readonly Drillable drillable;
		public readonly GameObject originalDrop;
		
		public GameObject drop;
		public int dropCount = 1;
		
		internal DrillableGrindingResult(BaseDrillableGrinderLogic lgc, TechType tt, Drillable d, GameObject go) {
			grinder = lgc;
			materialTech = tt;
			drillable = d;
			originalDrop = go;
			drop = originalDrop;
		}
		
	}
}
