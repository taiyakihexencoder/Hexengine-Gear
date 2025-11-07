using UnityEngine;

namespace com.hexengine.gear.animation {
	public interface IAnimationClipSource {
		AnimationClip GetClip(string key);
	}
}