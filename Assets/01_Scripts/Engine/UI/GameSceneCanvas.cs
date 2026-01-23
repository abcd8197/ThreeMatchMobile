using UnityEngine;
using TMPro;
using UniRx;

namespace ThreeMatch
{
    public class GameSceneCanvas : MonoBehaviour
    {
        [SerializeField] private UIButton btn_Setting;
        [SerializeField] private TextMeshProUGUI txt_Score;
        [SerializeField] private TextMeshProUGUI txt_RemainMove;

        private int _prevScore;
        private int _prevRemainMove;

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = Main.Instance.GetManager<GameManager>();
            btn_Setting.onClick.AsObservable().Subscribe(x => OnClickPause()).AddTo(this);
        }

        private void Update()
        {
            if (_prevScore != _gameManager.GetScore)
                UpdateScore();
            if (_prevRemainMove != _gameManager.RemainMove)
                UpdateRemainMove();
        }

        private void UpdateScore()
        {
            txt_Score.text = $"Score: {_gameManager.GetScore}";
        }

        private void UpdateRemainMove()
        {
            txt_RemainMove.text = $"남은 횟수: {_gameManager.RemainMove}";
        }

        private void OnClickPause()
        {
            var uiManager = Main.Instance.GetManager<UIManager>();
            if (uiManager.IsPopupActivated(PopupType.GameSettingPopup))
                return;

            uiManager.ShowPopup(PopupType.GameSettingPopup);
        }
    }
}
