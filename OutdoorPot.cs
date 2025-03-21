﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;

namespace ReikaKalseki.AqueousEngineering {
	
	public class OutdoorPot : Buildable {
		
		private readonly TechType pot;
		private readonly string prefabBase;
		
		private static readonly List<OutdoorPot> pots = new List<OutdoorPot>();
	        
		internal OutdoorPot(TechType tt) : base(generateName(tt), "Outdoor "+tt.AsString(), "A "+tt.AsString()+" for use outdoors.") {
			pot = tt;
			prefabBase = CraftData.GetClassIdForTechType(tt);
			pots.Add(this);
	    }
		
		private static string generateName(TechType tech) {
			string en = Enum.GetName(typeof(TechType), tech);
			return "outdoorpot_"+en.Substring(en.LastIndexOf('_')+1);
		}
		
		public static void updateLocale() {
			foreach (OutdoorPot d in pots) {
				CustomLocaleKeyDatabase.registerKey(d.TechType.AsString(), "Outdoor "+Language.main.Get(d.pot));
				CustomLocaleKeyDatabase.registerKey("Tooltip_"+d.TechType.AsString(), Language.main.Get("Tooltip_"+d.pot.AsString())+" Designed for outdoor use.");
				SNUtil.log("Relocalized "+d+" > "+Language.main.Get(d.TechType), AqueousEngineeringMod.modDLL);
			}
		}
		
		public void register() {
			Patch();
        	KnownTechHandler.Main.SetAnalysisTechEntry(pot, new List<TechType>(){TechType});
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}

		public override sealed TechGroup GroupForPDA {
			get {
				return TechGroup.ExteriorModules;
			}
		}

		public override sealed TechCategory CategoryForPDA {
			get {
				return TechCategory.ExteriorModule;
			}
		}
		
		protected override sealed TechData GetBlueprintRecipe() {
			return RecipeUtil.getRecipe(pot);/*new TechData
			{
				Ingredients = new List<Ingredient>{new Ingredient(TechType.Titanium, 2)},
				craftAmount = 1
			};*/
		}
		
		protected sealed override Atlas.Sprite GetItemSprite() {
			return SpriteManager.Get(pot);//TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
		}
			
	    public override GameObject GetGameObject() {
			GameObject world = ObjectUtil.createWorldObject(prefabBase, true, false);
			if (world != null) {
				world.SetActive(false);
				world.EnsureComponent<TechTag>().type = TechType;
				world.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
				Constructable c = world.EnsureComponent<Constructable>();
				c.techType = TechType;
				c.allowedInBase = false;
				c.allowedInSub = false;
				c.allowedOutside = true;
				c.allowedOnGround = true;
				Planter p = world.EnsureComponent<Planter>();
				p.environment = Planter.PlantEnvironment.Dynamic;
				p.isIndoor = false;
				world.SetActive(true);
				return world;
			}
			else {
				SNUtil.writeToChat("Could not fetch template GO for "+this);
				return null;
			}
	    }
			
	}
}
