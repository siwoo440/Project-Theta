using System;
using ProjectTheta.Balance;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 기획서 10.2절 동시 회수 보너스를 계산한다.
    /// 여러 명을 한 번에 회수할수록 배율이 오르지만, 그만큼 오래 데리고 다녀야 하므로
    /// 충동 폭주와 쟁탈 위험이 함께 커진다.
    /// </summary>
    public static class EssenceRecoveryLogic
    {
        public const float DefaultBatchWindowSeconds = 1.5f;

        /// <summary>첫 회수 이후 이 시간 안에 들어온 NPC를 한 묶음으로 정산한다.</summary>
        public static float BatchWindowSeconds =>
            BalanceOverrides.StageOrDefault.RecoveryBatchWindowSeconds;

        public static float GetSimultaneousMultiplier(
            int count)
        {
            if (count <= 1)
            {
                return 1.0f;
            }

            // 배열 인덱스 0이 1명, 1이 2명... 마지막 값이 상한이다.
            return StageBalanceValues.ReadClamped(
                BalanceOverrides.StageOrDefault.
                    SimultaneousMultipliers,
                count - 1,
                2.0f);
        }

        /// <summary>묶음 정기 합계에 동시 회수 배율을 적용한다.</summary>
        public static int ComputeBatchEssence(
            int baseEssenceSum,
            int count)
        {
            int safeSum =
                Math.Max(
                    0,
                    baseEssenceSum);

            if (safeSum == 0)
            {
                return 0;
            }

            double scaled =
                safeSum *
                (double)GetSimultaneousMultiplier(
                    count);

            return Math.Max(
                1,
                (int)Math.Round(
                    scaled,
                    MidpointRounding.AwayFromZero));
        }

        /// <summary>정산 창이 닫혔는지 판정한다.</summary>
        public static bool IsBatchWindowClosed(
            float elapsedSinceFirstRecovery,
            float windowSeconds)
        {
            return elapsedSinceFirstRecovery >=
                   Math.Max(
                       0f,
                       windowSeconds);
        }
    }
}
