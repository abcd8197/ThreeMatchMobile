using UnityEngine;

namespace ThreeMatch
{
    public abstract class PopupBase : MonoBehaviour
    {
        public bool Recycleable { get; protected set; } = false;

        public virtual void Show(int sortingOrder)
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }
        }

        public virtual void Hide()
        {

        }
    }
}
