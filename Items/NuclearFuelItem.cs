using System;
using System.Collections.Generic;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering
{
	public sealed class NuclearFuelItem : CustomEquipable {
		
		public NuclearFuelItem(string key) : base(AqueousEngineeringMod.itemLocale.getEntry(key), "WorldEntities/Natural/reactorrod") {
			preventNaturalUnlock();
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			RenderUtil.swapToModdedTextures(r0, this);
		}
		
		public override EquipmentType EquipmentType {
			get {
				return EquipmentType.NuclearReactor;
			}
		}

		public override float CraftingTime {
			get {
				return 4;
			}
		}
		
		public override Vector2int SizeInInventory {
			get {
				return new Vector2int(1, 1);
			}
		}

		public override CraftTree.Type FabricatorType {
			get {
				return CraftTree.Type.Fabricator;
			}
		}

		public override TechGroup GroupForPDA {
			get {
				return TechGroup.Resources;
			}
		}

		public override TechCategory CategoryForPDA {
			get {
				return TechCategory.Electronics;
			}
		}

		public override string[] StepsToFabricatorTab {
			get {
				return new string[]{"Resources", "Electronics"};
			}
		}
	}
}
