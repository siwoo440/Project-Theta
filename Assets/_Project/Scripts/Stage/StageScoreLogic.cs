using System;

namespace ProjectTheta.Stage
{
    /// <summary>점수 항목별 내역이다. 결과 화면이 이 값을 한 줄씩 보여준다.</summary>
    public struct StageScoreBreakdown
    {
        public int EssenceScore;
        public int FollowerScore;
        public int ComboScore;
        public int ReclaimScore;
        public int DuelScore;
        public int RiskyRecoveryScore;
        public int TimeScore;
        public int CapturePenalty;
        public int Total;
    }

    /// <summary>
    /// 기획서 16.1절 점수 요소를 계산한다.
    /// 정기(클리어 조건)와 점수(랭크 평가)를 분리해 "클리어는 했지만 점수는 낮다"가 성립하게 한다.
    /// </summary>
    public static class StageScoreLogic
    {
        public const int EssencePoints = 10;
        public const int FollowerPoints = 150;
        public const int ComboPointScale = 500;
        public const int ReclaimPoints = 200;
        public const int DuelWinPoints = 300;
        public const int RiskyRecoveryPoints = 250;
        public const int RemainingSecondPoints = 20;
        public const int CapturePenaltyPoints = 300;

        public static StageScoreBreakdown Compute(
            int recoveredEssence,
            int maximumFollowers,
            float bestComboMultiplier,
            int reclaimCount,
            int duelWinCount,
            int riskyRecoveryCount,
            float remainingSeconds,
            int captureCount)
        {
            StageScoreBreakdown breakdown =
                new StageScoreBreakdown
                {
                    EssenceScore =
                        Math.Max(
                            0,
                            recoveredEssence) *
                        EssencePoints,

                    FollowerScore =
                        Math.Max(
                            0,
                            maximumFollowers) *
                        FollowerPoints,

                    ComboScore =
                        (int)Math.Round(
                            Math.Max(
                                0f,
                                bestComboMultiplier -
                                ComboLogic.MinimumMultiplier) *
                            ComboPointScale,
                            MidpointRounding.AwayFromZero),

                    ReclaimScore =
                        Math.Max(
                            0,
                            reclaimCount) *
                        ReclaimPoints,

                    DuelScore =
                        Math.Max(
                            0,
                            duelWinCount) *
                        DuelWinPoints,

                    RiskyRecoveryScore =
                        Math.Max(
                            0,
                            riskyRecoveryCount) *
                        RiskyRecoveryPoints,

                    TimeScore =
                        (int)Math.Floor(
                            Math.Max(
                                0f,
                                remainingSeconds)) *
                        RemainingSecondPoints,

                    CapturePenalty =
                        Math.Max(
                            0,
                            captureCount) *
                        CapturePenaltyPoints
                };

            int total =
                breakdown.EssenceScore +
                breakdown.FollowerScore +
                breakdown.ComboScore +
                breakdown.ReclaimScore +
                breakdown.DuelScore +
                breakdown.RiskyRecoveryScore +
                breakdown.TimeScore -
                breakdown.CapturePenalty;

            breakdown.Total =
                Math.Max(
                    0,
                    total);

            return breakdown;
        }
    }
}
