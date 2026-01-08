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
        public float progress { get; private set; } = 0f;
        public bool AssetLoadCompleted { get; private set; } = false;

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
            progress = 0f;
            _assetService.LoadAssetPackAsync(_requiredPacks, SetProgress, LoadCompleteCallback);
        }

        public void Dispose()
        {
            _assetService.ReleaseAssetPack(_requiredPacks);
            _assetService.Dispose();
            _requiredPacks.Clear();
        }

        #region ## Get Methods ##
        /// <summary> Return Original Asset. Not Instantiated</summary>
        public T GetAsset<T>(BundleGroup pack, string assetName) where T : UnityEngine.Object
            => _assetService.GetAsset<T>(pack, assetName);

        /// <summary>Return new Prefab Instance that Instantiated</summary>
        public GameObject GetPrefabInstance(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null)
            => _assetService.GetPrefabInstance(pack, assetKey, worldPositionStay, parent);
        
        /// <summary>Return Component of the Instantiated Prefab's GameObject</summary>
        public T GetInstantiateComponent<T>(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null) where T : Component
            => _assetService.GetInstantiateComponent<T>(pack, assetKey, worldPositionStay, parent);
        #endregion

        #region ## Release Methods ##
        /// <summary>Release Prefab Instance that instantiated</summary>
        public void ReleasePrefab(BundleGroup pack, string assetName, GameObject go)
            => _assetService.ReleasePrefab(pack, assetName, go);

        /// <summary>Release Component of the Instatiated Prefab</summary>
        public void ReleaseInstantiatedComponent<T>(BundleGroup pack, string assetKey, T asset) where T : Component
            => _assetService.ReleaseInstantiatedComponent<T>(pack, assetKey, asset);

        #endregion

        private void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
        }

        private void LoadCompleteCallback()
        {
            AssetLoadCompleted = true;
        }
    }
}
