using UnityEngine;

namespace com.hexengine.gear.animation {
	public interface IAnimationClipSource {
		void StartLoad(in string[] keys);
		void Unload(in string[] keys);
		bool IsLoaded(string key);
		AnimationClip GetClip(string key);
	}
}