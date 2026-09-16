using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ClaimLogicTests
    {
        [Test]
        public void Gauge_Fills_In_About_Seven_Seconds_While_Talking()
        {
            float gauge = 0f;

            for (int i = 0; i < 66; i++)
            {
                gauge = ClaimLogic.Advance(gauge, true, false, 1f, 0.1f);
            }

            Assert.IsFalse(ClaimLogic.IsComplete(gauge), "6.6초 만에 차면 대처할 틈이 없습니다");

            for (int i = 0; i < 2; i++)
            {
                gauge = ClaimLogic.Advance(gauge, true, false, 1f, 0.1f);
            }

            Assert.IsTrue(ClaimLogic.IsComplete(gauge));
        }

        [Test]
        public void Guarding_Drains_Even_While_Talking()
        {
            float gauge = ClaimLogic.Advance(50f, true, true, 1f, 1f);

            Assert.Less(gauge, 50f);
        }

        [Test]
        public void Gauge_Fades_When_Out_Of_Reach()
        {
            Assert.Less(ClaimLogic.Advance(50f, false, false, 1f, 1f), 50f);
            Assert.AreEqual(0f, ClaimLogic.Advance(1f, false, false, 1f, 1f));
        }

        [Test]
        public void Rise_Scale_Zero_Stops_Claiming()
        {
            Assert.AreEqual(40f, ClaimLogic.Advance(40f, true, false, 0f, 1f));
        }

        [Test]
        public void Gauge_Stays_In_Range()
        {
            Assert.AreEqual(ClaimLogic.Maximum, ClaimLogic.Advance(99f, true, false, 10f, 10f));
            Assert.AreEqual(0f, ClaimLogic.Advance(float.NaN, true, false, 1f, 1f));
        }

        [Test]
        public void Pair_Targets_Different_Followers_From_The_Back()
        {
            Assert.AreEqual(4, ClaimLogic.PickTargetIndex(5, 0));
            Assert.AreEqual(3, ClaimLogic.PickTargetIndex(5, 1));

            // 동행자가 한 명뿐이면 두 번째 짝은 노릴 대상이 없다.
            Assert.AreEqual(0, ClaimLogic.PickTargetIndex(1, 0));
            Assert.AreEqual(-1, ClaimLogic.PickTargetIndex(1, 1));
            Assert.AreEqual(-1, ClaimLogic.PickTargetIndex(0, 0));
        }
    }

    public sealed class StallHoldLogicTests
    {
        [Test]
        public void Pulls_Only_Nearby_After_Cooldown()
        {
            Assert.IsTrue(StallHoldLogic.CanPull(2f, 0f));
            Assert.IsFalse(StallHoldLogic.CanPull(StallHoldLogic.PullRadius + 0.1f, 0f));
            Assert.IsFalse(StallHoldLogic.CanPull(1f, 1f));
        }

        [Test]
        public void Rescue_Needs_Staying_Close_For_One_Second()
        {
            float progress = 0f;

            progress = StallHoldLogic.AdvanceRescue(progress, 1f, 0.6f);
            Assert.IsFalse(StallHoldLogic.IsRescued(progress));

            // 한 번 떨어지면 처음부터다.
            progress = StallHoldLogic.AdvanceRescue(progress, 3f, 0.1f);
            Assert.AreEqual(0f, progress);

            progress = StallHoldLogic.AdvanceRescue(progress, 1f, 0.6f);
            progress = StallHoldLogic.AdvanceRescue(progress, 1f, 0.6f);
            Assert.IsTrue(StallHoldLogic.IsRescued(progress));
        }

        [Test]
        public void There_Is_Time_To_Walk_Back_And_Rescue()
        {
            // 붙잡힌 뒤 되돌아가 1초 붙어 있을 시간이 충분해야 한다.
            Assert.GreaterOrEqual(StallHoldLogic.HoldSeconds - StallHoldLogic.RescueSeconds, 4f);
        }
    }

    public sealed class BlockerLogicTests
    {
        [Test]
        public void Bump_Needs_Contact_And_Cooldown()
        {
            Assert.IsTrue(BlockerLogic.CanBump(0.5f, 0f));
            Assert.IsFalse(BlockerLogic.CanBump(1.5f, 0f));
            Assert.IsFalse(BlockerLogic.CanBump(0.5f, 0.5f));
        }

        [Test]
        public void One_Bump_Does_Not_Cause_A_Rampage()
        {
            // 충동 최대 100. 한 번 부딪쳐서 폭주하면 억울하다.
            Assert.Less(BlockerLogic.BumpImpulse, 30f);
            Assert.Greater(BlockerLogic.BumpCooldownSeconds, 0f);
        }
    }

    public sealed class PickpocketLogicTests
    {
        [Test]
        public void Steals_Thirty_Percent_Rounded()
        {
            Assert.AreEqual(30, PickpocketLogic.GetStolenAmount(100, PickpocketLogic.StealRatio));
            Assert.AreEqual(14, PickpocketLogic.GetStolenAmount(45, PickpocketLogic.StealRatio));
            Assert.AreEqual(0, PickpocketLogic.GetStolenAmount(0, PickpocketLogic.StealRatio));
            Assert.AreEqual(0, PickpocketLogic.GetStolenAmount(-10, PickpocketLogic.StealRatio));
            Assert.AreEqual(100, PickpocketLogic.GetStolenAmount(100, 5f));
        }

        [Test]
        public void Marked_Followers_Keep_The_Rest()
        {
            Assert.AreEqual(0.7f, PickpocketLogic.RemainingMultiplier, 0.0001f);
        }

        [Test]
        public void Steals_Only_Once_And_Only_When_There_Is_Something()
        {
            Assert.IsTrue(PickpocketLogic.WantsToSteal(50, 2f, false));
            Assert.IsFalse(PickpocketLogic.WantsToSteal(50, 2f, true));
            Assert.IsFalse(PickpocketLogic.WantsToSteal(0, 2f, false));
            Assert.IsFalse(PickpocketLogic.WantsToSteal(50, PickpocketLogic.ApproachDistance + 1f, false));
        }

        [Test]
        public void Dash_Can_Escape_And_Catch()
        {
            // 예고 시작 거리보다 훔치는 거리가 짧아야 예고 중에 도망칠 수 있다.
            Assert.Less(PickpocketLogic.StealDistance, PickpocketLogic.ApproachDistance);
            Assert.Greater(PickpocketLogic.EscapeSeconds, 10f);
            Assert.Greater(PickpocketLogic.CatchBonus, 0);
        }
    }

    public sealed class BeachMarketAbilityTests
    {
        [Test]
        public void Whistle_Alarm_Halves_Hypnosis_Only_While_Active()
        {
            Assert.AreEqual(0.5f, BeachMarketAbilityValues.GetAlarmMultiplier(3f));
            Assert.AreEqual(1f, BeachMarketAbilityValues.GetAlarmMultiplier(0f));
        }

        [Test]
        public void New_Abilities_Have_Telegraphs_And_Cooldowns()
        {
            DisruptorKind[] specials =
            {
                DisruptorKind.LifeguardChief,
                DisruptorKind.FilmCrew,
                DisruptorKind.Pickpocket
            };

            foreach (DisruptorKind kind in specials)
            {
                DisruptorProfile profile = DisruptorCatalog.Get(kind);

                Assert.IsTrue(profile.IsSpecial, kind.ToString());
                Assert.GreaterOrEqual(
                    SpecialAbilityLogic.SanitizeTelegraph(profile.TelegraphSeconds),
                    SpecialAbilityLogic.MinimumTelegraphSeconds,
                    kind.ToString());
                Assert.Greater(profile.CooldownSeconds, 0f, kind.ToString());
            }
        }

        [Test]
        public void Only_The_Pickpocket_Hides_Its_Name()
        {
            foreach (DisruptorKind kind in System.Enum.GetValues(typeof(DisruptorKind)))
            {
                Assert.AreEqual(
                    kind == DisruptorKind.Pickpocket,
                    DisruptorCatalog.Get(kind).HiddenUntilFired,
                    kind.ToString());
            }
        }
    }

    public sealed class BeachMarketPlacementTests
    {
        [Test]
        public void Beach_Has_Lifeguards_A_Hunter_Pair_And_The_Chief()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.Beach, 1);

            Assert.AreEqual(2, placements.FindAll(p => p.Kind == DisruptorKind.Lifeguard).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.LifeguardChief).Count);

            List<DisruptorPlacement> hunters =
                placements.FindAll(p => p.Kind == DisruptorKind.BeachHunter);

            Assert.AreEqual(2, hunters.Count);
            Assert.AreNotEqual(hunters[0].Variant, hunters[1].Variant, "2인조는 서로 다른 동행자를 노려야 합니다");
        }

        [Test]
        public void Night_Market_Has_A_Tout_At_Every_Stall()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.NightMarket, 1);

            foreach (float x in DisruptorCatalog.NightMarketStallX)
            {
                Assert.IsTrue(
                    placements.Exists(p => p.Kind == DisruptorKind.StallTout && p.X == x),
                    $"x={x} 노점에 호객꾼이 없습니다");
            }

            Assert.AreEqual(2, placements.FindAll(p => p.Kind == DisruptorKind.Drunkard).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.FilmCrew).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.Pickpocket).Count);
        }

        [Test]
        public void Stalls_And_Parasols_Stay_Clear_Of_Stairs_And_Recovery()
        {
            List<float> xs = new List<float>(DisruptorCatalog.NightMarketStallX);
            xs.AddRange(DisruptorCatalog.BeachParasolX);
            xs.Add(DisruptorCatalog.BeachTowerX);

            foreach (float x in xs)
            {
                Assert.Greater(System.Math.Abs(x - FloorLayout.UpStairX), 1.5f, x.ToString());
                Assert.Greater(System.Math.Abs(x - FloorLayout.DownStairX), 1.5f, x.ToString());
                Assert.Greater(System.Math.Abs(x - FloorLayout.RecoveryX), 2f, x.ToString());
                Assert.Greater(System.Math.Abs(x - FloorLayout.PlayerStartX), 1.5f, x.ToString());
            }
        }

        [Test]
        public void Only_Night_Market_Skips_Reinforcements()
        {
            Assert.AreEqual(DisruptorKind.TrainingAssistant, DisruptorCatalog.GetReinforcementKind(LocationId.TrainingCenter));
            Assert.AreEqual(DisruptorKind.Lifeguard, DisruptorCatalog.GetReinforcementKind(LocationId.Beach));
            Assert.IsNull(DisruptorCatalog.GetReinforcementKind(LocationId.NightMarket));
        }

        [Test]
        public void Other_Locations_Still_Have_No_Disruptors()
        {
            // 나머지 5곳은 24일차부터 채운다.
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                bool built =
                    location.Id == LocationId.TrainingCenter ||
                    location.Id == LocationId.Beach ||
                    location.Id == LocationId.NightMarket;

                Assert.AreEqual(
                    built,
                    DisruptorCatalog.GetPlacements(location.Id, location.FloorCount).Count > 0,
                    location.Id.ToString());
            }
        }
    }

    public sealed class TideLogicTests
    {
        [Test]
        public void Tide_Follows_Low_Warning_High()
        {
            Assert.AreEqual(TidePhase.Low, TideLogic.GetPhase(0f));
            Assert.AreEqual(TidePhase.Low, TideLogic.GetPhase(18.9f));
            Assert.AreEqual(TidePhase.Warning, TideLogic.GetPhase(19f));
            Assert.AreEqual(TidePhase.High, TideLogic.GetPhase(22f));
            Assert.AreEqual(TidePhase.High, TideLogic.GetPhase(29.9f));
            Assert.AreEqual(TidePhase.Low, TideLogic.GetPhase(30f));
            Assert.AreEqual(TidePhase.Warning, TideLogic.GetPhase(49.5f));
        }

        [Test]
        public void Invalid_Time_Is_Low_Tide()
        {
            Assert.AreEqual(TidePhase.Low, TideLogic.GetPhase(-1f));
            Assert.AreEqual(TidePhase.Low, TideLogic.GetPhase(float.NaN));
        }

        [Test]
        public void Countdowns_Match_The_Phase()
        {
            Assert.AreEqual(22f, TideLogic.SecondsUntilHigh(0f), 0.001f);
            Assert.AreEqual(2f, TideLogic.SecondsUntilHigh(20f), 0.001f);
            Assert.AreEqual(0f, TideLogic.SecondsUntilHigh(25f));
            Assert.AreEqual(5f, TideLogic.SecondsUntilLow(25f), 0.001f);
            Assert.AreEqual(0f, TideLogic.SecondsUntilLow(10f));
        }

        [Test]
        public void Only_The_Shore_Lanes_Flood_And_Only_At_High_Tide()
        {
            Assert.IsTrue(TideLogic.IsFlooded(TidePhase.High, FloorSpace.WalkMinY));
            Assert.IsFalse(TideLogic.IsFlooded(TidePhase.High, FloorSpace.WalkMaxY));
            Assert.IsFalse(TideLogic.IsFlooded(TidePhase.Warning, FloorSpace.WalkMinY));

            // 물에 잠기는 폭은 걷는 폭의 절반보다 좁아야 한다.
            float walkHeight = FloorSpace.WalkMaxY - FloorSpace.WalkMinY;
            Assert.Less(TideLogic.FloodTopLocalY - FloorSpace.WalkMinY, walkHeight * 0.5f);
        }

        [Test]
        public void Disruptor_Lane_And_Shade_Stay_Dry()
        {
            // 방해 세력 줄(-1.6)과 파라솔 그늘(-2.4 ± 0.9)은 밀물에 잠기지 않는다.
            Assert.IsFalse(TideLogic.IsFlooded(TidePhase.High, -1.6f));
            Assert.IsFalse(TideLogic.IsFlooded(TidePhase.High, -2.4f - ParasolShade.RadiusY));
        }
    }

    public sealed class AreaAndLanternLogicTests
    {
        [Test]
        public void Ellipse_Is_Wider_Than_Tall()
        {
            Assert.IsTrue(AreaLogic.IsInsideEllipse(1.5f, 0f, 1.6f, 0.9f));
            Assert.IsFalse(AreaLogic.IsInsideEllipse(0f, 1.5f, 1.6f, 0.9f));
            Assert.IsTrue(AreaLogic.IsInsideEllipse(0f, 0f, 1.6f, 0.9f));
            Assert.IsFalse(AreaLogic.IsInsideEllipse(0f, 0f, 0f, 0.9f));
        }

        [Test]
        public void Darkness_Shrinks_Range_Only_Outside_Light()
        {
            Assert.AreEqual(1f, LanternLogic.GetRangeMultiplier(false, false, 0.7f));
            Assert.AreEqual(1f, LanternLogic.GetRangeMultiplier(true, true, 0.7f));
            Assert.AreEqual(0.7f, LanternLogic.GetRangeMultiplier(true, false, 0.7f), 0.0001f);
        }

        [Test]
        public void Dark_Range_Scale_Is_Kept_Sane()
        {
            Assert.AreEqual(0.1f, LanternLogic.GetRangeMultiplier(true, false, -1f), 0.0001f);
            Assert.AreEqual(1f, LanternLogic.GetRangeMultiplier(true, false, 5f));
            Assert.AreEqual(LanternLogic.DefaultDarkRangeScale, LanternLogic.GetRangeMultiplier(true, false, float.NaN));
        }

        [Test]
        public void Default_Dark_Scale_Matches_Balance_Default()
        {
            Assert.AreEqual(
                LanternLogic.DefaultDarkRangeScale,
                new Balance.StageBalanceValues().DarkHypnosisRangeScale,
                0.0001f);
        }

        [Test]
        public void Stall_Lanterns_Leave_Dark_Gaps()
        {
            // 노점 등불(반경 2.2) 사이에 어두운 틈이 있어야 "등불을 따라 걷는" 판단이 생긴다.
            float[] stalls = DisruptorCatalog.NightMarketStallX;

            for (int i = 1; i < stalls.Length; i++)
            {
                Assert.Greater(stalls[i] - stalls[i - 1], 2.2f * 2f, $"{i}번째 노점 간격");
            }
        }
    }

    public sealed class LocationObjectiveLogicTests
    {
        [Test]
        public void Group_Carry_Rewards_Bringing_Three_Or_More_At_Once()
        {
            Assert.AreEqual(0.5f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.GroupCarry, 1));
            Assert.AreEqual(0.5f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.GroupCarry, 2));
            Assert.AreEqual(1.2f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.GroupCarry, 3));
            Assert.AreEqual(1.2f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.GroupCarry, 8));
        }

        [Test]
        public void Other_Objectives_Are_Unchanged()
        {
            Assert.AreEqual(1f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.EssenceQuota, 1));
            Assert.AreEqual(1f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.EssenceQuota, 5));
            Assert.AreEqual(1f, LocationObjectiveLogic.GetBatchMultiplier(LocationObjective.GroupCarry, 0));
        }

        [Test]
        public void Beach_Uses_Group_Carry_And_Shows_A_Hint()
        {
            Assert.AreEqual(LocationObjective.GroupCarry, LocationCatalog.Get(LocationId.Beach).Objective);
            Assert.IsNotEmpty(LocationObjectiveLogic.GetHint(LocationObjective.GroupCarry));
            Assert.IsEmpty(LocationObjectiveLogic.GetHint(LocationObjective.EssenceQuota));
        }
    }
}
