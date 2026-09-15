using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Run;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class LocationCatalogTests
    {
        [Test]
        public void Every_Location_Exists_Exactly_Once()
        {
            HashSet<LocationId> seen = new HashSet<LocationId>();

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.IsTrue(seen.Add(location.Id), $"{location.Id}가 두 번 들어 있습니다");
            }

            foreach (LocationId id in System.Enum.GetValues(typeof(LocationId)))
            {
                Assert.IsTrue(seen.Contains(id), $"{id} 장소가 표에 없습니다");
            }
        }

        [Test]
        public void Every_Location_Is_Playable()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrEmpty(location.DisplayName), location.Id.ToString());
                Assert.GreaterOrEqual(location.FloorCount, 1, location.Id.ToString());
                Assert.Greater(location.NpcDensity, 0f, location.Id.ToString());
                Assert.Greater(location.TimeLimitSeconds, 0f, location.Id.ToString());
                Assert.Greater(location.TargetEssence, 0, location.Id.ToString());
            }
        }

        [Test]
        public void Only_The_Start_Location_Shows_The_Tutorial()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.AreEqual(
                    location.Id == LocationCatalog.StartLocation,
                    location.ShowsTutorial,
                    location.Id.ToString());
            }
        }

        [Test]
        public void Training_Center_Keeps_The_Existing_Four_Floors()
        {
            // 21일차 전의 학교 4층 구조와 층·경쟁자 테스트가 그대로 유지되어야 한다.
            Assert.AreEqual(
                ProjectTheta.Stage.FloorPlanLogic.DefaultFloorCount,
                LocationCatalog.Get(LocationId.TrainingCenter).FloorCount);
        }

        [Test]
        public void Start_Is_Day_And_Final_Is_Night()
        {
            Assert.AreEqual(LocationTimeOfDay.Day, LocationCatalog.Get(LocationCatalog.StartLocation).TimeOfDay);
            Assert.AreEqual(LocationTimeOfDay.Night, LocationCatalog.Get(LocationCatalog.FinalLocation).TimeOfDay);
        }

        [Test]
        public void Floor_Label_Uses_The_Location_Prefix()
        {
            Assert.AreEqual("교육동 2F", LocationCatalog.Get(LocationId.TrainingCenter).GetFloorLabel(1));
            Assert.AreEqual("3F", new LocationDefinition().GetFloorLabel(2));
        }
    }

    public sealed class RunRouteTests
    {
        private const int SeedsToCheck = 300;

        /// <summary>시드마다 첫 번째 후보를 계속 골라 한 판 경로를 끝까지 만든다.</summary>
        private static List<LocationId> PlayRoute(
            int seed,
            int pickIndex)
        {
            RunSession session = new RunSession(seed);

            for (int step = 0; step < RunRouteLogic.ZoneCount; step++)
            {
                List<LocationId> candidates = session.GetCandidates();

                Assert.IsNotEmpty(candidates, $"시드 {seed}, 구역 {step + 1}에 후보가 없습니다");

                LocationId pick = candidates[System.Math.Min(pickIndex, candidates.Count - 1)];

                Assert.IsTrue(session.Select(pick));

                session.RecordZone(new ZoneRecord { Location = pick, Cleared = true, RankLabel = "A" });
            }

            Assert.IsTrue(session.IsFinished);

            return new List<LocationId>(session.Visited);
        }

        [Test]
        public void Route_Starts_At_Training_Center_And_Ends_At_Club()
        {
            for (int seed = 0; seed < SeedsToCheck; seed++)
            {
                List<LocationId> route = PlayRoute(seed, seed % 2);

                Assert.AreEqual(RunRouteLogic.ZoneCount, route.Count);
                Assert.AreEqual(LocationCatalog.StartLocation, route[0]);
                Assert.AreEqual(LocationCatalog.FinalLocation, route[route.Count - 1]);
            }
        }

        [Test]
        public void Route_Never_Visits_A_Place_Twice()
        {
            for (int seed = 0; seed < SeedsToCheck; seed++)
            {
                List<LocationId> route = PlayRoute(seed, seed % 2);

                Assert.AreEqual(route.Count, new HashSet<LocationId>(route).Count, $"시드 {seed}");
            }
        }

        [Test]
        public void Time_Never_Goes_Backwards()
        {
            for (int seed = 0; seed < SeedsToCheck; seed++)
            {
                List<LocationId> route = PlayRoute(seed, seed % 2);

                for (int i = 1; i < route.Count; i++)
                {
                    Assert.GreaterOrEqual(
                        LocationCatalog.Get(route[i]).TimeOfDay,
                        LocationCatalog.Get(route[i - 1]).TimeOfDay,
                        $"시드 {seed}: {route[i - 1]} → {route[i]}");
                }
            }
        }

        [Test]
        public void Middle_Steps_Offer_A_Real_Choice()
        {
            for (int seed = 0; seed < SeedsToCheck; seed++)
            {
                RunSession session = new RunSession(seed);

                session.Select(LocationCatalog.StartLocation);
                session.RecordZone(new ZoneRecord { Location = LocationCatalog.StartLocation, Cleared = true });

                // 2구역은 항상 두 곳 중에서 고른다.
                Assert.AreEqual(RunRouteLogic.ChoicesPerStep, session.GetCandidates().Count, $"시드 {seed}");
            }
        }

        [Test]
        public void First_And_Last_Steps_Have_A_Single_Fixed_Place()
        {
            CollectionAssert.AreEqual(
                new[] { LocationCatalog.StartLocation },
                RunRouteLogic.GetCandidates(0, new List<LocationId>(), 7));

            CollectionAssert.AreEqual(
                new[] { LocationCatalog.FinalLocation },
                RunRouteLogic.GetCandidates(RunRouteLogic.ZoneCount - 1, new List<LocationId>(), 7));
        }

        [Test]
        public void Same_Seed_Gives_The_Same_Candidates()
        {
            // 지도를 다시 열어도 후보가 바뀌면 안 된다.
            List<LocationId> visited = new List<LocationId> { LocationId.TrainingCenter };

            CollectionAssert.AreEqual(
                RunRouteLogic.GetCandidates(1, visited, 12345),
                RunRouteLogic.GetCandidates(1, visited, 12345));
        }

        [Test]
        public void Night_Places_Are_Not_Offered_Right_After_The_Start()
        {
            List<LocationId> visited = new List<LocationId> { LocationId.TrainingCenter };

            for (int seed = 0; seed < SeedsToCheck; seed++)
            {
                foreach (LocationId id in RunRouteLogic.GetCandidates(1, visited, seed))
                {
                    Assert.AreNotEqual(LocationTimeOfDay.Night, LocationCatalog.Get(id).TimeOfDay, $"시드 {seed}");
                }
            }
        }
    }

    public sealed class RunSessionTests
    {
        [Test]
        public void Cannot_Select_A_Place_That_Is_Not_Offered()
        {
            RunSession session = new RunSession(1);

            Assert.IsFalse(session.Select(LocationId.RooftopClub));
            Assert.IsNull(session.SelectedLocation);

            Assert.IsTrue(session.Select(LocationId.TrainingCenter));
        }

        [Test]
        public void Failing_A_Zone_Ends_The_Run()
        {
            RunSession session = new RunSession(1);

            session.Select(LocationId.TrainingCenter);
            session.RecordZone(new ZoneRecord { Location = LocationId.TrainingCenter, Cleared = false });

            Assert.IsTrue(session.IsFinished);
            Assert.IsEmpty(session.GetCandidates());
        }

        [Test]
        public void Zone_Is_Recorded_Only_Once()
        {
            RunSession session = new RunSession(1);

            session.Select(LocationId.TrainingCenter);

            ZoneRecord record = new ZoneRecord { Location = LocationId.TrainingCenter, Cleared = true, ContractEssence = 30 };

            session.RecordZone(record);
            session.RecordZone(record);

            Assert.AreEqual(1, session.Records.Count);
            Assert.AreEqual(1, session.NextStep);
            Assert.AreEqual(30, session.TotalContractEssence);
        }

        [Test]
        public void Level_And_Cards_Carry_Across_Zones()
        {
            RunSession session = new RunSession(1);

            session.Level.AddXp(1000);
            session.Upgrades.Apply(RunUpgradeCard.BindingGaze);

            int level = session.Level.Level;

            session.Select(LocationId.TrainingCenter);
            session.RecordZone(new ZoneRecord { Location = LocationId.TrainingCenter, Cleared = true });

            // 같은 객체를 다음 구역이 이어받는다.
            Assert.AreEqual(level, session.Level.Level);
            Assert.AreEqual(1, session.Upgrades.PickCount);
        }

        [Test]
        public void Abandon_Ends_The_Run()
        {
            RunSession session = new RunSession(1);

            session.Select(LocationId.TrainingCenter);
            session.Abandon();

            Assert.IsTrue(session.IsFinished);
            Assert.IsNull(session.SelectedLocation);
        }
    }
}
