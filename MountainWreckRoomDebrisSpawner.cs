using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.AqueousEngineering;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Assets;

namespace ReikaKalseki.AqueousEngineering {
	
	public class MountainWreckRoomDebrisSpawner : WorldGenerator {
		
		internal static readonly WeightedRandom<string> itemList = new WeightedRandom<string>();
		
		static MountainWreckRoomDebrisSpawner() {
			addItem("8fb8a082-d40a-4473-99ec-1ded36cc6813", 6);
			addItem("354ebf4e-def3-48a6-839d-bf0f478ca915", 6);
			addItem("423ab63d-38e0-4dd8-ab8d-fcd6c9ff0759", 6);
			addItem("65edb6a3-c1e6-4aaf-9747-108bd6a9dcc6", 6);
			addItem("7646d66b-01c0-4110-b6bf-305df024c2b1", 6);
			addItem("8b43e753-29a6-4365-bc53-822376d1cfa2", 6);
			addItem("8ba3be30-d89f-474b-87ca-94d3bfff25a4", 6);
			addItem("8c3d54c0-4330-4949-91ad-f046cfd67c7c", 6);
			addItem("a2104a9e-fe84-4c51-8874-69350507ef98", 6);
			addItem("af413920-4fe6-4447-9f62-4f04e605d6be", 6);
			addItem("c390fcfc-3bf4-470a-93bf-39dafb8b2267", 6);
			addItem("cc14ee20-80c5-4573-ae1b-68bebc0feadf", 6);
			addItem("d21bca5e-6dd2-48d8-bbf0-2f1d5df7fa9c", 6);
			addItem("ebc835bd-221a-4722-b1d0-becf08bd2f2c", 6);
			addItem("fb2886c4-7e03-4a47-a122-dc7242e7de5b", 6);
		}
	        
	    public MountainWreckRoomDebrisSpawner(Vector3 pos) : base(pos) {
			
	    }
		
		public override void saveToXML(XmlElement e) {
			
		}
		
		public override void loadFromXML(XmlElement e) {
			
		}
			
	    public override void generate(List<GameObject> li) {	
			for (int i = 0; i < 20; i++) {
				GameObject go = ObjectUtil.createWorldObject(itemList.getRandomEntry());
				if (!go)
					continue;
				go.transform.position = MathUtil.getRandomVectorAround(position, 3F);
				Rigidbody rb = go.EnsureComponent<Rigidbody>();
				rb.isKinematic = false;
				rb.velocity = MathUtil.getRandomVectorAround(Vector3.zero, 15);
				go.EnsureComponent<WorldForces>().underwaterGravity = 3;
				go.transform.localRotation = UnityEngine.Random.rotationUniform;					
				MountainWreckRoomDebrisItem prop = go.EnsureComponent<MountainWreckRoomDebrisItem>();
				prop.Invoke("fixInPlace", 30);
				li.Add(go);
			}
	    }
		
		public static void addItem(string item, int amt) {
			itemList.addEntry(item, amt);
		}
			
	}
	
	class MountainWreckRoomDebrisItem : MonoBehaviour {
		
		private static readonly Vector3 vent1 = new Vector3(-134.15F, -501, 940.29F);
		private static readonly Vector3 vent2 = new Vector3(-125.20F, -503, 936.16F);
		
		private Rigidbody body;
		
		private float time;
		
		void Update() {
			if (!body)
				body = GetComponentInChildren<Rigidbody>();
			time += Time.deltaTime;
			Vector3 pos = transform.position;
				
			if (time > 15F && body.velocity.magnitude < 0.03)
				fixInPlace();
		}
		
		void fixInPlace() {
			body.isKinematic = true;
			UnityEngine.Object.Destroy(this);
		}
		
	}
}
