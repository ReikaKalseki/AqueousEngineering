using System;
using System.Collections.Generic;
using System.Linq;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

using UnityEngine;

namespace ReikaKalseki.AqueousEngineering {
	public class ItemCollector : BasicCraftingItem {

		public ItemCollector(XMLLocale.LocaleEntry e) : base(e, "WorldEntities/Tools/Gravsphere") {
			sprite = TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/Items/ItemCollector");
			unlockRequirement = TechType.Unobtanium;

			craftingTime = 6;
			inventorySize = new Vector2int(3, 3);

			this.addIngredient(TechType.Gravsphere, 1);
			this.addIngredient(TechType.Titanium, 2);
			this.addIngredient(TechType.Magnetite, 1);
			this.addIngredient(TechType.Aerogel, 3);
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
				return new string[] { "Machines" };
			}
		}

		internal class ItemCollectorLogic : MonoBehaviour {

			private Gravsphere gravity;
			private Rigidbody body;

			private float lastInventoryCheckTime = -1;

			private readonly List<StorageContainer> targetInventories = new List<StorageContainer>();

			internal static bool canGrab(GameObject go) {
				//SNUtil.writeToChat("item collector tried to grab "+go);
				Pickupable pp = go.FindAncestor<Pickupable>();
				return (pp && pp.isPickupable && !pp.attached) || go.FindAncestor<BreakableResource>();
			}

			void Update() {
				if (!gravity)
					gravity = this.GetComponent<Gravsphere>();
				if (!body)
					body = this.GetComponent<Rigidbody>();

				if (Player.main.currentSub && Player.main.currentSub.isCyclops && Vector3.Distance(transform.position, Player.main.currentSub.transform.position) <= 120) {
					SubRoot sub = Player.main.currentSub;
					CyclopsMotorMode mode = sub.GetComponentInChildren<CyclopsMotorMode>();
					if (mode && mode.engineOn) {
						ItemCollectorCyclopsTetherLogic lgc = sub.GetComponentInChildren<ItemCollectorCyclopsTetherLogic>();
						if (lgc) {
							lgc.itemCollector = gameObject;
							Vector3 tgt = sub.transform.position+(sub.transform.up*-9)-(sub.transform.forward*18);
							Vector3 diff = tgt-transform.position;
							body.velocity = diff.normalized * Mathf.Min(diff.sqrMagnitude * 0.04F, 30);
							lgc.lineRenderer.attachPoint.position = tgt + (Vector3.up * 2);
						}
					}
				}

				float time = DayNightCycle.main.timePassedAsFloat;
				if (time - lastInventoryCheckTime >= 1) {
					lastInventoryCheckTime = time;
					targetInventories.Clear();
					WorldUtil.getGameObjectsNear(transform.position, 20, go => {
						this.tryAddTarget(go.FindAncestor<StorageContainer>());
						SubRoot sub = go.FindAncestor<SubRoot>();
						if (sub) {
							foreach (StorageContainer sc2 in sub.GetComponentsInChildren<StorageContainer>()) {
								this.tryAddTarget(sc2);
							}
						}
					});
				}

				if (gravity && targetInventories.Count > 0 && UnityEngine.Random.Range(0F, 1F) <= Time.deltaTime) {
					Rigidbody rb = gravity.attractableList.getRandomEntry();
					if (rb && rb.gameObject.activeInHierarchy && !rb.GetComponent<WaterParkItem>()) {
						Pickupable pp = rb.GetComponent<Pickupable>();
						if (pp && Vector3.Distance(pp.transform.position, transform.position) <= 8) {
							StorageContainer sc = targetInventories.getRandomEntry();
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
