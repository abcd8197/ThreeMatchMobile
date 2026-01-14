using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public class StageData
    {
        public int Stage;
        public int MoveLimit;
        public List<StageClearData> StageClearConditions;
        public List<FixedCellData> FixedCells;
    }
}
