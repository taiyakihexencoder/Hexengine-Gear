using UnityEngine;

namespace com.hexengine.gear.animation.editor {
	public class PoseParameterTable : ScriptableObject {
		[System.Serializable]
		public class BasePoseParameter {
			public string name;
			public string clipName;
			public float weight;
			public double speed;
		}

		[System.Serializable]
		public class OverridePoseParameter {
			public string name;
			public string clipName;
			public bool active;
			public double speed;
		}

		[System.Serializable]
		public class AdditivePoseParameter {
			public string name;
			public string clipName;
			public float weight;
			public double speed;
		}

		[System.Serializable]
		public class CharacterPoses {
			public string name;
  			public int defaultPoseIndex;
			public BasePoseParameter[] basePoseParameters;
			public OverridePoseParameter[] overridePoseParameters;
			public AdditivePoseParameter[] additivePoseParameters;
		}

		[SerializeField]
		private CharacterPoses[] _poseList;
		public CharacterPoses[] poseList => _poseList;
	}
}