using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Balance;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;
using ProjectTheta.Save;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class FailureRewardTests
    {
        [Test]
        public void Failure_Pays_Half_And_Clear_Is_Unchanged()
        {
            // 클리어: 300 × 0.5 = 150, 초과 없음
            Assert.AreEqual(150, ContractEssenceLogic.Compute(300, 300, true, "C"));

            // 시간 초과: 200 × 0.5 × 0.5 = 50
            Assert.AreEqual(50, ContractEssenceLogic.Compute(200, 300, false, "-"));
            Assert.AreEqual(50, ContractEssenceLogic.FailurePercent);
        }

        [Test]
        public void Falling_After_Goal_Loses_The_Overflow()
        {
            // 탈출 가능 상태에서 400 / 300으로 쓰러짐: 300까지만 인정 → 75
            Assert.AreEqual(75, ContractEssenceLogic.Compute(400, 300, false, "-"));

            // 같은 양으로 탈출했다면: 200 + 초과 100 × 0.3 = 230
            Assert.AreEqual(230, ContractEssenceLogic.Compute(400, 300, true, "C"));
        }

        [Test]
        public void Abandon_Still_Pays_Nothing()
        {
            Assert.AreEqual(0, UI.PauseMenuLogic.GetContractEssence(true, ContractEssenceLogic.Compute(200, 300, false, "-")));
        }

        [Test]
        public void Full_Rate_And_Note()
        {
            Assert.AreEqual(100, ContractEssenceLogic.GetFullRate(200));
            Assert.AreEqual(0, ContractEssenceLogic.GetFullRate(-5));

            StringAssert.Contains("보상 50%", ContractEssenceLogic.GetFailureNote(50));
            StringAssert.Contains("−50", ContractEssenceLogic.GetFailureNote(50));
            Assert.IsFalse(ContractEssenceLogic.GetFailureNote(0).Contains("−"));
        }
    }

    public sealed class ChasePopulationTests
    {
        [Test]
        public void Without_Chase_Nothing_Changes()
        {
            Assert.AreEqual(
                PopulationLogic.GetAllowed(45f, 20f),
                PopulationLogic.GetAllowed(45f, 20f, false, 99f, 10f, 6));

            Assert.AreEqual(4, PopulationLogic.GetCap(false, 6));
        }

        [Test]
        public void Chase_Adds_One_At_Once_Then_Every_Interval_Up_To_Max()
        {
            // 80초가 지나 이미 4명 허용 → 추격 시작 즉시 5명, 10초 뒤 6명, 그 뒤로는 6명
            Assert.AreEqual(5, PopulationLogic.GetAllowed(80f, 20f, true, 0f, 10f, 6));
            Assert.AreEqual(6, PopulationLogic.GetAllowed(80f, 20f, true, 10f, 10f, 6));
            Assert.AreEqual(6, PopulationLogic.GetAllowed(80f, 20f, true, 60f, 10f, 6));

            // 20초에 목표를 채움(1명 허용) → 2명부터
            Assert.AreEqual(2, PopulationLogic.GetAllowed(20f, 20f, true, 0f, 10f, 6));

            // 간격 0이면 곧바로 최대
            Assert.AreEqual(6, PopulationLogic.GetAllowed(0f, 20f, true, 0f, 0f, 6));
        }

        [Test]
        public void Chase_Cap_Is_Clamped()
        {
            Assert.AreEqual(6, PopulationLogic.GetCap(true, 6));
            Assert.AreEqual(4, PopulationLogic.GetCap(true, 2));
            Assert.AreEqual(PopulationLogic.HardMaxPerFloor, PopulationLogic.GetCap(true, 99));

            Assert.AreEqual(2, PopulationLogic.GetFreeSlots(6, 4));
            Assert.AreEqual(0, PopulationLogic.GetFreeSlots(99, PopulationLogic.HardMaxPerFloor));
        }

        [Test]
        public void Balance_Defaults()
        {
            StageBalanceValues values = new StageBalanceValues();

            Assert.AreEqual(6, values.ChaseMaxPerFloor);
            Assert.AreEqual(10f, values.ChaseRampSeconds);

            StageBalanceValues copy = values.Clone();
            Assert.AreEqual(values.ChaseMaxPerFloor, copy.ChaseMaxPerFloor);
            Assert.AreEqual(values.ChaseRampSeconds, copy.ChaseRampSeconds);
        }
    }

    public sealed class DangerLogicTests
    {
        private static DangerInputs Calm()
        {
            return new DangerInputs
            {
                HealthNormalized = 1f,
                Running = true,
                RemainingSeconds = 100f
            };
        }

        [Test]
        public void Calm_Stage_Has_No_Danger()
        {
            Assert.AreEqual(0f, DangerLogic.Resolve(Calm(), true));
        }

        [Test]
        public void Each_Source_Raises_Danger()
        {
            DangerInputs health = Calm();
            health.HealthNormalized = 0.1f;

            DangerInputs steal = Calm();
            steal.StealProgress = 0.9f;

            DangerInputs locked = Calm();
            locked.RecoveryLocked = true;

            DangerInputs time = Calm();
            time.RemainingSeconds = 5f;

            Assert.Greater(DangerLogic.Resolve(health, true), 0.35f);
            Assert.Greater(DangerLogic.Resolve(steal, true), 0.3f);
            Assert.AreEqual(DangerLogic.LockedIntensity, DangerLogic.Resolve(locked, true), 0.0001f);
            Assert.Greater(DangerLogic.Resolve(time, true), 0.3f);
        }

        [Test]
        public void Thresholds()
        {
            Assert.AreEqual(0f, DangerLogic.FromHealth(0.31f));
            Assert.AreEqual(0f, DangerLogic.FromHealth(0f));
            Assert.Greater(DangerLogic.FromHealth(0.1f), DangerLogic.FromHealth(0.3f));

            Assert.AreEqual(0f, DangerLogic.FromSteal(0.49f));
            Assert.AreEqual(0.8f, DangerLogic.FromSteal(1f), 0.0001f);

            Assert.AreEqual(0f, DangerLogic.FromTime(16f, true));
            Assert.AreEqual(0f, DangerLogic.FromTime(5f, false));
            Assert.Greater(DangerLogic.FromTime(1f, true), DangerLogic.FromTime(14f, true));
        }

        [Test]
        public void Strongest_Wins_And_Off_Means_Zero()
        {
            DangerInputs many = Calm();
            many.Rampage = 1f;
            many.HealthNormalized = 0.1f;
            many.RecoveryLocked = true;

            Assert.AreEqual(1f, DangerLogic.Resolve(many, true));
            Assert.AreEqual(0f, DangerLogic.Resolve(many, false));

            // 끝난 뒤에는 폭주 값만 따른다.
            DangerInputs ended = many;
            ended.Running = false;
            ended.Rampage = 0f;

            Assert.AreEqual(0f, DangerLogic.Resolve(ended, true));
        }
    }

    public sealed class SaveSlotLogicTests
    {
        [Test]
        public void Slot_Files_And_Labels()
        {
            Assert.AreEqual(3, SaveSlotLogic.SlotCount);
            Assert.AreEqual("projecttheta_slot1.json", SaveSlotLogic.GetFileName(0));
            Assert.AreEqual("projecttheta_slot3.json", SaveSlotLogic.GetFileName(2));
            Assert.AreEqual("2번 칸", SaveSlotLogic.GetSlotLabel(1));

            Assert.IsTrue(SaveSlotLogic.IsValid(0));
            Assert.IsFalse(SaveSlotLogic.IsValid(-1));
            Assert.IsFalse(SaveSlotLogic.IsValid(3));
        }

        [Test]
        public void New_Game_Keeps_Settings_But_Resets_Progress()
        {
            SaveData old = SaveDataLogic.CreateDefault();
            old.ContractEssence = 900;
            old.ClearCount = 7;
            old.TutorialCompleted = true;
            old.UnlockedAchievements = new[] { "first_clear" };
            old.MasterVolume = 0.3f;
            old.CursorSize = (int)CursorSize.Large;
            old.ScreenShakeDisabled = true;
            old.DangerEffectDisabled = true;
            old.KeyBindings = new[] { "Interact=G" };

            SaveData fresh =
                SaveSlotLogic.CreateNewGame(
                    SaveSlotLogic.ExtractSettings(old));

            Assert.AreEqual(0, fresh.ContractEssence);
            Assert.AreEqual(0, fresh.ClearCount);
            Assert.IsFalse(fresh.TutorialCompleted);
            Assert.AreEqual(0, fresh.UnlockedAchievements.Length);

            Assert.AreEqual(0.3f, fresh.MasterVolume, 0.0001f);
            Assert.AreEqual((int)CursorSize.Large, fresh.CursorSize);
            Assert.IsTrue(fresh.ScreenShakeDisabled);
            Assert.IsTrue(fresh.DangerEffectDisabled);
            CollectionAssert.AreEqual(new[] { "Interact=G" }, fresh.KeyBindings);

            // 설정을 복사했으므로 원본 배열과 따로 논다.
            fresh.KeyBindings[0] = "Interact=H";
            Assert.AreEqual("Interact=G", old.KeyBindings[0]);
        }

        [Test]
        public void Missing_Settings_Keep_Slot_Values()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.MasterVolume = 0.4f;

            SaveSlotLogic.ApplySettings(null, save);

            Assert.AreEqual(0.4f, save.MasterVolume, 0.0001f);
            Assert.IsNotNull(SaveSlotLogic.CreateNewGame(null));
            Assert.IsNull(SaveSlotLogic.ExtractSettings(null));
        }

        [Test]
        public void Summary_And_Description()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.ContractEssence = 1240;
            save.ClearCount = 5;
            save.Stats.TotalSeconds = 3900f;
            save.UnlockedAchievements = new[] { "a", "b" };
            save.LocationRecords = new[]
            {
                new LocationStats { Location = 0, Attempts = 2, Clears = 1 },
                new LocationStats { Location = 1, Attempts = 3, Clears = 0 },
                null
            };

            SaveSlotLogic.Stamp(save, new DateTime(2026, 9, 17, 12, 30, 0));

            SaveSlotSummary summary = SaveSlotLogic.Summarize(1, save);

            Assert.IsTrue(summary.Exists);
            Assert.AreEqual(1, summary.Slot);
            Assert.AreEqual(1, summary.ClearedLocations);
            Assert.AreEqual(2, summary.Achievements);
            Assert.AreEqual("2026-09-17 12:30", summary.SavedAt);

            string text = SaveSlotLogic.Describe(summary, 8);

            StringAssert.Contains("저장 2026-09-17 12:30", text);
            StringAssert.Contains("1시간 05분", text);
            StringAssert.Contains("계약 정기 1,240", text);
            StringAssert.Contains("클리어 장소 1/8", text);

            Assert.AreEqual("비어 있음", SaveSlotLogic.Describe(SaveSlotLogic.Summarize(0, null), 8));
            Assert.AreEqual("0분", SaveSlotLogic.FormatPlayTime(-1f));
            Assert.AreEqual("12분", SaveSlotLogic.FormatPlayTime(725f));
        }

        [Test]
        public void Latest_Slot()
        {
            List<SaveSlotSummary> summaries = new List<SaveSlotSummary>
            {
                new SaveSlotSummary { Slot = 0, Exists = true, SavedTicks = 100 },
                new SaveSlotSummary { Slot = 1, Exists = false, SavedTicks = 999 },
                new SaveSlotSummary { Slot = 2, Exists = true, SavedTicks = 300 }
            };

            Assert.AreEqual(2, SaveSlotLogic.FindLatest(summaries));
            Assert.AreEqual(2, SaveSlotLogic.GetFallbackSlot(summaries));
            Assert.IsTrue(SaveSlotLogic.AnyExists(summaries));

            List<SaveSlotSummary> empty = new List<SaveSlotSummary>
            {
                new SaveSlotSummary { Slot = 0 },
                new SaveSlotSummary { Slot = 1 }
            };

            Assert.AreEqual(-1, SaveSlotLogic.FindLatest(empty));
            Assert.AreEqual(0, SaveSlotLogic.GetFallbackSlot(empty));
            Assert.IsFalse(SaveSlotLogic.AnyExists(null));

            // 옛 세이브(시각 0)만 있어도 최근 칸으로 잡힌다.
            List<SaveSlotSummary> legacy = new List<SaveSlotSummary>
            {
                new SaveSlotSummary { Slot = 0, Exists = true, SavedTicks = 0 }
            };

            Assert.AreEqual(0, SaveSlotLogic.FindLatest(legacy));
        }

        [Test]
        public void Migration_Runs_Only_Once()
        {
            Assert.IsTrue(SaveSlotLogic.ShouldMigrate(true, false, false));
            Assert.IsFalse(SaveSlotLogic.ShouldMigrate(false, false, false));
            Assert.IsFalse(SaveSlotLogic.ShouldMigrate(true, true, false));
            Assert.IsFalse(SaveSlotLogic.ShouldMigrate(true, false, true));
        }

        [Test]
        public void Overwrite_Needs_A_Second_Press()
        {
            SaveSlotSummary empty = new SaveSlotSummary { Slot = 0 };
            SaveSlotSummary used = new SaveSlotSummary { Slot = 1, Exists = true };

            Assert.IsTrue(SaveSlotLogic.CanStartNewGame(empty, false));
            Assert.IsFalse(SaveSlotLogic.CanStartNewGame(used, false));
            Assert.IsTrue(SaveSlotLogic.CanStartNewGame(used, true));

            Assert.AreEqual("새로 시작", SaveSlotLogic.GetNewGameButton(empty, false));
            Assert.AreEqual("덮어쓰기", SaveSlotLogic.GetNewGameButton(used, false));
            StringAssert.Contains("한 번 더", SaveSlotLogic.GetNewGameButton(used, true));
        }

        [Test]
        public void Old_Json_Without_New_Fields_Loads()
        {
            SaveData save = new SaveData();

            Assert.IsFalse(save.DangerEffectDisabled);
            Assert.AreEqual(string.Empty, save.SavedAt);
            Assert.AreEqual(0L, save.SavedTicks);

            SaveData copy = save.Clone();
            Assert.AreEqual(save.SavedAt, copy.SavedAt);
        }
    }
}
