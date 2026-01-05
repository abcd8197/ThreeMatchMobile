using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ThreeMatch
{
    public class AddressableAssetService : IAssetService
    {
        private bool _initialized = false;
        public bool IsInitialized => _initialized;

        public void Dispose()
        {
            
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            _initialized = true;
            return Task.CompletedTask;
        }



        public Task PreparePackAsync(BundleGroup pack, IProgress<float> progress = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        public void ReleasePack(BundleGroup pack)
        {
            throw new NotImplementedException();
        }
    }
}
