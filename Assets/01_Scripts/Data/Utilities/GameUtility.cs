namespace ThreeMatch
{
    public static class GameUtility
    {
        public static bool IsValidDirection(int width, int height, CellCoordinate a, SwapDirection dir)
        {
            return dir switch
            {
                SwapDirection.Left => a.X >= 1,
                SwapDirection.Right => a.X < width - 1,
                SwapDirection.Up => a.Y < height - 1,
                SwapDirection.Down => a.Y >= 1,
                _ => false
            };
        }
        public static bool IsAdjancentTo(int width, int height, CellCoordinate a, CellCoordinate b)
        {
            if (a.X < 0 || a.X >= width || b.X < 0 || b.X >= width ||
                a.Y < 0 || a.Y >= height || b.Y < 0 || b.Y > height)
                return false;

            int dx = a.X - b.X;
            int dy = a.Y - b.Y;

            return dx == -1 || dx == 1 || dy == -1 || dy == 1;
        }
    }
}
