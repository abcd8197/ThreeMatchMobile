using System.Collections.Generic;
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
            CreateAssetManager(main);
            main.RegisterManager(new SceneManager());
            main.RegisterManager(new UIManager(main.GetManager<AssetManager>()));
            main.RegisterManager(new StageManager());
            main.RegisterManager(new AuthManager());
            main.RegisterManager(new GameManager());
            main.Build();
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
            List<string> requiredPacks = new();
#if UNITY_EDITOR
            assetService = new AddressableAssetService();
            requiredPacks.Add("defaultasset");
#endif
            var assetManager = new AssetManager(assetService, requiredPacks);
            main.RegisterManager(assetManager);
        }
    }
}