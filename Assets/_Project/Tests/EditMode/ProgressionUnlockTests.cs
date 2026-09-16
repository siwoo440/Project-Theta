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

            Assert.GreaterOrEqual(AchievementLogic.All.Length, 20);

            // 업적 창 두 줄에 들어가야 한다.
            Assert.LessOrEqual(AchievementLogic.All.Length, 24);
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
        public void Toast_Fades_In_Holds_And_Fades_Out()
        {
            Assert.AreEqual(0f, AchievementToast.GetAlpha(0f));
            Assert.AreEqual(0.5f, AchievementToast.GetAlpha(AchievementToast.FadeSeconds * 0.5f), 0.001f);
            Assert.AreEqual(1f, AchievementToast.GetAlpha(AchievementToast.ShowSeconds * 0.5f));
            Assert.AreEqual(0f, AchievementToast.GetAlpha(AchievementToast.ShowSeconds));
        }
    }
}
