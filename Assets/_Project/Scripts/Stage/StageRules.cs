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

        public static int ComputeProductionPerSecond(
            int followerCount,
            int essencePerFollower)
        {
            return Math.Max(
                       0,
                       followerCount) *
                   Math.Max(
                       0,
                       essencePerFollower);
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
