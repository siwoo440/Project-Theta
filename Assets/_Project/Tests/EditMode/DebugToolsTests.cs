using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Run;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class DebugSpeedTests
    {
        [Test]
        public void Debug_Speed_Multiplies_Normal_Play()
        {
            Assert.AreEqual(2f, TimeScaleLogic.Resolve(0, 0f, 0.05f, 2f), 0.0001f);
            Assert.AreEqual(0.25f, TimeScaleLogic.Resolve(0, 0f, 0.05f, 0.25f), 0.0001f);
        }

        [Test]
        public void Card_Screen_Stays_Paused_At_Any_Debug_Speed()
        {
            // 속도를 올려도 카드 화면이 풀리면 안 된다.
            Assert.AreEqual(0f, TimeScaleLogic.Resolve(1, 0f, 0.05f, 4f), 0.0001f);
        }

        [Test]
        public void Debug_Speed_Also_Scales_Hit_Stop()
        {
            Assert.AreEqual(0.1f, TimeScaleLogic.Resolve(0, 0.06f, 0.05f, 2f), 0.0001f);
        }

        [Test]
        public void Debug_Speed_Is_Clamped()
        {
            Assert.AreEqual(TimeScaleLogic.MaximumDebugSpeed, TimeScaleLogic.ClampDebugSpeed(100f), 0.0001f);
            Assert.AreEqual(TimeScaleLogic.MinimumDebugSpeed, TimeScaleLogic.ClampDebugSpeed(0f), 0.0001f);
            Assert.AreEqual(TimeScaleLogic.MinimumDebugSpeed, TimeScaleLogic.ClampDebugSpeed(-1f), 0.0001f);
            Assert.AreEqual(1f, TimeScaleLogic.ClampDebugSpeed(float.NaN), 0.0001f);
        }
    }

    public sealed class BalanceTuningTests
    {
        [SetUp]
        public void Clean()
        {
            BalanceBootstrap.Reset();
        }

        [TearDown]
        public void LeaveClean()
        {
            BalanceBootstrap.Reset();
        }

        private static TuningParameter Find(
            string id)
        {
            foreach (TuningParameter parameter in BalanceTuningCatalog.All)
            {
                if (parameter.Id == id)
                {
                    return parameter;
                }
            }

            Assert.Fail($"{id} 수치가 목록에 없습니다");

            return null;
        }

        [Test]
        public void Every_Parameter_Points_At_A_Real_Field()
        {
            HashSet<string> seen = new HashSet<string>();

            foreach (TuningParameter parameter in BalanceTuningCatalog.All)
            {
                Assert.IsNotNull(
                    typeof(StageBalanceValues).GetField(parameter.Id),
                    $"{parameter.Id} 필드가 없습니다");

                Assert.IsTrue(
                    seen.Add(parameter.Id),
                    $"{parameter.Id}가 두 번 들어 있습니다");
            }
        }

        [Test]
        public void Every_Parameter_Reads_And_Writes_Its_Own_Field()
        {
            // 목록을 복사해 붙이다 읽기와 쓰기가 다른 필드를 가리키면 잡는다.
            foreach (TuningParameter parameter in BalanceTuningCatalog.All)
            {
                StageBalanceValues values = new StageBalanceValues();
                FieldInfo field = typeof(StageBalanceValues).GetField(parameter.Id);

                float target = parameter.Sanitize(parameter.Maximum);

                parameter.Write(values, target);

                float stored = Convert.ToSingle(field.GetValue(values));

                Assert.AreEqual(target, stored, 0.0001f, parameter.Id);
                Assert.AreEqual(target, parameter.Read(values), 0.0001f, parameter.Id);
            }
        }

        [Test]
        public void Default_Values_Fit_Inside_Slider_Range()
        {
            StageBalanceValues defaults = new StageBalanceValues();

            foreach (TuningParameter parameter in BalanceTuningCatalog.All)
            {
                float value = parameter.Read(defaults);

                Assert.GreaterOrEqual(value, parameter.Minimum, parameter.Id);
                Assert.LessOrEqual(value, parameter.Maximum, parameter.Id);
            }
        }

        [Test]
        public void Integer_Parameters_Round_And_Clamp()
        {
            TuningParameter xp = Find(nameof(StageBalanceValues.LevelBaseXp));

            Assert.AreEqual(61f, xp.Sanitize(60.6f), 0.0001f);
            Assert.AreEqual(xp.Maximum, xp.Sanitize(99999f), 0.0001f);
            Assert.AreEqual(xp.Minimum, xp.Sanitize(-5f), 0.0001f);
        }

        [Test]
        public void Editing_Never_Touches_The_Asset_Values()
        {
            // 자산 주입을 흉내 낸다.
            StageBalanceValues asset = new StageBalanceValues();
            BalanceOverrides.Stage = asset;

            TuningParameter scale = Find(nameof(StageBalanceValues.PlayerHypnosisSpeedScale));

            BalanceTuningSession.Set(scale, 2.5f);

            Assert.AreEqual(1.3f, asset.PlayerHypnosisSpeedScale, 0.0001f, "자산 원본이 바뀌었습니다");
            Assert.AreNotSame(asset, BalanceOverrides.Stage);
            Assert.AreEqual(2.5f, BalanceOverrides.StageOrDefault.PlayerHypnosisSpeedScale, 0.0001f);
            Assert.IsTrue(BalanceTuningSession.IsModified(scale));
            Assert.AreEqual(1, BalanceTuningSession.CountModified());
        }

        [Test]
        public void Editing_Without_Asset_Never_Touches_Code_Defaults()
        {
            TuningParameter scale = Find(nameof(StageBalanceValues.PlayerHypnosisSpeedScale));

            BalanceTuningSession.Set(scale, 2f);
            BalanceTuningSession.ResetAll();

            Assert.IsNull(BalanceOverrides.Stage, "자산이 없던 상태로 돌아가야 합니다");
            Assert.AreEqual(1.3f, BalanceOverrides.StageOrDefault.PlayerHypnosisSpeedScale, 0.0001f);
            Assert.IsFalse(BalanceTuningSession.IsEditing);
        }

        [Test]
        public void Setting_The_Same_Value_Does_Not_Start_Editing()
        {
            TuningParameter scale = Find(nameof(StageBalanceValues.PlayerHypnosisSpeedScale));

            BalanceTuningSession.Set(scale, 1.3f);

            Assert.IsFalse(BalanceTuningSession.IsEditing);
            Assert.IsNull(BalanceOverrides.Stage);
        }

        [Test]
        public void Reset_All_Returns_To_The_Asset()
        {
            StageBalanceValues asset = new StageBalanceValues();
            BalanceOverrides.Stage = asset;

            BalanceTuningSession.Set(Find(nameof(StageBalanceValues.ChainRadius)), 5f);
            BalanceTuningSession.Set(Find(nameof(StageBalanceValues.LevelBaseXp)), 200f);

            Assert.AreEqual(2, BalanceTuningSession.CountModified());

            BalanceTuningSession.ResetAll();

            Assert.AreSame(asset, BalanceOverrides.Stage);
            Assert.AreEqual(0, BalanceTuningSession.CountModified());
        }

        [Test]
        public void Reset_Group_Only_Restores_That_Group()
        {
            TuningParameter chain = Find(nameof(StageBalanceValues.ChainRadius));
            TuningParameter xp = Find(nameof(StageBalanceValues.LevelBaseXp));

            BalanceTuningSession.Set(chain, 5f);
            BalanceTuningSession.Set(xp, 200f);

            BalanceTuningSession.ResetGroup(chain.Group);

            Assert.IsFalse(BalanceTuningSession.IsModified(chain));
            Assert.IsTrue(BalanceTuningSession.IsModified(xp));
            Assert.AreEqual(200f, BalanceTuningSession.Get(xp), 0.0001f);
        }

        [Test]
        public void Commit_Makes_Saved_Values_The_New_Baseline()
        {
            TuningParameter scale = Find(nameof(StageBalanceValues.PlayerHypnosisSpeedScale));

            BalanceTuningSession.Set(scale, 2f);

            StageBalanceValues saved = BalanceOverrides.StageOrDefault.Clone();

            BalanceTuningSession.CommitAsBaseline(saved);

            Assert.AreSame(saved, BalanceOverrides.Stage);
            Assert.IsFalse(BalanceTuningSession.IsModified(scale));
            Assert.AreEqual(2f, BalanceTuningSession.Get(scale), 0.0001f);
        }

        [Test]
        public void Bootstrap_Reset_Forgets_Editing()
        {
            BalanceTuningSession.Set(Find(nameof(StageBalanceValues.ChainRadius)), 5f);

            BalanceBootstrap.Reset();

            Assert.IsFalse(BalanceTuningSession.IsEditing);
            Assert.IsNull(BalanceOverrides.Stage);
        }

        [Test]
        public void Clone_Copies_Every_Field()
        {
            // 새 수치를 추가하고 Clone에 빠뜨리면, 디버그 패널로 바꾸는 순간 그 값이 기본값으로 튄다.
            StageBalanceValues original = new StageBalanceValues();

            foreach (FieldInfo field in typeof(StageBalanceValues).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(float))
                {
                    field.SetValue(original, (float)field.GetValue(original) + 7.25f);
                }
                else if (field.FieldType == typeof(int))
                {
                    field.SetValue(original, (int)field.GetValue(original) + 7);
                }
            }

            StageBalanceValues copy = original.Clone();

            foreach (FieldInfo field in typeof(StageBalanceValues).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(float[]))
                {
                    CollectionAssert.AreEqual((float[])field.GetValue(original), (float[])field.GetValue(copy), field.Name);
                    Assert.AreNotSame(field.GetValue(original), field.GetValue(copy), field.Name + " 배열은 따로 복사되어야 합니다");
                }
                else
                {
                    Assert.AreEqual(field.GetValue(original), field.GetValue(copy), field.Name);
                }
            }
        }

        [Test]
        public void Moved_Focus_Values_Keep_Their_Old_Defaults()
        {
            // 20일차에 PlayerFocus 안에서 자산으로 옮긴 값이다. 옮기면서 밸런스가 바뀌면 안 된다.
            StageBalanceValues v = new StageBalanceValues();

            Assert.AreEqual(6f, v.FocusHypnosisDrainPerSecond, 0.0001f);
            Assert.AreEqual(14f, v.FocusRecoveryPerSecond, 0.0001f);
            Assert.AreEqual(1f, v.ImpulseBuildScale, 0.0001f);
        }
    }

    public sealed class RunStatsTests
    {
        [Test]
        public void Time_Accumulates_Per_Floor()
        {
            RunStats stats = new RunStats(4);

            stats.Tick(10f, 0, false);
            stats.Tick(5f, 1, false);
            stats.Tick(3f, 0, false);

            Assert.AreEqual(13f, stats.FloorSeconds[0], 0.0001f);
            Assert.AreEqual(5f, stats.FloorSeconds[1], 0.0001f);
            Assert.AreEqual(18f, stats.TotalSeconds, 0.0001f);
            Assert.AreEqual(1, stats.HighestFloor);
        }

        [Test]
        public void Paused_Or_Broken_Time_Is_Ignored()
        {
            RunStats stats = new RunStats(2);

            stats.Tick(0f, 0, true);
            stats.Tick(-1f, 0, true);
            stats.Tick(float.NaN, 0, true);

            Assert.AreEqual(0f, stats.TotalSeconds, 0.0001f);
            Assert.AreEqual(0f, stats.FocusEmptySeconds, 0.0001f);
        }

        [Test]
        public void Unknown_Floor_Counts_Only_Toward_Total()
        {
            RunStats stats = new RunStats(2);

            stats.Tick(4f, 7, false);
            stats.Tick(4f, -1, false);

            Assert.AreEqual(8f, stats.TotalSeconds, 0.0001f);
            Assert.AreEqual(0f, stats.FloorSeconds[0] + stats.FloorSeconds[1], 0.0001f);
        }

        [Test]
        public void Focus_Empty_Ratio_Handles_Zero_Time()
        {
            RunStats stats = new RunStats(1);

            Assert.AreEqual(0f, stats.FocusEmptyRatio, 0.0001f);

            stats.Tick(6f, 0, false);
            stats.Tick(2f, 0, true);

            Assert.AreEqual(0.25f, stats.FocusEmptyRatio, 0.0001f);
        }

        [Test]
        public void Floor_Share_Is_Relative_To_The_Longest_Floor()
        {
            RunStats stats = new RunStats(3);

            Assert.AreEqual(0f, stats.GetFloorShare(0), 0.0001f);

            stats.Tick(20f, 0, false);
            stats.Tick(5f, 1, false);

            Assert.AreEqual(1f, stats.GetFloorShare(0), 0.0001f);
            Assert.AreEqual(0.25f, stats.GetFloorShare(1), 0.0001f);
            Assert.AreEqual(0f, stats.GetFloorShare(2), 0.0001f);
            Assert.AreEqual(0f, stats.GetFloorShare(9), 0.0001f);
        }

        [Test]
        public void Level_Ups_Are_Recorded_Once_Per_Level()
        {
            RunStats stats = new RunStats(1);

            stats.Tick(30f, 0, false);
            stats.RecordLevel(2);

            stats.Tick(30f, 0, false);

            // 한 번에 두 레벨이 올라도 레벨마다 한 줄씩 남는다.
            stats.RecordLevel(4);

            // 같은 레벨을 다시 알려도 늘지 않는다.
            stats.RecordLevel(4);

            Assert.AreEqual(3, stats.LevelUpTimes.Count);
            Assert.AreEqual(30f, stats.LevelUpTimes[0], 0.0001f);
            Assert.AreEqual(60f, stats.LevelUpTimes[1], 0.0001f);
            Assert.AreEqual(60f, stats.LevelUpTimes[2], 0.0001f);
        }

        [Test]
        public void Counts_Split_Reclaims_And_Recoveries()
        {
            RunStats stats = new RunStats(1);

            stats.RecordHypnosis(false);
            stats.RecordHypnosis(true);
            stats.RecordRecovery(3, 120);
            stats.RecordRecovery(-2, -10);

            Assert.AreEqual(2, stats.HypnosisCount);
            Assert.AreEqual(1, stats.ReclaimCount);
            Assert.AreEqual(3, stats.RecoveredFollowers);
            Assert.AreEqual(120, stats.RecoveredEssence);
        }

        [Test]
        public void Time_Is_Formatted_As_Minutes_And_Seconds()
        {
            Assert.AreEqual("00:00", RunStats.FormatTime(0f));
            Assert.AreEqual("01:05", RunStats.FormatTime(65.9f));
            Assert.AreEqual("00:00", RunStats.FormatTime(-3f));
        }
    }

    public sealed class DebugCheatsTests
    {
        [TearDown]
        public void LeaveClean()
        {
            DebugCheats.ResetForRun();
        }

        [Test]
        public void Turning_A_Cheat_On_Marks_The_Run()
        {
            DebugCheats.ResetForRun();

            DebugCheats.SetInvincible(false);
            Assert.IsFalse(DebugCheats.UsedThisRun, "끄기만 한 것은 치트 사용이 아닙니다");

            DebugCheats.SetInfiniteFocus(true);
            Assert.IsTrue(DebugCheats.UsedThisRun);
        }

        [Test]
        public void New_Run_Turns_Every_Cheat_Off()
        {
            DebugCheats.SetInvincible(true);
            DebugCheats.SetInfiniteFocus(true);

            DebugCheats.ResetForRun();

            Assert.IsFalse(DebugCheats.Invincible);
            Assert.IsFalse(DebugCheats.InfiniteFocus);
            Assert.IsFalse(DebugCheats.UsedThisRun);
        }
    }
}
