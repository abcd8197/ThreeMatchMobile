namespace ThreeMatch
{
    public enum SwapDirection { Left, Right, Up, Down}

    public static class SwapDirectionExtension
    {
        public static SwapDirection Reverse(this SwapDirection dir)
        {
            return dir switch
            {
                SwapDirection.Left => SwapDirection.Right,
                SwapDirection.Right => SwapDirection.Left,
                SwapDirection.Up => SwapDirection.Down,
                SwapDirection.Down => SwapDirection.Up,
                _ => SwapDirection.Left
            };
        }
    }
}
