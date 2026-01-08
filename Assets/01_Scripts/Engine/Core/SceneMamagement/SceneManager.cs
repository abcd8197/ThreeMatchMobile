using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThreeMatch
{
    public class SceneManager : IManager
    {
        private Coroutine _sceneLoadCoroutine;

        public SceneType CurrentSceneType { get; private set; } = SceneType.Empty;

        public SceneManager()
        {

        }

        public void Dispose()
        {
            if (_sceneLoadCoroutine != null)
                CoroutineHandler.Instance.StopCoroutine(_sceneLoadCoroutine);
        }

        public void LoadScene(SceneType sceneType)
        {
            if (_sceneLoadCoroutine == null)
                _sceneLoadCoroutine = CoroutineHandler.Instance.StartCoroutine(LoadSceneCoroutine(sceneType));
        }

        private IEnumerator LoadSceneCoroutine(SceneType sceneType)
        {
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
        }
    }
}
