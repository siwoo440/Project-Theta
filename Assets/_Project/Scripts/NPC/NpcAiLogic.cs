namespace ProjectTheta.NPC
{
    public static class NpcAiLogic
    {
        public static bool ShouldEnterAlert(
            float distance,
            float alertDistance)
        {
            return distance <= alertDistance;
        }

        public static bool ShouldLeaveAlert(
            float distance,
            float alertExitDistance)
        {
            return distance > alertExitDistance;
        }
    }
}
