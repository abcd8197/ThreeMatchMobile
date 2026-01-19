using System.Collections.Generic;
using System.Linq;

namespace ThreeMatch
{
    public static class StageManagerDataResolver
    {
        public static List<StageData> Resolve(List<StageMetaData> stageMetas, List<StageGoalData> goalData, List<StageFixedCellSetData> fixedCellDatas, List<StageSpawnRuleData> ruleDatas)
        {
            List<StageData> result = new();
            Dictionary<string, List<StageFixedCellData>> fixedCellSetDict = fixedCellDatas.ToDictionary(x => x.Key, x => x.Cells);
            Dictionary<string, StageGoalData> goalDataDict = goalData.ToDictionary(x => x.Key, x => new StageGoalData(x.GoalType, x.GoalValue, x.TargetColor, x.TargetPieceType));
            Dictionary<string, StageSpawnRuleData> ruleDataDict = ruleDatas.ToDictionary(x => x.Key, x => x);

            foreach (var metaData in stageMetas)
            {
                var stageData = new StageData();
                stageData.StageId = metaData.StageId;
                stageData.Width = metaData.Width;
                stageData.Height = metaData.Height;
                stageData.MoveLimit = metaData.MoveLimit;

                foreach (var goalKey in metaData.GoalSetKey)
                    stageData.Goals.Add(goalDataDict[goalKey]);

                foreach (var cellKey in metaData.FixedCellSetKey)
                    stageData.FixedCells.AddRange(fixedCellSetDict[cellKey]);

                stageData.SpawnRule = ruleDataDict[metaData.SpawnRuleKey];

                result.Add(stageData);
            }

            return result;
        }
    }
}
