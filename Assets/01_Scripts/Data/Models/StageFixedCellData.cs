using System;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageFixedCellData
    {
        public CellCoordinate Coord;
        public CellType CellType;

        public int Param0;
        public int Param1;

        public bool HasPortalOut;
        public CellCoordinate PortalOutCoord;

        public StageFixedCellData(CellCoordinate coord, CellType cellType, int param0 = default, int param1 = default, bool hasPortalOut = default, CellCoordinate portalOutCoord = default)
        {
            Coord = coord;
            CellType = cellType;
            Param0 = param0;
            Param1 = param1;
            HasPortalOut = hasPortalOut;
            PortalOutCoord = portalOutCoord;
        }
    }
}
