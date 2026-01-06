using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public interface IAssetService : IDisposable
    {
        public void LoadAssetPackAsync(IEnumerable<BundleGroup> bundleList, Action completeAction = null);
        public void ReleaseAssetPack(IEnumerable<BundleGroup> bundleList);

        public T GetAsset<T>(BundleGroup pack, string assetKey) where T : UnityEngine.Object;
        public void ReleaseAsset(BundleGroup pack, string assetKey);
    }
}
