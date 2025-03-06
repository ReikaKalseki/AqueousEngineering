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
	
	public class MoonpoolRotationSystem : SaveSystem.SaveHandler {
		
		public static readonly string BASECELL_ID = "9d3e9fa5-a5ac-496e-89f4-70e13c0bedd5";
		public static readonly string MOONPOOL_NAME = "BaseMoonpool(Clone)";
		
		private static readonly SoundManager.SoundData rotateSound = SoundManager.registerSound(AqueousEngineeringMod.modDLL, "rotatemoonpool", "Sounds/rotatemoonpool.ogg", SoundManager.soundMode3D, s => {SoundManager.setup3D(s, 64);}, SoundSystem.masterBus);
			
		public static readonly MoonpoolRotationSystem instance = new MoonpoolRotationSystem();
		
		private MoonpoolRotationSystem() {
			SaveSystem.addSaveHandler(BASECELL_ID, this);
			//SaveSystem.addSaveHandler(Base.pieces[(int)Base.Piece.Moonpool].prefab.GetComponent<PrefabIdentifier>().ClassId, this);
		}
		
		public void processObject(GameObject go) {
			if (go.name == MOONPOOL_NAME) {
				MoonpoolRotationManager mgr = go.EnsureComponent<MoonpoolRotationManager>();
			}
		}
    
		public void rotateMoonpool(HolographicControl.HolographicControlTag btn) {
			MoonpoolRotationManager mgr = btn.gameObject.FindAncestor<MoonpoolRotationManager>();
			if (mgr) {
				/*
				bool big = btn.controlRef == AqueousEngineeringMod.moonPoolRotateP90 || btn.controlRef == AqueousEngineeringMod.moonPoolRotateM90;
				bool up = btn.controlRef == AqueousEngineeringMod.moonPoolRotateP90 || btn.controlRef == AqueousEngineeringMod.moonPoolRotateP15;
				mgr.addRotation(big, up);
				 */mgr.setRotation(mgr.desiredRotation+180);
			}
		}
		
		public void rebuildBase(Base b) {
			foreach (MoonpoolRotationManager mgr in b.GetComponentsInChildren<MoonpoolRotationManager>()) {
				mgr.Invoke("rebuildButtons", 0.25F); //small delay to allow finish init
			}
		}
		
		public override void save(PrefabIdentifier pi) {
			MoonpoolRotationManager mgr = pi.GetComponentInChildren<MoonpoolRotationManager>();
			if (mgr) {
				SaveSystem.saveToXML(data, "rotation", mgr.desiredRotation);
			}
		}
		
		public override void load(PrefabIdentifier pi) {
			MoonpoolRotationManager mgr = pi.GetComponentInChildren<MoonpoolRotationManager>();
			if (mgr) {
				mgr.setRotation((float)data.getFloat("rotation", 0), false);
			}
		}
		
		public class MoonpoolRotationManager : MonoBehaviour {
			
			private Transform rotatable;
			private VehicleDockingBay dock;
			private BaseDeconstructable geometry;			
		/*
			private GameObject buttonHolder1;
			private GameObject buttonHolder2;
			private HolographicControl.HolographicControlTag button1;
			private HolographicControl.HolographicControlTag button2;
			*/
			
			public GameObject buttonHolder;
			private readonly List<ButtonMount> buttons = new List<ButtonMount>();
			
			//unfortunately cannot make anything but 180 because the walkways are part of the mesh with the arms and will float if anything else
			//public static float SMALL_STEP = 15;
			//public static float BIG_STEP = 90;
			
			public float desiredRotation {get; private set; }
			
			void Start() {
				dock = GetComponentInChildren<VehicleDockingBay>();
				rotatable = ObjectUtil.getChildObject(gameObject, "Launchbay_cinematic").transform;
				geometry = GetComponent<BaseDeconstructable>();
				rebuildButtons();
				/*
				buttonHolder1 = ObjectUtil.getChildObject(gameObject, "ButtonHolder1");
				if (!buttonHolder1) {
					buttonHolder1 = new GameObject("ButtonHolder1");
					buttonHolder1.transform.SetParent(transform);
				}
				buttonHolder1.transform.localPosition = new Vector3(0, 0.5F, -5.62F);
				buttonHolder1.transform.localRotation = Quaternion.identity;
				buttonHolder2 = ObjectUtil.getChildObject(gameObject, "ButtonHolder1");
				if (!buttonHolder2) {
					buttonHolder2 = new GameObject("ButtonHolder2");
					buttonHolder2.transform.SetParent(transform);
				}
				buttonHolder2.transform.localPosition = new Vector3(0, 0.5F, 5.62F);
				buttonHolder2.transform.localRotation = Quaternion.identity;
				//buttons = HolographicControl.addButtons(buttonHolder, AqueousEngineeringMod.moonPoolRotateP90, AqueousEngineeringMod.moonPoolRotateM90, AqueousEngineeringMod.moonPoolRotateP15, AqueousEngineeringMod.moonPoolRotateM15);
				button1 = HolographicControl.addButton(buttonHolder1, AqueousEngineeringMod.rotateMoonpool);
				button2 = HolographicControl.addButton(buttonHolder2, AqueousEngineeringMod.rotateMoonpool);
				//alignButtons();*/
			}
			
			public void reset() {
				buttons.Clear();
				UnityEngine.Object.DestroyImmediate(buttonHolder);
				rebuildButtons();
			}
			
			public void rebuildButtons() {
				foreach (ButtonMount b in buttons) {
					b.destroy();
				}
				buttons.Clear();
				
				buttonHolder = ObjectUtil.getChildObject(gameObject, "ButtonHolder");
				if (!buttonHolder) {
					buttonHolder = new GameObject("ButtonHolder");
					buttonHolder.transform.SetParent(transform);
					Utils.ZeroTransform(buttonHolder.transform);
				}
				
				foreach (BaseUpgradeConsoleGeometry geo in gameObject.FindAncestor<BaseRoot>().GetComponentsInChildren<BaseUpgradeConsoleGeometry>()) {
					if (geo) {
						BaseDeconstructable con = geo.GetComponent<BaseDeconstructable>();
						if (con && con.face != null && con.face.Value.cell.Within(geometry.bounds.mins, geometry.bounds.maxs)) {
							buttons.Add(new ButtonMount(buttonHolder, geo));
						}
					}
				}
				
				SNUtil.log("Added moonpool rotation buttons: "+buttons.toDebugString(), AqueousEngineeringMod.modDLL);
			}
		/*
			private void alignButtons() {
				float offset = -0.4F + buttons.Length * 0.3125F;
				for (int i = 0; i < buttons.Length; i++) {
					HolographicControl.HolographicControlTag tag = buttons[i];
					float f = (2F / buttons.Length) * i - offset;
					tag.transform.parent.localPosition = new Vector3(f, 0, 0.1F);
					tag.transform.parent.localScale = new Vector3(2, 2, 1F);
					tag.transform.localRotation = Quaternion.identity;
					tag.transform.parent.localRotation = Quaternion.identity;
				}
			}*/
			/*
			public void addRotation(bool big = true, bool up = true) {
				setRotation(desiredRotation + (big ? BIG_STEP : SMALL_STEP) * (up ? 1 : -1));
			}*/
			
			public void setRotation(float rot, bool vb = true) {
				rot = ((rot % 360F) + 360) % 360F;
				if (Mathf.Abs(rot - desiredRotation) < 0.1)
					return;
				desiredRotation = rot;
				if (rotatable)
					rotatable.transform.localEulerAngles = new Vector3(0, desiredRotation, 0);
				if (vb) { 
					SoundManager.playSoundAt(rotateSound, rotatable.transform.position, false, 40, 0.67F);
				}
			}
			
			internal void load(XmlElement data) {
				setRotation((float)data.getFloat("angle", 0), false);
			}
			
			internal void save(XmlElement data) {
				data.addProperty("angle", desiredRotation);
			}
			
		}
		
		public class ButtonMount {
			
			public readonly BaseUpgradeConsoleGeometry mount;
			public readonly HolographicControl.HolographicControlTag button;
			
			public ButtonMount(GameObject holder, BaseUpgradeConsoleGeometry geo) {
				mount = geo;
				button = HolographicControl.addButton(holder, AqueousEngineeringMod.rotateMoonpool);
				if (!button)
					throw new Exception("Failed to instantiate button in "+geo.gameObject.GetFullHierarchyPath());
				button.transform.parent.localEulerAngles = mount.transform.localEulerAngles+new Vector3(0, 90, 0);
				button.transform.parent.position = mount.transform.position+new Vector3(0.0F, 0.4F, 0.0F)+mount.transform.right*-1.08F;
				button.transform.localScale = button.transform.localScale*2;
				button.GetComponent<SphereCollider>().radius *= 0.25F;
			}
			
			public void destroy() {
				if (button)
					UnityEngine.Object.DestroyImmediate(button.gameObject);
			}
			
			public override string ToString() {
				return (button ? (button.controlRef+" @ "+button.transform.position) : "NULL BTN")+" in "+(mount ? mount.geometryFace.cell.ToString() : "NULL");
			}

			
		}
   	
	}
	
}
