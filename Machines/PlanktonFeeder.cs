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
	
	public class PlanktonFeeder : CustomMachine<PlanktonFeederLogic> {
		
		internal static readonly float POWER_COST = 0.5F; //per second
		internal static readonly float CONSUMPTION_RATE = 1F/240F; //per second
		internal static readonly float RANGE = 250F; //m
		
		internal static readonly Dictionary<TechType, WildFeedingBehavior> behaviors = new Dictionary<TechType, WildFeedingBehavior>();
		
		internal static BasicCraftingItem fuel;
		
		static PlanktonFeeder() {			
			behaviors[TechType.Peeper] = new WildFeedingBehavior(150, 60, 6, 0.15F, 10);
			behaviors[TechType.Bladderfish] = new WildFeedingBehavior(100, 60, 4, 0.05F, 10);
			behaviors[TechType.Reginald] = new WildFeedingBehavior(150, 40, 4, 0.1F, 10);
			behaviors[TechType.Hoopfish] = new WildFeedingBehavior(150, 40, 6, 0.15F, 10);
			behaviors[TechType.Spinefish] = new WildFeedingBehavior(180, 40, 9, 0.15F, 10);
			behaviors[TechType.Shuttlebug] = new WildFeedingBehavior(30, 20, 2, 0.05F, 5);
			behaviors[TechType.Eyeye] = new WildFeedingBehavior(200, 50, 2, 0.2F, 10);
			behaviors[TechType.LavaEyeye] = new WildFeedingBehavior(150, 30, 2, 0.1F, 10);
			behaviors[TechType.GhostLeviathan] = new WildFeedingBehavior(300, 0, 20, 0F, 40);
		}
		
		public PlanktonFeeder(XMLLocale.LocaleEntry e) : base("baseplanktonfeed", e.name, e.desc, "8fb8a082-d40a-4473-99ec-1ded36cc6813") {
			addIngredient(TechType.FiberMesh, 1);
			addIngredient(TechType.Pipe, 2);
			addIngredient(TechType.Titanium, 3);
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
			//ObjectUtil.removeComponent<>(go);
						
			PlanktonFeederLogic lgc = go.GetComponent<PlanktonFeederLogic>();
			
			Renderer r = go.GetComponentInChildren<Renderer>();
			RenderUtil.swapToModdedTextures(r, this);
		}
		
	}
		
	public class PlanktonFeederLogic : CustomMachineLogic {
		
		void Start() {
			SNUtil.log("Reinitializing base plankton feeder");
			AqueousEngineeringMod.planktonFeederBlock.initializeMachine(gameObject);
		}

		protected override float getTickRate() {
			return 1F;
		}
		
		protected override void updateEntity(float seconds) {
			//if (mainRenderer == null)
			//	mainRenderer = ObjectUtil.getChildObject(gameObject, "model").GetComponent<Renderer>();
			
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (Vector3.Distance(Player.main.transform.position, transform.position) >= PlanktonFeeder.RANGE)
				return;
			if (consumePower(PlanktonFeeder.POWER_COST, seconds) && getStorage().container.GetCount(PlanktonFeeder.fuel.TechType) > 0) {
				float r = PlanktonFeeder.RANGE;
				HashSet<Creature> set = WorldUtil.getObjectsNearWithComponent<Creature>(gameObject.transform.position, r);
				foreach (Creature c in set) {
					TechType tt = c.GetComponent<TechTag>().type;
					WildFeedingBehavior feed = PlanktonFeeder.behaviors.ContainsKey(tt) ? PlanktonFeeder.behaviors[tt] : null;
					if (feed != null) {
						float dd = Vector3.Distance(c.transform.position, transform.position);
						if (dd >= feed.maxAttractRange)
							continue;
						c.GetComponent<SwimBehaviour>().SwimTo(transform.position, feed.attractionSpeed);
						c.leashPosition = transform.position;
						StayAtLeashPosition leash = c.GetComponent<StayAtLeashPosition>();
						if (leash) {
							leash.leashDistance = feed.minRange;
							leash.swimVelocity = feed.attractionSpeed;
						}
						if (c is GhostLeviathan) {
							c.Aggression.Add(-0.005F*seconds);
						}
						c.Hunger.Add(-0.04F*seconds);
						if (dd <= feed.maxBreedRange && UnityEngine.Random.Range(0F, 1F) <= feed.breedChance*seconds) {
							tryBreed(c);
						}
					}
				}
				if (set.Count > 0 && UnityEngine.Random.Range(0F, 1F) <= PlanktonFeeder.CONSUMPTION_RATE*seconds) {
					getStorage().container.DestroyItem(PlanktonFeeder.fuel.TechType);
				}
			}
		}
		
		private void tryBreed(Creature c) {
			GameObject clone = ObjectUtil.createWorldObject(c.GetComponent<PrefabIdentifier>().ClassId);
			clone.transform.position = MathUtil.getRandomVectorAround(c.transform.position, 2);
		}
	}
	
	class WildFeedingBehavior {
		
		public readonly float maxAttractRange;
		public readonly float maxBreedRange;
		public readonly float attractionSpeed;
		public readonly float breedChance;
		public readonly float minRange;
		
		internal WildFeedingBehavior(float r, float br, float s, float c, float m) {
			maxAttractRange = r;
			maxBreedRange = br;
			attractionSpeed = s;
			breedChance = c;
			minRange = m;
		}
		
	}
}
