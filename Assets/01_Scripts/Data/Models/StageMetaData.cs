using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageMetaData
    {
        public string Key;
        public int StageId;
        public byte Width;
        public byte Height;
        public int MoveLimit;
        public List<string> GoalSetKey;
        public List<string> FixedCellSetKey;
        public string SpawnRuleKey;
    }
}
