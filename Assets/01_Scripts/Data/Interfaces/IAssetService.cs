using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreeMatch
{
    public interface IAssetService : IDisposable
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <typeparam name="T"></typeparam>
        /// <param name="key">Addressable : address/key, Resources : path</param>
        /// <returns></returns>
        public Task<T> LoadAssetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        public void ReleaseAsset(string key);
        public void ReleaseAllAssets();

        public Task<object> InstantiateAsync<T>(string key, T parentTransform, CancellationToken cancellationToken = default) where T : class;
        public void ReleaseInstance(object instance);
        public void ReleaseAllInstances();
    }
}
