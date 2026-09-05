using System;

namespace ProjectTheta.Rival
{
    public static class OpponentTargetingLogic
    {
        public static float ScoreGeumtaeyangTarget(
            float opponentDistance,
            float playerDistance)
        {
            float safeOpponentDistance =
                Math.Max(
                    0f,
                    opponentDistance);

            float safePlayerDistance =
                Math.Max(
                    0f,
                    playerDistance);

            return
                (safePlayerDistance * 0.65f) -
                safeOpponentDistance;
        }

        public static bool ShouldAbandon(
            float targetDistance,
            float pursuitElapsed,
            float maximumDistance,
            float maximumPursuitDuration)
        {
            return targetDistance >
                   Math.Max(
                       0f,
                       maximumDistance) ||
                   pursuitElapsed >=
                   Math.Max(
                       0f,
                       maximumPursuitDuration);
        }
    }
}
