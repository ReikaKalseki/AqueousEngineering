using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {

	public class BaseDrillableGrinder : CustomMachine<BaseDrillableGrinderLogic> {

		internal static readonly float POWER_COST = 0.1F; //per second
		internal static readonly float POWER_COST_ACTIVE = 25F; //per second

		public static event Action<DrillableGrindingResult> onDrillableGrindEvent;

		private static bool generatedUncraftingList;
		internal static readonly Dictionary<TechType, UncraftingRecipe> uncraftingList = new Dictionary<TechType, UncraftingRecipe>();
		public static readonly Dictionary<TechType, float> uncraftingIngredientRatios = new Dictionary<TechType, float>();
		public static readonly HashSet<TechCategory> uncraftableCategories = new HashSet<TechCategory>();
		public static readonly Dictionary<TechType, bool> uncraftabilityFlags = new Dictionary<TechType, bool>();

		static BaseDrillableGrinder() {
			uncraftableCategories.Add(TechCategory.BasicMaterials);
			uncraftableCategories.Add(TechCategory.AdvancedMaterials);
			uncraftableCategories.Add(TechCategory.Electronics);
			uncraftableCategories.Add(TechCategory.Equipment);
			uncraftableCategories.Add(TechCategory.Tools);
			uncraftableCategories.Add(TechCategory.VehicleUpgrades);
			uncraftableCategories.Add(TechCategory.CyclopsUpgrades);
			uncraftableCategories.Add(TechCategory.MapRoomUpgrades);
			uncraftableCategories.Add(TechCategory.Workbench);

			uncraftingIngredientRatios[TechType.Titanium] = 1;
			uncraftingIngredientRatios[TechType.Copper] = 1;
			uncraftingIngredientRatios[TechType.Gold] = 1;
			uncraftingIngredientRatios[TechType.Silver] = 1;
			uncraftingIngredientRatios[TechType.Lead] = 1;
			uncraftingIngredientRatios[TechType.Diamond] = 1;
			uncraftingIngredientRatios[TechType.Lithium] = 1;
			uncraftingIngredientRatios[TechType.Magnetite] = 1;
			uncraftingIngredientRatios[TechType.UraniniteCrystal] = 1;
			uncraftingIngredientRatios[TechType.Sulphur] = 1;
			uncraftingIngredientRatios[TechType.CrashPowder] = 1;
			uncraftingIngredientRatios[TechType.Salt] = 1;
			uncraftingIngredientRatios[TechType.Kyanite] = 1;
			uncraftingIngredientRatios[TechType.Nickel] = 1;
			uncraftingIngredientRatios[TechType.Quartz] = 1;
			uncraftingIngredientRatios[TechType.AluminumOxide] = 1;
			uncraftingIngredientRatios[TechType.PrecursorIonCrystal] = 1;
			uncraftingIngredientRatios[TechType.MercuryOre] = 1;

			uncraftingIngredientRatios[TechType.Battery] = 0;
			uncraftingIngredientRatios[TechType.PrecursorIonBattery] = 0;

			uncraftabilityFlags[TechType.Titanium] = false;
			uncraftabilityFlags[TechType.FiberMesh] = false;
			uncraftabilityFlags[TechType.Lubricant] = false;
			uncraftabilityFlags[TechType.Silicone] = false;
			uncraftabilityFlags[TechType.Bleach] = false;
			uncraftabilityFlags[TechType.Benzene] = false;
			uncraftabilityFlags[TechType.HydrochloricAcid] = false;
			uncraftabilityFlags[TechType.Polyaniline] = false;
			/*
			uncraftabilityFlags[TechType.PrecursorKey_Purple] = false;
			uncraftabilityFlags[TechType.PrecursorKey_Orange] = false;
			uncraftabilityFlags[TechType.PrecursorKey_Blue] = false;
			uncraftabilityFlags[TechType.PrecursorKey_White] = false;
			uncraftabilityFlags[TechType.PrecursorKey_Red] = false;
			*/
			uncraftabilityFlags[TechType.HatchingEnzymes] = false;

			//deprecated stuff
			uncraftabilityFlags[TechType.PowerGlide] = false;
			uncraftabilityFlags[TechType.LithiumIonBattery] = false;
			uncraftabilityFlags[TechType.Transfuser] = false;
		}

		public BaseDrillableGrinder(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "51eba507-317c-46bf-adde-4459dc8e002e") {
			this.addIngredient(TechType.PlasteelIngot, 1);
			this.addIngredient(TechType.WiringKit, 1);
			this.addIngredient(TechType.Diamond, 4);
		}

		public static void buildUncraftingList() {
			if (generatedUncraftingList)
				return;
			foreach (KeyValuePair<TechType, CraftData.TechData> rec in CraftData.techData) {
				if (uncraftingList.ContainsKey(rec.Key)) //do not overwrite existing recipe
					continue;
				if (uncraftabilityFlags.ContainsKey(rec.Key) && !uncraftabilityFlags[rec.Key])
					continue;
				if (rec.Value.linkedItemCount > 0)
					continue;
				RecipeUtil.getRecipeCategory(rec.Key, out TechGroup grp, out TechCategory cat);
				if (uncraftableCategories.Contains(cat)) {
					registerUncrafting(UncraftingRecipe.createBasicUncrafting(rec.Key, rec.Value));
				}
			}
			foreach (UncraftingRecipe r in uncraftingList.Values) {
				r.buildRecursiveYields();
			}
			generatedUncraftingList = true;
		}

		public static void registerUncrafting(UncraftingRecipe r) {
			uncraftingList[r.item] = r;
		}

		public static bool canUncraft(GameObject go, ref Pickupable pp) {
			pp = go.FindAncestor<Pickupable>();
			return pp && canUncraft(pp.GetTechType());
		}

		public static bool canUncraft(TechType tt) {
			buildUncraftingList();
			return tt != TechType.None && uncraftingList.ContainsKey(tt);
		}

		public class UncraftingRecipe {

			public readonly TechType item;

			public readonly Dictionary<TechType, float> directYields = new Dictionary<TechType, float>();
			public readonly Dictionary<TechType, float> yields = new Dictionary<TechType, float>();

			private bool isFinalized;

			public UncraftingRecipe(TechType tt) {
				item = tt;
			}

			internal void buildRecursiveYields() {
				if (isFinalized)
					return;
				//SNUtil.log("Computing final uncrafting recipe for "+item+" from "+directYields.toDebugString());
				foreach (KeyValuePair<TechType, float> kvp in directYields) {
					//SNUtil.log("Found recipe for "+kvp.Key.AsString()+": "+BaseDrillableGrinder.uncraftingList.ContainsKey(kvp.Key));
					if (BaseDrillableGrinder.uncraftingList.ContainsKey(kvp.Key)) {
						UncraftingRecipe rec = BaseDrillableGrinder.uncraftingList[kvp.Key];
						if (!rec.isFinalized) {
							rec.buildRecursiveYields();
						}
						foreach (KeyValuePair<TechType, float> kvp2 in rec.yields) {
							this.addYield(kvp2.Key, kvp2.Value * kvp.Value);
						}
					}
					else {
						this.addYield(kvp.Key, kvp.Value);
					}
				}
				isFinalized = true;
				SNUtil.log("Finalized uncrafting recipe " + this);
			}

			private void addYield(TechType tt, float amt) {
				if (amt > 0.01F)
					yields[tt] = (yields.ContainsKey(tt) ? yields[tt] : 0) + amt;
			}

			public static UncraftingRecipe createBasicUncrafting(TechType tt, TechData td) {
				UncraftingRecipe r = new UncraftingRecipe(tt);
				foreach (Ingredient i in td.Ingredients) {
					r.directYields[i.techType] = getDefaultYield(i.techType, i.amount);
				}
				return r;
			}

			public static UncraftingRecipe createBasicUncrafting(TechType tt, CraftData.TechData td) {
				UncraftingRecipe r = new UncraftingRecipe(tt);
				foreach (CraftData.Ingredient i in td._ingredients) {
					r.directYields[i.techType] = getDefaultYield(i.techType, i.amount);
				}
				return r;
			}

			private static float getDefaultYield(TechType tt, int amt) {
				float ratio = 0.9F;
				RecipeUtil.getRecipeCategory(tt, out TechGroup grp, out TechCategory cat);
				if (uncraftingIngredientRatios.ContainsKey(tt))
					ratio = uncraftingIngredientRatios[tt];
				else if (cat == TechCategory.AdvancedMaterials)
					ratio = 0.8F;
				else if (cat == TechCategory.Electronics)
					ratio = 0.5F;
				return amt * ratio;
			}

			public override string ToString() {
				return string.Format("Uncrafting of {0}, Yields={1} via {2}", item, yields.toDebugString(), directYields.toDebugString());
			}


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
			go.removeComponent<VendingMachine>();

			BaseDrillableGrinderLogic lgc = go.GetComponent<BaseDrillableGrinderLogic>();
			go.removeChildObject("collisions/handTarget");
			go.removeChildObject("Vending_machine/vending_machine_glass");
			go.removeChildObject("Vending_machine/vending_machine_snacks");

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

		private float shutdownCooldown;

		private readonly List<GameObject> drills = new List<GameObject>();

		internal static readonly SoundManager.SoundData workingSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "crusher", "Sounds/crusher.ogg", SoundManager.soundMode3D);
		internal static readonly SoundManager.SoundData jammedSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "crusherjam", "Sounds/crusherjam.ogg", SoundManager.soundMode3D);

		void Start() {
			SNUtil.log("Reinitializing base drillable grinder");
			AqueousEngineeringMod.grinderBlock.initializeMachine(gameObject);
		}

		public override void onStasisHit(StasisSphere s) {
			this.freeze(s.getLifespan() + 0.5F);
		}

		public void freeze(float duration) {
			if (shutdownCooldown <= 0) {
				shutdownCooldown = duration;
				SNUtil.writeToChat("Freezing " + this + " for " + duration);
			}
		}

		protected override void save(System.Xml.XmlElement data) {

		}

		protected override void updateEntity(float seconds) {
			isReady = !GameModeUtils.RequiresPower() || this.consumePower((isGrinding ? BaseDrillableGrinder.POWER_COST_ACTIVE : BaseDrillableGrinder.POWER_COST) * seconds);
			if (!isReady) {
				this.freeze(2);
			}
			isGrinding = false;

			shutdownCooldown -= seconds;

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
					GameObject turbine = UnityEngine.Object.Instantiate(air.getChildObject("model/_pipes_floating_air_intake_turbine_geo"));
					turbine.transform.SetParent(transform);
					turbine.transform.localScale = new Vector3(0.7F, 10F, 0.7F);
					turbine.transform.localPosition = new Vector3(1.35F, (i * 0.31F) + 1.23F, 0.77F);
					turbine.transform.localRotation = Quaternion.Euler(0, 0, 90);
					drills.Add(turbine);
				}
			}
		}

		internal void OnTriggerStay(Collider other) {
			if (!other.isTrigger && isReady && shutdownCooldown <= 0) {
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
					if (tt == TechType.Kyanite || aoe.center.y + transform.position.y >= 0.1F) {
						//too hard : make grinding noises and sparks and such
						sound = jammedSound;
						loopTime = 1.6805F;
					}
					else {
						this.drill(d, tt);
					}
					ReactsOnDrilled r = d.GetComponent<ReactsOnDrilled>();
					if (r != null)
						r.onDrilled(aoe.getWorldCenter());
				}
				else {
					CustomGrindable cg = other.gameObject.FindAncestor<CustomGrindable>();
					Pickupable pp = null;
					if (cg) {
						isGrinding = true;
						bool flag = cg.isRecyclable();
						if (flag) {
							int n = UnityEngine.Random.Range(cg.numberToYieldMin, cg.numberToYieldMax + 1);
							for (int i = 0; i < n; i++) {
								//CustomGrindableResult cres = new CustomGrindableResult(cg);
								//cg.resourceChoice.Invoke(cres);
								//GameObject drop = cres.drop;
								GameObject drop = cg.chooseRandomResource();//.Invoke(cg);
								if (drop) {
									DrillableGrindingResult res = new DrillableGrindingResult(this, cg.techType, cg, drop);
									this.doDrop(res);
								}
								else {
									//SNUtil.writeToChat("Custom Grindable resulted in null drop");
								}
							}
						}
						ReactsOnDrilled r = cg.GetComponent<ReactsOnDrilled>();
						if (r != null)
							r.onDrilled(aoe.getWorldCenter());
						if (flag)
							cg.gameObject.destroy(false);
					}
					else if (BaseDrillableGrinder.canUncraft(other.gameObject, ref pp)) {
						TechType tt = pp.GetTechType();
						isGrinding = true;
						BaseDrillableGrinder.UncraftingRecipe r = BaseDrillableGrinder.uncraftingList[tt];
						foreach (Pickupable pp2 in pp.GetComponentsInChildren<Pickupable>()) { //child items
							if (pp2 && pp2 != pp) {
								pp2.Drop();
							}
						}
						foreach (KeyValuePair<TechType, float> kvp in r.yields) {
							GameObject drop = ObjectUtil.lookupPrefab(kvp.Key);
							if (drop) {
								DrillableGrindingResult res = new DrillableGrindingResult(this, tt, pp, drop);
								int n = (int)kvp.Value;
								if (kvp.Value > n && UnityEngine.Random.Range(0F, 1F) <= kvp.Value - n)
									n++;
								res.dropCount = n;
								this.doDrop(res);
							}
						}
						pp.gameObject.destroy(false);
					}
					else {
						Player p = other.gameObject.FindAncestor<Player>();
						if (p && p.IsSwimming()) {
							isGrinding = true;
							p.liveMixin.TakeDamage(20 * Time.deltaTime, p.transform.position, DamageType.Drill, gameObject);
						}
						SeaMoth v = other.gameObject.FindAncestor<SeaMoth>();
						if (v) {
							isGrinding = true;
							v.liveMixin.TakeDamage(10 * Time.deltaTime, v.transform.position, DamageType.Drill, gameObject);
							sound = jammedSound;
							loopTime = 1.6805F;
						}
					}
				}

				if (isGrinding) {
					float dT = Time.deltaTime;
					foreach (GameObject dd in drills) {
						dd.transform.Rotate(new Vector3(0, dT * 100, 0), Space.Self);
					}

					if (DayNightCycle.main.timePassedAsFloat - lastSound >= loopTime - 0.1F) {
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
								this.doDrop(res);
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
					use.transform.position = aoe.transform.position + (Vector3.down * 0.7F) + (transform.forward * 0.25F);
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

		public virtual bool isRecyclable() {
			return true;
		}

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
