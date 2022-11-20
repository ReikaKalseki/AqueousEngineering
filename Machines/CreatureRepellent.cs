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
		
		public BaseCreatureRepellent(XMLLocale.LocaleEntry e) : base("basecreaturerepel", e.name, e.desc, "4cb154ef-bdb6-4ff4-9107-f378ce21a9b7") {
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
				fin.EnsureComponent<RepellentFin>();
				fin.transform.SetParent(child.transform);
				fin.transform.localScale = new Vector3(0.8F, 0.5F, 0.4F);
				fin.transform.localPosition = FIN_POSITIONS[i];
				Vector3 vec = FIN_ROTATIONS[i/2];
				fin.transform.localRotation = Quaternion.Euler(vec.x, vec.y, vec.z);
				Renderer r0 = fin.GetComponentInChildren<Renderer>();
				RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, r0, "Textures/Machines/RepellentFin");
				foreach (Material m in r0.materials) {
					m.EnableKeyword("FX_BUILDING");
					//material2.SetTexture(ShaderPropertyID._EmissiveTex, this._EmissiveTex);
					//m.SetFloat(ShaderPropertyID._Cutoff, 0.5F);
					m.SetColor(ShaderPropertyID._BorderColor, new Color(0.2f, 0.9f, 1f, 1f));
					m.SetVector(ShaderPropertyID._BuildParams, new Vector4(1f, 1f, 1.25f, -0.85f)); //last arg is speed, +ve is down
					m.SetFloat(ShaderPropertyID._NoiseStr, 0.08f);
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
	
	class RepellentFin : MonoBehaviour {
		
	}
		
	public class BaseCreatureRepellentLogic : CustomMachineLogic {
		
		private float cooldown;
		
		void Start() {
			SNUtil.log("Reinitializing base repellent");
			AqueousEngineeringMod.repellentBlock.initializeMachine(gameObject);
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
			if (consumePower(BaseCreatureRepellent.POWER_COST, seconds)) {
				float r0 = BaseCreatureRepellent.RANGE*2;
				float r = BaseCreatureRepellent.RANGE;
				bool flag = false;
				HashSet<Creature> set = WorldUtil.getObjectsNearWithComponent<Creature>(gameObject.transform.position, r);
				foreach (Creature c in set) {
					if (c && c.friend != Player.main.gameObject && (c.Aggression.Value > 0 || c.GetComponent<AggressiveWhenSeeTarget>())) {
						if (c is GhostLeviatanVoid)
							continue;
						float dd = Vector3.Distance(c.transform.position, transform.position);
						float f = dd <= BaseCreatureRepellent.RANGE_INNER ? 0.3F : 0.15F;
						if (c is GhostLeviathan || c is ReaperLeviathan || c is SeaDragon) {
							r = r0;
							f *= 0.5F;
						}
						if (dd >= r)
							continue;
						//SNUtil.writeToChat(c+" @ "+c.transform.position+" D="+dd+" > "+c.Scared.Value);
						c.flinch = 1;
						c.Scared.Add(f*seconds);
						c.Aggression.Add(-f*seconds*0.2F);
						flag = true;
						if (c.Scared.Value > 0.5F && dd < r*0.5F) {
							Vector3 vec = transform.position+((c.transform.position-transform.position)*3);
							c.GetComponent<SwimBehaviour>().SwimTo(vec, 20*f);
						}
					}
				}
				if (flag) {
					if (!consumePower(BaseCreatureRepellent.POWER_COST_ACTIVE-BaseCreatureRepellent.POWER_COST, seconds))
						cooldown = 5;
				}
			}
		}	
	}
}
