using UnityEngine;

namespace ProjectTheta.Rival
{
    public static class PopularGuyNeutralClaimLogic
    {
        public const int MaximumSteps = 3;

        public static int NextStep(
            int currentStep)
        {
            return Mathf.Clamp(
                currentStep + 1,
                0,
                MaximumSteps);
        }

        public static float Normalized(
            int currentStep)
        {
            return Mathf.Clamp01(
                currentStep /
                (float)MaximumSteps);
        }

        public static bool IsComplete(
            int currentStep)
        {
            return currentStep >=
                   MaximumSteps;
        }
    }
}
