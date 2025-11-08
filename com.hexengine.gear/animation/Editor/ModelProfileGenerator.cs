using System.IO;
using com.hexengine.gear.editor;
using UnityEditor;

namespace com.hexengine.gear.animation.editor {
	public static class ModelProfileGenerator {
		[MenuItem("Hexengine/animation/Generate model profile list")]
		public static void GenerateModelProfiles() {
		string exportPath = $"animation{Path.DirectorySeparatorChar}Scripts";
		HexengineGearConfig config = ScriptableObjectUtility.GetProjectSingleton<HexengineGearConfig>();
			PoseParameterTable table = ScriptableObjectUtility.GetProjectSingleton<PoseParameterTable>();
			HexengineProject.CreateTextFile(
				$"{exportPath}{Path.DirectorySeparatorChar}support{Path.DirectorySeparatorChar}ModelProfile.cs",
				writer => {
					Write(writer, table);
				}
			);

			if (!string.IsNullOrEmpty(config.animationScriptAssembly)) {
				string path = $"{exportPath}{Path.DirectorySeparatorChar}{config.animationScriptAssembly}.asmref";
				if(!HexengineProject.Exists(path)) {
					HexengineProject.CreateAssemblyReference(config.animationScriptAssembly, path);
				}
			}
		}

		private static void Write(StreamWriter writer, PoseParameterTable table)
		{
			writer.WriteLine($"namespace com.hexengine.gear.animation {{");
			writer.WriteLine($"\tpublic partial struct ModelProfile {{");
			writer.WriteLine($"\t\tpublic enum Id {{");
			foreach (PoseParameterTable.CharacterPoses poses in table.poseList) {
				writer.WriteLine($"\t\t\t{poses.name},");
			}
			writer.WriteLine($"\t\t}}");
			writer.WriteLine();

			foreach (PoseParameterTable.CharacterPoses poses in table.poseList) {
				writer.WriteLine($"\t\tpublic static readonly ModelProfile {poses.name} = new ModelProfile {{");
				writer.WriteLine($"\t\t\tresourceAddress = \"{poses.resourceName}\",");
				writer.WriteLine($"\t\t\tclipAddresses = new string[] {{");
				foreach (PoseParameterTable.BasePoseParameter basePose in poses.basePoseParameters) {
					writer.WriteLine($"\t\t\t\t\"{basePose.clipName}\",");
				}
				foreach (PoseParameterTable.OverridePoseParameter overridePose in poses.overridePoseParameters) {
					writer.WriteLine($"\t\t\t\t\"{overridePose.clipName}\",");
				}
				foreach (PoseParameterTable.AdditivePoseParameter additivePose in poses.additivePoseParameters) {
					writer.WriteLine($"\t\t\t\t\"{additivePose.clipName}\",");
				}
				writer.WriteLine($"\t\t\t}},");
				writer.WriteLine($"\t\t}};");
				writer.WriteLine();
			}

			writer.WriteLine($"\t\tpublic static ModelProfile GetProfile(Id id) {{");
			writer.WriteLine($"\t\t\tswitch (id) {{");
			foreach (PoseParameterTable.CharacterPoses poses in table.poseList) {
				writer.WriteLine($"\t\t\t\tcase Id.{poses.name}: return {poses.name};");
			}
			writer.WriteLine($"\t\t\t\tdefault: return default;");
			writer.WriteLine($"\t\t\t}}");
			writer.WriteLine($"\t\t}}");
			writer.WriteLine($"\t}}");
			writer.WriteLine($"}}");
		}

	}
}