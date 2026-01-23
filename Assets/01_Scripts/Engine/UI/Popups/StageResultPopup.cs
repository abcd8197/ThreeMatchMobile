using System;
using UnityEngine;
using TMPro;
using UniRx;

namespace ThreeMatch
{
    public class StageResultPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI txt_Result;
        [SerializeField] private TextMeshProUGUI txt_Score;
        [SerializeField] private UIButton btn_Confirm;

        public override PopupType PopupType => PopupType.StageResultPopup;

        protected override void Awake()
        {
            base.Awake();
            btn_Confirm.onClick.AsObservable().Subscribe(_ => OnClickConfirm());
        }

        public override void Show(int sortingOrder)
        {
            base.Show(sortingOrder);

            var gameManager = Main.Instance.GetManager<GameManager>();
            gameManager.RaycastEnabled(false);
            txt_Score.text = $"Score: {gameManager.GetScore}";
        }

        public override void Hide(Action<PopupBase> hideCompleteAction)
        {
            base.Hide(hideCompleteAction);
            Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
        }

        public void SetResult(bool result)
        {
            txt_Result.text = result ? "스테이지 클리어" : "실패";

            if (result)
            {
                Main.Instance.GetManager<StageManager>().CurrentStageCleared();
                Main.Instance.GetManager<SaveManager>().SaveData();
            }
        }

        public void OnClickConfirm()
        {
            Main.Instance.GetManager<UIManager>().HidePopup(PopupType);
            Main.Instance.GetManager<SceneManager>().LoadScene(SceneType.Main);
        }
    }
}
