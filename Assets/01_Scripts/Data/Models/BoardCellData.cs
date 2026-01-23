using System;

namespace ThreeMatch
{
    [Serializable]
    public class BoardCellData : IEquatable<BoardCellData>
    {
        public int CellID;
        public CellCoordinate Coordinate;
        public CellType CellType;
        public PieceType PieceType;
        public ColorType ColorType;

        public bool Equals(BoardCellData other) => CellID == other.CellID;

        public void SwapPiece(BoardCellData other)
        {
            var color = this.ColorType;
            var piece = this.PieceType;

            ColorType = other.ColorType;
            PieceType = other.PieceType;

            other.ColorType = color;
            other.PieceType = piece;
        }
    }
}
