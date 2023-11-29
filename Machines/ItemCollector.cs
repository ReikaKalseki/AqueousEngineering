using System;
using System.Collections.Generic;
using System.Linq;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;

using UnityEngine;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering
{
	public class ItemCollector : BasicCraftingItem {
		
		public ItemCollector(XMLLocale.LocaleEntry e) : base(e, "WorldEntities/Tools/Gravsphere") {
			sprite = TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/Items/ItemCollector");
			unlockRequirement = TechType.Unobtanium;
			
			craftingTime = 6;
			inventorySize = new Vector2int(3, 3);
			
			addIngredient(TechType.Gravsphere, 1);
			addIngredient(TechType.Titanium, 2);
			addIngredient(TechType.Magnetite, 1);
			addIngredient(TechType.Aerogel, 3);
		}

		public override void prepareGameObject(GameObject go, Renderer[] r0) {
			base.prepareGameObject(go, r0);
			foreach (Renderer r in r0) {
				RenderUtil.setEmissivity(r, 1);
			}
			RenderUtil.swapToModdedTextures(r0, this);
			go.EnsureComponent<ItemCollectorLogic>();
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
		
		internal class ItemCollectorLogic : MonoBehaviour {
			
			private Gravsphere gravity;
			
			private float lastInventoryCheckTime = -1;
			
			private readonly List<StorageContainer> targetInventories = new List<StorageContainer>();
			
			internal static bool canGrab(Rigidbody rb) {
				Pickupable pp = rb.GetComponent<Pickupable>();
				return (pp && pp.isPickupable && !pp.attached) || rb.GetComponent<BreakableResource>();
			}
			
			void Update() {
				if (!gravity)
					gravity = GetComponent<Gravsphere>();
				
				float time = DayNightCycle.main.timePassedAsFloat;
				if (time-lastInventoryCheckTime >= 1) {
					lastInventoryCheckTime = time;
					targetInventories.Clear();
					WorldUtil.getGameObjectsNear(transform.position, 20, go => {
						tryAddTarget(go.FindAncestor<StorageContainer>());
						SubRoot sub = go.FindAncestor<SubRoot>();
						if (sub) {
							foreach (StorageContainer sc2 in sub.GetComponentsInChildren<StorageContainer>()) {
								tryAddTarget(sc2);
							}
						}
					});
				}
				
				if (gravity && targetInventories.Count > 0 && UnityEngine.Random.Range(0F, 1F) <= Time.deltaTime) {
					Rigidbody rb = gravity.attractableList.GetRandom();
					if (rb && rb.gameObject.activeInHierarchy) {
						Pickupable pp = rb.GetComponent<Pickupable>();
						if (pp && Vector3.Distance(pp.transform.position, transform.position) <= 8) {
							StorageContainer sc = targetInventories.GetRandom();
							if (sc && sc.container.AddItem(pp) != null) {
								pp.PlayPickupSound();
								pp.gameObject.SetActive(false);
								gravity.removeList.Add(gravity.attractableList.IndexOf(rb));
							}
							else {
								SoundManager.playSoundAt(SoundManager.buildSound("event:/interface/select"), Player.main.transform.position, false, -1, 1);
							}
						}
						else {
							BreakableResource res = rb.GetComponent<BreakableResource>();
							if (res) {
								res.BreakIntoResources();
							}
						}
					}
				}
			}
			
			private void tryAddTarget(StorageContainer sc) {
				if (sc && sc.name.ToLowerInvariant().Contains("locker") && !targetInventories.Contains(sc)) {
					targetInventories.Add(sc);
				}
			}
			
		}
	}
}
