using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class LocationGuideCatalogTests
    {
        [Test]
        public void Every_Location_Has_A_Filled_Guide()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                string id = location.Id.ToString();
                LocationGuide guide = LocationGuideCatalog.Get(location.Id);

                Assert.IsTrue(LocationGuideCatalog.Has(location.Id), id);
                Assert.AreEqual(location.Id, guide.Id, id);
                Assert.That(guide.Difficulty, Is.InRange(LocationGuideCatalog.MinDifficulty, LocationGuideCatalog.MaxDifficulty), id);
                Assert.GreaterOrEqual(guide.Rules.Length, 2, id);
                Assert.GreaterOrEqual(guide.Enemies.Length, 2, id);
                Assert.GreaterOrEqual(guide.Tips.Length, 1, id);

                foreach (string rule in guide.Rules)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(rule), id);
                }

                foreach (LocationGuideEnemy enemy in guide.Enemies)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(enemy.Name), id);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(enemy.Effect), id);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(enemy.Counter), id);
                }
            }
        }

        [Test]
        public void Difficulty_Order()
        {
            Assert.AreEqual(1, LocationGuideCatalog.Get(LocationId.TrainingCenter).Difficulty);
            Assert.AreEqual(5, LocationGuideCatalog.Get(LocationId.RooftopClub).Difficulty);

            // 보스 장소가 가장 어렵다.
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.LessOrEqual(
                    LocationGuideCatalog.Get(location.Id).Difficulty,
                    LocationGuideCatalog.Get(LocationId.RooftopClub).Difficulty);
            }
        }

        [Test]
        public void Club_Does_Not_List_Rivals()
        {
            LocationDefinition club = LocationCatalog.Get(LocationId.RooftopClub);
            LocationDefinition beach = LocationCatalog.Get(LocationId.Beach);

            Assert.IsFalse(LocationGuideCatalog.RivalsAppear(club));
            Assert.IsTrue(LocationGuideCatalog.RivalsAppear(beach));

            StringAssert.DoesNotContain("금태양", LocationGuideLogic.BuildEnemyText(club, LocationGuideCatalog.Get(club.Id)));
            StringAssert.Contains("금태양", LocationGuideLogic.BuildEnemyText(beach, LocationGuideCatalog.Get(beach.Id)));
            StringAssert.Contains("경쟁자  없음", LocationGuideLogic.BuildCoreRows(club, 0));
        }
    }

    public sealed class LocationGuideLogicTests
    {
        private static SaveData WithClears(params LocationId[] cleared)
        {
            SaveData save = SaveDataLogic.CreateDefault();
            List<LocationStats> records = new List<LocationStats>();

            foreach (LocationId id in cleared)
            {
                records.Add(new LocationStats { Location = (int)id, Attempts = 1, Clears = 1 });
            }

            save.LocationRecords = records.ToArray();

            return save;
        }

        [Test]
        public void Recommended_Is_Easiest_Uncleared()
        {
            List<LocationId> all = new List<LocationId>();

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                all.Add(location.Id);
            }

            Assert.AreEqual(LocationId.TrainingCenter, LocationGuideLogic.GetRecommended(all, null));

            // 연수원을 깨면 난이도 2 중 목표가 낮은 곳(해변가 · 지하철 330 동률 → 앞 순서)
            LocationId? second = LocationGuideLogic.GetRecommended(all, WithClears(LocationId.TrainingCenter));

            Assert.IsNotNull(second);
            Assert.AreEqual(2, LocationGuideCatalog.Get(second.Value).Difficulty);

            Assert.IsNull(LocationGuideLogic.GetRecommended(all, WithClears(all.ToArray())));
            Assert.IsNull(LocationGuideLogic.GetRecommended(null, null));
        }

        [Test]
        public void First_Visit_Uses_Attempts()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.IsTrue(LocationGuideLogic.IsFirstVisit(save, LocationId.Beach));
            Assert.IsTrue(LocationGuideLogic.IsFirstVisit(null, LocationId.Beach));

            save.LocationRecords = new[] { new LocationStats { Location = (int)LocationId.Beach, Attempts = 1 } };

            Assert.IsFalse(LocationGuideLogic.IsFirstVisit(save, LocationId.Beach));
            Assert.IsTrue(LocationGuideLogic.IsFirstVisit(save, LocationId.NightMarket));
        }

        [Test]
        public void Formats()
        {
            Assert.AreEqual("◆◆◇◇◇", LocationGuideLogic.FormatDifficulty(2));
            Assert.AreEqual("◆◇◇◇◇", LocationGuideLogic.FormatDifficulty(-4));
            Assert.AreEqual("◆◆◆◆◆", LocationGuideLogic.FormatDifficulty(99));
            Assert.AreEqual("05", LocationGuideLogic.FormatNumber(4));
            Assert.AreEqual("01", LocationGuideLogic.FormatNumber(-1));
        }

        [Test]
        public void Detail_Toggle_And_Titles()
        {
            Assert.AreEqual(LocationDetailKind.Rules, LocationGuideLogic.Toggle(LocationDetailKind.None, LocationDetailKind.Rules));
            Assert.AreEqual(LocationDetailKind.None, LocationGuideLogic.Toggle(LocationDetailKind.Rules, LocationDetailKind.Rules));
            Assert.AreEqual(LocationDetailKind.Record, LocationGuideLogic.Toggle(LocationDetailKind.Rules, LocationDetailKind.Record));

            Assert.AreEqual("적 정보", LocationGuideLogic.GetDetailTitle(LocationDetailKind.Enemies));
            Assert.AreEqual(string.Empty, LocationGuideLogic.GetDetailTitle(LocationDetailKind.None));
        }

        [Test]
        public void Objective_Guides_Differ()
        {
            StringAssert.Contains("탈출", LocationGuideLogic.GetObjectiveGuide(LocationObjective.EssenceQuota));
            StringAssert.Contains("열차", LocationGuideLogic.GetObjectiveGuide(LocationObjective.Survival));
            StringAssert.Contains("함락", LocationGuideLogic.GetObjectiveGuide(LocationObjective.Boss));
        }

        [Test]
        public void Detail_Texts_Have_Content()
        {
            LocationDefinition beach = LocationCatalog.Get(LocationId.Beach);
            SaveData save = SaveDataLogic.CreateDefault();

            StringAssert.Contains("헌팅남", LocationGuideLogic.BuildDetail(LocationDetailKind.Enemies, beach, save));
            StringAssert.Contains("밀물", LocationGuideLogic.BuildDetail(LocationDetailKind.Rules, beach, save));
            StringAssert.Contains("공략 팁", LocationGuideLogic.BuildDetail(LocationDetailKind.Rules, beach, save));
            StringAssert.Contains("실패", LocationGuideLogic.BuildDetail(LocationDetailKind.Rewards, beach, save));
            StringAssert.Contains("아직 도전하지 않은", LocationGuideLogic.BuildDetail(LocationDetailKind.Record, beach, save));
            Assert.AreEqual(string.Empty, LocationGuideLogic.BuildDetail(LocationDetailKind.None, beach, save));

            save.LocationRecords = new[]
            {
                new LocationStats { Location = (int)LocationId.Beach, Attempts = 4, Clears = 2, BestRank = "A", BestEssence = 380, BestClearSeconds = 125f }
            };

            string record = LocationGuideLogic.BuildRecordText(PlayStatsLogic.Get(save, (int)LocationId.Beach));

            StringAssert.Contains("도전          4회", record);
            StringAssert.Contains("50%", record);
            StringAssert.Contains("2:05", record);
            StringAssert.Contains("다음 ★까지 클리어 1회", record);
        }

        [Test]
        public void Reward_Text_Lists_Location_Achievements()
        {
            LocationDefinition club = LocationCatalog.Get(LocationId.RooftopClub);
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.Greater(LocationGuideLogic.GetLocationAchievements(LocationId.RooftopClub).Count, 0);
            StringAssert.Contains("이 장소의 업적", LocationGuideLogic.BuildRewardText(club, PlayStatsLogic.Get(save, (int)club.Id), save));

            save.UnlockedAchievements = new[] { "s_club" };
            StringAssert.Contains("달성", LocationGuideLogic.BuildRewardText(club, PlayStatsLogic.Get(save, (int)club.Id), save));
        }
    }

    public sealed class IntroLogicTests
    {
        [Test]
        public void Five_Pages_With_Text()
        {
            Assert.AreEqual(5, IntroLogic.PageCount);

            foreach (IntroPage page in IntroLogic.Pages)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(page.Title));
                Assert.IsFalse(string.IsNullOrWhiteSpace(page.Body));
            }
        }

        [Test]
        public void Paging()
        {
            Assert.AreEqual(1, IntroLogic.Next(0));
            Assert.AreEqual(5, IntroLogic.Next(4));
            Assert.AreEqual(5, IntroLogic.Next(9));
            Assert.IsFalse(IntroLogic.IsFinished(4));
            Assert.IsTrue(IntroLogic.IsFinished(5));

            Assert.IsTrue(IntroLogic.IsLast(4));
            Assert.AreEqual("다음  ▶", IntroLogic.GetAdvanceLabel(0));
            StringAssert.Contains("시작하기", IntroLogic.GetAdvanceLabel(4));

            Assert.AreEqual("2 / 5", IntroLogic.GetCounter(1));
            Assert.AreEqual("5 / 5", IntroLogic.GetCounter(99));
        }

        [Test]
        public void Fade()
        {
            Assert.AreEqual(0f, IntroLogic.GetFade(0f));
            Assert.AreEqual(1f, IntroLogic.GetFade(10f));
            Assert.AreEqual(0.5f, IntroLogic.GetFade(IntroLogic.FadeSeconds * 0.5f), 0.001f);
        }
    }
}
