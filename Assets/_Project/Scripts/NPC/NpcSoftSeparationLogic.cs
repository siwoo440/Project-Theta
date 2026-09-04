using System;

namespace ProjectTheta.NPC
{
    public static class NpcSoftSeparationLogic
    {
        public static float ComputeWeight(
            float distance,
            float desiredDistance)
        {
            float safeDesired =
                Math.Max(
                    0.001f,
                    desiredDistance);

            if (distance >= safeDesired)
            {
                return 0f;
            }

            float safeDistance =
                Math.Max(
                    0f,
                    distance);

            return 1f -
                   (safeDistance /
                    safeDesired);
        }
    }
}
