namespace ThreeMatch
{
    public interface ISaveService
    {
        public SaveData LoadData();
        public void SaveData(SaveData saveData);
    }
}
