using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ZoneAlertLogicTests
    {
        [Test]
        public void Levels_Change_Exactly_At_The_Thresholds()
        {
            Assert.AreEqual(AlertLevel.Calm, ZoneAlertLogic.GetLevel(0f));
            Assert.AreEqual(AlertLevel.Calm, ZoneAlertLogic.GetLevel(29.9f));
            Assert.AreEqual(AlertLevel.Caution, ZoneAlertLogic.GetLevel(30f));
            Assert.AreEqual(AlertLevel.Caution, ZoneAlertLogic.GetLevel(59.9f));
            Assert.AreEqual(AlertLevel.Alert, ZoneAlertLogic.GetLevel(60f));
            Assert.AreEqual(AlertLevel.Alert, ZoneAlertLogic.GetLevel(89.9f));
            Assert.AreEqual(AlertLevel.Emergency, ZoneAlertLogic.GetLevel(90f));
            Assert.AreEqual(AlertLevel.Emergency, ZoneAlertLogic.GetLevel(100f));
        }

        [Test]
        public void Value_Stays_Between_Zero_And_Maximum()
        {
            Assert.AreEqual(100f, ZoneAlertLogic.Add(95f, 30f), 0.0001f);
            Assert.AreEqual(0f, ZoneAlertLogic.Add(10f, -50f), 0.0001f);
            Assert.AreEqual(40f, ZoneAlertLogic.Add(40f, float.NaN), 0.0001f);
        }

        [Test]
        public void Alert_Decays_Over_Time_But_Not_Below_Zero()
        {
            Assert.AreEqual(36f, ZoneAlertLogic.Decay(40f, 4f, 1f), 0.0001f);
            Assert.AreEqual(0f, ZoneAlertLogic.Decay(2f, 4f, 1f), 0.0001f);

            // 멈춘 시간(카드 화면)에는 줄지 않는다.
            Assert.AreEqual(40f, ZoneAlertLogic.Decay(40f, 4f, 0f), 0.0001f);
        }

        [Test]
        public void Reinforcements_Are_Limited_Per_Zone()
        {
            Assert.IsTrue(ZoneAlertLogic.CanReinforce(0));
            Assert.IsTrue(ZoneAlertLogic.CanReinforce(1));
            Assert.IsFalse(ZoneAlertLogic.CanReinforce(ZoneAlertLogic.MaximumReinforcements));
        }

        [Test]
        public void After_Emergency_The_Zone_Can_Escalate_Again()
        {
            // 비상 뒤 내려가는 값이 비상 아래여야 두 번째 비상이 가능하다.
            Assert.Less(ZoneAlertLogic.AfterEmergencyValue, ZoneAlertLogic.EmergencyThreshold);
            Assert.GreaterOrEqual(ZoneAlertLogic.AfterEmergencyValue, ZoneAlertLogic.CautionThreshold);
        }

        [Test]
        public void Level_Effects_Start_At_The_Documented_Stages()
        {
            Assert.AreEqual(1f, ZoneAlertLogic.GetHypnosisMultiplier(AlertLevel.Calm), 0.0001f);
            Assert.Less(ZoneAlertLogic.GetHypnosisMultiplier(AlertLevel.Caution), 1f);

            Assert.AreEqual(1f, ZoneAlertLogic.GetPatrolSpeedMultiplier(AlertLevel.Calm), 0.0001f);
            Assert.Greater(ZoneAlertLogic.GetPatrolSpeedMultiplier(AlertLevel.Caution), 1f);

            Assert.AreEqual(1f, ZoneAlertLogic.GetAbilityCooldownMultiplier(AlertLevel.Caution), 0.0001f);
            Assert.Less(ZoneAlertLogic.GetAbilityCooldownMultiplier(AlertLevel.Alert), 1f);
        }
    }

    public sealed class DetectionLogicTests
    {
        [Test]
        public void Target_In_Front_And_In_Range_Is_Seen()
        {
            Assert.IsTrue(DetectionLogic.IsInSight(0f, 0f, 1, 4f, 0.5f, 30f, 6f));
            Assert.IsTrue(DetectionLogic.IsInSight(0f, 0f, -1, -4f, 0.5f, 30f, 6f));
        }

        [Test]
        public void Target_Behind_Is_Not_Seen()
        {
            Assert.IsFalse(DetectionLogic.IsInSight(0f, 0f, 1, -2f, 0f, 30f, 6f));
            Assert.IsFalse(DetectionLogic.IsInSight(0f, 0f, -1, 2f, 0f, 30f, 6f));
        }

        [Test]
        public void Target_Outside_Range_Or_Angle_Is_Not_Seen()
        {
            Assert.IsFalse(DetectionLogic.IsInSight(0f, 0f, 1, 6.1f, 0f, 30f, 6f));
            Assert.IsTrue(DetectionLogic.IsInSight(0f, 0f, 1, 6f, 0f, 30f, 6f));

            // 30° 부채꼴 절반 기준: 가로 2, 세로 2는 45°라 밖이다.
            Assert.IsFalse(DetectionLogic.IsInSight(0f, 0f, 1, 2f, 2f, 30f, 6f));
            Assert.IsTrue(DetectionLogic.IsInSight(0f, 0f, 1, 2f, 2f, 45f, 6f));
        }

        [Test]
        public void Zero_Range_Sees_Nothing()
        {
            Assert.IsFalse(DetectionLogic.IsInSight(0f, 0f, 1, 0.5f, 0f, 30f, 0f));
        }

        [Test]
        public void Spotting_Takes_About_The_Documented_Time()
        {
            float progress = 0f;

            for (int i = 0; i < 7; i++)
            {
                progress = DetectionLogic.Advance(progress, true, 0, 0.1f);
            }

            Assert.Less(progress, 1f, "0.7초 만에 발각되면 안 됩니다");

            for (int i = 0; i < 2; i++)
            {
                progress = DetectionLogic.Advance(progress, true, 0, 0.1f);
            }

            Assert.AreEqual(1f, progress, 0.0001f, "0.9초면 발각되어야 합니다");
        }

        [Test]
        public void More_Followers_Are_Spotted_Faster()
        {
            float alone = DetectionLogic.Advance(0f, true, 0, 0.2f);
            float crowd = DetectionLogic.Advance(0f, true, 6, 0.2f);

            Assert.Greater(crowd, alone);
        }

        [Test]
        public void Suspicion_Fades_When_Nothing_Suspicious_Is_Seen()
        {
            Assert.AreEqual(0.5f, DetectionLogic.Advance(0.9f, false, 0, 0.16f), 0.0001f);
            Assert.AreEqual(0f, DetectionLogic.Advance(0.1f, false, 0, 1f), 0.0001f);
        }
    }

    public sealed class SpecialAbilityLogicTests
    {
        [Test]
        public void Telegraph_Can_Never_Be_Shorter_Than_One_Second()
        {
            Assert.AreEqual(1f, SpecialAbilityLogic.SanitizeTelegraph(0f), 0.0001f);
            Assert.AreEqual(1f, SpecialAbilityLogic.SanitizeTelegraph(-3f), 0.0001f);
            Assert.AreEqual(1f, SpecialAbilityLogic.SanitizeTelegraph(float.NaN), 0.0001f);
            Assert.AreEqual(1.5f, SpecialAbilityLogic.SanitizeTelegraph(1.5f), 0.0001f);
        }

        [Test]
        public void Ability_Goes_Ready_Telegraph_Fire_Cooldown_Ready()
        {
            AbilityTimer timer = new AbilityTimer();

            // 조건이 없으면 대기
            Assert.IsFalse(SpecialAbilityLogic.Tick(ref timer, 0.1f, false, false, 1.2f, 5f, AlertLevel.Calm));
            Assert.AreEqual(AbilityPhase.Ready, timer.Phase);

            // 조건이 생기면 예고
            Assert.IsFalse(SpecialAbilityLogic.Tick(ref timer, 0.1f, false, true, 1.2f, 5f, AlertLevel.Calm));
            Assert.AreEqual(AbilityPhase.Telegraph, timer.Phase);

            // 예고 도중에는 발동하지 않는다
            Assert.IsFalse(SpecialAbilityLogic.Tick(ref timer, 1.1f, false, false, 1.2f, 5f, AlertLevel.Calm));

            // 예고가 끝나는 프레임에 한 번 발동
            Assert.IsTrue(SpecialAbilityLogic.Tick(ref timer, 0.2f, false, false, 1.2f, 5f, AlertLevel.Calm));
            Assert.AreEqual(AbilityPhase.Cooldown, timer.Phase);
            Assert.AreEqual(5f, timer.Remaining, 0.0001f);

            // 재사용 대기가 끝나면 다시 대기
            Assert.IsFalse(SpecialAbilityLogic.Tick(ref timer, 5f, false, true, 1.2f, 5f, AlertLevel.Calm));
            Assert.AreEqual(AbilityPhase.Ready, timer.Phase);
        }

        [Test]
        public void Stun_Freezes_Telegraph_And_Cooldown()
        {
            AbilityTimer timer = new AbilityTimer { Phase = AbilityPhase.Telegraph, Remaining = 0.5f };

            Assert.IsFalse(SpecialAbilityLogic.Tick(ref timer, 3f, true, false, 1.2f, 5f, AlertLevel.Calm));
            Assert.AreEqual(AbilityPhase.Telegraph, timer.Phase);
            Assert.AreEqual(0.5f, timer.Remaining, 0.0001f);
        }

        [Test]
        public void Alert_Stage_Shortens_Cooldown()
        {
            AbilityTimer calm = new AbilityTimer { Phase = AbilityPhase.Telegraph, Remaining = 0.1f };
            AbilityTimer alert = calm;

            SpecialAbilityLogic.Tick(ref calm, 0.2f, false, false, 1.2f, 10f, AlertLevel.Calm);
            SpecialAbilityLogic.Tick(ref alert, 0.2f, false, false, 1.2f, 10f, AlertLevel.Alert);

            Assert.AreEqual(10f, calm.Remaining, 0.0001f);
            Assert.AreEqual(7f, alert.Remaining, 0.0001f);
        }

        [Test]
        public void Telegraph_Progress_Fills_From_Zero_To_One()
        {
            Assert.AreEqual(0f, SpecialAbilityLogic.GetTelegraphProgress(new AbilityTimer(), 2f), 0.0001f);

            AbilityTimer half = new AbilityTimer { Phase = AbilityPhase.Telegraph, Remaining = 1f };

            Assert.AreEqual(0.5f, SpecialAbilityLogic.GetTelegraphProgress(half, 2f), 0.0001f);
        }
    }

    public sealed class BreakTimeLogicTests
    {
        [Test]
        public void No_Bell_Right_After_The_Zone_Starts()
        {
            Assert.IsFalse(BreakTimeLogic.IsBreak(0f));
            Assert.IsFalse(BreakTimeLogic.IsBreak(BreakTimeLogic.PeriodSeconds - 0.1f));
        }

        [Test]
        public void Break_Repeats_Every_Period()
        {
            float p = BreakTimeLogic.PeriodSeconds;
            float b = BreakTimeLogic.BreakSeconds;

            Assert.IsTrue(BreakTimeLogic.IsBreak(p));
            Assert.IsTrue(BreakTimeLogic.IsBreak(p + b - 0.1f));
            Assert.IsFalse(BreakTimeLogic.IsBreak(p + b + 0.1f));
            Assert.IsTrue(BreakTimeLogic.IsBreak(p * 2f + 1f));
        }

        [Test]
        public void Npcs_Speed_Up_Only_During_Break()
        {
            Assert.AreEqual(1f, BreakTimeLogic.GetNpcSpeedMultiplier(10f), 0.0001f);
            Assert.Greater(BreakTimeLogic.GetNpcSpeedMultiplier(BreakTimeLogic.PeriodSeconds + 1f), 1f);
        }
    }

    public sealed class DisruptorCatalogTests
    {
        [Test]
        public void Every_Kind_Has_A_Sensible_Profile()
        {
            foreach (DisruptorKind kind in System.Enum.GetValues(typeof(DisruptorKind)))
            {
                DisruptorProfile profile = DisruptorCatalog.Get(kind);

                Assert.AreEqual(kind, profile.Kind, kind.ToString());
                Assert.IsFalse(string.IsNullOrEmpty(profile.DisplayName), kind.ToString());
                Assert.GreaterOrEqual(profile.MoveSpeed, 0f, kind.ToString());

                // 시야가 있으면 거리와 각도가 모두 있어야 한다. 촬영팀 · 쟁탈 · 길막 · 도둑은 부채꼴을 쓰지 않는다 (23일차).
                Assert.AreEqual(profile.SightRange > 0f, profile.SightHalfAngle > 0f, kind.ToString());

                if (profile.Role == DisruptorRole.Watcher &&
                    profile.Ability != SpecialAbilityKind.LiveBroadcast)
                {
                    Assert.Greater(profile.SightRange, 0f, kind.ToString());
                }

                // 움직이는 개체는 속도가 있어야 한다.
                if (!profile.Stationary)
                {
                    Assert.Greater(profile.MoveSpeed, 0f, kind.ToString());
                }

                // 특수 개체는 능력이 있어야 하고, 일반 개체는 능력이 없어야 한다.
                Assert.AreEqual(profile.IsSpecial, profile.Ability != SpecialAbilityKind.None, kind.ToString());
            }
        }

        [Test]
        public void Training_Center_Has_An_Assistant_On_Every_Floor()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.TrainingCenter, 4);

            for (int floor = 0; floor < 4; floor++)
            {
                Assert.IsTrue(
                    placements.Exists(p => p.Floor == floor && p.Kind == DisruptorKind.TrainingAssistant),
                    $"{floor + 1}F에 교육 조교가 없습니다");
            }
        }

        [Test]
        public void Tutorial_Floor_Has_No_Special_Disruptor()
        {
            // 1F · 2F는 기본을 배우는 층이다. 특수 능력은 3F부터 나온다 (부록 C.3 [0]).
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.TrainingCenter, 4);

            Assert.IsFalse(placements.Exists(p => p.Floor < 2 && DisruptorCatalog.Get(p.Kind).IsSpecial));
            Assert.IsTrue(placements.Exists(p => p.Floor >= 2 && DisruptorCatalog.Get(p.Kind).IsSpecial));
        }

        [Test]
        public void Placements_Stay_Inside_The_Building()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                foreach (DisruptorPlacement placement in DisruptorCatalog.GetPlacements(location.Id, location.FloorCount))
                {
                    Assert.Less(placement.Floor, location.FloorCount, location.Id.ToString());
                    Assert.GreaterOrEqual(placement.Floor, 0, location.Id.ToString());
                    Assert.Less(System.Math.Abs(placement.X), ProjectTheta.Stage.FloorSpace.WalkMaxX, location.Id.ToString());
                }
            }
        }
    }

    public sealed class AttendanceMarkRuleTests
    {
        [Test]
        public void Mark_Hurts_But_Does_Not_Instantly_Break_A_Follower()
        {
            // 표식 10초 동안 가까이 둔 동행자가 잃는 유지도는 절반 미만이어야 한다(최대 100).
            float nearbyLoss = AttendanceMark.DrainPerSecond * AttendanceCheckAbility.MarkSeconds;

            Assert.Greater(nearbyLoss, 0f);
            Assert.Less(nearbyLoss, 50f);

            Assert.Greater(AttendanceMark.EssencePenalty, 0f);
            Assert.Less(AttendanceMark.EssencePenalty, 1f);
        }
    }
}
