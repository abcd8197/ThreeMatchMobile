namespace ThreeMatch
{
    public static class CellTypeExtension
    {
        public static string GetImageName(this CellType cellType)
        {
            return cellType switch
            {
                CellType.Normal or CellType.Spawner => "cell_normal",
                CellType.Hole => "cell_hole",
                CellType.Blocked => "cell_blocked",
                CellType.PortarIn or CellType.PortarOut => "cell_portal",
                _ => "invalid CellType"
            };
        }
    }
}
