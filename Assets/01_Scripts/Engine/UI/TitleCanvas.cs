using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UniRx;

namespace ThreeMatch
{
    public class TitleCanvas : MonoBehaviour
    {
        [Header("Loading Panel")]
        public GameObject LoadingPanel;
        public Image LoadingIcon;
        public TextMeshProUGUI txt_Loading;

        [Header("ProgressBar Panel")]
        public GameObject ProgressBarPanel;
        public Image ProgressBarFill;
        public TextMeshProUGUI txt_Progress;
        public TextMeshProUGUI txt_InnerProgress;


        [Header("Login Panel")]
        public GameObject LoginPanel;
        public UIButton btn_GuestLogin;
        public UIButton btn_AppleLogin;
        public UIButton btn_GoogleLogin;

        private Coroutine _loadingTextCoroutine;
        private Coroutine _loadingProgressCoroutine;
        private Coroutine _loginCoroutine;

        private void Start()
        {
            LoadingEnabled(true);
            StartLoadingProgress();
            LoginPanel.SetActive(false);
            SetButtonMethods();
        }

        private void OnDisable()
        {
            LoadingEnabled(false);

            if (_loadingProgressCoroutine != null)
                StopCoroutine(_loadingProgressCoroutine);
            if (_loginCoroutine != null)
                StopCoroutine(_loginCoroutine);
        }

        #region ## Login Methods ##
        private void SetButtonMethods()
        {
            btn_GuestLogin.onClick.AsObservable().Subscribe(x => ClickLoginButton(AuthType.Guest)).AddTo(this);
            btn_AppleLogin.onClick.AsObservable().Subscribe(x => ClickLoginButton(AuthType.Apple)).AddTo(this);
            btn_GoogleLogin.onClick.AsObservable().Subscribe(x => ClickLoginButton(AuthType.Google)).AddTo(this);
        }

        private void ClickLoginButton(AuthType authType)
        {
            Main.Instance.GetManager<AuthManager>().Login(authType);
            LoadingEnabled(true);
            _loginCoroutine = StartCoroutine(WaitForAuth());
        }

        private IEnumerator WaitForAuth()
        {
            LoginPanel.SetActive(false);

            var authManager = Main.Instance.GetManager<AuthManager>();
            while (!authManager.Authenticated)
            {
                yield return null;
            }

            if (authManager.CurrentType == AuthType.None)
                LoginPanel.SetActive(true);

            yield return new WaitForSeconds(1f);
            LoadingEnabled(false);
            Main.Instance.GetManager<SceneManager>().LoadScene(SceneType.Main);

            _loginCoroutine = null;
        }
        #endregion

        #region ## Loading Panel Methods ##
        public void LoadingEnabled(bool enalbed)
        {
            LoadingPanel.SetActive(enalbed);

            if (enalbed)
            {
                LoadingIcon.rectTransform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
                _loadingTextCoroutine = StartCoroutine(LoadingTextDirection());
            }
            else
            {
                if (DOTween.IsTweening(LoadingIcon.rectTransform))
                    LoadingIcon.rectTransform.DOKill();
                if (_loadingTextCoroutine != null)
                    StopCoroutine(_loadingTextCoroutine);
            }
        }

        private IEnumerator LoadingTextDirection()
        {
            const string bseText = "데이터 로딩중";
            const float DotInterval = 0.5f;

            float cnt = 0f;
            var assetManager = Main.Instance.GetManager<AssetManager>();
            byte dotNumber = 3;

            while (!assetManager.AssetLoadCompleted)
            {
                txt_Loading.text = bseText;
                for (int i = 0; i < dotNumber; i++)
                {
                    while (cnt < DotInterval)
                    {
                        cnt += Time.unscaledDeltaTime;

                        if (assetManager.AssetLoadCompleted)
                            break;

                        yield return null;
                    }
                    cnt = 0f;
                    txt_Loading.text += ".";
                }
            }

            this.LoadingEnabled(false);
            LoginPanel.SetActive(true);
        }
        #endregion

        #region ## Progress Bar Methods ###
        private void StartLoadingProgress()
        {
            ProgressBarPanel.SetActive(true);
            _loadingProgressCoroutine = StartCoroutine(LoadingProgressDireciton());
        }

        private IEnumerator LoadingProgressDireciton()
        {
            var assetManager = Main.Instance.GetManager<AssetManager>();
            while (!assetManager.AssetLoadCompleted)
            {
                float progress = assetManager.progress * 100f;
                int bundleCount = assetManager.BundleCount;
                int loadedBundleCount = assetManager.LoadedBundleCount;

                ProgressBarFill.fillAmount = assetManager.progress;
                txt_Progress.text = $"{loadedBundleCount}/{bundleCount}%";
                txt_InnerProgress.text = $"{progress:0}%";
                yield return null;
            }

            ProgressBarFill.fillAmount = 1f;
            txt_Progress.text = $"100%";
            ProgressBarPanel.SetActive(false);
            _loadingProgressCoroutine = null;
        }
        #endregion
    }
}
