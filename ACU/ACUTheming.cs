using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;

namespace ReikaKalseki.AqueousEngineering {
	
	public static class ACUTheming {
		
		internal static readonly string ACU_DECO_SLOT_NAME = "ACUDecoHolder";
		
		private static readonly Dictionary<BiomeRegions.RegionType, WeightedRandom<ACUPropDefinition>> propTypes = new Dictionary<BiomeRegions.RegionType, WeightedRandom<ACUPropDefinition>>();
	   
		private static readonly Dictionary<string, MaterialPropertyDefinition> terrainGrassTextures = new Dictionary<string, MaterialPropertyDefinition>();
		
		private static readonly string rootCachePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "GrassTex");
		
		static ACUTheming() {
			registerGrassProp(BiomeRegions.RegionType.Kelp, null, 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.RedGrass, "Coral_reef_red_seaweed_03", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.RedGrass, "Coral_reef_red_seaweed_02", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.Koosh, "Coral_reef_small_deco_03_billboards", 15, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.Koosh, "coral_reef_grass_03_02", 15, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.GrandReef, "coral_reef_grass_11_02_gr", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.GrandReef, "coral_reef_grass_07_gr", 25, 0.5F);
			//registerGrassProp(BiomeRegions.RegionType.GrandReef, "coral_reef_grass_10_gr", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.BloodKelp, "coral_reef_grass_07_bk", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.LostRiver, "coral_reef_grass_11_03_lr", 25, 0.5F);
			registerGrassProp(BiomeRegions.RegionType.LavaZone, "coral_reef_grass_10_lava", 25, 0.5F);
				
			//registerProp(BiomeRegions.RegionType.Koosh, "eb5ea858-930d-4272-91b5-e9ebe2286ca8", 25, 0.5F);
			
			//foreach (string pfb in VanillaFlora.BLOOD_GRASS.getPrefabs(false, true))
			//	registerProp(BiomeRegions.RegionType.RedGrass, pfb, 15);
			
			registerProp(BiomeRegions.RegionType.Mushroom, "961194a9-e88b-40d7-900d-a48c5b739352", 5, false, 0.4F);
			registerProp(BiomeRegions.RegionType.Mushroom, "fe145621-5b25-4000-a3dd-74c1aaa961e2", 5, false, 0.4F);
			registerProp(BiomeRegions.RegionType.Mushroom, "f3de21af-550b-4901-a6e8-e45e31c1509d", 5, false, 0.4F);
			registerProp(BiomeRegions.RegionType.Mushroom, "5086a02a-ea6d-41ba-90c3-ea74d97cf6b5", 5, false, 0.4F);
			registerProp(BiomeRegions.RegionType.Mushroom, "7c7e0e95-8311-4ee0-80dd-30a61b151161", 5, false, 0.4F);
			
			registerProp(BiomeRegions.RegionType.BloodKelp, "7bfe0629-a008-43b8-bd16-d69ad056769f", 15, true, prepareBloodTendril);
			registerProp(BiomeRegions.RegionType.BloodKelp, "e291d076-bf95-4cdd-9dd9-6acd37566cf6", 15, true, prepareBloodTendril);
			registerProp(BiomeRegions.RegionType.BloodKelp, "2bfcbaf4-1ae6-4628-9816-28a6a26ff340", 15, true, prepareBloodTendril);
			registerProp(BiomeRegions.RegionType.BloodKelp, "2ab96dc4-5201-4a41-aa5c-908f0a9a0da8", 15, true, prepareBloodTendril);
			registerProp(BiomeRegions.RegionType.BloodKelp, "18229b4b-3ed3-4b35-ae30-43b1c31a6d8d", 25, true, 0.4F, 0.165F); //blood oil
			/* too finicky
			foreach (string pfb in VanillaFlora.DEEP_MUSHROOM.getPrefabs(false, true)) {
				Action<GameObject> a = go => {
					go.transform.localScale = Vector3.one*0.33F;
					go.transform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(260F, 280F), UnityEngine.Random.Range(0F, 360F)*0, 0);
				};
				registerProp(BiomeRegions.RegionType.BloodKelp, pfb, 5, true, a);
				//registerProp(BiomeRegions.RegionType.LostRiver, pfb, 5, a); is a native flora here
				//registerProp(BiomeRegions.RegionType.LavaZone, pfb, 5, a); and here
			}*/
			
			foreach (string pfb in VanillaFlora.JELLYSHROOM_TINY.getPrefabs(true, true))
				registerProp(BiomeRegions.RegionType.Jellyshroom, pfb, 5, false);
			
			foreach (string pfb in VanillaFlora.TREE_LEECH.getPrefabs(false, true))
				registerProp(BiomeRegions.RegionType.Mushroom, pfb, 5, false, 0.25F);
			foreach (string pfb in VanillaFlora.GRUE_CLUSTER.getPrefabs(true, true))
				registerProp(BiomeRegions.RegionType.Mushroom, pfb, 5, false, 0.00004F); //why the hell is this thing so huge in native scale and vanilla scales it to 0.0001F
			
			registerProp(BiomeRegions.RegionType.LostRiver, VanillaFlora.BRINE_LILY.getRandomPrefab(false), 10, false, 0.25F);
			foreach (string pfb in VanillaFlora.CLAW_KELP.getPrefabs(true, true))
				registerProp(BiomeRegions.RegionType.LostRiver, pfb, 5, true, 0.1F, 0, go => go.transform.rotation = Quaternion.Euler(270, 0, 0));
			
			registerProp(BiomeRegions.RegionType.GrandReef, VanillaFlora.ANCHOR_POD_SMALL1.getRandomPrefab(false), 10, true, 0.1F);
			registerProp(BiomeRegions.RegionType.GrandReef, VanillaFlora.ANCHOR_POD_SMALL2.getRandomPrefab(false), 10, true, 0.1F);
			
			registerProp(BiomeRegions.RegionType.LavaZone, "077ebe13-eb45-4ee4-8f6f-f566cfe11ab2", 10, false, 0.5F);
			
			if (Directory.Exists(rootCachePath)) {
				foreach (string folder in Directory.EnumerateDirectories(rootCachePath)) {
					string name = Path.GetFileName(folder);
					try {
						SNUtil.log("Loading cached grass material '"+name+"' from "+folder);
						MaterialPropertyDefinition m = new MaterialPropertyDefinition(name);
						m.readFromFile(AqueousEngineeringMod.modDLL, folder);
						terrainGrassTextures[m.name] = m;
					}
					catch (Exception ex) {
						SNUtil.log("Could not load cached grass material '"+name+"': "+ex);
					}
				}
			}
			else {
				SNUtil.writeToChat("Grass material cache does not exist at "+rootCachePath+".");
				Directory.CreateDirectory(rootCachePath);
			}
		}
		
		public static void cacheGrassMaterial(Material m) {
			string n = m.mainTexture.name.Replace(" (Instance)", "");
			if (!terrainGrassTextures.ContainsKey(n)) {
				MaterialPropertyDefinition def = new MaterialPropertyDefinition(m);
				terrainGrassTextures[n] = def;
				string path = Path.Combine(rootCachePath, n);
				def.writeToFile(path);
				SNUtil.log("Saved grass material '"+n+"' to "+path);
			}
		}
		
		private static void prepareBloodTendril(GameObject go) {
			go.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.15F, 0.25F);
			go.transform.rotation = Quaternion.identity;
		}
		
		private static void registerGrassProp(BiomeRegions.RegionType r, string texture, double wt, float scale, float voff = 0) {
			Action<GameObject> a = go => {
			    go.transform.localScale = Vector3.one*UnityEngine.Random.Range(scale*0.95F, scale*1.05F);
				go.transform.position = go.transform.position+Vector3.up*voff;
				if (!string.IsNullOrEmpty(texture)) {
					Renderer rn = go.GetComponentInChildren<Renderer>();
					if (terrainGrassTextures.ContainsKey(texture))
						terrainGrassTextures[texture].applyToMaterial(rn.materials[0], true, false);//.mainTexture = RenderUtil.getVanillaTexture(texture);
					else
						UnityEngine.Object.DestroyImmediate(go);
				}
			};
			registerProp(r, "880b59b7-8fd6-412f-bbcb-a4260b263124", wt*0.75F, false, a);
			registerProp(r, "bac42c90-8995-439f-be2f-29a6d164c82a", wt*0.25F, false, a);
		}
		
		private static void registerProp(BiomeRegions.RegionType r, string s, double wt, bool up, float scale, float voff = 0, Action<GameObject> a = null) {
			registerProp(r, s, wt, up, go => {
			    go.transform.localScale = Vector3.one*UnityEngine.Random.Range(scale*0.95F, scale*1.05F);
				go.transform.position = go.transform.position+Vector3.up*voff;
				if (a != null)
					a(go);
			});
		}
		
		private static void registerProp(BiomeRegions.RegionType r, string s, double wt, bool up, Action<GameObject> a = null) {
			WeightedRandom<ACUPropDefinition> wr = propTypes.ContainsKey(r) ? propTypes[r] : new WeightedRandom<ACUPropDefinition>();
			wr.addEntry(new ACUPropDefinition(s, wt, up, a), wt);
			propTypes[r] = wr;
		}
		
		private static ACUPropDefinition getRandomACUProp(WaterPark acu, BiomeRegions.RegionType r) {
			return propTypes.ContainsKey(r) ? propTypes[r].getRandomEntry() : null;
		}
		
		internal static void updateACUTheming(ACUCallbackSystem.ACUCallback acu, BiomeRegions.RegionType theme, bool changed) {
			if (!acu.lowestSegment)
				return;
			//SNUtil.writeToChat(""+li.Count);
			//SNUtil.writeToChat("##"+theme+" > "+floor+" & "+glass+" & "+decoHolders.Count);
			foreach (Transform t in acu.lowestSegment.transform) {
				string n = t.gameObject.name;
				if (n.StartsWith("Coral_reef_shell_plates", StringComparison.InvariantCulture)) { //because is flat, skip it
					t.gameObject.SetActive(false);
				}
				else if (n.StartsWith("Coral_reef_small_deco", StringComparison.InvariantCulture)) {
					bool flag = true;
					if (acu.decoHolders.Count > 0) {
						foreach (GameObject slot in acu.decoHolders) {
							if (Vector3.Distance(slot.transform.position, t.position) <= 0.05F) {
								UnityEngine.Object.DestroyImmediate(t.gameObject);
								flag = false;
								break;
							}
						}
					}
					if (flag) {
						GameObject slot = new GameObject();
						slot.name = ACU_DECO_SLOT_NAME;
						slot.SetActive(true);
						slot.transform.parent = acu.lowestSegment.transform;
						slot.transform.position = t.position;
						slot.transform.rotation = t.rotation;
						//slot.transform.rotation = Quaternion.identity;
						addProp(t.gameObject, slot, BiomeRegions.RegionType.Shallows);
						acu.decoHolders.Add(slot);
					}
				}
			}
			foreach (GameObject slot in acu.decoHolders) {
				if (!slot)
					continue;
				bool found = false;
				foreach (Transform bt in slot.transform) {
					GameObject biomeSlot = bt.gameObject;
					bool match = biomeSlot.name == Enum.GetName(typeof(BiomeRegions.RegionType), theme);
					biomeSlot.SetActive(match);
					if (match) {
						found = true;
						if (bt.childCount == 0) {
							ACUPropDefinition def = getRandomACUProp(acu.acu, theme);
							//SNUtil.writeToChat("$$"+def);
							//SNUtil.log("$$"+def);
							if (def != null)
								addProp(def.spawn(), slot, theme, biomeSlot);
						}
					}
				}
				if (!found) {
					addProp(null, slot, theme);
				}
			}
			
			if (!changed)
				return;
				
			string floorTex = Enum.GetName(typeof(BiomeRegions.RegionType), theme);
			if (!string.IsNullOrEmpty(floorTex)) {
				Renderer r = acu.floor.GetComponentInChildren<Renderer>();
				Texture2D tex = TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ACUFloor/"+floorTex);
				if (tex)
					r.material.mainTexture = tex;
			}
			BiomeRegions.Biome b = BiomeRegions.getAttr(theme);
			//SNUtil.writeToChat("::"+b);
			if (b != null) {
				mset.Sky biomeSky = WorldUtil.getSkybox(b.biomeName);
				if (biomeSky) {
					foreach (WaterParkPiece wp in acu.column) {
						GameObject glass = ObjectUtil.getChildObject(wp.gameObject, "model/Large_Aquarium_generic_room_glass_01");
						ObjectUtil.setSky(glass, biomeSky);
						Renderer r = glass.GetComponentInChildren<Renderer>();
						if (!r) {
							SNUtil.writeToChat("No glass renderer");
							return;
						}
						Material m = r.materials[0];
						if (!m) {
							SNUtil.writeToChat("No glass material");
							return;
						}
						m.SetFloat("_Fresnel", 0.5F);
						m.SetFloat("_Shininess", 7.5F);
						m.SetFloat("_SpecInt", 0.75F);
						m.SetColor("_Color", b.waterColor);
						m.SetColor("_SpecColor", b.waterColor);
						//m.SetInt("_ZWrite", 1);
					}
					foreach (WaterParkItem wp in acu.acu.items) {
						if (wp)
							ObjectUtil.setSky(wp.gameObject, biomeSky);
					}
					foreach (GameObject go in acu.decoHolders) {
						ObjectUtil.setSky(go, biomeSky);
					}
				}
			}
		}
		
		private static void addProp(GameObject go, GameObject slot, BiomeRegions.RegionType r, GameObject rSlot = null) {
			string rname = Enum.GetName(typeof(BiomeRegions.RegionType), r);
			if (!rSlot)
				rSlot = ObjectUtil.getChildObject(slot, rname);
			if (!rSlot) {
				rSlot = new GameObject();
				rSlot.name = rname;
				rSlot.transform.parent = slot.transform;
				rSlot.transform.localPosition = Vector3.zero;
				rSlot.transform.localRotation = Quaternion.identity;
			}
			if (go) {
				go.transform.parent = rSlot.transform;
				go.transform.localPosition = Vector3.zero;
				//go.transform.localRotation = Quaternion.identity;
				ObjectUtil.removeComponent<PrefabIdentifier>(go);
				ObjectUtil.removeComponent<TechTag>(go);
				ObjectUtil.removeComponent<Pickupable>(go);
				ObjectUtil.removeComponent<Collider>(go);
				ObjectUtil.removeComponent<PickPrefab>(go);
				ObjectUtil.removeComponent<Light>(go);
				ObjectUtil.removeComponent<SkyApplier>(go);
				SkyApplier sk = go.EnsureComponent<SkyApplier>();
				sk.renderers = go.GetComponentsInChildren<Renderer>(true);
				ObjectUtil.setSky(go, MarmoSkies.main.skyBaseInterior);
			}
		}
		
		class ACUPropDefinition {
			
			private readonly double weight;
			private readonly string prefab;
			private readonly bool forceUpright;
			private readonly Action<GameObject> modify;
			
			internal ACUPropDefinition(string pfb, double wt, bool up, Action<GameObject> a = null) {
				weight = wt;
				prefab = pfb;
				modify = a;
				forceUpright = up;
			}
			
			internal GameObject spawn() {
				GameObject go = ObjectUtil.createWorldObject(prefab, true, false);
				if (go == null) {
					SNUtil.writeToChat("Could not spawn GO for "+this);
					return null;
				}
				Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
				if (rs.Length == 1)
					go = rs[0].gameObject;//go.GetComponentInChildren<Renderer>(true).gameObject;
				go.SetActive(true);
				if (forceUpright)
					go.transform.rotation = Quaternion.identity;
				if (modify != null)
					modify(go);
				return go;
			}
			
			public override string ToString()
			{
				return string.Format("[ACUPropDefinition Weight={0}, Prefab={1}]", weight, prefab);
			}
			
		}
	}
	
}
