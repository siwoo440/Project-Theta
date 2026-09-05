using NUnit.Framework;
using ProjectTheta.Impulse;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ImpulseLogicTests
    {
        [Test]
        public void Build_ClampsAtMaximum()
        {
            float result =
                ImpulseLogic.Build(
                    98f,
                    100f,
                    10f,
                    1f);

            Assert.AreEqual(
                100f,
                result,
                0.001f);
        }

        [Test]
        public void Build_IncreasesByRateAndDelta()
        {
            float result =
                ImpulseLogic.Build(
                    10f,
                    100f,
                    3f,
                    2f);

            Assert.AreEqual(
                16f,
                result,
                0.001f);
        }

        [Test]
        public void ClassifyBand_BelowWarning_IsCalm()
        {
            Assert.AreEqual(
                ImpulseState.Calm,
                ImpulseLogic.ClassifyBand(
                    30f,
                    65f,
                    85f));
        }

        [Test]
        public void ClassifyBand_AtWarning_IsWarning()
        {
            Assert.AreEqual(
                ImpulseState.Warning,
                ImpulseLogic.ClassifyBand(
                    65f,
                    65f,
                    85f));
        }

        [Test]
        public void ClassifyBand_AtDanger_IsDanger()
        {
            Assert.AreEqual(
                ImpulseState.Danger,
                ImpulseLogic.ClassifyBand(
                    85f,
                    65f,
                    85f));
        }
    }
}
