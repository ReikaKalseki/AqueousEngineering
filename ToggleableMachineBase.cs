using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.AqueousEngineering {
		
	public abstract class ToggleableMachineBase : CustomMachineLogic {
		
		private float lastButtonCheck = -1;
		
		internal bool isEnabled;
		
		protected override void load(System.Xml.XmlElement data) {
			isEnabled = data.getBoolean("toggled");
		}
		
		protected override void save(System.Xml.XmlElement data) {
			data.addProperty("toggled", isEnabled);
		}
		
		internal void toggle() {
			isEnabled = !isEnabled;
		}
		
		protected override void updateEntity(float seconds) {
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastButtonCheck >= 1 && getSub()) {
				lastButtonCheck = time;
				foreach (BaseControlPanelLogic panel in getSub().GetComponentsInChildren<BaseControlPanelLogic>()) {
					panel.addButton(getButtonType());
				}
			}
			if (GameModeUtils.RequiresPower() && getSub() && getSub().powerRelay.GetPower() < 0.1F)
				isEnabled = false;
		}
		
		protected abstract HolographicControl getButtonType();
	}
}
