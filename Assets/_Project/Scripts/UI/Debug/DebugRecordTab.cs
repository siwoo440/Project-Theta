using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Run;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>
    /// ④ 기록 탭이다. 이번 판의 층별 시간과 횟수를 보여준다 (20일차).
    /// 판이 끝나면 같은 내용이 RunLogs 폴더에 파일로 남는다(<see cref="RunStatsRecorder"/>).
    /// </summary>
    public sealed class DebugRecordTab : IDebugTab
    {
        private const int MaximumFloors = 8;

        private readonly DebugPanelContext _context;

        private readonly StringBuilder _builder =
            new StringBuilder(128);

        private readonly UiBar[] _floorBars = new UiBar[MaximumFloors];
        private readonly Text[] _floorTexts = new Text[MaximumFloors];
        private readonly Text[] _floorLabels = new Text[MaximumFloors];

        private Text _countsLine1;
        private Text _countsLine2;
        private Text _countsLine3;
        private UiBar _focusEmptyBar;
        private Text _focusEmptyText;
        private Text _levelTimesText;
        private Text _cheatText;
        private Text _logPathText;

        public DebugRecordTab(
            DebugPanelContext context)
        {
            _context = context;
        }

        public string Title => "기록";

        public void Build(
            RectTransform root)
        {
            float y = 0f;
            float row = DebugUi.RowHeight;
            const float barX = 44f;
            const float barWidth = 250f;
            const float textX = 304f;

            y = DebugUi.Section(root, "층별 시간 (카드 화면 제외)", y);

            int floorCount =
                Mathf.Min(
                    MaximumFloors,
                    _context.Floors == null
                        ? 1
                        : _context.Floors.FloorCount);

            for (int i = 0;
                 i < floorCount;
                 i++)
            {
                _floorLabels[i] = DebugUi.Label(root, $"{i + 1}F", 0f, y, 40f, UiTheme.TextMuted, UiTheme.FontSmall, TextAnchor.MiddleLeft, true);
                _floorBars[i] = DebugUi.Bar(root, barX, y, barWidth, UiTheme.AccentSoft);
                _floorTexts[i] = DebugUi.Label(root, "-", textX, y, DebugUi.ContentWidth - textX, UiTheme.TextPrimary);
                y += row;
            }

            y += DebugUi.SectionGap;

            y = DebugUi.Section(root, "횟수", y);

            _countsLine1 = DebugUi.Label(root, string.Empty, 8f, y, DebugUi.ContentWidth - 8f, UiTheme.TextPrimary);
            y += row;
            _countsLine2 = DebugUi.Label(root, string.Empty, 8f, y, DebugUi.ContentWidth - 8f, UiTheme.TextPrimary);
            y += row;
            _countsLine3 = DebugUi.Label(root, string.Empty, 8f, y, DebugUi.ContentWidth - 8f, UiTheme.TextPrimary);
            y += row + DebugUi.SectionGap;

            y = DebugUi.Section(root, "집중력 0이었던 시간", y);

            _focusEmptyBar = DebugUi.Bar(root, 8f, y, barWidth + 36f, UiTheme.FocusExhausted);
            _focusEmptyText = DebugUi.Label(root, string.Empty, textX, y, DebugUi.ContentWidth - textX, UiTheme.TextPrimary);
            y += row + DebugUi.SectionGap;

            y = DebugUi.Section(root, "레벨업 시각", y);

            _levelTimesText =
                DebugUi.Label(root, string.Empty, 8f, y, DebugUi.ContentWidth - 8f, UiTheme.TextPrimary);

            // 레벨이 많이 오르면 줄을 바꿔 세 줄까지 쓴다.
            _levelTimesText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _levelTimesText.alignment = TextAnchor.UpperLeft;
            _levelTimesText.rectTransform.sizeDelta = new Vector2(DebugUi.ContentWidth - 8f, row * 3f);

            y += row * 3f + DebugUi.SectionGap;

            _cheatText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextMuted);
            y += row + 4f;

            DebugUi.Button(
                root,
                "기록 폴더 열기",
                0f,
                y,
                200f,
                36f,
                OpenLogFolder);

            y += 42f;

            _logPathText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextDisabled, UiTheme.FontTiny);
            _logPathText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logPathText.rectTransform.sizeDelta = new Vector2(DebugUi.ContentWidth, row * 2f);
        }

        public void Refresh()
        {
            RunStatsRecorder recorder = _context.Recorder;

            if (recorder == null)
            {
                return;
            }

            RunStats stats = recorder.Stats;

            int currentFloor =
                _context.Floors == null
                    ? 0
                    : _context.Floors.CurrentFloor;

            for (int i = 0;
                 i < MaximumFloors;
                 i++)
            {
                if (_floorTexts[i] == null)
                {
                    continue;
                }

                bool visited =
                    i < stats.FloorCount &&
                    stats.FloorSeconds[i] > 0f;

                bool here = i == currentFloor;

                _floorBars[i].SetValue(
                    stats.GetFloorShare(i));

                _floorTexts[i].text =
                    !visited
                        ? "-"
                        : here
                            ? RunStats.FormatTime(stats.FloorSeconds[i]) + "  ← 진행"
                            : RunStats.FormatTime(stats.FloorSeconds[i]);

                _floorLabels[i].color =
                    here
                        ? UiTheme.Gold
                        : UiTheme.TextMuted;
            }

            _countsLine1.text =
                $"최면 성공 {stats.HypnosisCount}     되찾기 {stats.ReclaimCount}     빼앗김 {stats.StolenCount}";

            _countsLine2.text =
                $"회수 {stats.RecoveredFollowers}명 ({stats.RecoveredEssence} 정기)     힘겨루기 승리 {stats.DuelWins}";

            _countsLine3.text =
                $"폭주 준비 {stats.RampageWindups}     회피 {stats.RampageSurvived}     포획 {stats.CaptureCount}";

            _focusEmptyBar.SetValue(stats.FocusEmptyRatio);

            _focusEmptyText.text =
                $"{DebugUi.Percent(stats.FocusEmptyRatio)}   ({RunStats.FormatTime(stats.FocusEmptySeconds)})";

            _builder.Length = 0;

            for (int i = 0;
                 i < stats.LevelUpTimes.Count;
                 i++)
            {
                if (i > 0)
                {
                    _builder.Append("  ·  ");
                }

                _builder.Append("Lv");
                _builder.Append(i + 2);
                _builder.Append(' ');
                _builder.Append(RunStats.FormatTime(stats.LevelUpTimes[i]));
            }

            _levelTimesText.text =
                _builder.Length == 0
                    ? "아직 없음"
                    : _builder.ToString();

            _cheatText.text =
                stats.Cheated
                    ? "이 판은 치트를 썼습니다 — 밸런스 판단에서 빼세요"
                    : $"총 {RunStats.FormatTime(stats.TotalSeconds)}   치트 없음";

            _cheatText.color =
                stats.Cheated
                    ? UiTheme.Gold
                    : UiTheme.TextMuted;

            _logPathText.text =
                string.IsNullOrEmpty(recorder.LastLogPath)
                    ? "판이 끝나면 기록 파일이 저장됩니다"
                    : "저장됨: " + recorder.LastLogPath;
        }

        private static void OpenLogFolder()
        {
            string folder =
                RunStatsRecorder.LogFolder;

            try
            {
                Directory.CreateDirectory(
                    folder);

                Application.OpenURL(
                    "file:///" +
                    folder.Replace('\\', '/'));
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogWarning(
                    $"기록 폴더를 열지 못했습니다: {exception.Message}");
            }
        }
    }
}
