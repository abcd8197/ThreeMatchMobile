using System;
using System.Collections;
using UnityEngine;

namespace ThreeMatch
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour
    {
        protected CanvasGroup _canvasGroup;

        private Coroutine _fadeCoroutine;
        public bool Recycleable { get; protected set; } = false;
        public abstract PopupType PopupType { get; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void OnDestroy()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
        }

        public virtual void Show(int sortingOrder)
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();

            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(true));
        }

        public virtual void Hide(Action<PopupBase> hideCompleteAction)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(false, hideCompleteAction));
        }

        public virtual void Fade(bool enabled)
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(true));
        }

        protected virtual IEnumerator FadeCoroutine(bool fadeIn, Action<PopupBase> hideCompleteAction = null)
        {
            const float FadeDuration = 0.35f;
            float cnt = 0f;
            _canvasGroup.alpha = fadeIn ? 0 : 1;
            while (cnt < FadeDuration)
            {
                cnt += Time.deltaTime;
                if (fadeIn)
                    _canvasGroup.alpha = Mathf.Clamp01(cnt / FadeDuration);
                else
                    _canvasGroup.alpha = Mathf.Clamp01(1f - (cnt / FadeDuration));

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _fadeCoroutine = null;
            hideCompleteAction?.Invoke(this);
        }
    }
}
