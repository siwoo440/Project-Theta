using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Presentation;

namespace ProjectTheta.EditorTools
{
    /// <summary>
    /// Windows 실행 파일을 만드는 에디터 메뉴다 (38일차).
    ///
    ///   Project Theta/빌드/Windows 실행 파일      → Builds/Windows/ProjectTheta.exe
    ///   Project Theta/빌드/Windows 개발 빌드      → Builds/WindowsDev/ProjectTheta.exe (Profiler 연결 · F1 디버그 창 · 기록 파일)
    ///   Project Theta/빌드/빌드 폴더 열기
    ///
    /// 빌드 전에 씬 목록 · 음원 파일을 확인하고, 제품 이름 · 회사 이름 · 버전을 <see cref="BuildInfo"/> 값으로 맞춘다.
    /// 끝나면 같은 폴더에 build_report.txt를 남긴다.
    ///
    /// 명령줄(Unity를 닫은 상태):
    ///   Unity.exe -batchmode -quit -projectPath F:\Project-Theta
    ///     -executeMethod ProjectTheta.EditorTools.BuildTool.BuildReleaseFromCommandLine -logFile Builds/unity_build.log
    /// </summary>
    public static class BuildTool
    {
        private const string Menu = "Project Theta/빌드/";

        [MenuItem(Menu + "Windows 실행 파일", priority = 1)]
        private static void BuildReleaseMenu()
        {
            Build(false, true);
        }

        [MenuItem(Menu + "Windows 개발 빌드 (Profiler 연결)", priority = 2)]
        private static void BuildDevelopmentMenu()
        {
            Build(true, true);
        }

        [MenuItem(Menu + "빌드 폴더 열기", priority = 20)]
        private static void OpenBuildFolder()
        {
            string folder = GetProjectPath(BuildInfo.BuildRoot);

            Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }

        public static void BuildReleaseFromCommandLine()
        {
            EditorApplication.Exit(Build(false, false) ? 0 : 1);
        }

        public static void BuildDevelopmentFromCommandLine()
        {
            EditorApplication.Exit(Build(true, false) ? 0 : 1);
        }

        private static bool Build(
            bool development,
            bool interactive)
        {
            string[] scenes = GetEnabledScenes();

            List<string> problems = BuildInfo.CheckScenes(scenes);
            List<string> blocking = new List<string>(problems);

            problems.AddRange(CheckAudioFiles());

            if (blocking.Count > 0)
            {
                string message = string.Join("\n", blocking);

                Debug.LogError($"[빌드] 씬 목록을 확인하세요.\n{message}");

                if (interactive)
                {
                    EditorUtility.DisplayDialog("빌드할 수 없습니다", message, "확인");
                }

                return false;
            }

            ApplyPlayerSettings();

            string outputPath = BuildInfo.GetOutputPath(development);

            BuildPlayerOptions options =
                new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = development
                        ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                        : BuildOptions.None
                };

            DateTime started = DateTime.Now;
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            bool succeeded = summary.result == BuildResult.Succeeded;

            BuildReportInfo info =
                new BuildReportInfo
                {
                    Development = development,
                    Succeeded = succeeded,
                    FinishedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Seconds = (DateTime.Now - started).TotalSeconds,
                    TotalBytes = succeeded
                        ? GetFolderBytes(GetProjectPath(BuildInfo.GetOutputFolder(development)))
                        : 0L,
                    Warnings = summary.totalWarnings,
                    Errors = summary.totalErrors,
                    Scenes = GetSceneNames(scenes),
                    Problems = problems
                };

            WriteReport(development, info);

            if (succeeded)
            {
                Debug.Log($"[빌드] 완료: {outputPath} ({BuildInfo.FormatSize(info.TotalBytes)}, {info.Seconds:0.0}초)");
            }
            else
            {
                Debug.LogError($"[빌드] 실패: {summary.result} · 오류 {summary.totalErrors}개. Console을 확인하세요.");
            }

            if (interactive)
            {
                if (succeeded &&
                    EditorUtility.DisplayDialog(
                        "빌드 완료",
                        BuildInfo.FormatReport(info),
                        "폴더 열기",
                        "닫기"))
                {
                    EditorUtility.RevealInFinder(GetProjectPath(outputPath));
                }
                else if (!succeeded)
                {
                    EditorUtility.DisplayDialog("빌드 실패", BuildInfo.FormatReport(info), "확인");
                }
            }

            return succeeded;
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = BuildInfo.ProductName;
            PlayerSettings.companyName = BuildInfo.CompanyName;
            PlayerSettings.bundleVersion = BuildInfo.Version;

            // 같은 PC에서 창을 두 개 띄우면 저장 파일을 서로 덮을 수 있다.
            PlayerSettings.forceSingleInstance = true;
        }

        private static string[] GetEnabledScenes()
        {
            List<string> scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }

        private static string[] GetSceneNames(
            string[] paths)
        {
            string[] names = new string[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                names[i] = BuildInfo.GetSceneName(paths[i]);
            }

            return names;
        }

        /// <summary>음원이 빠져도 대체음으로 돌아가므로 막지는 않고 보고서에만 적는다.</summary>
        private static List<string> CheckAudioFiles()
        {
            List<string> missing = new List<string>();
            string resources = GetProjectPath("Assets/_Project/Resources");

            foreach (MusicTrack track in Enum.GetValues(typeof(MusicTrack)))
            {
                if (track == MusicTrack.None)
                {
                    continue;
                }

                if (!HasAudio(resources, MusicLogic.GetPath(track)))
                {
                    missing.Add($"배경음악 {track} 파일이 없습니다.");
                }
            }

            foreach (GameSfx sfx in Enum.GetValues(typeof(GameSfx)))
            {
                if (!HasAudio(resources, GameAudio.ResourceFolder + "/" + sfx))
                {
                    missing.Add($"효과음 {sfx} 파일이 없어 대체음이 나옵니다.");
                }
            }

            return missing;
        }

        private static bool HasAudio(
            string resources,
            string path)
        {
            foreach (string extension in new[] { ".wav", ".ogg", ".mp3" })
            {
                if (File.Exists(Path.Combine(resources, path + extension)))
                {
                    return true;
                }
            }

            return false;
        }

        private static void WriteReport(
            bool development,
            BuildReportInfo info)
        {
            try
            {
                string folder = GetProjectPath(BuildInfo.GetOutputFolder(development));

                Directory.CreateDirectory(folder);
                File.WriteAllText(
                    Path.Combine(folder, BuildInfo.ReportFileName),
                    BuildInfo.FormatReport(info));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[빌드] 보고서를 쓰지 못했습니다: {exception.Message}");
            }
        }

        private static long GetFolderBytes(
            string folder)
        {
            if (!Directory.Exists(folder))
            {
                return 0L;
            }

            long total = 0L;

            foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                // 이전 보고서 · 디버그 심볼 폴더는 크기에서 뺀다.
                if (file.EndsWith(BuildInfo.ReportFileName, StringComparison.OrdinalIgnoreCase) ||
                    file.Contains("_BurstDebugInformation_DoNotShip"))
                {
                    continue;
                }

                total += new FileInfo(file).Length;
            }

            return total;
        }

        private static string GetProjectPath(
            string relative)
        {
            string root = Path.GetDirectoryName(Application.dataPath);

            return Path.GetFullPath(Path.Combine(root, relative));
        }
    }
}
