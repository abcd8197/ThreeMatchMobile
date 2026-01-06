using UnityEngine;

namespace ThreeMatch
{
    public class UIRoot : MonoBehaviour
    {
        public Transform PopupRoot { get; private set; }

        private void Start()
        {
            CreatePopupRoot();
            CreateTitleCanvas();

            void CreatePopupRoot()
            {
                PopupRoot = new GameObject("PopupRoot").AddComponent<RectTransform>();
                PopupRoot.SetParent(this.transform, false);

                var rectTransform = PopupRoot.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            void CreateTitleCanvas()
            {
                var titleCanvas = Instantiate(Resources.Load<GameObject>("TitleScene/Prefab/UI/Popup/TitleCanvas"), transform).GetComponent<TitleCanvas>();
                titleCanvas.LoadingEnabled(true);
            }
        }
    }
}
