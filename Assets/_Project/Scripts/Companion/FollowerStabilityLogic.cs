using System;

namespace ProjectTheta.Companion
{
    public static class FollowerStabilityLogic
    {
        public static float Tick(
            float current,
            float maximum,
            float leaderDistance,
            float breakDistance,
            float decayPerSecond,
            float recoveryPerSecond,
            float deltaTime)
        {
            float safeMaximum =
                Math.Max(1f, maximum);

            float safeCurrent =
                Math.Clamp(
                    current,
                    0f,
                    safeMaximum);

            float safeDelta =
                Math.Max(0f, deltaTime);

            bool tooFar =
                leaderDistance >
                Math.Max(0f, breakDistance);

            float rate =
                tooFar
                    ? -Math.Max(
                        0f,
                        decayPerSecond)
                    : Math.Max(
                        0f,
                        recoveryPerSecond);

            return Math.Clamp(
                safeCurrent +
                (rate * safeDelta),
                0f,
                safeMaximum);
        }
    }
}
