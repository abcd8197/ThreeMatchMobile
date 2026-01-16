using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public class StageSpawnRuleData
    {
        public int UsedColorCount = 5;
        public List<SpawnWeightData> ColorWeights = new();

        public StageSpawnRuleData()
        {

        }
    }
}
