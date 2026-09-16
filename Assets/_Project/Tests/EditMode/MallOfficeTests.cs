using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class RadioLogicTests
    {
        [Test]
        public void Relays_Only_On_The_Same_Floor_While_Connected()
        {
            Assert.IsTrue(RadioLogic.CanRelay(1, 1, 0f));
            Assert.IsFalse(RadioLogic.CanRelay(1, 2, 0f));
            Assert.IsFalse(RadioLogic.CanRelay(1, 1, 2f), "무전이 끊긴 동안은 모이지 않는다");
        }
    }

    public sealed class ShutterLogicTests
    {
        [Test]
        public void Triggers_Only_From_Alert_Level()
        {
            Assert.IsFalse(ShutterLogic.CanTrigger(AlertLevel.Calm));
            Assert.IsFalse(ShutterLogic.CanTrigger(AlertLevel.Caution));
            Assert.IsTrue(ShutterLogic.CanTrigger(AlertLevel.Alert));
            Assert.IsTrue(ShutterLogic.CanTrigger(AlertLevel.Emergency));
        }

        [Test]
        public void Closed_Shutter_Pushes_Back_To_The_Same_Side()
        {
            Assert.AreEqual(5f + ShutterLogic.BlockHalfWidth, ShutterLogic.PushOut(5f, 5.2f), 0.0001f);
            Assert.AreEqual(5f - ShutterLogic.BlockHalfWidth, ShutterLogic.PushOut(5f, 4.9f), 0.0001f);
            Assert.AreEqual(3f, ShutterLogic.PushOut(5f, 3f));
        }

        [Test]
        public void One_Frame_Of_Dash_Cannot_Pass_Through()
        {
            // 대시 11m/s × 한 프레임(1/30초)이 셔터 반폭보다 짧아야 뚫고 지나가지 못한다.
            Assert.Greater(ShutterLogic.BlockHalfWidth, 11f / 30f);
        }
    }

    public sealed class ManagerLogicTests
    {
        [Test]
        public void Shouts_Only_Near_A_Started_Hypnosis_Outside_Break()
        {
            Assert.IsTrue(ManagerLogic.ShouldShout(0.3f, 3f, false, 0f));
            Assert.IsFalse(ManagerLogic.ShouldShout(0.3f, 5f, false, 0f));
            Assert.IsFalse(ManagerLogic.ShouldShout(0.01f, 3f, false, 0f));
            Assert.IsFalse(ManagerLogic.ShouldShout(0.3f, 3f, true, 0f));
            Assert.IsFalse(ManagerLogic.ShouldShout(0.3f, 3f, false, 1f));
        }

        [Test]
        public void Coffee_Break_Comes_Periodically()
        {
            Assert.IsFalse(ManagerLogic.IsOnBreak(0f));
            Assert.IsFalse(ManagerLogic.IsOnBreak(24f));
            Assert.IsTrue(ManagerLogic.IsOnBreak(26f));
            Assert.IsFalse(ManagerLogic.IsOnBreak(36f));
            Assert.IsTrue(ManagerLogic.IsOnBreak(61f));
        }
    }

    public sealed class MeetingLogicTests
    {
        [Test]
        public void At_Most_Two_Employees_Are_Summoned()
        {
            Assert.AreEqual(0, MeetingLogic.GetSummonCount(0));
            Assert.AreEqual(1, MeetingLogic.GetSummonCount(1));
            Assert.AreEqual(2, MeetingLogic.GetSummonCount(5));
            Assert.AreEqual(0, MeetingLogic.GetSummonCount(-3));
        }

        [Test]
        public void Meetings_End_Before_The_Next_Call()
        {
            Assert.Less(
                MeetingLogic.MeetingSeconds,
                DisruptorCatalog.Get(DisruptorKind.ChiefSecretary).CooldownSeconds);
        }

        [Test]
        public void Visitors_Are_Not_Employees()
        {
            Assert.AreEqual(string.Empty, NpcRoleMark.GetLabel(NpcRole.Employee));
            Assert.IsNotEmpty(NpcRoleMark.GetLabel(NpcRole.Visitor));
            Assert.IsNotEmpty(NpcRoleMark.GetLabel(NpcRole.PassHolder));
            Assert.IsNotEmpty(NpcRoleMark.GetLabel(NpcRole.SecurityRoom));
        }
    }

    public sealed class PassGateLogicTests
    {
        [Test]
        public void Opens_With_A_Pass_Holder_On_The_Floor_And_Stays_Open()
        {
            Assert.IsFalse(PassGateLogic.ShouldOpen(false, true, false));
            Assert.IsFalse(PassGateLogic.ShouldOpen(false, false, true));
            Assert.IsTrue(PassGateLogic.ShouldOpen(false, true, true));
            Assert.IsTrue(PassGateLogic.ShouldOpen(true, false, false));
        }

        [Test]
        public void Emergency_Stairs_Cost_Alert_But_Not_An_Emergency()
        {
            Assert.Greater(PassGateLogic.EmergencyStairAlertRise, 0f);
            Assert.Less(PassGateLogic.EmergencyStairAlertRise, ZoneAlertLogic.EmergencyThreshold);
            Assert.Greater(PassGateLogic.EmergencyConfirmSeconds, 1f);
        }

        [Test]
        public void No_Lock_Without_Gates()
        {
            // 오피스가 아닌 곳(게이트가 하나도 없는 곳)은 계단이 잠기지 않는다.
            Assert.AreEqual(0, PassGate.ActiveCount);
            Assert.IsFalse(PassGate.IsLocked(0));
        }
    }

    public sealed class ClosingAndBlackoutLogicTests
    {
        [Test]
        public void Closing_Is_Announced_At_Seventy_Percent()
        {
            Assert.IsFalse(ClosingLogic.ShouldAnnounce(132f, 190f));
            Assert.IsTrue(ClosingLogic.ShouldAnnounce(133f, 190f));
            Assert.IsFalse(ClosingLogic.ShouldAnnounce(100f, 0f));
            Assert.AreEqual(ClosingLogic.DefaultTimeScale, new Balance.StageBalanceValues().ClosingTimeScale, 0.0001f);
        }

        [Test]
        public void Blackout_Happens_Twice_In_Order()
        {
            Assert.AreEqual(2, BlackoutLogic.TimesPerZone);
            Assert.IsFalse(BlackoutLogic.ShouldStart(0, 69f, 200f));
            Assert.IsTrue(BlackoutLogic.ShouldStart(0, 70f, 200f));
            Assert.IsFalse(BlackoutLogic.ShouldStart(1, 100f, 200f));
            Assert.IsTrue(BlackoutLogic.ShouldStart(1, 140f, 200f));
            Assert.IsFalse(BlackoutLogic.ShouldStart(2, 199f, 200f));
        }

        [Test]
        public void Only_Flashlight_Guards_See_In_The_Dark()
        {
            Assert.IsTrue(BlackoutLogic.CanSee(false, false));
            Assert.IsFalse(BlackoutLogic.CanSee(true, false));
            Assert.IsTrue(BlackoutLogic.CanSee(true, true));

            foreach (DisruptorKind kind in System.Enum.GetValues(typeof(DisruptorKind)))
            {
                Assert.AreEqual(
                    kind == DisruptorKind.NightGuard,
                    DisruptorCatalog.Get(kind).SeesInBlackout,
                    kind.ToString());
            }
        }

        [Test]
        public void Blackout_Is_Over_Before_The_Next_One()
        {
            float timeLimit = LocationCatalog.Get(LocationId.OfficeTower).TimeLimitSeconds;
            float gap = (BlackoutLogic.StartAt[1] - BlackoutLogic.StartAt[0]) * timeLimit;

            Assert.Greater(gap, BlackoutLogic.WarningSeconds + BlackoutLogic.DarkSeconds);
            Assert.Less(BlackoutLogic.RangeMultiplier, 1f);
        }
    }

    public sealed class StealthLogicTests
    {
        [Test]
        public void Undetected_Stealth_Batches_Get_A_Bonus()
        {
            Assert.AreEqual(StealthLogic.UndetectedMultiplier, StealthLogic.GetBatchMultiplier(LocationObjective.Stealth, false));
            Assert.AreEqual(1f, StealthLogic.GetBatchMultiplier(LocationObjective.Stealth, true));
            Assert.AreEqual(1f, StealthLogic.GetBatchMultiplier(LocationObjective.EssenceQuota, false));
        }

        [Test]
        public void Mall_Uses_Stealth_And_Still_Clears_By_Essence()
        {
            Assert.AreEqual(LocationObjective.Stealth, LocationCatalog.Get(LocationId.ShoppingMall).Objective);
            Assert.AreEqual("잠입", LocationCatalog.GetObjectiveLabel(LocationObjective.Stealth));
            Assert.IsNotEmpty(LocationObjectiveLogic.GetHint(LocationObjective.Stealth));

            Assert.AreEqual(
                StageState.Cleared,
                ObjectiveStateLogic.Resolve(LocationObjective.Stealth, 10f, 170, 170, 5, 0, 3, 0));
        }

        [Test]
        public void Pantry_Doubles_Contest_Only_In_The_Office()
        {
            Assert.AreEqual(2f, MallOfficeValues.GetPantryMultiplier(-5f, -6f, true));
            Assert.AreEqual(1f, MallOfficeValues.GetPantryMultiplier(0f, -6f, true));
            Assert.AreEqual(1f, MallOfficeValues.GetPantryMultiplier(-6f, -6f, false));
        }
    }

    public sealed class MallOfficePlacementTests
    {
        [Test]
        public void Mall_Has_Guards_Cameras_Promo_And_Chief()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.ShoppingMall, 3);

            for (int floor = 0; floor < 3; floor++)
            {
                Assert.GreaterOrEqual(placements.FindAll(p => p.Floor == floor && p.Kind == DisruptorKind.SecurityGuard).Count, 2, $"{floor + 1}F");
                Assert.AreEqual(MallLayout.GetCameraX(floor).Length, placements.FindAll(p => p.Floor == floor && p.Kind == DisruptorKind.SecurityCamera).Count, $"{floor + 1}F");
            }

            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.PromoStaff).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.SecurityChief).Count);
        }

        [Test]
        public void Office_Has_A_Manager_Per_Floor_And_The_Secretary_On_Top()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.OfficeTower, 3);

            for (int floor = 0; floor < 3; floor++)
            {
                Assert.IsTrue(placements.Exists(p => p.Floor == floor && p.Kind == DisruptorKind.OfficeManager), $"{floor + 1}F");
            }

            Assert.IsTrue(placements.Exists(p => p.Kind == DisruptorKind.NightGuard));
            Assert.IsTrue(placements.Exists(p => p.Kind == DisruptorKind.OfficeRomeo));

            DisruptorPlacement secretary = placements.Find(p => p.Kind == DisruptorKind.ChiefSecretary);

            Assert.AreEqual(2, secretary.Floor);
        }

        [Test]
        public void Cameras_Are_Objects_And_Everything_Else_On_The_Mall_Is_A_Person()
        {
            Assert.IsTrue(DisruptorCatalog.Get(DisruptorKind.SecurityCamera).IsObject);
            Assert.IsFalse(DisruptorCatalog.Get(DisruptorKind.SecurityGuard).IsObject);
            Assert.IsFalse(DisruptorCatalog.Get(DisruptorKind.SecurityChief).IsObject);
        }

        [Test]
        public void New_Specials_Have_Telegraphs()
        {
            foreach (DisruptorKind kind in new[] { DisruptorKind.SecurityChief, DisruptorKind.ChiefSecretary })
            {
                DisruptorProfile profile = DisruptorCatalog.Get(kind);

                Assert.IsTrue(profile.IsSpecial, kind.ToString());
                Assert.GreaterOrEqual(profile.TelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds, kind.ToString());
                Assert.Greater(profile.CooldownSeconds, 0f, kind.ToString());
            }
        }

        [Test]
        public void Reinforcements_Match_The_Location()
        {
            Assert.AreEqual(DisruptorKind.SecurityGuard, DisruptorCatalog.GetReinforcementKind(LocationId.ShoppingMall));
            Assert.AreEqual(DisruptorKind.NightGuard, DisruptorCatalog.GetReinforcementKind(LocationId.OfficeTower));
        }

        [Test]
        public void Layout_Stays_Clear_Of_Stairs_And_Recovery()
        {
            List<float> xs = new List<float>(MallLayout.ShutterX);
            xs.AddRange(MallLayout.PillarX);
            xs.AddRange(OfficeLayout.PantryX);
            xs.AddRange(OfficeLayout.MeetingRoomX);
            xs.Add(MallLayout.SecurityRoomX);
            xs.Add(OfficeLayout.PassHolderX);
            xs.Add(OfficeLayout.ExecutiveX);

            foreach (float x in xs)
            {
                Assert.Greater(System.Math.Abs(x - FloorLayout.UpStairX), 1.5f, x.ToString());
                Assert.Greater(System.Math.Abs(x - FloorLayout.DownStairX), 1.5f, x.ToString());
                Assert.Greater(System.Math.Abs(x - FloorLayout.RecoveryX), 2f, x.ToString());
            }

            // 게이트는 위층 계단 바로 앞, 회수 지점보다 안쪽이다.
            Assert.Less(OfficeLayout.GateX, FloorLayout.UpStairX);
            Assert.Greater(OfficeLayout.GateX, OfficeLayout.ExecutiveX);
        }

        [Test]
        public void Every_Mall_And_Office_Floor_Has_Layout_Data()
        {
            Assert.AreEqual(LocationCatalog.Get(LocationId.ShoppingMall).FloorCount, MallLayout.CameraX.Length);
            Assert.AreEqual(LocationCatalog.Get(LocationId.OfficeTower).FloorCount, OfficeLayout.PantryX.Length);
            Assert.AreEqual(LocationCatalog.Get(LocationId.OfficeTower).FloorCount, OfficeLayout.MeetingRoomX.Length);
        }
    }
}
