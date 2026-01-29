using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ThreeMatch
{
    public class BoardController : IDisposable
    {
        private readonly List<CellController> _cellControllers = new();
        private StageData _stageData;
        private BoardService _service;
        private IBoardView _view;
        private Vector2 _accumulated;

        private Coroutine _loopCoroutine = null;
        private bool _isResolving;
        private List<BoardCellData> _cellDatas;

        private int _score;
        private int _chainCount = 0;
        private GameManager _gameManager;
        private MissionService _mission;

        public event Action<int, int> OnScoreChanged;
        public event Action<StageGoalData, int> OnGoalDataChanged;
        public int Score => _score;

        public BoardController(GameManager gameManager)
        {
            _stageData = Main.Instance.GetManager<StageManager>().GetCurrentStageData();
            AdjustCellData(_stageData, _cellControllers);
            _cellDatas = _cellControllers.Select(x => x.Data).ToList();
            _service = new BoardService(_stageData, _cellDatas);
            _gameManager = gameManager;
            _gameManager.SetMoveChance(_stageData.MoveLimit);

            CreateMissionService();
        }

        private void CreateMissionService()
        {
            _mission = new MissionService(_stageData);
            _mission.OnProgressChanged += OnMissionProgressChanged;
            _mission.OnSuccess += OnSuccess;
            _mission.OnFail += OnFail;
        }

        private void OnMissionProgressChanged(StageGoalData goal, int current)
        {
            OnGoalDataChanged?.Invoke(goal, current);
        }


        private void OnSuccess()
        {
            var uiManager = Main.Instance.GetManager<UIManager>();
            uiManager.ShowPopup(PopupType.StageResultPopup);
            uiManager.GetActivatePopup<StageResultPopup>(PopupType.StageResultPopup).SetResult(true);
        }

        private void OnFail()
        {
            var uiManager = Main.Instance.GetManager<UIManager>();
            uiManager.ShowPopup(PopupType.StageResultPopup);
            uiManager.GetActivatePopup<StageResultPopup>(PopupType.StageResultPopup).SetResult(false);
        }

        public void SetBoardView(IBoardView boardView)
        {
            _view = boardView;

            var cellView = _view.CreateCellView(_stageData, _cellDatas);

            for (int i = 0; i < _cellControllers.Count; i++)
                _cellControllers[i].SetCellView(cellView[i]);
        }

        private void AdjustCellData(StageData stageData, List<CellController> cellDataList)
        {
            _cellControllers?.Clear();

            int total = stageData.Width * stageData.Height;

            var fixedCellDatas = stageData.FixedCells;
            var fixedCellMap = new Dictionary<int, StageFixedCellData>();

            for (int i = 0; i < fixedCellDatas.Count; i++)
                fixedCellMap.Add(fixedCellDatas[i].Coord.ToIndex(stageData.Width), fixedCellDatas[i]);

            cellDataList?.Clear();
            cellDataList.Capacity = total;

            for (int i = 0; i < total; i++)
            {
                BoardCellData cellData;
                if (fixedCellMap.TryGetValue(i, out var fixedCell))
                {
                    cellData = fixedCell.ToBoardCellData(stageData.Width);
                }
                else
                {
                    int x = i % stageData.Width;
                    int y = i / stageData.Width;

                    cellData = new()
                    {
                        CellID = i,
                        PieceType = PieceType.Normal,
                        CellType = CellType.Normal,
                        ColorType = PickColorNoInitialMatch(x, y, stageData, cellDataList),
                        Coordinate = new CellCoordinate(i % stageData.Width, i / stageData.Width)
                    };
                }

                var controller = new CellController(cellData, OnDragMethod);
                _cellControllers.Add(controller);
            }

            fixedCellMap.Clear();
        }

        private ColorType PickColorNoInitialMatch(int x, int y, StageData stageData, List<CellController> cellControllers)
        {
            var colors = Enum.GetValues(typeof(ColorType)).Cast<ColorType>().Where(c => c != ColorType.None).ToList();
            var banned = new HashSet<ColorType>();

            int w = stageData.Width;

            if (x >= 2)
            {
                var c1 = cellControllers[(x - 1) + y * w].Data;
                var c2 = cellControllers[(x - 2) + y * w].Data;

                if (c1.PieceType == PieceType.Normal && c2.PieceType == PieceType.Normal &&
                    c1.ColorType != ColorType.None && c1.ColorType == c2.ColorType)
                {
                    banned.Add(c1.ColorType);
                }
            }

            if (y >= 2)
            {
                var c1 = cellControllers[x + (y - 1) * w].Data;
                var c2 = cellControllers[x + (y - 2) * w].Data;

                if (c1.PieceType == PieceType.Normal && c2.PieceType == PieceType.Normal && c1.ColorType != ColorType.None && c1.ColorType == c2.ColorType)
                {
                    banned.Add(c1.ColorType);
                }
            }

            var candidates = colors.Where(c => !banned.Contains(c)).ToList();

            if (candidates.Count == 0)
                candidates = colors;

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }


        public void OnDragMethod(Vector2 delta, BoardCellData data)
        {
            const float threadHold = 20f;
            _accumulated += delta;

            if (_accumulated.x > -threadHold && _accumulated.x < threadHold &&
                _accumulated.y > -threadHold && _accumulated.y < threadHold)
                return;

            SwapDirection dir;
            if (_accumulated.x < -threadHold)
                dir = SwapDirection.Left;
            else if (_accumulated.x > threadHold)
                dir = SwapDirection.Right;
            else if (_accumulated.y < -threadHold)
                dir = SwapDirection.Down;
            else
                dir = SwapDirection.Up;

            _accumulated = Vector2.zero;

            if (_gameManager.RemainMove > 0)
            {
                if (GameUtility.IsValidDirection(_stageData.Width, _stageData.Height, data.Coordinate, dir))
                    Request(new SwapRequest(data, dir));
                else
                    Request(new ShakeRequest(data.CellID));
            }
        }

        public void Request(IResolveRequest request)
        {
            if (request == null)
                return;

            if (request.Type == ResolveRequestType.Swap)
            {
                _chainCount = 0;
                _gameManager.UseMoveChance();
            }

            _service.Request(request);
            EnsureResolveLoopRunning();
        }

        private void EnsureResolveLoopRunning()
        {
            if (_isResolving) return;

            _isResolving = true;
            _loopCoroutine = CoroutineHandler.Instance.StartCoroutine(ResolveLoop());
        }

        private void AddScore(int add)
        {
            if (add <= 0)
                return;
            _score += add;
            OnScoreChanged?.Invoke(_score, add);
        }


        private int CalcScoreAndAdvanceChain(IReadOnlyList<BoardChange> changes)
        {
            if (changes == null || changes.Count == 0) return 0;

            int add = 0;

            for (int i = 0; i < changes.Count; i++)
            {
                if (changes[i] is RemoveChange rm)
                {
                    int perCell = 100 + (100 * _chainCount);
                    add += rm.Removed.Count * perCell;

                    _chainCount++;
                }
            }

            return add;
        }


        private IEnumerator ResolveLoop()
        {
            try
            {
                Main.Instance.GetManager<GameManager>().RaycastEnabled(false);

                const int maxSteps = 500;

                for (int step = 0; step < maxSteps; step++)
                {
                    if (!_service.TryResolveNext(out var changes))
                        break;

                    if (changes == null || changes.Count == 0)
                        continue;

                    int scoreAdd = CalcScoreAndAdvanceChain(changes);

                    if (_view != null)
                    {
                        Task t = _view.Resolve(changes);
                        yield return WaitTask(t);
                    }

                    _mission.ApplyChanges(changes);

                    if (scoreAdd > 0)
                    {
                        AddScore(scoreAdd);
                        _mission.ApplyScore(scoreAdd);
                    }
                }

                _mission.TryEnd(_gameManager.RemainMove);

            }
            finally
            {
                Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
                _isResolving = false;
                _loopCoroutine = null;
            }
        }

        private IEnumerator WaitTask(Task task)
        {
            if (task == null)
                yield break;

            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted && task.Exception != null)
                Debug.LogException(task.Exception);
        }

        public void Dispose()
        {
            _cellControllers.Clear();
            _stageData = null;

            _service?.Dispose();
            _service = null;

            _view?.Dispose();
            _view = null;

            _mission?.Dispose();
            _mission = null;

            if (_loopCoroutine != null && CoroutineHandler.Instance != null)
                CoroutineHandler.Instance.StopCoroutine(_loopCoroutine);

            OnScoreChanged = null;
            OnGoalDataChanged = null;

            _loopCoroutine = null;
            _isResolving = false;

            _gameManager = null;
        }
    }
}
