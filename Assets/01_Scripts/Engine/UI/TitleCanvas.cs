using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

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
        public UIButton btn_AppleLogin;
        public UIButton btn_GoogleLogin;

        private Coroutine _loadingTextCoroutine;
        private Coroutine _loadingProgressCoroutine;

        private void Start()
        {
            LoadingEnabled(true);
            StartLoadingProgress();
            LoginPanel.SetActive(false);
        }

        private void OnDisable()
        {
            LoadingEnabled(false);
        }

        #region ## Loading Panel Methods ##
        public void LoadingEnabled(bool enalbed)
        {
            LoginPanel.SetActive(enalbed);

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
            var assetManager = Main.Instance.GetManager<AssetManager>();
            byte dotNumber = 3;

            while (!assetManager.AssetLoadCompleted)
            {
                txt_Loading.text = bseText;
                for (int i = 0; i < dotNumber; i++)
                {
                    yield return new WaitForSeconds(0.5f);
                    txt_Loading.text += ".";
                }
            }

            LoginPanel.SetActive(true);
            this.LoadingEnabled(false);
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
                ProgressBarFill.fillAmount = assetManager.progress;
                txt_Progress.text = $"{progress:0}%";
                txt_InnerProgress.text = $"데이터 로딩중... {progress:0}%";
                yield return null;
            }

            ProgressBarFill.fillAmount = 1f;
            txt_Progress.text = $"100%";
            ProgressBarPanel.SetActive(false);
        }
        #endregion
    }
}
