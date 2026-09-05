using NUnit.Framework;
using ProjectTheta.Duel;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OpponentDuelLogicTests
    {
        [Test]
        public void CorrectAlternatingInput_AddsPlayerPush()
        {
            Assert.IsTrue(
                OpponentDuelLogic.IsCorrectInput(
                    OpponentDuelInputSide.Left,
                    OpponentDuelInputSide.Left));

            float result =
                OpponentDuelLogic.AddPlayerPush(
                    50f,
                    100f,
                    8f);

            Assert.AreEqual(
                58f,
                result,
                0.001f);
        }

        [Test]
        public void SameSideTwice_IsNotCorrectAlternation()
        {
            OpponentDuelInputSide next =
                OpponentDuelLogic.GetNextExpected(
                    OpponentDuelInputSide.Left);

            Assert.AreEqual(
                OpponentDuelInputSide.Right,
                next);

            Assert.IsFalse(
                OpponentDuelLogic.IsCorrectInput(
                    next,
                    OpponentDuelInputSide.Left));
        }

        [Test]
        public void OpponentPressure_PushesGaugeTowardZero()
        {
            float result =
                OpponentDuelLogic.ApplyOpponentPressure(
                    50f,
                    10f,
                    2f);

            Assert.AreEqual(
                30f,
                result,
                0.001f);
        }

        [TestCase(10f)]
        [TestCase(9.9f)]
        [TestCase(0f)]
        public void Resolve_AtOrBelowTenPercent_IsOpponentWin(
            float progress)
        {
            Assert.AreEqual(
                OpponentDuelResult.OpponentWin,
                OpponentDuelLogic.Resolve(
                    progress,
                    100f,
                    0.10f,
                    0.90f));
        }

        [TestCase(90f)]
        [TestCase(90.1f)]
        [TestCase(100f)]
        public void Resolve_AtOrAboveNinetyPercent_IsPlayerWin(
            float progress)
        {
            Assert.AreEqual(
                OpponentDuelResult.PlayerWin,
                OpponentDuelLogic.Resolve(
                    progress,
                    100f,
                    0.10f,
                    0.90f));
        }

        [TestCase(10.1f)]
        [TestCase(50f)]
        [TestCase(89.9f)]
        public void Resolve_BetweenThresholds_ContinuesDuel(
            float progress)
        {
            Assert.AreEqual(
                OpponentDuelResult.None,
                OpponentDuelLogic.Resolve(
                    progress,
                    100f,
                    0.10f,
                    0.90f));
        }
    }
}
