using System;


namespace ThreeMatch
{
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;
        public int Version = CurrentVersion;
        public StageSaveData StageSaveData;
        public ItemSaveData ItemSaveData;
    }
}
