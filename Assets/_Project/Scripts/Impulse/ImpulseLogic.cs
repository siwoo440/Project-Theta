using UnityEngine;

namespace ProjectTheta.Impulse
{
    public static class ImpulseLogic
    {
        public static float Build(
            float current,
            float maximum,
            float buildPerSecond,
            float deltaTime)
        {
            float safeMaximum =
                Mathf.Max(
                    1f,
                    maximum);

            float safeRate =
                Mathf.Max(
                    0f,
                    buildPerSecond);

            float safeDelta =
                Mathf.Max(
                    0f,
                    deltaTime);

            return Mathf.Clamp(
                current +
                (safeRate * safeDelta),
                0f,
                safeMaximum);
        }

        public static ImpulseState ClassifyBand(
            float current,
            float warningThreshold,
            float dangerThreshold)
        {
            if (current >= dangerThreshold)
            {
                return ImpulseState.Danger;
            }

            if (current >= warningThreshold)
            {
                return ImpulseState.Warning;
            }

            return ImpulseState.Calm;
        }
    }
}
