using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class StageManager : IManager, ISaveModule
    {
        private StageSaveData _saveData;
        private readonly List<StageData> _stageData = new();


        public Type ModuleType => typeof(ISaveModule);

        public StageManager(List<StageData> datas)
        {
            _stageData.AddRange(datas);
        }

        private void LoadStageData(string json)
        {
            // StageData에 필요한 데이터들 Load

        }

        public void Dispose()
        {

        }

        #region ## ISaveModule ##
        public void InitializeSaveData(SaveData saveData)
        {
            saveData.StageSaveData ??= new();
            _saveData = saveData.StageSaveData;
        }

        public void SaveData()
        {

        }
        #endregion

        #region ## API ##
        public void CurrentStageCleared()
        {
            if (_saveData.CurrentStage > _saveData.MaxStage)
                _saveData.MaxStage = _saveData.CurrentStage;
            _saveData.CurrentStage = 0;
        }
        public int GetMaxStage() => _saveData.MaxStage;
        public void SetCurrentStage(int stage) => _saveData.CurrentStage = stage;
        public int GetCurrentStage() => _saveData.CurrentStage;
        #endregion
    }
}
