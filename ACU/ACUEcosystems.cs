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
	
	public static class ACUEcosystems {
		
		internal static readonly float FOOD_SCALAR = 0.2F; //all food values and metabolism multiplied by this, to give granularity
		
		private static readonly Dictionary<TechType, AnimalFood> edibleFish = new Dictionary<TechType, AnimalFood>();		
		private static readonly Dictionary<string, PlantFood> ediblePlants = new Dictionary<string, PlantFood>();
		
		 private static readonly Dictionary<TechType, ACUMetabolism> metabolisms = new Dictionary<TechType, ACUMetabolism>() {
			{TechType.RabbitRay, new ACUMetabolism(2F, 0.2F, 0.2F, false, BiomeRegions.Shallows)},
			{TechType.Crash, new ACUMetabolism(1.0F, 0.1F, 0.8F, true, BiomeRegions.Shallows)},
			{TechType.Biter, new ACUMetabolism(0.5F, 0.2F, 0.4F, true, BiomeRegions.RedGrass, BiomeRegions.Other)},
			{TechType.Blighter, new ACUMetabolism(0.33F, 0.1F, 0.2F, true, BiomeRegions.BloodKelp)},
			{TechType.Gasopod, new ACUMetabolism(4F, 1F, 0.8F, false, BiomeRegions.Shallows, BiomeRegions.Other)},
			{TechType.Jellyray, new ACUMetabolism(2.5F, 0.8F, 0.6F, false, BiomeRegions.Mushroom)},
	    	{TechType.Stalker, new ACUMetabolism(0.75F, 1F, 1F, true, BiomeRegions.Kelp)},
	    	{TechType.Sandshark, new ACUMetabolism(0.67F, 0.6F, 1.2F, true, BiomeRegions.RedGrass)},
	    	{TechType.BoneShark, new ACUMetabolism(1.5F, 0.6F, 1.6F, true, BiomeRegions.Koosh, BiomeRegions.Mushroom, BiomeRegions.Other)},
	    	{TechType.Shocker, new ACUMetabolism(1F, 2F, 1F, true, BiomeRegions.Koosh, BiomeRegions.BloodKelp)},
	    	{TechType.Crabsnake, new ACUMetabolism(1.25F, 1.6F, 2F, true, BiomeRegions.Jellyshroom)},
	    	{TechType.CrabSquid, new ACUMetabolism(1.5F, 3F, 2F, true, BiomeRegions.BloodKelp, BiomeRegions.LostRiver, BiomeRegions.GrandReef)},
	    	{TechType.LavaLizard, new ACUMetabolism(1F, 1F, 1F, true, BiomeRegions.LavaZone)},
	    	{TechType.SpineEel, new ACUMetabolism(0.75F, 0.6F, 3F, true, BiomeRegions.LostRiver)},
			{TechType.GhostRayBlue, new ACUMetabolism(3F, 0.67F, 0.6F, false, BiomeRegions.LostRiver)},
			{TechType.GhostRayRed, new ACUMetabolism(3F, 1.25F, 0.6F, false, BiomeRegions.LavaZone)},
			{TechType.Mesmer, new ACUMetabolism(0.5F, 0.1F, 0.7F, true, BiomeRegions.Koosh, BiomeRegions.LostRiver)},
	    };
		
		static ACUEcosystems() {
			addFood(new AnimalFood(TechType.Reginald, BiomeRegions.RedGrass, BiomeRegions.BloodKelp, BiomeRegions.LostRiver, BiomeRegions.GrandReef, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Peeper, BiomeRegions.Shallows, BiomeRegions.RedGrass, BiomeRegions.Mushroom, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.HoleFish, BiomeRegions.Shallows));
			addFood(new AnimalFood(TechType.Oculus, BiomeRegions.Jellyshroom));
			addFood(new AnimalFood(TechType.GarryFish, BiomeRegions.Shallows, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Boomerang, BiomeRegions.Shallows, BiomeRegions.RedGrass, BiomeRegions.Koosh, BiomeRegions.GrandReef, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Spadefish, BiomeRegions.RedGrass, BiomeRegions.GrandReef, BiomeRegions.Mushroom, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Bladderfish, BiomeRegions.Shallows, BiomeRegions.RedGrass, BiomeRegions.Mushroom, BiomeRegions.GrandReef, BiomeRegions.LostRiver, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Eyeye, BiomeRegions.Jellyshroom, BiomeRegions.GrandReef, BiomeRegions.Koosh));
			addFood(new AnimalFood(TechType.LavaEyeye, BiomeRegions.LavaZone));
			addFood(new AnimalFood(TechType.LavaBoomerang, BiomeRegions.LavaZone));
			addFood(new AnimalFood(TechType.Hoopfish, BiomeRegions.Kelp, BiomeRegions.Koosh, BiomeRegions.GrandReef, BiomeRegions.Other));
			addFood(new AnimalFood(TechType.Spinefish, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));
			addFood(new AnimalFood(TechType.Hoverfish, BiomeRegions.Kelp));
			
			addFood(new PlantFood(VanillaFlora.CREEPVINE, 0.15F, BiomeRegions.Kelp));
			addFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.25F, BiomeRegions.Kelp));
			addFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.25F, BiomeRegions.BloodKelp));
			addFood(new PlantFood(VanillaFlora.JELLYSHROOM_SMALL, 0.25F, BiomeRegions.Jellyshroom));
			addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.15F, BiomeRegions.BloodKelp, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, BiomeRegions.LostRiver));
			addFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, BiomeRegions.Koosh));
			addFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, BiomeRegions.GrandReef));
			addFood(new PlantFood(VanillaFlora.PAPYRUS, 0.15F, BiomeRegions.RedGrass, BiomeRegions.Jellyshroom, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.VIOLET_BEAU, 0.2F, BiomeRegions.Jellyshroom, BiomeRegions.RedGrass, BiomeRegions.Koosh, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.CAVE_BUSH, 0.05F, BiomeRegions.Koosh, BiomeRegions.Jellyshroom, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, BiomeRegions.GrandReef, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.REDWORT, 0.15F, BiomeRegions.RedGrass, BiomeRegions.Koosh, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.ROUGE_CRADLE, 0.05F, BiomeRegions.RedGrass, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.SEACROWN, 0.4F, BiomeRegions.Koosh, BiomeRegions.RedGrass));
			addFood(new PlantFood(VanillaFlora.SPOTTED_DOCKLEAF, 0.25F, BiomeRegions.Koosh, BiomeRegions.Other));
			addFood(new PlantFood(VanillaFlora.VEINED_NETTLE, 0.15F, BiomeRegions.Shallows));
			addFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
			addFood(new PlantFood(VanillaFlora.BLUE_PALM, 0.25F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
			addFood(new PlantFood(VanillaFlora.PYGMY_FAN, 0.33F, BiomeRegions.Mushroom));
			addFood(new PlantFood(VanillaFlora.TIGER, 0.5F, BiomeRegions.RedGrass));
			addFood(new PlantFood(VanillaFlora.DEEP_MUSHROOM, 0.1F, BiomeRegions.LostRiver, BiomeRegions.LavaZone));
		}
		
		public static void addPost() {
			TechType tt = SNUtil.getTechType("StellarThalassacean");
			if (tt != TechType.None)
				addPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other);
			
			tt = SNUtil.getTechType("JasperThalassacean");
			if (tt != TechType.None)
				addPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.LostRiver);
			
			tt = SNUtil.getTechType("Twisteel");
			if (tt != TechType.None)
				addPredatorType(tt, 2F, 0.5F, 0.8F, true, BiomeRegions.BloodKelp, BiomeRegions.Koosh);
			
			tt = SNUtil.getTechType("JellySpinner");
			if (tt != TechType.None)
				addFood(new AnimalFood(tt, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));
			
			tt = SNUtil.getTechType("TriangleFish");
			if (tt != TechType.None)
				addFood(new AnimalFood(tt, BiomeRegions.Shallows));
			
			tt = SNUtil.getTechType("Axetail");
			if (tt != TechType.None)
				addFood(new AnimalFood(tt, BiomeRegions.RedGrass));
			
			tt = SNUtil.getTechType("RibbonRay");
			if (tt != TechType.None)
				addFood(new AnimalFood(tt, BiomeRegions.Shallows, BiomeRegions.Mushroom));
			
			tt = SNUtil.getTechType("GrandGlider");
			if (tt != TechType.None) {
				addFood(new AnimalFood(tt, 2, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other));
				addPredatorType(tt, 3.0F, 0.8F, 0.75F, false, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other);
			}
			
			tt = SNUtil.getTechType("Filtorb");
			if (tt != TechType.None)
				addFood(new AnimalFood(tt, 0.1F, BiomeRegions.Shallows, BiomeRegions.RedGrass, BiomeRegions.GrandReef, BiomeRegions.Other));
			
			addClownPincher("EmeraldClownPincher", BiomeRegions.Kelp);
			addClownPincher("SapphireClownPincher", BiomeRegions.GrandReef);
			addClownPincher("RubyClownPincher", BiomeRegions.LavaZone);
			addClownPincher("AmberClownPincher", BiomeRegions.Other);
			addClownPincher("CitrineClownPincher", BiomeRegions.Other);
			
			tt = SNUtil.getTechType("GulperLeviathanBaby");
			if (tt != TechType.None)
				addPredatorType(tt, 5F, 4F, 0.2F, true, BiomeRegions.GrandReef);
			tt = SNUtil.getTechType("GulperLeviathan");
			if (tt != TechType.None)
				addPredatorType(tt, 8F, 8F, 0.2F, true, BiomeRegions.BloodKelp, BiomeRegions.Other);
		}
		
		private static void addClownPincher(string id, BiomeRegions.RegionType br) {
			TechType tt = SNUtil.getTechType(id);
			if (tt != TechType.None) {
				addFood(new AnimalFood(tt, br));
				addPredatorType(tt, 1.5F, 2F, 1.6F, false, br);
			}
		}
		
		public static void addPredatorType(TechType tt, float relativeValue, float metaRate, float pooChance, bool carn, params BiomeRegions.RegionType[] rr) {
			List<BiomeRegions.RegionType> li = rr.ToList();
			li.RemoveAt(0);
			ACUMetabolism am = new ACUMetabolism(relativeValue, metaRate, pooChance, carn, rr[0], li);
			metabolisms[tt] = am;
		}
		
		public static void addFood(Food f) {
			if (f is AnimalFood) {
				edibleFish[((AnimalFood)f).item] = (AnimalFood)f;
			}
			else if (f is PlantFood) {
				foreach (string s in ((PlantFood)f).classIDs)
					ediblePlants[s] = (PlantFood)f;
			}
		}
		
		public static ACUMetabolism getMetabolismForAnimal(TechType tt) {
			return metabolisms.ContainsKey(tt) ? metabolisms[tt] : null;
		}
		
		public static AnimalFood getAnimalFood(TechType tt) {
			return edibleFish.ContainsKey(tt) ? edibleFish[tt] : null;
		}
		
		public static PlantFood getPlantFood(string pfb) {
			return ediblePlants.ContainsKey(pfb) ? ediblePlants[pfb] : null;
		}
		
		public static List<PlantFood> getPlantsForBiome(BiomeRegions.RegionType r) {
			List<PlantFood> li = new List<PlantFood>();
			foreach (PlantFood f in ediblePlants.Values) {
				if (f.isRegion(r))
					li.Add(f);
			}
			return li;
		}
		
		public static List<AnimalFood> getSmallFishForBiome(BiomeRegions.RegionType r) {
			List<AnimalFood> li = new List<AnimalFood>();
			foreach (AnimalFood f in edibleFish.Values) {
				if (f.isRegion(r))
					li.Add(f);
			}
			return li;
		}
		
		public static List<TechType> getPredatorsForBiome(BiomeRegions.RegionType r) {
			List<TechType> li = new List<TechType>();
			foreach (KeyValuePair<TechType, ACUMetabolism> kvp in metabolisms) {
				if (kvp.Value.isRegion(r, false))
					li.Add(kvp.Key);
			}
			return li;
		}
		
		internal static Creature handleCreature(ACUCallbackSystem.ACUCallback acu, float dT, WaterParkCreature wp, TechType tt, List<WaterParkCreature> foodFish, PrefabIdentifier[] plants, bool acuRoom, HashSet<BiomeRegions.RegionType> possibleBiomes) {
			if (edibleFish.ContainsKey(tt)) {
				if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
					acu.sparkleCount++;
				else if (tt == TechType.Cutefish)
					acu.cuddleCount++;
				else if (tt == TechType.Gasopod)
					acu.gasopodCount++;
				else //sparkle peepers and cuddlefish are always valid
					possibleBiomes.IntersectWith(edibleFish[tt].regionType);
				//if (possibleBiomes.Count <= 0)
				//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
				if (acu.nextIsDebug)
					SNUtil.writeToChat(tt+" > "+edibleFish[tt]+" > "+string.Join(",", possibleBiomes));
				foodFish.Add(wp);
				acu.herbivoreCount++;
			}
			else if (metabolisms.ContainsKey(tt)) {
				ACUMetabolism am = metabolisms[tt];
				if (am.isCarnivore)
					acu.carnivoreCount += am.relativeValue;
				else
					acu.herbivoreCount += am.relativeValue;
				List<BiomeRegions.RegionType> li = new List<BiomeRegions.RegionType>(am.additionalRegions);
				li.Add(am.primaryRegion);
				possibleBiomes.IntersectWith(li);
				if (acu.nextIsDebug)
					SNUtil.writeToChat(tt+" > "+am+" > "+string.Join(",", possibleBiomes));
				//if (possibleBiomes.Count <= 0)
				//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+am);
				Creature c = wp.gameObject.GetComponentInChildren<Creature>();
				if (wp.isMature) {
					c.Hunger.Add(dT*am.metabolismPerSecond*FOOD_SCALAR);
					c.Hunger.Falloff = 0;
					if (c.Hunger.Value >= 0.5F) {
						eat(acu, wp, c, am, plants, acuRoom);
					}
				}
				return c;
			}
			return null;
		}
		
		internal static HashSet<PlantFood> collectPlants(ACUCallbackSystem.ACUCallback acu, PrefabIdentifier[] plants, HashSet<BiomeRegions.RegionType> possibleBiomes) {
			HashSet<PlantFood> set = new HashSet<PlantFood>();
			foreach (PrefabIdentifier pi in plants) {
				if (pi) {
					if (ediblePlants.ContainsKey(pi.ClassId)) {
						PlantFood pf = ediblePlants[pi.ClassId];
						possibleBiomes.IntersectWith(pf.regionType);
						//if (possibleBiomes.Count <= 0)
						//	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
						if (acu.nextIsDebug)
							SNUtil.writeToChat(pi+" > "+pf+" & "+string.Join(",", pf.regionType)+" > "+string.Join(",", possibleBiomes));
						set.Add(pf);
						acu.plantCount += getPlantValue(pi);
					}
				}
			}
			return set;
		}
		
		private static float getPlantValue(PrefabIdentifier pi) {
			if (VanillaFlora.WRITHING_WEED.includes(pi.ClassId) || VanillaFlora.GELSACK.includes(pi.ClassId))
				return 0.5F;
			if (VanillaFlora.ACID_MUSHROOM.includes(pi.ClassId) || VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId))
				return 0.33F;
			if (VanillaFlora.BLOOD_KELP.includes(pi.ClassId) || VanillaFlora.CREEPVINE.includes(pi.ClassId) || VanillaFlora.CREEPVINE_FERTILE.includes(pi.ClassId))
				return 2.5F;
			BasicCustomPlant bp = BasicCustomPlant.getPlant(pi.ClassId);
			if (bp != null)
				return bp.getSize() == Plantable.PlantSize.Large ? 1 : 0.5F;
			return 1;
		}
		
		private static void eat(ACUCallbackSystem.ACUCallback acu, WaterParkCreature wp, Creature c, ACUMetabolism am, PrefabIdentifier[] plants, bool acuRoom) {
			Food amt;
			GameObject eaten;
			if (tryEat(acu, c, am, plants, out amt, out eaten)) {
				ACUEcosystems.onEaten(acu, wp, c, am, amt, eaten, acuRoom);
			}
		}
		
		private static bool tryEat(ACUCallbackSystem.ACUCallback acu, Creature c, ACUMetabolism am, PrefabIdentifier[] pia, out Food amt, out GameObject eaten) {
			if (am.isCarnivore) {
				WaterParkItem wp = acu.acu.items[UnityEngine.Random.Range(0, acu.acu.items.Count)];
				if (wp) {
					Pickupable pp = wp.gameObject.GetComponentInChildren<Pickupable>();
					TechType tt = pp ? pp.GetTechType() : TechType.None;
					if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero) { //do not allow eating sparkle peepers
						amt = null;
						eaten = null;
						return false;
					}
					//SNUtil.writeToChat(pp+" > "+tt+" > "+edibleFish.ContainsKey(tt));
					if (edibleFish.ContainsKey(tt)) {
						eaten = pp.gameObject;
						amt = edibleFish[tt];
						//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt+", is now "+c.Hunger.Value);
						return true;
					}
				}
				amt = null;
				eaten = null;
				return false;
			}
			else if (pia.Length > 0) {
				int idx = UnityEngine.Random.Range(0, pia.Length);
				PrefabIdentifier tt = pia[idx];
				if (tt) {
					//SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
					if (ediblePlants.ContainsKey(tt.ClassId)) {
						amt = ediblePlants[tt.ClassId];
						//SNUtil.writeToChat(c+" ate a "+vf+" and got "+amt);
						eaten = tt.gameObject;
						return true;
					}
				}
			}
			amt = null;
			eaten = null;
			return false;
		}
		
		private static void onEaten(ACUCallbackSystem.ACUCallback acu, WaterParkCreature wp, Creature c, ACUMetabolism am, Food amt, GameObject eaten, bool acuRoom) {
			float food = amt.foodValue*FOOD_SCALAR*2.5F;
			if (acuRoom)
				food *= 1.2F;
			if (amt.isRegion(am.primaryRegion)) {
				food *= 3;
			}
			else {
				foreach (BiomeRegions.RegionType r in am.additionalRegions) {
					if (amt.isRegion(r)) {
						food *= 2;
						break;
					}
				}
			}
			InfectedMixin inf = eaten ? eaten.GetComponent<InfectedMixin>() : null;
			if (inf && inf.IsInfected()) {
				food *= 0.4F;
				c.gameObject.EnsureComponent<InfectedMixin>().IncreaseInfectedAmount(0.2F);
			}
			if (c.Hunger.Value >= food) {
				c.Happy.Add(1F);
				c.Hunger.Add(-food);
				float f = am.normalizedPoopChance*amt.foodValue*Mathf.Pow(wp.age, 2F);
				f *= AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.POO_RATE);
				if (acuRoom)
					f *= 1.5F;
				//SNUtil.writeToChat(c+" ate > "+f);
				amt.consume(c, acu, acu.planter, eaten);
				if (f > 0 && UnityEngine.Random.Range(0F, 1F) < f) {
					GameObject poo = ObjectUtil.createWorldObject(AqueousEngineeringMod.poo.ClassID);
					poo.transform.position = c.transform.position+Vector3.down*0.05F;
					poo.transform.rotation = UnityEngine.Random.rotationUniform;
					acu.acu.AddItem(poo.GetComponent<Pickupable>());
					//SNUtil.writeToChat("Poo spawned");
				}
				ACUCallbackSystem.CreatureCache cache = acu.getOrCreateCreatureStatus(c);
				if (cache != null) {
					cache.hunger = c.Hunger.Value;
					cache.happy = c.Happy.Value;
				}
			}
		}
		
		public abstract class Food {
			
			public readonly float foodValue;
			internal readonly HashSet<BiomeRegions.RegionType> regionType = new HashSet<BiomeRegions.RegionType>();
			
			internal Food(float f, params BiomeRegions.RegionType[] r) {
				foodValue = f;
				regionType.AddRange(r.ToList());
			}
			
			public bool isRegion(BiomeRegions.RegionType r) {
				return regionType.Contains(r);
			}
			
			public override string ToString()
			{
				return string.Format("[Food FoodValue={0}, BiomeRegions.RegionType=[{1}]]", foodValue, string.Join(",", regionType));
			}
			
			public void addBiome(BiomeRegions.RegionType r) {
				regionType.Add(r);
			}
			
			internal abstract void consume(Creature c, ACUCallbackSystem.ACUCallback acu, StorageContainer sc, GameObject go);
		}
		
		public class AnimalFood : Food {
			
			internal readonly TechType item;
			
			public AnimalFood(Spawnable s, params BiomeRegions.RegionType[] r) : this(s.TechType, r) {
				
			}
			
			internal AnimalFood(TechType tt, params BiomeRegions.RegionType[] r) : base(calculateFoodValue(tt), r) {
				item = tt;
			}
			
			internal AnimalFood(TechType tt, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
				item = tt;
			}
			
			public AnimalFood(Spawnable s, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
				item = s.TechType;
			}
			
			public static float calculateFoodValue(TechType tt) {
				GameObject go = ObjectUtil.lookupPrefab(SNUtil.getTechType("Cooked"+tt));
				if (!go)
					go = ObjectUtil.lookupPrefab(SNUtil.getTechType(tt+"Cooked"));
				Eatable ea = go ? go.GetComponent<Eatable>() : null;
				return ea ? ea.foodValue*0.01F : 0.2F; //so a reginald is ~40%
			}
			
			internal override void consume(Creature c, ACUCallbackSystem.ACUCallback acu, StorageContainer sc, GameObject go) {
				acu.acu.RemoveItem(go.GetComponent<WaterParkCreature>());
				UnityEngine.Object.DestroyImmediate(go);
			}
			
		}
		
		public class PlantFood : Food {
			
			internal readonly HashSet<string> classIDs = new HashSet<string>();
			
			internal PlantFood(VanillaFlora vf, float f, params BiomeRegions.RegionType[] r) : this(vf.getPrefabs(true, true), f, r) {
				
			}
			
			public PlantFood(Spawnable sp, float f, params BiomeRegions.RegionType[] r) : this(new List<string>{sp.ClassID}, f, r) {
				
			}
			
			internal PlantFood(IEnumerable<string> ids, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
				classIDs.AddRange(ids);
			}
			
			internal override void consume(Creature c, ACUCallbackSystem.ACUCallback acu, StorageContainer sc, GameObject go) {
				if (UnityEngine.Random.Range(0F, 1F) <= acu.getBoostStrength(DayNightCycle.main.timePassedAsFloat))
					return;
				LiveMixin lv = go.GetComponent<LiveMixin>();
				if (lv && lv.IsAlive())
					lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
				else
					sc.container.DestroyItem(CraftData.GetTechType(go));
			}
			
		}
		
		public class ACUMetabolism {
			
			public readonly float relativeValue;
			public readonly bool isCarnivore;
			public readonly float metabolismPerSecond;
			public readonly float normalizedPoopChance;
			public readonly BiomeRegions.RegionType primaryRegion;
			internal readonly HashSet<BiomeRegions.RegionType> additionalRegions = new HashSet<BiomeRegions.RegionType>();
			
			internal ACUMetabolism(float v, float mf, float pp, bool isc, BiomeRegions.RegionType r, params BiomeRegions.RegionType[] rr) : this(v, mf, pp, isc, r, rr.ToList()) {
				
			}
			
			internal ACUMetabolism(float v, float mf, float pp, bool isc, BiomeRegions.RegionType r, List<BiomeRegions.RegionType> rr) {
				relativeValue = v;
				normalizedPoopChance = pp*6;
				metabolismPerSecond = mf*0.0003F;
				isCarnivore = isc;
				primaryRegion = r;
				additionalRegions.AddRange(rr);
			}
			
			public bool isRegion(BiomeRegions.RegionType r, bool primaryOnly) {
				return r == primaryRegion || (!primaryOnly && additionalRegions.Contains(r));
			}
			
			public void addBiome(BiomeRegions.RegionType r) {
				additionalRegions.Add(r);
			}
			
			public override string ToString()
			{
				return string.Format("[ACUMetabolism IsCarnivore={0}, MetabolismPerSecond={1}, NormalizedPoopChance={2}, PrimaryRegion={3}, AdditionalRegions=[{4}]]]", isCarnivore, metabolismPerSecond.ToString("0.0000"), normalizedPoopChance, primaryRegion, string.Join(",", additionalRegions));
			}			
		}
	}
	
}
