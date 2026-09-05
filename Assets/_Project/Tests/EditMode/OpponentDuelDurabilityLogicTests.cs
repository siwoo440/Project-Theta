using NUnit.Framework;
using ProjectTheta.Duel;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OpponentDuelDurabilityLogicTests
    {
        [Test]
        public void ThirdLoss_DefeatsOpponent()
        {
            int losses = 0;

            losses =
                OpponentDuelDurabilityLogic.AddLoss(
                    losses,
                    3);

            losses =
                OpponentDuelDurabilityLogic.AddLoss(
                    losses,
                    3);

            Assert.IsFalse(
                OpponentDuelDurabilityLogic.IsDefeated(
                    losses,
                    3));

            losses =
                OpponentDuelDurabilityLogic.AddLoss(
                    losses,
                    3);

            Assert.IsTrue(
                OpponentDuelDurabilityLogic.IsDefeated(
                    losses,
                    3));
        }
    }
}
