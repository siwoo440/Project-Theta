using NUnit.Framework;
using ProjectTheta.Companion;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class FollowerFormationLogicTests
    {
        [Test]
        public void GetHorizontalDistance_FirstTwoSlotsShareFirstColumn()
        {
            Assert.AreEqual(
                1.0f,
                FollowerFormationLogic.GetHorizontalDistance(0, 1.0f),
                0.001f);

            Assert.AreEqual(
                1.0f,
                FollowerFormationLogic.GetHorizontalDistance(1, 1.0f),
                0.001f);
        }

        [Test]
        public void GetHorizontalDistance_ThirdSlotUsesSecondColumn()
        {
            Assert.AreEqual(
                2.0f,
                FollowerFormationLogic.GetHorizontalDistance(2, 1.0f),
                0.001f);
        }

        [Test]
        public void GetVerticalOffset_AlternatesRows()
        {
            Assert.AreEqual(
                -0.25f,
                FollowerFormationLogic.GetVerticalOffset(0, 0.5f),
                0.001f);

            Assert.AreEqual(
                0.25f,
                FollowerFormationLogic.GetVerticalOffset(1, 0.5f),
                0.001f);
        }
    }
}
