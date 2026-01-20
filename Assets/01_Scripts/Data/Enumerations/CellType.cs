namespace ThreeMatch
{
    public enum CellType
    {
        /// <summary>일반 셀</summary>
        Normal = 0,
        /// <summary>구멍 (비어있음)</summary>
        Hole,
        /// <summary>막힘</summary>
        Blocked,
        /// <summary>자신의 상단에서 스폰되는 셀</summary>
        Spawner,
        /// <summary>포탈 시작하는곳</summary>
        PortarIn,
        /// <summary>포탈 도착지 셀</summary>
        PortarOut,
    }
}
