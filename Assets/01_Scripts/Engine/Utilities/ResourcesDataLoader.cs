using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ThreeMatch
{
    public static class ResourcesDataLoader
    {
        public static List<T> LoadDataToList<T>(string filePath)
        {
            var textAsset = Resources.Load<TextAsset>(filePath);

            if (textAsset == null)
                return new List<T>();

            try
            {
                var json = textAsset.text;
                var datas = JsonToObject<List<T>>(json);
                Resources.UnloadAsset(textAsset);
                return datas;
            }
            catch (Exception e)
            {
                Debug.LogError($"File Not Found. {filePath} {typeof(T)} {e}");
            }
            finally
            {
                Resources.UnloadAsset(textAsset);
            }

            return new List<T>();
        }

        public static T JsonToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
