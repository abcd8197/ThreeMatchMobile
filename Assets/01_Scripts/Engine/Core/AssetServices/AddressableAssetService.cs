using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ThreeMatch
{
    public class AddressableAssetService : IAssetService
    {
        public T LoadAsset<T>(string assetPath) where T : class
        {
            return null;
        }

        public void ReleaseAsset<T>(T asset) where T : class
        {
            
        }

    }
}
