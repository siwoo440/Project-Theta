using NUnit.Framework;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcAiLogicTests
    {
        [Test]
        public void ShouldEnterAlert_WhenInsideDistance_ReturnsTrue()
        {
            Assert.IsTrue(
                NpcAiLogic.ShouldEnterAlert(
                    3.0f,
                    3.2f));
        }

        [Test]
        public void ShouldEnterAlert_WhenOutsideDistance_ReturnsFalse()
        {
            Assert.IsFalse(
                NpcAiLogic.ShouldEnterAlert(
                    3.5f,
                    3.2f));
        }

        [Test]
        public void ShouldLeaveAlert_WhenOutsideExitDistance_ReturnsTrue()
        {
            Assert.IsTrue(
                NpcAiLogic.ShouldLeaveAlert(
                    4.3f,
                    4.2f));
        }
    }
}
