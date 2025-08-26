using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	public class AEConfig {
		public enum ConfigEntries {
			[ConfigEntry("ACU feces drop rate", typeof(float), 1F, 0, 4, float.NaN)]POO_RATE, //A multiplier to ACU mini feces generation rate
			[ConfigEntry("Seabase Battery power cell storage equivalence", typeof(int), 2, 1, 5, float.NaN)]BATTCELLS, //How many power cells worth of storage a SB battery can store. Also sets its craft cost.
			[ConfigEntry("ATP Tap Generation Rate", typeof(int), 12, 5, 40, float.NaN)]ATPTAPRATE, //How many power cells worth of storage a SB battery can store. Also sets its craft cost.
			[ConfigEntry("Show room status messages in HUD", true)]ROOMCHAT, //Whether to print text to the upper-left every time a room changes type or decoration rating.
			[ConfigEntry("Leisure Room Deco Rating Requirement", typeof(float), 15, 8, 30, float.NaN)]LEISUREDECO, //What decoration rating must be achieved for a room to count as a leisure room.
			[ConfigEntry("Add deco value to rooms from outside environment", true)]ROOMENVIRODECO, //Whether to add deco bonuses to rooms from windows based on the environment. Disable this as a last resort for lag.
			[ConfigEntry("ACU Creatures Make Sound", true, true)]ACUSOUND, //Whether to enable ambient creature noises for creatures in ACUs
			[ConfigEntry("BioReactor Power Threshold", typeof(float), 90F, 5F, 100, 100)]BIOTHRESH, //What the base power percentage must fall below for bioreactors to activate.
			[ConfigEntry("Nuclear Reactor Power Threshold", typeof(float), 75F, 5F, 100, 100)]NUCTHRESH, //What the base power percentage must fall below for nuclear reactors to activate.
			[ConfigEntry("Base Pillar Hull Boost", typeof(float), 3, 1, 10, float.NaN)]PILLARHULL, //How much hull reinforcement a pillar provides at base.
			[ConfigEntry("Base Pillar Limit", typeof(int), 1, 1, 5, float.NaN)]PILLARLIM, //How many pillars a room can have before their hull bonus begins to drop.
			[ConfigEntry("Sleep Morale Restoration", typeof(int), 25, 5, 50, float.NaN)]SLEEPMORALE, //How much morale sleeping restores.
			[ConfigEntry("Deco Morale Decay/Gain Speed", typeof(float), 1, 0.1F, 10, float.NaN)]MORALESPEED, //A multiplier for how fast morale decays or grows based on deco rating.
		}
	}
}
