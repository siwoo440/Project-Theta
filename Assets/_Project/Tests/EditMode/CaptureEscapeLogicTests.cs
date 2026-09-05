using NUnit.Framework;
using ProjectTheta.Capture;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class CaptureEscapeLogicTests
    {
        [Test]
        public void IsCorrectInput_MatchingSide_ReturnsTrue()
        {
            Assert.IsTrue(
                CaptureEscapeLogic.IsCorrectInput(
                    CaptureInputSide.Left,
                    CaptureInputSide.Left));
        }

        [Test]
        public void GetNextExpected_LeftBecomesRight()
        {
            Assert.AreEqual(
                CaptureInputSide.Right,
                CaptureEscapeLogic.GetNextExpected(
                    CaptureInputSide.Left));
        }

        [Test]
        public void AddEscapeProgress_ClampsAtMaximum()
        {
            float result =
                CaptureEscapeLogic.AddEscapeProgress(
                    95f,
                    100f,
                    12f);

            Assert.AreEqual(
                100f,
                result,
                0.001f);
        }
    }
}
