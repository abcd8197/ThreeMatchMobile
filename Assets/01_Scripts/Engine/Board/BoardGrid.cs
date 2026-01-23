using UnityEngine;
using System.Collections.Generic;

namespace ThreeMatch
{
    public sealed class BoardGrid : MonoBehaviour
    {
        // 기존 값 유지
        [SerializeField] private Vector2 minPos = new Vector2(-2, -3);
        [SerializeField] private Vector2 cellDelta = Vector2.one * 0.5f;

        private Vector2[] _cellLocalPositions;

        public void SortGrid(List<ICellView> cellviewList, int width, int height)
        {
            if (cellviewList == null || cellviewList.Count == 0)
                return;

            _cellLocalPositions = new Vector2[cellviewList.Count];

            for (int i = 0; i < cellviewList.Count; i++)
            {
                int x = i % width;
                int y = i / width;

                float posX = minPos.x + (cellDelta.x * x);
                float posY = minPos.y + (cellDelta.y * y);

                _cellLocalPositions[i] = new Vector2(posX, posY);
                cellviewList[i].SetPosition(posX, posY);
            }
        }

        public Vector3 GetCellWorldPosition(int cellId)
        {
            if (_cellLocalPositions == null || cellId < 0 || cellId >= _cellLocalPositions.Length)
                return transform.TransformPoint(Vector3.zero);

            return transform.TransformPoint(_cellLocalPositions[cellId]);
        }
    }
}
