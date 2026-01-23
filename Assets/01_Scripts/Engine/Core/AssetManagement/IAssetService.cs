using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ThreeMatch
{
    public interface IAssetService : IDisposable
    {
        public Task LoadAssetPackAsync(IEnumerable<BundleGroup> bundleList, Action<int, float> progressAction = null, Action completeAction = null, CancellationToken ct = default);
        public void ReleaseAssetPack(IEnumerable<BundleGroup> bundleList);

        public T GetAsset<T>(BundleGroup pack, string assetKey) where T : UnityEngine.Object;
        public Sprite GetSprite(BundleGroup pack, string atlasName, string fileName);
        public GameObject GetPrefabInstance(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null);
        public T GetInstantiateComponent<T>(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null) where T : Component;

        public void ReleasePrefab(BundleGroup pack, string assetKey, GameObject go);
        public void ReleaseInstantiatedComponent<T>(BundleGroup pack, string assetKey, T asset) where T : Component;
    }
}
