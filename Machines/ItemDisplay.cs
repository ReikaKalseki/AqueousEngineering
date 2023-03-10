using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class ItemDisplay : CustomMachine<ItemDisplayLogic> {
		
		public ItemDisplay(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc, "f1cde32e-101a-4dd5-8084-8c950b9c2432") {
			addIngredient(TechType.Titanium, 2);
			addIngredient(TechType.Silver, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return true;
			}
		}
		
		public override bool isOutdoors() {
			return false;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<Trashcan>(go);
			
			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.getChildObject(ObjectUtil.lookupPrefab("0fbf203a-a940-4b6e-ac63-0fe2737d84c2"), "model/Base_interior_Planter_Pot_03"));
			ObjectUtil.removeChildObject(mdl, "pot_generic_plant_03");
			mdl.transform.localScale = new Vector3(0.75F, 0.75F, 1.5F);
			mdl.transform.localRotation = Quaternion.Euler(-90, 0, 0);
			GameObject container = ObjectUtil.getChildObject(mdl, "Base_interior_Planter_Pot_03 1");
			GameObject floor = ObjectUtil.getChildObject(mdl, "Base_exterior_Planter_Tray_ground");
			floor.transform.localScale = new Vector3(0.02F, 0.02F, 0.005F);
			floor.transform.localPosition = new Vector3(0, 0, 0.3F);
						
			ItemDisplayLogic lgc = go.GetComponent<ItemDisplayLogic>();
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			initializeStorageContainer(con, 3, 3);
			con.errorSound = null;
			lgc.initStorageExt(con);
			
			ObjectUtil.removeChildObject(go, "descent_trashcan_01");
			
			Renderer r = container.GetComponentInChildren<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetColor("_Color", Color.white);
			r.materials[0].SetFloat("_Fresnel", 0.6F);
			r.materials[0].SetFloat("_SpecInt", 8F);
			r.materials[0].SetFloat("_Shininess", 15F);
			/*
			//SNUtil.dumpTextures(r);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			lgc.mainRenderer = r;*/
			
			Renderer screen = floor.GetComponentInChildren<Renderer>();
			RenderUtil.swapTextures(AqueousEngineeringMod.modDLL, screen, "Textures/Machines/ItemDisplayScreen");
			RenderUtil.setEmissivity(screen, 1.5F, "GlowStrength");
			screen.materials[0].SetColor("_Color", Color.white);
			screen.materials[0].SetColor("_GlowColor", Color.white);
			screen.materials[0].SetFloat("_Fresnel", 0F);
			screen.materials[0].SetFloat("_SpecInt", 5F);
			screen.materials[0].SetFloat("_Shininess", 0F);
			
			go.GetComponent<Constructable>().model = mdl;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
		}
		
	}
		
	public class ItemDisplayLogic : CustomMachineLogic {
		
		private static readonly string DISPLAY_OBJECT_NAME = "DisplayItem";
		
		private GameObject display;
		private Renderer displayRender;
		private float additionalRenderSpace = 0.1F;
		
		private bool needsUpdate = true;
		
		//private Vector3 rotationSpeed = Vector3.zero;
		//private Vector3 rotationSpeedTargets = Vector3.zero;
		
		void Start() {
			SNUtil.log("Reinitializing base item display");
			AqueousEngineeringMod.displayBlock.initializeMachine(gameObject);
		}
		
		internal void initStorageExt(StorageContainer sc) {
			initStorage(sc);
		}
		
		protected override void initStorage(StorageContainer sc) {
			sc.container.onAddItem += updateStoredItem;
			sc.container.onRemoveItem += ii => setDisplay(null);
			sc.container.isAllowedToAdd = new IsAllowedToAdd((pp, vb) => getStorage().container.GetItemTypes().Count == 0);
		}
		
		private void OnDestroy() {
			if (display)
				UnityEngine.Object.DestroyImmediate(display);
		}
		
		private void OnDisable() {
			if (display)
				UnityEngine.Object.DestroyImmediate(display);
		}
		
		protected override float getTickRate() {
			return 0;
		}
		
		protected override void updateEntity(float seconds) {
			if (needsUpdate && DIHooks.getWorldAge() > 0.5F) {
				updateStoredItem();
				needsUpdate = false;
			}
			if (display) {/*
				updateRotationSpeedTarget(ref rotationSpeed.x, ref rotationSpeedTargets.x);
				updateRotationSpeedTarget(ref rotationSpeed.y, ref rotationSpeedTargets.y);
				updateRotationSpeedTarget(ref rotationSpeed.z, ref rotationSpeedTargets.z);
				
				//display.transform.Rotate(rotationSpeed, Space.Self);
				Vector3 ctr = displayRender.bounds.center*2-displayRender.transform.position;
				display.transform.RotateAround(ctr, new Vector3(1, 0, 0), rotationSpeed.x);
				display.transform.RotateAround(ctr, new Vector3(0, 1, 0), rotationSpeed.y);
				display.transform.RotateAround(ctr, new Vector3(0, 0, 1), rotationSpeed.z);
				*/
				float itemOffset = 0.75F+0.1F*Mathf.Pow(Mathf.Max(0, 1+Mathf.Sin(DayNightCycle.main.timePassedAsFloat*0.781F+gameObject.GetInstanceID()*0.147F)), 2);
				float itemRotation = 9*Mathf.Max(0, 4F-itemOffset*3.5F);
				display.transform.position = transform.position+Vector3.up*(itemOffset+additionalRenderSpace);
				display.transform.Rotate(display.transform.up, Space.Self);
			}
		}
		
		public void updateStoredItem() {
			StorageContainer sc = getStorage();
			if (sc) {
				List<TechType> items = sc.container.GetItemTypes();
				if (items.Count > 0) {
					IList<InventoryItem> li = sc.container.GetItems(items[0]);
					if (li.Count > 0) {
						updateStoredItem(li[0]);
						return;
					}
				}
			}
			updateStoredItem(null);
		}
		
		public void updateStoredItem(InventoryItem ii) {
			//SNUtil.writeToChat("Set display to "+(ii != null && ii.item ? ii.item+"" : "null"));
			/*
			setDisplay(null);
			StorageContainer sc = getStorage();
			if (sc) {
				List<TechType> items = sc.container.GetItemTypes();
				if (items.Count > 0) {
					IList<InventoryItem> li = sc.container.GetItems(items[0]);
					if (li.Count > 0) {
						setDisplay(li[0].item.gameObject);
					}
				}
			}*/
			setDisplay(ii != null && ii.item ? ii.item.gameObject : null);
		}
		
		private void updateRotationSpeedTarget(ref float speed, ref float target) {
			if (Mathf.Approximately(speed, target)) {
				target = UnityEngine.Random.Range(-4F, 4F);
			}
			else if (speed > target) {
				speed = Mathf.Max(target, speed-0.15F);
			}
			else if (speed < target) {
				speed = Mathf.Min(target, speed+0.15F);
			}
		}
		
		private void setDisplay(GameObject go) {
			displayRender = null;
			if (display)
				UnityEngine.Object.DestroyImmediate(display);
			GameObject old = ObjectUtil.getChildObject(gameObject, DISPLAY_OBJECT_NAME);
			if (old)
				UnityEngine.Object.DestroyImmediate(display);
			if (!go)
				return;
			Renderer[] rr = go.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in rr) {
				if (r.GetComponent<Light>())
					continue;
				GameObject cloneFrom = r.gameObject;
				if (cloneFrom.GetFullHierarchyPath().Contains("$DisplayRoot")) {
					while (cloneFrom.transform.parent && !cloneFrom.name.Contains("$DisplayRoot"))
						cloneFrom = cloneFrom.transform.parent.gameObject;
					string seek = "offset=";
					int idx = cloneFrom.name.IndexOf(seek, StringComparison.InvariantCultureIgnoreCase);
					if (idx >= 0) {
						additionalRenderSpace = float.Parse(cloneFrom.name.Substring(idx+seek.Length), System.Globalization.CultureInfo.InvariantCulture);
					}
				}
				GameObject renderObj = UnityEngine.Object.Instantiate(cloneFrom);
				RenderUtil.convertToModel(renderObj);
				display = new GameObject(DISPLAY_OBJECT_NAME);
				renderObj.transform.SetParent(display.transform);
				display.SetActive(true);
				renderObj.SetActive(true);
				display.transform.SetParent(transform);
				display.transform.position = transform.position+Vector3.up;
				renderObj.transform.localPosition = Vector3.zero;
				renderObj.transform.localRotation = r.transform.localRotation;
				displayRender = r;
				break;
			}
		}
	}
}
