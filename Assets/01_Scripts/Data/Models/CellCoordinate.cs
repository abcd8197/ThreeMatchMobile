using System;

namespace ThreeMatch
{
    public readonly struct CellCoordinate : IEquatable<CellCoordinate>
    {
        public readonly byte X;
        public readonly byte Y;

        public CellCoordinate(byte x, byte y)
        {
            X = x;
            Y = y;
        }
        public CellCoordinate(int x, int y)
        {
            X = (byte)x;
            Y = (byte)y;
        }

        public int ToIndex(int width) => X + (Y * width);

        public bool Equals(CellCoordinate other) => this.X == other.X && this.Y == other.Y;
        public override bool Equals(object obj) => obj is CellCoordinate other && Equals(other);
        public override int GetHashCode() => (X << 8) | Y;
        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(CellCoordinate a, CellCoordinate b) => a.Equals(b);
        public static bool operator !=(CellCoordinate a, CellCoordinate b) => !a.Equals(b);
    }
}
