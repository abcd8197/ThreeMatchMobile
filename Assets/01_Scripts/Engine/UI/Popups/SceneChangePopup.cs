using System;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch
{
    public class SceneChangePopup : PopupBase
    {
        [SerializeField] private Image img_Icon;
        public override PopupType PopupType => PopupType.SceneChangePopup;

        public override void Show(int sortingOrder)
        {
            base.Show(sortingOrder);
            SetRandomTipImage();
            Main.Instance.GetManager<GameManager>().RaycastEnabled(false);
        }

        public override void Hide(Action<PopupBase> hideCompleteAction)
        {
            base.Hide(hideCompleteAction);
            Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
        }

        private void SetRandomTipImage()
        {
            var enumLength = Enum.GetValues(typeof(PieceType)).Length;
            var rndPieceType = (PieceType)UnityEngine.Random.Range((int)PieceType.Normal, enumLength);
            string rndSpriteName = rndPieceType.GetImageName(rndPieceType == PieceType.Normal ? UnityEngine.Random.Range((int)ColorType.Red, (int)ColorType.Purple + 1) : 0);
            var rndSprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", rndSpriteName);

            img_Icon.sprite = rndSprite;
            img_Icon.SetNativeSize();
            img_Icon.rectTransform.sizeDelta *= 3;
        }
    }
}
