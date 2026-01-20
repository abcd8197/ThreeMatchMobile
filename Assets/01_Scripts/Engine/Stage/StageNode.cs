using UnityEngine;
using TMPro;

namespace ThreeMatch
{
    public class StageNode : MonoBehaviour, IRaycastable
    {
        [SerializeField] private TextMeshPro txt_Stage;
        [SerializeField] private SpriteRenderer sp_Back;
        public int RaycastOrder => 1;
        private int _stage;
        public void SetData(int stage, int maxStage)
        {
            _stage = stage;
            sp_Back.color = stage <= maxStage ? Color.white : Color.gray;
            txt_Stage.text = stage.ToString();
        }

        #region ## IRaycastable ##
        public void OnBeginDrag() { }

        public void OnDrag(Vector2 delta) { }

        public void OnEndDrag() { }

        public void OnPointerDown()
        {

        }

        public void OnPointerUp()
        {

        }

        public void OnPointerClick()
        {
            var uiManager = Main.Instance.GetManager<UIManager>();

            if (!uiManager.IsPopupActivated(PopupType.StageEnterPopup))
            {
                Main.Instance.GetManager<StageManager>().SetCurrentStage(_stage);
                uiManager.ShowPopup(PopupType.StageEnterPopup);
            }
        }
        #endregion
    }
}
