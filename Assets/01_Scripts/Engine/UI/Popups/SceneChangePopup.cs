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

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }

        public override void Show(int sortingOrder)
        {
            base.Show(sortingOrder);
            SetRandomTipImage();
        }

        private void SetRandomTipImage()
        {
            string rndSpriteName = spriteNames[Random.Range(0, spriteNames.Length)];
            var rndSprite = Main.Instance.GetManager<AssetManager>().GetAsset<Sprite>(BundleGroup.defaultasset, rndSpriteName);
        }
    }
}
