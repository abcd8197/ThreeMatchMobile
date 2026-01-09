using UnityEngine;

namespace ThreeMatch
{
    public class UIRoot : MonoBehaviour
    {
        private TitleCanvas _titleCanvas;
        public Transform PopupRoot { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            PopupRoot = Instantiate(Resources.Load<GameObject>("TitleScene/Prefab/UI/Popup/PopupRoot"), transform, false).transform;
            _titleCanvas = Instantiate(Resources.Load<GameObject>("TitleScene/Prefab/UI/Popup/TitleCanvas"), transform).GetComponent<TitleCanvas>();
        }

        public void DestroyTitleCanvas()
        {
            Destroy(_titleCanvas.gameObject);
        }
    }
}
