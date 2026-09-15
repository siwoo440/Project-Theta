using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 스테이지 종료 결과 화면이다.
    ///
    /// 회수 정기부터 남은 시간까지의 항목이 하나씩 "딱" 하고 튀어나오고,
    /// 총점이 나온 뒤 위쪽에 랭크가 도장처럼 찍힌다.
    /// 마우스를 클릭하면 연출을 건너뛰고 전부 즉시 표시한다.
    ///
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
    /// 타이밍 계산은 그대로 <see cref="StageResultRevealLogic"/>이 맡고,
    /// 여기서는 그 값을 RectTransform 크기와 색에 적용하기만 한다.
    /// </summary>
    public sealed class StageResultPanel : MonoBehaviour
    {
        private const int RowCount = 9;

        /// <summary>총점부터는 글자를 키워 구분한다.</summary>
        private const int EmphasisRowStart = RowCount - 2;

        [SerializeField] private float _rowInterval =
            StageResultRevealLogic.DefaultRowInterval;

        [SerializeField] private float _popDuration =
            StageResultRevealLogic.DefaultPopDuration;

        [SerializeField] private float _rankDelay =
            StageResultRevealLogic.DefaultRankDelay;

        [SerializeField] private float _rankDuration =
            StageResultRevealLogic.DefaultRankDuration;

        private StageSessionController _stage;
        private StageScoreTracker _tracker;
        private FloorTransitionController _floors;

        private StageScoreBreakdown _breakdown;
        private StageRank _rank;
        private int _contractEssence;
        private bool _snapshotTaken;
        private bool _skipped;
        private float _elapsed;
        private int _playedRowTicks;
        private bool _playedStamp;

        private Canvas _canvas;
        private Text _titleText;
        private Text _rankText;
        private Text _hintText;
        private UiButton _hubButton;

        private readonly RectTransform[] _rowRects =
            new RectTransform[RowCount];

        private readonly Text[] _rowLabels =
            new Text[RowCount];

        private readonly Text[] _rowValues =
            new Text[RowCount];

        public void Configure(
            StageSessionController stage,
            StageScoreTracker tracker)
        {
            _stage = stage;
            _tracker = tracker;
        }

        private void Update()
        {
            if (_stage == null ||
                _stage.IsRunning)
            {
                return;
            }

            if (!_snapshotTaken)
            {
                TakeSnapshot();
            }

            if (!_skipped)
            {
                _elapsed +=
                    Time.unscaledDeltaTime;

                PlayPendingSounds();

                if (ReadSkipPressed())
                {
                    Skip();
                }
            }

            ApplyReveal();
        }

        /// <summary>
        /// 프로젝트가 Input System 패키지를 사용하므로 구 Input 클래스를 쓰지 않는다.
        /// 좌·우 클릭 중 아무거나 누르면 연출을 건너뛴다.
        /// </summary>
        private static bool ReadSkipPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null)
            {
                return false;
            }

            return Mouse.current.leftButton.
                       wasPressedThisFrame ||
                   Mouse.current.rightButton.
                       wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(
                       0) ||
                   Input.GetMouseButtonDown(
                       1);
#endif
        }

        /// <summary>연출을 건너뛰고 모든 항목을 즉시 최종 상태로 만든다.</summary>
        public void Skip()
        {
            if (_skipped)
            {
                return;
            }

            _skipped = true;

            _elapsed =
                StageResultRevealLogic.GetTotalDuration(
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            if (!_playedStamp)
            {
                _playedStamp = true;

                GameAudio.Play(
                    GameSfx.UiStamp);
            }

            _playedRowTicks =
                RowCount;

            ApplyReveal();
        }

        private void TakeSnapshot()
        {
            _snapshotTaken = true;

            _elapsed = 0f;
            _playedRowTicks = 0;
            _playedStamp = false;
            _skipped = false;

            if (_tracker == null)
            {
                return;
            }

            _floors =
                FindFirstObjectByType<
                    FloorTransitionController>();

            _breakdown =
                _tracker.BuildBreakdown();

            _rank =
                StageRankLogic.Resolve(
                    _breakdown.Total,
                    _tracker.IsSConditionMet);

            SubmitResultToSession();

            Build();
            FillTexts();
        }

        /// <summary>이번 판의 결과를 허브로 넘긴다. 세이브 반영은 허브가 한다.</summary>
        private void SubmitResultToSession()
        {
            if (GameSession.Instance == null)
            {
                return;
            }

            bool cleared =
                _stage.State ==
                StageState.Cleared;

            string rankLabel =
                cleared
                    ? StageRankLogic.GetLabel(
                        _rank)
                    : "-";

            _contractEssence =
                ContractEssenceLogic.Compute(
                    _tracker.RecoveredEssence,
                    _stage.TargetEssence,
                    cleared,
                    rankLabel);

            GameSession.Instance.SubmitStageResult(
                new StageResultSummary
                {
                    Cleared = cleared,
                    RecoveredEssence =
                        _tracker.RecoveredEssence,
                    TotalScore =
                        _breakdown.Total,
                    RankLabel =
                        rankLabel,
                    ContractEssence =
                        _contractEssence,
                    TargetEssence =
                        _stage.TargetEssence
                });
        }

        private void PlayPendingSounds()
        {
            int visibleRows =
                StageResultRevealLogic.GetVisibleRowCount(
                    _elapsed,
                    RowCount,
                    _rowInterval);

            while (_playedRowTicks < visibleRows)
            {
                _playedRowTicks++;

                GameAudio.Play(
                    GameSfx.UiTick);
            }

            if (_playedStamp)
            {
                return;
            }

            float rankProgress =
                StageResultRevealLogic.GetRankProgress(
                    _elapsed,
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            if (rankProgress > 0f)
            {
                _playedStamp = true;

                GameAudio.Play(
                    GameSfx.UiStamp);
            }
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            if (_canvas != null)
            {
                return;
            }

            // 결과창은 HUD 위에 떠야 하므로 정렬 순서를 높게 준다.
            _canvas =
                UiFactory.CreateCanvas(
                    "StageResultCanvas",
                    200,
                    transform);

            Image dim =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Dim",
                    new Color(0f, 0f, 0f, 0.74f));

            UiFactory.Stretch(
                dim.rectTransform);

            // 클릭으로 건너뛰기를 하므로 뒤쪽 클릭은 막아 둔다.
            dim.raycastTarget = true;

            RectTransform panel =
                UiFactory.CreatePanel(
                    _canvas.transform,
                    "Panel",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge,
                    3f);

            UiFactory.Place(
                panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -20f),
                new Vector2(760f, 620f));

            _titleText =
                UiFactory.CreateText(
                    panel,
                    "Title",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _titleText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -28f),
                new Vector2(700f, 30f));

            // 랭크 도장은 패널 위쪽 바깥으로 살짝 걸치게 둔다.
            _rankText =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Rank",
                    string.Empty,
                    UiTheme.FontTitle + 14,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _rankText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 318f),
                new Vector2(700f, 90f));

            BuildRows(
                panel);

            _hintText =
                UiFactory.CreateText(
                    panel,
                    "Hint",
                    "클릭하면 건너뜁니다",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _hintText.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 34f),
                new Vector2(600f, 24f));

            _hubButton =
                UiFactory.CreateButton(
                    panel,
                    "HubButton",
                    "허브로",
                    UiTheme.FontSubheading,
                    true);

            UiFactory.Place(
                _hubButton.Background.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 30f),
                new Vector2(280f, 52f));

            _hubButton.Button.onClick.AddListener(
                () =>
                {
                    GameSession.Instance?.GoTo(
                        SceneDestination.Hub);
                });

            _hubButton.Button.gameObject.SetActive(
                false);
        }

        private void BuildRows(
            RectTransform panel)
        {
            const float firstRowTop = 82f;
            const float rowHeight = 46f;

            for (int i = 0;
                 i < RowCount;
                 i++)
            {
                bool emphasis =
                    i >= EmphasisRowStart;

                RectTransform row =
                    UiFactory.CreateRect(
                        panel,
                        $"Row{i}");

                UiFactory.PlaceRow(
                    row,
                    firstRowTop +
                    (rowHeight * i),
                    rowHeight,
                    40f);

                // 총점 · 계약 정기 줄은 배경을 깔아 눈에 띄게 한다.
                if (emphasis)
                {
                    Image highlight =
                        UiFactory.CreateImage(
                            row,
                            "Highlight",
                            new Color(
                                UiTheme.Accent.r * 0.28f,
                                UiTheme.Accent.g * 0.18f,
                                UiTheme.Accent.b * 0.32f,
                                0.85f));

                    UiFactory.Stretch(
                        highlight.rectTransform,
                        2f);
                }
                else
                {
                    // 일반 줄은 아래쪽에 가는 선만 둔다.
                    Image underline =
                        UiFactory.CreateImage(
                            row,
                            "Underline",
                            new Color(
                                UiTheme.TextMuted.r,
                                UiTheme.TextMuted.g,
                                UiTheme.TextMuted.b,
                                0.16f));

                    RectTransform underlineRect =
                        underline.rectTransform;

                    underlineRect.anchorMin = new Vector2(0f, 0f);
                    underlineRect.anchorMax = new Vector2(1f, 0f);
                    underlineRect.pivot = new Vector2(0.5f, 0f);
                    underlineRect.offsetMin = Vector2.zero;
                    underlineRect.offsetMax = Vector2.zero;
                    underlineRect.sizeDelta = new Vector2(0f, 1f);
                }

                int fontSize =
                    emphasis
                        ? UiTheme.FontHeading
                        : UiTheme.FontBody + 2;

                Text label =
                    UiFactory.CreateText(
                        row,
                        "Label",
                        GetRowLabel(i),
                        fontSize,
                        emphasis
                            ? UiTheme.TextPrimary
                            : UiTheme.TextMuted,
                        TextAnchor.MiddleLeft,
                        FontStyle.Bold);

                UiFactory.Stretch(
                    label.rectTransform,
                    14f);

                Text value =
                    UiFactory.CreateText(
                        row,
                        "Value",
                        string.Empty,
                        fontSize,
                        emphasis
                            ? UiTheme.Gold
                            : UiTheme.TextPrimary,
                        TextAnchor.MiddleRight,
                        FontStyle.Bold);

                UiFactory.Stretch(
                    value.rectTransform,
                    14f);

                row.localScale = Vector3.zero;

                _rowRects[i] = row;
                _rowLabels[i] = label;
                _rowValues[i] = value;
            }
        }

        private void FillTexts()
        {
            if (_titleText != null)
            {
                _titleText.text =
                    _stage.GetStateLabel();
            }

            for (int i = 0;
                 i < RowCount;
                 i++)
            {
                if (_rowLabels[i] != null)
                {
                    _rowLabels[i].text =
                        GetRowLabel(i);
                }

                if (_rowValues[i] != null)
                {
                    _rowValues[i].text =
                        GetRowValue(i);
                }
            }

            if (_rankText != null)
            {
                _rankText.text =
                    "RANK  " +
                    StageRankLogic.GetLabel(
                        _rank);
            }
        }

        // 연출 적용 ------------------------------------------------------

        private void ApplyReveal()
        {
            if (_canvas == null)
            {
                return;
            }

            for (int i = 0;
                 i < RowCount;
                 i++)
            {
                RectTransform row =
                    _rowRects[i];

                if (row == null)
                {
                    continue;
                }

                float scale =
                    StageResultRevealLogic.GetRowScale(
                        i,
                        _elapsed,
                        _rowInterval,
                        _popDuration);

                // 스케일 0인 동안은 아예 꺼 두어 그리기 비용도 없앤다.
                bool visible =
                    scale > 0f;

                if (row.gameObject.activeSelf != visible)
                {
                    row.gameObject.SetActive(
                        visible);
                }

                if (visible)
                {
                    row.localScale =
                        new Vector3(
                            scale,
                            scale,
                            1f);
                }
            }

            ApplyRankReveal();

            bool complete =
                StageResultRevealLogic.IsComplete(
                    _elapsed,
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            if (_hintText != null)
            {
                _hintText.gameObject.SetActive(
                    !complete);
            }

            // 연출이 끝난 뒤에만 허브로 돌아갈 수 있다.
            if (_hubButton.Button != null &&
                _hubButton.Button.gameObject.activeSelf != complete)
            {
                _hubButton.Button.gameObject.SetActive(
                    complete);
            }
        }

        private void ApplyRankReveal()
        {
            if (_rankText == null)
            {
                return;
            }

            if (_stage.State !=
                StageState.Cleared)
            {
                _rankText.gameObject.SetActive(
                    false);

                return;
            }

            float progress =
                StageResultRevealLogic.GetRankProgress(
                    _elapsed,
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            bool visible =
                progress > 0f;

            if (_rankText.gameObject.activeSelf != visible)
            {
                _rankText.gameObject.SetActive(
                    visible);
            }

            if (!visible)
            {
                return;
            }

            float scale =
                StageResultRevealLogic.GetRankScale(
                    progress);

            _rankText.rectTransform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f);

            _rankText.color =
                GetRankColor(
                    _rank,
                    StageResultRevealLogic.GetRankAlpha(
                        progress));
        }

        // 내용 ----------------------------------------------------------

        private string GetRowLabel(
            int index)
        {
            switch (index)
            {
                case 0:
                    return "회수 정기";

                case 1:
                    return "최대 동행";

                case 2:
                    return "최고 콤보";

                case 3:
                    return "재탈환";

                case 4:
                    return "힘겨루기 승리";

                case 5:
                    return "남은 시간";

                case 6:
                    return "도달 층";

                case 7:
                    return "총점";

                default:
                    return "계약 정기";
            }
        }

        private string GetRowValue(
            int index)
        {
            if (_tracker == null)
            {
                return "-";
            }

            switch (index)
            {
                case 0:
                    return $"{_stage.CurrentEssence} / {_stage.TargetEssence}";

                case 1:
                    return $"{_tracker.MaximumFollowers}명";

                case 2:
                    return $"×{_tracker.BestComboMultiplier:0.0}";

                case 3:
                    return $"{_tracker.ReclaimCount}회";

                case 4:
                    return $"{_tracker.DuelWinCount}회";

                case 5:
                    return FormatTime(
                        _stage.RemainingTime);

                case 6:
                    return _floors == null ||
                           _floors.Run == null
                        ? "1F"
                        : $"{FloorPlanLogic.GetLabel(_floors.Run.HighestReached)}   ({_floors.Run.VisitedCount}개 층)";

                case 7:
                    return $"{_breakdown.Total:N0}";

                default:
                    return $"+{_contractEssence:N0}";
            }
        }

        private static Color GetRankColor(
            StageRank rank,
            float alpha)
        {
            switch (rank)
            {
                case StageRank.S:
                    return new Color(1.00f, 0.82f, 0.26f, alpha);

                case StageRank.A:
                    return new Color(1.00f, 0.42f, 0.78f, alpha);

                case StageRank.B:
                    return new Color(0.40f, 0.72f, 1.00f, alpha);

                case StageRank.C:
                default:
                    return new Color(0.86f, 0.88f, 0.92f, alpha);
            }
        }

        private static string FormatTime(
            float seconds)
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.FloorToInt(
                        seconds));

            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
    }
}
