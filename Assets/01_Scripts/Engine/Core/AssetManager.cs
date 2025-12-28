using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class AssetManager : IManager
    {
        private IAssetService _assetService;
        private readonly Dictionary<string, Object> _loadedAssets = new();

        public AssetManager(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public void Dispose()
        {
            foreach (var asset in _loadedAssets.Values)
            {
                if (asset != null)
                    _assetService.ReleaseAsset(asset);
            }

            _loadedAssets?.Clear();
        }
    }
}
