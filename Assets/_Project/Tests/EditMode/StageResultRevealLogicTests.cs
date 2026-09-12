using NUnit.Framework;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class StageResultRevealLogicTests
    {
        private const int Rows = 7;
        private const float Interval = 0.26f;
        private const float Pop = 0.18f;
        private const float RankDelay = 0.40f;
        private const float RankDuration = 0.26f;

        [Test]
        public void Rows_Appear_One_By_One()
        {
            Assert.AreEqual(
                1,
                StageResultRevealLogic.GetVisibleRowCount(
                    0f,
                    Rows,
                    Interval));

            Assert.AreEqual(
                2,
                StageResultRevealLogic.GetVisibleRowCount(
                    Interval,
                    Rows,
                    Interval));

            Assert.AreEqual(
                4,
                StageResultRevealLogic.GetVisibleRowCount(
                    Interval * 3.5f,
                    Rows,
                    Interval));
        }

        [Test]
        public void Visible_Row_Count_Never_Exceeds_Total()
        {
            Assert.AreEqual(
                Rows,
                StageResultRevealLogic.GetVisibleRowCount(
                    100f,
                    Rows,
                    Interval));
        }

        [Test]
        public void Row_Is_Hidden_Before_Its_Turn()
        {
            Assert.AreEqual(
                0f,
                StageResultRevealLogic.GetRowScale(
                    3,
                    Interval * 2f,
                    Interval,
                    Pop),
                0.0001f);

            Assert.IsFalse(
                StageResultRevealLogic.IsRowVisible(
                    3,
                    Interval * 2f,
                    Interval));
        }

        [Test]
        public void Row_Pops_Big_Then_Settles_To_One()
        {
            float appearTime =
                StageResultRevealLogic.GetRowAppearTime(
                    2,
                    Interval);

            float atAppear =
                StageResultRevealLogic.GetRowScale(
                    2,
                    appearTime,
                    Interval,
                    Pop);

            float midway =
                StageResultRevealLogic.GetRowScale(
                    2,
                    appearTime + (Pop * 0.5f),
                    Interval,
                    Pop);

            float settled =
                StageResultRevealLogic.GetRowScale(
                    2,
                    appearTime + Pop,
                    Interval,
                    Pop);

            Assert.AreEqual(
                1f + StageResultRevealLogic.RowPopOvershoot,
                atAppear,
                0.0001f);

            Assert.Less(
                midway,
                atAppear);

            Assert.Greater(
                midway,
                settled);

            Assert.AreEqual(
                1f,
                settled,
                0.0001f);
        }

        [Test]
        public void Rank_Waits_Until_All_Rows_Are_Out()
        {
            float lastRowTime =
                StageResultRevealLogic.GetRowAppearTime(
                    Rows - 1,
                    Interval);

            Assert.AreEqual(
                0f,
                StageResultRevealLogic.GetRankProgress(
                    lastRowTime,
                    Rows,
                    Interval,
                    RankDelay,
                    RankDuration),
                0.0001f);

            Assert.Greater(
                StageResultRevealLogic.GetRankProgress(
                    lastRowTime + RankDelay + 0.01f,
                    Rows,
                    Interval,
                    RankDelay,
                    RankDuration),
                0f);
        }

        [Test]
        public void Rank_Stamps_Down_From_A_Large_Scale()
        {
            float atStart =
                StageResultRevealLogic.GetRankScale(
                    0f);

            float atEnd =
                StageResultRevealLogic.GetRankScale(
                    1f);

            Assert.AreEqual(
                1f + StageResultRevealLogic.RankStampOvershoot,
                atStart,
                0.0001f);

            Assert.AreEqual(
                1f,
                atEnd,
                0.0001f);

            Assert.Greater(
                atStart,
                StageResultRevealLogic.GetRankScale(
                    0.5f));
        }

        [Test]
        public void Rank_Fades_In_Quickly()
        {
            Assert.AreEqual(
                0f,
                StageResultRevealLogic.GetRankAlpha(
                    0f),
                0.0001f);

            Assert.AreEqual(
                1f,
                StageResultRevealLogic.GetRankAlpha(
                    0.4f),
                0.0001f);

            Assert.AreEqual(
                1f,
                StageResultRevealLogic.GetRankAlpha(
                    1f),
                0.0001f);
        }

        [Test]
        public void Skipping_To_Total_Duration_Shows_Everything_Final()
        {
            // 클릭 스킵은 경과 시간을 전체 길이로 바꾸는 것으로 구현한다.
            float total =
                StageResultRevealLogic.GetTotalDuration(
                    Rows,
                    Interval,
                    RankDelay,
                    RankDuration);

            Assert.AreEqual(
                Rows,
                StageResultRevealLogic.GetVisibleRowCount(
                    total,
                    Rows,
                    Interval));

            for (int i = 0;
                 i < Rows;
                 i++)
            {
                Assert.AreEqual(
                    1f,
                    StageResultRevealLogic.GetRowScale(
                        i,
                        total,
                        Interval,
                        Pop),
                    0.0001f,
                    $"row {i}");
            }

            Assert.AreEqual(
                1f,
                StageResultRevealLogic.GetRankScale(
                    StageResultRevealLogic.GetRankProgress(
                        total,
                        Rows,
                        Interval,
                        RankDelay,
                        RankDuration)),
                0.0001f);

            Assert.IsTrue(
                StageResultRevealLogic.IsComplete(
                    total,
                    Rows,
                    Interval,
                    RankDelay,
                    RankDuration));
        }

        [Test]
        public void Is_Not_Complete_Midway()
        {
            Assert.IsFalse(
                StageResultRevealLogic.IsComplete(
                    Interval * 2f,
                    Rows,
                    Interval,
                    RankDelay,
                    RankDuration));
        }

        [Test]
        public void Negative_Elapsed_Shows_Nothing()
        {
            Assert.AreEqual(
                0,
                StageResultRevealLogic.GetVisibleRowCount(
                    -1f,
                    Rows,
                    Interval));

            Assert.AreEqual(
                0f,
                StageResultRevealLogic.GetRowScale(
                    0,
                    -1f,
                    Interval,
                    Pop),
                0.0001f);
        }
    }
}
