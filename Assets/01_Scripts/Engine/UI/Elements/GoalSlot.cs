using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ThreeMatch
{
    public class GoalSlot : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private TextMeshProUGUI txt_GoalValue;

        public string GoalKey { get; private set; }

        public void SetData(StageGoalData goalData, int currentValue)
        {
            GoalKey = goalData.Key;
            Icon.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", goalData.GoalType.GetImageName(goalData.TargetColor));
            Icon.SetNativeSize();
            Icon.rectTransform.sizeDelta = Icon.rectTransform.sizeDelta * 0.6f;

            switch (goalData.GoalType)
            {
                case StageGoalType.Score:
                    txt_GoalValue.text = goalData.GoalValue.ToString();
                    break;
                case StageGoalType.CollectColor:
                    txt_GoalValue.text = Mathf.Max(0, (goalData.GoalValue - currentValue)).ToString();
                    break;
            }
        }
    }
}
