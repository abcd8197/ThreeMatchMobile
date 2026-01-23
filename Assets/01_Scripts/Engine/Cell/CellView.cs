using System;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace ThreeMatch
{
    public class CellView : MonoBehaviour, ICellView, IRaycastable
    {
        [SerializeField] private SpriteRenderer cellRenderer;
        [SerializeField] private SpriteRenderer pieceRenderer;
        [SerializeField] private SpriteRenderer portalRenderer;

        private Action<float, float> _onDrag;
        public int RaycastOrder => 3;

        public void SetData(BoardCellData data, Action<float, float> onDrag)
        {
            _onDrag = onDrag;
        }

        public void SetCellType(CellType cellType)
        {
            cellRenderer.enabled = cellType != CellType.Hole;
            if (cellType != CellType.Hole)
                cellRenderer.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", cellType.GetImageName());
            portalRenderer.enabled = cellType == CellType.PortarIn || cellType == CellType.PortarOut;
        }

        public void SetPieceType(PieceType pieceType, ColorType colorType = ColorType.None)
        {
            pieceRenderer.enabled = pieceType != PieceType.None;

            if (pieceType != PieceType.None)
            {
                if (pieceType == PieceType.Normal)
                {
                    pieceRenderer.sprite = Main.Instance.GetManager<AssetManager>()
                        .GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", pieceType.GetImageName((int)colorType));
                }
                else
                {
                    pieceRenderer.sprite = Main.Instance.GetManager<AssetManager>()
                        .GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", pieceType.GetImageName());
                }
            }

            pieceRenderer.transform.localPosition = Vector3.zero;
            pieceRenderer.transform.localScale = Vector3.one;
            var c = pieceRenderer.color; 
            c.a = 1f; 
            pieceRenderer.color = c;
        }

        public void SetPosition(float x, float y)
        {
            transform.localPosition = new Vector2(x, y);
        }


        public Vector3 GetPieceAnchorWorldPosition()
        {
            return pieceRenderer.transform.parent.TransformPoint(Vector3.zero);
        }

        public void SetPieceVisible(bool visible)
        {
            pieceRenderer.enabled = visible;
        }

        public Task MovePieceToWorld(Vector3 worldPos, float duration, Ease ease = Ease.OutQuad)
        {
            Tween t = pieceRenderer.transform.DOMove(worldPos, duration).SetEase(ease).SetRecyclable(true);
            return AwaitTween(t);
        }

        public Task PlayRemove(float duration = 0.14f)
        {
            Tween tScale = pieceRenderer.transform.DOScale(0f, duration).SetEase(Ease.InBack).SetRecyclable(true);
            Tween tAlpha = TweenPieceAlpha(0f, duration);

            return AwaitTween(DOTween.Sequence().Join(tScale).Join(tAlpha));
        }

        public Task PlaySpawn(float duration = 0.14f)
        {
            pieceRenderer.transform.localScale = Vector3.zero;
            var c = pieceRenderer.color; c.a = 0f; pieceRenderer.color = c;

            Tween tScale = pieceRenderer.transform.DOScale(1f, duration).SetEase(Ease.OutBack).SetRecyclable(true);
            Tween tAlpha = TweenPieceAlpha(1f, duration);

            return AwaitTween(DOTween.Sequence().Join(tScale).Join(tAlpha));
        }

        private Tween TweenPieceAlpha(float targetA, float duration)
        {
            return DOTween.To(
                () => pieceRenderer.color.a,
                a =>
                {
                    var c = pieceRenderer.color;
                    c.a = a;
                    pieceRenderer.color = c;
                },
                targetA,
                duration
            ).SetEase(Ease.OutQuad).SetRecyclable(true);
        }

        private static Task AwaitTween(Tween tween)
        {
            if (tween == null) 
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            tween.OnComplete(() => tcs.TrySetResult(true));
            tween.OnKill(() =>
            {
                if (!tcs.Task.IsCompleted) tcs.TrySetResult(true);
            });
            return tcs.Task;
        }

        public Task MoveTo(SwapDirection direction)
        {
            const float duration = 0.15f;
            const float step = 0.5f;

            Vector3 delta = direction switch
            {
                SwapDirection.Left => new Vector3(-step, 0f, 0f),
                SwapDirection.Right => new Vector3(+step, 0f, 0f),
                SwapDirection.Up => new Vector3(0f, +step, 0f),
                SwapDirection.Down => new Vector3(0f, -step, 0f),
                _ => Vector3.zero
            };

            Tween t = pieceRenderer.transform.DOLocalMove(pieceRenderer.transform.localPosition + delta, duration)
                .SetEase(Ease.OutQuad).SetRecyclable(true);

            return AwaitTween(t);
        }

        public Task Shake(float duration = 0.25f, float strength = 0.10f)
        {
            Tween t = pieceRenderer.transform.DOShakePosition(duration, strength).SetRecyclable(true);
            return AwaitTween(t);
        }

        #region ## IRaycastable ##
        public void OnBeginDrag() { }
        public void OnDrag(Vector2 delta) => _onDrag?.Invoke(delta.x, delta.y);
        public void OnEndDrag() { }
        public void OnPointerClick() { }
        public void OnPointerDown() 
        { 

        }
        public void OnPointerUp() { }
        #endregion
    }
}
