using System;
using System.Threading.Tasks;

namespace ThreeMatch
{
    public interface ICellView
    {
        public void SetData(BoardCellData cellType, Action<float, float> onDrag);
        public void SetPosition(float x, float y);
        public void SetCellType(CellType cellType);
        public void SetPieceType(PieceType pieceType, ColorType colorType = ColorType.None);
        /// <summary>If InValid Move.</summary>
        public Task Shake();
        /// <returns>Move Duration</returns>
        public Task MoveTo(SwapDirection direction);
    }
}
