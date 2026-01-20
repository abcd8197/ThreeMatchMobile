using UnityEngine;
using NUnit.Framework;
using UnityEditor.TestRunner;


namespace ThreeMatch.UnitTest
{
    public class SaveDataIntergrityTest
    {
        [SetUp]
        public void SetUp()
        {
            var main = Main.Instance;
            SetUpSave(main);
            main.GetManager<SaveManager>().InitializeSaveData();
        }

        private void SetUpSave(Main main)
        {
            SaveData saveData = null;
            ISaveService saveService = null;

            saveService = new PlayerPrefService();
            saveData = saveService.LoadData();

            var saveManager = new SaveManager(saveData, saveService);
            main.RegisterManager(saveManager);
        }

        [TearDown]
        public void TearDown()
        {
            Main.Instance?.Dispose();
        }

        [Test]
        public void Test001_SaveData_Integrity()
        {
            var main = Main.Instance;
            var saveManager = main.GetManager<SaveManager>();

            saveManager.SaveData();

            var saveService = new PlayerPrefService();
            var reloadedData = saveService.LoadData();

            Assert.IsNotNull(reloadedData, "Reloaded save data should not be null.");
        }
    }
}
