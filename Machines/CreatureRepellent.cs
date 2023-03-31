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
	
	public class BaseCreatureRepellent : CustomMachine<BaseCreatureRepellentLogic> {
		
		internal static readonly float POWER_COST = 0.25F; //per second
		internal static readonly float POWER_COST_ACTIVE = 2.0F; //per second
		internal static readonly float RANGE = 50F; //m
		internal static readonly float RANGE_INNER = 20F; //m
		
		internal static readonly Color electricalColor = new Color(0.2f, 0.9f, 1f, 1f);
		
		internal static readonly Dictionary<TechType, CreatureEffectivity> effectivityMap = new Dictionary<TechType, CreatureEffectivity>();
		internal static float maxRangeFactor = -1;
		
		static BaseCreatureRepellent() {
			skipCreature(TechType.Peeper);
			skipCreature(TechType.Oculus);
			skipCreature(TechType.Reginald);
			skipCreature(TechType.Hoopfish);
			skipCreature(TechType.Spinefish);
			skipCreature(TechType.Eyeye);
			skipCreature(TechType.Hoverfish);
			skipCreature(TechType.Boomerang);
			skipCreature(TechType.LavaBoomerang);
			skipCreature(TechType.LavaEyeye);
			skipCreature(TechType.Jumper);
			skipCreature(TechType.RabbitRay);
			skipCreature(TechType.GarryFish);
			skipCreature(TechType.Bladderfish);
			skipCreature(TechType.Spadefish);
			skipCreature(TechType.Rockgrub);
			skipCreature(TechType.HoleFish);
			skipCreature(TechType.Floater);
			skipCreature(TechType.Cutefish);
			skipCreature(TechType.Skyray);
			skipCreature(TechType.Shuttlebug);
			
			addCreatureType(TechType.Gasopod, 0.5F, 0.5F, 0.75F);
			addCreatureType(TechType.Shocker, 1, 0.5F, 0F);
			addCreatureType(TechType.LavaLarva, 2, -1F, 0F);
			
			addCreatureType(TechType.Bleeder, 0.5F, 1F, 1F);
			
			addCreatureType(TechType.Jellyray, 1, 0.25F, 0.25F);
			addCreatureType(TechType.GhostRayBlue, 1, 0.25F, 0.25F);
			addCreatureType(TechType.GhostRayRed, 1, 0.25F, 0.25F);
			
			addCreatureType(TechType.GhostLeviathan, 2, 0.5F, 0.25F);
			addCreatureType(TechType.GhostLeviathanJuvenile, 1.5F, 0.5F, 0.25F);
			addCreatureType(TechType.ReaperLeviathan, 2, 0.67F, 1F);
			addCreatureType(TechType.SeaDragon, 2, 0.33F, 0.5F);
			
			skipCreature(TechType.SeaTreader);
		}
		
		private static void skipCreature(TechType c) {
			addCreatureType(c, 0, 0, 0);
		}
		
		private static void addCreatureType(TechType c, float r, float e, float d) {
			effectivityMap[c] = new CreatureEffectivity(c, r, e, d);
			maxRangeFactor = Mathf.Max(maxRangeFactor, r);
		}
		
		private static readonly Vector3[] FIN_POSITIONS = new Vector3[]{
			new Vector3(0, 0.25F, 0.7F),
			new Vector3(0, 0.25F, -0.7F),
			new Vector3(0.12F, 0.1F, 0.7F),
			new Vector3(0.12F, 0.1F, -0.7F),
			new Vector3(-0.12F, 0.1F, 0.7F),
			new Vector3(-0.12F, 0.1F, -0.7F),
		};
		private static readonly Vector3[] FIN_ROTATIONS = new Vector3[]{
			new Vector3(-90, 90, 0),
			new Vector3(0, 90, 0),
			new Vector3(180, 90, 0),
		};
		
		public BaseCreatureRepellent(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
			addIngredient(TechType.Polyaniline, 1);
			addIngredient(TechType.AdvancedWiringKit, 1);
			addIngredient(TechType.PowerCell, 2);
			
			glowIntensity = 1;
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
			ObjectUtil.removeComponent<Bench>(go);
						
			BaseCreatureRepellentLogic lgc = go.GetComponent<BaseCreatureRepellentLogic>();
			
			go.GetComponent<Constructable>().model = ObjectUtil.getChildObject(go, "bench");
			
			go.transform.localScale = new Vector3(0.4F, 0.5F, 2);
			
			string name = "FinHolder";
			GameObject child = ObjectUtil.getChildObject(go, name);
			if (child == null) {
				child = new GameObject(name);
				child.transform.SetParent(go.transform);
			}
			PrefabIdentifier[] pi = child.GetComponentsInChildren<PrefabIdentifier>();
			for (int i = pi.Length; i < 6; i++) {
				GameObject fin = ObjectUtil.createWorldObject("cf1df719-905c-4385-98da-b638fdfd53f7");
				RenderUtil.convertToModel(fin);
				fin.transform.SetParent(child.transform);
				fin.transform.localScale = new Vector3(0.8F, 0.5F, 0.4F);
				fin.transform.localPosition = FIN_POSITIONS[i];
				Vector3 vec = FIN_ROTATIONS[i/2];
				fin.transform.localRotation = Quaternion.Euler(vec.x, vec.y, vec.z);
				fin.GetComponentInChildren<Collider>().gameObject.EnsureComponent<RepellentFin>();
				Renderer r0 = fin.GetComponentInChildren<Renderer>();
				RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, r0, "Textures/Machines/RepellentFin");
				foreach (Material m in r0.materials) {
					m.EnableKeyword("FX_BUILDING");
					//material2.SetTexture(ShaderPropertyID._EmissiveTex, this._EmissiveTex);
					//m.SetFloat(ShaderPropertyID._Cutoff, 0.5F);
					m.SetColor(ShaderPropertyID._BorderColor, electricalColor);
					m.SetVector(ShaderPropertyID._BuildParams, new Vector4(1f, 1f, 1.25f, -0.85f)); //last arg is speed, +ve is down
					m.SetFloat(ShaderPropertyID._NoiseStr, 0.8f);
					m.SetFloat(ShaderPropertyID._NoiseThickness, 0.05f);
					m.SetFloat(ShaderPropertyID._BuildLinear, 0.25f);
					m.SetFloat(ShaderPropertyID._MyCullVariable, 0f);
				}
			}
			
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
		
	}
	
	class CreatureEffectivity {
		
		internal readonly TechType creatureType;
		internal readonly float rangeFactor;
		internal readonly float effectivity;
		internal readonly float damageFactor;
		
		internal CreatureEffectivity(TechType c, float r, float e, float d) {
			creatureType = c;
			rangeFactor = r;
			effectivity = e;
			damageFactor = d;
		}
		
	}
	
	class RepellentFin : MonoBehaviour {
		
		private BaseCreatureRepellentLogic root;
		private Rigidbody body;
		private Renderer render;
		
		void Update() {
			if (!root) {
				root = gameObject.FindAncestor<BaseCreatureRepellentLogic>();
			}
			if (!body) {
				body = gameObject.FindAncestor<Rigidbody>();
			}
			if (!render) {
				render = transform.parent.parent.GetComponentInChildren<Renderer>();
			}
			//body.WakeUp();
			render.materials[0].SetColor(ShaderPropertyID._BorderColor, Color.Lerp(Color.black, BaseCreatureRepellent.electricalColor, root.getIntensity()));
		}
		
		void OnCollisionStay(Collision c) {
			SNUtil.writeToChat(""+c.collider);
			if (root && root.isFunctional() && c.collider && (c.collider.gameObject.FindAncestor<Creature>() || c.collider.gameObject.FindAncestor<Player>())) {
				LiveMixin live = c.gameObject.FindAncestor<LiveMixin>();
				if (live)
					live.TakeDamage(8*Time.deltaTime, c.contacts[0].point, DamageType.Electrical, gameObject);
			}
		}
		
	}
		
	public class BaseCreatureRepellentLogic : CustomMachineLogic {
		
		private float cooldown;
		private float lastTickTime;
		
		void Start() {
			SNUtil.log("Reinitializing base repellent");
			AqueousEngineeringMod.repellentBlock.initializeMachine(gameObject);
		}
		
		internal float getIntensity() {
			float time = DayNightCycle.main.timePassedAsFloat;
			float dT = time-lastTickTime;
			return (float)MathUtil.linterpolate(dT, 0.5, 2, 1, 0, true);
		}
		
		public bool isFunctional() {
			return cooldown <= 0.01F && DayNightCycle.main.timePassedAsFloat-lastTickTime <= 0.5F;
		}
		
		protected override void updateEntity(float seconds) {
			//if (mainRenderer == null)
			//	mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
						
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (Vector3.Distance(Player.main.transform.position, transform.position) >= BaseCreatureRepellent.RANGE*4)
				return;
			if (cooldown > 0) {
				cooldown -= seconds;
				return;
			}
			if (consumePower(BaseCreatureRepellent.POWER_COST*seconds)) {
				lastTickTime = DayNightCycle.main.timePassedAsFloat;
				bool flag = false;
				HashSet<Creature> set = WorldUtil.getObjectsNearWithComponent<Creature>(gameObject.transform.position, BaseCreatureRepellent.RANGE*BaseCreatureRepellent.maxRangeFactor);
				foreach (Creature c in set) {
					if (c && c.friend != Player.main.gameObject && (c.Aggression.Value > 0 || c.GetComponent<AggressiveWhenSeeTarget>())) {
						if (c is GhostLeviatanVoid)
							continue;
						float r = BaseCreatureRepellent.RANGE;
						float r0 = BaseCreatureRepellent.RANGE_INNER;
						CreatureEffectivity ce = null;
						TechType tt = CraftData.GetTechType(c.gameObject);
						if (tt != TechType.None && BaseCreatureRepellent.effectivityMap.ContainsKey(tt)) {
							ce = BaseCreatureRepellent.effectivityMap[tt];
							r *= ce.rangeFactor;
							r0 *= ce.rangeFactor;
						}
						float dd = Vector3.Distance(c.transform.position, transform.position);
						float f = dd <= r0 ? 0.3F : 0.15F;
						if (dd >= r)
							continue;
						//SNUtil.writeToChat(c+" @ "+c.transform.position+" D="+dd+" > "+c.Scared.Value);
						c.flinch = 1;
						f *= 2;
						if (ce != null)
							f *= ce.effectivity;
						c.Scared.Add(f*seconds);
						c.Aggression.Add(-f*seconds*0.4F);
						flag = true;
						if (c.Scared.Value > 0.5F && dd < r*0.5F) {
							Vector3 vec = transform.position+((c.transform.position-transform.position)*3);
							c.GetComponent<SwimBehaviour>().SwimTo(vec, 20*f);
						}
						if (dd <= 4F && c.liveMixin) {
							float dmg = ce != null ? ce.damageFactor : 1;
							c.liveMixin.TakeDamage(3*seconds*dmg, c.transform.position, DamageType.Electrical, gameObject);
						}
					}
				}
				if (Player.main.IsSwimming()) {
					float ddp = Vector3.Distance(Player.main.transform.position, transform.position);
					if (ddp <= 2.5F) {
						Player.main.liveMixin.TakeDamage(8*seconds, Player.main.transform.position, DamageType.Electrical, gameObject);
					}
				}
				if (flag) {
					if (!consumePower((BaseCreatureRepellent.POWER_COST_ACTIVE-BaseCreatureRepellent.POWER_COST)*seconds))
						cooldown = 5;
				}
			}
		}	
	}
}
