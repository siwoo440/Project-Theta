using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    public static class PopularGuyDuelLogic
    {
        public static bool CanStart(
            bool duelLocked,
            float stunRemaining,
            OpponentState state,
            OpponentTargetMode mode,
            bool hasTarget,
            NpcOwner targetOwner)
        {
            if (duelLocked ||
                stunRemaining >
                    0f ||
                !hasTarget)
            {
                return false;
            }

            switch (mode)
            {
                case OpponentTargetMode.NeutralClaim:
                    return
                        targetOwner ==
                            NpcOwner.Neutral &&
                        (state ==
                             OpponentState.Approach ||
                         state ==
                             OpponentState.Claiming);

                case OpponentTargetMode.Contest:
                    if (!PopularGuyLogic.CanContest(
                            targetOwner))
                    {
                        return false;
                    }

                    return
                        state ==
                            OpponentState.Approach ||
                        state ==
                            OpponentState.Contest;

                case OpponentTargetMode.None:
                default:
                    return false;
            }
        }
    }
}
