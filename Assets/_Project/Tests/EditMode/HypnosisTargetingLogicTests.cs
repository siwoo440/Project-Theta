using NUnit.Framework;
using ProjectTheta.Hypnosis;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class HypnosisTargetingLogicTests
    {
        [Test]
        public void IsCandidate_TargetOnRightWithinRange_ReturnsTrue()
        {
            Assert.IsTrue(
                HypnosisTargetingLogic.IsCandidate(
                    3f,
                    0.5f,
                    4.5f,
                    2.4f));
        }

        [Test]
        public void IsCandidate_TargetOnLeftWithinRange_ReturnsTrue()
        {
            Assert.IsTrue(
                HypnosisTargetingLogic.IsCandidate(
                    -2f,
                    0f,
                    4.5f,
                    2.4f));
        }

        [Test]
        public void IsCandidate_TooFarVertically_ReturnsFalse()
        {
            Assert.IsFalse(
                HypnosisTargetingLogic.IsCandidate(
                    2f,
                    3f,
                    4.5f,
                    2.4f));
        }

        [Test]
        public void IsCandidate_OutsideMaximumRange_ReturnsFalse()
        {
            Assert.IsFalse(
                HypnosisTargetingLogic.IsCandidate(
                    5f,
                    0f,
                    4.5f,
                    2.4f));
        }

        [Test]
        public void BuildProgress_ClampsAtMaximum()
        {
            float result =
                HypnosisTargetingLogic.BuildProgress(
                    95f,
                    100f,
                    32f,
                    1f);

            Assert.AreEqual(
                100f,
                result,
                0.001f);
        }
    }
}
