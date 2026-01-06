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

        [Header("ProgressBar Panel")]
        public GameObject ProgressBarPanel;
        public Image ProgressBarFill;
        public TextMeshProUGUI txt_Progress;
        public TextMeshProUGUI txt_InnerProgress;


        [Header("Login Panel")]
        public GameObject LoginPanel;
        public UIButton btn_AppleLogin;
        public UIButton btn_GoogleLogin;


        private void Awake()
        {
            
        }

        public void LoadingEnabled(bool enalbed)
        {
            if (enalbed)
            {
                LoadingIcon.rectTransform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
                LoginPanel.SetActive(false);
            }
            else if (DOTween.IsTweening(LoadingIcon.rectTransform))
            {
                LoginPanel.SetActive(true);
                LoadingIcon.rectTransform.DOKill();
            }
        }
    }
}
