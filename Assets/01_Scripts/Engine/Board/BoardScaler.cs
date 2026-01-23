using UnityEngine;

namespace ThreeMatch
{
    public class BoardScaler : MonoBehaviour
    {
        private Vector2Int _prevResolution = Vector2Int.zero;

        private void Update()
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            if (_prevResolution.x == Screen.width && _prevResolution.y == Screen.height)
                return;


        }
    }
}
