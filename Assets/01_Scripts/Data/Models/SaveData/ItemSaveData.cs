using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    [Serializable]
    public class ItemSaveData
    {
        public const int CurrentVersion = 1;
        public int Version = CurrentVersion;

        public Dictionary<ItemType, int> Items = new();
    }
}
