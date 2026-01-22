using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public sealed class BoardService : IDisposable
    {
        private readonly Queue<IResolveRequest> _requests = new();

        private readonly StageData _stageData;
        private readonly List<BoardCellData> _cells;

        private readonly int _width;
        private readonly int _height;

        private readonly Random _rng = new Random();
        private readonly bool[] _marked;

        // ===== Step machine state =====
        private ResolvePhase _phase = ResolvePhase.Idle;

        private int _swapFromId;
        private int _swapToId;
        private SwapDirection _swapDir;

        private List<int> _matched; // 현재 라운드 매치 결과

        private int _chainGuard;
        private const int ChainGuardMax = 500;

        public BoardService(StageData stageData, List<BoardCellData> cells)
        {
            _stageData = stageData ?? throw new ArgumentNullException(nameof(stageData));
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));

            _width = _stageData.Width;
            _height = _stageData.Height;

            _marked = new bool[_cells.Count];
        }

        public void Request(IResolveRequest request)
        {
            if (request == null) return;
            _requests.Enqueue(request);
        }

        /// <summary>
        /// "한 스텝"만 진행하고 그 스텝에서 발생한 change들을 반환.
        /// 더 진행할 게 없으면 false.
        /// </summary>
        public bool TryResolveNext(out List<BoardChange> changes)
        {
            changes = s_Empty;

            // 안전 가드
            if (_phase != ResolvePhase.Idle && _chainGuard++ >= ChainGuardMax)
            {
                // 가드 초과 시 강제 종료
                ResetState();
                return false;
            }

            // Idle이면 새 요청 시작
            if (_phase == ResolvePhase.Idle)
            {
                if (_requests.Count == 0) return false;

                var req = _requests.Dequeue();
                if (req == null) return false;

                if (req.Type == ResolveRequestType.Shake)
                {
                    var sh = (ShakeRequest)req;
                    changes = new List<BoardChange>(1) { new ShakeChange(sh.CellId) };
                    // 상태는 Idle 유지
                    return true;
                }

                if (req.Type != ResolveRequestType.Swap)
                    return false;

                // Swap 요청을 "스텝 머신"에 올리기
                var swapReq = (SwapRequest)req;
                if (!BeginSwapRequest(swapReq, out changes))
                {
                    // BeginSwapRequest가 ShakeChange 등을 changes로 채워줌
                    ResetState();
                    return changes.Count > 0;
                }

                // 첫 스텝은 Swap Applied만 리턴하고 다음 스텝으로 넘어감
                _phase = ResolvePhase.MatchCheckAfterSwap;
                return true;
            }

            switch (_phase)
            {
                case ResolvePhase.MatchCheckAfterSwap:
                    changes = Step_MatchCheckAfterSwap();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;

                case ResolvePhase.Gravity:
                    changes = Step_GravityOneStep();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;

                case ResolvePhase.Spawn:
                    changes = Step_SpawnFromSpawners();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;

                case ResolvePhase.MatchCheckStable:
                    changes = Step_MatchCheckStable();
                    return changes.Count > 0 || _phase != ResolvePhase.Idle;

                default:
                    ResetState();
                    return false;
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

            // Swap 적용(1스텝)
            SwapPieces(_swapFromId, _swapToId);
            changes.Add(new SwapChange(_swapFromId, _swapToId, _swapDir, SwapChangeKind.Applied));

            _chainGuard = 0;
            return true;
        }

        // === Step 2: 스왑 후 매치 검사 (없으면 revert / 있으면 remove로 진행) ===
        private List<BoardChange> Step_MatchCheckAfterSwap()
        {
            var changes = new List<BoardChange>(32);

            _matched = FindMatches();
            if (_matched.Count == 0)
            {
                // revert 1스텝
                SwapPieces(_swapFromId, _swapToId);
                changes.Add(new SwapChange(_swapFromId, _swapToId, _swapDir, SwapChangeKind.Reverted));
                ResetState();
                return changes;
            }

            // 매치 있으면 제거 1스텝
            ApplyRemove(_matched);
            changes.Add(new RemoveChange(_matched));

            _phase = ResolvePhase.Gravity;
            return changes;
        }

        // === Gravity: 한 번 호출에 "한 칸씩"만 이동한 결과 ===
        private List<BoardChange> Step_GravityOneStep()
        {
            var falls = ApplyGravityOneStep(); // 당신이 이미 OneStep 버전 만들어둔 그대로 사용
            var changes = new List<BoardChange>(falls.Count);

            if (falls.Count > 0)
            {
                for (int i = 0; i < falls.Count; i++)
                    changes.Add(falls[i]);

                // 여전히 Gravity phase 유지
                return changes;
            }

            // 더 이상 떨어질 게 없으면 Spawn으로
            _phase = ResolvePhase.Spawn;
            return s_Empty;
        }

        // === Spawn: Spawner에서만, 그리고 빈 칸만 ===
        private List<BoardChange> Step_SpawnFromSpawners()
        {
            var spawns = ApplySpawnOnlySpawner();
            var changes = new List<BoardChange>(spawns.Count);

            if (spawns.Count > 0)
            {
                for (int i = 0; i < spawns.Count; i++)
                    changes.Add(spawns[i]);

                // 스폰했으니 다시 떨어질 수 있음
                _phase = ResolvePhase.Gravity;
                return changes;
            }

            // 스폰도 없으면 안정 상태 → 매치 검사
            _phase = ResolvePhase.MatchCheckStable;
            return s_Empty;
        }

        // === 안정 상태에서 매치 검사: 있으면 remove, 없으면 종료 ===
        private List<BoardChange> Step_MatchCheckStable()
        {
            var changes = new List<BoardChange>(32);

            _matched = FindMatches();
            if (_matched.Count == 0)
            {
                ResetState();
                return s_Empty;
            }

            ApplyRemove(_matched);
            changes.Add(new RemoveChange(_matched));

            // 제거했으니 다시 중력
            _phase = ResolvePhase.Gravity;
            return changes;
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

        private List<int> FindMatches()
        {
            Array.Clear(_marked, 0, _marked.Length);

            var result = new List<int>(32);

            for (int y = 0; y < _height; y++)
            {
                int runStartX = 0;
                int runLen = 1;

                for (int x = 1; x < _width; x++)
                {
                    if (IsSameMatchKey(x - 1, y, x, y)) runLen++;
                    else
                    {
                        if (runLen >= 3) MarkRunHorizontal(y, runStartX, runLen, result);
                        runStartX = x;
                        runLen = 1;
                    }
                }
                if (runLen >= 3) MarkRunHorizontal(y, runStartX, runLen, result);
            }

            for (int x = 0; x < _width; x++)
            {
                int runStartY = 0;
                int runLen = 1;

                for (int y = 1; y < _height; y++)
                {
                    if (IsSameMatchKey(x, y - 1, x, y)) runLen++;
                    else
                    {
                        if (runLen >= 3) MarkRunVertical(x, runStartY, runLen, result);
                        runStartY = y;
                        runLen = 1;
                    }
                }
                if (runLen >= 3) MarkRunVertical(x, runStartY, runLen, result);
            }

            return result;
        }

        private bool IsSameMatchKey(int ax, int ay, int bx, int by)
        {
            int aId = ToId(ax, ay);
            int bId = ToId(bx, by);

            var a = _cells[aId];
            var b = _cells[bId];

            if (a.CellType == CellType.Hole || b.CellType == CellType.Hole) return false;
            if (a.PieceType != PieceType.Normal || b.PieceType != PieceType.Normal) return false;
            if (a.ColorType == ColorType.None || b.ColorType == ColorType.None) return false;

            return a.ColorType == b.ColorType;
        }

        private void MarkRunHorizontal(int y, int startX, int len, List<int> result)
        {
            for (int i = 0; i < len; i++)
            {
                int id = ToId(startX + i, y);
                if (_marked[id]) continue;
                _marked[id] = true;
                result.Add(id);
            }
        }

        private void MarkRunVertical(int x, int startY, int len, List<int> result)
        {
            for (int i = 0; i < len; i++)
            {
                int id = ToId(x, startY + i);
                if (_marked[id]) continue;
                _marked[id] = true;
                result.Add(id);
            }
        }


        private void ApplyRemove(List<int> matchedCellIds)
        {
            for (int i = 0; i < matchedCellIds.Count; i++)
            {
                int id = matchedCellIds[i];
                if (!IsValidCellId(id)) continue;

                var c = _cells[id];
                c.PieceType = PieceType.None;
                c.ColorType = ColorType.None;
                _cells[id] = c;
            }
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
            // None 제외하고 안전하게 랜덤
            var values = (ColorType[])Enum.GetValues(typeof(ColorType));
            int count = 0;
            for (int i = 0; i < values.Length; i++)
                if (values[i] != ColorType.None) count++;

            if (count <= 0) return ColorType.None;

            int pick = _rng.Next(0, count);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == ColorType.None) continue;
                if (pick-- == 0) return values[i];
            }

            return ColorType.None;
        }


        private void SwapPieces(int aId, int bId)
        {
            var a = _cells[aId];
            var b = _cells[bId];
            a.SwapPiece(b); // class라면 재대입 불필요
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
            _requests.Clear();
        }

        private static readonly List<BoardChange> s_Empty = new List<BoardChange>(0);

        private enum ResolvePhase
        {
            Idle = 0,
            MatchCheckAfterSwap = 1,
            Gravity = 2,
            Spawn = 3,
            MatchCheckStable = 4,
        }
    }
}
