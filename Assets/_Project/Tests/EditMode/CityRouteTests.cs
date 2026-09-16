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

    /// <summary>29일차: 판이 없다. 장소 8곳 어디든 몇 번이든 고른다.</summary>
    public sealed class RunRouteTests
    {
        [Test]
        public void Every_Location_Can_Always_Be_Chosen()
        {
            List<LocationId> locations = RunRouteLogic.GetLocations();

            Assert.AreEqual(LocationCatalog.All.Count, locations.Count);

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.Contains(location.Id, locations);
            }
        }

        [Test]
        public void Locations_Are_Sorted_And_Stable()
        {
            List<LocationId> first = RunRouteLogic.GetLocations();
            List<LocationId> second = RunRouteLogic.GetLocations();

            CollectionAssert.AreEqual(first, second);

            for (int i = 1; i < first.Count; i++)
            {
                Assert.Less((int)first[i - 1], (int)first[i]);
            }
        }

        [Test]
        public void Tier_Rises_From_Start_To_Boss()
        {
            Assert.AreEqual(0, RunRouteLogic.GetTier(LocationCatalog.StartLocation));
            Assert.AreEqual(4, RunRouteLogic.GetTier(LocationCatalog.FinalLocation));

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                int tier = RunRouteLogic.GetTier(location.Id);

                Assert.GreaterOrEqual(tier, 0);
                Assert.LessOrEqual(tier, 4);

                if (location.Id != LocationCatalog.StartLocation &&
                    location.Id != LocationCatalog.FinalLocation)
                {
                    Assert.AreEqual(1 + (int)location.TimeOfDay, tier, location.Id.ToString());
                }
            }
        }
    }

    public sealed class RunSessionTests
    {
        [Test]
        public void Each_Attempt_Starts_Fresh()
        {
            RunSession first = new RunSession(1, LocationId.Beach);

            first.Level.AddXp(1000);
            first.Upgrades.Apply(RunUpgradeCard.BindingGaze);

            // 29일차: 장소마다 새 도전을 만들므로 레벨 · 카드가 이어지지 않는다.
            RunSession next = new RunSession(2, LocationId.NightMarket);

            Assert.AreEqual(1, next.Level.Level);
            Assert.AreEqual(0, next.Upgrades.PickCount);
            Assert.IsFalse(next.StartContractTaken);
            Assert.AreEqual(LocationId.NightMarket, next.Location);
        }

        [Test]
        public void Result_Is_Recorded_Only_Once()
        {
            RunSession session = new RunSession(1, LocationId.TrainingCenter);

            Assert.IsFalse(session.IsRecorded);
            Assert.IsTrue(session.MarkRecorded());
            Assert.IsTrue(session.IsRecorded);
            Assert.IsFalse(session.MarkRecorded());
        }
    }
}
