using UnityEngine;
using UniRx;

namespace ThreeMatch
{
    public class LoginPanel : MonoBehaviour
    {
        public UIButton btn_AppleLogin;

        private void Awake()
        {
            btn_AppleLogin.onClick.AsObservable()
                .Subscribe(_ => OnAppleLoginClicked())
                .AddTo(this);
        }

        private void OnAppleLoginClicked()
        {

        }
    }
}
