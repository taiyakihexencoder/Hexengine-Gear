using UnityEngine;

namespace com.hexengine.gear.parameters.editor {
	public abstract class ParameterTable<T> : ScriptableObject where T: Parcelable {
		[SerializeField]
		private T[] _records = new T[0];
		public T[] records => _records;
	}
}