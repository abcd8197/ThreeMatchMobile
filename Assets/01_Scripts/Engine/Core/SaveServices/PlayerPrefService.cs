using UnityEngine;
using Newtonsoft.Json;

namespace ThreeMatch
{
    public class PlayerPrefService : ISaveService
    {
        private const string SaveDataKey = "SaveData";
        public SaveData LoadData()
        {
            string json = PlayerPrefs.GetString(SaveDataKey);
            return JsonConvert.DeserializeObject<SaveData>(json);
        }

        public void SaveData(SaveData saveData)
        {
            string json = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(SaveDataKey, json);
        }
    }
}
