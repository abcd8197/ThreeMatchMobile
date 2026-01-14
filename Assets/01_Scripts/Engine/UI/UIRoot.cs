using UnityEngine;

namespace ThreeMatch
{
    public class UIRoot : MonoBehaviour
    {
        private TitleCanvas _titleCanvas;
        private AssetManager _assetManager;
        private GameObject TopPanelCanvas;
        public Transform PopupRoot { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PopupRoot = Instantiate(Resources.Load<GameObject>("TitleScene/Prefab/UI/Popup/PopupRoot"), transform, false).transform;
            _titleCanvas = Instantiate(Resources.Load<GameObject>("TitleScene/Prefab/UI/Popup/TitleCanvas"), transform).GetComponent<TitleCanvas>();
            _assetManager = Main.Instance.GetManager<AssetManager>();
        }

        public void OnChangedScene(SceneType sceneType)
        {
            if (sceneType != SceneType.Title && _titleCanvas != null)
                Destroy(_titleCanvas.gameObject);

            if (sceneType != SceneType.Main && TopPanelCanvas != null)
                Destroy(TopPanelCanvas);

            switch (sceneType)
            {
                case SceneType.Empty:
                    break;
                case SceneType.Title:
                    break;
                case SceneType.Main:
                    TopPanelCanvas = _assetManager.GetPrefabInstance(BundleGroup.defaultasset, "TopPanelCanvas", parent: transform);
                    _assetManager.GetPrefabInstance(BundleGroup.defaultasset, "StageRoot");
                    break;
                case SceneType.Game:
                    break;
            }
        }
    }
}
