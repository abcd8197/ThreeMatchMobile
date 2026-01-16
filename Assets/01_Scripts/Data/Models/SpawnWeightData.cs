using System;

namespace ThreeMatch
{
    [Serializable]
    public class SpawnWeightData
    {
        public ColorType Color;
        public int Weight;

        public SpawnWeightData(ColorType color, int weight)
        {
            Color = color;
            Weight = weight;
        }
    }
}
