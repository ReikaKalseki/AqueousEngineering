﻿using System;
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
			Vector3 sc = new Vector3(1, 2, 1);
			r.transform.localScale = sc;
			RenderUtil.swapToModdedTextures(r, this);
			
			foreach (Collider c in go.GetComponentsInChildren<Collider>())
				c.gameObject.transform.localScale = new Vector3(1, 1, sc.y);
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
		
		private float shutdownCooldown = -1;
		
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
			if (!isReady) {
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time >= shutdownCooldown) {
					shutdownCooldown = time+2;
				}
			}
			isGrinding = false;
			
			if (!aoe) {
				GameObject go = new GameObject("AoEHolder");
				go.transform.SetParent(transform);
				aoe = go.EnsureComponent<BoxCollider>();
				go.EnsureComponent<BaseDrillableGrinderColliderTag>().machine = this;
			}
			aoe.transform.localPosition = new Vector3(0, 1.5F, 0.75F);
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
			if (!other.isTrigger && isReady && shutdownCooldown < DayNightCycle.main.timePassedAsFloat) {
				SoundManager.SoundData sound = workingSound;
				float loopTime = 3.25F;
				Drillable d = other.gameObject.FindAncestor<Drillable>();
				//SNUtil.writeToChat("Drillable: "+d);
				if (d && d.resources.Length > 0) {
					SpecialDrillable s = other.gameObject.FindAncestor<SpecialDrillable>();
					if (s && !s.allowAutomatedGrinding())
						return;
					isGrinding = true;
					TechType tt = d.resources[0].techType;
					if (tt == TechType.Kyanite || aoe.center.y+transform.position.y >= 0.1F) {
						//too hard : make grinding noises and sparks and such
						sound = jammedSound;
						loopTime = 1.6805F;
					}
					else {
						drill(d, tt);
					}
					if (d.GetComponent<ReactsOnDrilled>())
						d.gameObject.SendMessage("onDrilled");
				}
				else {
					CustomGrindable cg = other.gameObject.FindAncestor<CustomGrindable>();
					if (cg) {
						isGrinding = true;
						int n = UnityEngine.Random.Range(cg.numberToYieldMin, cg.numberToYieldMax + 1);
						for (int i = 0; i < n; i++) {
							//CustomGrindableResult cres = new CustomGrindableResult(cg);
							//cg.resourceChoice.Invoke(cres);
							//GameObject drop = cres.drop;
							GameObject drop = cg.chooseRandomResource();//.Invoke(cg);
							if (drop) {
								DrillableGrindingResult res = new DrillableGrindingResult(this, cg.techType, cg, drop);
								doDrop(res);
							}
							else {
								//SNUtil.writeToChat("Custom Grindable resulted in null drop");
							}
						}
						if (cg.GetComponent<ReactsOnDrilled>())
							cg.gameObject.SendMessage("onDrilled");
						UnityEngine.Object.Destroy(cg.gameObject);
					}
					else {
						Player p = other.gameObject.FindAncestor<Player>();
						if (p && p.IsSwimming()) {
							isGrinding = true;
							p.liveMixin.TakeDamage(20*Time.deltaTime, p.transform.position, DamageType.Drill, gameObject);
						}
						SeaMoth v = other.gameObject.FindAncestor<SeaMoth>();
						if (v) {
							isGrinding = true;
							v.liveMixin.TakeDamage(10*Time.deltaTime, v.transform.position, DamageType.Drill, gameObject);
							sound = jammedSound;
							loopTime = 1.6805F;
						}
					}
				}
				
				if (isGrinding) {
					float dT = Time.deltaTime;
					foreach (GameObject dd in drills) {
						dd.transform.Rotate(new Vector3(0, dT*100, 0), Space.Self);
					}
					
					if (DayNightCycle.main.timePassedAsFloat-lastSound >= loopTime-0.1F) {
						lastSound = DayNightCycle.main.timePassedAsFloat;
						SoundManager.playSoundAt(sound, transform.position);
					}
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
								doDrop(res);
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
		
		private void doDrop(DrillableGrindingResult res) {
			//SNUtil.writeToChat("Rock Crusher dropping "+res);
			BaseDrillableGrinder.pingEvent(res);
			//SNUtil.writeToChat("Post-Event Drop: "+res.drop+" x"+res.dropCount);
			if (res.drop) {
				for (int a = 0; a < res.dropCount; a++) {
					GameObject use = UnityEngine.Object.Instantiate(res.drop);
					use.SetActive(true);
					use.transform.position = aoe.transform.position + Vector3.down * 0.7F + transform.forward * 0.25F;
					use.transform.rotation = UnityEngine.Random.rotationUniform;
					Rigidbody rb = use.GetComponent<Rigidbody>();
					rb.isKinematic = false;
					rb.AddForce(transform.forward.normalized * 12);
				}
			}
			else {
				//SNUtil.writeToChat("Rock Crusher received null drop after event");
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
		public readonly MonoBehaviour drillable;
		public readonly GameObject originalDrop;
		
		public GameObject drop;
		public int dropCount = 1;
		
		internal DrillableGrindingResult(BaseDrillableGrinderLogic lgc, TechType tt, MonoBehaviour d, GameObject go) {
			grinder = lgc;
			materialTech = tt;
			drillable = d;
			originalDrop = go;
			drop = originalDrop;
		}
		
		public override string ToString() {
			return string.Format("[DrillableGrindingResult Grinder={0}, MaterialTech={1}, Drillable={2}, OriginalDrop={3}, Drop={4}, DropCount={5}]", grinder.transform.position, materialTech.AsString(), drillable, originalDrop, drop, dropCount);
		}

		
	}
	/*
	public sealed class CustomTypesGrindable : CustomGrindable {
		
		public WeightedRandom<TechType> dropTable;
		
		public override GameObject chooseRandomResource() {
			return ObjectUtil.lookupPrefab(dropTable.getRandomEntry());
		}
		
	}
	
	public sealed class CustomTypeGrindable : CustomGrindable {
		
		public TechType dropType;
		
		public override GameObject chooseRandomResource() {
			return ObjectUtil.lookupPrefab(dropType);
		}
		
	}
	*/
	public abstract class CustomGrindable : MonoBehaviour {
		
		public TechType techType;
		public int numberToYieldMin = 1;
		public int numberToYieldMax = 1;
		//public Func<CustomGrindable, GameObject> chooseRandomResource = (cg) => {SNUtil.writeToChat("Unset drops for CustomGrindable "+cg.gameObject.name)+"!"; return null;};
		//public readonly UnityEngine.Events.UnityEvent<CustomGrindableResult> resourceChoice = new GrindableResultEvent(); //does not copy with listeners
		
		public abstract GameObject chooseRandomResource();
		
	}
	/*
	public class CustomGrindableResult {
		
		public readonly CustomGrindable grindable;
		public GameObject drop;
		
		internal CustomGrindableResult(CustomGrindable g) {
			grindable = g;
		}
		
	}*/
}
