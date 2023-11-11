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
			[ConfigEntry("ACU feces drop rate", typeof(float), 1F, 0, 4, 0)]POO_RATE, //A multiplier to ACU mini feces generation rate
			[ConfigEntry("Seabase Battery power cell storage equivalence", typeof(int), 2, 1, 5, 0)]BATTCELLS, //How many power cells worth of storage a SB battery can store. Also sets its craft cost.
			[ConfigEntry("ATP Tap Generation Rate", typeof(int), 12, 5, 40, 0)]ATPTAPRATE, //How many power cells worth of storage a SB battery can store. Also sets its craft cost.
			[ConfigEntry("Show room status messages in HUD", true)]ROOMCHAT, //Whether to print text to the upper-left every time a room changes type or decoration rating.
			[ConfigEntry("Leisure Room Deco Rating Requirement", typeof(float), 15, 8, 30, 0)]LEISUREDECO, //What decoration rating must be achieved for a room to count as a leisure room.
		}
	}
}
