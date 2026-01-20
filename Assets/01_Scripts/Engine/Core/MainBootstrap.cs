using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class MainBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (Main.IsInitialized)
            {
                Destroy(this.gameObject);
                return;
            }
            var main = Main.Instance;

            CreateSaveManager(main);
            CreateAssetManager(main);
            main.RegisterManager(new SceneManager());
            main.RegisterManager(new UIManager(main.GetManager<AssetManager>()));
            CreateStageManager(main);
            main.RegisterManager(new AuthManager());
            main.RegisterManager(new GameManager());
            main.RegisterManager(new ItemManager());
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
            requiredPacks.Add("defaultasset_tex");
            requiredPacks.Add("defaultasset");
#endif
            var assetManager = new AssetManager(assetService, requiredPacks);
            main.RegisterManager(assetManager);
        }

        private void CreateStageManager(Main main)
        {
            const string stageDataPath = "Datas/";

            var stageManager = new StageManager();
            stageManager.SetStageData(
                ResourcesDataLoader.LoadDataToList<StageMetaData>(stageDataPath + "StageData"),
                ResourcesDataLoader.LoadDataToList<StageGoalData>(stageDataPath + "StageGoalData"),
                ResourcesDataLoader.LoadDataToList<StageFixedCellSetData>(stageDataPath + "StageFixedCellData"),
                ResourcesDataLoader.LoadDataToList<StageSpawnRuleData>(stageDataPath + "StageSpawnRule")
                );

            main.RegisterManager(stageManager);
        }
    }
}