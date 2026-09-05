using NUnit.Framework;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PopularGuyNeutralClaimLogicTests
    {
        [Test]
        public void ClaimProgress_FillsInThreeDiscreteSteps()
        {
            int step = 0;

            step =
                PopularGuyNeutralClaimLogic.NextStep(
                    step);

            Assert.AreEqual(
                1f / 3f,
                PopularGuyNeutralClaimLogic.Normalized(
                    step),
                0.001f);

            step =
                PopularGuyNeutralClaimLogic.NextStep(
                    step);

            Assert.AreEqual(
                2f / 3f,
                PopularGuyNeutralClaimLogic.Normalized(
                    step),
                0.001f);

            step =
                PopularGuyNeutralClaimLogic.NextStep(
                    step);

            Assert.AreEqual(
                1f,
                PopularGuyNeutralClaimLogic.Normalized(
                    step),
                0.001f);

            Assert.IsTrue(
                PopularGuyNeutralClaimLogic.IsComplete(
                    step));
        }

        [Test]
        public void ClaimStep_ClampsAtThree()
        {
            Assert.AreEqual(
                3,
                PopularGuyNeutralClaimLogic.NextStep(
                    3));
        }
    }
}
