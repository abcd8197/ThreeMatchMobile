using System;
using System.Collections;
using UnityEngine;

namespace ThreeMatch
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;

        private Coroutine _fadeCoroutine;
        public bool Recycleable { get; protected set; } = false;
        public PopupType PopupType { get; protected set; } = PopupType.None;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show(int sortingOrder)
        {
            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }
        }

        public virtual void Hide(Action<PopupBase> hideCompleteAction)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(false, hideCompleteAction));
        }

        public virtual void Fade(bool enabled)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeCoroutine(true));
        }

        protected virtual IEnumerator FadeCoroutine(bool fadeIn, Action<PopupBase> hideCompleteAction = null)
        {
            const float FadeDuration = 0.75f;
            float cnt = 0f;
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
