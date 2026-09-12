using NUnit.Framework;
using ProjectTheta.Stage;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class EssenceRecoveryLogicTests
    {
        [Test]
        public void Single_Recovery_Has_No_Bonus()
        {
            Assert.AreEqual(
                1.0f,
                EssenceRecoveryLogic.GetSimultaneousMultiplier(
                    1),
                0.0001f);

            Assert.AreEqual(
                1.0f,
                EssenceRecoveryLogic.GetSimultaneousMultiplier(
                    0),
                0.0001f);
        }

        [Test]
        public void Multiplier_Rises_With_Count_And_Caps_At_Two()
        {
            Assert.AreEqual(1.2f, EssenceRecoveryLogic.GetSimultaneousMultiplier(2), 0.0001f);
            Assert.AreEqual(1.4f, EssenceRecoveryLogic.GetSimultaneousMultiplier(3), 0.0001f);
            Assert.AreEqual(1.7f, EssenceRecoveryLogic.GetSimultaneousMultiplier(4), 0.0001f);
            Assert.AreEqual(2.0f, EssenceRecoveryLogic.GetSimultaneousMultiplier(5), 0.0001f);
            Assert.AreEqual(2.0f, EssenceRecoveryLogic.GetSimultaneousMultiplier(9), 0.0001f);
        }

        [Test]
        public void Batch_Essence_Applies_The_Multiplier()
        {
            // 일반 3명(10+10+10)을 동시에 회수하면 ×1.4
            Assert.AreEqual(
                42,
                EssenceRecoveryLogic.ComputeBatchEssence(
                    30,
                    3));

            // 희귀 1명 + 일반 1명(40+10)을 동시에 회수하면 ×1.2
            Assert.AreEqual(
                60,
                EssenceRecoveryLogic.ComputeBatchEssence(
                    50,
                    2));
        }

        [Test]
        public void Batch_Essence_Handles_Empty_Batch()
        {
            Assert.AreEqual(
                0,
                EssenceRecoveryLogic.ComputeBatchEssence(
                    0,
                    3));
        }

        [Test]
        public void Batch_Window_Closes_At_The_Boundary()
        {
            Assert.IsFalse(
                EssenceRecoveryLogic.IsBatchWindowClosed(
                    1.49f,
                    1.5f));

            Assert.IsTrue(
                EssenceRecoveryLogic.IsBatchWindowClosed(
                    1.5f,
                    1.5f));
        }
    }

    public sealed class ComboLogicTests
    {
        [Test]
        public void Combo_Starts_At_Base_Multiplier()
        {
            Assert.AreEqual(
                1.0f,
                ComboLogic.GetMultiplier(
                    0),
                0.0001f);
        }

        [Test]
        public void Each_Combo_Adds_A_Tenth()
        {
            Assert.AreEqual(1.1f, ComboLogic.GetMultiplier(1), 0.0001f);
            Assert.AreEqual(1.5f, ComboLogic.GetMultiplier(5), 0.0001f);
        }

        [Test]
        public void Multiplier_Caps_At_Three()
        {
            Assert.AreEqual(
                3.0f,
                ComboLogic.GetMultiplier(
                    ComboLogic.MaximumCombo),
                0.0001f);

            Assert.AreEqual(
                3.0f,
                ComboLogic.GetMultiplier(
                    500),
                0.0001f);
        }

        [Test]
        public void Combo_Count_Is_Clamped()
        {
            int combo =
                ComboLogic.MaximumCombo;

            Assert.AreEqual(
                ComboLogic.MaximumCombo,
                ComboLogic.AddCombo(
                    combo));
        }

        [Test]
        public void Combo_Breaks_After_Timeout()
        {
            Assert.AreEqual(
                4,
                ComboLogic.Tick(
                    4,
                    5.9f));

            Assert.AreEqual(
                0,
                ComboLogic.Tick(
                    4,
                    6.0f));
        }

        [Test]
        public void Remaining_Seconds_Counts_Down()
        {
            Assert.AreEqual(
                6.0f,
                ComboLogic.GetRemainingSeconds(
                    1,
                    0f),
                0.0001f);

            Assert.AreEqual(
                2.0f,
                ComboLogic.GetRemainingSeconds(
                    1,
                    4f),
                0.0001f);

            Assert.AreEqual(
                0f,
                ComboLogic.GetRemainingSeconds(
                    0,
                    0f),
                0.0001f);
        }
    }

    public sealed class StageScoreLogicTests
    {
        [Test]
        public void Each_Element_Contributes_Its_Weight()
        {
            StageScoreBreakdown breakdown =
                StageScoreLogic.Compute(
                    100,
                    6,
                    2.0f,
                    3,
                    2,
                    1,
                    30f,
                    0);

            Assert.AreEqual(1000, breakdown.EssenceScore);
            Assert.AreEqual(900, breakdown.FollowerScore);
            Assert.AreEqual(500, breakdown.ComboScore);
            Assert.AreEqual(600, breakdown.ReclaimScore);
            Assert.AreEqual(600, breakdown.DuelScore);
            Assert.AreEqual(250, breakdown.RiskyRecoveryScore);
            Assert.AreEqual(600, breakdown.TimeScore);
            Assert.AreEqual(0, breakdown.CapturePenalty);
            Assert.AreEqual(4450, breakdown.Total);
        }

        [Test]
        public void Capture_Subtracts_From_The_Total()
        {
            StageScoreBreakdown clean =
                StageScoreLogic.Compute(
                    100, 0, 1f, 0, 0, 0, 0f, 0);

            StageScoreBreakdown caught =
                StageScoreLogic.Compute(
                    100, 0, 1f, 0, 0, 0, 0f, 2);

            Assert.AreEqual(
                600,
                caught.CapturePenalty);

            Assert.AreEqual(
                clean.Total - 600,
                caught.Total);
        }

        [Test]
        public void Total_Never_Goes_Negative()
        {
            StageScoreBreakdown breakdown =
                StageScoreLogic.Compute(
                    0, 0, 1f, 0, 0, 0, 0f, 10);

            Assert.AreEqual(
                0,
                breakdown.Total);
        }

        [Test]
        public void Base_Combo_Multiplier_Scores_Nothing()
        {
            StageScoreBreakdown breakdown =
                StageScoreLogic.Compute(
                    0, 0, ComboLogic.MinimumMultiplier, 0, 0, 0, 0f, 0);

            Assert.AreEqual(
                0,
                breakdown.ComboScore);
        }

        [Test]
        public void Negative_Inputs_Are_Treated_As_Zero()
        {
            StageScoreBreakdown breakdown =
                StageScoreLogic.Compute(
                    -50, -3, 0.2f, -1, -1, -1, -10f, -1);

            Assert.AreEqual(
                0,
                breakdown.Total);
        }
    }

    public sealed class StageRankLogicTests
    {
        [Test]
        public void Thresholds_Map_To_Ranks()
        {
            Assert.AreEqual(StageRank.C, StageRankLogic.Resolve(0, false));
            Assert.AreEqual(StageRank.C, StageRankLogic.Resolve(StageRankLogic.BThreshold - 1, false));
            Assert.AreEqual(StageRank.B, StageRankLogic.Resolve(StageRankLogic.BThreshold, false));
            Assert.AreEqual(StageRank.A, StageRankLogic.Resolve(StageRankLogic.AThreshold, false));
        }

        [Test]
        public void High_Score_Alone_Is_Not_Enough_For_S()
        {
            // 점수만으로 S를 주면 일반 NPC를 안전하게 반복 회수하는 것이 최적이 되어버린다.
            Assert.AreEqual(
                StageRank.A,
                StageRankLogic.Resolve(
                    StageRankLogic.SThreshold + 5000,
                    false));

            Assert.AreEqual(
                StageRank.S,
                StageRankLogic.Resolve(
                    StageRankLogic.SThreshold,
                    true));
        }

        [Test]
        public void S_Condition_Requires_No_Capture_And_A_High_Grade_Recovery()
        {
            Assert.IsTrue(
                StageRankLogic.IsSConditionMet(
                    0,
                    1));

            Assert.IsFalse(
                StageRankLogic.IsSConditionMet(
                    1,
                    1));

            Assert.IsFalse(
                StageRankLogic.IsSConditionMet(
                    0,
                    0));
        }

        [Test]
        public void Labels_Are_Single_Letters()
        {
            Assert.AreEqual("S", StageRankLogic.GetLabel(StageRank.S));
            Assert.AreEqual("A", StageRankLogic.GetLabel(StageRank.A));
            Assert.AreEqual("B", StageRankLogic.GetLabel(StageRank.B));
            Assert.AreEqual("C", StageRankLogic.GetLabel(StageRank.C));
        }
    }
}
