using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ThreeMatch
{
    public class CellController
    {
        public ICellView CellView { get; private set; }
        public BoardCellData Data { get; private set; }

        private Action<Vector2, BoardCellData> _onDrag;

        public int RaycastOrder => 1;

        public CellController(BoardCellData data, Action<Vector2, BoardCellData> onDragAction)
        {
            Data = data;
            _onDrag = onDragAction;
        }

        public void SetCellView(ICellView cellview)
        {
            CellView = cellview;
            cellview?.SetData(Data, OnDrag);
            UpdateCellView();
        }

        public void UpdateCellView()
        {
            if (CellView == null)
                return;

            CellView.SetCellType(Data.CellType);
            CellView.SetPieceType(Data.PieceType, Data.ColorType);
        }

        public Task ShakeCellView()
        {
            if (CellView == null)
                return Task.CompletedTask;

            return CellView?.Shake();
        }

        private void OnDrag(float x, float y) => _onDrag?.Invoke(new Vector2(x, y), Data);

    }
}
