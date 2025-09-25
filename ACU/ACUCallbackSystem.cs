using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.AqueousEngineering {

	public class ACUCallbackSystem {

		public static readonly ACUCallbackSystem instance = new ACUCallbackSystem();

		private static readonly Dictionary<TechType, float> toyValues = new Dictionary<TechType, float>();

		static ACUCallbackSystem() {
			addStalkerToy(TechType.Titanium, 0.1F);
			addStalkerToy(TechType.ScrapMetal, 0.25F);
			addStalkerToy(TechType.Silver, 0.5F);
			addStalkerToy(TechType.Gold, 0.75F);
			addStalkerToy(AqueousEngineeringMod.toy.TechType, 1.5F);
		}

		public static void addStalkerToy(TechType tt, float amt) {
			toyValues[tt] = amt;
		}

		public static bool isStalkerToy(TechType tt) {
			return toyValues.ContainsKey(tt);
		}

		private readonly string oldSaveDir = Path.Combine(Path.GetDirectoryName(AqueousEngineeringMod.modDLL.Location), "acu_data_cache");
		private static readonly string saveFileName = "ACUCache.dat";

		private readonly Dictionary<Vector3, CachedACUData> cache = new Dictionary<Vector3, CachedACUData>();

		private ACUCallbackSystem() {

		}

		internal void register() {
			IngameMenuHandler.Main.RegisterOnLoadEvent(this.loadSave);
			IngameMenuHandler.Main.RegisterOnSaveEvent(this.save);
			SNUtil.migrateSaveDataFolder(oldSaveDir, ".xml", saveFileName);
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

			public override string ToString() {
				return string.Format("[CreatureCache EntityID={0}, Hunger={1}, Happy={2}]", entityID, hunger, happy);
			}


		}

		internal class CachedACUData {

			internal readonly Vector3 acuRoot;

			internal float lastPlanktonBoost;
			internal float boostStrength;
			internal float lastTick;
			internal float nextSoundTime;

			internal Dictionary<string, CreatureCache> creatureData = new Dictionary<string, CreatureCache>();

			internal CachedACUData(Vector3 pos) {
				acuRoot = pos;
			}

			internal void loadFromXML(XmlElement e) {
				lastPlanktonBoost = (float)e.getFloat("plankton", double.NaN);
				boostStrength = (float)e.getFloat("boost", double.NaN);
				lastTick = (float)e.getFloat("tick", double.NaN);

				foreach (XmlElement e2 in e.getDirectElementsByTagName("creatureStatus")) {
					CreatureCache c = new CreatureCache(e2.getProperty("entityID"));
					c.loadFromXML(e2);
				}
			}

			internal void saveToXML(XmlElement e) {
				e.addProperty("position", acuRoot);
				e.addProperty("plankton", lastPlanktonBoost);
				e.addProperty("boost", boostStrength);
				e.addProperty("tick", lastTick);

				foreach (CreatureCache go in creatureData.Values) {
					XmlElement e2 = e.OwnerDocument.CreateElement("creatureStatus");
					go.saveToXML(e2);
					e.AppendChild(e2);
				}
			}

			public override string ToString() {
				return string.Format("[CachedACUData AcuRoot={0}, LastPlanktonBoost={1}, LastTick={2}, CreatureData={3}]", acuRoot, lastPlanktonBoost, lastTick, creatureData.toDebugString());
			}


		}

		private void loadSave() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
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
						SNUtil.log("Error parsing entry '" + e.InnerXml + "': " + ex.ToString());
					}
				}
			}
			SNUtil.log("Loaded ACU data cache: ");
			SNUtil.log(cache.toDebugString());
		}

		private void save() {
			string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
			XmlDocument doc = new XmlDocument();
			XmlElement rootnode = doc.CreateElement("Root");
			doc.AppendChild(rootnode);
			foreach (CachedACUData go in cache.Values) {
				XmlElement e = doc.CreateElement("cache");
				go.saveToXML(e);
				doc.DocumentElement.AppendChild(e);
			}
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
				SNUtil.writeToChat("ACU @ " + wp.transform.position + ": ");
				ACUCallback call = wp.GetComponent<ACUCallback>();
				if (!call)
					SNUtil.writeToChat("No hook");
				SNUtil.writeToChat("Biome set: [" + string.Join(", ", call.potentialBiomes) + "]");
				SNUtil.writeToChat("Plant count: " + call.plantCount);
				SNUtil.writeToChat("Prey count: " + call.herbivoreCount);
				SNUtil.writeToChat("Predator count: " + call.carnivoreCount);
				SNUtil.writeToChat("Sparkle count: " + call.sparkleCount);
				call.nextIsDebug = true;
			}
		}

		internal enum ACUWarnings {
			NOTHEME,
			NOPLANTS,
			SAMEPLANT,
			NOHERBS,
			NOCARNS,
			CARNPREY,
			CARNSPACE,
			HERBFOOD,
			MIXEDTHEME,
		}

		internal class ACUContentView : StorageContainer {

			private ACUCallback controller;

			private static readonly Type resourceMonitorLogic = InstructionHandlers.getTypeBySimpleName("ResourceMonitor.Components.ResourceMonitorLogic");

			internal void setController(ACUCallback acu) {
				if (controller == acu)
					return;
				controller = acu;/*
				storageRoot = gameObject.EnsureComponent<ChildObjectIdentifier>();
				storageRoot.ClassId = "ACUFakeInv";
				storageRoot.id = "";*/
				container = new ACUContainerRelay(acu);
			}

			void Update() {
				if (container != null)
					((ACUContainerRelay)container).tick();
			}

			public override void Awake() {
				creationTime = DayNightCycle.main.timePassedAsFloat; //do not invoke createContainer, which means need to do the below hook manually
				if (resourceMonitorLogic != null)
					this.Invoke("notifyResourceMonitor", 10);
			}

			private void notifyResourceMonitor() {
				//SNUtil.writeToChat("Updating StorageMonitors with ACU "+controller.acu.transform.position);
				Type t2 = resourceMonitorLogic.Assembly.GetType("ResourceMonitor.Patchers.StorageContainerAwakePatcher");
				IList li = (IList)t2.GetField("registeredResourceMonitors", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetValue(null);
				MethodInfo call = resourceMonitorLogic.GetMethod("AlertNewStorageContainerPlaced", BindingFlags.Public | BindingFlags.Instance);
				foreach (object obj in li) {
					call.Invoke(obj, BindingFlags.Default, null, new object[] { this }, null);
				}
			}

		}

		internal class ACUContainerRelay : ItemsContainer {

			private readonly ACUCallback controller;

			internal ACUContainerRelay(ACUCallback call) : base(8, 10, call.transform, "ACURelay", null) {
				controller = call;
				onRemoveItem += (ii) => { controller.acu.RemoveItem(ii.item.GetComponent<WaterParkItem>()); };
			}

			internal void tick() {
				_items.Clear();
				foreach (WaterParkItem wp in controller.acu.items) {
					Pickupable pp = wp.GetComponent<Pickupable>();
					if (pp) {
						TechType tt = pp.GetTechType();
						Vector2int sz = CraftData.GetItemSize(tt);
						ItemGroup grp = _items.ContainsKey(tt) ? _items[tt] : new ItemGroup(_items.Count, sz.x, sz.y);
						grp.items.Add(new InventoryItem(pp));
						_items[tt] = grp;
					}
				}
				//SNUtil.writeToChat(_items.toDebugString());
			}

		}

		internal class ACUCallback : MonoBehaviour {

			internal WaterPark acu;

			internal BaseRoot seabase;
			internal BaseCell baseCell;
			internal ACUContentView contentView;
			internal StorageContainer planter;
			internal List<WaterParkPiece> column;
			internal GameObject lowestSegment;
			internal GameObject floor;
			internal List<GameObject> decoHolders;

			internal HashSet<BiomeRegions.RegionType> potentialBiomes = new HashSet<BiomeRegions.RegionType>();
			internal BiomeRegions.RegionType currentTheme = BiomeRegions.Shallows;
			internal float plantCount;
			internal float herbivoreCount;
			internal float carnivoreCount;
			internal int sparkleCount;
			internal int cuddleCount;
			internal int gasopodCount;
			internal bool consistentBiome;

			internal BaseBioReactor isAboveBioreactor;

			internal float infectedTotal;
			internal float currentBonus;
			internal float stalkerToyValue;

			private readonly List<ACUWarnings> currentWarnings = new List<ACUWarnings>();

			internal bool nextIsDebug = false;

			internal float lastThemeUpdate;
			internal bool appliedTheme;

			private GameObject bubbleVents;
			private ParticleSystem[] ventBubbleEmitters;

			private CachedACUData cache;

			internal void setACU(WaterPark w) {
				if (acu != w) {

					this.CancelInvoke("tick");
					if (contentView)
						contentView.destroy(false);
					contentView = null;
					planter = null;
					seabase = null;
					column = null;
					decoHolders = null;
					lowestSegment = null;
					isAboveBioreactor = null;
					floor = null;
					cache = null;

					acu = w;

					if (acu) {
						//SNUtil.writeToChat("Setup ACU Hook");
						SNUtil.log("Switching ACU " + acu + " @ " + acu.transform.position + " to " + this);
						this.InvokeRepeating("tick", 0, 1);
						seabase = acu.GetComponentInParent<BaseRoot>();
						planter = acu.planter.GetComponentInChildren<StorageContainer>();
						column = ACUCallbackSystem.instance.getACUComponents(acu);
						lowestSegment = ACUCallbackSystem.instance.getACUFloor(column);
						baseCell = ObjectUtil.getBaseRoom(seabase, lowestSegment);
						if (lowestSegment) {
							floor = lowestSegment.getChildObject("Large_Aquarium_Room_generic_ground");
							decoHolders = lowestSegment.getChildObjects(ACUTheming.ACU_DECO_SLOT_NAME);
							bubbleVents = lowestSegment.getChildObject("Bubbles");
						}
						else {
							SNUtil.log("ACU " + acu.transform.position + " had no lowest segment??? C=" + column.Select<WaterParkPiece, string>(wpp => wpp.transform.position.ToString()).toDebugString());
						}
						ventBubbleEmitters = bubbleVents.GetComponentsInChildren<ParticleSystem>();
						contentView = acu.gameObject.EnsureComponent<ACUContentView>();
						contentView.enabled = true;
						contentView.setController(this);
						isAboveBioreactor = ACUCallbackSystem.instance.checkAboveBioreactor(acu, this);
						this.load();
					}
				}
			}

			private void load() {
				cache = instance.getOrCreateCache(this);
				foreach (CreatureCache cc in cache.creatureData.Values) {
					WaterParkItem wp = this.getItemByID(cc.entityID);
					if (wp) {
						Creature c = wp.GetComponent<Creature>();
						if (c) {
							cc.apply(c);
							SNUtil.log("Deserializing cached ACU creature status " + cc);
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
				return dt <= 15 ? (1 - (dt / 15F)) * cache.boostStrength : 0;
			}

			public void boost(ACUFuel amt) {
				cache.lastPlanktonBoost = DayNightCycle.main.timePassedAsFloat;
				cache.boostStrength = amt.effectStrength;
			}

			internal void printTerminalInfo() {/*
				SNUtil.writeToChat("Biome Archetype: "+currentTheme);
				SNUtil.writeToChat("Plant Count: "+plantCount);
				SNUtil.writeToChat("Herbivore Count: "+herbivoreCount);
				SNUtil.writeToChat("Carnivore Count: "+carnivoreCount);
				
				SNUtil.writeToChat("Stalker Toy Rating: "+stalkerToyValue.ToString("0.0"));*/
				Dictionary<string, object> values = new Dictionary<string, object>();
				double day = DayNightCycle.main.GetDay();
				int dday = (int)day;
				double frac = day-dday;
				values["day"] = dday;
				values["time"] = (int)(frac * 1200) + "s";
				values["contents"] = this.generateContentList();
				values["biome"] = currentTheme.getName();
				values["plants"] = plantCount.ToString("0.0");
				values["herbivores"] = herbivoreCount.ToString("0.0");
				values["carnivores"] = carnivoreCount.ToString("0.0");
				values["sparkles"] = sparkleCount;
				values["infected"] = infectedTotal.ToString("0.00");
				values["bonus"] = (currentBonus * 100).ToString("0.00");
				values["stalkerToy"] = stalkerToyValue.ToString("0.0");
				values["height"] = acu.height;
				values["count"] = acu.usedSpace;
				values["capacity"] = acu.wpPieceCapacity * acu.height;
				values["alerts"] = currentWarnings.Count == 0 ? "[None]" : string.Join("\n", currentWarnings.Select<ACUWarnings, string>(w => AqueousEngineeringMod.acuLocale.getEntry(w.ToString()).desc));

				XMLLocale.LocaleEntry e = AqueousEngineeringMod.acuMonitorBlock.locale;
				PDAManager.PDAPage pp = PDAManager.getPage(e.key+"PDA");
				pp.unlock();
				pp.setPlaceholderValues(e.pda, values, true);
				pp.show(pda => pp.update(AqueousEngineeringMod.acuLocale.getEntry("NotTerminalPDA").desc, true, false));
			}

			private string generateContentList() {
				CountMap<TechType> counts = new CountMap<TechType>();
				Dictionary<TechType, int> sizes = new Dictionary<TechType, int>();
				foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) {
					if (!wp)
						continue;
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (tt != TechType.None) {
						int sz = wp.GetSize();
						if (sz > 0) {
							sizes[tt] = sz;
							counts.add(tt);
						}
					}
				}
				StringBuilder sb = new StringBuilder();
				foreach (TechType tt in counts.getItems()) {
					sb.Append(Language.main.Get(tt));
					sb.Append(": ");
					sb.Append(counts.getCount(tt));
					sb.Append(" (");
					sb.Append(sizes[tt]);
					sb.Append(" occupancy slots each)\n");
				}
				return sb.ToString();
			}

			public void tick() {
				if (!floor || !lowestSegment) {
					this.setACU(null);
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
				currentWarnings.Clear();
				potentialBiomes.Clear();
				potentialBiomes.AddRange(BiomeRegions.getAllBiomes());
				//SNUtil.writeToChat("SC:"+sc);
				PrefabIdentifier[] plants = planter.GetComponentsInChildren<PrefabIdentifier>();
				plantCount = 0;
				herbivoreCount = 0;
				carnivoreCount = 0;
				int teeth = 0;
				cuddleCount = 0;
				gasopodCount = 0;
				infectedTotal = 0;
				sparkleCount = 0;
				//SNUtil.writeToChat("@@"+string.Join(",", possibleBiomes));
				List<InfectedMixin> infectedFish = new List<InfectedMixin>();
				List<WaterParkCreature> foodFish = new List<WaterParkCreature>();
				List<Stalker> stalkers = new List<Stalker>();
				stalkerToyValue = 0;
				bool hasStalkerToy = false;
				bool acuRoom = BaseRoomSpecializationSystem.instance.getSavedType(acu) == BaseRoomSpecializationSystem.RoomTypes.ACU;
				foreach (WaterParkItem wp in new List<WaterParkItem>(acu.items)) { //clone because might change in ACUEcosystems.handleCreature
					if (!wp)
						continue;
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (isStalkerToy(tt)) {
						hasStalkerToy |= tt == AqueousEngineeringMod.toy.TechType;
						stalkerToyValue += toyValues[tt];
						pp.gameObject.transform.localScale = Vector3.one * 0.5F;
					}
					else if (tt == TechType.StalkerTooth) {
						pp.gameObject.transform.localScale = Vector3.one * 0.125F;
						teeth++;
					}
					else if (wp is WaterParkCreature wpc) {
						if (wpc.parameters == null) {
							SNUtil.log("WaterParkCreature had null params: " + wpc.name + " / " + wp.GetComponent<Creature>());
							ObjectUtil.dumpObjectData(wpc.gameObject);
							wpc.parameters = WaterParkCreature.GetParameters(wpc.GetComponent<Pickupable>().GetTechType());
							if (wpc.parameters == null) {
								SNUtil.log("Fetches null params!");
								wpc.parameters = WaterParkCreatureParameters.GetDefaultValue();
							}
						}
						InfectedMixin mix = wp.GetComponent<InfectedMixin>();
						if (mix) {
							float amt = mix.GetInfectedAmount();
							if (amt > 0) {
								infectedTotal += amt;
								infectedFish.Add(mix);
							}
						}
						Creature c = ACUEcosystems.handleCreature(this, dT, wpc, tt, foodFish, plants, acuRoom, potentialBiomes);
						if (tt == TechType.Stalker) {
							stalkers.Add((Stalker)c);
						}
					}
				}

				if (AqueousEngineeringMod.config.getBoolean(AEConfig.ConfigEntries.ACUSOUND) && time >= cache.nextSoundTime && acu.items.Count > 0) {
					WaterParkCreature wpc = acu.items.GetRandom() as WaterParkCreature;
					if (wpc) {
						bool flag = false;
						FMOD_CustomLoopingEmitter[] ca = wpc.GetComponentsInChildren<FMOD_CustomLoopingEmitter>();
						if (ca != null && ca.Length > 0) {
							ca.GetRandom().Play();
							flag = true;
						}
						else {
							AttackLastTarget[] a = wpc.GetComponentsInChildren<AttackLastTarget>();
							if (a != null && a.Length > 0) {
								FMOD_CustomEmitter emit = a.GetRandom().attackStartSound;
								if (emit) {
									emit.Play();
									flag = true;
								}
							}
						}
						cache.nextSoundTime = flag ? time + UnityEngine.Random.Range(1F, 5F) : time + UnityEngine.Random.Range(1F, 2F);
					}
				}

				HashSet<ACUEcosystems.PlantFood> plantTypes = ACUEcosystems.collectPlants(this, plants, potentialBiomes);
				consistent = potentialBiomes.Count > 0 && plantCount > 0;
				int max = potentialBiomes.Count == 1 ? ACUEcosystems.getPlantsForBiome(potentialBiomes.First<BiomeRegions.RegionType>()).Count : 99;
				bool plantVar = plantTypes.Count >= Mathf.Min(2, max);
				bool tooManyCarnisPrey = carnivoreCount > Math.Max(1, herbivoreCount/Mathf.Max(1, 6-(sparkleCount*0.5F)));
				bool tooManyCarnisSpace = carnivoreCount > acu.height*(acuRoom ? 2F : 1.5F);
				bool tooManyHerbis = herbivoreCount > plantCount*(4+(sparkleCount*0.5F))*(acuRoom ? 1.5F : 1F);
				bool hasPlants = plantCount > 0;
				bool hasHerbis = herbivoreCount > 0;
				bool hasCarnis = carnivoreCount > 0;
				healthy = hasPlants && hasHerbis && hasCarnis && !tooManyCarnisPrey && !tooManyCarnisSpace && !tooManyHerbis;
				if (!hasPlants)
					currentWarnings.Add(ACUWarnings.NOPLANTS);
				if (!plantVar && hasPlants)
					currentWarnings.Add(ACUWarnings.SAMEPLANT);
				if (!hasHerbis)
					currentWarnings.Add(ACUWarnings.NOHERBS);
				if (!hasCarnis)
					currentWarnings.Add(ACUWarnings.NOCARNS);
				if (tooManyCarnisPrey)
					currentWarnings.Add(ACUWarnings.CARNPREY);
				if (tooManyCarnisSpace)
					currentWarnings.Add(ACUWarnings.CARNSPACE);
				if (tooManyHerbis && hasHerbis)
					currentWarnings.Add(ACUWarnings.HERBFOOD);

				currentBonus = 0;
				if (consistent)
					currentBonus += 1F;
				else
					currentWarnings.Add(ACUWarnings.NOTHEME);
				if (healthy)
					currentBonus += 2F;
				if (sparkleCount > 0)
					currentBonus *= 1 + (sparkleCount * 0.5F);
				if (nextIsDebug)
					SNUtil.writeToChat(plantCount + "/" + herbivoreCount + "/" + carnivoreCount + "$" + sparkleCount + " & " + string.Join(", ", potentialBiomes) + " > " + healthy + " & " + consistent + " > " + currentBonus);
				float f0 = this.getBoostStrength(time);
				if (ventBubbleEmitters != null) {
					foreach (ParticleSystem p in ventBubbleEmitters) {
						if (p && p.gameObject.name == "xBubbleColumn") {
							ParticleSystem.MainModule main = p.main;
							main.startColor = Color.Lerp(Color.white, new Color(0.2F, 1F, 0.4F), f0);
							main.startSizeMultiplier = 0.5F + (1.5F * f0);
							main.startLifetimeMultiplier = 1.7F + (2.3F * f0);
						}
					}
				}
				currentBonus += 5F * f0;
				if (infectedTotal > 0) {
					currentBonus -= infectedTotal * 2;
					if (UnityEngine.Random.Range(0F, 1F) <= infectedTotal * 0.015F * dT) {
						GameObject go = ObjectUtil.createWorldObject(VanillaCreatures.WARPER.prefab);
						bool inACU = UnityEngine.Random.Range(0F, 1F) < 0.2F;
						go.transform.position = inACU ? acu.transform.position : MathUtil.getRandomVectorAround(acu.transform.position, new Vector3(10, 0, 10));
						Warper wp = go.GetComponent<Warper>();
						wp.WarpIn(null);
						if (inACU) {
							go.EnsureComponent<ACUWarper>();
						}
						else {
							AttractToTarget.attractCreatureToTarget(wp, acu.gameObject.FindAncestor<BaseCell>().GetComponent<LiveMixin>(), false);
						}
					}
				}
				if (currentBonus > 0) {
					float boost = currentBonus*dT;
					foreach (WaterParkCreature wp in foodFish) {
						//SNUtil.writeToChat(wp+" > "+boost+" > "+wp.matureTime+"/"+wp.timeNextBreed);
						if (wp.canBreed) {
							Peeper pp = wp.gameObject.GetComponent<Peeper>();
							if (pp && pp.isHero)
								wp.timeNextBreed = DayNightCycle.main.timePassedAsFloat + 1000; //prevent sparkle peepers from breeding
							else if (wp.isMature)
								wp.timeNextBreed -= boost;
							else
								wp.matureTime -= boost;
						}
					}
				}
				if (consistent && healthy && potentialBiomes.Contains(BiomeRegions.Kelp)) {
					bool single = potentialBiomes.Count == 1;
					foreach (Stalker s in stalkers) {
						if (hasStalkerToy)
							s.Happy.Add(dT * 0.05F);
						if (teeth < 6) {
							float f = dT*Mathf.Min(8, stalkerToyValue)*0.00012F*(1+(2*s.Happy.Value))*(single ? 1 : 0.2F);
							//SNUtil.writeToChat(s.Happy.Value+" x "+stalkerToyValue+" > "+f);
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
				}
				if (nextIsDebug)
					SNUtil.writeToChat("Final biome set: [" + string.Join(", ", potentialBiomes) + "]");
				if (potentialBiomes.Count == 1) {
					BiomeRegions.RegionType theme = potentialBiomes.First<BiomeRegions.RegionType>();
					if (theme == BiomeRegions.Other)
						theme = BiomeRegions.Shallows;
					bool changed = theme != currentTheme;
					currentTheme = theme;
					consistentBiome = true;
					ACUTheming.updateACUTheming(this, theme, time, changed || time - lastThemeUpdate > 5 || !appliedTheme);
				}
				else if (potentialBiomes.Count > 1) {
					currentWarnings.Add(ACUWarnings.MIXEDTHEME);
					consistentBiome = false;
				}
				else {
					consistentBiome = false;
				}
				nextIsDebug = false;
			}

			public bool isHealthy() {
				return currentWarnings.Count == 0;
			}
		}

		class ACUWarper : MonoBehaviour {

			void Update() {
				transform.localScale = Vector3.one * 0.4F;
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

		internal BaseBioReactor checkAboveBioreactor(WaterPark acu, ACUCallback call) {
			//BaseCell cell = call.lowestSegment.FindAncestor<BaseCell>();
			Vector3 at = acu.transform.position;//cell.transform.position; //acu pos is lowest one
			Vector3 seek = at+(Vector3.down*3.5F);
			/*
			BaseCell below = null;
			foreach (BaseCell bc in cell.transform.parent.GetComponentsInChildren<BaseCell>()) {
				if ((bc.transform.position-seek).sqrMagnitude < 0.01) {
					below = bc;
					break;
				}
			}
			if (below) {
				
			}*/
			foreach (BaseBioReactor bio in acu.transform.parent.GetComponentsInChildren<BaseBioReactor>()) { //acu is parented directly to base, as is bioreactor
				if ((bio.transform.position - seek).sqrMagnitude < 0.01) {
					return bio;
				}
			}
			return null;
		}
	}

}
