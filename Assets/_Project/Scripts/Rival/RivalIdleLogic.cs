using System;

namespace ProjectTheta.Rival
{
    public static class RivalIdleLogic
    {
        public static float ResolveDuration(
            float minimum,
            float maximum,
            float normalizedRandom)
        {
            float min =
                Math.Max(
                    0f,
                    minimum);

            float max =
                Math.Max(
                    min,
                    maximum);

            float t =
                Math.Max(
                    0f,
                    Math.Min(
                        1f,
                        normalizedRandom));

            return min +
                   ((max - min) * t);
        }
    }
}
