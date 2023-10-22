using System;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;

namespace ReikaKalseki.AqueousEngineering {
	
	public class ACUCallbackSystem {
		
		public static readonly ACUCallbackSystem instance = new ACUCallbackSystem();
		
		private static readonly Dictionary<TechType, float> toyValues = new Dictionary<TechType, float>();
		
		static ACUCallbackSystem() {
			addStalkerToy(TechType.Titanium, 0.5F);
			addStalkerToy(TechType.ScrapMetal, 1F);
			addStalkerToy(TechType.Silver, 2F);
		}
		
		public static void addStalkerToy(TechType tt, float amt) {
			toyValues[tt] = amt;
		}
		
		public static bool isStalkerToy(TechType tt) {
			return toyValues.ContainsKey(tt);
		}
		
		private readonly string xmlPathRoot;
		
		private readonly Dictionary<Vector3, CachedACUData> cache = new Dictionary<Vector3, CachedACUData>();
		
		private ACUCallbackSystem() {
			xmlPathRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "acu_data_cache");
		}
		
		internal void register() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(save);
		}
		
		internal class CreatureCache {
			
			internal readonly string entityID;
			
			internal float hunger;
			internal float happy;
			
			internal CreatureCache(string id) {
				entityID = id;
			}
			
			internal void loadFromXML(XmlElement e) {
				hunger = (float)e.getFloat("hunger", double.NaN);
				happy = (float)e.getFloat("happy", double.NaN);
			}
			
			internal void saveToXML(XmlElement e) {
				e.addProperty("hunger", hunger);
				e.addProperty("happy", happy);
				e.addProperty("entityID", entityID);
			}
			
			internal void apply(Creature c) {
				c.Hunger.Value = hunger;
				c.Happy.Value = happy;
			}
			
			public override string ToString()
			{
				return string.Format("[CreatureCache EntityID={0}, Hunger={1}, Happy={2}]", entityID, hunger, happy);
			}

			
		}
		
		internal class CachedACUData {
			
			internal readonly Vector3 acuRoot;
			
			internal float lastPlanktonBoost;
			internal float lastTick;
			
			internal Dictionary<string, CreatureCache> creatureData = new Dictionary<string, CreatureCache>();
			
			internal CachedACUData(Vector3 pos) {
				acuRoot = pos;
			}
			
			internal void loadFromXML(XmlElement e) {
				lastPlanktonBoost = (float)e.getFloat("plankton", double.NaN);
				lastTick = (float)e.getFloat("tick", double.NaN);
				
				foreach (XmlElement e2 in e.getDirectElementsByTagName("creatureStatus")) {
					CreatureCache c = new CreatureCache(e2.getProperty("entityID"));
					c.loadFromXML(e2);
				}
			}
			
			internal void saveToXML(XmlElement e) {
				e.addProperty("position", acuRoot);
				e.addProperty("plankton", lastPlanktonBoost);
				e.addProperty("tick", lastTick);
				
				foreach (CreatureCache go in creatureData.Values) {
					XmlElement e2 = e.OwnerDocument.CreateElement("creatureStatus");
					go.saveToXML(e2);
					e.AppendChild(e2);
				}
			}
			
			public override string ToString()
			{
				return string.Format("[CachedACUData AcuRoot={0}, LastPlanktonBoost={1}, LastTick={2}, CreatureData={3}]", acuRoot, lastPlanktonBoost, lastTick, creatureData.toDebugString());
			}

			
		}
		
		private void loadSave() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			if (File.Exists(path)) {
				XmlDocument doc = new XmlDocument();
				doc.Load(path);
				foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
					try {
						CachedACUData pfb = new CachedACUData(e.getVector("position").Value);
						pfb.loadFromXML(e);
						cache[pfb.acuRoot] = pfb;
					}
					catch (Exception ex) {
						SNUtil.log("Error parsing entry '"+e.InnerXml+"': "+ex.ToString());
					}
				}
			}
			SNUtil.log("Loaded ACU data cache: ");
			SNUtil.log(cache.toDebugString());
		}
		
		private void save() {
			string path = Path.Combine(xmlPathRoot, SaveLoadManager.main.currentSlot+".xml");
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (CachedACUData go in cache.Values) {
				XmlElement e = doc.CreateElement("cache");
				go.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
			Directory.CreateDirectory(xmlPathRoot);
			doc.Save(path);
		}
		
		public void tick(WaterPark acu) {
			if (acu && acu.gameObject) {
				ACUCallback com = acu.gameObject.EnsureComponent<ACUCallback>();
				com.setACU(acu);
			}
		}
		
		private CachedACUData getOrCreateCache(ACUCallback acu) {
			Vector3 pos = acu.lowestSegment.transform.position;
			if (!cache.ContainsKey(pos)) {
				cache[pos] = new CachedACUData(pos);
			}
			return cache[pos];
		}
		
		public void debugACU() {
			WaterPark wp = Player.main.currentWaterPark;
			if (wp) {
				SNUtil.writeToChat("ACU @ "+wp.transform.position+": ");
				ACUCallback call = wp.GetComponent<ACUCallback>();
				if (!call)
					SNUtil.writeToChat("No hook");
				SNUtil.writeToChat("Biome set: ["+string.Join(", ", call.potentialBiomes)+"]");
				SNUtil.writeToChat("Plant count: "+call.plantCount);
				SNUtil.writeToChat("Prey count: "+call.herbivoreCount);
				SNUtil.writeToChat("Predator count: "+call.carnivoreCount);
				SNUtil.writeToChat("Sparkle count: "+call.sparkleCount);
				call.nextIsDebug = true;
			}
		}
		
		internal class ACUCallback : MonoBehaviour {
			
			internal WaterPark acu;
			
			internal StorageContainer sc;
			internal List<WaterParkPiece> column;
			internal GameObject lowestSegment;
			internal GameObject floor;
			internal List<GameObject> decoHolders;
			
			internal HashSet<BiomeRegions.RegionType> potentialBiomes = new HashSet<BiomeRegions.RegionType>();
			internal BiomeRegions.RegionType currentTheme = BiomeRegions.RegionType.Shallows;
			internal int plantCount;
			internal int herbivoreCount;
			internal int carnivoreCount;
			internal int sparkleCount;
			internal int cuddleCount;
			internal float stalkerToyValue;
			
			internal bool nextIsDebug = false;
			
			internal float lastThemeUpdate;
			
			private GameObject bubbleVents;
			private ParticleSystem[] ventBubbleEmitters;
			
			private CachedACUData cache;
			
			internal void setACU(WaterPark w) {
				if (acu != w) {
					
					CancelInvoke("tick");
					sc = null;
					column = null;
					decoHolders = null;
					lowestSegment = null;
					floor = null;
					cache = null;
					
					acu = w;
					
					if (acu) {
						//SNUtil.writeToChat("Setup ACU Hook");
						SNUtil.log("Switching ACU "+acu+" @ "+acu.transform.position+" to "+this);
						InvokeRepeating("tick", 0, 1);
						sc = acu.planter.GetComponentInChildren<StorageContainer>();
						column = ACUCallbackSystem.instance.getACUComponents(acu);
						lowestSegment = ACUCallbackSystem.instance.getACUFloor(column);
						floor = ObjectUtil.getChildObject(lowestSegment, "Large_Aquarium_Room_generic_ground");
						decoHolders = ObjectUtil.getChildObjects(lowestSegment, ACUTheming.ACU_DECO_SLOT_NAME);
						bubbleVents = ObjectUtil.getChildObject(lowestSegment, "Bubbles");
						ventBubbleEmitters = bubbleVents.GetComponentsInChildren<ParticleSystem>();
						load();
					}
				}
			}
			
			private void load() {
				cache = instance.getOrCreateCache(this);
				foreach (CreatureCache cc in cache.creatureData.Values) {
					WaterParkItem wp = getItemByID(cc.entityID);
					if (wp) {
						Creature c = wp.GetComponent<Creature>();
						if (c) {
							cc.apply(c);
							SNUtil.log("Deserializing cached ACU creature status "+cc);
						}
					}
				}
			}
			
			internal WaterParkItem getItemByID(string id) {
				foreach (WaterParkItem wp in acu.items) {
					PrefabIdentifier pi = wp.GetComponent<PrefabIdentifier>();
					if (pi && pi.Id == id)
						return wp;
				}
				return null;
			}
			
			internal CreatureCache getOrCreateCreatureStatus(MonoBehaviour wp) {
				if (cache == null)
					return null;
				string id = wp.gameObject.FindAncestor<PrefabIdentifier>().Id; //NOT classID
				if (!cache.creatureData.ContainsKey(id)) {
					cache.creatureData[id] = new CreatureCache(id);
				}
				return cache.creatureData[id];
			}
			
			public float getBoostStrength(float time) {
				if (cache == null)
					return 0;
				float dt = time-cache.lastPlanktonBoost;
				return dt <= 15 ? 1-dt/15F : 0;
			}
			
			public void boost() {
				cache.lastPlanktonBoost = DayNightCycle.main.timePassedAsFloat;
			}
		
			public void tick() {
				if (!floor || !lowestSegment) {
					setACU(null);
					return;
				}
				float time = DayNightCycle.main.timePassedAsFloat;
				float dT = time-cache.lastTick;
				cache.lastTick = time;
				if (dT <= 0.0001)
					return;
				//SNUtil.writeToChat(dT+" s");
				bool healthy = false;
				bool consistent = true;
				potentialBiomes.Clear();
				potentialBiomes.AddRange((IEnumerable<BiomeRegions.RegionType>)Enum.GetValues(typeof(BiomeRegions.RegionType)));
				//SNUtil.writeToChat("SC:"+sc);
				PrefabIdentifier[] plants = sc.GetComponentsInChildren<PrefabIdentifier>();
				plantCount = 0;
				herbivoreCount = 0;
				carnivoreCount = 0;
				int teeth = 0;
				cuddleCount = 0;
				sparkleCount = 0;
				//SNUtil.writeToChat("@@"+string.Join(",", possibleBiomes));
				List<WaterParkCreature> foodFish = new List<WaterParkCreature>();
				List<Stalker> stalkers = new List<Stalker>();
				stalkerToyValue = 0;
				foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
					if (!wp)
						continue;
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (isStalkerToy(tt)) {
						stalkerToyValue += toyValues[tt];
						pp.gameObject.transform.localScale = Vector3.one*0.5F;
					}
					else if (tt == TechType.StalkerTooth) {
						pp.gameObject.transform.localScale = Vector3.one*0.125F;
						teeth++;
					}
					else if (wp is WaterParkCreature) {
						Creature c = ACUEcosystems.handleCreature(this, dT, wp, tt, foodFish, plants, ref potentialBiomes);
						if (tt == TechType.Stalker) {
							stalkers.Add((Stalker)c);
						}
					}
		   	 	}
				HashSet<ACUEcosystems.PlantFood> plantTypes = ACUEcosystems.collectPlants(this, plants, ref potentialBiomes);
				consistent = potentialBiomes.Count > 0 && plantCount > 0;
				int max = potentialBiomes.Count == 1 ? ACUEcosystems.getPlantsForBiome(potentialBiomes.First<BiomeRegions.RegionType>()).Count : 99;
				healthy = plantCount > 0 && plantTypes.Count >= Mathf.Min(2, max) && herbivoreCount > 0 && carnivoreCount > 0 && carnivoreCount <= Math.Max(1, herbivoreCount/Mathf.Max(1, 6-sparkleCount*0.5F)) && carnivoreCount <= acu.height*1.5F && herbivoreCount > 0 && herbivoreCount <= plantCount*(4+sparkleCount*0.5F);
				float boost = 0;
				if (consistent)
					boost += 1F;
				if (healthy)
					boost += 2F;
				if (sparkleCount > 0)
					boost *= 1+sparkleCount*0.5F;
				if (nextIsDebug)
					SNUtil.writeToChat(plantCount+"/"+herbivoreCount+"/"+carnivoreCount+"$"+sparkleCount+" & "+string.Join(", ", potentialBiomes)+" > "+healthy+" & "+consistent+" > "+boost);
				float f0 = getBoostStrength(time);
				if (ventBubbleEmitters != null) {
					foreach (ParticleSystem p in ventBubbleEmitters) {
						if (p && p.gameObject.name == "xBubbleColumn") {
							ParticleSystem.MainModule main = p.main;
							main.startColor = Color.Lerp(Color.white, new Color(0.2F, 1F, 0.4F), f0);
							main.startSizeMultiplier = 0.5F+1.5F*f0;
							main.startLifetimeMultiplier = 1.7F+2.3F*f0;
						}					
					}
				}
				boost += 5F*f0;
				if (boost > 0) {
					boost *= dT;
					foreach (WaterParkCreature wp in foodFish) {
						//SNUtil.writeToChat(wp+" > "+boost+" > "+wp.matureTime+"/"+wp.timeNextBreed);
						if (wp.canBreed) {
							Peeper pp = wp.gameObject.GetComponent<Peeper>();
							if (pp && pp.isHero)
								wp.timeNextBreed = DayNightCycle.main.timePassedAsFloat+1000; //prevent sparkle peepers from breeding
							else if (wp.isMature)
								wp.timeNextBreed -= boost;
							else
								wp.matureTime -= boost;
						}
					}
				}
				if (teeth < 10 && consistent && healthy && potentialBiomes.Contains(BiomeRegions.RegionType.Kelp)) {
					foreach (Stalker s in stalkers) {
						float f = dT*stalkerToyValue*0.006F*s.Happy.Value;
						//SNUtil.writeToChat(s.Happy.Value+" x "+stalkerToys.Count+" > "+f);
						if (UnityEngine.Random.Range(0F, 1F) < f) {
							//do not use, so can have ref to GO; reimplement // s.LoseTooth();
							GameObject go = UnityEngine.Object.Instantiate<GameObject>(s.toothPrefab);
							//SNUtil.writeToChat(s+" > "+go);
							go.transform.position = s.loseToothDropLocation.transform.position;
							go.transform.rotation = s.loseToothDropLocation.transform.rotation;
							if (go.activeSelf && s.isActiveAndEnabled) {
								foreach (Collider c in go.GetComponentsInChildren<Collider>())
									Physics.IgnoreCollision(s.stalkerBodyCollider, c);
							}
							Utils.PlayFMODAsset(s.loseToothSound, go.transform, 8f);
							LargeWorldEntity.Register(go);
							acu.AddItem(go.GetComponent<Pickupable>());
						}
					}
				}
				if (nextIsDebug)
					SNUtil.writeToChat("Final biome set: ["+string.Join(", ", potentialBiomes)+"]");
				if (potentialBiomes.Count == 1) {
					BiomeRegions.RegionType theme = potentialBiomes.First<BiomeRegions.RegionType>();
					if (theme == BiomeRegions.RegionType.Other)
						theme = BiomeRegions.RegionType.Shallows;
					bool changed = theme != currentTheme;
					currentTheme = theme;
					ACUTheming.updateACUTheming(this, theme, time, changed || time-lastThemeUpdate > 5);
				}
				nextIsDebug = false;
			}
		}
		
		internal List<WaterParkPiece> getACUComponents(WaterPark acu) {
			List<WaterParkPiece> li = new List<WaterParkPiece>();
			foreach (WaterParkPiece wp in acu.transform.parent.GetComponentsInChildren<WaterParkPiece>()) {
				if (wp && wp.name.ToLowerInvariant().Contains("bottom") && wp.GetBottomPiece().GetModule() == acu)
					li.Add(wp);
			}
			return li;
		}
		
		internal GameObject getACUFloor(IEnumerable<WaterParkPiece> li) {
			foreach (WaterParkPiece wp in li) {
				if (wp.floorBottom && wp.floorBottom.activeSelf && wp.IsBottomPiece())
					return wp.floorBottom;
			}
			return null;
		}
		
		internal GameObject getACUCeiling(IEnumerable<WaterParkPiece> li) {
			foreach (WaterParkPiece wp in li) {
				if (wp.ceilingTop && wp.ceilingTop.activeSelf)
					return wp.ceilingTop;
			}
			return null;
		}
	}
	
}
