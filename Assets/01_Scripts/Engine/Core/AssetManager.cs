using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

namespace ThreeMatch
{
    public class AssetManager : IManager
    {
        private IAssetService _assetService;
        private readonly List<BundleGroup> _requiredPacks = new();
        private readonly Dictionary<BundleGroup, int> _packRefCounts = new();
        private readonly Dictionary<BundleGroup, Dictionary<string, UnityEngine.Object>> _loadedAssets = new();

        public AssetManager(IAssetService assetService, List<string> requirePacks)
        {
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            if(requirePacks == null) throw new ArgumentNullException(nameof(requirePacks));

            foreach (var packName in requirePacks)
            {
                if (string.IsNullOrEmpty(packName)) continue;

                if (Enum.TryParse<BundleGroup>(packName, out var pack))
                {
                    _requiredPacks.Add(pack);
                    _packRefCounts[pack] = 0;
                }
                else
                {
                    Debug.LogWarning($"[AssetManager] Unknown pack name: {packName}");
                }
            }
            // 중복 제거
            _requiredPacks = _requiredPacks.Distinct().ToList();
        }

        public void Dispose()
        {
            foreach(var pack in _packRefCounts.Keys.ToList())
                _assetService.ReleasePack(pack);
        }

        public async Task InitializeAsync(IProgress<float> progress = null, CancellationToken ct = default)
        {
            await _assetService.InitializeAsync(ct);

            if (_requiredPacks.Count == 0)
            {
                progress?.Report(1f);
                return;
            }

            
            float perPack = 1f / _requiredPacks.Count;
            float baseProgress = 0f;

            foreach (var pack in _requiredPacks)
            {
                ct.ThrowIfCancellationRequested();

                _packRefCounts[pack] = 1;

                var packProgress = new Progress<float>(p => progress?.Report(baseProgress + p * perPack));

                await _assetService.PreparePackAsync(pack, packProgress, ct);

                baseProgress += perPack;
                progress?.Report(baseProgress);
            }

            progress?.Report(1f);
        }

        public void ReleasePack(BundleGroup pack)
        {
            if (_packRefCounts.ContainsKey(pack))
            {
                _packRefCounts[pack]--;
                if (_packRefCounts[pack] <= 0)
                {
                    _assetService.ReleasePack(pack);
                    _packRefCounts.Remove(pack);
                }
            }
            else
            {
                Debug.LogWarning($"[AssetManager] Attempted to release a pack that is not loaded: {pack}");
            }
        }

        public T GetNewInstance<T>(BundleGroup bundleGroup, string assetName, Transform parent) where T : UnityEngine.Object
        {
            return _loadedAssets.TryGetValue(bundleGroup, out var assetsDict) && assetsDict.TryGetValue(assetName, out var asset)
                ? UnityEngine.Object.Instantiate(asset, parent) as T
                : null;
        }

        public void ReleaseInstance(UnityEngine.Object instance)
        {
            UnityEngine.Object.Destroy(instance);
        }
    }
}
