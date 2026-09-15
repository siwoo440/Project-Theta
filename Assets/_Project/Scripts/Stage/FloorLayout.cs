namespace ProjectTheta.Stage
{
    /// <summary>
    /// 층 하나의 고정 배치다. 모든 층이 같은 복도 구조를 쓰므로
    /// 계단·회수 지점 위치는 층이 달라도 같고, 세로 좌표만 층만큼 올라간다.
    /// </summary>
    public static class FloorLayout
    {
        /// <summary>
        /// 위층으로 가는 계단이다. 복도 오른쪽에 둔다.
        /// 맨 오른쪽 끝은 회수 지점이 쓰므로, 회수 판정 구역과 겹치지 않게 조금 안쪽에 둔다.
        /// </summary>
        public const float UpStairX = 13.4f;

        /// <summary>아래층으로 가는 계단이다. 복도 왼쪽에 둔다.</summary>
        public const float DownStairX = -13.8f;

        /// <summary>이 거리 안에 들어오면 계단을 쓸 수 있다.</summary>
        public const float StairInteractRadius = 1.4f;

        /// <summary>회수 지점 판정 구역의 가로 폭이다.</summary>
        public const float RecoveryWidth = 1.8f;

        /// <summary>계단 앞에 서는 지점의 세로 좌표다. 보행 구역 안쪽이다.</summary>
        public const float StairStandY = 0.35f;

        /// <summary>
        /// 회수 지점은 복도 오른쪽 끝이다.
        /// 올라온 계단(왼쪽)에서 회수 지점까지 복도를 가로질러야 하므로
        /// "데리고 이동하는" 행위가 층마다 생긴다.
        /// </summary>
        public const float RecoveryX = 16.2f;

        public const float RecoveryY = -2.15f;

        /// <summary>플레이어가 1층에서 판을 시작하는 자리다.</summary>
        public const float PlayerStartX = -13.5f;

        public const float PlayerStartY = -0.45f;
    }
}
