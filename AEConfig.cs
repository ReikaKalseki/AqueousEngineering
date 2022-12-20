using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering
{
	public class AEConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("ACU feces drop rate", typeof(float), 1F, 0, 4, 0)]POO_RATE,
			[ConfigEntry("Seabase Battery power cell storage equivalence", typeof(int), 2, 1, 5, 0)]BATTCELLS,
		}
	}
}
