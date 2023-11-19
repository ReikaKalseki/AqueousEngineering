using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering
{
	public class StalkerToy : BasicCraftingItem {
		
		public StalkerToy(XMLLocale.LocaleEntry e) : base(e, "WorldEntities/Food/CuredHoopfish") {
			sprite = TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/Items/StalkerToy");
			unlockRequirement = TechType.Unobtanium;
			craftingTime = 6;
			inventorySize = new Vector2int(2, 2);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			foreach (Renderer r in r0) {
				RenderUtil.setEmissivity(r, 0);
				RenderUtil.setGlossiness(r, 9, 15, 0);
			}
			RenderUtil.swapToModdedTextures(r0, this);
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Fabricator;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Personal;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Equipment;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Machines"};
			}
		}
	}
}
