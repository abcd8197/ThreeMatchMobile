using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;

namespace ThreeMatch
{
    public class AddressableAssetService : IAssetService
    {
        private readonly Dictionary<BundleGroup, Dictionary<AssetType, Dictionary<string, object>>> _bundleManifest = new();

        public void Dispose()
        {
            foreach(var bundleGroup in _bundleManifest.Values)
            {
                foreach(var assetType in bundleGroup.Values)
                {
                    assetType.Clear();
                }
                bundleGroup.Clear();
            }
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<object> InstantiateAsync<T>(string key, T parentTransform, CancellationToken cancellationToken = default) where T : class
        {
            return Task.FromResult<object>(null);
        }

        public Task<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            return Task.FromResult<T>(null);
        }

        public void ReleaseAllAssets()
        {
            
        }

        public void ReleaseAllInstances()
        {
            
        }

        public void ReleaseAsset(string key)
        {
            
        }

        public void ReleaseInstance(object instance)
        {
            
        }
    }
}
