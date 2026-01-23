namespace ThreeMatch
{
    public enum PieceType
    {
        /// <summary>아무것도 없음</summary>
        None,
        /// <summary>일반 피스타입. 색상은 ColorType 따라감.</summary>
        Normal,
        /// <summary>얼음. 직접 타격을 입거나 주위 타일이 터지면 HP가 깎임. HP는 2개.</summary>
        Ice,

        /// <summary>가로 라인을 모두 없애는 아이템. 클릭 또는 직접 타격을 입으면 발동.</summary>
        RocketRow,
        /// <summary>세로 라인을 모두 없애는 아이템. 클릭 또는 직접 타격을 입으면 발동.</summary>
        RocketCol,
        /// <summary>자신을 기준으로 상하좌우 2칸씩 좌상,우상,좌하,우하 1칸씩을 HP1씩 깎는 폭탄 아이템. 클릭 또는 직접 타격을 입으면 발동</summary>
        Bomb,
        /// <summary>현재 보드의 가장 많은 색상을 모두 터트리는 아이템. 클릭 또는 직접 타격을 입으면 발동</summary>
        Rainbow,
    }
}
