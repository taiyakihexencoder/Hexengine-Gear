using UnityEngine;

namespace com.hexengine.gear.parameters {
	[System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class)]
	public sealed class Parcelize : PropertyAttribute { 
		public readonly bool createTable;
		public readonly string[] namespaces;

		public Parcelize(bool createTable = true, params string[] namespaces) {
			this.createTable = createTable;
			this.namespaces = namespaces;
		}
	}
}
