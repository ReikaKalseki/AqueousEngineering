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
	}
	
	class ContainmentFacilityDragonRepellent : MonoBehaviour {
		
		void Update() {
			float r = 80;
			if (Player.main.transform.position.y <= 1350 && Vector3.Distance(transform.position, Player.main.transform.position) <= 100) {
				RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, r, new Vector3(1, 1, 1), r);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						SeaDragon c = rh.transform.gameObject.GetComponent<SeaDragon>();
						if (c) {
							Vector3 vec = transform.position+((c.transform.position-transform.position).normalized*120);
							c.GetComponent<SwimBehaviour>().SwimTo(vec, 20);
						}
					}
				}
			}
		}
		
	}
}
