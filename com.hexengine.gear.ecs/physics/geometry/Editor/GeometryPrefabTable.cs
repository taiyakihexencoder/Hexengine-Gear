using UnityEngine;

namespace com.hexengine.gear.ecs.editor {
	public sealed class GeometryPrefabTable : ScriptableObject {
		[System.Serializable]
		public struct GeometrySphere {
			public int key;
			public string name;
			public UUID uuid;
			public float radius;
			public int belongsTo;
			public int collidesWith;
			public ColliderEventType eventType;
		}

		[System.Serializable]
		public struct GeometryBox {
			public int key;
			public string name;
			public UUID uuid;
			public Vector3 extent;
			public int belongsTo;
			public int collidesWith;
			public ColliderEventType eventType;
		}

		[System.Serializable]
		public struct GeometryCylinder {
			public int key;
			public string name;
			public UUID uuid;
			public float radius;
			public float height;
			public int belongsTo;
			public int collidesWith;
			public ColliderEventType eventType;
		}

		[System.Serializable]
		public struct GeometryCapsule {
			public int key;
			public string name;
			public UUID uuid;
			public float radius;
			public float height;
			public int belongsTo;
			public int collidesWith;
		}

		[SerializeField]
		private string[] _layerList = new string[0];
		public string[] layerList => _layerList;

		[SerializeField]
		private int[] _layerIndexList = new int[0];
		public int[] layerIndexList => _layerIndexList;

		[SerializeField]
		private string[] _keys = new string[0];
		public string[] keys => _keys;

		[SerializeField]
		private int[] _keyIndexList = new int[0];
		public int[] keyIndexList => _keyIndexList;

		[SerializeField]
		private GeometrySphere[] _triggerSphereList = new GeometrySphere[0];
		public GeometrySphere[] triggerSphereList => _triggerSphereList;

		[SerializeField]
		private GeometryBox[] _triggerBoxList = new GeometryBox[0];
		public GeometryBox[] triggerBoxList => _triggerBoxList;

		[SerializeField]
		private GeometryCylinder[] _triggerCylinderList = new GeometryCylinder[0];
		public GeometryCylinder[] triggerCylinderList => _triggerCylinderList;


		[SerializeField]
		private GeometrySphere[] _collisionSphereList = new GeometrySphere[0];
		public GeometrySphere[] collisionSphereList => _collisionSphereList;

		[SerializeField]
		private GeometryBox[] _collisionBoxList = new GeometryBox[0];
		public GeometryBox[] collisionBoxList => _collisionBoxList;

		[SerializeField]
		private GeometryCylinder[] _collisionCylinderList = new GeometryCylinder[0];
		public GeometryCylinder[] collisionCylinderList => _collisionCylinderList;

		[SerializeField]
		private GeometryCapsule[] _characterGeometryList = new GeometryCapsule[0];
		public GeometryCapsule[] characterGeometryList => _characterGeometryList;
	}
}