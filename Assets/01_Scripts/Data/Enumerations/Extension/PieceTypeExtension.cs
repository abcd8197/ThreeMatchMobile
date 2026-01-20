namespace ThreeMatch
{
    public static class PieceTypeExtension
    {
        public static string GetImageName(this PieceType type)
        {
            return type switch
            {
                PieceType.Normal => "img_cell_normal",
                PieceType.Blocked => "img_cell_blocked",
                PieceType.RocketRow => "img_cell_rocketrow",
                PieceType.RocketCol => "img_cell_rocketcol",
                PieceType.Bomb => "img_cell_bomb",
                PieceType.Rainbow => "img_cell_rainbow",
                _ => ""
            };
        }

    }
}
