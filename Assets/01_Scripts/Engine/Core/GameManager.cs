using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class GameManager : IModuleRegistrar<IGameNotifyModule>, ISceneChangeNotifyModule
    {
        private readonly Dictionary<Type, IGameNotifyModule> _gameNotifyModules = new();

        private RaycastHandler _raycastHandler;
        public float GameSpeed { get; private set; } = 1f;

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

        #region ## IModuleRegistrar<IGameNotifyModule> ##
        public void Register(IGameNotifyModule module)
        {
            var type = module.GetType();
            if (_gameNotifyModules.ContainsKey(type))
            {
                Debug.LogWarning($"{nameof(IGameNotifyModule)} of type {type} is already registered.");
                return;
            }
            _gameNotifyModules[type] = module;
        }


        public void Dispose()
        {
            if (_raycastHandler != null)
                UnityEngine.Object.DestroyImmediate(_raycastHandler);
        }
        #endregion

        #region ## ISceneChangeNotifyModule ##
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
        #endregion

        #region ## API ##
        private void OnChangedGameState(GameState state)
        {
            foreach (var module in _gameNotifyModules.Values)
                module?.OnChangedGameState(state);
        }

        private void OnGamePaused(bool paused)
        {
            foreach (var module in _gameNotifyModules.Values)
                module?.OnGamePaused(paused);
        }

        public void StartGame()
        {
            OnChangedGameState(GameState.Start);
        }
        #endregion
    }
}
