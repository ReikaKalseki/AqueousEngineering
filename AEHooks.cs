using System;
using System.IO;
using System.Xml;
using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using UnityEngine;

using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;

namespace ReikaKalseki.AqueousEngineering {
	
	public static class AEHooks {
	    
	    static AEHooks() {
	    	DIHooks.onWorldLoadedEvent += onWorldLoaded;
	    	DIHooks.constructabilityEvent += enforceACUBuildability;
	    }
	    
	    public static void onWorldLoaded() {	        
	    	OutdoorPot.updateLocale();
	    }
	   
	   	public static void tickACU(WaterPark acu) {
	   		ACUCallbackSystem.instance.tick(acu);
	   	}
	   
	   	public static bool canAddItemToACU(Pickupable item) {
			if (!item)
		   		return false;
			TechType tt = item.GetTechType();
			if (tt == TechType.ScrapMetal || tt == TechType.Titanium || tt == TechType.Silver)
				return true;
			GameObject go = item.gameObject;
			if (go.GetComponent<Creature>() == null && go.GetComponent<CreatureEgg>() == null)
				return false;
			LiveMixin lv = go.GetComponent<LiveMixin>();
			return !lv || lv.IsAlive();
	   	}
	   
		public static void onChunkGenGrass(IVoxelandChunk2 chunk) {
		 	foreach (Renderer r in chunk.grassRenders) {
		   		ACUTheming.cacheGrassMaterial(r.materials[0]);
			}
	   	}
		
		public static float getCameraDistanceForRenderFX(MapRoomCamera cam, MapRoomScreen scr) {
			SubRoot sub = cam.dockingPoint ? cam.dockingPoint.gameObject.GetComponentInParent<SubRoot>() : null;
			if (!sub) {
				sub = WorldUtil.getClosest<SubRoot>(cam.gameObject);
			}
			if (sub && Vector3.Distance(sub.transform.position, cam.transform.position) <= 400) {
				RemoteCameraAntennaLogic lgc = sub.GetComponentInChildren<RemoteCameraAntennaLogic>();
				if (lgc && lgc.isReady())
					return 0;
			}
			return cam.GetScreenDistance(scr);
		}
		
		private static bool isBuildingACUBuiltBlock() {
			if (AqueousEngineeringMod.acuBoosterBlock != null && Builder.constructableTechType == AqueousEngineeringMod.acuBoosterBlock.TechType)
				return true;
			if (AqueousEngineeringMod.acuCleanerBlock != null && Builder.constructableTechType == AqueousEngineeringMod.acuCleanerBlock.TechType)
				return true;
			return false;
		}
		/*
		private static bool isOnACU(Collider c) {
			if (!c)
				return false;
			BaseExplicitFace face = c.gameObject.FindAncestor<BaseExplicitFace>();
			if (!face)
				return false;
			SNUtil.writeToChat(face+" > "+face.gameObject.GetFullHierarchyPath()+" > "+face.gameObject.name.Contains("WaterPark"));
			return face && face.gameObject.name.Contains("WaterPark");
		}
	    */
	    public static void enforceACUBuildability(DIHooks.BuildabilityCheck check) {	        
			if (isBuildingACUBuiltBlock()) {
	   			check.placeable = check.placeOn && check.placeOn.gameObject.FindAncestor<WaterParkPiece>();//isOnACU(check.placeOn && chec);
				check.ignoreSpaceRequirements = true;
			}
	   		else if (Builder.constructableTechType == AqueousEngineeringMod.ampeelAntennaBlock.TechType && check.placeOn && Player.main.currentWaterPark && check.placeOn.gameObject.FindAncestor<WaterParkPiece>().GetWaterParkModule() == Player.main.currentWaterPark) {
	   			check.placeable = true;
				check.ignoreSpaceRequirements = true;
		   	}
	    }
	}
}
