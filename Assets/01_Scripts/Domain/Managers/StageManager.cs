using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class StageManager : IManager, ISaveModule
    {
        public readonly int CellSizeX = 9;
        public readonly int CellSizeY = 9;

        private StageSaveData _saveData;
        private readonly List<StageData> _stageData;

        public int MaxStage => _saveData == null ? 0 : _saveData.MaxStage;

        public Type ModuleType => typeof(ISaveModule);

        public StageManager()
        { 

        }

        public void Dispose()
        {

        }

        public void InitializeSaveData(SaveData saveData)
        {
            saveData.StageSaveData ??= new();
            _saveData = saveData.StageSaveData;
        }

        public void SaveData()
        {

        }

        public void SetMaxStage(int maxStage)
        {
            _saveData.MaxStage = maxStage;
        }

        public void SetStageData(int stage)
        {
            _saveData.CurrentStage = stage;
        }

        public int GetCurrentStage() => _saveData.CurrentStage;
    }
}
