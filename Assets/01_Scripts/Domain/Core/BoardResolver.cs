using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreeMatch
{
    
    public class BoardResolver : IDisposable
    {
        // TODO : BoardController에서 기능들을 이곳으로 분리하여 ResolveState등으로 
        
        private readonly Queue<ResolveContext> _resolveQueue = new();
        private readonly StageData _stageData;
        private readonly List<BoardCellData> _cellDatas;
        private readonly HashSet<BoardCellData> _modifiedCells = new();

        public Queue<ResolveContext> Queue => _resolveQueue;

        public BoardResolver(StageData stageData, List<BoardCellData> cellDatas)
        {
            _stageData = stageData;
            _cellDatas = cellDatas;
        }

        public void Drag(SwapDirection dir, BoardCellData data)
        {

        }

        

        public void Dispose()
        {

        }

    }
}
