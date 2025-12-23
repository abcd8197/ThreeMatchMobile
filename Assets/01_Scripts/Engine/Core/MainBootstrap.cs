using UnityEngine;

namespace ThreeMatch
{
    public class MainBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            var main = Main.Instance;
            main.Initialize();

            main.RegisterManager(new SaveManager());
        }
    }
}