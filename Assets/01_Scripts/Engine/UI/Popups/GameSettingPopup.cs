using UnityEngine;
using UniRx;
using System;

namespace ThreeMatch
{
    public class GameSettingPopup : PopupBase
    {
        [SerializeField] private UIButton btn_Exit;
        [SerializeField] private UIButton btn_Home;
        public override PopupType PopupType => PopupType.GameSettingPopup;
        protected override void Awake()
        {
            base.Awake();
            btn_Exit.onClick.AsObservable().Subscribe(_ => OnClickExit()).AddTo(this);
            btn_Home.onClick.AsObservable().Subscribe(_ => OnClickHome()).AddTo(this);
        }

        public override void Show(int sortingOrder)
        {
            base.Show(sortingOrder);
            Main.Instance.GetManager<GameManager>().RaycastEnabled(false);
        }

        public override void Hide(Action<PopupBase> hideCompleteAction)
        {
            base.Hide(hideCompleteAction);
            Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
        }

        private void OnClickHome()
        {
            Main.Instance.GetManager<GameManager>().OnChangedGameState(GameState.GiveUp);
            Main.Instance.GetManager<SceneManager>().LoadScene(SceneType.Main);
            Main.Instance.GetManager<UIManager>().HidePopup(PopupType);
        }

        private void OnClickExit()
        {
            Main.Instance.GetManager<UIManager>().HidePopup(PopupType);
        }
    }
}
