using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Save;
using ProjectTheta.Stage;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class SettingsLogicTests
    {
        [Test]
        public void New_Save_Has_Default_Settings()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.IsTrue(save.SettingsInitialized);
            Assert.AreEqual(SettingsLogic.DefaultVolume, save.MasterVolume);
            Assert.AreEqual(SettingsLogic.DefaultVolume, save.SfxVolume);
            Assert.IsTrue(save.Fullscreen);
            Assert.AreEqual(2, save.ResolutionIndex);
            Assert.AreEqual((int)CursorSize.Normal, save.CursorSize);
        }

        [Test]
        public void Old_Save_Without_Settings_Is_Not_Silent()
        {
            // 예전 세이브는 설정 칸이 없어 0으로 읽힌다.
            SaveData old = new SaveData
            {
                SettingsInitialized = false,
                MasterVolume = 0f,
                SfxVolume = 0f,
                MusicVolume = 0f,
                ResolutionIndex = 0
            };

            SaveDataLogic.Normalize(old);

            Assert.IsTrue(old.SettingsInitialized);
            Assert.Greater(SettingsLogic.GetEffectiveSfx(old), 0f);
            Assert.AreEqual(SettingsLogic.DefaultResolutionIndex, old.ResolutionIndex);
        }

        [Test]
        public void Saved_Zero_Volume_Stays_Zero()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.MasterVolume = 0f;

            SaveDataLogic.Normalize(save);

            Assert.AreEqual(0f, save.MasterVolume);
            Assert.AreEqual(0f, SettingsLogic.GetEffectiveSfx(save));
        }

        [Test]
        public void Values_Are_Clamped()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.MasterVolume = 3f;
            save.SfxVolume = -1f;
            save.MusicVolume = float.NaN;
            save.ResolutionIndex = 9;
            save.CursorSize = -4;

            SaveDataLogic.Normalize(save);

            Assert.AreEqual(1f, save.MasterVolume);
            Assert.AreEqual(0f, save.SfxVolume);
            Assert.AreEqual(SettingsLogic.DefaultVolume, save.MusicVolume);
            Assert.AreEqual(SettingsLogic.DefaultResolutionIndex, save.ResolutionIndex);
            Assert.AreEqual((int)CursorSize.Normal, save.CursorSize);
        }

        [Test]
        public void Effective_Volume_Multiplies_Master()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.MasterVolume = 0.5f;
            save.SfxVolume = 0.6f;
            save.MusicVolume = 1f;

            Assert.AreEqual(0.3f, SettingsLogic.GetEffectiveSfx(save), 0.0001f);
            Assert.AreEqual(0.5f, SettingsLogic.GetEffectiveMusic(save), 0.0001f);
            Assert.Greater(SettingsLogic.GetEffectiveSfx(null), 0f);
        }

        [Test]
        public void Only_Three_Resolutions()
        {
            Assert.AreEqual(3, SettingsLogic.ResolutionCount);
            Assert.AreEqual("1280×720", SettingsLogic.GetResolutionLabel(0));
            Assert.AreEqual("1600×900", SettingsLogic.GetResolutionLabel(1));
            Assert.AreEqual("1920×1080", SettingsLogic.GetResolutionLabel(2));
            Assert.AreEqual(1920, SettingsLogic.GetWidth(-1));
        }

        [Test]
        public void Cursor_Sizes_Grow()
        {
            Assert.Less(SettingsLogic.GetCursorScale(0), SettingsLogic.GetCursorScale(1));
            Assert.Less(SettingsLogic.GetCursorScale(1), SettingsLogic.GetCursorScale(2));
            Assert.AreEqual(1f, SettingsLogic.GetCursorScale(1));
            Assert.AreEqual(1f, SettingsLogic.GetCursorScale(7));
            Assert.AreEqual(SettingsLogic.CursorScales.Length, SettingsLogic.CursorLabels.Length);
        }

        [Test]
        public void Volume_Text()
        {
            Assert.AreEqual("80%", SettingsLogic.FormatVolume(0.8f));
            Assert.AreEqual("0%", SettingsLogic.FormatVolume(-2f));
        }

        [Test]
        public void Clone_Copies_Settings()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.MasterVolume = 0.3f;
            save.CursorSize = 2;
            save.Fullscreen = false;

            SaveData copy = save.Clone();

            Assert.AreEqual(0.3f, copy.MasterVolume);
            Assert.AreEqual(2, copy.CursorSize);
            Assert.IsFalse(copy.Fullscreen);
            Assert.IsTrue(copy.SettingsInitialized);
        }
    }

    public sealed class UiEscapeStackTests
    {
        [SetUp]
        public void SetUp()
        {
            UiEscapeStack.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            UiEscapeStack.Clear();
        }

        [Test]
        public void Only_The_Top_Window_Takes_Escape()
        {
            object pause = new object();
            object settings = new object();

            UiEscapeStack.Push(pause);
            UiEscapeStack.Push(settings);

            Assert.IsFalse(UiEscapeStack.TryConsume(pause, 1));
            Assert.IsTrue(UiEscapeStack.TryConsume(settings, 1));

            // 같은 프레임의 Esc는 한 번만 쓰인다.
            UiEscapeStack.Remove(settings);
            Assert.IsFalse(UiEscapeStack.TryConsume(pause, 1));

            Assert.IsTrue(UiEscapeStack.TryConsume(pause, 2));
        }

        [Test]
        public void Pause_Opens_Only_When_Nothing_Is_Open()
        {
            object stats = new object();

            UiEscapeStack.Push(stats);
            Assert.IsFalse(UiEscapeStack.TryConsumeWhenEmpty(5));

            Assert.IsTrue(UiEscapeStack.TryConsume(stats, 6));
            UiEscapeStack.Remove(stats);

            // 창을 닫은 같은 프레임에 일시정지가 열리지 않는다.
            Assert.IsFalse(UiEscapeStack.TryConsumeWhenEmpty(6));
            Assert.IsTrue(UiEscapeStack.TryConsumeWhenEmpty(7));
        }

        [Test]
        public void Pushing_Again_Moves_To_Top_Without_Duplicates()
        {
            object a = new object();
            object b = new object();

            UiEscapeStack.Push(a);
            UiEscapeStack.Push(b);
            UiEscapeStack.Push(a);
            UiEscapeStack.Push(null);

            Assert.AreEqual(2, UiEscapeStack.Count);
            Assert.IsTrue(UiEscapeStack.IsTop(a));

            UiEscapeStack.Remove(a);
            UiEscapeStack.Remove(b);

            Assert.IsTrue(UiEscapeStack.IsEmpty);
        }
    }

    public sealed class PauseMenuLogicTests
    {
        [Test]
        public void Pause_Menu_Opens_Only_During_Normal_Play()
        {
            Assert.IsTrue(PauseMenuLogic.CanOpen(true, false, false, false));

            Assert.IsFalse(PauseMenuLogic.CanOpen(false, false, false, false), "결과 화면");
            Assert.IsFalse(PauseMenuLogic.CanOpen(true, true, false, false), "카드 선택");
            Assert.IsFalse(PauseMenuLogic.CanOpen(true, false, true, false), "엔딩");
            Assert.IsFalse(PauseMenuLogic.CanOpen(true, false, false, true), "로딩");
        }

        [Test]
        public void Abandon_Gives_No_Contract_Essence()
        {
            Assert.AreEqual(0, PauseMenuLogic.GetContractEssence(true, 250));
            Assert.AreEqual(250, PauseMenuLogic.GetContractEssence(false, 250));
        }

        [Test]
        public void Abandoned_State_Keeps_Old_Values()
        {
            // 예전 값의 번호가 바뀌면 안 된다(세이브 · 기록 호환).
            Assert.AreEqual(0, (int)StageState.Running);
            Assert.AreEqual(1, (int)StageState.Cleared);
            Assert.AreEqual(2, (int)StageState.FailedByTime);
            Assert.AreEqual(3, (int)StageState.FailedByHealth);
            Assert.AreEqual(4, (int)StageState.Abandoned);
        }

        [Test]
        public void Abandoned_Attempt_Counts_But_Pays_Nothing()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            save = SaveDataLogic.ApplyStageResult(
                save,
                new StageResultSummary
                {
                    HasLocation = true,
                    LocationId = 0,
                    Cleared = false,
                    RecoveredEssence = 120,
                    ContractEssence = PauseMenuLogic.GetContractEssence(true, 90),
                    TotalScore = 0,
                    RankLabel = "-"
                });

            Assert.AreEqual(1, save.Stats.Attempts);
            Assert.AreEqual(0, save.Stats.Clears);
            Assert.AreEqual(0, save.Stats.ContractEssence);

            // 계약 정기는 "첫 출근" 업적 보상만 들어온다.
            Assert.AreEqual(AchievementLogic.Get("first_step").Reward, save.ContractEssence);
        }
    }

    public sealed class ControlsCatalogTests
    {
        [Test]
        public void Every_Row_Is_Filled()
        {
            ControlRow[] rows = ControlsCatalog.Build(Core.InputBindingLogic.CreateDefault());

            Assert.GreaterOrEqual(rows.Length, 8);

            foreach (ControlRow row in rows)
            {
                Assert.IsFalse(string.IsNullOrEmpty(row.Group));
                Assert.IsFalse(string.IsNullOrEmpty(row.Action));
                Assert.IsFalse(string.IsNullOrEmpty(row.Keys));
            }
        }

        [Test]
        public void Main_Actions_Are_Listed()
        {
            List<string> keys = new List<string>();

            foreach (ControlRow row in ControlsCatalog.Build(Core.InputBindingLogic.CreateDefault()))
            {
                keys.Add(row.Keys);
            }

            string all = string.Join(" | ", keys);

            StringAssert.Contains("W A S D", all);
            StringAssert.Contains("Shift", all);
            StringAssert.Contains("F", all);
            StringAssert.Contains("마우스 왼쪽", all);
            StringAssert.Contains("마우스 오른쪽", all);
            StringAssert.Contains("Esc", all);
        }

        [Test]
        public void Groups_Are_Contiguous()
        {
            // 조작법 창은 그룹 제목을 바뀔 때만 한 번 찍는다. 같은 그룹은 붙어 있어야 한다.
            HashSet<string> finished = new HashSet<string>();
            string current = null;

            foreach (ControlRow row in ControlsCatalog.Build(Core.InputBindingLogic.CreateDefault()))
            {
                if (row.Group == current)
                {
                    continue;
                }

                Assert.IsFalse(finished.Contains(row.Group), row.Group);

                if (current != null)
                {
                    finished.Add(current);
                }

                current = row.Group;
            }
        }
    }
}
