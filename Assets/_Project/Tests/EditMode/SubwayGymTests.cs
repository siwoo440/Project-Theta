using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PopulationLogicTests
    {
        [Test]
        public void Starts_Empty_And_Grows_To_Four()
        {
            Assert.AreEqual(0, PopulationLogic.GetAllowed(0f, 20f));
            Assert.AreEqual(0, PopulationLogic.GetAllowed(19.9f, 20f));
            Assert.AreEqual(1, PopulationLogic.GetAllowed(20f, 20f));
            Assert.AreEqual(3, PopulationLogic.GetAllowed(65f, 20f));
            Assert.AreEqual(4, PopulationLogic.GetAllowed(80f, 20f));
            Assert.AreEqual(4, PopulationLogic.GetAllowed(999f, 20f));
        }

        [Test]
        public void Zero_Ramp_Means_Full_From_The_Start()
        {
            Assert.AreEqual(PopulationLogic.MaxPerFloor, PopulationLogic.GetAllowed(0f, 0f));
            Assert.AreEqual(PopulationLogic.MaxPerFloor, PopulationLogic.GetAllowed(0f, float.NaN));
            Assert.AreEqual(0, PopulationLogic.GetAllowed(float.NaN, 20f));
        }

        [Test]
        public void Free_Slots_Never_Exceed_The_Cap()
        {
            Assert.AreEqual(2, PopulationLogic.GetFreeSlots(3, 1));
            Assert.AreEqual(0, PopulationLogic.GetFreeSlots(2, 5));
            Assert.AreEqual(4, PopulationLogic.GetFreeSlots(10, 0));
            Assert.AreEqual(0, PopulationLogic.GetFreeSlots(-1, 0));
        }

        [Test]
        public void No_Location_Places_More_Than_Four_On_A_Floor()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                List<DisruptorPlacement> placements =
                    DisruptorCatalog.GetPlacements(location.Id, location.FloorCount);

                for (int floor = 0; floor < location.FloorCount; floor++)
                {
                    Assert.LessOrEqual(
                        PopulationLogic.CountOnFloor(placements, floor),
                        PopulationLogic.MaxPerFloor,
                        $"{location.Id} {floor + 1}F");
                }
            }
        }

        [Test]
        public void Announcer_Is_Not_Counted()
        {
            List<DisruptorPlacement> placements = new List<DisruptorPlacement>
            {
                new DisruptorPlacement(DisruptorKind.PlatformAnnouncer, 0, 0f),
                new DisruptorPlacement(DisruptorKind.StationAttendant, 0, 1f)
            };

            Assert.AreEqual(1, PopulationLogic.CountOnFloor(placements, 0));
        }

        [Test]
        public void Ramp_Default_Matches_Balance_And_Fills_Before_Time_Runs_Out()
        {
            float ramp = new Balance.StageBalanceValues().DisruptorRampSeconds;

            Assert.AreEqual(PopulationLogic.DefaultRampSeconds, ramp, 0.0001f);

            // 가장 짧은 장소에서도 제한 시간 절반 안에 최대 인원이 된다.
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.Less(ramp * PopulationLogic.MaxPerFloor, location.TimeLimitSeconds * 0.5f, location.Id.ToString());
            }
        }
    }

    public sealed class GateLogicTests
    {
        [Test]
        public void Small_Groups_And_Crowd_Rush_Pass_Freely()
        {
            Assert.IsFalse(GateLogic.NeedsCheck(4, false));
            Assert.IsTrue(GateLogic.NeedsCheck(5, false));
            Assert.IsFalse(GateLogic.NeedsCheck(8, true));
        }

        [Test]
        public void Only_Followers_Across_The_Gate_Are_Held()
        {
            Assert.IsTrue(GateLogic.IsOnOtherSide(0f, 2f, -1f));
            Assert.IsFalse(GateLogic.IsOnOtherSide(0f, 2f, 1f));

            // 개찰구 한가운데는 어느 쪽도 아니다.
            Assert.IsFalse(GateLogic.IsOnOtherSide(0f, 0f, -1f));
            Assert.IsFalse(GateLogic.IsOnOtherSide(0f, 2f, 0.01f));
        }

        [Test]
        public void Release_Waits_One_Second()
        {
            Assert.IsFalse(GateLogic.CanRelease(0.5f));
            Assert.IsTrue(GateLogic.CanRelease(1f));
        }
    }

    public sealed class RescueLogicTests
    {
        [Test]
        public void Rushes_Only_When_It_Sees_A_Started_Hypnosis()
        {
            Assert.IsTrue(RescueLogic.ShouldRush(0.3f, true, true, 0f));
            Assert.IsFalse(RescueLogic.ShouldRush(0.01f, true, true, 0f), "막 시작한 순간은 넘어간다");
            Assert.IsFalse(RescueLogic.ShouldRush(0.3f, true, false, 0f), "등 뒤는 못 본다");
            Assert.IsFalse(RescueLogic.ShouldRush(0.3f, false, true, 0f), "이미 넘어간 회원은 포기");
            Assert.IsFalse(RescueLogic.ShouldRush(0.3f, true, true, 2f), "재사용 대기 중");
        }

        [Test]
        public void Drain_Halves_The_Gauge()
        {
            Assert.AreEqual(40f, RescueLogic.Drain(80f), 0.0001f);
            Assert.AreEqual(0f, RescueLogic.Drain(-5f));
            Assert.AreEqual(0f, RescueLogic.Drain(float.NaN));
        }
    }

    public sealed class BrawlLogicTests
    {
        [Test]
        public void Dash_During_Windup_Counters()
        {
            Assert.AreEqual(BrawlLogic.Outcome.Countered, BrawlLogic.Resolve(1f, true));
            Assert.AreEqual(BrawlLogic.Outcome.Hit, BrawlLogic.Resolve(1f, false));
            Assert.AreEqual(BrawlLogic.Outcome.Miss, BrawlLogic.Resolve(3f, false));
            Assert.AreEqual(BrawlLogic.Outcome.Miss, BrawlLogic.Resolve(3f, true));
        }

        [Test]
        public void Windup_Is_Long_Enough_To_React()
        {
            // 자세가 너무 짧으면 받아칠 수 없다. 대시 재사용(0.6초)보다 길어야 한다.
            Assert.Greater(BrawlLogic.WindupSeconds, 0.6f);
            Assert.GreaterOrEqual(BrawlLogic.HitDistance, BrawlLogic.EngageDistance);
            Assert.Greater(BrawlLogic.CounterStunSeconds, BrawlLogic.CooldownSeconds);
        }
    }

    public sealed class CrowdFlowLogicTests
    {
        [Test]
        public void Drift_Follows_Direction_Unless_Anchored()
        {
            Assert.AreEqual(CrowdFlowLogic.DriftSpeed, CrowdFlowLogic.GetDrift(true, false, 1));
            Assert.AreEqual(-CrowdFlowLogic.DriftSpeed, CrowdFlowLogic.GetDrift(true, false, -3));
            Assert.AreEqual(0f, CrowdFlowLogic.GetDrift(true, true, 1));
            Assert.AreEqual(0f, CrowdFlowLogic.GetDrift(false, false, 1));
        }

        [Test]
        public void Followers_Slow_Only_While_Flowing()
        {
            Assert.AreEqual(CrowdFlowLogic.SpeedMultiplier, CrowdFlowLogic.GetSpeedMultiplier(true, false));
            Assert.AreEqual(1f, CrowdFlowLogic.GetSpeedMultiplier(true, true));
            Assert.AreEqual(1f, CrowdFlowLogic.GetSpeedMultiplier(false, false));
        }

        [Test]
        public void Anchor_Lasts_Most_Of_The_Flow()
        {
            Assert.Less(CrowdFlowLogic.AnchorSeconds, CrowdFlowLogic.FlowSeconds);
            Assert.Greater(CrowdFlowLogic.AnchorSeconds, 0f);
        }
    }

    public sealed class DroneLogicTests
    {
        [Test]
        public void Spotted_After_Three_Seconds_In_View()
        {
            float seen = 0f;

            for (int i = 0; i < 29; i++)
            {
                seen = DroneLogic.AdvanceSeen(seen, true, 0.1f);
            }

            Assert.IsFalse(DroneLogic.IsSpotted(seen));

            seen = DroneLogic.AdvanceSeen(seen, true, 0.2f);
            Assert.IsTrue(DroneLogic.IsSpotted(seen));
        }

        [Test]
        public void Leaving_The_Spot_Drains_Twice_As_Fast()
        {
            Assert.AreEqual(1f, DroneLogic.AdvanceSeen(2f, false, 0.5f), 0.0001f);
            Assert.AreEqual(0f, DroneLogic.AdvanceSeen(0.5f, false, 1f));
            Assert.AreEqual(0f, DroneLogic.AdvanceSeen(float.NaN, true, 1f));
        }

        [Test]
        public void Orbit_Stays_Inside_The_Beach()
        {
            for (float angle = 0f; angle < 6.3f; angle += 0.5f)
            {
                DroneLogic.GetOrbitOffset(angle, out float x, out float y);

                float worldX = DisruptorCatalog.BeachDroneX + x;
                float localY = -1.6f + y;

                Assert.Less(System.Math.Abs(worldX) + DroneLogic.SpotRadius, FloorSpace.WalkMaxX, angle.ToString());
                Assert.IsFalse(TideLogic.IsFlooded(TidePhase.High, localY), "드론 궤도 중심이 물에 잠기면 안 됩니다");
            }
        }
    }

    public sealed class TrainLogicTests
    {
        [Test]
        public void Trains_Arrive_Every_Period()
        {
            Assert.AreEqual(0, TrainLogic.GetArrivedCount(0f));
            Assert.AreEqual(0, TrainLogic.GetArrivedCount(44.9f));
            Assert.AreEqual(1, TrainLogic.GetArrivedCount(45f));
            Assert.AreEqual(3, TrainLogic.GetArrivedCount(135f));
            Assert.AreEqual(0, TrainLogic.GetArrivedCount(float.NaN));
        }

        [Test]
        public void Doors_Open_Briefly_After_Each_Arrival()
        {
            Assert.IsFalse(TrainLogic.IsDoorOpen(2f), "판 시작 직후는 문이 열리지 않는다");
            Assert.IsTrue(TrainLogic.IsDoorOpen(46f));
            Assert.IsFalse(TrainLogic.IsDoorOpen(51f));
        }

        [Test]
        public void Arrival_Is_Warned_Before_The_Doors()
        {
            Assert.IsFalse(TrainLogic.IsArriving(30f));
            Assert.IsTrue(TrainLogic.IsArriving(42f));
            Assert.AreEqual(3f, TrainLogic.SecondsUntilNext(42f), 0.001f);
        }

        [Test]
        public void Later_Trains_Are_More_Crowded_But_Capped()
        {
            Assert.AreEqual(TrainLogic.MinimumCommuters, TrainLogic.GetCommuterCount(0));
            Assert.Greater(TrainLogic.GetCommuterCount(2), TrainLogic.GetCommuterCount(0));
            Assert.AreEqual(TrainLogic.MaximumCommuters, TrainLogic.GetCommuterCount(10));
        }

        [Test]
        public void Required_Trains_Fit_In_The_Subway_Time_Limit()
        {
            LocationDefinition subway = LocationCatalog.Get(LocationId.SubwayStation);

            Assert.AreEqual(LocationObjective.Survival, subway.Objective);

            // 밸런스 슬라이더 최대값(3대)까지 제한 시간 안에 모두 도착해야 한다.
            Assert.IsTrue(TrainLogic.FitsTimeLimit(3, subway.TimeLimitSeconds));
            Assert.AreEqual(TrainLogic.DefaultRequiredTrains, new Balance.StageBalanceValues().SurvivalTrainCount);
        }
    }

    public sealed class HeartRateLogicTests
    {
        [Test]
        public void Workout_Is_Fast_And_Yoga_Is_Calm()
        {
            Assert.Greater(HeartRateLogic.GetHypnosisMultiplier(GymZoneKind.Workout, false, 1.3f), 1f);
            Assert.Less(HeartRateLogic.GetHypnosisMultiplier(GymZoneKind.Yoga, false, 1.3f), 1f);
            Assert.AreEqual(1f, HeartRateLogic.GetHypnosisMultiplier(null, true, 1.3f));
            Assert.AreEqual(1f, HeartRateLogic.GetHypnosisMultiplier(GymZoneKind.Pool, true, 1.3f));
        }

        [Test]
        public void Group_Pt_Speeds_Up_Workout_Hypnosis()
        {
            Assert.Greater(
                HeartRateLogic.GetHypnosisMultiplier(GymZoneKind.Workout, true, 1.3f),
                HeartRateLogic.GetHypnosisMultiplier(GymZoneKind.Workout, false, 1.3f));
        }

        [Test]
        public void Yoga_Room_Blocks_Impulse_Even_During_Pt()
        {
            Assert.AreEqual(0f, HeartRateLogic.GetImpulseMultiplier(GymZoneKind.Yoga, true, 1.6f, 1f));
            Assert.AreEqual(1.6f, HeartRateLogic.GetImpulseMultiplier(null, true, 1.6f, 1f), 0.0001f);
            Assert.AreEqual(1.5f * 1.6f, HeartRateLogic.GetImpulseMultiplier(GymZoneKind.Workout, true, 1.6f, 1f), 0.0001f);
            Assert.AreEqual(1f, HeartRateLogic.GetImpulseMultiplier(null, false, 1.6f, 1f));
        }

        [Test]
        public void Heart_Rate_Scale_Only_Changes_The_Workout_Bonus()
        {
            Assert.AreEqual(1f, HeartRateLogic.GetImpulseMultiplier(GymZoneKind.Workout, false, 1.6f, 0f));
            Assert.AreEqual(2f, HeartRateLogic.GetImpulseMultiplier(GymZoneKind.Workout, false, 1.6f, 2f), 0.0001f);
        }
    }

    public sealed class ObjectiveStateLogicTests
    {
        [Test]
        public void Survival_Clears_After_Trains_With_A_Follower()
        {
            Assert.AreEqual(
                StageState.Cleared,
                ObjectiveStateLogic.Resolve(LocationObjective.Survival, 10f, 0, 150, 5, 3, 3, 1));
        }

        [Test]
        public void Survival_Ignores_The_Essence_Target()
        {
            Assert.AreEqual(
                StageState.Running,
                ObjectiveStateLogic.Resolve(LocationObjective.Survival, 10f, 999, 150, 5, 1, 3, 4));
        }

        [Test]
        public void Survival_Without_Followers_Keeps_Running_Until_Time_Out()
        {
            Assert.AreEqual(
                StageState.Running,
                ObjectiveStateLogic.Resolve(LocationObjective.Survival, 10f, 0, 150, 5, 3, 3, 0));

            Assert.AreEqual(
                StageState.FailedByTime,
                ObjectiveStateLogic.Resolve(LocationObjective.Survival, 0f, 0, 150, 5, 3, 3, 0));
        }

        [Test]
        public void Health_Failure_Comes_First()
        {
            Assert.AreEqual(
                StageState.FailedByHealth,
                ObjectiveStateLogic.Resolve(LocationObjective.Survival, 10f, 0, 150, 0, 3, 3, 2));
        }

        [Test]
        public void Other_Objectives_Use_The_Essence_Rule()
        {
            Assert.AreEqual(
                StageState.Cleared,
                ObjectiveStateLogic.Resolve(LocationObjective.SpecialTarget, 10f, 160, 160, 5, 0, 3, 0));

            Assert.AreEqual(
                StageRules.ResolveState(0f, 10, 150, 5),
                ObjectiveStateLogic.Resolve(LocationObjective.EssenceQuota, 0f, 10, 150, 5, 9, 3, 9));
        }

        [Test]
        public void Hints_Exist_For_New_Objectives()
        {
            Assert.IsNotEmpty(LocationObjectiveLogic.GetHint(LocationObjective.Survival));
            Assert.IsNotEmpty(LocationObjectiveLogic.GetHint(LocationObjective.SpecialTarget));
        }
    }

    public sealed class SubwayGymPlacementTests
    {
        [Test]
        public void Subway_Has_An_Attendant_On_Every_Floor_And_One_Announcer()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.SubwayStation, 2);

            for (int floor = 0; floor < 2; floor++)
            {
                Assert.IsTrue(placements.Exists(p => p.Floor == floor && p.Kind == DisruptorKind.StationAttendant));
            }

            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.PlatformAnnouncer).Count);

            // 행인은 배치하지 않고 열차가 내린다.
            Assert.IsFalse(placements.Exists(p => p.Kind == DisruptorKind.PhoneCommuter));
        }

        [Test]
        public void Fitness_Center_Has_Trainers_Veteran_Director_And_Coach()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.FitnessCenter, 2);

            int trainers = placements.FindAll(p => p.Kind == DisruptorKind.PersonalTrainer).Count;

            Assert.GreaterOrEqual(trainers, 2);
            Assert.LessOrEqual(trainers, 3);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.GymVeteran).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.GymDirector).Count);
            Assert.AreEqual(1, placements.FindAll(p => p.Kind == DisruptorKind.SwimCoach).Count);
        }

        [Test]
        public void Swim_Coach_Stands_In_The_Pool()
        {
            DisruptorPlacement coach =
                DisruptorCatalog.GetPlacements(LocationId.FitnessCenter, 2)
                    .Find(p => p.Kind == DisruptorKind.SwimCoach);

            bool inPool = false;

            foreach (GymZoneSpec zone in GymLayout.Zones)
            {
                if (zone.Kind == GymZoneKind.Pool &&
                    zone.Floor == coach.Floor &&
                    coach.X >= zone.MinX &&
                    coach.X <= zone.MaxX)
                {
                    inPool = true;
                }
            }

            Assert.IsTrue(inPool);
        }

        [Test]
        public void Every_Gym_Floor_Has_A_Workout_Zone_For_The_Athlete()
        {
            for (int floor = 0; floor < LocationCatalog.Get(LocationId.FitnessCenter).FloorCount; floor++)
            {
                float x = GymLayout.GetAthleteX(floor);

                Assert.IsTrue(
                    System.Array.Exists(
                        GymLayout.Zones,
                        z => z.Floor == floor && z.Kind == GymZoneKind.Workout && x >= z.MinX && x <= z.MaxX),
                    $"{floor + 1}F");
            }
        }

        [Test]
        public void Gym_Zones_Do_Not_Overlap_Or_Cover_Stairs_And_Recovery()
        {
            GymZoneSpec[] zones = GymLayout.Zones;

            for (int i = 0; i < zones.Length; i++)
            {
                Assert.Less(zones[i].MinX, zones[i].MaxX);
                Assert.Greater(zones[i].MinX, FloorLayout.DownStairX, $"{i}");
                Assert.Less(zones[i].MaxX, FloorLayout.UpStairX, $"{i}");
                Assert.Less(zones[i].MaxX, FloorLayout.RecoveryX, $"{i}");

                for (int j = i + 1; j < zones.Length; j++)
                {
                    if (zones[i].Floor != zones[j].Floor)
                    {
                        continue;
                    }

                    Assert.IsTrue(
                        zones[i].MaxX <= zones[j].MinX || zones[j].MaxX <= zones[i].MinX,
                        $"{i}와 {j} 구역이 겹칩니다");
                }
            }
        }

        [Test]
        public void Reinforcements_Match_The_Location()
        {
            Assert.IsNull(DisruptorCatalog.GetReinforcementKind(LocationId.SubwayStation));
            Assert.AreEqual(DisruptorKind.PersonalTrainer, DisruptorCatalog.GetReinforcementKind(LocationId.FitnessCenter));
        }

        [Test]
        public void Beach_Now_Has_A_Drone_Photographer()
        {
            DisruptorProfile drone = DisruptorCatalog.Get(DisruptorKind.DronePhotographer);

            Assert.IsTrue(drone.IgnoresShade);
            Assert.IsTrue(drone.IsSpecial);
            Assert.IsTrue(
                DisruptorCatalog.GetPlacements(LocationId.Beach, 1)
                    .Exists(p => p.Kind == DisruptorKind.DronePhotographer));
        }

        [Test]
        public void Only_The_Announcer_Is_An_Object()
        {
            foreach (DisruptorKind kind in System.Enum.GetValues(typeof(DisruptorKind)))
            {
                Assert.AreEqual(
                    kind == DisruptorKind.PlatformAnnouncer,
                    DisruptorCatalog.Get(kind).IsObject,
                    kind.ToString());
            }
        }

        [Test]
        public void Athlete_Is_Hard_But_Rewarding()
        {
            Assert.Less(CityAbilityValues.AthleteHypnosisMultiplier, 1f);
            Assert.Greater(CityAbilityValues.AthleteHypnosisMultiplier, 0f);
            Assert.Greater(CityAbilityValues.AthleteBonusEssence, 0);
        }
    }
}
