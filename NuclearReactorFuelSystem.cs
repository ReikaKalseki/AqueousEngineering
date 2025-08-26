using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using ReikaKalseki.DIAlterra;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.AqueousEngineering {

	public class NuclearReactorFuelSystem : SaveSystem.SaveHandler {

		public static readonly NuclearReactorFuelSystem instance = new NuclearReactorFuelSystem();

		public static readonly string REACTOR_CLASSID = "864f7780-a4c3-4bf2-b9c7-f4296388b70f";

		private readonly Dictionary<TechType, NuclearFuel> fuels = new Dictionary<TechType, NuclearFuel>();

		private NuclearReactorFuelSystem() {

		}

		public void register() {
			this.registerReactorFuel(TechType.ReactorRod, BaseNuclearReactor.charge[TechType.ReactorRod], 4.166666F, 1); //default PPS for reactor was 4.1666665f

			SaveSystem.addSaveHandler(REACTOR_CLASSID, this);
		}

		public void registerReactorFuel(TechType tt, float capacity, float pps, float rad) {
			this.registerReactorFuel(tt, capacity, pps, rad, TechType.DepletedReactorRod);
		}

		public void registerReactorFuelRelative(TechType tt, float capacity, float pps, float rad, TechType depleted) {
			NuclearFuel baseline = fuels[TechType.ReactorRod];
			this.registerReactorFuel(tt, capacity * baseline.energyPerItem, pps * baseline.maxPPS, rad, depleted);
		}

		public void registerReactorFuel(TechType tt, float capacity, float pps, float rad, TechType depleted) {
			BaseNuclearReactor.charge[tt] = capacity;
			fuels[tt] = new NuclearFuel(tt, capacity, pps, rad, depleted);
		}

		public override void save(PrefabIdentifier pi) {
			ReactorManager lgc = pi.GetComponentInChildren<ReactorManager>();
			if (lgc)
				lgc.save(data);
		}

		public override void load(PrefabIdentifier pi) {
			ReactorManager lgc = pi.GetComponentInChildren<ReactorManager>();
			if (lgc)
				lgc.load(data);
		}

		class NuclearFuel {

			public readonly TechType itemType;
			public readonly float energyPerItem;
			public readonly float maxPPS;
			public readonly float radiationIntensityFactor;
			public readonly TechType depletedFuel;

			internal NuclearFuel(TechType tt, float e, float p, float r) : this(tt, e, p, r, TechType.DepletedReactorRod) {

			}

			internal NuclearFuel(TechType tt, float e, float p, float r, TechType dep) {
				itemType = tt;
				energyPerItem = e;
				maxPPS = p;
				radiationIntensityFactor = r;
				depletedFuel = dep;
			}

			public override string ToString() {
				return string.Format("[NuclearFuel ItemType={0}, EnergyPerItem={1}, MaxPPS={2}, Radiation={3}, DepletedFuel={4}]", itemType.AsString(), energyPerItem, maxPPS, radiationIntensityFactor, depletedFuel.AsString());
			}


		}
		/*
		internal class NuclearReactorRadiationManager : MonoBehaviour {
			
			private RadiatePlayerInRange radiation;
			private BaseNuclearReactor reactor;
			
			private float radiationIntensity;
				
			void Start() {
				reactor = gameObject.GetComponent<BaseNuclearReactor>();
				radiation = gameObject.EnsureComponent<RadiatePlayerInRange>();
				radiation.tracker = gameObject.EnsureComponent<PlayerDistanceTracker>();
			}
			
			void Update() {
				radiationIntensity = 0;
				if (reactor) {
					List<string> slots = new List<string>();
					reactor.equipment.GetSlots(EquipmentType.NuclearReactor, slots);
					foreach (string slot in slots) {
						InventoryItem ii = reactor.equipment.GetItemInSlot(slot);
						if (ii != null) {
							float f = 1;
							radiationIntensity += 1F/slots.Count*f;
						}
					}
				}
				radiationIntensity = Mathf.Clamp01(radiationIntensity);
				radiation.enabled = radiationIntensity > 0;
				radiation.radiateRadius = 6*radiationIntensity;
			}
			
		}
		*/
		internal class ReactorFuelDisplay : MonoBehaviour {

			private uGUI_EquipmentSlot slot;

			private GameObject fillBar;
			private Image background;
			private Image foreground;

			private float currentFillLevel = 0;

			void Update() {
				if (!slot)
					slot = this.GetComponent<uGUI_EquipmentSlot>();
				if (!fillBar) {
					fillBar = new GameObject("FuelLifeIndicator");
					fillBar.transform.SetParent(transform, false);
					fillBar.transform.localRotation = Quaternion.Euler(0, 0, 202.5F);
					fillBar.layer = gameObject.layer;
					fillBar.transform.localPosition = Vector3.zero;//new Vector3(0.55F, -0.55F, 0);
					fillBar.transform.localScale = Vector3.one * 2.5F;
					GameObject go = new GameObject("FuelLifeIndicatorBackground");
					go.transform.SetParent(fillBar.transform, false);
					go.layer = gameObject.layer;
					background = go.AddComponent<Image>();
					background.sprite = Sprite.Create(TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ReactorFuelBarBack"), new Rect(0, 0, 128, 128), Vector2.zero);
					background.rectTransform.offsetMin = new Vector2(-32f, -32f);
					background.rectTransform.offsetMax = new Vector2(32f, 32f);
					GameObject go2 = new GameObject("FuelLifeIndicatorForeground");
					go2.transform.SetParent(fillBar.transform, false);
					go2.layer = gameObject.layer;
					foreground = go2.AddComponent<Image>();
					foreground.sprite = Sprite.Create(TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ReactorFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
					foreground.rectTransform.offsetMin = new Vector2(-32f, -32f);
					foreground.rectTransform.offsetMax = new Vector2(32f, 32f);
					foreground.type = Image.Type.Filled;
					foreground.fillMethod = Image.FillMethod.Radial360;
					foreground.fillClockwise = true;
					foreground.fillOrigin = 2;
					fillBar.SetActive(false);
				}
				if (slot && slot.gameObject.activeInHierarchy && Player.main.GetPDA().isOpen) {
					InventoryItem ii = slot.manager.equipment.GetItemInSlot(slot.slot);
					fillBar.SetActive(ii != null && ii.item && ii.item.GetTechType() != TechType.DepletedReactorRod);
					ReactorManager rm = slot.manager.equipment.owner.GetComponent<ReactorManager>();
					if (rm) {
						currentFillLevel = rm.getReactorRodLife(slot.slot);
						//SNUtil.writeToChat(slot.slot+": "+(frac*100).ToString("0.0")+"%");
						float f = (currentFillLevel * 0.875F) + 0.125F;
						foreground.fillAmount = f;
						foreground.color = f < 0.5F ? new Color(1, f * 2, 0, 1) : new Color(1 - ((f - 0.5F) * 2), 1, 0, 1);
					}
				}
				else {
					fillBar.SetActive(false);
				}
			}

		}

		internal class ReactorManager : MonoBehaviour {

			private readonly Dictionary<string, float> usedEnergy = new Dictionary<string, float>();

			private BaseNuclearReactor reactor;

			private AoERadiationZone radiation;

			private float radiationIntensity;

			void Start() {
				reactor = gameObject.GetComponent<BaseNuclearReactor>();
				radiation = gameObject.EnsureComponent<AoERadiationZone>();
			}

			internal float getReactorRodLife(string slot) {
				if (!usedEnergy.ContainsKey(slot))
					return 0;
				InventoryItem itemInSlot = reactor.equipment.GetItemInSlot(slot);
				if (itemInSlot == null || !itemInSlot.item)
					return 0;
				TechType tt = itemInSlot.item.GetTechType();
				return 1F - (usedEnergy[slot] / NuclearReactorFuelSystem.instance.fuels[tt].energyPerItem);
			}

			void Update() {
				if (!reactor)
					reactor = this.GetComponent<BaseNuclearReactor>();
				float dT = DayNightCycle.main.deltaTime;
				if (dT > 0) {
					radiationIntensity = 0;
					if (reactor && reactor.constructed >= 1f) {
						float space = reactor._powerSource.maxPower-reactor._powerSource.power;
						if (space > 0.1F) {
							SubRoot sub = gameObject.FindAncestor<SubRoot>();
							bool on = sub && sub.powerRelay && sub.powerRelay.GetPower()*100F/sub.powerRelay.GetMaxPower() < AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.NUCTHRESH);
							if (on) {
								for (int i = 0; i < BaseNuclearReactor.slotIDs.Length; i++) {
									string slot = BaseNuclearReactor.slotIDs[i];
									InventoryItem itemInSlot = reactor.equipment.GetItemInSlot(slot);
									if (itemInSlot != null) {
										Pickupable item = itemInSlot.item;
										if (item) {
											TechType techType = item.GetTechType();
											if (techType != TechType.None && techType != TechType.DepletedReactorRod) {
												NuclearFuel fuel = NuclearReactorFuelSystem.instance.fuels[techType];
												float dP = fuel.maxPPS*dT;
												if (BaseRoomSpecializationSystem.instance.getSavedType(reactor) == BaseRoomSpecializationSystem.RoomTypes.POWER)
													dP *= 1.25F;
												DIHooks.addPowerToSeabaseDelegate(reactor._powerSource, dP, out float added, reactor);
												radiationIntensity += 1F / BaseNuclearReactor.slotIDs.Length * fuel.radiationIntensityFactor;
												//SNUtil.writeToChat("Reactor @ "+reactor.transform.position+" generated "+added+" from "+fuel+" in slot "+slot);
												this.use(slot, added, fuel);
											}
										}
									}
								}
							}
						}
					}
					radiationIntensity = Mathf.Clamp(radiationIntensity, 0, 2);
					radiation.enabled = radiationIntensity > 0; //will not do damage unless add a DamagePlayerInRadius
					radiation.setRadii(4.5F, 2F); //about the radius of its room
					radiation.maxIntensity = radiationIntensity * 0.25F;
				}
			}

			private void use(string slot, float amt, NuclearFuel fuel) {
				usedEnergy[slot] = usedEnergy.ContainsKey(slot) ? usedEnergy[slot] + amt : amt;

				if (usedEnergy[slot] >= fuel.energyPerItem) {
					usedEnergy[slot] -= fuel.energyPerItem;
					reactor.equipment.RemoveItem(slot, true, false).item.gameObject.destroy(false);
					if (fuel.depletedFuel != TechType.None)
						reactor.equipment.AddItem(slot, this.createDepletedRod(fuel), true);
				}
			}

			private InventoryItem createDepletedRod(NuclearFuel from) {
				return new InventoryItem(UnityEngine.Object.Instantiate<GameObject>(CraftData.GetPrefabForTechType(from.depletedFuel, true)).GetComponent<Pickupable>().Pickup(false));
			}

			internal void load(XmlElement data) {
				usedEnergy.Clear();
				foreach (XmlNode n in data.ChildNodes) {
					if (n is XmlElement e) {
						usedEnergy[e.Name] = float.Parse(e.InnerText);
					}
				}
			}

			internal void save(XmlElement data) {
				foreach (KeyValuePair<string, float> kvp in usedEnergy) {
					data.addProperty(kvp.Key, kvp.Value);
				}
			}

		}

	}

}
