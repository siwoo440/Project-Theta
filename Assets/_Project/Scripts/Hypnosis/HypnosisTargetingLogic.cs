using System;

namespace ProjectTheta.Hypnosis
{
    public static class HypnosisTargetingLogic
    {
        public static bool IsCandidate(
            float deltaX,
            float deltaY,
            float maximumRange,
            float verticalTolerance)
        {
            if (Math.Abs(deltaY) >
                verticalTolerance)
            {
                return false;
            }

            float distanceSquared =
                (deltaX * deltaX) +
                (deltaY * deltaY);

            return distanceSquared <=
                   maximumRange * maximumRange;
        }

        public static float BuildProgress(
            float current,
            float maximum,
            float buildPerSecond,
            float deltaTime)
        {
            float safeMaximum =
                maximum <= 0f
                    ? 1f
                    : maximum;

            float safeCurrent =
                current < 0f
                    ? 0f
                    : current;

            float safeRate =
                buildPerSecond < 0f
                    ? 0f
                    : buildPerSecond;

            float safeDelta =
                deltaTime < 0f
                    ? 0f
                    : deltaTime;

            float next =
                safeCurrent +
                (safeRate * safeDelta);

            return next >
                safeMaximum
                    ? safeMaximum
                    : next;
        }
    }
}
