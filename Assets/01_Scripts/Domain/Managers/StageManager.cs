using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class StageManager : IManager, ISaveModule
    {
        private StageSaveData _saveData;
        private readonly List<StageData> _stageData = new();


        public Type ModuleType => typeof(ISaveModule);

        public void SetStageData(List<StageMetaData> stageDatas, List<StageGoalData> goalData, List<StageFixedCellSetData> fixedCellDatas, List<StageSpawnRuleData> ruleDatas)
        {
            _stageData?.Clear();
            _stageData.AddRange(StageManagerDataResolver.Resolve(stageDatas, goalData, fixedCellDatas, ruleDatas));
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
