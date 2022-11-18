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
	
	public class ACUCallbackSystem {
		
		public static readonly ACUCallbackSystem instance = new ACUCallbackSystem();
		
		private ACUCallbackSystem() {
			
		}
		
		public void tick(WaterPark acu) {
			if (acu && acu.gameObject)
				acu.gameObject.EnsureComponent<ACUCallback>().setACU(acu);
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
			private float lastTick;
			
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
			
			private float lastPlanktonBoost;
			
			internal void setACU(WaterPark w) {
				if (acu != w) {
					
					CancelInvoke("tick");
					sc = null;
					column = null;
					decoHolders = null;
					lowestSegment = null;
					floor = null;
					
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
					}
				}
			}
			
			public float getBoostStrength(float time) {
				float dt = time-lastPlanktonBoost;
				return dt <= 15 ? 1-dt/15F : 0;
			}
			
			public void boost() {
				lastPlanktonBoost = DayNightCycle.main.timePassedAsFloat;
			}
		
			public void tick() {
				float time = DayNightCycle.main.timePassedAsFloat;
				float dT = time-lastTick;
				lastTick = time;
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
					if (tt == TechType.Titanium || tt == TechType.ScrapMetal || tt == TechType.Silver) {
						pp.gameObject.transform.localScale = Vector3.one*0.5F;
						float v = 0;
						switch(tt) {
							case TechType.Titanium:
								v = 0.5F;
								break;
							case TechType.ScrapMetal:
								v = 1;
								break;
							case TechType.Silver:
								v = 2;
								break;
						}
						stalkerToyValue += v;
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
				HashSet<VanillaFlora> plantTypes = ACUEcosystems.collectPlants(this, plants, ref potentialBiomes);
				consistent = potentialBiomes.Count > 0 && plantCount > 0;
				healthy = plantCount > 0 && plantTypes.Count > (potentialBiomes.Count == 1 && potentialBiomes.First<BiomeRegions.RegionType>() == BiomeRegions.RegionType.LavaZone ? 0 : 1) && herbivoreCount > 0 && carnivoreCount > 0 && carnivoreCount <= Math.Max(1, herbivoreCount/Mathf.Max(1, 6-sparkleCount*0.5F)) && carnivoreCount <= acu.height*1.5F && herbivoreCount > 0 && herbivoreCount <= plantCount*(4+sparkleCount*0.5F);
				float boost = 0;
				if (consistent)
					boost += 1F;
				if (healthy)
					boost += 2F;
				if (sparkleCount > 0)
					boost *= 1+sparkleCount*0.5F;
				if (nextIsDebug)
					SNUtil.writeToChat(plantCount+"/"+herbivoreCount+"/"+carnivoreCount+"$"+sparkleCount+" & "+string.Join(", ", potentialBiomes)+" > "+healthy+" & "+consistent+" > "+boost);
				boost += 5F*getBoostStrength(time);
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
						float f = dT*stalkerToyValue*0.001F*s.Happy.Value;
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
					ACUTheming.updateACUTheming(this, theme, changed);
				}
				nextIsDebug = false;
			}
		}
		
		private List<WaterParkPiece> getACUComponents(WaterPark acu) {
			List<WaterParkPiece> li = new List<WaterParkPiece>();
			foreach (WaterParkPiece wp in acu.transform.parent.GetComponentsInChildren<WaterParkPiece>()) {
				if (wp && wp.name.ToLowerInvariant().Contains("bottom") && wp.GetBottomPiece().GetModule() == acu)
					li.Add(wp);
			}
			return li;
		}
		
		private GameObject getACUFloor(List<WaterParkPiece> li) {
			foreach (WaterParkPiece wp in li) {
				if (wp.floorBottom && wp.floorBottom.activeSelf && wp.IsBottomPiece())
					return wp.floorBottom;
			}
			return null;
		}
	}
	
}
