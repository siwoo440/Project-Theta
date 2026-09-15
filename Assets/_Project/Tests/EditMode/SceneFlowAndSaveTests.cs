using NUnit.Framework;
using ProjectTheta.Core;
using ProjectTheta.Save;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class SceneFlowLogicTests
    {
        [Test]
        public void Boot_Goes_Only_To_Main_Menu()
        {
            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Boot,
                    SceneDestination.MainMenu));

            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.Boot,
                    SceneDestination.Stage));

            Assert.AreEqual(
                SceneDestination.MainMenu,
                SceneFlowLogic.BootDestination);
        }

        [Test]
        public void Main_Menu_Goes_Only_To_Hub()
        {
            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.MainMenu,
                    SceneDestination.Hub));

            // 타이틀에서 스테이지로 바로 갈 수 없다. 허브를 거쳐야 한다.
            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.MainMenu,
                    SceneDestination.Stage));
        }

        [Test]
        public void Hub_Goes_To_Map_Or_Back_To_Title()
        {
            // 21일차: 출격하면 지도를 거친다. 허브에서 스테이지로 바로 가지 않는다.
            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Hub,
                    SceneDestination.Map));

            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.Hub,
                    SceneDestination.Stage));

            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Hub,
                    SceneDestination.MainMenu));
        }

        [Test]
        public void Map_Goes_To_Stage_Or_Gives_Up_To_Hub()
        {
            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Map,
                    SceneDestination.Stage));

            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Map,
                    SceneDestination.Hub));

            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.Map,
                    SceneDestination.MainMenu));

            Assert.AreEqual(
                SceneNames.Map,
                SceneFlowLogic.GetSceneName(
                    SceneDestination.Map));
        }

        [Test]
        public void Stage_Goes_Back_To_Map_Or_Hub()
        {
            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Stage,
                    SceneDestination.Hub));

            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.Stage,
                    SceneDestination.Map));

            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.Stage,
                    SceneDestination.MainMenu));
        }

        [Test]
        public void Legacy_Stage_Name_Is_Still_Recognized()
        {
            Assert.IsTrue(
                SceneFlowLogic.IsStageScene(
                    SceneNames.LegacyStage));

            Assert.IsTrue(
                SceneFlowLogic.CanTransition(
                    SceneNames.LegacyStage,
                    SceneDestination.Hub));
        }

        [Test]
        public void Unknown_Scene_And_None_Destination_Are_Refused()
        {
            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    "SomeOtherScene",
                    SceneDestination.Hub));

            Assert.IsFalse(
                SceneFlowLogic.CanTransition(
                    SceneNames.Hub,
                    SceneDestination.None));
        }

        [Test]
        public void Save_Happens_When_Leaving_The_Stage()
        {
            Assert.IsTrue(
                SceneFlowLogic.ShouldSaveOnTransition(
                    SceneNames.Stage,
                    SceneDestination.Hub));

            // 구역을 클리어하고 지도로 갈 때도 그 구역 결과를 저장한다.
            Assert.IsTrue(
                SceneFlowLogic.ShouldSaveOnTransition(
                    SceneNames.Stage,
                    SceneDestination.Map));

            Assert.IsFalse(
                SceneFlowLogic.ShouldSaveOnTransition(
                    SceneNames.Hub,
                    SceneDestination.Map));

            Assert.IsFalse(
                SceneFlowLogic.ShouldSaveOnTransition(
                    SceneNames.Map,
                    SceneDestination.Stage));
        }

        [Test]
        public void Destination_Maps_To_Scene_Name()
        {
            Assert.AreEqual(
                SceneNames.Hub,
                SceneFlowLogic.GetSceneName(
                    SceneDestination.Hub));

            Assert.AreEqual(
                string.Empty,
                SceneFlowLogic.GetSceneName(
                    SceneDestination.None));
        }
    }

    public sealed class SaveDataLogicTests
    {
        [Test]
        public void Default_Save_Is_Empty_But_Valid()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                data.Version);

            Assert.AreEqual(
                0,
                data.ClearCount);

            Assert.AreEqual(
                SaveDataLogic.UpgradeTrackCount,
                data.UpgradeLevels.Length);

            Assert.AreEqual(
                "-",
                data.BestRankLabel);
        }

        [Test]
        public void Null_Save_Falls_Back_To_Default()
        {
            SaveData data =
                SaveDataLogic.Normalize(
                    null);

            Assert.IsNotNull(
                data);

            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                data.Version);
        }

        [Test]
        public void Corrupt_Save_Is_Repaired()
        {
            SaveData broken =
                new SaveData
                {
                    Version = 0,
                    ClearCount = -5,
                    PlayCount = -3,
                    BestScore = -100,
                    BestRankLabel = null,
                    ContractEssence = -20,
                    UpgradeLevels = new[] { -1, 2 }
                };

            SaveData fixedData =
                SaveDataLogic.Normalize(
                    broken);

            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                fixedData.Version);

            Assert.AreEqual(0, fixedData.ClearCount);
            Assert.AreEqual(0, fixedData.PlayCount);
            Assert.AreEqual(0, fixedData.BestScore);
            Assert.AreEqual(0, fixedData.ContractEssence);
            Assert.AreEqual("-", fixedData.BestRankLabel);

            Assert.AreEqual(
                SaveDataLogic.UpgradeTrackCount,
                fixedData.UpgradeLevels.Length);

            Assert.AreEqual(0, fixedData.UpgradeLevels[0]);
            Assert.AreEqual(2, fixedData.UpgradeLevels[1]);
        }

        [Test]
        public void Play_Count_Is_Never_Below_Clear_Count()
        {
            SaveData data =
                SaveDataLogic.Normalize(
                    new SaveData
                    {
                        ClearCount = 7,
                        PlayCount = 2
                    });

            Assert.AreEqual(
                7,
                data.PlayCount);
        }

        [Test]
        public void Rank_Order_Is_S_A_B_C()
        {
            Assert.Greater(
                SaveDataLogic.GetRankOrder("S"),
                SaveDataLogic.GetRankOrder("A"));

            Assert.Greater(
                SaveDataLogic.GetRankOrder("A"),
                SaveDataLogic.GetRankOrder("B"));

            Assert.Greater(
                SaveDataLogic.GetRankOrder("B"),
                SaveDataLogic.GetRankOrder("C"));

            Assert.Greater(
                SaveDataLogic.GetRankOrder("C"),
                SaveDataLogic.GetRankOrder("-"));
        }

        [Test]
        public void Clear_Increases_Both_Counters()
        {
            SaveData data =
                SaveDataLogic.ApplyStageResult(
                    SaveDataLogic.CreateDefault(),
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 5000,
                        RankLabel = "B"
                    });

            Assert.AreEqual(1, data.PlayCount);
            Assert.AreEqual(1, data.ClearCount);
            Assert.AreEqual(5000, data.BestScore);
            Assert.AreEqual("B", data.BestRankLabel);
        }

        [Test]
        public void Failure_Counts_As_A_Play_But_Not_A_Clear()
        {
            SaveData data =
                SaveDataLogic.ApplyStageResult(
                    SaveDataLogic.CreateDefault(),
                    new StageResultSummary
                    {
                        Cleared = false,
                        TotalScore = 3000,
                        RankLabel = "-"
                    });

            Assert.AreEqual(1, data.PlayCount);
            Assert.AreEqual(0, data.ClearCount);

            // 점수는 실패해도 최고 기록이 될 수 있다.
            Assert.AreEqual(3000, data.BestScore);

            // 랭크는 클리어한 판에서만 갱신된다.
            Assert.AreEqual("-", data.BestRankLabel);
        }

        [Test]
        public void Lower_Records_Do_Not_Overwrite_Best()
        {
            SaveData data =
                SaveDataLogic.ApplyStageResult(
                    SaveDataLogic.CreateDefault(),
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 12000,
                        RankLabel = "A"
                    });

            data =
                SaveDataLogic.ApplyStageResult(
                    data,
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 4000,
                        RankLabel = "C"
                    });

            Assert.AreEqual(12000, data.BestScore);
            Assert.AreEqual("A", data.BestRankLabel);
            Assert.AreEqual(2, data.ClearCount);
        }

        [Test]
        public void Better_Rank_Overwrites_Best()
        {
            SaveData data =
                SaveDataLogic.ApplyStageResult(
                    SaveDataLogic.CreateDefault(),
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 4000,
                        RankLabel = "C"
                    });

            data =
                SaveDataLogic.ApplyStageResult(
                    data,
                    new StageResultSummary
                    {
                        Cleared = true,
                        TotalScore = 20000,
                        RankLabel = "S"
                    });

            Assert.AreEqual("S", data.BestRankLabel);
            Assert.AreEqual(20000, data.BestScore);
        }

        [Test]
        public void Clone_Is_Independent()
        {
            SaveData original =
                SaveDataLogic.CreateDefault();

            original.UpgradeLevels[0] = 3;

            SaveData copy =
                original.Clone();

            copy.UpgradeLevels[0] = 5;
            copy.ClearCount = 9;

            Assert.AreEqual(3, original.UpgradeLevels[0]);
            Assert.AreEqual(0, original.ClearCount);
        }

        [Test]
        public void Migration_Always_Lands_On_Current_Version()
        {
            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                SaveDataLogic.Migrate(0));

            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                SaveDataLogic.Migrate(
                    SaveDataLogic.CurrentVersion));

            // 미래 버전 파일을 만나도 현재 버전으로 다룬다.
            Assert.AreEqual(
                SaveDataLogic.CurrentVersion,
                SaveDataLogic.Migrate(999));
        }
    }
}
