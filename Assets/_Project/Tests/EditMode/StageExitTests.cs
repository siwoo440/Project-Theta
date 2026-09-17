using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class StageExitLogicTests
    {
        private static StageState Resolve(
            LocationObjective objective,
            float remaining,
            int essence,
            int target = 300,
            int health = 5,
            bool exit = false,
            bool bossDefeated = false,
            int trains = 0,
            int followers = 0)
        {
            return StageExitLogic.Resolve(objective, remaining, essence, target, health, trains, 3, followers, bossDefeated, exit);
        }

        [Test]
        public void Essence_Places_Use_Exit_But_Survival_And_Boss_Do_Not()
        {
            Assert.IsTrue(StageExitLogic.UsesExit(LocationObjective.EssenceQuota));
            Assert.IsTrue(StageExitLogic.UsesExit(LocationObjective.GroupCarry));
            Assert.IsTrue(StageExitLogic.UsesExit(LocationObjective.SpecialTarget));
            Assert.IsTrue(StageExitLogic.UsesExit(LocationObjective.Stealth));
            Assert.IsFalse(StageExitLogic.UsesExit(LocationObjective.Survival));
            Assert.IsFalse(StageExitLogic.UsesExit(LocationObjective.Boss));
        }

        [Test]
        public void Goal_Makes_Exit_Ready_Instead_Of_Clear()
        {
            Assert.AreEqual(StageState.Running, Resolve(LocationObjective.EssenceQuota, 100f, 299));
            Assert.AreEqual(StageState.ExitReady, Resolve(LocationObjective.EssenceQuota, 100f, 300));
            Assert.AreEqual(StageState.ExitReady, Resolve(LocationObjective.Stealth, 100f, 900));
        }

        [Test]
        public void Exit_Or_Time_Up_Clears_After_Goal()
        {
            Assert.AreEqual(StageState.Cleared, Resolve(LocationObjective.EssenceQuota, 100f, 300, exit: true));
            Assert.AreEqual(StageState.Cleared, Resolve(LocationObjective.EssenceQuota, 0f, 300));

            // 목표 전에는 탈출 요청이 있어도 끝나지 않는다.
            Assert.AreEqual(StageState.Running, Resolve(LocationObjective.EssenceQuota, 100f, 10, exit: true));
            Assert.AreEqual(StageState.FailedByTime, Resolve(LocationObjective.EssenceQuota, 0f, 10));
        }

        [Test]
        public void Falling_Before_Exit_Is_Still_A_Failure()
        {
            Assert.AreEqual(StageState.FailedByHealth, Resolve(LocationObjective.EssenceQuota, 100f, 500, health: 0));
            Assert.AreEqual(StageState.FailedByHealth, Resolve(LocationObjective.EssenceQuota, 100f, 500, health: 0, exit: true));
        }

        [Test]
        public void Survival_And_Boss_Keep_Their_Old_Rules()
        {
            // 보스: 정기를 채워도 끝나지 않고, 함락하면 바로 클리어.
            Assert.AreEqual(StageState.Running, Resolve(LocationObjective.Boss, 100f, 999));
            Assert.AreEqual(StageState.Cleared, Resolve(LocationObjective.Boss, 100f, 0, bossDefeated: true));

            // 생존: 열차 3대를 버티고 동행자가 있으면 바로 클리어.
            Assert.AreEqual(StageState.Cleared, Resolve(LocationObjective.Survival, 100f, 0, trains: 3, followers: 1));
            Assert.AreEqual(StageState.Running, Resolve(LocationObjective.Survival, 100f, 999, trains: 1, followers: 1));
        }

        [Test]
        public void Exit_Kind()
        {
            Assert.AreEqual(StageExitKind.Escaped, StageExitLogic.GetExitKind(LocationObjective.EssenceQuota, StageState.Cleared, true));
            Assert.AreEqual(StageExitKind.TimeUp, StageExitLogic.GetExitKind(LocationObjective.EssenceQuota, StageState.Cleared, false));
            Assert.AreEqual(StageExitKind.None, StageExitLogic.GetExitKind(LocationObjective.EssenceQuota, StageState.FailedByTime, false));
            Assert.AreEqual(StageExitKind.None, StageExitLogic.GetExitKind(LocationObjective.Boss, StageState.Cleared, false));

            StringAssert.Contains("탈출", StageExitLogic.GetTitleSuffix(StageExitKind.Escaped));
            StringAssert.Contains("시간 종료", StageExitLogic.GetTitleSuffix(StageExitKind.TimeUp));
            Assert.AreEqual(string.Empty, StageExitLogic.GetTitleSuffix(StageExitKind.None));
        }

        [Test]
        public void Can_Exit_Only_On_First_Floor_Inside_The_Zone()
        {
            Assert.IsTrue(StageExitLogic.CanExit(StageState.ExitReady, 0, true, false));

            Assert.IsFalse(StageExitLogic.CanExit(StageState.Running, 0, true, false));
            Assert.IsFalse(StageExitLogic.CanExit(StageState.ExitReady, 1, true, false));
            Assert.IsFalse(StageExitLogic.CanExit(StageState.ExitReady, 0, false, false));
            Assert.IsFalse(StageExitLogic.CanExit(StageState.ExitReady, 0, true, true));
        }

        [Test]
        public void Overflow_And_Banner()
        {
            Assert.AreEqual(0, StageExitLogic.GetOverflow(100, 300));
            Assert.AreEqual(45, StageExitLogic.GetOverflow(345, 300));

            string first = StageExitLogic.GetBanner("F", 0, 45, 30.2f);
            string upper = StageExitLogic.GetBanner("G", 2, 0, 0f);

            StringAssert.Contains("[F] 탈출", first);
            StringAssert.Contains("+45", first);
            StringAssert.Contains("31초", first);
            StringAssert.Contains("1F로 내려가", upper);
        }

        [Test]
        public void New_State_Keeps_Old_Numbers()
        {
            Assert.AreEqual(4, (int)StageState.Abandoned);
            Assert.AreEqual(5, (int)StageState.ExitReady);
        }

        [Test]
        public void Targets_Were_Raised_For_Longer_Play()
        {
            // 33일차: 최면 6명 정도로 끝나지 않게 목표 정기를 약 2.2배로 올렸다.
            Assert.AreEqual(310, LocationCatalog.Get(LocationId.TrainingCenter).TargetEssence);
            Assert.AreEqual(440, LocationCatalog.Get(LocationId.RooftopClub).TargetEssence);

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.GreaterOrEqual(location.TargetEssence, 300, location.Id.ToString());
            }
        }
    }

    public sealed class BalanceReportLogicTests
    {
        private static RunLogEntry Entry(
            string location,
            string result,
            float seconds,
            int essence,
            string exit = "None",
            bool cheated = false,
            int levelUps = 0)
        {
            RunStats stats = new RunStats(2)
            {
                TotalSeconds = seconds,
                RecoveredEssence = essence,
                HypnosisCount = essence / 30,
                Cheated = cheated
            };

            for (int i = 0; i < levelUps; i++)
            {
                stats.LevelUpTimes.Add(i);
            }

            return new RunLogEntry
            {
                Location = location,
                Result = result,
                Exit = exit,
                TargetEssence = 300,
                SecondsAfterGoal = exit == "Escaped" ? 20f : 0f,
                Stats = stats
            };
        }

        [Test]
        public void Groups_By_Location_And_Skips_Cheats_And_Old_Logs()
        {
            List<RunLogEntry> entries = new List<RunLogEntry>
            {
                Entry("Beach", "Cleared", 120f, 360, "Escaped", levelUps: 2),
                Entry("Beach", "FailedByTime", 180f, 200),
                Entry("Beach", "Cleared", 150f, 330, "TimeUp"),
                Entry("OfficeTower", "Cleared", 90f, 400, "Escaped"),
                Entry("OfficeTower", "Cleared", 30f, 999, "Escaped", cheated: true),
                Entry(null, "Cleared", 30f, 200),
                null
            };

            List<BalanceReportRow> rows = BalanceReportLogic.Build(entries, out int cheated, out int unknown);

            Assert.AreEqual(1, cheated);
            Assert.AreEqual(1, unknown);
            Assert.AreEqual(2, rows.Count);

            BalanceReportRow beach = rows[0];

            Assert.AreEqual("Beach", beach.Location);
            Assert.AreEqual(3, beach.Count);
            Assert.AreEqual(2, beach.Clears);
            Assert.AreEqual(1, beach.Escapes);
            Assert.AreEqual(150f, beach.AverageSeconds, 0.01f);
            Assert.AreEqual(120f, beach.MinSeconds);
            Assert.AreEqual(180f, beach.MaxSeconds);
            Assert.AreEqual(0.667f, beach.ClearRate, 0.01f);
            Assert.AreEqual(0.5f, beach.EscapeRate, 0.01f);
            Assert.AreEqual(1f + 2f / 3f, beach.AverageLevel, 0.01f);

            Assert.AreEqual(1, rows[1].Count);
        }

        [Test]
        public void Markdown_Table_And_Summary()
        {
            List<BalanceReportRow> rows =
                BalanceReportLogic.Build(
                    new List<RunLogEntry> { Entry("Beach", "Cleared", 125f, 360, "Escaped") },
                    out int cheated,
                    out int unknown);

            string markdown = BalanceReportLogic.ToMarkdown(rows, cheated, unknown, "2026-09-17 12:00");

            StringAssert.Contains("# 밸런스 보고서", markdown);
            StringAssert.Contains("| Beach | 1 | 100% | 100% | 2:05 |", markdown);
            StringAssert.Contains("360 / 300", markdown);

            Assert.AreEqual("기록이 없습니다", BalanceReportLogic.ToSummary(new List<BalanceReportRow>()));
            StringAssert.Contains("평균 2:05", BalanceReportLogic.ToSummary(rows));
            Assert.AreEqual("0:00", BalanceReportLogic.Seconds(-3f));
        }
    }
}
