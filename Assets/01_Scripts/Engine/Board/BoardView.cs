using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private BoardGrid _grid;
        private List<ICellView> _cellViews = new();
        private int _width;
        private int _height;
        public List<ICellView> CreateCellView(List<BoardCellData> cellDatas, int width, int height)
        {
            var assetManager = Main.Instance.GetManager<AssetManager>();
            _width = width;
            _height = height;


            for (int i = 0; i < cellDatas.Count; i++)
            {
                var cellView = assetManager.GetInstantiateComponent<CellView>(BundleGroup.defaultasset, "BoardCell", parent: _grid.transform);
                _cellViews.Add(cellView);
            }

            SortGrid();
            return _cellViews ?? new List<ICellView>();
        }

        [ContextMenu("Sort Cells")]
        private void SortGrid()
        {
            if (_grid == null)
                return;

            _grid?.SortGrid(_cellViews, _width, _height);
        }
    }
}
