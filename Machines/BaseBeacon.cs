using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
	
	public class BaseBeacon : CustomMachine<BaseBeaconLogic> {
		
		public BaseBeacon(XMLLocale.LocaleEntry e) : base("basebeacon", e.name, e.desc, "8949b0da-5173-431f-a989-e621af02f942") {
			addIngredient(TechType.MapRoomUpgradeScanRange, 1);
			addIngredient(TechType.Beacon, 1);
			addIngredient(TechType.LEDLight, 1);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		public override bool isOutdoors() {
			return true;
		}
		
		public override void initializeMachine(GameObject go) {
			base.initializeMachine(go);
			ObjectUtil.removeComponent<PowerRelay>(go);
						
			BaseBeaconLogic lgc = go.GetComponent<BaseBeaconLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();/*
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			r.materials[0].SetFloat("_Shininess", 7.5F);
			r.materials[0].SetFloat("_Fresnel", 1F);
			r.materials[0].SetFloat("_SpecInt", 15F);
			lgc.mainRenderer = r;*/
			
			//go.GetComponent<Constructable>().model = go;
			//go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			//go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
			
			Beacon b = go.EnsureComponent<Beacon>();
			b.beaconActiveState = true;
			
			PingInstance ping = go.EnsureComponent<PingInstance>();
			ping.pingType = PingType.Beacon;
			ping.colorIndex = 0;
			ping.origin = go.transform;
			ping.minDist = 18f;
			ping.maxDist = 4000;
		}
		
	}
		
	public class BaseBeaconLogic : CustomMachineLogic {
		
		private Beacon beacon;
		private PingInstance ping;
		
		private string vehicleString = "";
		
		private float lastVehicleCalcTime = -1;
		private float lastInventoryCalcTime = -1;
		
		void Start() {
			SNUtil.log("Reinitializing base beacon");
			AqueousEngineeringMod.beaconBlock.initializeMachine(gameObject);
		}
		
		protected override void updateEntity(float seconds) {
			if (!beacon) {
				beacon = gameObject.GetComponent<Beacon>();
				ping = gameObject.GetComponent<PingInstance>();
			}
			if (seconds > 0 && beacon) {
				SubRoot sub = getSub();
				if (sub) {
					float time = DayNightCycle.main.timePassedAsFloat;
					if (time-lastVehicleCalcTime >= 0.5) {
						List<Vehicle> docked = new List<Vehicle>();
						VehicleDockingBay[] docks = sub.gameObject.GetComponentsInChildren<VehicleDockingBay>();
						if (docks.Length == 0) {
							vehicleString = "";
						}
						else {
							foreach (VehicleDockingBay dock in docks) {
								Vehicle v = dock.dockedVehicle;
								if (v)
									docked.Add(v);
							}
							vehicleString = docked.Count == 0 ? "No docked vehicles" : "Docked Vehicles: "+string.Join(", ", docked.Select<Vehicle, string>(v => v.GetName()));
						}
						lastVehicleCalcTime = time;
					}
					beacon.label = generateBeaconLabel(sub);
					ping.SetLabel(beacon.label);
				}
			}
		}
		
		private string generateBeaconLabel(SubRoot sub) {
			string loc = "Location: "+WorldUtil.getBiomeFriendlyName(WaterBiomeManager.main.GetBiome(transform.position, false))+" ("+(int)Ocean.main.GetDepthOf(gameObject)+"m)";
			string pw = "Power: "+sub.powerRelay.GetPower()+"/"+sub.powerRelay.GetMaxPower()+" ("+sub.powerRelay.powerStatus+")";
			string ret = loc+"\n"+pw;
			if (!string.IsNullOrEmpty(vehicleString))
				ret = ret+"\n"+vehicleString;
			return ret;
		}
	}
}
