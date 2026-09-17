using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 실행 파일 이름 · 버전 · 빌드 규칙이다 (38일차).
    ///
    /// 에디터 빌드 도구(<c>Editor/BuildTool</c>)가 빌드할 때 이 값으로 플레이어 설정을 맞추고,
    /// 게임 안에서는 메인 메뉴 아래 버전 표기에 쓴다.
    /// 에디터 코드와 떨어져 있어야 테스트할 수 있어서 런타임 쪽에 둔다.
    /// </summary>
    public static class BuildInfo
    {
        public const string ProductName = "Project Theta";
        public const string CompanyName = "siwoo440";

        /// <summary>일차가 버전이다. 38일차 → 0.38.</summary>
        public const string Version = "0.38";

        public const string ExecutableName = "ProjectTheta.exe";
        public const string BuildRoot = "Builds";
        public const string ReleaseFolder = "Windows";
        public const string DevelopmentFolder = "WindowsDev";
        public const string ReportFileName = "build_report.txt";

        /// <summary>빌드에 들어가야 하는 씬 순서다. 첫 씬이 Boot여야 저장 · 설정이 먼저 읽힌다.</summary>
        public static readonly string[] RequiredScenes =
        {
            SceneNames.Boot,
            SceneNames.MainMenu,
            SceneNames.Hub,
            SceneNames.Map,
            SceneNames.Stage
        };

        public static string GetVersionLabel(
            bool developmentBuild)
        {
            return developmentBuild
                ? $"v{Version} · 개발 빌드"
                : $"v{Version}";
        }

        /// <summary>메인 메뉴 맨 아래 한 줄이다.</summary>
        public static string GetFooter(
            bool developmentBuild)
        {
            return $"{GetVersionLabel(developmentBuild)}  ·  저장 칸 3개  ·  Esc 일시정지  ·  F3 성능 표시";
        }

        /// <summary>프로젝트 폴더 기준 실행 파일 경로다(슬래시 구분).</summary>
        public static string GetOutputPath(
            bool developmentBuild)
        {
            return $"{GetOutputFolder(developmentBuild)}/{ExecutableName}";
        }

        public static string GetOutputFolder(
            bool developmentBuild)
        {
            return $"{BuildRoot}/{(developmentBuild ? DevelopmentFolder : ReleaseFolder)}";
        }

        /// <summary>
        /// 빌드 씬 목록을 확인한다. 문제가 없으면 빈 목록이다.
        /// <paramref name="scenePaths"/>는 Build Settings에서 켜진 씬 경로 순서 그대로다.
        /// </summary>
        public static List<string> CheckScenes(
            IList<string> scenePaths)
        {
            List<string> problems = new List<string>();

            if (scenePaths == null ||
                scenePaths.Count == 0)
            {
                problems.Add("빌드에 들어갈 씬이 없습니다.");
                return problems;
            }

            List<string> names = new List<string>();

            foreach (string path in scenePaths)
            {
                names.Add(GetSceneName(path));
            }

            if (names[0] != SceneNames.Boot)
            {
                problems.Add($"첫 씬이 {SceneNames.Boot}가 아닙니다 (지금 {names[0]}).");
            }

            foreach (string required in RequiredScenes)
            {
                if (!names.Contains(required))
                {
                    problems.Add($"{required} 씬이 빠졌습니다.");
                }
            }

            return problems;
        }

        public static string GetSceneName(
            string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string name = path.Replace('\\', '/');
            int slash = name.LastIndexOf('/');

            if (slash >= 0)
            {
                name = name.Substring(slash + 1);
            }

            return name.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - ".unity".Length)
                : name;
        }

        public static string FormatSize(
            long bytes)
        {
            if (bytes < 0)
            {
                bytes = 0;
            }

            if (bytes < 1024)
            {
                return $"{bytes} B";
            }

            if (bytes < 1024L * 1024L)
            {
                return (bytes / 1024d).ToString("0.0", CultureInfo.InvariantCulture) + " KB";
            }

            if (bytes < 1024L * 1024L * 1024L)
            {
                return (bytes / (1024d * 1024d)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
            }

            return (bytes / (1024d * 1024d * 1024d)).ToString("0.00", CultureInfo.InvariantCulture) + " GB";
        }

        /// <summary>빌드가 끝나면 남기는 보고서 내용이다.</summary>
        public static string FormatReport(
            BuildReportInfo info)
        {
            StringBuilder text = new StringBuilder();

            text.AppendLine($"{ProductName} {GetVersionLabel(info.Development)}");
            text.AppendLine($"빌드 시각   {info.FinishedAt}");
            text.AppendLine($"결과        {(info.Succeeded ? "성공" : "실패")}");
            text.AppendLine($"실행 파일   {GetOutputPath(info.Development)}");
            text.AppendLine($"걸린 시간   {info.Seconds.ToString("0.0", CultureInfo.InvariantCulture)}초");
            text.AppendLine($"크기        {FormatSize(info.TotalBytes)}");
            text.AppendLine($"경고 · 오류 {info.Warnings} · {info.Errors}");
            text.AppendLine($"씬          {string.Join(" → ", info.Scenes ?? new string[0])}");

            if (info.Problems != null &&
                info.Problems.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("확인할 것");

                foreach (string problem in info.Problems)
                {
                    text.AppendLine($"- {problem}");
                }
            }

            return text.ToString();
        }
    }

    /// <summary>빌드 보고서에 적을 값이다 (38일차).</summary>
    public sealed class BuildReportInfo
    {
        public bool Development;
        public bool Succeeded;
        public string FinishedAt = string.Empty;
        public double Seconds;
        public long TotalBytes;
        public int Warnings;
        public int Errors;
        public string[] Scenes;
        public List<string> Problems = new List<string>();
    }
}
