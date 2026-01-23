namespace ThreeMatch
{
    public enum SwapChangeKind
    {
        Applied = 0, // 스왑 적용
        Reverted, // 매치가 없어서 되돌림
        Rejected, // 스왑 불가능
    }
}
