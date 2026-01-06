using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThreeMatch
{
    public class AddressableAssetService : IAssetService
    {
        private readonly Dictionary<BundleGroup, Dictionary<string, AssetData>> _loadedAssetDatas = new();

        public void Dispose()
        {
            foreach(var pack in _loadedAssetDatas.Keys)
            {
                ReleaseAssetPack(pack);
            }
        }

        public async void LoadAssetPackAsync(IEnumerable<BundleGroup> bundleList, Action completeAction = null)
        {
            foreach (var pack in bundleList)
            {
                await Task.Run(() => LoadPack(pack));
            }

            async void LoadPack(BundleGroup pack)
            {
                if (_loadedAssetDatas.ContainsKey(pack))
                    return;

                AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(pack.ToString(), Addressables.MergeMode.Union);

                await locationHandle.Task;

                if (!locationHandle.IsValid())
                    return; 

                CancellationTokenSource source = new();
                CancellationToken semCt = source.Token;
                SemaphoreSlim semaphore = new(8, 8);

                foreach (var location in locationHandle.Result)
                {
                    semCt.ThrowIfCancellationRequested();

                    AsyncOperationHandle handle = Addressables.LoadAssetAsync<object>(location);
                    handle.Completed += op =>
                    {
                        if (handle.IsValid())
                        {
                            if (!_loadedAssetDatas.ContainsKey(pack))
                                _loadedAssetDatas[pack] = new();
                            if (!_loadedAssetDatas[pack].ContainsKey(location.PrimaryKey))
                                _loadedAssetDatas[pack][location.PrimaryKey] = new AssetData(handle);

                            semaphore.Wait(semCt);
                        }
                        else
                            source.Cancel(true);
                    };
                    await handle.Task;
                }

                Addressables.Release(locationHandle);
                semaphore.Release();
                semaphore.Dispose();
            }
        }

        public void ReleaseAssetPack(IEnumerable<BundleGroup> bundleList)
        {
            foreach (var pack in bundleList)
            {
                ReleaseAssetPack(pack);
            }
        }

        private void ReleaseAssetPack(BundleGroup pack)
        {
            foreach (var packAssets in _loadedAssetDatas[pack])
            {
                Addressables.Release(packAssets.Value.Handler);
            }

            _loadedAssetDatas[pack].Clear();
            _loadedAssetDatas.Remove(pack);
        }

        public T GetAsset<T>(BundleGroup pack, string assetKey) where T : UnityEngine.Object
        {
            if (_loadedAssetDatas.ContainsKey(pack))
            {
                if (_loadedAssetDatas[pack].ContainsKey(assetKey))
                {
                    var handle = _loadedAssetDatas[pack][assetKey];
                    _loadedAssetDatas[pack][assetKey].RefCount++;
                    return Addressables.InstantiateAsync(handle) as T;
                }
                else
                {
                    Debug.LogError($"{pack}/{assetKey} is Not Loaded.");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"{pack} is Not Loaded.");
                return null;
            }
        }

        public void ReleaseAsset(BundleGroup pack, string assetKey)
        {
            if(_loadedAssetDatas.TryGetValue(pack, out var packAssets) && _loadedAssetDatas[pack].TryGetValue(assetKey, out var assetData))
            {
                assetData.RefCount--;
                //if (assetData.RefCount <= 0)
                //{
                //    Addressables.Release(assetData.Handler);
                //}
            }
        }
    }
}
