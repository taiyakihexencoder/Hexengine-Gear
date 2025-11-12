using System.Reflection;
using UnityEditor;

namespace com.hexengine.gear {
	public static class SerializedPropertyExtensions {
 		public static void SetUUIDValue(this SerializedProperty property, UUID uuid) {
			System.Type type = typeof(UUID);
			FieldInfo fieldA = type.GetField("a", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo fieldB = type.GetField("b", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo fieldC = type.GetField("c", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo fieldD = type.GetField("d", BindingFlags.Instance | BindingFlags.NonPublic);

			property.FindPropertyRelative("a").intValue = (int)fieldA.GetValue(uuid);
			property.FindPropertyRelative("b").intValue = (short)fieldB.GetValue(uuid);
			property.FindPropertyRelative("c").intValue = (short)fieldC.GetValue(uuid);
			property.FindPropertyRelative("d").longValue = (long)fieldD.GetValue(uuid);
		}

		public static UUID GetUUIDValue(this SerializedProperty property) {
			return UUID.Get(
				property.FindPropertyRelative("a").intValue,
				(short)property.FindPropertyRelative("b").intValue,
				(short)property.FindPropertyRelative("c").intValue,
				property.FindPropertyRelative("d").longValue
			);
		}

	}
}