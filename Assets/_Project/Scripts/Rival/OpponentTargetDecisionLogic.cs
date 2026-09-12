namespace ProjectTheta.Rival
{
    public enum OpponentTargetDecision
    {
        Search,
        Wait,
        Continue
    }

    public static class OpponentTargetDecisionLogic
    {
        public static OpponentTargetDecision Resolve(
            bool hasTarget,
            bool targetIsValid)
        {
            if (!hasTarget)
            {
                return OpponentTargetDecision.Search;
            }

            if (!targetIsValid)
            {
                return OpponentTargetDecision.Wait;
            }

            return OpponentTargetDecision.Continue;
        }
    }
}
