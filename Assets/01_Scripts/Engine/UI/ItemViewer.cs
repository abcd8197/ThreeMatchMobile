using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

namespace ThreeMatch
{
    public class ItemViewer : MonoBehaviour
    {
        [SerializeField] private ItemType _itemType;
        [SerializeField] private Image img_Icon;
        [SerializeField] private TextMeshProUGUI txt_Amount;

        private void Awake()
        {
            if (img_Icon != null)
            {
                img_Icon.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", _itemType.GetImageName());
            }
            if (txt_Amount != null)
                Main.Instance.GetManager<ItemManager>().Bind(_itemType, x => txt_Amount.text = x.ToString()).AddTo(this);
        }
    }
}
