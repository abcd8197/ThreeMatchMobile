using System;
using UnityEngine;

namespace ThreeMatch
{
    public class GameManager : IManager, ISceneChangeNotifyModule
    {
        private RaycastHandler _raycastHandler;

        public Type ModuleType => typeof(ISceneChangeNotifyModule);

        public GameManager()
        {
            CreateRaycastHandler();
        }

        private void CreateRaycastHandler()
        {
            _raycastHandler = new GameObject("RaycastHandler").AddComponent<RaycastHandler>();
            UnityEngine.Object.DontDestroyOnLoad(_raycastHandler);
            _raycastHandler.RaycastEnabled(false);
        }

        public void Dispose()
        {
            
        }

        public void OnStartSceneChange(SceneType fromScene, SceneType toScene)
        {

        }

        public void OnSceneChanged(SceneType sceneType)
        {
            switch (sceneType)
            {
                case SceneType.Main:
                case SceneType.Game:
                    _raycastHandler.RaycastEnabled(true);
                    _raycastHandler.UpdateCurrentMainCamera();
                    break;
                default:
                    _raycastHandler.RaycastEnabled(false);
                    break;
            }
        }
    }
}
