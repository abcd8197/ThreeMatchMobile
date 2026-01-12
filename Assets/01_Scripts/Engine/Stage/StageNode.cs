using UnityEngine;
using TMPro;

namespace ThreeMatch
{
    public class StageNode : MonoBehaviour, IRaycastable
    {
        [SerializeField] private TextMeshPro txt_Stage;
        [SerializeField] private SpriteRenderer sp_Lock;

        public void SetData(int stage, int maxStage)
        {
            sp_Lock.enabled = stage < maxStage;
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
        #endregion
    }
}
