namespace ThreeMatch
{
    public interface ISceneChangeNotifyModule : IModule
    {
        public void OnStartSceneChange(SceneType fromScene, SceneType toScene);
        public void OnSceneChanged(SceneType sceneType);
    }
}
