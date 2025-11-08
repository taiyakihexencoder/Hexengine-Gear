using UnityEngine;

namespace com.hexengine.gear.animation {
	public interface IModelSource {
		void StartLoad(string key);
		void Unload(string key);
		bool IsLoaded(string key);
		GameObject GetModel(string key);
	}
}