using System;

namespace ThreeMatch
{
    public class StageManager : IManager, ISaveModule
    {
        private StageSaveData _saveData;

        public int MaxStage => _saveData == null ? 0 : _saveData.MaxStage;

        public Type ModuleType => typeof(ISaveModule);

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
    }
}
