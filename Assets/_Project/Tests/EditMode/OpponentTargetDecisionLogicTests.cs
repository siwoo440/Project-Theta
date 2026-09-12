using NUnit.Framework;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OpponentTargetDecisionLogicTests
    {
        [Test]
        public void Resolve_NoTarget_SearchesInsteadOfWaitingAgain()
        {
            Assert.AreEqual(
                OpponentTargetDecision.Search,
                OpponentTargetDecisionLogic.Resolve(
                    false,
                    false));
        }

        [Test]
        public void Resolve_InvalidExistingTarget_Waits()
        {
            Assert.AreEqual(
                OpponentTargetDecision.Wait,
                OpponentTargetDecisionLogic.Resolve(
                    true,
                    false));
        }

        [Test]
        public void Resolve_ValidTarget_Continues()
        {
            Assert.AreEqual(
                OpponentTargetDecision.Continue,
                OpponentTargetDecisionLogic.Resolve(
                    true,
                    true));
        }
    }
}
