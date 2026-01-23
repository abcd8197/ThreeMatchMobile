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

        private int _remainMove = 0;
        public int RemainMove => _remainMove;
        public float GameSpeed { get; private set; } = 1f;
        public int GetScore => _boardController != null ? _boardController.Score : 0;

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
            {
                _boardController?.Dispose();
                _boardController = null;
            }

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
            _boardController = new(this);
            _boardController.SetBoardView(boardView);
        }

        public void OnChangedGameState(GameState state)
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

        public void SetMoveChance(int moveChance) => _remainMove = moveChance;
        public void UseMoveChance()
        {
            if (_remainMove > 0)
                _remainMove--;
        }

        public void SubscbireOnGoalValueChanged(Action<StageGoalData, int> method)
        {
            if (_boardController != null)
                _boardController.OnGoalDataChanged += method;
        }
        public void UnSubscbireOnGoalValueChanged(Action<StageGoalData, int> method)
        {
            if (_boardController != null)
                _boardController.OnGoalDataChanged -= method;
        }

        #endregion
    }
}
