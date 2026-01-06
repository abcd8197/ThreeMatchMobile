using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ThreeMatch
{
    public class AssetManager : IManager
    {
        private IAssetService _assetService;
        private readonly List<BundleGroup> _requiredPacks = new();

        public AssetManager(IAssetService assetService, List<string> requirePacks)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            if (requirePacks == null) throw new ArgumentNullException(nameof(requirePacks));

            foreach (var packName in requirePacks)
            {
                if (string.IsNullOrEmpty(packName)) continue;

                if (Enum.TryParse<BundleGroup>(packName, out var pack))
                {
                    _requiredPacks.Add(pack);
                }
                else
                {
                    Debug.LogWarning($"[AssetManager] Unknown pack name: {packName}");
                }
            }

            // 중복 제거
            _requiredPacks = _requiredPacks.Distinct().ToList();
            _assetService.LoadAssetPackAsync(_requiredPacks);
        }

        public void Dispose()
        {
            _assetService.ReleaseAssetPack(_requiredPacks);
            _assetService.Dispose();
            _requiredPacks.Clear();
        }

        public T GetNewInstance<T>(BundleGroup bundleGroup, string assetName, Transform parent) where T : UnityEngine.Object
        {
            return _assetService.GetAsset<T>(bundleGroup, assetName);
        }

        public void ReleaseInstance(BundleGroup group, string assetName)
        {
            _assetService.ReleaseAsset(group, assetName);
        }
    }
}
