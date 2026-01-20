using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class SceneManager : IModuleRegistrar<ISceneChangeNotifyModule>
    {
        private Coroutine _sceneLoadCoroutine;
        private Dictionary<Type, ISceneChangeNotifyModule> _modules = new();
        public SceneType CurrentSceneType { get; private set; } = SceneType.Title;

        public SceneManager()
        {

        }

        public void Dispose()
        {
            if (_sceneLoadCoroutine != null)
                CoroutineHandler.Instance.StopCoroutine(_sceneLoadCoroutine);
        }

        public void Register(ISceneChangeNotifyModule module)
        {
            var type = module.GetType();
            if (!_modules.ContainsKey(type))
                _modules[type] = module;
        }

        public void LoadScene(SceneType sceneType)
        {
            if (_sceneLoadCoroutine == null)
                _sceneLoadCoroutine = CoroutineHandler.Instance.StartCoroutine(LoadSceneCoroutine(sceneType));
        }

        private IEnumerator LoadSceneCoroutine(SceneType sceneType)
        {
            OnNotifyStartSceneChange(CurrentSceneType, sceneType);

            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)SceneType.Empty);
            asyncOperation.allowSceneActivation = true;

            while (asyncOperation.progress < 1f)
            {
                if (asyncOperation.isDone && asyncOperation.progress > 0.9f)
                {
                    // Complete Load Scene
                }

                yield return null;
            }

            OnNotifySceneChanged(SceneType.Empty);
            yield return new WaitForSeconds(1f);

            asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneType);
            asyncOperation.allowSceneActivation = true;


            while (asyncOperation.progress < 1f)
            {
                if (asyncOperation.isDone && asyncOperation.progress > 0.9f)
                {
                    // Complete Load Scene
                }

                yield return null;
            }

            CurrentSceneType = sceneType;
            OnNotifySceneChanged(CurrentSceneType);
        }

        private void OnNotifyStartSceneChange(SceneType fromScene, SceneType toScene)
        {
            foreach (var module in _modules.Values)
                module?.OnStartSceneChange(fromScene, toScene);
        }

        private void OnNotifySceneChanged(SceneType sceneType)
        {
            foreach (var module in _modules.Values)
                module?.OnSceneChanged(sceneType);
        }
    }
}
