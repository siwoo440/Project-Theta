using ProjectTheta.Balance;

namespace ProjectTheta.Stage
{
    /// <summary>기획서 16.2절 랭크다.</summary>
    public enum StageRank
    {
        C,
        B,
        A,
        S
    }

    /// <summary>
    /// 점수 구간으로 랭크를 매긴다.
    ///
    /// S는 단순 고득점이 아니라 추가 조건을 요구한다.
    /// 점수만으로 S를 주면 일반 NPC를 안전하게 반복 회수하는 것이 최적이 되어
    /// "위험 행동과 고난도 NPC를 적극적으로 활용한 플레이"라는 S의 정의가 깨지기 때문이다.
    ///
    /// 구간 값은 프로토타입 플레이 데이터를 보고 조정할 임시 기준이다.
    /// </summary>
    public static class StageRankLogic
    {
        public const int DefaultBThreshold = 6000;
        public const int DefaultAThreshold = 11000;
        public const int DefaultSThreshold = 16000;

        public static int BThreshold =>
            BalanceOverrides.StageOrDefault.RankBThreshold;

        public static int AThreshold =>
            BalanceOverrides.StageOrDefault.RankAThreshold;

        public static int SThreshold =>
            BalanceOverrides.StageOrDefault.RankSThreshold;

        public static StageRank Resolve(
            int totalScore,
            bool sConditionMet)
        {
            if (totalScore >= SThreshold &&
                sConditionMet)
            {
                return StageRank.S;
            }

            if (totalScore >= AThreshold)
            {
                return StageRank.A;
            }

            if (totalScore >= BThreshold)
            {
                return StageRank.B;
            }

            return StageRank.C;
        }

        /// <summary>
        /// S 랭크 추가 조건이다.
        /// 한 번도 붙잡히지 않고, 고등급(희귀·특수·각성) NPC를 최소 한 명 회수해야 한다.
        /// </summary>
        public static bool IsSConditionMet(
            int captureCount,
            int highGradeRecoveredCount)
        {
            return captureCount <= 0 &&
                   highGradeRecoveredCount >= 1;
        }

        public static string GetLabel(
            StageRank rank)
        {
            switch (rank)
            {
                case StageRank.S:
                    return "S";

                case StageRank.A:
                    return "A";

                case StageRank.B:
                    return "B";

                case StageRank.C:
                default:
                    return "C";
            }
        }
    }
}
