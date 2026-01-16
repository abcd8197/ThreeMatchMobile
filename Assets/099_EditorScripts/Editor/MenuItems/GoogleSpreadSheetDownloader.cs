using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

namespace ThreeMatch.Editor
{
    public class GoogleSpreadSheetDownloader : EditorWindow
    {
        private const string SheetDataPath = "Assets/099_EditorScripts/Editor/MenuItems/SheetData/";
        private const string SheetDataFileName = "GoogleSpreadSheetData";
        private const string BasePath = "Assets/Resources/Datas/";

        private static string Path = BasePath;
        private static string InputPath = BasePath;

        private static string inputSheetName = string.Empty;
        private static string inputSheetDocID = string.Empty;
        private static string inputSheetGID = string.Empty;

        private static List<SheetData> _sheetDatas = new();
        private static List<int> _removableDataIndices = new();


        [MenuItem("Tools/ThreeMatch/Open Google Spread Sheet Downloader")]
        private static void OpenWindow()
        {
            GetWindow<GoogleSpreadSheetDownloader>("Google Spread Sheet Downloader");
            LoadSheetDatas();
        }

        private void OnDisable()
        {
            SaveSheetDatas();
            _removableDataIndices.Clear();
        }

        private static void LoadSheetDatas()
        {
            var loadedAsset = AssetDatabase.LoadAssetAtPath(SheetDataPath + SheetDataFileName + ".txt", typeof(TextAsset));
            if (loadedAsset != null)
            {
                var textAsset = loadedAsset as TextAsset;
                try
                {
                    _sheetDatas = JsonConvert.DeserializeObject<List<SheetData>>(textAsset.text);
                }
                catch
                {
                    _sheetDatas.Clear();
                }
            }
        }

        private static void SaveSheetDatas()
        {
            var json = JsonConvert.SerializeObject(_sheetDatas);
            var fullPath = SheetDataPath + SheetDataFileName + ".txt";
            var splitedPath = SheetDataPath.Split('/');
            string path = string.Empty;

            foreach (var item in splitedPath)
            {
                path += item;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            DrawPathPanel();
            DrawSheetViewer();
            DrawAddSheetButton();
            DrawDownloadButton();
        }

        private void DrawPathPanel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Download Path: " + Path, EditorStyles.boldLabel);

            if (GUILayout.Button("Initalize Path", GUILayout.Width(100)))
            {
                InputPath = BasePath;
            }
            EditorGUILayout.Space();

            InputPath = EditorGUILayout.TextField("Change Path", InputPath);

            if (GUILayout.Button("Change Path", GUILayout.Width(100)))
            {
                if (string.IsNullOrEmpty(InputPath))
                    Debug.LogError("Invalid Path!");
                if (InputPath[InputPath.Length - 1] != '/')
                    InputPath += '/';

                Path = InputPath;
                InputPath = "";
            }
        }

        private void DrawSheetViewer()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sheets", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < _sheetDatas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{_sheetDatas[i].Name} / {_sheetDatas[i].DocId} / {_sheetDatas[i].GID}");
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    _removableDataIndices.Add(i);
                EditorGUILayout.EndHorizontal();
            }

            bool removed = false;
            for (int i = 0; i < _removableDataIndices.Count; i++)
            {
                removed = true;
                _sheetDatas.RemoveAt(_removableDataIndices[i]);
            }

            if (removed)
                SaveSheetDatas();

            _removableDataIndices.Clear();
            EditorGUILayout.EndVertical();
        }

        private void DrawAddSheetButton()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            inputSheetName = EditorGUILayout.TextField("Sheet Name", inputSheetName);
            inputSheetDocID = EditorGUILayout.TextField("Sheet Document ID", inputSheetDocID);
            inputSheetGID = EditorGUILayout.TextField("Sheet GID", inputSheetGID);

            if (GUILayout.Button("Add Sheet Data", GUILayout.Width(100)))
            {
                if (string.IsNullOrEmpty(inputSheetDocID) || string.IsNullOrEmpty(inputSheetDocID) || string.IsNullOrEmpty(inputSheetGID))
                    Debug.LogError("Invalid Sheet");

                bool duplicate = false;
                foreach (var sheetData in _sheetDatas)
                {
                    duplicate = sheetData.Name == inputSheetName && sheetData.DocId == inputSheetDocID && sheetData.GID == inputSheetGID;
                    if (duplicate)
                    {
                        Debug.LogError("this Sheet Already Added");
                        break;
                    }
                }

                if (!duplicate)
                {
                    _sheetDatas.Add(new SheetData(inputSheetName, inputSheetDocID, inputSheetGID));
                    _sheetDatas.Sort((x, y) => x.GID.CompareTo(y.GID));
                }

                SaveSheetDatas();
            }

            EditorGUILayout.EndVertical();
        }

        private async void DrawDownloadButton()
        {

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Download SheetDatas"))
            {
                Debug.Log("Start Sheet Download");
                foreach (var sheetData in _sheetDatas)
                {
                    var csv = await RequestSheetData(sheetData.DocId, sheetData.GID);
                    WriteJson(csv, sheetData.Name);
                }
                Debug.Log("End of Sheet Download");
            }

        }

        private static void WriteJson(string csv, string fileName)
        {
            var fullPath = Path + fileName + ".txt";
            string path = string.Empty;
            var splitedPath = Path.Split('/');

            foreach (var item in splitedPath)
            {
                path += item;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            var convertedJson = CsvParser.ReadAndConvertToListJson(csv);
            File.WriteAllText(fullPath, convertedJson);
        }

        private static async Task<string> RequestSheetData(string docId, string gid = null)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{docId}/export?format=csv";

            if (!string.IsNullOrEmpty(gid))
                url += $"&gid={gid}";

            using var www = UnityWebRequest.Get(url);

            var asyncOperation = www.SendWebRequest();

            while (!asyncOperation.isDone)
                await Task.Delay(500);

            if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
                return www.downloadHandler.text;

            Debug.Log($"Error : {www.result}");
            return null;
        }
    }

    public class SheetData
    {
        public string Name;
        public string DocId;
        public string GID;

        public SheetData(string name, string docId, string gid)
        {
            Name = name;
            DocId = docId;
            GID = gid;
        }
    }
}