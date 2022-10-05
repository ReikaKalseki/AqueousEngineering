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
	
	public static class BiomeRegions {
		
		public enum RegionType {
			[Biome("SafeShallows", 1F, 1F, 1F, 0.3F)]Shallows,
			[Biome("KelpForest", 0.3F, 0.6F, 0.3F, 0.67F)]Kelp,
			[Biome("GrassyPlateaus", 1F, 1F, 1F, 0.3F)]RedGrass,
			[Biome("MushroomForest", 1F, 1F, 1F, 0.3F)]Mushroom,
			[Biome("JellyshroomCaves", 0.8F, 0.2F, 0.8F, 0.8F)]Jellyshroom,
			[Biome("KooshZone", 0.6F, 0.3F, 0.8F, 0.8F)]Koosh,
			[Biome("BloodKelp", 0, 0, 0, 0.95F)]BloodKelp,
			[Biome("GrandReef", 0, 0, 0.5F, 0.9F)]GrandReef,
			[Biome("lostriver_bonesfield", 0.1F, 0.5F, 0.2F, 0.92F)]LostRiver,
			[Biome("ilzchamber", 0.7F, 0.5F, 0.1F, 0.75F)]LavaZone,
			[Biome("Dunes", 0.1F, 0.4F, 0.7F, 0.5F)]Other,
		}
		
		internal class Biome : Attribute {
			
			public readonly string biomeName;
			internal readonly Color waterColor;
			
			internal Biome(string b, float r, float g, float bl, float a) {
				biomeName = b;
				waterColor = new Color(r, g, bl, a);
			}
			
			public override string ToString() {
				return biomeName;
			}
		}
		
		internal static Biome getAttr(RegionType key) {
			FieldInfo info = typeof(RegionType).GetField(Enum.GetName(typeof(RegionType), key));
			return (Biome)Attribute.GetCustomAttribute(info, typeof(Biome));
		}
	}
	
}
