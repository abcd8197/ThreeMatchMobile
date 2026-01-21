using System;
using UnityEngine;
using System.Collections.Generic;

namespace ThreeMatch
{

    public sealed class BoardGrid : MonoBehaviour
    {
        public void SortGrid(List<ICellView> cellviewList, int width, int height)
        {
            if (cellviewList == null || cellviewList.Count == 0)
                return;

            Vector2 minPos = new Vector2(-2, -3);
            Vector2 maxPos = new Vector2(2, 1);
            Vector2 delta = Vector2.one * 0.5f;

            for (int i = 0; i < cellviewList.Count; i++)
            {
                int x = i % width;
                int y = i / width;

                float posX = minPos.x + (delta.x * x);
                float posY = minPos.y + (delta.y * y);
                cellviewList[i].SetPosition(posX, posY);
            }
        }
    }
}
