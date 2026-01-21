using System;
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
        private BoardResolver _boardResolver;
        private BoardView _boardView;
        private Vector2 _accumulated;

        public BoardController()
        {
            _stageData = Main.Instance.GetManager<StageManager>().GetCurrentStageData();
            AdjustCellData(_stageData, _cellControllers);

            if (Application.isPlaying)
            {
                _boardView = Main.Instance.GetManager<AssetManager>().GetInstantiateComponent<BoardView>(BundleGroup.defaultasset, nameof(BoardView));
                CreateCellView(_boardView, _stageData);
            }
            _boardResolver = new BoardResolver(_stageData, _cellControllers.Select(x => x.Data).ToList());
        }

        private void AdjustCellData(StageData stageData, List<CellController> cellDataList)
        {
            _cellControllers?.Clear();

            int totalCellCount = stageData.Width * stageData.Height;
            var fixedCellDatas = stageData.FixedCells;
            Dictionary<int, StageFixedCellData> fixedCellIndices = new();

            for (int i = 0; i < fixedCellDatas.Count; i++)
                fixedCellIndices.Add(fixedCellDatas[i].Coord.ToIndex(stageData.Width), fixedCellDatas[i]);

            cellDataList?.Clear();
            cellDataList.Capacity = totalCellCount;

            for (int i = 0; i < totalCellCount; i++)
            {
                BoardCellData cellData;
                if (fixedCellIndices.ContainsKey(i))
                {
                    cellData = fixedCellIndices[i].ToBoardCellData(stageData.Width);
                }
                else
                {
                    cellData = new()
                    {
                        CellID = i,
                        PieceType = PieceType.Normal,
                        CellType = CellType.Normal,
                        ColorType = GetRandomColorType(),
                        Coordinate = new CellCoordinate(i % stageData.Width, i / stageData.Width)
                    };
                }

                CellController controller = new(cellData, OnDragMethod);
                _cellControllers.Add(controller);
            }

            fixedCellIndices.Clear();

            ColorType GetRandomColorType()
            {
                var enumValues = Enum.GetValues(typeof(ColorType)).Length;
                return (ColorType)UnityEngine.Random.Range((int)ColorType.Red, enumValues);
            }
        }

        private void CreateCellView(BoardView boardRoot, StageData stageData)
        {
            if (boardRoot == null)
                return;

            var dataList = _cellControllers.Select(x => x.Data).ToList();
            var fixedCellList = stageData.FixedCells;
            var cellView = boardRoot.CreateCellView(dataList, stageData.Width, stageData.Height);

            for (int i = 0; i < _cellControllers.Count; i++)
                _cellControllers[i].SetCellView(cellView[i]);
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

            StartMove(dir, data);
            _accumulated = Vector2.zero;
        }

        public void StartMove(SwapDirection dir, BoardCellData data)
        {
            if (GameUtility.IsValidDirection(_stageData.Width, _stageData.Height, data.Coordinate, dir))
                MoveTo(dir, data);
            else
                CellviewShake(data.CellID);
        }

        private async Task CellviewShake(int cellID)
        {
            Main.Instance.GetManager<GameManager>().RaycastEnabled(false);
            await _cellControllers[cellID].ShakeCellView();
            Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
        }

        public async Task MoveTo(SwapDirection dir, BoardCellData data)
        {
            try
            {
                Main.Instance.GetManager<GameManager>().RaycastEnabled(false);
                var dirData = GetDirData(dir, data);
                if (dirData != null)
                {
                    int fromIdx = data.Coordinate.ToIndex(_stageData.Width);
                    int toIdx = dirData.Coordinate.ToIndex(_stageData.Width);

                    bool cellviewExist = _cellControllers[fromIdx].CellView != null && _cellControllers[toIdx].CellView != null;
                    if (cellviewExist)
                    {
                        Task t1 = _cellControllers[fromIdx].CellView.MoveTo(dir);
                        Task t2 = _cellControllers[toIdx].CellView.MoveTo(dir.Reverse());
                        await Task.WhenAll(t1, t2);
                    }

                    if (TrySwap(dir, data))
                    {
                        await Task.CompletedTask;
                    }
                    else
                    {
                        Task t1 = _cellControllers[fromIdx].CellView.MoveTo(dir.Reverse());
                        Task t2 = _cellControllers[toIdx].CellView.MoveTo(dir);
                        await Task.WhenAll(t1, t2);
                    }
                }
            }
            finally
            {
                Main.Instance.GetManager<GameManager>().RaycastEnabled(true);
            }
        }

        public bool TrySwap(SwapDirection dir, BoardCellData data)
        {
            if (!GameUtility.IsValidDirection(_stageData.Width, _stageData.Height, data.Coordinate, dir))
                return false;

            var dirData = GetDirData(dir, data);
            int fromIdx = data.Coordinate.ToIndex(_stageData.Width);
            int toIdx = dirData.Coordinate.ToIndex(_stageData.Width);
            _cellControllers[fromIdx].Data.Swap(_cellControllers[toIdx].Data);

            _cellControllers[fromIdx].UpdateCellView();
            _cellControllers[toIdx].UpdateCellView();
            return true;
        }

        public BoardCellData GetDirData(SwapDirection dir, BoardCellData data)
        {
            var dataIndex = data.CellID;

            return dir switch
            {
                SwapDirection.Left => _cellControllers[dataIndex - 1].Data,
                SwapDirection.Right => _cellControllers[dataIndex + 1].Data,
                SwapDirection.Up => _cellControllers[dataIndex + _stageData.Width].Data,
                SwapDirection.Down => _cellControllers[dataIndex - _stageData.Width].Data,
                _ => null
            };
        }

        public void Dispose()
        {
            _cellControllers?.Clear();
            _stageData = null;
            _boardResolver?.Dispose();
            _boardResolver = null;
            if (_boardView != null)
                UnityEngine.Object.Destroy(_boardView.gameObject);
            _boardView = null;
        }
    }
}
