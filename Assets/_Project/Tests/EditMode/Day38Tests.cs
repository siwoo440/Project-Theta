using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class BuildInfoTests
    {
        [Test]
        public void Names_And_Paths()
        {
            Assert.AreEqual("v0.38", BuildInfo.GetVersionLabel(false));
            StringAssert.Contains("개발 빌드", BuildInfo.GetVersionLabel(true));
            StringAssert.Contains("F3", BuildInfo.GetFooter(false));

            Assert.AreEqual("Builds/Windows/ProjectTheta.exe", BuildInfo.GetOutputPath(false));
            Assert.AreEqual("Builds/WindowsDev/ProjectTheta.exe", BuildInfo.GetOutputPath(true));
            Assert.IsFalse(string.IsNullOrEmpty(BuildInfo.ProductName));
            Assert.AreNotEqual("DefaultCompany", BuildInfo.CompanyName);
        }

        [Test]
        public void Scene_Check()
        {
            string[] good =
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Hub.unity",
                "Assets/_Project/Scenes/Map.unity",
                "Assets/_Project/Scenes/TestStage.unity"
            };

            Assert.AreEqual(0, BuildInfo.CheckScenes(good).Count);

            // 첫 씬이 Boot가 아니면 저장 · 설정을 읽기 전에 메뉴가 뜬다.
            List<string> swapped = BuildInfo.CheckScenes(new[] { good[1], good[0], good[2], good[3], good[4] });
            Assert.AreEqual(1, swapped.Count);
            StringAssert.Contains("첫 씬", swapped[0]);

            List<string> missing = BuildInfo.CheckScenes(new[] { good[0], good[1], good[2], good[3] });
            Assert.AreEqual(1, missing.Count);
            StringAssert.Contains("TestStage", missing[0]);

            Assert.AreEqual(1, BuildInfo.CheckScenes(new string[0]).Count);
            Assert.AreEqual(1, BuildInfo.CheckScenes(null).Count);

            Assert.AreEqual("Boot", BuildInfo.GetSceneName(@"Assets\_Project\Scenes\Boot.unity"));
            Assert.AreEqual(string.Empty, BuildInfo.GetSceneName(null));
        }

        [Test]
        public void Size_And_Report()
        {
            Assert.AreEqual("512 B", BuildInfo.FormatSize(512));
            Assert.AreEqual("1.5 KB", BuildInfo.FormatSize(1536));
            Assert.AreEqual("12.0 MB", BuildInfo.FormatSize(12L * 1024 * 1024));
            Assert.AreEqual("1.50 GB", BuildInfo.FormatSize(1536L * 1024 * 1024));
            Assert.AreEqual("0 B", BuildInfo.FormatSize(-5));

            string report =
                BuildInfo.FormatReport(
                    new BuildReportInfo
                    {
                        Succeeded = true,
                        Seconds = 42.3,
                        TotalBytes = 200L * 1024 * 1024,
                        Scenes = new[] { "Boot", "MainMenu" },
                        Problems = new List<string> { "효과음 Save 파일이 없어 대체음이 나옵니다." }
                    });

            StringAssert.Contains("성공", report);
            StringAssert.Contains("42.3초", report);
            StringAssert.Contains("200.0 MB", report);
            StringAssert.Contains("Boot → MainMenu", report);
            StringAssert.Contains("확인할 것", report);

            StringAssert.DoesNotContain("확인할 것", BuildInfo.FormatReport(new BuildReportInfo()));
        }
    }

    public sealed class PerfLogicTests
    {
        [Test]
        public void Grades()
        {
            Assert.AreEqual(PerfGrade.Good, PerfLogic.GetGrade(60f));
            Assert.AreEqual(PerfGrade.Good, PerfLogic.GetGrade(30f));
            Assert.AreEqual(PerfGrade.Warn, PerfLogic.GetGrade(29f));
            Assert.AreEqual(PerfGrade.Bad, PerfLogic.GetGrade(19f));
            Assert.AreEqual(PerfGrade.Bad, PerfLogic.GetGrade(float.NaN));

            Assert.AreEqual(60f, PerfLogic.ToFps(1f / 60f), 0.01f);
            Assert.AreEqual(0f, PerfLogic.ToFps(0f));
        }

        [Test]
        public void Sampler_Average_And_Slow_Frames()
        {
            PerfSampler sampler = new PerfSampler();

            Assert.AreEqual(0f, sampler.Compute().Fps);

            // 60FPS 198프레임 + 10FPS 2프레임
            for (int i = 0; i < 198; i++)
            {
                sampler.Add(1f / 60f);
            }

            sampler.Add(0.1f);
            sampler.Add(0.1f);

            PerfSnapshot snapshot = sampler.Compute();

            Assert.AreEqual(200, sampler.Count);
            Assert.AreEqual(100f, snapshot.WorstMs, 0.01f);
            Assert.AreEqual(10f, snapshot.LowFps, 0.01f);
            Assert.Greater(snapshot.Fps, 50f);
            Assert.Less(snapshot.Fps, 60f);
            Assert.AreEqual(-1, snapshot.Npcs);

            // 잘못된 값은 넣지 않는다.
            sampler.Add(0f);
            sampler.Add(float.NaN);
            sampler.Add(float.PositiveInfinity);
            Assert.AreEqual(200, sampler.Count);

            sampler.Clear();
            Assert.AreEqual(0, sampler.Count);
        }

        [Test]
        public void Sampler_Only_Looks_At_Recent_Window()
        {
            PerfSampler sampler = new PerfSampler(100, 1f);

            // 오래된 느린 프레임 뒤에 빠른 프레임이 1초 넘게 이어지면 느린 프레임은 창 밖이다.
            sampler.Add(0.25f);

            for (int i = 0; i < 70; i++)
            {
                sampler.Add(1f / 60f);
            }

            Assert.AreEqual(60f, sampler.Compute().LowFps, 0.5f);

            // 링 버퍼가 넘쳐도 크기를 넘지 않는다.
            for (int i = 0; i < 500; i++)
            {
                sampler.Add(0.001f);
            }

            Assert.AreEqual(100, sampler.Count);
        }

        [Test]
        public void Run_Stats()
        {
            PerfRunStats run = new PerfRunStats();

            for (int i = 0; i < 59; i++)
            {
                run.Add(1f / 60f);
            }

            run.Add(0.05f);

            // 로딩처럼 한 번 튀는 프레임은 빼고, 잘못된 값도 뺀다.
            run.Add(2f);
            run.Add(-1f);

            Assert.AreEqual(60, run.Frames);
            Assert.AreEqual(1, run.Hitches);
            Assert.AreEqual(0.05f, run.WorstSeconds, 0.0001f);
            Assert.Greater(run.AverageFps, 55f);

            run.GcCount = 3;

            string line = PerfLogic.FormatRunLine("2026-09-17 12:00:00", "RooftopClub", true, run);

            StringAssert.Contains("RooftopClub ☾", line);
            StringAssert.Contains("최장 50 ms", line);
            StringAssert.Contains("끊김 1회", line);
            StringAssert.Contains("GC 3회", line);

            run.Reset();
            Assert.AreEqual(0, run.Frames);
            Assert.AreEqual(0f, run.AverageFps);
        }

        [Test]
        public void Overlay_Text()
        {
            PerfSnapshot outside =
                new PerfSnapshot
                {
                    Fps = 60f,
                    AverageMs = 16.6f,
                    LowFps = 48f,
                    WorstMs = 25f,
                    GcCount = 2,
                    ManagedBytes = 50L * 1024 * 1024,
                    Npcs = -1
                };

            string text = PerfLogic.Format(outside);

            StringAssert.Contains("FPS 60", text);
            StringAssert.Contains("1% 느린 48 FPS", text);
            StringAssert.Contains("50.0 MB", text);
            StringAssert.DoesNotContain("NPC", text);

            outside.Npcs = 24;
            outside.Disruptors = 3;

            StringAssert.Contains("NPC 24  ·  방해자 3", PerfLogic.Format(outside));
        }

        [Test]
        public void Save_Folder_Text()
        {
            Assert.AreEqual(
                "file:///C:/Users/My%20Name/AppData/LocalLow/siwoo440/Project%20Theta",
                Save.SettingsLogic.GetFolderUrl(@"C:\Users\My Name\AppData\LocalLow\siwoo440\Project Theta"));

            Assert.AreEqual("file:///home/a", Save.SettingsLogic.GetFolderUrl("/home/a"));
            Assert.AreEqual(string.Empty, Save.SettingsLogic.GetFolderUrl(null));

            Assert.AreEqual("C:/a/b", Save.SettingsLogic.GetFolderLabel(@"C:\a\b"));
            Assert.AreEqual("-", Save.SettingsLogic.GetFolderLabel(string.Empty));

            string longLabel = Save.SettingsLogic.GetFolderLabel(new string('x', 100), 20);

            Assert.AreEqual(20, longLabel.Length);
            StringAssert.StartsWith("…", longLabel);
        }

        [Test]
        public void Old_Save_Folder_Is_Copied_Once()
        {
            string current = @"C:\Users\me\AppData\LocalLow\siwoo440\Project Theta";

            Assert.AreEqual(
                @"C:\Users\me\AppData\LocalLow\DefaultCompany\Project-Theta".Replace('\\', '/'),
                Save.SaveSlotLogic.GetOldFolder(current).Replace('\\', '/'));

            // 이미 예전 폴더를 쓰고 있으면(빌드 도구를 한 번도 안 돌림) 가져올 것이 없다.
            Assert.AreEqual(string.Empty, Save.SaveSlotLogic.GetOldFolder(@"C:\Users\me\AppData\LocalLow\DefaultCompany\Project-Theta\"));
            Assert.AreEqual(string.Empty, Save.SaveSlotLogic.GetOldFolder(null));
            Assert.AreEqual(string.Empty, Save.SaveSlotLogic.GetOldFolder("Project Theta"));

            List<string> names = Save.SaveSlotLogic.GetCopyFileNames();

            CollectionAssert.Contains(names, Save.SaveSlotLogic.SettingsFileName);
            CollectionAssert.Contains(names, Save.SaveSlotLogic.GetFileName(2));
            CollectionAssert.Contains(names, Save.SaveSlotLogic.LegacyFileName);
            Assert.AreEqual(Save.SaveSlotLogic.SlotCount + 2, names.Count);

            Assert.IsTrue(Save.SaveSlotLogic.ShouldCopyOldFolder(true, false));
            Assert.IsFalse(Save.SaveSlotLogic.ShouldCopyOldFolder(true, true));
            Assert.IsFalse(Save.SaveSlotLogic.ShouldCopyOldFolder(false, false));
        }

        [Test]
        public void F3_Is_Listed_In_Controls()
        {
            string all = string.Empty;

            foreach (ControlRow row in ControlsCatalog.Build(InputBindingLogic.CreateDefault()))
            {
                all += row.Keys + " | ";
            }

            StringAssert.Contains("F3", all);
        }
    }
}
