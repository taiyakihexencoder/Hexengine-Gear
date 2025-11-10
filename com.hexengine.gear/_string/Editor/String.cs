using UnityEditor;

namespace com.hexengine.gear.editor {
	internal static class Res {
		internal static class String {
			internal static string add => L10n.Tr("add");
			internal static string cancel => L10n.Tr($"cancel");
			internal static string delete => L10n.Tr("delete");
			internal static string duplicate => L10n.Tr("duplicate");
			internal static string export => L10n.Tr("export");
			internal static string insert => L10n.Tr("insert");
			internal static string move_down => L10n.Tr("move_down");
			internal static string move_up => L10n.Tr("move_up");
			internal static string ok => L10n.Tr("ok");

			internal static class Animation {
				private const string PREFIX = "Animation";
				internal static string profile => L10n.Tr($"{PREFIX}.profile");
				internal static string resource_name => L10n.Tr($"{PREFIX}.resource_name");
				internal static string base_pose => L10n.Tr($"{PREFIX}.base_pose");
				internal static string override_pose => L10n.Tr($"{PREFIX}.override_pose");
				internal static string additive_pose => L10n.Tr($"{PREFIX}.additive_pose");
				internal static string pose_name => L10n.Tr($"{PREFIX}.pose_name");
				internal static string clip_key => L10n.Tr($"{PREFIX}.clip_key");
				internal static string weight => L10n.Tr($"{PREFIX}.weight");
				internal static string speed => L10n.Tr($"{PREFIX}.speed");
				internal static string is_default_pose => L10n.Tr($"{PREFIX}.is_default_pose");
				internal static string is_active => L10n.Tr($"{PREFIX}.is_active");
				internal static string auto => L10n.Tr($"{PREFIX}.auto");
				internal static string validation_export_error => L10n.Tr($"{PREFIX}.validation_export_error");
				internal static string validation_failed_profile_name => L10n.Tr($"{PREFIX}.validation_failed_profile_name");
				internal static string validation_failed_empty_pose_name => L10n.Tr($"{PREFIX}.validation_failed_empty_pose_name");
				internal static string validation_failed_no_default_pose => L10n.Tr($"{PREFIX}.validation_failed_no_default_pose");
				internal static string validation_duplicated_name => L10n.Tr($"{PREFIX}.validation_duplicated_name");
				internal static string validation_duplicated_clip_name => L10n.Tr($"{PREFIX}.validation_duplicated_clip_name");
				internal static string validation_invalid_name => L10n.Tr($"{PREFIX}.validation_invalid_name");
			}
		}
	}
}