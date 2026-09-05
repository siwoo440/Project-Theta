using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    public static class PopularGuyDuelLogic
    {
        public static bool CanStart(
            bool duelLocked,
            float stunRemaining,
            PopularGuyState state,
            PopularGuyTargetMode mode,
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
                case PopularGuyTargetMode.NeutralClaim:
                    return
                        targetOwner ==
                            NpcOwner.Neutral &&
                        (state ==
                             PopularGuyState.Approach ||
                         state ==
                             PopularGuyState.Claiming);

                case PopularGuyTargetMode.Contest:
                    if (!PopularGuyLogic.CanContest(
                            targetOwner))
                    {
                        return false;
                    }

                    return
                        state ==
                            PopularGuyState.Approach ||
                        state ==
                            PopularGuyState.Contest;

                case PopularGuyTargetMode.None:
                default:
                    return false;
            }
        }
    }
}
