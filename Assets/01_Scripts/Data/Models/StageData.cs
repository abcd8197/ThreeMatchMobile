using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageData
    {
        public int StageId;
        public byte Width = 9;
        public byte Height = 9;
        public int MoveLimit;
        public List<StageGoalData> Goals = new();
        public List<StageFixedCellData> FixedCells = new();
        public StageSpawnRuleData SpawnRule;
    }
}
