using NUnit.Framework;
using ProjectTheta.Ownership;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PopularGuyDuelLogicTests
    {
        [Test]
        public void NeutralClaimApproach_AllowsDuel()
        {
            Assert.IsTrue(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Approach,
                    OpponentTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));
        }

        [Test]
        public void NeutralClaiming_AllowsDuel()
        {
            Assert.IsTrue(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Claiming,
                    OpponentTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));
        }

        [Test]
        public void PlayerContestApproach_AllowsDuel()
        {
            Assert.IsTrue(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Approach,
                    OpponentTargetMode.Contest,
                    true,
                    NpcOwner.Player));
        }

        [Test]
        public void PlayerContest_AllowsDuel()
        {
            Assert.IsTrue(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Contest,
                    OpponentTargetMode.Contest,
                    true,
                    NpcOwner.Player));
        }

        [Test]
        public void GeumtaeyangContest_AllowsDuel()
        {
            Assert.IsTrue(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Contest,
                    OpponentTargetMode.Contest,
                    true,
                    NpcOwner.Geumtaeyang));
        }

        [Test]
        public void IdleOrLockedOrSelfOwned_DoesNotAllowDuel()
        {
            Assert.IsFalse(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Idle,
                    OpponentTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));

            Assert.IsFalse(
                PopularGuyDuelLogic.CanStart(
                    true,
                    0f,
                    OpponentState.Claiming,
                    OpponentTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));

            Assert.IsFalse(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    OpponentState.Contest,
                    OpponentTargetMode.Contest,
                    true,
                    NpcOwner.PopularGuy));
        }
    }
}
