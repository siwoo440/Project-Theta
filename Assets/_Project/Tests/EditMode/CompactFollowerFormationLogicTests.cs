using NUnit.Framework;
using ProjectTheta.Companion;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class CompactFollowerFormationLogicTests
    {
        [Test]
        public void CompactFormation_ThreeRows_KeepsTenFollowersWithinFourColumns()
        {
            float distance =
                FollowerFormationLogic.
                    GetCompactHorizontalDistance(
                        9,
                        0.92f,
                        3);

            Assert.AreEqual(
                3.68f,
                distance,
                0.001f);
        }

        [Test]
        public void CompactFormation_ThreeRows_CentersVerticalOffsets()
        {
            Assert.AreEqual(
                -0.48f,
                FollowerFormationLogic.
                    GetCompactVerticalOffset(
                        0,
                        0.48f,
                        3),
                0.001f);

            Assert.AreEqual(
                0f,
                FollowerFormationLogic.
                    GetCompactVerticalOffset(
                        1,
                        0.48f,
                        3),
                0.001f);

            Assert.AreEqual(
                0.48f,
                FollowerFormationLogic.
                    GetCompactVerticalOffset(
                        2,
                        0.48f,
                        3),
                0.001f);
        }
    }
}
