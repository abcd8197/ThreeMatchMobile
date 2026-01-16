using System;

namespace ThreeMatch
{
    public struct CellCoordinate : IEquatable<CellCoordinate>
    {
        public readonly byte X;
        public readonly byte Y;

        public bool Equals(CellCoordinate other) => this.X == other.X && this.Y == other.Y;
        public override bool Equals(object obj) => obj is CellCoordinate other && Equals(other);
        public override int GetHashCode() => (X << 8) | Y;
        public override string ToString() => $"({X},{Y})";
    }
}
