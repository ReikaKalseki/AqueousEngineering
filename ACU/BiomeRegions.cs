using System;

using System.Collections;
using System.Collections.ObjectModel;
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
	
	public static class BiomeRegions {
		
		private static readonly Dictionary<string, RegionType> biomeList = new Dictionary<string, RegionType>();
		
		public static readonly RegionType Shallows = new RegionType("Shallows", "SafeShallows", 1F, 1F, 1F, 0.3F);
		public static readonly RegionType Kelp = new RegionType("Kelp", "KelpForest", 0.3F, 0.6F, 0.3F, 0.67F);
		public static readonly RegionType RedGrass = new RegionType("RedGrass", "GrassyPlateaus", 1F, 1F, 1F, 0.3F);
		public static readonly RegionType Mushroom = new RegionType("Mushroom", "MushroomForest", 1F, 1F, 1F, 0.3F);
		public static readonly RegionType Jellyshroom = new RegionType("Jellyshroom", "JellyshroomCaves", 0.8F, 0.2F, 0.8F, 0.8F);
		public static readonly RegionType Koosh = new RegionType("Koosh", "KooshZone", 0.6F, 0.3F, 0.8F, 0.8F);
		public static readonly RegionType BloodKelp = new RegionType("BloodKelp", "BloodKelp", 0, 0, 0, 0.95F);
		public static readonly RegionType GrandReef = new RegionType("GrandReef", "GrandReef", 0, 0, 0.5F, 0.9F);
		public static readonly RegionType LostRiver = new RegionType("LostRiver", "lostriver_bonesfield", 0.1F, 0.5F, 0.2F, 0.92F);
		public static readonly RegionType LavaZone = new RegionType("LavaZone", "ilzchamber", 0.7F, 0.5F, 0.1F, 0.75F);
		public static readonly RegionType Other = new RegionType("Other", "Dunes", 0.1F, 0.4F, 0.7F, 0.5F);
		
		public static IEnumerable<RegionType> getAllBiomes() {
			return new ReadOnlyCollection<RegionType>(biomeList.Values.ToList());
		}
		
		public class RegionType {
			
			public readonly string ID;
			public readonly string baseBiome;
			internal readonly Color waterColor;
			
			public RegionType(string id, string b, float r, float g, float bl, float a) : this(id, b, new Color(r, g, bl, a)) {

			}
			
			public RegionType(string id, string b, Color c) {
				ID = id;
				baseBiome = b;
				waterColor = c;
				biomeList[id] = this;
			}
			
			public string getName() {
				return BiomeBase.getBiome(baseBiome).displayName;
			}
			
			public override string ToString() {
				return baseBiome;
			}
		}
	}
	
}
