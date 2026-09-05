using NUnit.Framework;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class RivalTargetDecisionLogicTests
    {
        [Test]
        public void Resolve_NoTarget_SearchesInsteadOfWaitingAgain()
        {
            Assert.AreEqual(
                RivalTargetDecision.Search,
                RivalTargetDecisionLogic.Resolve(
                    false,
                    false));
        }

        [Test]
        public void Resolve_InvalidExistingTarget_Waits()
        {
            Assert.AreEqual(
                RivalTargetDecision.Wait,
                RivalTargetDecisionLogic.Resolve(
                    true,
                    false));
        }

        [Test]
        public void Resolve_ValidTarget_Continues()
        {
            Assert.AreEqual(
                RivalTargetDecision.Continue,
                RivalTargetDecisionLogic.Resolve(
                    true,
                    true));
        }
    }
}
