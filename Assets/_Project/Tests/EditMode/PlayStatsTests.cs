using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Boss;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PlayStatsLogicTests
    {
        private static StageResultSummary Result(
            LocationId location,
            bool cleared,
            float seconds,
            int essence,
            string rank = "B")
        {
            return new StageResultSummary
            {
                HasLocation = true,
                LocationId = (int)location,
                Cleared = cleared,
                PlaySeconds = seconds,
                RecoveredEssence = essence,
                ContractEssence = essence / 10,
                RankLabel = cleared ? rank : "-",
                HypnosisCount = 5,
                MaxFollowers = 3,
                RecoveredFollowers = 4,
                StolenCount = 1,
                ReclaimCount = 1,
                RampageWindups = 2,
                RampageSurvived = 1,
                CaptureCount = 1,
                DuelWins = 1,
                LevelUps = 2,
                CardsPicked = 3
            };
        }

        [Test]
        public void Old_Saves_Get_Empty_Stats()
        {
            SaveData old = new SaveData { Stats = null, LocationRecords = null };

            SaveData normalized = SaveDataLogic.Normalize(old);

            Assert.IsNotNull(normalized.Stats);
            Assert.IsNotNull(normalized.LocationRecords);
            Assert.AreEqual(0, normalized.Stats.Attempts);
        }

        [Test]
        public void Old_Json_Without_Stats_Loads()
        {
            SaveData loaded =
                SaveDataLogic.Normalize(
                    JsonUtility.FromJson<SaveData>("{\"Version\":1,\"ClearCount\":2,\"PlayCount\":3}"));

            Assert.AreEqual(2, loaded.ClearCount);
            Assert.IsNotNull(loaded.Stats);
            Assert.AreEqual(0, loaded.LocationRecords.Length);
        }

        [Test]
        public void Results_Add_Up()
        {
            SaveData data = SaveDataLogic.CreateDefault();

            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.Beach, true, 150f, 180));
            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.Beach, false, 90f, 60));

            PlayStats s = data.Stats;

            Assert.AreEqual(2, s.Attempts);
            Assert.AreEqual(1, s.Clears);
            Assert.AreEqual(240f, s.TotalSeconds, 0.001f);
            Assert.AreEqual(10, s.Hypnosis);
            Assert.AreEqual(3, s.MaxFollowers);
            Assert.AreEqual(240, s.RecoveredEssence);
            Assert.AreEqual(180, s.BestEssence);
            Assert.AreEqual(4, s.LevelUps);
            Assert.AreEqual(6, s.CardsPicked);
            Assert.AreEqual(2, s.DuelWins);
        }

        [Test]
        public void Location_Records_Keep_The_Best()
        {
            SaveData data = SaveDataLogic.CreateDefault();

            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.OfficeTower, true, 180f, 150, "B"));
            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.OfficeTower, true, 140f, 120, "A"));
            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.OfficeTower, true, 170f, 200, "C"));
            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.OfficeTower, false, 20f, 10));

            LocationStats office = PlayStatsLogic.Get(data, (int)LocationId.OfficeTower);

            Assert.AreEqual(4, office.Attempts);
            Assert.AreEqual(3, office.Clears);
            Assert.AreEqual(200, office.BestEssence);
            Assert.AreEqual(140f, office.BestClearSeconds, 0.001f);
            Assert.AreEqual("A", office.BestRank);

            // 다른 장소는 건드리지 않는다.
            Assert.AreEqual(0, PlayStatsLogic.Get(data, (int)LocationId.Beach).Attempts);
            Assert.AreEqual(1, data.LocationRecords.Length);
        }

        [Test]
        public void Failed_Fast_Attempts_Do_Not_Become_Best_Clear_Time()
        {
            SaveData data = SaveDataLogic.CreateDefault();

            data = SaveDataLogic.ApplyStageResult(data, Result(LocationId.Beach, false, 5f, 0));

            Assert.AreEqual(0f, PlayStatsLogic.Get(data, (int)LocationId.Beach).BestClearSeconds);
            Assert.AreEqual("-", PlayStatsLogic.FormatClock(0f));
        }

        [Test]
        public void Boss_Defeat_Counts_As_Ending()
        {
            SaveData data = SaveDataLogic.CreateDefault();
            StageResultSummary result = Result(LocationId.RooftopClub, true, 200f, 250);
            result.BossDefeated = true;

            data = SaveDataLogic.ApplyStageResult(data, result);

            Assert.AreEqual(1, data.Stats.Endings);
        }

        [Test]
        public void Cheated_Or_Unknown_Results_Are_Not_Counted_In_Stats()
        {
            SaveData data = SaveDataLogic.CreateDefault();

            StageResultSummary cheated = Result(LocationId.Beach, true, 100f, 100);
            cheated.Cheated = true;

            data = SaveDataLogic.ApplyStageResult(data, cheated);
            data = SaveDataLogic.ApplyStageResult(data, StageResultSummary.Empty);

            Assert.AreEqual(0, data.Stats.Attempts);
            Assert.AreEqual(0, data.LocationRecords.Length);

            // 기존 기록(플레이 횟수)은 예전처럼 센다.
            Assert.AreEqual(2, data.PlayCount);
        }

        [Test]
        public void Clone_Copies_Stats_Deeply()
        {
            SaveData data = SaveDataLogic.ApplyStageResult(SaveDataLogic.CreateDefault(), Result(LocationId.Beach, true, 100f, 100));
            SaveData copy = data.Clone();

            copy.Stats.Attempts = 99;
            copy.LocationRecords[0].Clears = 99;

            Assert.AreEqual(1, data.Stats.Attempts);
            Assert.AreEqual(1, data.LocationRecords[0].Clears);
        }

        [Test]
        public void Duplicate_Location_Records_Are_Removed()
        {
            SaveData data = SaveDataLogic.CreateDefault();
            data.LocationRecords = new[]
            {
                new LocationStats { Location = 1, Attempts = 2 },
                null,
                new LocationStats { Location = 1, Attempts = 5, BestRank = null }
            };

            SaveDataLogic.Normalize(data);

            Assert.AreEqual(1, data.LocationRecords.Length);
            Assert.AreEqual(2, data.LocationRecords[0].Attempts);
        }

        [Test]
        public void Formatting()
        {
            Assert.AreEqual("12초", PlayStatsLogic.FormatDuration(12.9f));
            Assert.AreEqual("3분 07초", PlayStatsLogic.FormatDuration(187f));
            Assert.AreEqual("1시간 02분 05초", PlayStatsLogic.FormatDuration(3725f));
            Assert.AreEqual("2:35", PlayStatsLogic.FormatClock(155.4f));
            Assert.AreEqual("0%", PlayStatsLogic.FormatPercent(PlayStatsLogic.GetClearRate(0, 0)));
            Assert.AreEqual("67%", PlayStatsLogic.FormatPercent(PlayStatsLogic.GetClearRate(3, 2)));
        }

        [Test]
        public void Stats_Window_Has_All_Sections()
        {
            List<StatsSection> sections = PlayStatsLogic.BuildSections(new PlayStats { Attempts = 4, Clears = 1 });

            Assert.AreEqual(5, sections.Count);
            Assert.AreEqual("전체", sections[0].Title);
            Assert.AreEqual("25%", sections[0].Rows[3].Value);

            foreach (StatsSection section in sections)
            {
                Assert.IsNotEmpty(section.Rows);

                // 카드 높이 안에 들어가야 한다.
                Assert.LessOrEqual(section.Rows.Length, 5, section.Title);
            }

            Assert.AreEqual(PlayStatsLogic.LocationColumns.Length, PlayStatsLogic.BuildLocationRow(null).Length);
        }
    }

    public sealed class EndingLogicTests
    {
        [Test]
        public void Timeline_Is_In_Order()
        {
            Assert.Less(EndingLogic.WaitSeconds, EndingLogic.LinesStart);
            Assert.Less(EndingLogic.LinesStart, EndingLogic.FadeOutStart);
            Assert.Less(EndingLogic.FadeOutStart, EndingLogic.TotalSeconds);
            Assert.LessOrEqual(EndingLogic.TotalSeconds, 12f);
            Assert.IsNotEmpty(EndingLogic.Lines);
        }

        [Test]
        public void Backdrop_Darkens_Then_Clears()
        {
            Assert.AreEqual(0f, EndingLogic.GetBackdropAlpha(0.5f));
            Assert.Greater(EndingLogic.GetBackdropAlpha(EndingLogic.WaitSeconds + 0.4f), 0f);
            Assert.AreEqual(EndingLogic.BackdropAlpha, EndingLogic.GetBackdropAlpha(EndingLogic.LinesStart + 1f));
            Assert.AreEqual(0f, EndingLogic.GetBackdropAlpha(EndingLogic.TotalSeconds), 0.001f);
        }

        [Test]
        public void Lines_Appear_One_By_One()
        {
            float firstVisible = EndingLogic.LinesStart + EndingLogic.LineFadeSeconds;

            Assert.AreEqual(1f, EndingLogic.GetTextAlpha(firstVisible, 0), 0.001f);
            Assert.AreEqual(1f, EndingLogic.GetTextAlpha(firstVisible, -1), 0.001f);
            Assert.AreEqual(0f, EndingLogic.GetTextAlpha(firstVisible, 1), 0.001f);

            float allVisible = EndingLogic.FadeOutStart - 0.01f;

            for (int i = 0; i < EndingLogic.Lines.Length; i++)
            {
                Assert.AreEqual(1f, EndingLogic.GetTextAlpha(allVisible, i), 0.001f, $"line {i}");
                Assert.AreEqual(0f, EndingLogic.GetTextAlpha(EndingLogic.TotalSeconds, i), 0.001f);
            }
        }

        [Test]
        public void Skip_Jumps_To_Fade_Out_Only_While_Lines_Show()
        {
            Assert.AreEqual(0.5f, EndingLogic.Skip(0.5f));
            Assert.AreEqual(EndingLogic.FadeOutStart, EndingLogic.Skip(EndingLogic.LinesStart + 0.1f));

            float fading = EndingLogic.FadeOutStart + 0.1f;
            Assert.AreEqual(fading, EndingLogic.Skip(fading));

            Assert.IsFalse(EndingLogic.IsFinished(EndingLogic.FadeOutStart));
            Assert.IsTrue(EndingLogic.IsFinished(EndingLogic.TotalSeconds));
        }
    }
}
