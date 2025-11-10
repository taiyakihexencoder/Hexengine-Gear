using UnityEngine;

namespace com.hexengine.gear.animation {
	public class ModelResourceLoader {
		private IModelSource modelSource;
		private IAnimationClipSource animationClipSource;

		public ModelResourceLoader(
			IModelSource modelSource,
			IAnimationClipSource animationClipSource
		) {
			this.modelSource = modelSource;
			this.animationClipSource = animationClipSource;
		}

		public bool IsLoaded(in ModelProfile profile) {
			if (!modelSource.IsLoaded(profile.resourceAddress)) {
				return false;
			}

			foreach (string clipAddress in profile.clipAddresses) {
				if (!animationClipSource.IsLoaded(clipAddress)) {
					return false;
				}
			}
			return true;
		}

		public void StartLoad(in ModelProfile profile) {
			modelSource.StartLoad(profile.resourceAddress);
			animationClipSource.StartLoad(profile.clipAddresses);
		}

		public GameObject GetModel(in ModelProfile profile) {
			return modelSource.GetModel(profile.resourceAddress);
		}

		public void Unload(in ModelProfile profile) {
			modelSource.Unload(profile.resourceAddress);
			animationClipSource.Unload(profile.clipAddresses);
		}
	}
}
