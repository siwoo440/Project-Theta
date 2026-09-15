namespace ProjectTheta.Stage
{
    /// <summary>
    /// 경쟁자를 어느 층에 두고, 언제부터 쫓아오게 할지의 규칙이다.
    ///
    /// 16일차에는 금태양·인기남이 둘 다 1층에서 시작해서,
    /// 연습용이어야 할 1층이 오히려 가장 험했다.
    ///
    ///   1F  연습      경쟁자 없음
    ///   2F  첫 경쟁   금태양 대기 → 만나면 이후 계단으로 추격
    ///   3F  이중 경쟁  인기남 대기 → 만나면 이후 계단으로 추격
    ///   4F  고가치    만난 경쟁자가 따라와 있음
    /// </summary>
    public static class OpponentFloorPlan
    {
        /// <summary>빠르고 공격적인 금태양은 조작을 익힌 뒤인 2층에 둔다.</summary>
        public const int GeumtaeyangStartFloor = 1;

        /// <summary>느리지만 멀리서 노리는 인기남은 3층에 둔다.</summary>
        public const int PopularGuyStartFloor = 2;

        /// <summary>
        /// 경쟁자가 플레이어를 따라 층을 옮겨야 하는지 판정한다.
        /// 한 번도 만나지 않았으면 제자리에서 기다린다.
        /// 1층에서 2층 금태양이 곧바로 내려오면 층별로 배치한 의미가 없기 때문이다.
        /// </summary>
        public static bool ShouldChase(
            bool hasMetPlayer,
            int opponentFloor,
            int playerFloor)
        {
            return hasMetPlayer &&
                   opponentFloor != playerFloor;
        }
    }
}
