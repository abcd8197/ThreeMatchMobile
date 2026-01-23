using System.Collections.Generic;
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
        [SerializeField] private RectTransform goalParent;

        private int _prevScore;
        private int _prevRemainMove;

        private List<GoalSlot> _goalSlots;

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = Main.Instance.GetManager<GameManager>();
            btn_Setting.onClick.AsObservable().Subscribe(x => OnClickPause()).AddTo(this);
            CreateGoalSlots();
        }

        private void Start()
        {
            _gameManager.SubscbireOnGoalValueChanged(UpdateGoal);
        }

        private void OnDestroy()
        {
            _gameManager.UnSubscbireOnGoalValueChanged(UpdateGoal);
        }

        private void CreateGoalSlots()
        {
            var stageData = Main.Instance.GetManager<StageManager>().GetCurrentStageData();
            if (stageData.Goals == null || stageData.Goals.Count <= 0)
                return;

            _goalSlots = new();

            for (int i = 0; i < stageData.Goals.Count; i++)
            {
                var goalData = stageData.Goals[i];
                var goalSlot = Main.Instance.GetManager<AssetManager>().GetInstantiateComponent<GoalSlot>(BundleGroup.defaultasset, "GoalSlot", parent: goalParent);
                goalSlot.SetData(goalData, 0);
                _goalSlots.Add(goalSlot);
            }
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

        private void UpdateGoal(StageGoalData goalData, int goalValue)
        {
            for (int i = 0; i < _goalSlots.Count; i++)
            {
                if (goalData.Key.Equals(_goalSlots[i].GoalKey))
                {
                    _goalSlots[i].SetData(goalData, goalValue);
                    break;
                }
            }
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
