namespace ThreeMatch
{
    public static class PieceTypeExtension
    {
        public static string GetImageName(this PieceType type, int param = 0)
        {
            return type switch
            {
                PieceType.Normal => (ColorType)param switch
                {
                    ColorType.Red => "piece_normal_red",
                    ColorType.Green => "piece_normal_green",
                    ColorType.Blue => "piece_normal_blue",
                    ColorType.Yellow => "piece_normal_yellow",
                    ColorType.Purple => "piece_normal_purple",
                    _ => "Invalid Color param"
                },
                PieceType.Ice => param == 0 ? "piece_ice_0" : "piece_ice_1",
                PieceType.RocketRow => "piece_rocket_row",
                PieceType.RocketCol => "piece_rocket_col",
                PieceType.Bomb => "piece_bomb",
                PieceType.Rainbow => "piece_rainbow",
                _ => "Invalid piece Type"
            };
        }

    }
}
