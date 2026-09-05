namespace ProjectTheta.Rival
{
    public enum RivalTargetDecision
    {
        Search,
        Wait,
        Continue
    }

    public static class RivalTargetDecisionLogic
    {
        public static RivalTargetDecision Resolve(
            bool hasTarget,
            bool targetIsValid)
        {
            if (!hasTarget)
            {
                return RivalTargetDecision.Search;
            }

            if (!targetIsValid)
            {
                return RivalTargetDecision.Wait;
            }

            return RivalTargetDecision.Continue;
        }
    }
}
