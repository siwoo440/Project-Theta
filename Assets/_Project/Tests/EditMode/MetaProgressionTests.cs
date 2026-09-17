using NUnit.Framework;
using ProjectTheta.Balance;
using ProjectTheta.Hypnosis;
using ProjectTheta.Items;
using ProjectTheta.NPC;
using ProjectTheta.Save;
using ProjectTheta.Stage;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ContractEssenceLogicTests
    {
        [Test]
        public void Recovered_Essence_Converts_At_Half_Rate()
        {
            // 100 회수 × 0.5 = 50, C 랭크는 보너스 없음, 초과분도 없음
            Assert.AreEqual(
                50,
                ContractEssenceLogic.Compute(
                    100,
                    140,
                    true,
                    "C"));
        }

        [Test]
        public void Rank_Bonus_Is_Added_On_Clear()
        {
            Assert.AreEqual(0, ContractEssenceLogic.GetRankBonus("C"));
            Assert.AreEqual(20, ContractEssenceLogic.GetRankBonus("B"));
            Assert.AreEqual(50, ContractEssenceLogic.GetRankBonus("A"));
            Assert.AreEqual(100, ContractEssenceLogic.GetRankBonus("S"));
            Assert.AreEqual(0, ContractEssenceLogic.GetRankBonus("-"));
        }

        [Test]
        public void Overflow_Is_Rewarded_Separately()
        {
            // 200 회수 / 목표 140 → 기본 100 + A 보너스 50 + 초과 60 × 0.3 = 18
            Assert.AreEqual(
                168,
                ContractEssenceLogic.Compute(
                    200,
                    140,
                    true,
                    "A"));
        }

        [Test]
        public void Failure_Still_Pays_Part_Of_What_Was_Recovered()
        {
            // 기획서 24장: 확정 회수분 일부는 보존해 반복 실패 피로도를 줄인다.
            // 34일차: 실패하면 절반만 받는다. 80 × 0.5 × 0.5 = 20
            Assert.AreEqual(
                20,
                ContractEssenceLogic.Compute(
                    80,
                    140,
                    false,
                    "-"));
        }

        [Test]
        public void Failure_Gets_No_Rank_Bonus_Or_Overflow()
        {
            // 실패했으면 랭크 라벨이 있어도 보너스를 주지 않는다.
            // 34일차: 목표(140)를 넘긴 정기도 인정하지 않는다. 140 × 0.5 × 0.5 = 35
            Assert.AreEqual(
                35,
                ContractEssenceLogic.Compute(
                    200,
                    140,
                    false,
                    "S"));
        }

        [Test]
        public void Negative_Input_Yields_Zero()
        {
            Assert.AreEqual(
                0,
                ContractEssenceLogic.Compute(
                    -50,
                    140,
                    false,
                    "-"));
        }
    }

    public sealed class UpgradeLogicTests
    {
        [Test]
        public void Levels_Are_Clamped_To_Five()
        {
            Assert.AreEqual(0, UpgradeLogic.ClampLevel(-3));
            Assert.AreEqual(5, UpgradeLogic.ClampLevel(99));
            Assert.IsTrue(UpgradeLogic.IsMaxLevel(5));
            Assert.IsFalse(UpgradeLogic.IsMaxLevel(4));
        }

        [Test]
        public void Costs_Are_Progressive()
        {
            Assert.AreEqual(40, UpgradeLogic.GetNextLevelCost(0));
            Assert.AreEqual(70, UpgradeLogic.GetNextLevelCost(1));
            Assert.AreEqual(110, UpgradeLogic.GetNextLevelCost(2));
            Assert.AreEqual(160, UpgradeLogic.GetNextLevelCost(3));
            Assert.AreEqual(220, UpgradeLogic.GetNextLevelCost(4));

            // 만렙에서는 더 살 것이 없다.
            Assert.AreEqual(0, UpgradeLogic.GetNextLevelCost(5));
        }

        [Test]
        public void One_Track_Costs_Six_Hundred_In_Total()
        {
            Assert.AreEqual(
                600,
                UpgradeLogic.TrackTotalCost);
        }

        [Test]
        public void Purchase_Requires_Enough_Essence()
        {
            Assert.IsTrue(
                UpgradeLogic.CanPurchase(
                    0,
                    40));

            Assert.IsFalse(
                UpgradeLogic.CanPurchase(
                    0,
                    39));

            Assert.IsFalse(
                UpgradeLogic.CanPurchase(
                    UpgradeLogic.MaximumLevel,
                    99999));
        }

        [Test]
        public void Remaining_Essence_Is_Unchanged_When_Purchase_Fails()
        {
            Assert.AreEqual(
                39,
                UpgradeLogic.GetRemainingAfterPurchase(
                    0,
                    39));

            Assert.AreEqual(
                10,
                UpgradeLogic.GetRemainingAfterPurchase(
                    0,
                    50));
        }

        [Test]
        public void Each_Track_Improves_With_Level()
        {
            Assert.Greater(
                UpgradeLogic.GetHypnosisSpeedMultiplier(5),
                UpgradeLogic.GetHypnosisSpeedMultiplier(0));

            Assert.Less(
                UpgradeLogic.GetImpulseBuildMultiplier(5),
                UpgradeLogic.GetImpulseBuildMultiplier(0));

            Assert.Less(
                UpgradeLogic.GetDashCostMultiplier(5),
                UpgradeLogic.GetDashCostMultiplier(0));

            Assert.Greater(
                UpgradeLogic.GetMoveSpeedMultiplier(5),
                UpgradeLogic.GetMoveSpeedMultiplier(0));
        }

        [Test]
        public void Control_Track_Raises_The_Limit_From_Four_To_Eight()
        {
            // 기획서 6.2절: 기본 4명, 성장 후 최대 8명.
            Assert.AreEqual(
                4,
                UpgradeLogic.GetStableFollowerLimit(
                    0,
                    4));

            Assert.AreEqual(
                8,
                UpgradeLogic.GetStableFollowerLimit(
                    4,
                    4));

            // 5레벨에서도 한도는 8을 넘지 않는다.
            Assert.AreEqual(
                8,
                UpgradeLogic.GetStableFollowerLimit(
                    5,
                    4));
        }

        [Test]
        public void Multipliers_Never_Reach_Zero()
        {
            Assert.Greater(
                UpgradeLogic.GetImpulseBuildMultiplier(99),
                0f);

            Assert.Greater(
                UpgradeLogic.GetDashCostMultiplier(99),
                0f);
        }
    }

    public sealed class SaveUpgradeTests
    {
        [Test]
        public void Contract_Essence_Accumulates_From_Results()
        {
            SaveData data =
                SaveDataLogic.ApplyStageResult(
                    SaveDataLogic.CreateDefault(),
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 5000,
                        RankLabel = "B",
                        ContractEssence = 128
                    });

            Assert.AreEqual(
                128,
                data.ContractEssence);

            data =
                SaveDataLogic.ApplyStageResult(
                    data,
                    new StageResultSummary
                    {
                        Cleared = false,
                        TotalScore = 100,
                        RankLabel = "-",
                        ContractEssence = 30
                    });

            Assert.AreEqual(
                158,
                data.ContractEssence);
        }

        [Test]
        public void Purchase_Spends_Essence_And_Raises_Level()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            data.ContractEssence = 100;

            Assert.IsTrue(
                SaveDataLogic.TryPurchaseUpgrade(
                    data,
                    UpgradeTrack.Stability));

            Assert.AreEqual(60, data.ContractEssence);

            Assert.AreEqual(
                1,
                SaveDataLogic.GetUpgradeLevel(
                    data,
                    UpgradeTrack.Stability));

            // 다른 계열은 그대로다.
            Assert.AreEqual(
                0,
                SaveDataLogic.GetUpgradeLevel(
                    data,
                    UpgradeTrack.Hypnosis));
        }

        [Test]
        public void Purchase_Fails_Without_Enough_Essence()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            data.ContractEssence = 39;

            Assert.IsFalse(
                SaveDataLogic.TryPurchaseUpgrade(
                    data,
                    UpgradeTrack.Hypnosis));

            Assert.AreEqual(39, data.ContractEssence);

            Assert.AreEqual(
                0,
                SaveDataLogic.GetUpgradeLevel(
                    data,
                    UpgradeTrack.Hypnosis));
        }

        [Test]
        public void Purchase_Stops_At_Max_Level()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            data.ContractEssence = 100000;

            for (int i = 0;
                 i < UpgradeLogic.MaximumLevel;
                 i++)
            {
                Assert.IsTrue(
                    SaveDataLogic.TryPurchaseUpgrade(
                        data,
                        UpgradeTrack.Mobility));
            }

            Assert.IsFalse(
                SaveDataLogic.TryPurchaseUpgrade(
                    data,
                    UpgradeTrack.Mobility));

            Assert.AreEqual(
                UpgradeLogic.MaximumLevel,
                SaveDataLogic.GetUpgradeLevel(
                    data,
                    UpgradeTrack.Mobility));

            // 만렙까지 정확히 계열 총비용만 쓴다.
            Assert.AreEqual(
                100000 - UpgradeLogic.TrackTotalCost,
                data.ContractEssence);
        }

        [Test]
        public void Corrupt_Levels_Are_Clamped_On_Load()
        {
            SaveData data =
                SaveDataLogic.Normalize(
                    new SaveData
                    {
                        UpgradeLevels =
                            new[] { -4, 99, 2, 3 }
                    });

            Assert.AreEqual(0, data.UpgradeLevels[0]);
            Assert.AreEqual(UpgradeLogic.MaximumLevel, data.UpgradeLevels[1]);
            Assert.AreEqual(2, data.UpgradeLevels[2]);
        }
    }

    public sealed class BalanceFallbackTests
    {
        [Test]
        public void Tables_Work_Without_Any_Asset()
        {
            // 자산이 없을 때(=테스트 환경) 코드 기본값이 그대로 쓰여야 한다.
            Assert.IsNull(
                NpcGradeTable.Override);

            Assert.IsFalse(
                BalanceOverrides.HasStageAsset);

            Assert.AreEqual(
                40,
                NpcGradeTable.Get(
                    NpcGrade.Rare).EssenceValue);

            Assert.AreEqual(
                StageRankLogic.DefaultBThreshold,
                StageRankLogic.BThreshold);

            Assert.AreEqual(
                ComboLogic.DefaultTimeoutSeconds,
                ComboLogic.TimeoutSeconds,
                0.0001f);

            Assert.AreEqual(
                HypnosisWaveLogic.DefaultFocusCost,
                HypnosisWaveLogic.FocusCost,
                0.0001f);
        }

        [Test]
        public void Combo_Maximum_Is_Derived_From_Step_And_Cap()
        {
            // 1.0 + 0.1 × 20 = 3.0
            Assert.AreEqual(
                20,
                ComboLogic.MaximumCombo);
        }

        [Test]
        public void Difficulty_Defaults_To_Normal()
        {
            Assert.AreEqual(
                DifficultyLevel.Normal,
                DifficultyTable.Normal.Level);

            Assert.AreEqual(
                1f,
                DifficultyTable.Normal.TargetEssence,
                0.0001f);
        }

        [Test]
        public void Story_Is_Easier_And_Challenge_Is_Harder()
        {
            DifficultyMultipliers story =
                DifficultyTable.Get(
                    DifficultyLevel.Story);

            DifficultyMultipliers challenge =
                DifficultyTable.Get(
                    DifficultyLevel.Challenge);

            Assert.Greater(story.HypnosisSpeed, challenge.HypnosisSpeed);
            Assert.Less(story.ImpulseBuild, challenge.ImpulseBuild);
            Assert.Greater(story.TimeLimit, challenge.TimeLimit);
            Assert.Less(story.TargetEssence, challenge.TargetEssence);
        }

        [Test]
        public void Unknown_Difficulty_Falls_Back_To_Normal()
        {
            Assert.AreEqual(
                DifficultyLevel.Normal,
                DifficultyTable.Get(
                    (DifficultyLevel)999).Level);
        }

        [Test]
        public void Consumable_Table_Works_Without_Asset()
        {
            Assert.IsNull(
                ConsumableItemTable.Override);

            Assert.AreEqual(
                "진정제",
                ConsumableItemTable.Get(
                    ConsumableItem.Sedative).DisplayName);
        }
    }
}
