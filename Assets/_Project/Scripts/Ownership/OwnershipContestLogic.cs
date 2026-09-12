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

        public static bool CanGeumtaeyangContest(
            NpcOwner owner)
        {
            return owner ==
                   NpcOwner.Player;
        }

        public static bool CanPopularGuyContest(
            NpcOwner owner)
        {
            return owner !=
                   NpcOwner.Neutral &&
                   owner !=
                   NpcOwner.PopularGuy;
        }

        /// <summary>
        /// 공격 주체별 쟁탈 가능 여부를 한 곳에서 판정한다.
        /// 경쟁자가 늘어나면 이 분기만 확장한다.
        /// </summary>
        public static bool CanContest(
            NpcOwner attacker,
            NpcOwner owner)
        {
            switch (attacker)
            {
                case NpcOwner.Player:
                    return CanPlayerContest(
                        owner);

                case NpcOwner.Geumtaeyang:
                    return CanGeumtaeyangContest(
                        owner);

                case NpcOwner.PopularGuy:
                    return CanPopularGuyContest(
                        owner);

                case NpcOwner.Neutral:
                default:
                    return false;
            }
        }
    }
}
