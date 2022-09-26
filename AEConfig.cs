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
			[ConfigEntry("Feces drop rate", typeof(float), 1F, 0, 4, 0)]POO_RATE,
		}
	}
}
