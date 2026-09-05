using NUnit.Framework;
using ProjectTheta.Ownership;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OwnershipContestLogicTests
    {
        [Test]
        public void Drain_ReducesControlByRateAndTime()
        {
            float result =
                OwnershipContestLogic.Drain(
                    100f,
                    18f,
                    2f);

            Assert.AreEqual(
                64f,
                result,
                0.001f);
        }

        [Test]
        public void Drain_ClampsAtZero()
        {
            float result =
                OwnershipContestLogic.Drain(
                    5f,
                    18f,
                    1f);

            Assert.AreEqual(
                0f,
                result,
                0.001f);
        }

        [Test]
        public void PlayerCanContestNeutralAndRivalButNotPlayer()
        {
            Assert.IsTrue(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Neutral));

            Assert.IsTrue(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Rival));

            Assert.IsFalse(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Player));
        }

        [Test]
        public void RivalCanContestOnlyPlayerOwnedNpc()
        {
            Assert.IsTrue(
                OwnershipContestLogic.CanRivalContest(
                    NpcOwner.Player));

            Assert.IsFalse(
                OwnershipContestLogic.CanRivalContest(
                    NpcOwner.Neutral));

            Assert.IsFalse(
                OwnershipContestLogic.CanRivalContest(
                    NpcOwner.Rival));
        }
    }
}
