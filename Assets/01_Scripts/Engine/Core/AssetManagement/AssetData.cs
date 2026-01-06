using UnityEngine.ResourceManagement.AsyncOperations;

namespace ThreeMatch
{
    public class AssetData
    {
        public AsyncOperationHandle Handler { get; private set; }
        public int RefCount { get; set; }
        public AssetData(AsyncOperationHandle handler)
        {
            Handler = handler;
            RefCount = 0;
        }
    }
}
