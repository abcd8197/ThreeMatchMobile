namespace ThreeMatch
{
    public static class ItemTypeExtension
    {
        public static string GetImageName(this ItemType itemType) => itemType.ToString();
    }
}
