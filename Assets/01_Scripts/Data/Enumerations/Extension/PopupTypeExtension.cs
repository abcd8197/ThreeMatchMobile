namespace ThreeMatch
{
    public static class PopupTypeExtension
    {
        public static BundleGroup GetBundleGroup(this PopupType popupType)
        {
            return popupType switch
            {
                _ => BundleGroup.defaultasset,
            };
        }
    }
}
