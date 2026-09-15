namespace ProjectTheta.Stage
{
    /// <summary>
    /// 층 하나의 고정 배치다. 모든 층이 같은 복도 구조를 쓰므로
    /// 계단·회수 지점 위치는 층이 달라도 같고, 세로 좌표만 층만큼 올라간다.
    /// </summary>
    public static class FloorLayout
    {
        /// <summary>위층으로 가는 계단이다. 복도 왼쪽 끝 계단실에 둔다.</summary>
        public const float UpStairX = -15.6f;

        /// <summary>아래층으로 가는 계단이다. 위층 계단과 나란히 둔다.</summary>
        public const float DownStairX = -12.4f;

        /// <summary>계단 앞에 서는 지점의 세로 좌표다. 보행 구역 안쪽이다.</summary>
        public const float StairStandY = 0.35f;

        /// <summary>
        /// 회수 지점은 계단 반대편 끝이다.
        /// 계단과 회수 지점을 멀리 떼어 놓아야 "데리고 이동하는" 행위가 생긴다.
        /// </summary>
        public const float RecoveryX = 16.2f;

        public const float RecoveryY = -2.15f;

        /// <summary>플레이어가 1층에서 판을 시작하는 자리다.</summary>
        public const float PlayerStartX = -13.5f;

        public const float PlayerStartY = -0.45f;
    }
}
