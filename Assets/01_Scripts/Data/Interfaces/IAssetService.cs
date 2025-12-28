namespace ThreeMatch
{
    public interface IAssetService
    {
        public T LoadAsset<T>(string assetPath) where T : class;
        public void ReleaseAsset<T>(T asset) where T : class;
    }
}
