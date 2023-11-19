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
	
	public class MiniPoo : Spawnable {
		
		public MiniPoo(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
			OnFinishedPatching += () => {ItemRegistry.instance.addItem(this);};
		}
		
		public override GameObject GetGameObject() {
			GameObject go = ObjectUtil.createWorldObject("61ac1241-e990-4646-a618-bddb6960325b");
			go.transform.localScale = Vector3.one*0.2F;
			return go;
		}
		
		protected override Atlas.Sprite GetItemSprite() {
			return SpriteManager.Get(TechType.SeaTreaderPoop);
		}
	}
}
