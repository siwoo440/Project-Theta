using NUnit.Framework;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class RivalIdleLogicTests
    {
        [Test]
        public void ResolveDuration_ZeroRandom_ReturnsMinimum()
        {
            Assert.AreEqual(
                1.2f,
                RivalIdleLogic.ResolveDuration(
                    1.2f,
                    3.0f,
                    0f),
                0.001f);
        }

        [Test]
        public void ResolveDuration_OneRandom_ReturnsMaximum()
        {
            Assert.AreEqual(
                3.0f,
                RivalIdleLogic.ResolveDuration(
                    1.2f,
                    3.0f,
                    1f),
                0.001f);
        }

        [Test]
        public void ResolveDuration_ClampsRandomInput()
        {
            Assert.AreEqual(
                3.0f,
                RivalIdleLogic.ResolveDuration(
                    1.2f,
                    3.0f,
                    4f),
                0.001f);
        }
    }
}
