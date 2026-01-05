using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreeMatch
{
    public interface IAssetService : IDisposable
    {
        public bool IsInitialized { get; }

        public Task InitializeAsync(CancellationToken ct = default);

        public Task PreparePackAsync(BundleGroup pack, IProgress<float> progress = null, CancellationToken ct = default);

        public void ReleasePack(BundleGroup pack);
    }
}
