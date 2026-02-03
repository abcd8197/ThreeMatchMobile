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
            if (saveData.StageSaveData == null)
            {
                saveData.StageSaveData = new();
                saveData.StageSaveData.MaxStage = 0;
            }
            _saveData = saveData.StageSaveData;
        }

        public void SaveData()
        {

        }
        #endregion

        #region ## API ##
        public int GetStageDataCount() => _stageData.Count;
        public void CurrentStageCleared()
        {
            if (_saveData.CurrentStage >= _saveData.MaxStage)
                _saveData.MaxStage = _saveData.CurrentStage + 1;
            _saveData.CurrentStage = 0;
        }
        public int GetMaxStage() => _saveData.MaxStage;
        public void SetCurrentStage(int stage) => _saveData.CurrentStage = stage;
        public int GetCurrentStage() => _saveData.CurrentStage;
        public bool IsEnterableStage(int stageId) => stageId <= GetMaxStage();

        public StageData GetStageData(int stageId) => _stageData[stageId];
        public StageData GetCurrentStageData() => GetStageData(GetCurrentStage());
        #endregion
    }
}
