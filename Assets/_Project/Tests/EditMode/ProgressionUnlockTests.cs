using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Run;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class MasteryLogicTests
    {
        [Test]
        public void Stars_Rise_At_One_Three_Six_Clears()
        {
            Assert.AreEqual(0, MasteryLogic.GetStars(0));
            Assert.AreEqual(1, MasteryLogic.GetStars(1));
            Assert.AreEqual(1, MasteryLogic.GetStars(2));
            Assert.AreEqual(2, MasteryLogic.GetStars(3));
            Assert.AreEqual(3, MasteryLogic.GetStars(6));
            Assert.AreEqual(3, MasteryLogic.GetStars(99));
        }

        [Test]
        public void Clears_To_Next_Star()
        {
            Assert.AreEqual(1, MasteryLogic.GetClearsToNextStar(0));
            Assert.AreEqual(2, MasteryLogic.GetClearsToNextStar(1));
            Assert.AreEqual(1, MasteryLogic.GetClearsToNextStar(5));
            Assert.AreEqual(0, MasteryLogic.GetClearsToNextStar(6));
        }

        [Test]
        public void Stars_Raise_Target_And_Reward()
        {
            Assert.AreEqual(200, MasteryLogic.GetTargetEssence(200, 0));
            Assert.AreEqual(210, MasteryLogic.GetTargetEssence(200, 1));
            Assert.AreEqual(230, MasteryLogic.GetTargetEssence(200, 3));
            Assert.AreEqual(230, MasteryLogic.GetTargetEssence(200, 9));

            Assert.AreEqual(100, MasteryLogic.GetContractEssence(100, 0));
            Assert.AreEqual(130, MasteryLogic.GetContractEssence(100, 3));
            Assert.AreEqual(0, MasteryLogic.GetContractEssence(-5, 3));

            // 보상 증가폭이 목표 증가폭보다 커야 숙련할 이유가 생긴다.
            Assert.Greater(MasteryLogic.RewardBonusPerStar, MasteryLogic.TargetBonusPerStar);
        }

        [Test]
        public void Star_Text()
        {
            Assert.AreEqual("☆☆☆", MasteryLogic.FormatStars(0));
            Assert.AreEqual("★★☆", MasteryLogic.FormatStars(2));
            Assert.AreEqual("★★★", MasteryLogic.FormatStars(5));
        }

        [Test]
        public void Ending_Unlocks_Four_Start_Cards()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.IsFalse(MasteryLogic.HasEndingUnlock(save));
            Assert.AreEqual(3, MasteryLogic.GetStartChoiceCount(save));
            Assert.AreEqual(3, MasteryLogic.GetStartChoiceCount(null));

            save.Stats.Endings = 1;

            Assert.IsTrue(MasteryLogic.HasEndingUnlock(save));
            Assert.AreEqual(4, MasteryLogic.GetStartChoiceCount(save));
        }

        [Test]
        public void Four_Start_Cards_Come_From_Different_Categories()
        {
            List<RunUpgradeCard> cards =
                RunUpgradeDrawLogic.Draw(
                    new RunUpgradeState(),
                    new System.Random(3),
                    MasteryLogic.EndingStartChoices);

            Assert.AreEqual(4, cards.Count);

            HashSet<RunUpgradeCategory> categories = new HashSet<RunUpgradeCategory>();

            foreach (RunUpgradeCard card in cards)
            {
                categories.Add(RunUpgradeTable.Get(card).Category);
            }

            Assert.AreEqual(4, categories.Count);
        }

        [Test]
        public void Stars_Come_From_The_Save()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            for (int i = 0; i < 3; i++)
            {
                save = SaveDataLogic.ApplyStageResult(
                    save,
                    new StageResultSummary
                    {
                        HasLocation = true,
                        LocationId = (int)LocationId.Beach,
                        Cleared = true,
                        PlaySeconds = 100f,
                        RankLabel = "B"
                    });
            }

            Assert.AreEqual(2, MasteryLogic.GetStars(save, (int)LocationId.Beach));
            Assert.AreEqual(0, MasteryLogic.GetStars(save, (int)LocationId.OfficeTower));
            Assert.AreEqual("★★☆", PlayStatsLogic.BuildLocationRow(PlayStatsLogic.Get(save, (int)LocationId.Beach))[5]);
        }
    }

    public sealed class AchievementLogicTests
    {
        private static StageResultSummary Clear(
            LocationId location,
            string rank = "B",
            int hypnosis = 0)
        {
            return new StageResultSummary
            {
                HasLocation = true,
                LocationId = (int)location,
                Cleared = true,
                PlaySeconds = 120f,
                RankLabel = rank,
                RecoveredEssence = 100,
                HypnosisCount = hypnosis
            };
        }

        [Test]
        public void Ids_Are_Unique_And_Definitions_Valid()
        {
            HashSet<string> ids = new HashSet<string>();

            foreach (AchievementDefinition definition in AchievementLogic.All)
            {
                Assert.IsTrue(ids.Add(definition.Id), definition.Id);
                Assert.IsFalse(string.IsNullOrEmpty(definition.Name));
                Assert.IsFalse(string.IsNullOrEmpty(definition.Description));
                Assert.Greater(definition.Target, 0, definition.Id);
                Assert.Greater(definition.Reward, 0, definition.Id);
            }

            // 32일차: 23개 → 46개. 36일차: 심야 모드 3개 → 49개.
            // (32일차부터 업적 창은 한 줄씩 스크롤이라 칸 수 제한은 없다.)
            Assert.AreEqual(49, AchievementLogic.All.Length);

            // 장소를 지정하는 업적만 장소 번호가 있다.
            foreach (AchievementDefinition definition in AchievementLogic.All)
            {
                Assert.AreEqual(
                    definition.Stat == AchievementStat.LocationSRank,
                    definition.Location >= 0,
                    definition.Id);
            }
        }

        [Test]
        public void First_Clear_Unlocks_And_Pays_Once()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.TrainingCenter));

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "first_step"));
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "first_clear"));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "clear_10"));

            int essence = save.ContractEssence;

            save = SaveDataLogic.ApplyStageResult(save, new StageResultSummary { HasLocation = true, LocationId = 0 });

            // 이미 받은 보상은 다시 주지 않는다(결과 자체의 계약 정기는 0).
            Assert.AreEqual(essence, save.ContractEssence);
        }

        [Test]
        public void Reward_Is_Added_To_Contract_Essence()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            List<AchievementDefinition> unlocked;

            save.Stats.Attempts = 1;
            unlocked = AchievementLogic.UnlockNew(save);

            Assert.AreEqual(1, unlocked.Count);
            Assert.AreEqual(AchievementLogic.Get("first_step").Reward, save.ContractEssence);
            Assert.IsEmpty(AchievementLogic.UnlockNew(save));
        }

        [Test]
        public void Location_Based_Achievements()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                save = SaveDataLogic.ApplyStageResult(save, Clear(location.Id));
            }

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "tour_4"));
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "tour_8"));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "mastery_1"));

            for (int i = 0; i < 5; i++)
            {
                save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.Beach, "S"));
            }

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "mastery_1"));
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "s_rank_5"));
            Assert.AreEqual(5, save.Stats.SRanks);
            Assert.AreEqual(1, AchievementLogic.GetValue(save, AchievementStat.MasteredLocations));
        }

        [Test]
        public void Progress_Is_Clamped()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            AchievementDefinition hypnosis = AchievementLogic.Get("hypnosis_50");

            save.Stats.Hypnosis = 25;
            Assert.AreEqual(0.5f, AchievementLogic.GetProgress(save, hypnosis), 0.001f);

            save.Stats.Hypnosis = 500;
            Assert.AreEqual(1f, AchievementLogic.GetProgress(save, hypnosis), 0.001f);

            save.Stats.TotalSeconds = 3599f;
            Assert.AreEqual(59, AchievementLogic.GetValue(save, AchievementStat.PlayMinutes));
        }

        [Test]
        public void Diff_Finds_Only_New_Ids()
        {
            List<AchievementDefinition> diff =
                AchievementLogic.Diff(
                    new[] { "first_step" },
                    new[] { "first_step", "first_clear", "unknown" });

            Assert.AreEqual(1, diff.Count);
            Assert.AreEqual("first_clear", diff[0].Id);
            Assert.IsEmpty(AchievementLogic.Diff(null, null));
        }

        [Test]
        public void Old_Saves_And_Bad_Ids_Are_Cleaned()
        {
            SaveData save = new SaveData { UnlockedAchievements = null };

            SaveDataLogic.Normalize(save);
            Assert.IsNotNull(save.UnlockedAchievements);
            Assert.IsEmpty(save.UnlockedAchievements);

            save.UnlockedAchievements = new[] { "first_step", "first_step", "removed_id", null };
            SaveDataLogic.Normalize(save);

            CollectionAssert.AreEqual(new[] { "first_step" }, save.UnlockedAchievements);
            Assert.AreEqual(1, AchievementLogic.CountUnlocked(save));
        }

        [Test]
        public void Clone_Copies_Achievements()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.UnlockedAchievements = new[] { "first_step" };

            SaveData copy = save.Clone();
            copy.UnlockedAchievements[0] = "changed";

            Assert.AreEqual("first_step", save.UnlockedAchievements[0]);
        }

        [Test]
        public void Location_S_Rank_Achievements_Need_That_Place()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.Beach, "S"));

            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "s_training"));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "s_club"));

            save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.TrainingCenter, "S"));

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "s_training"));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "s_club"));

            save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.RooftopClub, "S"));

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "s_club"));
            Assert.AreEqual(3, AchievementLogic.GetValue(save, AchievementStat.SRankLocations));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "s_all"));

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                save = SaveDataLogic.ApplyStageResult(save, Clear(location.Id, "S"));
            }

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "s_all"));
        }

        [Test]
        public void Fast_Clear_Needs_Ninety_Seconds_Or_Less()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            StageResultSummary slow = Clear(LocationId.OfficeTower);
            slow.PlaySeconds = 91f;
            save = SaveDataLogic.ApplyStageResult(save, slow);

            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "fast_clear"));

            StageResultSummary fast = Clear(LocationId.OfficeTower);
            fast.PlaySeconds = AchievementLogic.FastClearSeconds;
            save = SaveDataLogic.ApplyStageResult(save, fast);

            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "fast_clear"));
        }

        [Test]
        public void Clean_Clears_Skip_Stolen_Or_Failed_Attempts()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            StageResultSummary stolen = Clear(LocationId.Beach);
            stolen.StolenCount = 2;

            StageResultSummary failed = Clear(LocationId.Beach);
            failed.Cleared = false;

            save = SaveDataLogic.ApplyStageResult(save, stolen);
            save = SaveDataLogic.ApplyStageResult(save, failed);

            Assert.AreEqual(0, save.Stats.CleanClears);

            for (int i = 0; i < 10; i++)
            {
                save = SaveDataLogic.ApplyStageResult(save, Clear(LocationId.Beach));
            }

            Assert.AreEqual(10, save.Stats.CleanClears);
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "clean_10"));
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "clear_10"));
        }

        [Test]
        public void New_Stat_Achievements_Read_The_Right_Numbers()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            save.Stats.RecoveredFollowers = 500;
            save.Stats.ContractEssence = 4999;
            save.Stats.Captures = 20;
            save.Stats.LevelUps = 100;

            List<AchievementDefinition> unlocked = AchievementLogic.UnlockNew(save);
            List<string> ids = unlocked.ConvertAll(d => d.Id);

            Assert.Contains("recover_500", ids);
            Assert.Contains("captured_20", ids);
            Assert.Contains("levelups_100", ids);
            CollectionAssert.DoesNotContain(ids, "contract_5000");

            Assert.AreEqual(4999f / 5000f, AchievementLogic.GetProgress(save, AchievementLogic.Get("contract_5000")), 0.0001f);
        }

        [Test]
        public void Total_Rewards_Match_The_Plan()
        {
            int total = 0;

            foreach (AchievementDefinition definition in AchievementLogic.All)
            {
                total += definition.Reward;
            }

            // 30일차 23개 2,690 + 32일차 23개 6,000.
            // 36일차: 심야 업적 150 + 300 + 600
            Assert.AreEqual(9740, total);
        }

        [Test]
        public void Toast_Fades_In_Holds_And_Fades_Out()
        {
            Assert.AreEqual(0f, AchievementToast.GetAlpha(0f));
            Assert.AreEqual(0.5f, AchievementToast.GetAlpha(AchievementToast.FadeSeconds * 0.5f), 0.001f);
            Assert.AreEqual(1f, AchievementToast.GetAlpha(AchievementToast.ShowSeconds * 0.5f));
            Assert.AreEqual(0f, AchievementToast.GetAlpha(AchievementToast.ShowSeconds));
        }
    }
}
