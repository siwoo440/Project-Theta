using NUnit.Framework;
using ProjectTheta.Companion;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class FollowerStabilityLogicTests
    {
        [Test]
        public void Tick_WhenTooFar_Decays()
        {
            float result =
                FollowerStabilityLogic.Tick(
                    100f,
                    100f,
                    10f,
                    9f,
                    35f,
                    20f,
                    1f);

            Assert.AreEqual(
                65f,
                result,
                0.001f);
        }

        [Test]
        public void Tick_WhenInRange_Recovers()
        {
            float result =
                FollowerStabilityLogic.Tick(
                    60f,
                    100f,
                    4f,
                    9f,
                    35f,
                    20f,
                    1f);

            Assert.AreEqual(
                80f,
                result,
                0.001f);
        }

        [Test]
        public void Tick_ClampsToValidRange()
        {
            float recovered =
                FollowerStabilityLogic.Tick(
                    95f,
                    100f,
                    1f,
                    9f,
                    35f,
                    20f,
                    1f);

            float decayed =
                FollowerStabilityLogic.Tick(
                    10f,
                    100f,
                    20f,
                    9f,
                    35f,
                    20f,
                    1f);

            Assert.AreEqual(
                100f,
                recovered,
                0.001f);

            Assert.AreEqual(
                0f,
                decayed,
                0.001f);
        }
    }
}
