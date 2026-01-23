using System;

namespace ThreeMatch
{
    [Serializable]
    public sealed class StageGoalData
    {
        public string Key;
        public StageGoalType GoalType;
        public int GoalValue;
        public ColorType TargetColor;
        public PieceType TargetPieceType;

        public StageGoalData(string key, StageGoalType goalType, int goalValue, ColorType targetColor = ColorType.None, PieceType targetPiece = PieceType.None)
        {
            Key = key;
            GoalType = goalType;
            GoalValue = goalValue;
            TargetColor = targetColor;
            TargetPieceType = targetPiece;
        }
    }
}
