using UnityEngine;

namespace ProjectTheta.Duel
{
    public static class OpponentDuelDurabilityLogic
    {
        public static int AddLoss(
            int currentLosses,
            int maximumLosses)
        {
            int maximum =
                Mathf.Max(
                    1,
                    maximumLosses);

            return Mathf.Clamp(
                currentLosses + 1,
                0,
                maximum);
        }

        public static bool IsDefeated(
            int currentLosses,
            int maximumLosses)
        {
            return currentLosses >=
                   Mathf.Max(
                       1,
                       maximumLosses);
        }
    }
}
