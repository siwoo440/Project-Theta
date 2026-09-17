using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Run;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.Story;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    internal static class Day36Saves
    {
        public static SaveData With(params LocationStats[] records)
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.LocationRecords = records;

            return save;
        }

        public static SaveData Ended()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.Stats.Endings = 1;

            return save;
        }

        public static StageResultSummary Result(LocationId id, bool cleared, bool night)
        {
            return new StageResultSummary
            {
                HasLocation = true,
                LocationId = (int)id,
                Cleared = cleared,
                NightMode = night,
                RankLabel = cleared ? "B" : "-",
                PlaySeconds = 120f,
                RecoveredEssence = 300
            };
        }
    }

    public sealed class NightModeLogicTests
    {
        [Test]
        public void Unlocks_After_Ending()
        {
            Assert.IsFalse(NightModeLogic.IsUnlocked(SaveDataLogic.CreateDefault()));
            Assert.IsFalse(NightModeLogic.IsUnlocked(null));
            Assert.IsTrue(NightModeLogic.IsUnlocked(Day36Saves.Ended()));

            Assert.IsFalse(NightModeLogic.Resolve(true, SaveDataLogic.CreateDefault()));
            Assert.IsTrue(NightModeLogic.Resolve(true, Day36Saves.Ended()));
            Assert.IsFalse(NightModeLogic.Resolve(false, Day36Saves.Ended()));
        }

        [Test]
        public void Numbers()
        {
            Assert.AreEqual(403, NightModeLogic.GetTarget(310, true));
            Assert.AreEqual(310, NightModeLogic.GetTarget(310, false));
            Assert.AreEqual(162f, NightModeLogic.GetTimeLimit(180f, true), 0.01f);
            Assert.AreEqual(180f, NightModeLogic.GetTimeLimit(180f, false));
            Assert.AreEqual(7, NightModeLogic.GetChaseMax(6, true));
            Assert.AreEqual(8, NightModeLogic.GetChaseMax(8, true));
            Assert.AreEqual(6, NightModeLogic.GetChaseMax(6, false));
            Assert.AreEqual(12f, NightModeLogic.GetAlertAmount(10f, true), 0.001f);
            Assert.AreEqual(-5f, NightModeLogic.GetAlertAmount(-5f, true));
            Assert.AreEqual(150, NightModeLogic.GetReward(100, true));
            Assert.AreEqual(100, NightModeLogic.GetReward(100, false));
            Assert.AreEqual(0, NightModeLogic.GetReward(-10, true));
        }

        [Test]
        public void Labels()
        {
            StringAssert.Contains("잠김", NightModeLogic.GetToggleLabel(false, false));
            StringAssert.Contains("켜짐", NightModeLogic.GetToggleLabel(true, true));
            StringAssert.Contains("꺼짐", NightModeLogic.GetToggleLabel(true, false));
            StringAssert.Contains("☾", NightModeLogic.GetTitleSuffix(true));
            Assert.AreEqual(string.Empty, NightModeLogic.GetTitleSuffix(false));
            StringAssert.Contains("보상 ×1.5", NightModeLogic.GetShortSummary());
        }

        [Test]
        public void Night_Clear_Is_Recorded_And_Unlocks_Achievement()
        {
            SaveData save = Day36Saves.Ended();

            save = SaveDataLogic.ApplyStageResult(save, Day36Saves.Result(LocationId.Beach, true, true));
            save = SaveDataLogic.ApplyStageResult(save, Day36Saves.Result(LocationId.Beach, false, true));
            save = SaveDataLogic.ApplyStageResult(save, Day36Saves.Result(LocationId.NightMarket, true, false));

            Assert.AreEqual(1, PlayStatsLogic.Get(save, (int)LocationId.Beach).NightClears);
            Assert.AreEqual(0, PlayStatsLogic.Get(save, (int)LocationId.NightMarket).NightClears);
            Assert.AreEqual(1, AchievementLogic.GetValue(save, AchievementStat.NightLocations));
            Assert.IsTrue(AchievementLogic.IsUnlocked(save, "night_1"));
            Assert.IsFalse(AchievementLogic.IsUnlocked(save, "night_4"));

            StringAssert.Contains("(☾1)", PlayStatsLogic.BuildLocationRow(PlayStatsLogic.Get(save, (int)LocationId.Beach))[1]);
        }

        [Test]
        public void Report_Groups_Night_Runs_Separately()
        {
            RunLogEntry day = new RunLogEntry { Location = "Beach", Result = "Cleared", Stats = new RunStats(2) { TotalSeconds = 100f } };
            RunLogEntry night = new RunLogEntry { Location = "Beach", Result = "Cleared", NightMode = true, Stats = new RunStats(2) { TotalSeconds = 150f } };

            List<BalanceReportRow> rows = BalanceReportLogic.Build(new List<RunLogEntry> { day, night }, out _, out _);

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("Beach", rows[0].Location);
            Assert.AreEqual("Beach ☾", rows[1].Location);
        }

        [Test]
        public void Map_Core_Rows_Show_Night_Numbers()
        {
            LocationDefinition training = LocationCatalog.Get(LocationId.TrainingCenter);

            StringAssert.Contains("목표 정기  310", LocationGuideLogic.BuildCoreRows(training, 0));
            StringAssert.Contains("목표 정기  403", LocationGuideLogic.BuildCoreRows(training, 0, true));
            StringAssert.Contains("2:42", LocationGuideLogic.BuildCoreRows(training, 0, true));
        }
    }

    public sealed class DominionLogicTests
    {
        [Test]
        public void Points_Per_Location()
        {
            Assert.AreEqual(0, DominionLogic.GetPoints(null));
            Assert.AreEqual(0, DominionLogic.GetPoints(new LocationStats { Attempts = 3 }));
            Assert.AreEqual(2, DominionLogic.GetPoints(new LocationStats { Clears = 1 }));
            Assert.AreEqual(4, DominionLogic.GetPoints(new LocationStats { Clears = 6 }));
            Assert.AreEqual(5, DominionLogic.GetPoints(new LocationStats { Clears = 6, NightClears = 1 }));
            Assert.AreEqual(40, DominionLogic.MaxPoints);
        }

        [Test]
        public void Percent_And_Titles()
        {
            Assert.AreEqual(0, DominionLogic.GetPercent(SaveDataLogic.CreateDefault()));
            Assert.AreEqual("신참 서큐버스", DominionLogic.GetTitle(0));

            // 2곳 × 5점 = 10 / 40 = 25%
            SaveData quarter = Day36Saves.With(
                new LocationStats { Location = 0, Clears = 6, NightClears = 1 },
                new LocationStats { Location = 1, Clears = 6, NightClears = 1 });

            Assert.AreEqual(25, DominionLogic.GetPercent(quarter));
            Assert.AreEqual("밤거리 단골", DominionLogic.GetTitle(25));
            Assert.AreEqual(50, DominionLogic.GetNextThreshold(25));

            List<LocationStats> all = new List<LocationStats>();

            for (int i = 0; i < 8; i++)
            {
                all.Add(new LocationStats { Location = i, Clears = 6, NightClears = 1 });
            }

            SaveData full = Day36Saves.With(all.ToArray());

            Assert.AreEqual(100, DominionLogic.GetPercent(full));
            Assert.AreEqual(1f, DominionLogic.GetRatio(full));
            Assert.AreEqual("도시의 주인", DominionLogic.GetTitle(100));
            Assert.AreEqual(-1, DominionLogic.GetNextThreshold(100));
            Assert.IsFalse(DominionLogic.GetSummary(full).Contains("다음 칭호"));

            // 39점은 반올림해도 100이 아니다.
            all[7].NightClears = 0;
            Assert.AreEqual(97, DominionLogic.GetPercent(Day36Saves.With(all.ToArray())));
        }

        [Test]
        public void Describe_Location()
        {
            string text = DominionLogic.DescribeLocation(new LocationStats { Clears = 3 });

            StringAssert.Contains("클리어 ✓", text);
            StringAssert.Contains("★★☆", text);
            StringAssert.Contains("☾ ✗", text);
            StringAssert.Contains("3/5", text);
        }

        [Test]
        public void Slot_Summary_Shows_Dominion()
        {
            SaveData save = Day36Saves.With(new LocationStats { Location = 0, Clears = 1 });
            SaveSlotSummary summary = SaveSlotLogic.Summarize(0, save);

            Assert.AreEqual(5, summary.DominionPercent);
            StringAssert.Contains("도시 지배도 5%", SaveSlotLogic.Describe(summary, 8));
        }
    }

    public sealed class StoryLogicTests
    {
        [Test]
        public void Catalog_Is_Complete()
        {
            HashSet<string> ids = new HashSet<string>();

            foreach (StoryScene scene in StoryCatalog.All)
            {
                Assert.IsTrue(ids.Add(scene.Id), scene.Id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(scene.Title), scene.Id);
                Assert.GreaterOrEqual(scene.Lines.Length, 2, scene.Id);

                foreach (StoryLine line in scene.Lines)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line.Text), scene.Id);
                }
            }

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.AreEqual(2, StoryLogic.CountForLocation(location.Id), location.Id.ToString());
            }

            Assert.AreEqual(20, StoryCatalog.All.Length);
            Assert.AreSame(StoryCatalog.All[0], StoryCatalog.Get(StoryCatalog.All[0].Id));
            Assert.IsNull(StoryCatalog.Get("nope"));
            Assert.IsNull(StoryCatalog.Get(null));
        }

        [Test]
        public void Enter_Scene_Once()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            StoryScene scene = StoryLogic.GetEnterScene(save, LocationId.RooftopClub);

            Assert.AreEqual("enter_club", scene.Id);
            Assert.IsTrue(StoryLogic.MarkSeen(save, scene.Id));
            Assert.IsFalse(StoryLogic.MarkSeen(save, scene.Id));
            Assert.IsNull(StoryLogic.GetEnterScene(save, LocationId.RooftopClub));
            Assert.IsFalse(StoryLogic.MarkSeen(null, "x"));
            Assert.IsFalse(StoryLogic.MarkSeen(save, null));
        }

        [Test]
        public void Pending_Follows_Progress()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            Assert.AreEqual(0, StoryLogic.GetPending(save).Count);

            // 3곳 클리어 → 클리어 장면 3개 + 도발 1개
            save.LocationRecords = new[]
            {
                new LocationStats { Location = (int)LocationId.TrainingCenter, Attempts = 1, Clears = 1 },
                new LocationStats { Location = (int)LocationId.Beach, Attempts = 1, Clears = 1 },
                new LocationStats { Location = (int)LocationId.SubwayStation, Attempts = 1, Clears = 1 }
            };

            List<StoryScene> pending = StoryLogic.GetPending(save);
            List<string> ids = pending.ConvertAll(s => s.Id);

            CollectionAssert.AreEqual(new[] { "clear_training", "clear_beach", "clear_subway", "rival_taunt_3" }, ids);

            foreach (StoryScene scene in pending)
            {
                StoryLogic.MarkSeen(save, scene.Id);
            }

            Assert.AreEqual(0, StoryLogic.GetPending(save).Count);

            // 엔딩 → 후일담
            save.Stats.Endings = 1;
            CollectionAssert.Contains(StoryLogic.GetPending(save).ConvertAll(s => s.Id), "epilogue_ending");
            CollectionAssert.DoesNotContain(StoryLogic.GetPending(save).ConvertAll(s => s.Id), "epilogue_dominion");
        }

        [Test]
        public void Seen_Lists_And_Transcript()
        {
            SaveData save = SaveDataLogic.CreateDefault();

            StringAssert.Contains("아직 본 이야기가 없습니다", StoryLogic.BuildTranscript(StoryLogic.GetSeenForLocation(save, LocationId.Beach), 2));

            StoryLogic.MarkSeen(save, "enter_beach");
            StoryLogic.MarkSeen(save, "rival_taunt_3");

            Assert.AreEqual(2, StoryLogic.GetSeen(save).Count);
            Assert.AreEqual(1, StoryLogic.GetSeenForLocation(save, LocationId.Beach).Count);

            string transcript = StoryLogic.BuildTranscript(StoryLogic.GetSeenForLocation(save, LocationId.Beach), 2);

            StringAssert.Contains("본 이야기 1 / 2", transcript);
            StringAssert.Contains("뜨거운 백사장", transcript);
            StringAssert.Contains("라이프가드", transcript);

            StringAssert.Contains("뜨거운 백사장", LocationGuideLogic.BuildDetail(LocationDetailKind.Story, LocationCatalog.Get(LocationId.Beach), save));
            Assert.AreEqual("이야기", LocationGuideLogic.GetDetailTitle(LocationDetailKind.Story));
        }

        [Test]
        public void Typewriter()
        {
            Assert.AreEqual(0, StoryLogic.GetVisibleCharacters(0f, 10));
            Assert.AreEqual(4, StoryLogic.GetVisibleCharacters(0.1f, 10));
            Assert.AreEqual(10, StoryLogic.GetVisibleCharacters(99f, 10));
            Assert.IsTrue(StoryLogic.IsLineComplete(StoryLogic.GetLineSeconds(10), 10));
            Assert.IsFalse(StoryLogic.IsLineComplete(0.01f, 10));
        }

        [Test]
        public void Speaker_Colors_Differ()
        {
            Assert.AreNotEqual(StoryLogic.GetSpeakerColor(StoryCatalog.Me), StoryLogic.GetSpeakerColor(StoryCatalog.Rival));
            Assert.AreNotEqual(StoryLogic.GetSpeakerColor(string.Empty), StoryLogic.GetSpeakerColor("교육 조교"));
        }

        [Test]
        public void Old_Save_Without_Stories_Loads()
        {
            SaveData save = SaveDataLogic.CreateDefault();
            save.SeenStories = null;

            SaveData normalized = SaveDataLogic.Normalize(save);

            Assert.IsNotNull(normalized.SeenStories);
            Assert.IsFalse(StoryLogic.IsSeen(normalized, "enter_beach"));

            normalized.SeenStories = new[] { "enter_beach" };
            CollectionAssert.AreEqual(new[] { "enter_beach" }, normalized.Clone().SeenStories);
        }
    }

    public sealed class HubRoomLogicTests
    {
        [Test]
        public void Toggle_And_Objects()
        {
            Assert.AreEqual(HubPanel.Upgrades, HubRoomLogic.Toggle(HubPanel.None, HubPanel.Upgrades));
            Assert.AreEqual(HubPanel.None, HubRoomLogic.Toggle(HubPanel.Upgrades, HubPanel.Upgrades));
            Assert.AreEqual(HubPanel.Stats, HubRoomLogic.Toggle(HubPanel.Upgrades, HubPanel.Stats));

            Assert.AreEqual(HubPanel.Upgrades, HubRoomLogic.GetPanel(HubRoomObject.Notebook));
            Assert.AreEqual(HubPanel.Difficulty, HubRoomLogic.GetPanel(HubRoomObject.Clock));
            Assert.AreEqual(HubPanel.Stats, HubRoomLogic.GetPanel(HubRoomObject.Bookshelf));
            Assert.AreEqual(HubPanel.Diary, HubRoomLogic.GetPanel(HubRoomObject.Diary));
            Assert.AreEqual(HubPanel.None, HubRoomLogic.GetPanel(HubRoomObject.Window));
            Assert.IsTrue(HubRoomLogic.IsSortie(HubRoomObject.Window));
            Assert.IsFalse(HubRoomLogic.IsSortie(HubRoomObject.Notebook));
        }

        [Test]
        public void Labels_Exist()
        {
            foreach (HubPanel panel in HubRoomLogic.BottomPanels)
            {
                Assert.IsFalse(string.IsNullOrEmpty(HubRoomLogic.GetPanelTitle(panel)), panel.ToString());
                Assert.IsFalse(string.IsNullOrEmpty(HubRoomLogic.GetButtonLabel(panel)), panel.ToString());
            }

            StringAssert.Contains("강화", HubRoomLogic.GetObjectLabel(HubRoomObject.Notebook));
            StringAssert.Contains("출격", HubRoomLogic.GetObjectLabel(HubRoomObject.Window));
            Assert.AreEqual(string.Empty, HubRoomLogic.GetObjectLabel(HubRoomObject.None));
        }

        [Test]
        public void Background_Stays_Visible()
        {
            Assert.Less(HubRoomLogic.WindowDim, 0.5f);
            Assert.Less(HubRoomLogic.WindowAlpha, 1f);
            Assert.Less(HubRoomLogic.BarAlpha, 1f);
        }

        [Test]
        public void City_Lights_And_Clock()
        {
            Assert.AreEqual(0, HubRoomLogic.GetTintedLights(0f, 40));
            Assert.AreEqual(10, HubRoomLogic.GetTintedLights(0.25f, 40));
            Assert.AreEqual(40, HubRoomLogic.GetTintedLights(3f, 40));
            Assert.AreEqual(0, HubRoomLogic.GetTintedLights(0.5f, 0));

            Assert.AreEqual(-90f, HubRoomLogic.GetHourAngle(15, 0), 0.001f);
            Assert.AreEqual(-105f, HubRoomLogic.GetHourAngle(3, 30), 0.001f);
            Assert.AreEqual(-180f, HubRoomLogic.GetMinuteAngle(30), 0.001f);
        }
    }

    public sealed class HubSuccubusLogicTests
    {
        [Test]
        public void Spot_Never_Repeats_And_Covers_All()
        {
            System.Random random = new System.Random(1);
            HashSet<int> seen = new HashSet<int>();
            int last = -1;

            for (int i = 0; i < 300; i++)
            {
                int spot = HubSuccubusLogic.PickSpot(random, last);

                Assert.That(spot, Is.InRange(0, HubSuccubusLogic.Spots.Length - 1));
                Assert.AreNotEqual(last, spot);

                seen.Add(spot);
                last = spot;
            }

            Assert.AreEqual(HubSuccubusLogic.Spots.Length, seen.Count);
            Assert.AreEqual(6, HubSuccubusLogic.Spots.Length);
        }

        [Test]
        public void Spots_Stay_Inside_The_Room()
        {
            foreach (HubSuccubusSpot spot in HubSuccubusLogic.Spots)
            {
                float height = HubSuccubusLogic.GetHeight(spot.Pose);

                // 아래 띠(96px) 위, 위 띠(76px) 아래, 좌우 화면 안
                Assert.Greater(spot.Y, -540f + 96f, spot.Name);
                Assert.Less(spot.Y + height, 540f - 76f, spot.Name);
                Assert.Less(System.Math.Abs(spot.X) + height * 0.3f, 960f, spot.Name);
            }
        }

        [Test]
        public void Art_Paths()
        {
            Assert.AreEqual("Characters/Succubus/Room_Stand", HubSuccubusLogic.GetArtPath(HubSuccubusPose.Stand));
            Assert.AreEqual("Characters/Succubus/Room_Sit", HubSuccubusLogic.GetArtPath(HubSuccubusPose.Sit));
            Assert.AreEqual("Characters/Succubus/Idle", HubSuccubusLogic.FallbackArtPath);
            Assert.Greater(HubSuccubusLogic.GetHeight(HubSuccubusPose.Stand), HubSuccubusLogic.GetHeight(HubSuccubusPose.Sit));
        }

        [Test]
        public void Lines_Follow_Progress()
        {
            List<string> fresh = HubSuccubusLogic.GetLines(SaveDataLogic.CreateDefault());

            Assert.IsTrue(fresh.Exists(l => l.Contains("연수원부터")));
            Assert.IsFalse(fresh.Exists(l => l.Contains("심야 모드")));
            Assert.AreEqual(HubSuccubusLogic.CommonLines.Length, HubSuccubusLogic.GetLines(null).Count);

            SaveData ended = Day36Saves.Ended();
            ended.Stats.Attempts = 5;
            ended.ContractEssence = 0;

            List<string> after = HubSuccubusLogic.GetLines(ended);

            Assert.IsFalse(after.Exists(l => l.Contains("연수원부터")));
            Assert.IsTrue(after.Exists(l => l.Contains("심야 모드")));
            Assert.IsFalse(after.Exists(l => l.Contains("강화해 볼까")));

            ended.ContractEssence = 99999;
            Assert.IsTrue(HubSuccubusLogic.CanBuyAnyUpgrade(ended));
            Assert.IsTrue(HubSuccubusLogic.GetLines(ended).Exists(l => l.Contains("강화해 볼까")));

            foreach (string line in HubSuccubusLogic.GetLines(ended))
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line));
            }
        }

        [Test]
        public void Pick_Line_Avoids_Last()
        {
            System.Random random = new System.Random(3);
            List<string> lines = new List<string> { "a", "b" };

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual("b", HubSuccubusLogic.PickLine(lines, random, "a"));
            }

            Assert.AreEqual("a", HubSuccubusLogic.PickLine(new List<string> { "a" }, random, "a"));
            Assert.AreEqual(string.Empty, HubSuccubusLogic.PickLine(null, random, null));
        }

        [Test]
        public void Bubble_Time_And_Flip()
        {
            Assert.AreEqual(2.5f, HubSuccubusLogic.GetBubbleSeconds("짧음"));
            Assert.AreEqual(6f, HubSuccubusLogic.GetBubbleSeconds(new string('가', 200)));
            Assert.AreEqual(2.5f, HubSuccubusLogic.GetBubbleSeconds(null));

            // 오른쪽 위에 두면 화면 밖으로 나가는 자리만 뒤집는다.
            Assert.IsFalse(HubSuccubusLogic.ShouldFlip(0f, 30f, HubSuccubusLogic.BubbleWidth));
            Assert.IsTrue(HubSuccubusLogic.ShouldFlip(700f, 30f, HubSuccubusLogic.BubbleWidth));
        }
    }
}
