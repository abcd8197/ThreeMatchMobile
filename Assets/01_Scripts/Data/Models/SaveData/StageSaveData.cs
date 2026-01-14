using System;

namespace ThreeMatch
{
    [Serializable]
    public class StageSaveData
    {
        public const int CurrentVersion = 1;
        public int Version = CurrentVersion;

        public int MaxStage { get; set; }
        public int CurrentStage { get; set; }
    }
}
