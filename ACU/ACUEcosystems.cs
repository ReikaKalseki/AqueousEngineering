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
		private static readonly Dictionary<VanillaFlora, PlantFood> ediblePlants = new Dictionary<VanillaFlora, PlantFood>();
		
		 private static readonly Dictionary<TechType, ACUMetabolism> metabolisms = new Dictionary<TechType, ACUMetabolism>() {
			{TechType.RabbitRay, new ACUMetabolism(0.01F, 0.1F, false, BiomeRegions.RegionType.Shallows)},
			{TechType.Biter, new ACUMetabolism(0.01F, 0.2F, true, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Other)},
			{TechType.Blighter, new ACUMetabolism(0.005F, 0.1F, true, BiomeRegions.RegionType.BloodKelp)},
			{TechType.Gasopod, new ACUMetabolism(0.05F, 0.4F, false, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.Other)},
			{TechType.Jellyray, new ACUMetabolism(0.04F, 0.3F, false, BiomeRegions.RegionType.Mushroom)},
	    	{TechType.Stalker, new ACUMetabolism(0.05F, 0.5F, true, BiomeRegions.RegionType.Kelp)},
	    	{TechType.Sandshark, new ACUMetabolism(0.03F, 0.6F, true, BiomeRegions.RegionType.RedGrass)},
	    	{TechType.BoneShark, new ACUMetabolism(0.03F, 0.8F, true, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Mushroom, BiomeRegions.RegionType.Other)},
	    	{TechType.Shocker, new ACUMetabolism(0.1F, 0.5F, true, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.BloodKelp)},
	    	{TechType.Crabsnake, new ACUMetabolism(0.08F, 1F, true, BiomeRegions.RegionType.Jellyshroom)},
	    	{TechType.CrabSquid, new ACUMetabolism(0.15F, 1F, true, BiomeRegions.RegionType.BloodKelp, BiomeRegions.RegionType.LostRiver, BiomeRegions.RegionType.GrandReef)},
	    	{TechType.LavaLizard, new ACUMetabolism(0.05F, 0.5F, true, BiomeRegions.RegionType.LavaZone)},
	    	{TechType.SpineEel, new ACUMetabolism(0.03F, 1.5F, true, BiomeRegions.RegionType.LostRiver)},
			{TechType.GhostRayBlue, new ACUMetabolism(0.033F, 0.3F, false, BiomeRegions.RegionType.LostRiver)},
			{TechType.GhostRayRed, new ACUMetabolism(0.06F, 0.3F, false, BiomeRegions.RegionType.LavaZone)},
	    };
		
		static ACUEcosystems() {
			addFood(new AnimalFood(TechType.Reginald, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.BloodKelp, BiomeRegions.RegionType.LostRiver, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Peeper, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Mushroom, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.HoleFish, BiomeRegions.RegionType.Shallows));
			addFood(new AnimalFood(TechType.Oculus, BiomeRegions.RegionType.Jellyshroom));
			addFood(new AnimalFood(TechType.GarryFish, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Boomerang, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Spadefish, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Mushroom, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Bladderfish, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Mushroom, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.LostRiver, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Eyeye, BiomeRegions.RegionType.Jellyshroom, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Koosh));
			addFood(new AnimalFood(TechType.LavaEyeye, BiomeRegions.RegionType.LavaZone));
			addFood(new AnimalFood(TechType.LavaBoomerang, BiomeRegions.RegionType.LavaZone));
			addFood(new AnimalFood(TechType.Hoopfish, BiomeRegions.RegionType.Kelp, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Other));
			addFood(new AnimalFood(TechType.Spinefish, BiomeRegions.RegionType.BloodKelp, BiomeRegions.RegionType.LostRiver));
			addFood(new AnimalFood(TechType.Hoverfish, BiomeRegions.RegionType.Kelp));
			
			addFood(new PlantFood(VanillaFlora.CREEPVINE, 0.15F, BiomeRegions.RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.25F, BiomeRegions.RegionType.Kelp));
			addFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.25F, BiomeRegions.RegionType.BloodKelp));
			addFood(new PlantFood(VanillaFlora.JELLYSHROOM, 0.25F, BiomeRegions.RegionType.Jellyshroom));
			addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.15F, BiomeRegions.RegionType.BloodKelp, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, BiomeRegions.RegionType.LostRiver));
			addFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, BiomeRegions.RegionType.Koosh));
			addFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, BiomeRegions.RegionType.GrandReef));
			addFood(new PlantFood(VanillaFlora.PAPYRUS, 0.15F, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Jellyshroom, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.VIOLET_BEAU, 0.2F, BiomeRegions.RegionType.Jellyshroom, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.CAVE_BUSH, 0.05F, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Jellyshroom, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, BiomeRegions.RegionType.GrandReef, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.REDWORT, 0.15F, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.ROUGE_CRADLE, 0.05F, BiomeRegions.RegionType.RedGrass, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.SEACROWN, 0.4F, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.RedGrass));
			addFood(new PlantFood(VanillaFlora.SPOTTED_DOCKLEAF, 0.25F, BiomeRegions.RegionType.Koosh, BiomeRegions.RegionType.Other));
			addFood(new PlantFood(VanillaFlora.VEINED_NETTLE, 0.15F, BiomeRegions.RegionType.Shallows));
			addFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.BLUE_PALM, 0.25F, BiomeRegions.RegionType.Shallows, BiomeRegions.RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.PYGMY_FAN, 0.33F, BiomeRegions.RegionType.Mushroom));
			addFood(new PlantFood(VanillaFlora.TIGER, 0.5F, BiomeRegions.RegionType.RedGrass));
			addFood(new PlantFood(VanillaFlora.DEEP_MUSHROOM, 0.1F, BiomeRegions.RegionType.LostRiver, BiomeRegions.RegionType.LavaZone));
		}
		
		private static void addFood(Food f) {
			if (f is AnimalFood) {
				edibleFish[((AnimalFood)f).item] = (AnimalFood)f;
			}
			else if (f is PlantFood) {
				ediblePlants[((PlantFood)f).plant] = (PlantFood)f;
			}
		}
		
		internal static Creature handleCreature(ACUCallbackSystem.ACUCallback acu, float dT, WaterParkItem wp, TechType tt, List<WaterParkCreature> foodFish, PrefabIdentifier[] plants, HashSet<BiomeRegions.RegionType> possibleBiomes) {
			if (edibleFish.ContainsKey(tt)) {
				if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
					acu.sparkleCount++;
				else //sparkle peepers are always valid
					possibleBiomes = new HashSet<BiomeRegions.RegionType>(possibleBiomes.Intersect(edibleFish[tt].regionType));
				//if (possibleBiomes.Count <= 0)
				//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
				//SNUtil.writeToChat(tt+" > "+edibleFish[tt]+" > "+string.Join(",", possibleBiomes));
				foodFish.Add((WaterParkCreature)wp);
				acu.herbivoreCount++;
			}
			else if (metabolisms.ContainsKey(tt)) {
				ACUMetabolism am = metabolisms[tt];
				if (am.isCarnivore)
					acu.carnivoreCount++;
				else
					acu.herbivoreCount += tt == TechType.Gasopod ? 4 : (tt == TechType.GhostRayRed || tt == TechType.GhostRayBlue ? 3 : 2);
				List<BiomeRegions.RegionType> li = new List<BiomeRegions.RegionType>(am.additionalRegions);
				li.Add(am.primaryRegion);
				possibleBiomes = new HashSet<BiomeRegions.RegionType>(possibleBiomes.Intersect(li));
				//SNUtil.writeToChat(tt+" > "+am+" > "+string.Join(",", possibleBiomes));
				//if (possibleBiomes.Count <= 0)
				//	SNUtil.writeToChat("Biome list empty after "+tt+" > "+am);
				Creature c = wp.gameObject.GetComponentInChildren<Creature>();
				c.Hunger.Add(dT*am.metabolismPerSecond*FOOD_SCALAR);
				c.Hunger.Falloff = 0;
				if (c.Hunger.Value >= 0.5F) {
					eat(acu, wp, c, am, plants);
				}
				return c;
			}
			return null;
		}
		
		internal static HashSet<VanillaFlora> collectPlants(ACUCallbackSystem.ACUCallback acu, PrefabIdentifier[] plants, HashSet<BiomeRegions.RegionType> possibleBiomes) {
			HashSet<VanillaFlora> set = new HashSet<VanillaFlora>();
			foreach (PrefabIdentifier pi in plants) {
				if (pi) {
					VanillaFlora vf = VanillaFlora.getFromID(pi.ClassId);
					if (vf != null && ediblePlants.ContainsKey(vf)) {
						PlantFood pf = ediblePlants[vf];
						possibleBiomes = new HashSet<BiomeRegions.RegionType>(possibleBiomes.Intersect(pf.regionType));
						//if (possibleBiomes.Count <= 0)
						//	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
						//SNUtil.writeToChat(vf+" > "+pf+" > "+string.Join(",", possibleBiomes));
						set.Add(vf);
						acu.plantCount++;
					}
				}
			}
			return set;
		}
		
		private static void eat(ACUCallbackSystem.ACUCallback acu, WaterParkItem wp, Creature c, ACUMetabolism am, PrefabIdentifier[] plants) {
			Food amt;
			GameObject eaten;
			if (tryEat(acu, c, am, plants, out amt, out eaten)) {
				ACUEcosystems.onEaten(acu, wp, c, am, amt, eaten);
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
						//SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt);
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
					VanillaFlora vf = VanillaFlora.getFromID(tt.ClassId);
					//SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
					if (vf != null && ediblePlants.ContainsKey(vf)) {
						amt = ediblePlants[vf];
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
		
		private static void onEaten(ACUCallbackSystem.ACUCallback acu, WaterParkItem wp, Creature c, ACUMetabolism am, Food amt, GameObject eaten) {
			float food = amt.foodValue*FOOD_SCALAR;
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
				food *= 0.25F;
				c.gameObject.EnsureComponent<InfectedMixin>().IncreaseInfectedAmount(0.2F);
			}
			if (c.Hunger.Value >= food) {
				c.Happy.Add(0.25F);
				c.Hunger.Add(-food);
				float f = am.normalizedPoopChance*amt.foodValue*Mathf.Pow(((WaterParkCreature)wp).age, 2F);
				f *= AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.POO_RATE);
				//SNUtil.writeToChat(c+" ate > "+f);
				amt.consume(c, acu.acu, acu.sc, eaten);
				if (f > 0 && UnityEngine.Random.Range(0F, 1F) < f) {
					GameObject poo = ObjectUtil.createWorldObject(AqueousEngineeringMod.poo.ClassID);
					poo.transform.position = c.transform.position+Vector3.down*0.05F;
					poo.transform.rotation = UnityEngine.Random.rotationUniform;
					//SNUtil.writeToChat("Poo spawned");
				}
			}
		}
		
		abstract class Food {
			
			internal readonly float foodValue;
			internal readonly HashSet<BiomeRegions.RegionType> regionType = new HashSet<BiomeRegions.RegionType>();
			
			internal Food(float f, params BiomeRegions.RegionType[] r) {
				foodValue = f;
				regionType.AddRange(r.ToList());
			}
			
			internal bool isRegion(BiomeRegions.RegionType r) {
				return regionType.Contains(r);
			}
			
			public override string ToString()
			{
				return string.Format("[Food FoodValue={0}, BiomeRegions.RegionType=[{1}]]", foodValue, string.Join(",", regionType));
			}
			
			internal abstract void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go);
		}
		
		class AnimalFood : Food {
			
			internal readonly TechType item;
			
			internal AnimalFood(TechType tt, params BiomeRegions.RegionType[] r) : base(calculateFoodValue(tt), r) {
				item = tt;
			}
			
			static float calculateFoodValue(TechType tt) {
				GameObject go = CraftData.GetPrefabForTechType(SNUtil.getTechType("Cooked"+tt));
				Eatable ea = go.GetComponent<Eatable>();
				return ea.foodValue*0.01F; //so a reginald is ~40%
			}
			
			internal override void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go) {
				acu.RemoveItem(go.GetComponent<WaterParkCreature>());
				UnityEngine.Object.DestroyImmediate(go);
			}
			
		}
		
		class PlantFood : Food {
			
			internal readonly VanillaFlora plant;
			
			internal PlantFood(VanillaFlora vf, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
				plant = vf;
			}
			
			internal override void consume(Creature c, WaterPark acu, StorageContainer sc, GameObject go) {
				LiveMixin lv = go.GetComponent<LiveMixin>();
				if (lv && lv.IsAlive())
					lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
				else
					sc.container.DestroyItem(CraftData.GetTechType(go));
			}
			
		}
		
		class ACUMetabolism {
			
			internal readonly bool isCarnivore;
			internal readonly float metabolismPerSecond;
			internal readonly float normalizedPoopChance;
			internal readonly BiomeRegions.RegionType primaryRegion;
			internal readonly HashSet<BiomeRegions.RegionType> additionalRegions = new HashSet<BiomeRegions.RegionType>();
			
			internal ACUMetabolism(float mf, float pp, bool isc, BiomeRegions.RegionType r, params BiomeRegions.RegionType[] rr) {
				normalizedPoopChance = pp*2;
				metabolismPerSecond = mf*0.033F;
				isCarnivore = isc;
				primaryRegion = r;
				additionalRegions.AddRange(rr.ToList());
			}
			
			public override string ToString()
			{
				return string.Format("[ACUMetabolism IsCarnivore={0}, MetabolismPerSecond={1}, NormalizedPoopChance={2}, PrimaryRegion={3}, AdditionalRegions=[{4}]]]", isCarnivore, metabolismPerSecond.ToString("0.0000"), normalizedPoopChance, primaryRegion, string.Join(",", additionalRegions));
			}			
		}
	}
	
}
