using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace ThreeMatch
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class SpriteRendererButton : MonoBehaviour, IRaycastable
    {
        private const float FromScaleX = 1.2f;
        private const float FromScaleY = 1.2f;
        private const float TweenDuration = 1f;
        public int RaycastOrder => 0;

        private Sequence _seq;
        private bool _isAnimated = false;

        public bool UseTween = true;
        public UnityEvent onClick;
        public UnityEvent onBeginDrag;
        public UnityEvent<Vector2> onDrag;
        public UnityEvent onEndDrag;

        private void OnDisable()
        {
            _seq?.Complete(false);
            _isAnimated = false;
        }

        private void OnDestroy()
        {
            if (DOTween.IsTweening(transform))
                DOTween.Kill(transform);
            _seq = null;
        }

        public void OnBeginDrag()
        {
            onBeginDrag?.Invoke();
        }

        public void OnDrag(Vector2 delta)
        {
            onDrag?.Invoke(delta);
        }

        public void OnEndDrag()
        {
            onEndDrag?.Invoke();
        }

        public void OnPointerDown()
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
                .OnComplete(() => _isAnimated = false);
            _seq.Play();
        }

        public void OnPointerUp()
        {
            if (!UseTween)
                return;

            if (_isAnimated)
                onClick?.Invoke();
        }

        public void OnPointerClick()
        {
            if (!UseTween)
                onClick?.Invoke();
        }
    }
}
