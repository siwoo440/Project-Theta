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
                    PopularGuyState.Approach,
                    PopularGuyTargetMode.NeutralClaim,
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
                    PopularGuyState.Claiming,
                    PopularGuyTargetMode.NeutralClaim,
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
                    PopularGuyState.Approach,
                    PopularGuyTargetMode.Contest,
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
                    PopularGuyState.Contest,
                    PopularGuyTargetMode.Contest,
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
                    PopularGuyState.Contest,
                    PopularGuyTargetMode.Contest,
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
                    PopularGuyState.Idle,
                    PopularGuyTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));

            Assert.IsFalse(
                PopularGuyDuelLogic.CanStart(
                    true,
                    0f,
                    PopularGuyState.Claiming,
                    PopularGuyTargetMode.NeutralClaim,
                    true,
                    NpcOwner.Neutral));

            Assert.IsFalse(
                PopularGuyDuelLogic.CanStart(
                    false,
                    0f,
                    PopularGuyState.Contest,
                    PopularGuyTargetMode.Contest,
                    true,
                    NpcOwner.PopularGuy));
        }
    }
}
