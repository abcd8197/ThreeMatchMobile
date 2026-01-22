using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreeMatch
{
    public interface IBoardView : IDisposable
    {
        public IReadOnlyList<ICellView> CreateCellView(StageData stageData, List<BoardCellData> cellDatas);
        public Task Resolve(IReadOnlyList<BoardChange> changes);
    }
}
