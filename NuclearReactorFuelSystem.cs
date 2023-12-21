using System;
using System.IO;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class NuclearReactorFuelSystem : SaveSystem.SaveHandler {
		
		public static readonly NuclearReactorFuelSystem instance = new NuclearReactorFuelSystem();
		
		public static readonly string REACTOR_CLASSID = "864f7780-a4c3-4bf2-b9c7-f4296388b70f";
		
		private readonly Dictionary<TechType, NuclearFuel> fuels = new Dictionary<TechType, NuclearFuel>();
		
		private NuclearReactorFuelSystem() {
			
		}
		
		public void register() {
			registerReactorFuel(TechType.ReactorRod, BaseNuclearReactor.charge[TechType.ReactorRod], 4.166666F); //default PPS for reactor was 4.1666665f
			
			SaveSystem.addSaveHandler(REACTOR_CLASSID, this);
		}
		
		public void registerReactorFuel(TechType tt, float capacity, float pps) {
			registerReactorFuel(tt, capacity, pps, TechType.DepletedReactorRod);
		}
		
		public void registerReactorFuelRelative(TechType tt, float capacity, float pps, TechType depleted) {
			NuclearFuel baseline = fuels[TechType.ReactorRod];
			registerReactorFuel(tt, capacity*baseline.energyPerItem, pps*baseline.maxPPS, depleted);
		}
		
		public void registerReactorFuel(TechType tt, float capacity, float pps, TechType depleted) {
			BaseNuclearReactor.charge[tt] = capacity;
			fuels[tt] = new NuclearFuel(tt, capacity, pps, depleted);
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
			public readonly TechType depletedFuel;
			
			internal NuclearFuel(TechType tt, float e, float p) : this(tt, e, p, TechType.DepletedReactorRod) {
				
			}
			
			internal NuclearFuel(TechType tt, float e, float p, TechType dep) {
				itemType = tt;
				energyPerItem = e;
				maxPPS = p;
				depletedFuel = dep;
			}
			
			public override string ToString()
			{
				return string.Format("[NuclearFuel ItemType={0}, EnergyPerItem={1}, MaxPPS={2}, DepletedFuel={3}]", itemType.AsString(), energyPerItem, maxPPS, depletedFuel.AsString());
			}

			
		}
		
		internal class ReactorFuelDisplay : MonoBehaviour {
			
			private uGUI_EquipmentSlot slot;
			
			private GameObject fillBar;
			private Image background;
			private Image foreground;
			
			private float currentFillLevel = 0;
			
			void Update() {
				if (!slot)
					slot = GetComponent<uGUI_EquipmentSlot>();
				if (!fillBar) {
					fillBar = new GameObject("FuelLifeIndicator");
					fillBar.transform.SetParent(transform, false);
					fillBar.transform.localRotation = Quaternion.identity;
					fillBar.layer = gameObject.layer;
					fillBar.transform.localPosition = Vector3.zero;//new Vector3(0.55F, -0.55F, 0);
					fillBar.transform.localScale = Vector3.one*2.5F;
					GameObject go = new GameObject("FuelLifeIndicatorBackground");
					go.transform.SetParent(fillBar.transform, false);
					go.layer = gameObject.layer;
					this.background = go.AddComponent<Image>();
					this.background.sprite = Sprite.Create(TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ReactorFuelBarBack"), new Rect(0, 0, 128, 128), Vector2.zero);
					this.background.rectTransform.offsetMin = new Vector2(-32f, -32f);
					this.background.rectTransform.offsetMax = new Vector2(32f, 32f);
					GameObject go2 = new GameObject("FuelLifeIndicatorForeground");
					go2.transform.SetParent(fillBar.transform, false);
					go2.layer = gameObject.layer;
					this.foreground = go2.AddComponent<Image>();
					this.foreground.sprite = Sprite.Create(TextureManager.getTexture(AqueousEngineeringMod.modDLL, "Textures/ReactorFuelBar"), new Rect(0, 0, 128, 128), Vector2.zero);
					this.foreground.rectTransform.offsetMin = new Vector2(-32f, -32f);
					this.foreground.rectTransform.offsetMax = new Vector2(32f, 32f);
					this.foreground.type = Image.Type.Filled;
					this.foreground.fillMethod = Image.FillMethod.Radial360;
					this.foreground.fillClockwise = true;
					this.foreground.fillOrigin = 2;
					fillBar.SetActive(false);
				}
				if (slot && slot.gameObject.activeInHierarchy && Player.main.GetPDA().isOpen) {
					InventoryItem ii = slot.manager.equipment.GetItemInSlot(slot.slot);
					fillBar.SetActive(ii != null && ii.item && ii.item.GetTechType() != TechType.DepletedReactorRod);
					ReactorManager rm = slot.manager.equipment.owner.GetComponent<ReactorManager>();
					if (rm) {
						currentFillLevel = rm.getReactorRodLife(slot.slot);
						//SNUtil.writeToChat(slot.slot+": "+(frac*100).ToString("0.0")+"%");
						float f = currentFillLevel * 0.875F + 0.125F;
						foreground.fillAmount = f;
						foreground.color = f < 0.5F ? new Color(1, f*2, 0, 1) : new Color(1-(f-0.5F)*2, 1, 0, 1);
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
			
			internal float getReactorRodLife(string slot) {
				if (!usedEnergy.ContainsKey(slot))
					return 0;
				InventoryItem itemInSlot = reactor.equipment.GetItemInSlot(slot);
				if (itemInSlot == null || !itemInSlot.item)
					return 0;
				TechType tt = itemInSlot.item.GetTechType();
				return 1F-(usedEnergy[slot]/NuclearReactorFuelSystem.instance.fuels[tt].energyPerItem);
			}
			
			void Update() {
				if (!reactor)
					reactor = GetComponent<BaseNuclearReactor>();
				if (reactor && reactor.constructed >= 1f) {
					float space = reactor._powerSource.maxPower-reactor._powerSource.power;
					if (space > 0.1F) {
						float dT = DayNightCycle.main.deltaTime;
						for (int i = 0; i < BaseNuclearReactor.slotIDs.Length; i++) {
							string slot = BaseNuclearReactor.slotIDs[i];
							InventoryItem itemInSlot = reactor.equipment.GetItemInSlot(slot);
							if (itemInSlot != null) {
								Pickupable item = itemInSlot.item;
								if (item != null) {
									TechType techType = item.GetTechType();
									if (techType != TechType.None && techType != TechType.DepletedReactorRod) {
										float added;
										NuclearFuel fuel = NuclearReactorFuelSystem.instance.fuels[techType];
										DIHooks.addPowerToSeabaseDelegate(reactor._powerSource, fuel.maxPPS*dT, out added, reactor);
										//SNUtil.writeToChat("Reactor @ "+reactor.transform.position+" generated "+added+" from "+fuel+" in slot "+slot);
										use(slot, added, fuel);
									}
								}
							}
						}
					}
				}
			}
			
			private void use(string slot, float amt, NuclearFuel fuel) {
				if (usedEnergy.ContainsKey(slot))
					usedEnergy[slot] = usedEnergy[slot]+amt;
				else
					usedEnergy[slot] = amt;
				
				if (usedEnergy[slot] >= fuel.energyPerItem) {
					usedEnergy[slot] -= fuel.energyPerItem;
					UnityEngine.Object.Destroy(reactor.equipment.RemoveItem(slot, true, false).item.gameObject);
					if (fuel.depletedFuel != TechType.None)
						reactor.equipment.AddItem(slot, createDepletedRod(fuel), true);
				}
			}
			
			private InventoryItem createDepletedRod(NuclearFuel from) {
				return new InventoryItem(UnityEngine.Object.Instantiate<GameObject>(CraftData.GetPrefabForTechType(from.depletedFuel, true)).GetComponent<Pickupable>().Pickup(false));
			}
			
			internal void load(XmlElement data) {
				usedEnergy.Clear();
				foreach (XmlNode n in data.ChildNodes) {
					if (n is XmlElement) {
						XmlElement e = (XmlElement)n;
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
