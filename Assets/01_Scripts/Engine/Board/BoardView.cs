using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ThreeMatch
{
    public sealed class BoardView : MonoBehaviour, IBoardView
    {
        [SerializeField] private BoardGrid _grid;

        private IReadOnlyList<ICellView> _cellViews;
        private List<BoardCellData> _cellDatas;
        private StageData _stageData;

        public IReadOnlyList<ICellView> CreateCellView(StageData stageData, List<BoardCellData> cellDatas)
        {
            _stageData = stageData;
            _cellDatas = cellDatas;

            var assetManager = Main.Instance.GetManager<AssetManager>();
            var list = new List<ICellView>(cellDatas.Count);

            for (int i = 0; i < cellDatas.Count; i++)
            {
                var cv = assetManager.GetInstantiateComponent<CellView>(
                    BundleGroup.defaultasset, "BoardCell", parent: _grid.transform);

                list.Add(cv);
            }

            _cellViews = list;
            _grid.SortGrid(list, stageData.Width, stageData.Height);

            // 최초 동기화
            for (int i = 0; i < _cellViews.Count; i++)
                ApplyCellVisual(i);

            return _cellViews;
        }

        public async Task Resolve(IReadOnlyList<BoardChange> changes)
        {
            if (changes == null || changes.Count == 0) return;
            if (_cellViews == null || _cellDatas == null) return;

            int i = 0;
            while (i < changes.Count)
            {
                if (changes[i] is SwapChange sw)
                {
                    await PlaySwap(sw);
                    i++;
                    continue;
                }

                if (changes[i] is ShakeChange sh)
                {
                    await PlayShake(sh);
                    i++;
                    continue;
                }

                if (changes[i] is RemoveChange rm)
                {
                    await PlayRemove(rm);
                    i++;
                    continue;
                }

                if (changes[i] is FallChange)
                {
                    int start = i;

                    while (i < changes.Count && changes[i] is FallChange) 
                        i++;

                    await PlayFallsRowBatched(changes, start, i - start);
                    continue;
                }

                if (changes[i] is SpawnChange)
                {
                    int start = i;
                    while (i < changes.Count && changes[i] is SpawnChange) 
                        i++;

                    await PlaySpawns(changes, start, i - start);
                    continue;
                }

                i++;
            }
        }

        private async Task PlaySwap(SwapChange sw)
        {
            if (!TryGet(sw.FromCellID, out var from) || !TryGet(sw.ToCellID, out var to))
                return;

            // 목적지(셀)의 anchor 월드 위치
            Vector3 fromTarget = from.GetPieceAnchorWorldPosition(); // to가 갈 위치
            Vector3 toTarget = to.GetPieceAnchorWorldPosition();     // from이 갈 위치

            // swap은 서로 교환이니까 타겟을 반대로
            Task t1 = from.MovePieceToWorld(toTarget, 0.15f);
            Task t2 = to.MovePieceToWorld(fromTarget, 0.15f);

            await Task.WhenAll(t1, t2);

            // 데이터 기준으로 확정
            ApplyCellVisual(sw.FromCellID);
            ApplyCellVisual(sw.ToCellID);
        }

        private async Task PlayShake(ShakeChange sh)
        {
            if (!TryGet(sh.CellID, out var cv)) return;
            await cv.Shake();
            ApplyCellVisual(sh.CellID);
        }

        private async Task PlayRemove(RemoveChange rm)
        {
            var tasks = new List<Task>(rm.Removed.Count);

            for (int k = 0; k < rm.Removed.Count; k++)
            {
                int id = rm.Removed[k].CellId;
                if (!TryGet(id, out var cv)) continue;

                tasks.Add(cv.PlayRemove(0.12f));
            }

            await Task.WhenAll(tasks);

            for (int k = 0; k < rm.Removed.Count; k++)
                ApplyCellVisual(rm.Removed[k].CellId);
        }

        private async Task PlayFallsRowBatched(IReadOnlyList<BoardChange> changes, int startIndex, int count)
        {
            if (_stageData == null) return;

            int width = _stageData.Width;
            int height = _stageData.Height;

            var buckets = new Dictionary<int, List<FallChange>>(height);

            for (int n = 0; n < count; n++)
            {
                var f = (FallChange)changes[startIndex + n];
                int toY = f.ToCellID / width;

                if (!buckets.TryGetValue(toY, out var list))
                {
                    list = new List<FallChange>(8);
                    buckets.Add(toY, list);
                }
                list.Add(f);
            }

            for (int y = 0; y < height; y++)
            {
                if (!buckets.TryGetValue(y, out var list) || list.Count == 0)
                    continue;

                for (int k = 0; k < list.Count; k++)
                {
                    if (TryGet(list[k].ToCellID, out var to))
                        to.SetPieceVisible(false);
                }

                var tasks = new List<Task>(list.Count);

                for (int k = 0; k < list.Count; k++)
                {
                    var f = list[k];
                    if (!TryGet(f.FromCellID, out var from) || !TryGet(f.ToCellID, out var to))
                        continue;

                    Vector3 target = to.GetPieceAnchorWorldPosition();
                    tasks.Add(from.MovePieceToWorld(target, 0.12f));
                }

                await Task.WhenAll(tasks);

                for (int k = 0; k < list.Count; k++)
                {
                    var f = list[k];
                    ApplyCellVisual(f.FromCellID);
                    ApplyCellVisual(f.ToCellID);
                }
            }
        }


        private async Task PlaySpawns(IReadOnlyList<BoardChange> changes, int startIndex, int count)
        {
            var tasks = new List<Task>(count);

            for (int n = 0; n < count; n++)
            {
                var sp = (SpawnChange)changes[startIndex + n];
                if (!TryGet(sp.CellID, out var cv)) continue;

                ApplyCellVisual(sp.CellID);
                tasks.Add(cv.PlaySpawn(0.12f));
            }

            await Task.WhenAll(tasks);
        }

        private void ApplyCellVisual(int cellId)
        {
            if (cellId < 0 || cellId >= _cellDatas.Count) return;

            var data = _cellDatas[cellId];
            if (!TryGet(cellId, out var cv)) return;

            cv.SetCellType(data.CellType);
            cv.SetPieceType(data.PieceType, data.ColorType);
        }

        private bool TryGet(int cellId, out CellView cv)
        {
            cv = null;
            if (_cellViews == null) return false;
            if (cellId < 0 || cellId >= _cellViews.Count) return false;

            cv = _cellViews[cellId] as CellView;
            return cv != null;
        }

        public void Dispose()
        {
            _cellViews = null;
            _cellDatas = null;
            _stageData = null;
        }
    }
}
