using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class MiniPoo : BasicCraftingItem {
		
		public MiniPoo(XMLLocale.LocaleEntry e) : base("MiniPoop", e.name, e.desc, "WorldEntities/Natural/SeaTreaderPoop") {
			sprite = SpriteManager.Get(TechType.SeaTreaderPoop);
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			
			go.transform.localScale = Vector3.one*0.2F;
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.None;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Uncategorized;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Misc;
			}
		}
		
	}
}
