namespace ThreeMatch
{
    public interface IGameNotifyModule : IModule
    {
        public void OnChangedGameState(GameState state);
        public void OnGamePaused(bool paused);
    }
}
