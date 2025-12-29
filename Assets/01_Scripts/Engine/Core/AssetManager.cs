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

        }
    }
}
