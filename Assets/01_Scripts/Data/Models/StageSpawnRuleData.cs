using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageSpawnRuleData
    {
        public string Key;
        public List<ColorType> Colors = new();
        public List<float> Weights = new();
    }
}
