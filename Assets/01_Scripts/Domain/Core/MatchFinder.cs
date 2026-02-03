using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public class MatchFinder : IDisposable
    {
        // 보드 셀 데이터 리스트 참조
        private List<BoardCellData> _cells;

        // 매치 검사용 마킹 배열
        private bool[] _marked;
        private int _width;
        private int _height;

        public void SetData(List<BoardCellData> cells, int width, int height)
        {
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));
            _width = width;
            _height = height;

            int cellCount = width * height;

            if (_marked == null || _marked.Length != cellCount)
            {
                Array.Resize(ref _marked, cellCount);
            }
        }

        public List<int> FindMatches()
        {
            Array.Clear(_marked, 0, _marked.Length);

            var result = new List<int>(32);

            for (int y = 0; y < _height; y++)
            {
                int runStartX = 0;
                int runLen = 1;

                for (int x = 1; x < _width; x++)
                {
                    if (IsSameMatchKey(x - 1, y, x, y))
                        runLen++;
                    else
                    {
                        if (runLen >= 3)
                            MarkRunHorizontal(y, runStartX, runLen, result);

                        runStartX = x;
                        runLen = 1;
                    }
                }

                if (runLen >= 3)
                    MarkRunHorizontal(y, runStartX, runLen, result);
            }

            for (int x = 0; x < _width; x++)
            {
                int runStartY = 0;
                int runLen = 1;

                for (int y = 1; y < _height; y++)
                {
                    if (IsSameMatchKey(x, y - 1, x, y))
                        runLen++;
                    else
                    {
                        if (runLen >= 3)
                            MarkRunVertical(x, runStartY, runLen, result);

                        runStartY = y;
                        runLen = 1;
                    }
                }

                if (runLen >= 3)
                    MarkRunVertical(x, runStartY, runLen, result);
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

                if (_marked[id])
                    continue;

                _marked[id] = true;
                result.Add(id);
            }
        }

        private void MarkRunVertical(int x, int startY, int len, List<int> result)
        {
            for (int i = 0; i < len; i++)
            {
                int id = ToId(x, startY + i);

                if (_marked[id])
                    continue;

                _marked[id] = true;
                result.Add(id);
            }
        }

        private int ToId(int x, int y) => x + (y * _width);

        public void Dispose()
        {
            _cells = null;
        }
    }
}
