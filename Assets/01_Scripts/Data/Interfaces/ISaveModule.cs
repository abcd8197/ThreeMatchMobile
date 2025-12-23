namespace ThreeMatch
{
    public interface ISaveModule : IModule
    {
        public void InitializeSaveData(SaveData saveData);
        public void SaveData();
    }
}
