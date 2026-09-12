using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Save;
using ProjectTheta.Stage;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 스테이지 종료 결과 화면이다.
    ///
    /// 회수 정기부터 남은 시간까지의 항목이 하나씩 "딱" 하고 튀어나오고,
    /// 총점이 나온 뒤 위쪽에 랭크가 도장처럼 찍힌다.
    /// 마우스를 클릭하면 연출을 건너뛰고 전부 즉시 표시한다.
    /// </summary>
    public sealed class StageResultPanel : MonoBehaviour
    {
        private const int RowCount = 7;

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

        private StageScoreBreakdown _breakdown;
        private StageRank _rank;
        private bool _snapshotTaken;
        private bool _skipped;
        private float _elapsed;
        private int _playedRowTicks;
        private bool _playedStamp;

        private AudioSource _audioSource;
        private AudioClip _tickClip;
        private AudioClip _stampClip;

        private GUIStyle _rowLabelStyle;
        private GUIStyle _rowValueStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _totalStyle;
        private GUIStyle _totalValueStyle;
        private GUIStyle _rankStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _buttonStyle;

        public void Configure(
            StageSessionController stage,
            StageScoreTracker tracker)
        {
            _stage = stage;
            _tracker = tracker;
        }

        private void Awake()
        {
            _audioSource =
                gameObject.AddComponent<
                    AudioSource>();

            _audioSource.playOnAwake =
                false;

            _audioSource.spatialBlend =
                0f;

            _tickClip =
                RuntimeUiSfx.CreateTick();

            _stampClip =
                RuntimeUiSfx.CreateStamp();
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

            if (_skipped)
            {
                return;
            }

            _elapsed +=
                Time.unscaledDeltaTime;

            PlayPendingSounds();

            if (ReadSkipPressed())
            {
                Skip();
            }
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

                PlayClip(
                    _stampClip);
            }

            _playedRowTicks =
                RowCount;
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

            _breakdown =
                _tracker.BuildBreakdown();

            _rank =
                StageRankLogic.Resolve(
                    _breakdown.Total,
                    _tracker.IsSConditionMet);

            SubmitResultToSession();
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

            GameSession.Instance.SubmitStageResult(
                new StageResultSummary
                {
                    Cleared = cleared,
                    RecoveredEssence =
                        _tracker.RecoveredEssence,
                    TotalScore =
                        _breakdown.Total,
                    RankLabel =
                        cleared
                            ? StageRankLogic.GetLabel(
                                _rank)
                            : "-"
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

                PlayClip(
                    _tickClip);
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

                PlayClip(
                    _stampClip);
            }
        }

        private void PlayClip(
            AudioClip clip)
        {
            if (_audioSource == null ||
                clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(
                clip);
        }

        private void OnGUI()
        {
            if (_stage == null ||
                _stage.IsRunning ||
                !_snapshotTaken)
            {
                return;
            }

            EnsureStyles();

            float width =
                Mathf.Min(
                    560f,
                    Screen.width *
                    0.72f);

            const float height = 396f;

            float x =
                (Screen.width -
                 width) *
                0.5f;

            float y =
                (Screen.height -
                 height) *
                0.5f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height),
                string.Empty);

            DrawTitle(
                x,
                y,
                width);

            DrawRows(
                x,
                y,
                width);

            DrawRank(
                x,
                y,
                width);

            DrawHint(
                x,
                y,
                width,
                height);
        }

        private void DrawTitle(
            float x,
            float y,
            float width)
        {
            GUI.Label(
                new Rect(
                    x,
                    y + 14f,
                    width,
                    26f),
                _stage.GetStateLabel(),
                _titleStyle);
        }

        private void DrawRows(
            float x,
            float y,
            float width)
        {
            const float firstRowY = 104f;
            const float rowHeight = 30f;

            for (int i = 0;
                 i < RowCount;
                 i++)
            {
                float scale =
                    StageResultRevealLogic.GetRowScale(
                        i,
                        _elapsed,
                        _rowInterval,
                        _popDuration);

                if (scale <= 0f)
                {
                    continue;
                }

                Rect rowRect =
                    new Rect(
                        x + 32f,
                        y +
                        firstRowY +
                        (rowHeight * i),
                        width - 64f,
                        rowHeight);

                Matrix4x4 previousMatrix =
                    GUI.matrix;

                GUIUtility.ScaleAroundPivot(
                    new Vector2(
                        scale,
                        scale),
                    new Vector2(
                        rowRect.center.x,
                        rowRect.center.y));

                bool isTotalRow =
                    i == RowCount - 1;

                GUIStyle labelStyle =
                    isTotalRow
                        ? _totalStyle
                        : _rowLabelStyle;

                GUIStyle valueStyle =
                    isTotalRow
                        ? _totalValueStyle
                        : _rowValueStyle;

                GUI.Label(
                    rowRect,
                    GetRowLabel(i),
                    labelStyle);

                GUI.Label(
                    rowRect,
                    GetRowValue(i),
                    valueStyle);

                GUI.matrix =
                    previousMatrix;
            }
        }

        private void DrawRank(
            float x,
            float y,
            float width)
        {
            if (_stage.State !=
                StageState.Cleared)
            {
                return;
            }

            float progress =
                StageResultRevealLogic.GetRankProgress(
                    _elapsed,
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            if (progress <= 0f)
            {
                return;
            }

            float scale =
                StageResultRevealLogic.GetRankScale(
                    progress);

            float alpha =
                StageResultRevealLogic.GetRankAlpha(
                    progress);

            Rect rankRect =
                new Rect(
                    x,
                    y + 40f,
                    width,
                    58f);

            Matrix4x4 previousMatrix =
                GUI.matrix;

            Color previousColor =
                GUI.color;

            GUIUtility.ScaleAroundPivot(
                new Vector2(
                    scale,
                    scale),
                new Vector2(
                    rankRect.center.x,
                    rankRect.center.y));

            GUI.color =
                GetRankColor(
                    _rank,
                    alpha);

            GUI.Label(
                rankRect,
                "RANK  " +
                StageRankLogic.GetLabel(
                    _rank),
                _rankStyle);

            GUI.color =
                previousColor;

            GUI.matrix =
                previousMatrix;
        }

        private void DrawHint(
            float x,
            float y,
            float width,
            float height)
        {
            bool complete =
                StageResultRevealLogic.IsComplete(
                    _elapsed,
                    RowCount,
                    _rowInterval,
                    _rankDelay,
                    _rankDuration);

            if (!complete)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y + height - 30f,
                        width,
                        22f),
                    "클릭하면 건너뜁니다",
                    _hintStyle);

                return;
            }

            // 연출이 끝난 뒤에만 허브로 돌아갈 수 있다.
            if (GUI.Button(
                    new Rect(
                        x + (width * 0.3f),
                        y + height - 46f,
                        width * 0.4f,
                        36f),
                    "허브로",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.Hub);
            }
        }

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

                default:
                    return "총점";
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

                default:
                    return $"{_breakdown.Total:N0}";
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

        private void EnsureStyles()
        {
            if (_rowLabelStyle != null)
            {
                return;
            }

            _titleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 18,
                    fontStyle =
                        FontStyle.Bold
                };

            _rankStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 40,
                    fontStyle =
                        FontStyle.Bold
                };

            _rowLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 16,
                    fontStyle =
                        FontStyle.Bold
                };

            _rowValueStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleRight,
                    fontSize = 16,
                    fontStyle =
                        FontStyle.Bold
                };

            _totalStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 19,
                    fontStyle =
                        FontStyle.Bold
                };

            _totalValueStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleRight,
                    fontSize = 19,
                    fontStyle =
                        FontStyle.Bold
                };

            _hintStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 13
                };

            _buttonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle =
                        FontStyle.Bold
                };
        }

        private void OnDestroy()
        {
            if (_tickClip != null)
            {
                Destroy(
                    _tickClip);
            }

            if (_stampClip != null)
            {
                Destroy(
                    _stampClip);
            }
        }
    }
}
