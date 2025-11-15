using System.Collections.Generic;
using System.IO;
using com.hexengine.gear.editor;

namespace com.hexengine.gear.ecs.editor {
	public static class GeometryDataGenerator {
		private static char sep => Path.DirectorySeparatorChar;
		private static string assemblyName => "com.hexengine.gear.ecs";

		public static void Generate() {
			GeometryPrefabTable table = ScriptableObjectUtility.GetProjectSingleton<GeometryPrefabTable>();
			HexengineProject.CreateTextFile(
				$"ecs{sep}Scripts{sep}physics{sep}GeometryPrefabRequest.cs",
				writer => {
					writer.WriteLine($"using Unity.Mathematics;");
					writer.WriteLine();
					writer.WriteLine($"namespace com.hexengine.gear.ecs {{");
					writer.WriteLine($"\tpublic static partial class GeometryPrefabRequest {{");

					writer.WriteLine($"\t\tprivate static partial class Layer {{");
					for (int i = 0; i < table.layerList.Length; ++i) {
						writer.WriteLine($"\t\t\tpublic const int { table.layerList[ table.layerIndexList[i] ] } = { 1 << table.layerIndexList[i] };");
					}
					writer.WriteLine($"\t\t}}");
					writer.WriteLine();

					writer.WriteLine($"\t\tpublic static partial class Key {{");
					for (int i = 0; i < table.keys.Length; ++i) {
						writer.WriteLine($"\t\t\tpublic const long { table.keys[ table.keyIndexList[i] ] } = {i+1} << 32;");
					}
					writer.WriteLine($"\t\t}}");
					writer.WriteLine();

					writer.WriteLine($"\t\tpublic static partial class GeometryId {{");
					{
						int id = 1;
						foreach (GeometryPrefabTable.GeometrySphere triggerSphere in table.triggerSphereList) {
							writer.WriteLine($"\t\t\tpublic const int {triggerSphere.name} = {id};");
							id++;
						}
						foreach(GeometryPrefabTable.GeometryBox triggerBox in table.triggerBoxList) {
							writer.WriteLine($"\t\t\tpublic const int {triggerBox.name} = {id};");
							id++;
						}
						foreach(GeometryPrefabTable.GeometryCylinder triggerCylinder in table.triggerCylinderList) {
							writer.WriteLine($"\t\t\tpublic const int {triggerCylinder.name} = {id};");
							id++;
						}
						foreach(GeometryPrefabTable.GeometrySphere collisionSphere in table.collisionSphereList) {
							writer.WriteLine($"\t\t\tpublic const int {collisionSphere.name} = {id};");
							id++;
						}
						foreach(GeometryPrefabTable.GeometryBox collisionBox in table.collisionBoxList) {
							writer.WriteLine($"\t\t\tpublic const int {collisionBox.name} = {id};");
							id++;
						}
						foreach(GeometryPrefabTable.GeometryCylinder collisionCylinder in table.collisionCylinderList) {
							writer.WriteLine($"\t\t\tpublic const int {collisionCylinder.name} = {id};");
							id++;
						}
						foreach (GeometryPrefabTable.GeometryCapsule character in table.characterGeometryList){
							writer.WriteLine($"\t\t\tpublic const int {character.name} = {id};");
							id++;
						}
					}
					writer.WriteLine($"\t\t}}");
					writer.WriteLine();

					writer.WriteLine($"\t\tpublic static partial class Trigger {{");
					foreach(GeometryPrefabTable.GeometrySphere triggerSphere in table.triggerSphereList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{triggerSphere.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateSphereGeometryPrefabRequest {triggerSphere.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateSphereGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, triggerSphere.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{triggerSphere.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{triggerSphere.name}\",");
						writer.WriteLine($"\t\t\t\t\tradius = {triggerSphere.radius}f,");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, triggerSphere.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, triggerSphere.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = false,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(triggerSphere.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}
					foreach(GeometryPrefabTable.GeometryBox triggerBox in table.triggerBoxList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{triggerBox.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateBoxGeometryPrefabRequest {triggerBox.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateBoxGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, triggerBox.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{triggerBox.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{triggerBox.name}\",");
						writer.WriteLine($"\t\t\t\t\textent = new float3({triggerBox.extent.x}f, {triggerBox.extent.y}f, {triggerBox.extent.z}f),");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, triggerBox.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, triggerBox.collidesWith)},");
						writer.WriteLine($"\t\t\t\t\thasBody = false,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(triggerBox.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}

					foreach(GeometryPrefabTable.GeometryCylinder triggerCylinder in table.triggerCylinderList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{triggerCylinder.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateCylinderGeometryPrefabRequest {triggerCylinder.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateCylinderGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, triggerCylinder.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{triggerCylinder.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{triggerCylinder.name}\",");
						writer.WriteLine($"\t\t\t\t\tradius = {triggerCylinder.radius}f,");
						writer.WriteLine($"\t\t\t\t\theight = {triggerCylinder.height}f,");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, triggerCylinder.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, triggerCylinder.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = false,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(triggerCylinder.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}
					writer.WriteLine($"\t\t}}");

					writer.WriteLine();
					
					writer.WriteLine($"\t\tpublic static partial class Collision {{");
					foreach(GeometryPrefabTable.GeometrySphere collisionSphere in table.collisionSphereList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{collisionSphere.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateSphereGeometryPrefabRequest {collisionSphere.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateSphereGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, collisionSphere.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{collisionSphere.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{collisionSphere.name}\",");
						writer.WriteLine($"\t\t\t\t\tradius = {collisionSphere.radius}f,");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, collisionSphere.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, collisionSphere.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = true,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(collisionSphere.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}
					foreach(GeometryPrefabTable.GeometryBox collisionBox in table.collisionBoxList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{collisionBox.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateBoxGeometryPrefabRequest {collisionBox.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateBoxGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, collisionBox.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{collisionBox.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{collisionBox.name}\",");
						writer.WriteLine($"\t\t\t\t\textent = new float3({collisionBox.extent.x}f, {collisionBox.extent.y}f, {collisionBox.extent.z}f),");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = {LayerFlagString(table.layerList, collisionBox.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, collisionBox.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = true,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(collisionBox.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}

					foreach(GeometryPrefabTable.GeometryCylinder collisionCylinder in table.collisionCylinderList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{collisionCylinder.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateCylinderGeometryPrefabRequest {collisionCylinder.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateCylinderGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, collisionCylinder.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{collisionCylinder.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{collisionCylinder.name}\",");
						writer.WriteLine($"\t\t\t\t\tradius = {collisionCylinder.radius}f,");
						writer.WriteLine($"\t\t\t\t\theight = {collisionCylinder.height}f,");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, collisionCylinder.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, collisionCylinder.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = true,");
						writer.WriteLine($"\t\t\t\t\teventType = { EventTypeString(collisionCylinder.eventType) },");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}

					writer.WriteLine($"\t\t}}");

					writer.WriteLine();

					writer.WriteLine($"\t\tpublic static partial class Character {{");
					
					foreach(GeometryPrefabTable.GeometryCapsule character in table.characterGeometryList) {
						writer.WriteLine($"\t\t\t[GeometryPrimitive(\"{character.uuid}\")]");
						writer.WriteLine($"\t\t\tpublic static CreateCapsuleGeometryPrefabRequest {character.name}() {{");
						writer.WriteLine($"\t\t\t\treturn new CreateCapsuleGeometryPrefabRequest {{");
						writer.WriteLine($"\t\t\t\t\tkey = {KeyString(table.keys, character.key)},");
						writer.WriteLine($"\t\t\t\t\tgeometryId = GeometryId.{character.name},");
						writer.WriteLine($"\t\t\t\t\tname = \"{character.name}\",");
						writer.WriteLine($"\t\t\t\t\tradius = {character.radius}f,");
						writer.WriteLine($"\t\t\t\t\theight = {character.height}f,");
						writer.WriteLine($"\t\t\t\t\tbelongsTo = { LayerFlagString(table.layerList, character.belongsTo) },");
						writer.WriteLine($"\t\t\t\t\tcollidesWith = { LayerFlagString(table.layerList, character.collidesWith) },");
						writer.WriteLine($"\t\t\t\t\thasBody = true,");
						writer.WriteLine($"\t\t\t\t}};");
						writer.WriteLine($"\t\t\t}}");
						writer.WriteLine();
					}

					writer.WriteLine($"\t\t}}");

					writer.WriteLine($"\t}}");
					writer.WriteLine($"}}");
				}
			);

			string asmrefPath = $"ecs{sep}Scripts{sep}{assemblyName}.asmref";
			if (!HexengineProject.Exists(asmrefPath)) {
				HexengineProject.CreateAssemblyReference(assemblyName, asmrefPath);
			}
		}

		private static string KeyString(in string[] names, int index) {
			return 0 <= index && index < names.Length ? $"Key.{names[index]}" : "-1";
		}

		private static string LayerFlagString(in string[] names, int flag) {
			List<string> result = new List<string>();
			for(int i = 0, bit = 1; i < names.Length; ++i, bit <<= 1) {
				if( (flag & bit) == bit ){
					result.Add($"Layer.{names[i]}");
				}
			}
			return result.Count > 0 ? string.Join(" | ", result) : "0";
		}

		private static string EventTypeString(ColliderEventType type) {
			List<string> result = new List<string>();
			foreach(ColliderEventType flag in System.Enum.GetValues(typeof(ColliderEventType))) {
				if (flag == ColliderEventType.None) { continue; }
				if (type.HasFlag(flag)) {
					result.Add($"{typeof(ColliderEventType).Name}.{flag}");
				}
			}

			return result.Count > 0 ? string.Join(" | ", result) : $"{typeof(ColliderEventType).Name}.{ColliderEventType.None}";
		}
	}
}