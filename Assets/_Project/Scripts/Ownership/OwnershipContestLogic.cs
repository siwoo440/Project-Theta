using UnityEngine;

namespace ProjectTheta.Ownership
{
    public static class OwnershipContestLogic
    {
        public static float Drain(
            float current,
            float drainPerSecond,
            float deltaTime)
        {
            return Mathf.Max(
                0f,
                current -
                Mathf.Max(
                    0f,
                    drainPerSecond) *
                Mathf.Max(
                    0f,
                    deltaTime));
        }

        public static bool IsDepleted(
            float current)
        {
            return current <= 0f;
        }

        public static bool CanPlayerContest(
            NpcOwner owner)
        {
            return owner !=
                   NpcOwner.Player;
        }

        public static bool CanRivalContest(
            NpcOwner owner)
        {
            return owner ==
                   NpcOwner.Player;
        }
    }
}
