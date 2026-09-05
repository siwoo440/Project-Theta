using NUnit.Framework;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OpponentTargetingLogicTests
    {
        [Test]
        public void GeumtaeyangScore_PrefersFollowerFartherFromPlayerWhenOpponentDistanceMatches()
        {
            float closeToPlayer =
                OpponentTargetingLogic.ScoreGeumtaeyangTarget(
                    3f,
                    1f);

            float farFromPlayer =
                OpponentTargetingLogic.ScoreGeumtaeyangTarget(
                    3f,
                    5f);

            Assert.Greater(
                farFromPlayer,
                closeToPlayer);
        }

        [Test]
        public void ShouldAbandon_WhenTooFar()
        {
            Assert.IsTrue(
                OpponentTargetingLogic.ShouldAbandon(
                    14f,
                    1f,
                    13f,
                    5f));
        }

        [Test]
        public void ShouldAbandon_WhenPursuitTimeExpires()
        {
            Assert.IsTrue(
                OpponentTargetingLogic.ShouldAbandon(
                    5f,
                    5f,
                    13f,
                    5f));
        }
    }
}
