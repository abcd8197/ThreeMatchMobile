using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

namespace ThreeMatch
{
    public class UIButton : Image, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private const float FromScaleX = 1.2f;
        private const float FromScaleY = 1.2f;
        private const float TweenDuration = 1f;

        private Sequence _seq;
        private bool _isAnimated = false;

        public bool UseTween = true;
        public UnityEvent onClick;

        protected override void OnDisable()
        {
            base.OnDisable();
            _seq?.Complete(false);
            _isAnimated = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (DOTween.IsTweening(transform))
                DOTween.Kill(transform);
            _seq = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!UseTween)
                return;
            if (_isAnimated)
                return;

            _isAnimated = true;

            _seq?.Kill(false);
            _seq = DOTween.Sequence()
                .SetRecyclable(true)
                .Append(transform.DOScaleX(1f, TweenDuration * 0.5f).From(FromScaleX).SetEase(Ease.OutBounce))
                .Join(transform.DOScaleY(1f, TweenDuration).From(FromScaleY).SetEase(Ease.OutBounce))
                .OnComplete(() =>  _isAnimated = false );
            _seq.Play();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(!UseTween)
                return;

            if (_isAnimated)
                onClick?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!UseTween)
                onClick?.Invoke();
        }
    }
}
