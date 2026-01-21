using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public class ResolveContext
    {
        public CellResolveState State;
        public List<BoardCellData> Datas;
        public BoardCellData MergeTarget;
    }
}
