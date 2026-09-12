using System;

namespace ProjectTheta.Stage
{
    public static class StageRules
    {
        public static int AddEssence(
            int current,
            int amount,
            int target)
        {
            int safeCurrent =
                Math.Max(
                    0,
                    current);

            int safeAmount =
                Math.Max(
                    0,
                    amount);

            int safeTarget =
                Math.Max(
                    1,
                    target);

            return Math.Min(
                safeTarget,
                safeCurrent +
                safeAmount);
        }

        public static float TickTime(
            float current,
            float deltaTime)
        {
            float safeCurrent =
                Math.Max(
                    0f,
                    current);

            float safeDelta =
                Math.Max(
                    0f,
                    deltaTime);

            return Math.Max(
                0f,
                safeCurrent -
                safeDelta);
        }

        /// <summary>
        /// 기준 보상에 NPC 등급 정기 배율을 적용한다.
        /// 기준 보상이 0보다 크면 결과는 최소 1을 보장해 고등급이 아닌 NPC도 보상이 사라지지 않는다.
        /// </summary>
        public static int ScaleEssenceReward(
            int baseAmount,
            float valueMultiplier)
        {
            int safeBase =
                Math.Max(
                    0,
                    baseAmount);

            if (safeBase == 0)
            {
                return 0;
            }

            float safeMultiplier =
                Math.Max(
                    0f,
                    valueMultiplier);

            return Math.Max(
                1,
                (int)Math.Round(
                    safeBase *
                    (double)safeMultiplier,
                    MidpointRounding.AwayFromZero));
        }

        public static StageState ResolveState(
            float remainingTime,
            int currentEssence,
            int targetEssence,
            int currentHealth)
        {
            if (currentHealth <= 0)
            {
                return StageState.FailedByHealth;
            }

            if (currentEssence >=
                Math.Max(
                    1,
                    targetEssence))
            {
                return StageState.Cleared;
            }

            if (remainingTime <= 0f)
            {
                return StageState.FailedByTime;
            }

            return StageState.Running;
        }
    }
}
