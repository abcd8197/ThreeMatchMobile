using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThreeMatch
{
    public class AddressableAssetService : IAssetService
    {
        private readonly Dictionary<BundleGroup, Dictionary<string, AssetData>> _loadedAssetDatas = new();
        private readonly Dictionary<BundleGroup, Dictionary<string, SpriteAtlas>> _loadedAltases = new();
        private readonly object _lock = new();

        public void Dispose()
        {
            foreach (var pack in new List<BundleGroup>(_loadedAssetDatas.Keys))
            {
                ReleaseAssetPack(pack);
            }
        }

        #region ## Load Methods ##
        public async Task LoadAssetPackAsync(IEnumerable<BundleGroup> bundleList, Action<int, float> progressAction = null, Action completeAction = null, CancellationToken ct = default)
        {
            Dictionary<BundleGroup, AsyncOperationHandle<IList<IResourceLocation>>> locationHandles = new();
            bool success = false;
            try
            {
                // Step 1 : 리소스 위치 불러오기
                foreach (var pack in bundleList)
                {
                    if (_loadedAssetDatas.ContainsKey(pack))
                        continue;

                    var label = pack.ToString();
                    var handle = Addressables.LoadResourceLocationsAsync(label);
                    await handle.Task;
                    locationHandles[pack] = handle;
                }


                // Step 2 : 유효한 리소스 위치 필터링, Total Count 계산
                int totalAssetCount = 0;
                var removePacks = new List<BundleGroup>();
                foreach (var kv in locationHandles)
                {
                    var pack = kv.Key;
                    var handle = kv.Value;

                    if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null || handle.Result.Count == 0)
                    {
                        removePacks.Add(pack);
                        string msg = $"{pack} Asset Location Load Failed.\nStatus: {handle.Status}\nResult: {handle.Result}";
                        if (handle.Result != null)
                            msg += $"\nCount: {handle.Result.Count}";
                        else
                            msg += "\nCount: null";
                        Debug.LogError(msg);
                        continue;
                    }

                    totalAssetCount += handle.Result.Count;
                }

                // Step 3 : 유효하지 않은 팩 제거
                foreach (var pack in removePacks)
                {
                    if (locationHandles.TryGetValue(pack, out var handle) && handle.IsValid())
                        Addressables.Release(handle);
                    locationHandles.Remove(pack);
                }

                // Step 3-1 : 로드할 에셋이 없는 경우 처리
                if (totalAssetCount == 0)
                {
                    progressAction?.Invoke(locationHandles.Count, 1f);
                    completeAction?.Invoke();
                    return;
                }

                // Step 4 : 로드 할 에셋 Queue에 삽입
                int loadedAssetBundleGroup = 0;
                int loadedAssetCount = 0;
                var queue = new Queue<(BundleGroup, IResourceLocation)>(totalAssetCount);
                var dickLock = new object();
                var dictAssetNumber = new Dictionary<BundleGroup, int>();

                foreach (var kv in locationHandles)
                {
                    dictAssetNumber.Add(kv.Key, 0);
                    foreach (var location in kv.Value.Result)
                    {
                        if (location.ResourceType == typeof(Sprite) || location.ResourceType == typeof(Texture2D))
                        {
                            totalAssetCount--;
                            continue;
                        }
                        queue.Enqueue((kv.Key, location));
                        dictAssetNumber[kv.Key]++;
                    }
                }

                // Step 5 : 병렬 로드 시작
                var qLock = new object();
                var workers = new Task[8];
                for (int i = 0; i < workers.Length; i++)
                {
                    workers[i] = WorkerAsync();
                }

                await Task.WhenAll(workers);
                success = true;

                // async가 없이 return Task.Run으로 감싸서 사용했더니
                // C#의 Thread Pool로 넘어가 Unity Addressable 동작을 하지 않는 것을 수정
                // Task.Run으로 감싼것을 없애고 async 키워드를 추가하여
                // Unity의 메인 스레드에서 동작하도록 "UnitySynchronizationContext"가 덮어쓸수있도록 수정
                async Task WorkerAsync()
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        (BundleGroup, IResourceLocation) item;
                        lock (qLock)
                        {
                            if (queue.Count == 0)
                                break;
                            item = queue.Dequeue();
                        }

                        await LoadAssetAsync(item.Item1, item.Item2, item.Item1.ToString().EndsWith("_tex"), ct);

                        lock (dickLock)
                        {
                            dictAssetNumber[item.Item1]--;
                            if (dictAssetNumber[item.Item1] <= 0)
                                loadedAssetBundleGroup++;
                        }

                        int done = Interlocked.Increment(ref loadedAssetCount);
                        progressAction?.Invoke(loadedAssetBundleGroup, (float)done / totalAssetCount);
                    }
                }
            }
            finally
            {
                foreach (var handle in locationHandles.Values)
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }

                if (success)
                {
                    progressAction?.Invoke(locationHandles.Count, 1f);
                    completeAction?.Invoke();
                }
            }
        }

        private async Task LoadAssetAsync(BundleGroup pack, IResourceLocation location, bool isAtlas, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (isAtlas)
            {
                if (location.ResourceType != typeof(SpriteAtlas))
                    Debug.LogError("It is not SpriteAtlas!");
                var h = Addressables.LoadAssetAsync<SpriteAtlas>(location);
                await h.Task;

                if (h.Status != AsyncOperationStatus.Succeeded || !h.IsValid())
                    throw new Exception($"LoadAssetAsync failed. pack={pack}, key={location.PrimaryKey}");
                StoreAssetData(pack, location, h, isAtlas);
            }
            else
            {
                var h = Addressables.LoadAssetAsync<object>(location);
                await h.Task;

                if (h.Status != AsyncOperationStatus.Succeeded || !h.IsValid())
                    throw new Exception($"LoadAssetAsync failed. pack={pack}, key={location.PrimaryKey}");
                StoreAssetData(pack, location, h, isAtlas);
            }
        }

        private void StoreAssetData(BundleGroup pack, IResourceLocation location, AsyncOperationHandle opHandle, bool isAtlas)
        {
            lock (_lock)
            {
                if (isAtlas)
                {
                    if(!_loadedAltases.TryGetValue(pack, out var dict))
                    {
                        dict = new();
                        _loadedAltases[pack] = dict;
                    }

                    string[] seperated = location.PrimaryKey.Split('.')[0].Split('/');
                    string fileName = seperated[seperated.Length - 1];
                    dict[fileName] = opHandle.Result as SpriteAtlas;
                }
                else
                {
                    if (!_loadedAssetDatas.TryGetValue(pack, out var dict))
                    {
                        dict = new();
                        _loadedAssetDatas[pack] = dict;
                    }

                    string[] seperated = location.PrimaryKey.Split('.')[0].Split('/');
                    string fileName = seperated[seperated.Length - 1];
                    dict[fileName] = new AssetData(opHandle);
                }
            }
        }
        #endregion

        #region ## Get Asset Methods ##
        public T GetAsset<T>(BundleGroup pack, string assetKey) where T : UnityEngine.Object
        {
            if (!_loadedAssetDatas.TryGetValue(pack, out var packAssets))
            {
                Debug.LogError($"{pack} is Not Loaded.");
                return null;
            }

            if (!packAssets.TryGetValue(assetKey, out var assetData))
            {
                Debug.LogError($"{pack}/{assetKey} is Not Loaded.");
                return null;
            }

            assetData.RefCount++;
            return assetData.Handler.Result as T;
        }

        public Sprite GetSprite(BundleGroup pack, string atlasName, string fileName)
        {
            if (!_loadedAltases.TryGetValue(pack, out var dict))
            {
                Debug.LogError($"{pack} is Not Loaded.");
                return null;
            }

            if (!dict.TryGetValue(atlasName, out var atlas))
            {
                Debug.LogError($"{pack} is Not Loaded.");
                return null;
            }

            var sprite = atlas.GetSprite(fileName);

            if(sprite == null)
            {
                Debug.LogError($"{pack}/{atlasName}/{fileName} is not exist.");
                return null;
            }

            return sprite;
        }

        public GameObject GetPrefabInstance(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null)
        {
            var prefab = GetAsset<GameObject>(pack, assetKey);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStay);
            return go;
        }

        public T GetInstantiateComponent<T>(BundleGroup pack, string assetKey, bool worldPositionStay = false, Transform parent = null) where T : Component
        {
            var prefab = GetPrefabInstance(pack, assetKey, worldPositionStay, parent);
            if (prefab == null)
                return null;
            if (prefab.TryGetComponent<T>(out var component))
            {
                return component;
            }
            else
            {
                Debug.LogError($"{pack}/{assetKey} does not have component of type {typeof(T).Name}");
                ReleasePrefab(pack, assetKey, prefab);
                return null;
            }
        }
        #endregion

        #region ## Release Methods ##
        public void ReleaseAssetPack(IEnumerable<BundleGroup> bundleList)
        {
            foreach (var pack in bundleList)
            {
                ReleaseAssetPack(pack);
            }
        }

        private void ReleaseAssetPack(BundleGroup pack)
        {
            if (!_loadedAssetDatas.ContainsKey(pack))
                return;

            foreach (var packAssets in _loadedAssetDatas[pack])
            {
                if (packAssets.Value.RefCount > 0)
                    Debug.LogError($"{pack}/{packAssets.Key} is still in use. RefCount: {packAssets.Value.RefCount}");
                Addressables.Release(packAssets.Value.Handler);
            }

            _loadedAssetDatas[pack].Clear();
            _loadedAssetDatas.Remove(pack);
        }
        public void ReleasePrefab(BundleGroup pack, string assetKey, GameObject go)
        {
            if (_loadedAssetDatas.TryGetValue(pack, out var packAssets) && packAssets.TryGetValue(assetKey, out var assetData))
            {
                assetData.RefCount--;
                UnityEngine.Object.Destroy(go);

                if (assetData.RefCount < 0)
                {
                    Debug.LogError($"{pack}/{assetKey} RefCount is below zero.");
                }
            }
        }

        public void ReleaseInstantiatedComponent<T>(BundleGroup pack, string assetKey, T asset) where T : Component
        {
            if (_loadedAssetDatas.TryGetValue(pack, out var packAssets) && packAssets.TryGetValue(assetKey, out var assetData))
            {
                assetData.RefCount--;
                UnityEngine.Object.Destroy(asset.gameObject);

                if (assetData.RefCount < 0)
                {
                    Debug.LogError($"{pack}/{assetKey} RefCount is below zero.");
                }
            }
        }
        #endregion
    }
}
