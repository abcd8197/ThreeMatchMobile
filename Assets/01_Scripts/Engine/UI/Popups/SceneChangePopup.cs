using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch
{
    public class SceneChangePopup : PopupBase
    {
        [SerializeField] private Image img_Tip;
        private readonly string[] spriteNames = new string[2]
        {
            "img_ui_home", "img_ui_setting"
        };

        public override PopupType PopupType => PopupType.SceneChangePopup;

        public override void Show(int sortingOrder)
        {
            base.Show(sortingOrder);
            SetRandomTipImage();
            Main.Instance.GetManager<GameManager>().RaycastEnabled(false);
        }

        public override void Hide(System.Action<PopupBase> hideCompleteAction)
        {
            base.Hide(hideCompleteAction);
            Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
        }

        private void SetRandomTipImage()
        {
            string rndSpriteName = spriteNames[Random.Range(0, spriteNames.Length)];
            var rndSprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", rndSpriteName);
        }
    }
}
