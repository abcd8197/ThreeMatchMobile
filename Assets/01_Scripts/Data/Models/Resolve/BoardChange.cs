using System;
using System.Collections.Generic;

namespace ThreeMatch
{

    [Serializable]
    public abstract class BoardChange
    {
        public ResolveState State { get; }

        public BoardChange(ResolveState state)
        {
            State = state;
        }
    }

    #region ## Swap ##
    [Serializable]
    public sealed class SwapChange : BoardChange
    {
        public int FromCellID { get; }
        public int ToCellID { get; }
        public SwapDirection Direction { get; }
        public SwapChangeKind Kind { get; }

        public SwapChange(int fromCellId, int toCellId, SwapDirection dir, SwapChangeKind kind)
            : base(ResolveState.Swap)
        {
            FromCellID = fromCellId;
            ToCellID = toCellId;
            Direction = dir;
            Kind = kind;
        }
    }
    #endregion

    #region ## Fall ##
    [Serializable]
    public sealed class FallChange : BoardChange
    {
        public int FromCellID { get; }
        public int ToCellID { get; }

        public FallChange(int fromCellId, int toCellId)
            : base(ResolveState.Fall)
        {
            FromCellID = fromCellId;
            ToCellID = toCellId;
        }
    }
    #endregion

    #region ## Remove / Spawn ##

    public readonly struct RemovedCellInfo
    {
        public readonly int CellId;
        public readonly PieceType PieceType;
        public readonly ColorType ColorType;
        public RemovedCellInfo(int id, PieceType pt, ColorType ct)
        {
            CellId = id; PieceType = pt; ColorType = ct;
        }
    }
    [Serializable]
    public sealed class RemoveChange : BoardChange
    {
        public IReadOnlyList<RemovedCellInfo> Removed { get; }
        public RemoveChange(List<RemovedCellInfo> removed) : base(ResolveState.Removed)
            => Removed = removed;
    }

    [Serializable]
    public sealed class SpawnChange : BoardChange
    {
        public int CellID { get; }
        public PieceType PieceType { get; }
        public ColorType ColorType { get; }

        public SpawnChange(int cellId, PieceType pieceType, ColorType colorType)
            : base(ResolveState.Spawned)
        {
            CellID = cellId;
            PieceType = pieceType;
            ColorType = colorType;
        }
    }
    #endregion

    #region ## Damage ##
    [Serializable]
    public sealed class DamagedChange : BoardChange
    {
        public CellCoordinate At { get; }
        public int LevelBefor { get; }
        public int LevelAfter { get; }
        public DamagedChange(CellCoordinate at, int levelBefor, int levelAfter) : base(ResolveState.Damaged)
        {
            At = at;
            LevelBefor = levelBefor;
            LevelAfter = levelAfter;
        }
    }
    #endregion

    #region ## Merge ##
    [Serializable]
    public sealed class MergedChange : BoardChange
    {
        public IReadOnlyList<CellCoordinate> ConsumedCells { get; }
        public CellCoordinate ResultCellCoord { get; }
        public PieceType ResultPieceType { get; }
        public ColorType ResultColorType { get; }

        public MergedChange(IReadOnlyList<CellCoordinate> consumedCells, CellCoordinate resultCellCoord, PieceType resultPieceType, ColorType resultColorType)
            : base(ResolveState.Merged)
        {
            ConsumedCells = consumedCells;
            ResultCellCoord = resultCellCoord;
            ResultPieceType = resultPieceType;
            ResultColorType = resultColorType;
        }
    }
    #endregion

    #region ## Shake ##
    [Serializable]
    public sealed class ShakeChange : BoardChange
    {
        public int CellID { get; }

        public ShakeChange(int cellId)
            : base(ResolveState.Shake)
        {
            CellID = cellId;
        }
    }
    #endregion
}
