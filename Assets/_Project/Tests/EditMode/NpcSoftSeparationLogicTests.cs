using NUnit.Framework;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcSoftSeparationLogicTests
    {
        [Test]
        public void ComputeWeight_AtSamePosition_ReturnsOne()
        {
            Assert.AreEqual(
                1f,
                NpcSoftSeparationLogic.ComputeWeight(
                    0f,
                    0.65f),
                0.001f);
        }

        [Test]
        public void ComputeWeight_InsideDistance_ReturnsPartialWeight()
        {
            Assert.AreEqual(
                0.5f,
                NpcSoftSeparationLogic.ComputeWeight(
                    0.325f,
                    0.65f),
                0.001f);
        }

        [Test]
        public void ComputeWeight_AtDesiredDistance_ReturnsZero()
        {
            Assert.AreEqual(
                0f,
                NpcSoftSeparationLogic.ComputeWeight(
                    0.65f,
                    0.65f),
                0.001f);
        }

        [Test]
        public void ComputeWeight_OutsideDesiredDistance_ReturnsZero()
        {
            Assert.AreEqual(
                0f,
                NpcSoftSeparationLogic.ComputeWeight(
                    1.0f,
                    0.65f),
                0.001f);
        }
    }
}
