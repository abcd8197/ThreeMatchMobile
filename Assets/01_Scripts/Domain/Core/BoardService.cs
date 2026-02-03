using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public sealed class BoardService : IDisposable
    {
        private enum ResolvePhase
        {
            Idle = 0,
            MatchCheckAfterSwap = 1,
            Gravity = 2,
            Spawn = 3,
            MatchCheckStable = 4,

        }

        private const int ChainGuardMax = 500;

        // 요청 큐
        private readonly Queue<IResolveRequest> _requests = new();
        // 데이터 참조
        private readonly StageData _stageData;
        // 보드 셀 데이터 리스트 참조
        private readonly List<BoardCellData> _cells;
        // 보드 크기
        private readonly int _width;
        private readonly int _height;
        private readonly Random _rng = new Random();

        // 페이즈 상태
        private ResolvePhase _phase = ResolvePhase.Idle;
        // 스왑 정보
        private int _swapFromId;
        private int _swapToId;
        private SwapDirection _swapDir;

        // 매치된 셀 ID 리스트
        private List<int> _matched;
        // 체인 가드(무한 루프 방지용)
        private int _chainGuard;

        private readonly MatchFinder _matchFinder = new();
        // 빈 리스트
        private static readonly List<BoardChange> _emptyChanges = new(0);

        public BoardService(StageData stageData, List<BoardCellData> cells)
        {
            _stageData = stageData ?? throw new ArgumentNullException(nameof(stageData));
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));
            _width = _stageData.Width;
            _height = _stageData.Height;

            _matchFinder.SetData(cells, stageData.Width, stageData.Height);
        }

        /// <summary>Service 요청</summary>
        public void Request(IResolveRequest request)
        {
            if (request == null) return;
            _requests.Enqueue(request);
        }

        public bool TryResolveNext(out List<BoardChange> changes)
        {
            changes = _emptyChanges;

            // 페이즈 및 체인 가드 체크
            if (_phase != ResolvePhase.Idle && _chainGuard++ >= ChainGuardMax)
            {
                ResetState();
                return false;
            }

            switch (_phase)
            {
                case ResolvePhase.Idle:
                {
                    if (_requests.Count == 0)
                        return false;

                    var req = _requests.Dequeue();

                    if (req == null)
                        return false;

                    switch (req.Type)
                    {
                        case ResolveRequestType.Shake:
                        {
                            var sh = (ShakeRequest)req;
                            changes = new List<BoardChange>(1) { new ShakeChange(sh.CellId) };
                            return true;
                        }
                        case ResolveRequestType.Swap:
                        {
                            var swapReq = (SwapRequest)req;
                            if (!BeginSwapRequest(swapReq, out changes))
                            {
                                ResetState();
                                return changes.Count > 0;
                            }

                            _phase = ResolvePhase.MatchCheckAfterSwap;
                            return true;
                        }
                        default:
                            return false;
                    }
                }
                case ResolvePhase.MatchCheckAfterSwap:
                {
                    changes = Step_MatchCheckAfterSwap();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;
                }
                case ResolvePhase.Gravity:
                {
                    changes = Step_GravityOneStep();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;
                }
                case ResolvePhase.Spawn:
                {
                    changes = Step_SpawnFromSpawners();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;
                }
                case ResolvePhase.MatchCheckStable:
                {
                    changes = Step_MatchCheckStable();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;
                }
                default:
                {
                    ResetState();
                    return false;
                }
            }
        }

        private bool BeginSwapRequest(SwapRequest req, out List<BoardChange> changes)
        {
            changes = new List<BoardChange>(8);

            int fromId = req.From.CellID;
            if (!IsValidCellId(fromId))
            {
                changes.Add(new ShakeChange(fromId));
                return false;
            }

            var from = _cells[fromId];

            if (!GameUtility.IsValidDirection(_width, _height, from.Coordinate, req.Direction))
            {
                changes.Add(new ShakeChange(fromId));
                return false;
            }

            int toId = GetNeighborCellId(fromId, req.Direction);
            if (!IsValidCellId(toId))
            {
                changes.Add(new ShakeChange(fromId));
                return false;
            }

            var to = _cells[toId];

            if (from.CellType == CellType.Hole || to.CellType == CellType.Hole)
            {
                changes.Add(new ShakeChange(fromId));
                return false;
            }

            if (from.PieceType == PieceType.None || to.PieceType == PieceType.None)
            {
                changes.Add(new ShakeChange(fromId));
                return false;
            }

            _swapFromId = fromId;
            _swapToId = toId;
            _swapDir = req.Direction;

            SwapPieces(_swapFromId, _swapToId);
            changes.Add(new SwapChange(_swapFromId, _swapToId, _swapDir, SwapChangeKind.Applied));

            _chainGuard = 0;
            return true;
        }

        private List<BoardChange> Step_MatchCheckAfterSwap()
        {
            var changes = new List<BoardChange>(32);

            _matched = _matchFinder.FindMatches();

            // 매치된 셀이 없다면 되돌린 후 상황 종료
            if (_matched.Count == 0)
            {
                SwapPieces(_swapFromId, _swapToId);
                changes.Add(new SwapChange(_swapFromId, _swapToId, _swapDir, SwapChangeKind.Reverted));
                ResetState();
                return changes;
            }

            var removed = ApplyRemoveAndCollect(_matched);
            changes.Add(new RemoveChange(removed));

            _phase = ResolvePhase.Gravity;
            return changes;
        }

        private List<BoardChange> Step_GravityOneStep()
        {
            var falls = ApplyGravityOneStep();
            var changes = new List<BoardChange>(falls.Count);

            if (falls.Count > 0)
            {
                for (int i = 0; i < falls.Count; i++)
                    changes.Add(falls[i]);

                return changes;
            }

            _phase = ResolvePhase.Spawn;
            return _emptyChanges;
        }

        private List<BoardChange> Step_SpawnFromSpawners()
        {
            var spawns = ApplySpawnOnlySpawner();
            var changes = new List<BoardChange>(spawns.Count);

            if (spawns.Count > 0)
            {
                for (int i = 0; i < spawns.Count; i++)
                    changes.Add(spawns[i]);

                _phase = ResolvePhase.Gravity;
                return changes;
            }

            _phase = ResolvePhase.MatchCheckStable;
            return _emptyChanges;
        }

        private List<BoardChange> Step_MatchCheckStable()
        {
            var changes = new List<BoardChange>(32);

            _matched = _matchFinder.FindMatches();
            if (_matched.Count == 0)
            {
                ResetState();
                return _emptyChanges;
            }

            var removed = ApplyRemoveAndCollect(_matched);
            changes.Add(new RemoveChange(removed));

            _phase = ResolvePhase.Gravity;
            return changes;
        }

        private List<RemovedCellInfo> ApplyRemoveAndCollect(List<int> matchedCellIds)
        {
            var removed = new List<RemovedCellInfo>(matchedCellIds.Count);

            for (int i = 0; i < matchedCellIds.Count; i++)
            {
                int id = matchedCellIds[i];

                if (!IsValidCellId(id))
                    continue;

                var c = _cells[id];

                if (c.CellType == CellType.Hole)
                    continue;
                if (c.PieceType == PieceType.None)
                    continue;

                removed.Add(new RemovedCellInfo(id, c.PieceType, c.ColorType));

                c.PieceType = PieceType.None;
                c.ColorType = ColorType.None;
            }

            return removed;
        }

        private void ResetState()
        {
            _phase = ResolvePhase.Idle;
            _swapFromId = -1;
            _swapToId = -1;
            _swapDir = default;
            _matched = null;
            _chainGuard = 0;
        }

        private List<FallChange> ApplyGravityOneStep()
        {
            var falls = new List<FallChange>(64);

            for (int y = 1; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int fromId = ToId(x, y);
                    if (!HasPiece(fromId)) continue;

                    if (TryGetNextFallTarget(fromId, out int nextId))
                    {
                        MovePiece(fromId, nextId);
                        falls.Add(new FallChange(fromId, nextId));
                    }
                }
            }

            return falls;
        }

        private bool TryGetNextFallTarget(int fromId, out int nextId)
        {
            nextId = -1;

            if (!HasPiece(fromId))
                return false;

            int x = fromId % _width;
            int y = fromId / _width;

            // 바닥이면 불가
            if (y <= 0)
                return false;

            // 1) 바로 밑
            int downId = ToId(x, y - 1);
            if (IsFallableEmptyCell(downId))
            {
                nextId = downId;
                return true;
            }

            // 2) 좌하단
            if (x > 0)
            {
                int leftDownId = ToId(x - 1, y - 1);
                if (IsFallableEmptyCell(leftDownId))
                {
                    nextId = leftDownId;
                    return true;
                }
            }

            // 3) 우하단
            if (x < _width - 1)
            {
                int rightDownId = ToId(x + 1, y - 1);
                if (IsFallableEmptyCell(rightDownId))
                {
                    nextId = rightDownId;
                    return true;
                }
            }

            return false;
        }

        private bool IsFallableEmptyCell(int id)
        {
            if (!IsValidCellId(id)) return false;
            return !HasPiece(id);
        }

        private List<SpawnChange> ApplySpawnOnlySpawner()
        {
            var spawns = new List<SpawnChange>(32);

            for (int i = 0; i < _cells.Count; i++)
            {
                var c = _cells[i];
                if (c.CellType != CellType.Spawner) continue;
                if (c.PieceType != PieceType.None) continue;

                c.PieceType = PieceType.Normal;
                c.ColorType = RandomColorType();
                _cells[i] = c;

                spawns.Add(new SpawnChange(i, c.PieceType, c.ColorType));
            }

            return spawns;
        }

        private ColorType RandomColorType()
        {
            var values = (ColorType[])Enum.GetValues(typeof(ColorType));
            int pick = _rng.Next((int)ColorType.Red, values.Length);
            return values[pick];
        }
        private void SwapPieces(int aId, int bId)
        {
            var a = _cells[aId];
            var b = _cells[bId];
            a.SwapPiece(b);
        }

        private void MovePiece(int fromId, int toId)
        {
            var from = _cells[fromId];
            var to = _cells[toId];

            to.PieceType = from.PieceType;
            to.ColorType = from.ColorType;

            from.PieceType = PieceType.None;
            from.ColorType = ColorType.None;

            _cells[fromId] = from;
            _cells[toId] = to;
        }

        private bool HasPiece(int cellId)
        {
            if (!IsValidCellId(cellId)) return false;
            var c = _cells[cellId];
            if (c.CellType == CellType.Hole) return false;
            return c.PieceType != PieceType.None;
        }

        private int GetNeighborCellId(int fromId, SwapDirection dir)
        {
            var c = _cells[fromId].Coordinate;
            int x = c.X;
            int y = c.Y;

            return dir switch
            {
                SwapDirection.Left => (x - 1 >= 0) ? ToId(x - 1, y) : -1,
                SwapDirection.Right => (x + 1 < _width) ? ToId(x + 1, y) : -1,
                SwapDirection.Up => (y + 1 < _height) ? ToId(x, y + 1) : -1,
                SwapDirection.Down => (y - 1 >= 0) ? ToId(x, y - 1) : -1,
                _ => -1
            };
        }

        private int ToId(int x, int y) => x + (y * _width);
        private bool IsValidCellId(int id) => id >= 0 && id < _cells.Count;

        public void Dispose()
        {
            _requests?.Clear();
            _matchFinder?.Dispose();
        }
    }
}
