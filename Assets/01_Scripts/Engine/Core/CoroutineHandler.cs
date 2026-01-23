using UnityEngine;

namespace ThreeMatch
{
    public class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler sInstance;
        public static CoroutineHandler Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject obj = new GameObject("_CoroutineHandler");
                    DontDestroyOnLoad(obj);
                    sInstance = obj.AddComponent<CoroutineHandler>();
                }
                return sInstance;
            }
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
