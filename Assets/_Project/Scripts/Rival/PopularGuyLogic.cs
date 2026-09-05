using System;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    public static class PopularGuyLogic
    {
        public const float TimingMultiplier = 1.5f;

        public static float ScaleTiming(
            float baseSeconds)
        {
            return Math.Max(
                       0f,
                       baseSeconds) *
                   TimingMultiplier;
        }

        public static float ScaleContestDrain(
            float baseDrainPerSecond)
        {
            return Math.Max(
                       0f,
                       baseDrainPerSecond) /
                   TimingMultiplier;
        }

        public static bool CanContest(
            NpcOwner owner)
        {
            return OwnershipContestLogic.
                CanPopularGuyContest(
                    owner);
        }
    }
}
