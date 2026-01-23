namespace ThreeMatch
{
    public static class StageGoalTypeExtension
    {
        public static string GetImageName(this StageGoalType stageGoalType, ColorType colorType = ColorType.None)
        {
            return stageGoalType switch
            {
                StageGoalType.Score => "goal_score",
                StageGoalType.CollectColor => colorType switch
                {
                    ColorType.Red => "piece_normal_red",
                    ColorType.Green => "piece_normal_green",
                    ColorType.Blue => "piece_normal_blue",
                    ColorType.Yellow => "piece_normal_yellow",
                    ColorType.Purple => "piece_normal_purple",
                    _ => string.Empty
                },
                _ => string.Empty
            };
        }
    }
}
