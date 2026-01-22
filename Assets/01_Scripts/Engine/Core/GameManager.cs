using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class GameManager : IModuleRegistrar<IGameNotifyModule>, ISceneChangeNotifyModule
    {
        private readonly Dictionary<Type, IGameNotifyModule> _gameNotifyModules = new();

        private RaycastHandler _raycastHandler;
        private BoardController _boardController;
        public float GameSpeed { get; private set; } = 1f;

        public GameManager()
        {
            CreateRaycastHandler();
        }

        private void CreateRaycastHandler()
        {
            _raycastHandler = new GameObject("RaycastHandler").AddComponent<RaycastHandler>();
            UnityEngine.Object.DontDestroyOnLoad(_raycastHandler);
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
            if (sceneType != SceneType.Game)
                _boardController?.Dispose();

            switch (sceneType)
            {
                case SceneType.Main:
                    _raycastHandler.RaycastEnabled(true);
                    _raycastHandler.UpdateCurrentMainCamera();
                    break;
                case SceneType.Game:
                    _raycastHandler.RaycastEnabled(true);
                    _raycastHandler.UpdateCurrentMainCamera();
                    CreateBoardController();
                    OnChangedGameState(GameState.Start);
                    break;
                default:
                    _raycastHandler.RaycastEnabled(false);
                    break;
            }
        }
        #endregion

        private void CreateBoardController()
        {
            if (_boardController != null)
                return;

            IBoardView boardView = null;
            if (Application.isPlaying)
                boardView = Main.Instance.GetManager<AssetManager>().GetInstantiateComponent<BoardView>(BundleGroup.defaultasset, "BoardView");
            _boardController = new();
            _boardController.SetBoardView(boardView);
        }


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
        #region ## API ##
        public void RaycastEnabled(bool enabled) => _raycastHandler.RaycastEnabled(enabled);

        public void ChangeGameSpeed(float speed)
        {
            GameSpeed = speed;

            foreach (var module in _gameNotifyModules.Values)
                module?.OnChangedGameSpeed(GameSpeed);
        }
        #endregion
    }
}
