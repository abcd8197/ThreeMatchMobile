using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class AssetManager : IManager
    {
        private IAssetService _assetService;
        private readonly Dictionary<BundleGroup, Dictionary<AssetType, Object>> _assetCache = new();

        public AssetManager(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public void Dispose()
        {

        }

    }
}
