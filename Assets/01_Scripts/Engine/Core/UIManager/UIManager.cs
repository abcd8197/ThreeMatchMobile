using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class UIManager : IManager
    {
        private const int BaseSortingOrder = 1000;
        private readonly Dictionary<PopupType, PopupBase> _activePopups = new();
        private readonly Dictionary<PopupType, PopupBase> _recyclablePopupCache = new();
        private readonly AssetManager _assetManager;
        private readonly UIRoot _uiRoot;

        public UIManager(AssetManager assetManager)
        {
            _assetManager = assetManager ?? throw new ArgumentNullException(nameof(assetManager));
            _uiRoot = new GameObject("UIRoot").AddComponent<UIRoot>();
        }

        public void ShowPopup(PopupType popupType)
        {
            if (_activePopups.ContainsKey(popupType))
            {
                return;
            }

            var popupInstance = _assetManager.GetNewInstance<PopupBase>(popupType.GetBundleGroup(), popupType.ToString(), _uiRoot.PopupRoot);
            popupInstance.Show(BaseSortingOrder + _activePopups.Count);
            _activePopups.Add(popupType, popupInstance);
        }

        public void HidePopup(PopupType popupType)
        {
            if (_activePopups.TryGetValue(popupType, out var popupInstance))
            {
                popupInstance.Hide();
                if (popupInstance.Recycleable)
                {
                    popupInstance.gameObject.SetActive(false);
                    _recyclablePopupCache.Add(popupType, popupInstance);
                }
                else
                {
                    _assetManager.ReleaseInstance(popupType.GetBundleGroup(), popupType.ToString());
                    GameObject.Destroy(popupInstance.gameObject);
                }
                _activePopups.Remove(popupType);
            }
        }

        public T GetActivatePopup<T>(PopupType popupType) where T : PopupBase
        {
            if (_activePopups.TryGetValue(popupType, out var popupInstance))
            {
                return popupInstance as T;
            }
            return null;
        }

        public bool IsAnyPopupActive() => _activePopups.Count > 0;

        public void Dispose()
        {
            foreach (var popup in _activePopups.Values)
            {
                popup.Hide();
                GameObject.Destroy(popup.gameObject);
            }

            _activePopups.Clear();
            GameObject.Destroy(_uiRoot.gameObject);
        }
    }
}
