namespace ThreeMatch
{
    public class SwapRequest : IResolveRequest
    {
        public ResolveRequestType Type => ResolveRequestType.Swap;
        public BoardCellData From { get; }
        public SwapDirection Direction { get; }

        public SwapRequest(BoardCellData from, SwapDirection direction)
        {
            From = from;
            Direction = direction;
        }
    }

    public class ShakeRequest : IResolveRequest
    {
        public ResolveRequestType Type => ResolveRequestType.Shake;
        public int CellId { get; }

        public ShakeRequest(int cellId)
        {
            CellId = cellId;
        }
    }
}
