using UnityEngine;
using UniRx;
using TMPro;

namespace ThreeMatch
{
    public class StageEnterPopup : PopupBase
    {
        public override PopupType PopupType => PopupType.StageEnterPopup;

        [SerializeField] private TextMeshProUGUI txt_StageNumber;

        [Header("Buttons")]
        [SerializeField] private UIButton Btn_Start;
        [SerializeField] private UIButton Btn_Exit;

        protected override void Awake()
        {
            base.Awake();
            Btn_Start.onClick.AsObservable().Subscribe(_ => OnClickStart()).AddTo(this);
            Btn_Exit.onClick.AsObservable().Subscribe(_ => OnClickExit()).AddTo(this);

            var stageNumber = Main.Instance.GetManager<StageManager>().GetCurrentStage();
            txt_StageNumber.text = $"Stage {stageNumber}";
        }

        private void OnClickStart()
        {
            var stageManger = Main.Instance.GetManager<StageManager>();
            var stageDataMax = stageManger.GetStageDataCount();

            if(stageManger.GetCurrentStage() >= stageDataMax)
            {
                // Stage 오픈 안됨
                return;
            }


            if (Main.Instance.GetManager<ItemManager>().GetItemCount(ItemType.Stemina) > 0)
            {
                Main.Instance.GetManager<ItemManager>().UseItem(ItemType.Stemina, 1);
                Main.Instance.GetManager<SceneManager>().LoadScene(SceneType.Game);
                Main.Instance.GetManager<UIManager>().HidePopup(PopupType);
            }
        }

        private void OnClickExit()
        {
            Main.Instance.GetManager<UIManager>().HidePopup(PopupType);
        }
    }
}
