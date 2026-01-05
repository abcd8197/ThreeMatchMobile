using UnityEngine;

namespace ThreeMatch
{
    public class MainBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            var main = Main.Instance;
            main.Initialize();

            CreateSaveManager(main);

            main.GetManager<SaveManager>().InitializeSaveData();

            CreateAssetManager(main);
        }

        private void CreateSaveManager(Main main)
        {
            SaveData saveData = null;
            ISaveService saveService = null;

#if UNITY_EDITOR
            saveService = new PlayerPrefService();
            saveData = saveService.LoadData();
#endif

            var saveManager = new SaveManager(saveData, saveService);
            main.RegisterManager(saveManager);
        }

        private void CreateAssetManager(Main main)
        {
            IAssetService assetService = null;
#if UNITY_EDITOR
            //assetService = new AddressableAssetService();
#endif
            var assetManager = new AssetManager(assetService, null);
            main.RegisterManager(assetManager);
        }
    }
}