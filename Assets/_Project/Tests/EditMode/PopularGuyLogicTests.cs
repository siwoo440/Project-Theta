using NUnit.Framework;
using ProjectTheta.Ownership;
using ProjectTheta.Rival;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PopularGuyLogicTests
    {
        [Test]
        public void ScaleTiming_IsOnePointFiveTimesGeumtaeyang()
        {
            Assert.AreEqual(
                0.9f,
                PopularGuyLogic.ScaleTiming(
                    0.6f),
                0.001f);

            Assert.AreEqual(
                2.25f,
                PopularGuyLogic.ScaleTiming(
                    1.5f),
                0.001f);
        }

        [Test]
        public void ScaleContestDrain_MakesContestTakeOnePointFiveTimesLonger()
        {
            Assert.AreEqual(
                12f,
                PopularGuyLogic.ScaleContestDrain(
                    18f),
                0.001f);
        }

        [Test]
        public void CanContest_RejectsNeutralAndOwnFollowers()
        {
            Assert.IsFalse(
                PopularGuyLogic.CanContest(
                    NpcOwner.Neutral));

            Assert.IsFalse(
                PopularGuyLogic.CanContest(
                    NpcOwner.PopularGuy));

            Assert.IsTrue(
                PopularGuyLogic.CanContest(
                    NpcOwner.Player));

            Assert.IsTrue(
                PopularGuyLogic.CanContest(
                    NpcOwner.Geumtaeyang));
        }
        [Test]
        public void PopularGuyRuntimeTuningValues_AreSlower()
        {
            Assert.AreEqual(
                2.1f,
                4.2f * 0.5f,
                0.001f);

            Assert.AreEqual(
                1.8f,
                0.9f * 2f,
                0.001f);

            Assert.AreEqual(
                4.5f,
                2.25f * 2f,
                0.001f);

            Assert.AreEqual(
                0.9f,
                0.45f * 2f,
                0.001f);
        }

    }
}
