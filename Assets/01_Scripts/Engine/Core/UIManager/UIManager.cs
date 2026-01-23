using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch
{
    public class UIManager : IManager, ISceneChangeNotifyModule
    {
        private const int BaseSortingOrder = 1000;
        private readonly Dictionary<PopupType, PopupBase> _activePopups = new();
        private readonly Dictionary<PopupType, PopupBase> _recyclablePopupCache = new();
        private readonly Dictionary<PopupType, PopupBase> _popupToRemove = new();
        private readonly AssetManager _assetManager;
        private readonly UIRoot _uiRoot;

        public Type ModuleType => typeof(ISceneChangeNotifyModule);

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

            var popupInstance = _assetManager.GetInstantiateComponent<PopupBase>(popupType.GetBundleGroup(), popupType.ToString(), parent: _uiRoot.PopupRoot);
            popupInstance.Show(BaseSortingOrder + _activePopups.Count);
            _activePopups.Add(popupType, popupInstance);
        }

        public void HidePopup(PopupType popupType)
        {
            if (_activePopups.TryGetValue(popupType, out var popupInstance))
            {
                _activePopups.Remove(popupType);
                _popupToRemove.Add(popupType, popupInstance);
                popupInstance.Hide(HidePopupAfterFadeout);
            }
        }

        private void HidePopupAfterFadeout(PopupBase popup)
        {
            if (popup.Recycleable)
            {
                _recyclablePopupCache.Add(popup.PopupType, popup);
            }
            else
            {
                _assetManager.ReleasePrefab(popup.PopupType.GetBundleGroup(), popup.PopupType.ToString(), popup.gameObject);
            }

            _popupToRemove.Remove(popup.PopupType);
        }

        public T GetActivatePopup<T>(PopupType popupType) where T : PopupBase
        {
            if (_activePopups.TryGetValue(popupType, out var popupInstance))
            {
                return popupInstance as T;
            }
            return null;
        }

        public bool IsPopupActivated(PopupType popupType) => _activePopups.ContainsKey(popupType);
        public bool IsAnyPopupActive() => _activePopups.Count > 0;

        public void Dispose()
        {
            foreach (var popup in _activePopups.Values)
            {
                GameObject.Destroy(popup.gameObject);
            }

            _activePopups.Clear();
            GameObject.Destroy(_uiRoot.gameObject);
        }

        public void OnStartSceneChange(SceneType fromScene, SceneType toScene)
        {
            ShowPopup(PopupType.SceneChangePopup);
        }

        public void OnSceneChanged(SceneType sceneType)
        {
            if (sceneType != SceneType.Empty)
                HidePopup(PopupType.SceneChangePopup);
            _uiRoot.OnChangedScene(sceneType);
        }
    }
}
