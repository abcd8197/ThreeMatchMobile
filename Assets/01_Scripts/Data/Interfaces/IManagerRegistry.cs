namespace ThreeMatch
{
    public interface IManagerRegistry<T> where T : IManager
    {
        public void Register(T manager);
    }
}
