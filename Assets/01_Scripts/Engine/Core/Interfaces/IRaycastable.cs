using UnityEngine;

namespace ThreeMatch
{
    public interface IRaycastable
    {
        public int RaycastOrder { get; }
        public void OnBeginDrag();
        public void OnDrag(Vector2 delta);
        public void OnEndDrag();
        public void OnPointerDown();
        public void OnPointerUp();
        public void OnPointerClick();
    }
}
