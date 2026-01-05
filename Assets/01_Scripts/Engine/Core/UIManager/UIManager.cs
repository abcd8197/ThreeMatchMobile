using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class UIManager
    {
        private const int BaseSortingOrder = 1000;
        private readonly Dictionary<PopupType, PopupBase> _activePopups = new();
        private AssetManager _assetManager;

        public UIManager(AssetManager assetManager)
        {
            _assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
        }

        public void ShowPopup(PopupType popupType)
        {
            if (_activePopups.ContainsKey(popupType))
            {
                return;
            }

            var popupInstance = _assetManager.GetNewInstance<PopupBase>(BundleGroup.Default, popupType.ToString(), null);
            popupInstance.Show(BaseSortingOrder + _activePopups.Count);
            _activePopups.Add(popupType, popupInstance);
        }

        public void HidePopup(PopupType popupType)
        {
            if (_activePopups.TryGetValue(popupType, out var popupInstance))
            {
                popupInstance.Hide();
                GameObject.Destroy(popupInstance.gameObject);
                _activePopups.Remove(popupType);
            }
        }
    }
}
