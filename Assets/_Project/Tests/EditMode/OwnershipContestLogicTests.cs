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
        public void PlayerCanContestAllNonPlayerOwners()
        {
            Assert.IsTrue(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Neutral));

            Assert.IsTrue(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Geumtaeyang));

            Assert.IsTrue(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.PopularGuy));

            Assert.IsFalse(
                OwnershipContestLogic.CanPlayerContest(
                    NpcOwner.Player));
        }

        [Test]
        public void GeumtaeyangCanContestOnlyPlayer()
        {
            Assert.IsTrue(
                OwnershipContestLogic.CanGeumtaeyangContest(
                    NpcOwner.Player));

            Assert.IsFalse(
                OwnershipContestLogic.CanGeumtaeyangContest(
                    NpcOwner.Neutral));

            Assert.IsFalse(
                OwnershipContestLogic.CanGeumtaeyangContest(
                    NpcOwner.PopularGuy));
        }

        [Test]
        public void PopularGuyCanContestPlayerAndGeumtaeyang()
        {
            Assert.IsTrue(
                OwnershipContestLogic.CanPopularGuyContest(
                    NpcOwner.Player));

            Assert.IsTrue(
                OwnershipContestLogic.CanPopularGuyContest(
                    NpcOwner.Geumtaeyang));

            Assert.IsFalse(
                OwnershipContestLogic.CanPopularGuyContest(
                    NpcOwner.Neutral));

            Assert.IsFalse(
                OwnershipContestLogic.CanPopularGuyContest(
                    NpcOwner.PopularGuy));
        }
    }
}
