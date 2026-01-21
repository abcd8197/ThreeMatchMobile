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
        private Action<float, float> _onDrag;
        public int RaycastOrder => 3;

        public void SetData(BoardCellData data, Action<float, float> onDrag)
        {
            var assetManager = Main.Instance.GetManager<AssetManager>();
            _onDrag = onDrag;
        }

        public void SetCellType(CellType cellType)
        {
            cellRenderer.enabled = cellType != CellType.Hole;
            if (cellType != CellType.Hole)
                cellRenderer.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", cellType.GetImageName());
        }

        public void SetPieceType(PieceType pieceType, ColorType colorType = ColorType.None)
        {
            pieceRenderer.enabled = pieceType != PieceType.None;
            if (pieceType != PieceType.None)
            {
                if (pieceType == PieceType.Normal)
                    pieceRenderer.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", pieceType.GetImageName((int)colorType));
                else
                    pieceRenderer.sprite = Main.Instance.GetManager<AssetManager>().GetSprite(BundleGroup.defaultasset_tex, "defaultAtlas", pieceType.GetImageName());
            }

            pieceRenderer.transform.localPosition = Vector3.zero;
        }

        #region ## IRaycastable ##
        public void OnBeginDrag()
        {

        }

        public void OnDrag(Vector2 delta)
        {
            _onDrag?.Invoke(delta.x, delta.y);
        }

        public void OnEndDrag()
        {

        }

        public void OnPointerClick()
        {

        }

        public void OnPointerDown()
        {

        }

        public void OnPointerUp()
        {

        }
        #endregion

        public void SetPosition(float x, float y)
        {
            transform.localPosition = new Vector2(x, y);
        }

        public async Task MoveTo(SwapDirection direction)
        {
            const float duration = 0.5f;
            var tcs = new TaskCompletionSource<bool>();

            Tween tween = direction switch
            {
                SwapDirection.Left => pieceRenderer.transform.DOLocalMoveX(0.5f, duration).SetRecyclable(true).SetEase(Ease.OutExpo),
                SwapDirection.Right => pieceRenderer.transform.DOLocalMoveX(0.5f, duration).SetRecyclable(true).SetEase(Ease.OutExpo),
                SwapDirection.Up => pieceRenderer.transform.DOLocalMoveY(0.5f, duration).SetRecyclable(true).SetEase(Ease.OutExpo),
                SwapDirection.Down => pieceRenderer.transform.DOLocalMoveY(0.5f, duration).SetRecyclable(true).SetEase(Ease.OutExpo),
                _ => null
            };

            tween.SetEase(Ease.OutExpo).OnComplete(() => tcs.TrySetResult(true));
            await tcs.Task;
        }

        public async Task Shake()
        {
            const float shakeDuration = 0.5f;
            Vector2 start = pieceRenderer.transform.position;
            pieceRenderer.transform.DOShakePosition(shakeDuration, 0.1f);
            await Task.Delay((int)(shakeDuration * 1000));
        }
    }
}
