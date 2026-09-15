using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Capture;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Player;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>디버그 패널이 읽고 조작하는 대상 묶음이다.</summary>
    public sealed class DebugPanelContext
    {
        public HypnosisCaster Caster;
        public StageSessionController Stage;
        public PlayerHealth Health;
        public PlayerFocus Focus;
        public FollowerManager Followers;
        public RunProgression Run;
        public FloorTransitionController Floors;
        public RampageCoordinator Rampage;
        public PlayerCaptureController Capture;
        public RunStatsRecorder Recorder;
    }

    /// <summary>
    /// 개발용 디버그 패널이다 (20일차). F1로 열고 닫는다.
    ///
    /// 15일차 이전에 만든 IMGUI 디버그 창(PrototypeHud)을 대신한다.
    /// 예전 창은 보기만 했지만, 이 패널은 네 장의 탭으로 보고 · 바꾸고 · 만들고 · 기록한다.
    ///
    ///   상태  지금 게임 상태
    ///   수치  밸런스 수치를 슬라이더로 조정 (자산 원본은 그대로)
    ///   치트  층 이동 · 무적 · 레벨업 · 폭주 유도 · 게임 속도
    ///   기록  층별 시간 · 횟수 · 집중력 0 비율
    ///
    /// 열려 있어도 게임은 계속 돈다. 패널 위에서 누른 마우스는 최면·파동으로 새지 않는다(<see cref="PointerGuard"/>).
    /// 에디터와 개발 빌드에서만 만들어진다.
    /// </summary>
    public sealed class DebugPanel : MonoBehaviour
    {
        private const float PanelWidth = 480f;
        private const float Margin = 20f;
        private const float Padding = 20f;
        private const float RefreshInterval = 0.1f;

        private DebugPanelContext _context;
        private IDebugTab[] _tabs;
        private RectTransform[] _tabRoots;
        private UiButton[] _tabButtons;
        private Image[] _tabUnderlines;
        private int _activeTab;

        private Canvas _canvas;
        private RectTransform _panel;
        private Text _fpsText;

        private bool _built;
        private float _refreshRemaining;
        private float _smoothedDelta = 1f / 60f;

        public bool IsVisible =>
            _canvas != null &&
            _canvas.gameObject.activeSelf;

        public void Configure(
            DebugPanelContext context)
        {
            _context = context;

            _tabs =
                new IDebugTab[]
                {
                    new DebugStatusTab(context),
                    new DebugTuningTab(),
                    new DebugCheatTab(context),
                    new DebugRecordTab(context)
                };
        }

        private void OnDisable()
        {
            PointerGuard.IsOverOverlay = false;
        }

        /// <summary>스테이지를 떠날 때 느리게·빠르게 한 게임 속도가 허브로 새지 않게 되돌린다.</summary>
        private void OnDestroy()
        {
            GameplayPause.SetDebugSpeed(
                1f);
        }

        private void Update()
        {
            if (ReadTogglePressed())
            {
                SetVisible(!IsVisible);
            }

            if (!IsVisible)
            {
                return;
            }

            _smoothedDelta =
                Mathf.Lerp(
                    _smoothedDelta,
                    Time.unscaledDeltaTime,
                    0.1f);

            PointerGuard.IsOverOverlay =
                IsPointerOverPanel();

            _refreshRemaining -=
                Time.unscaledDeltaTime;

            if (_refreshRemaining > 0f)
            {
                return;
            }

            _refreshRemaining = RefreshInterval;

            RefreshActive();
        }

        public void SetVisible(
            bool visible)
        {
            if (visible &&
                !_built)
            {
                Build();
            }

            if (_canvas == null)
            {
                return;
            }

            _canvas.gameObject.SetActive(
                visible);

            if (!visible)
            {
                PointerGuard.IsOverOverlay = false;

                return;
            }

            // 열자마자 최신 값이 보이게 바로 한 번 갱신한다.
            _refreshRemaining = 0f;
        }

        private void RefreshActive()
        {
            if (_tabs == null)
            {
                return;
            }

            _tabs[_activeTab].Refresh();

            float fps =
                _smoothedDelta <= 0f
                    ? 0f
                    : 1f / _smoothedDelta;

            _fpsText.text =
                $"{fps:0} fps   ×{Time.timeScale:0.##}";

            _fpsText.color =
                fps < 50f
                    ? UiTheme.Danger
                    : UiTheme.TextMuted;
        }

        private bool IsPointerOverPanel()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null ||
                _panel == null)
            {
                return false;
            }

            // 화면 위 오버레이 캔버스라 카메라는 null이다.
            return RectTransformUtility.RectangleContainsScreenPoint(
                _panel,
                Mouse.current.position.ReadValue(),
                null);
#else
            return _panel != null &&
                   RectTransformUtility.RectangleContainsScreenPoint(
                       _panel,
                       Input.mousePosition,
                       null);
#endif
        }

        private static bool ReadTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null &&
                   Keyboard.current.f1Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F1);
#endif
        }

        // 조립 ----------------------------------------------------------

        /// <summary>
        /// 처음 열 때 한 번 만든다. 스테이지 시작 시간에 조각 수백 개를 미리 만들지 않기 위해서다.
        /// </summary>
        private void Build()
        {
            _built = true;

            // 결과 화면(200)과 카드 화면보다 위에 뜬다.
            _canvas =
                UiFactory.CreateCanvas(
                    "DebugPanelCanvas",
                    500,
                    transform);

            _panel =
                UiFactory.CreatePanel(
                    _canvas.transform,
                    "Panel",
                    DebugUi.Background,
                    UiTheme.PanelEdge);

            _panel.GetComponent<Image>().raycastTarget = true;

            _panel.anchorMin = new Vector2(1f, 0f);
            _panel.anchorMax = new Vector2(1f, 1f);
            _panel.pivot = new Vector2(1f, 0.5f);
            _panel.offsetMin = new Vector2(-(PanelWidth + Margin), Margin);
            _panel.offsetMax = new Vector2(-Margin, -Margin);

            BuildHeader();
            BuildTabs();

            SelectTab(0);
        }

        private void BuildHeader()
        {
            // 윗변 금색 띠
            Image accent =
                UiFactory.CreateImage(
                    _panel,
                    "Accent",
                    UiTheme.Gold);

            UiFactory.PlaceRow(
                accent.rectTransform,
                2f,
                3f,
                2f);

            Text title =
                UiFactory.CreateText(
                    _panel,
                    "Title",
                    "◆ DEBUG",
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            DebugUi.PlaceTopLeft(
                title.rectTransform,
                Padding,
                10f,
                140f,
                36f);

            _fpsText =
                UiFactory.CreateText(
                    _panel,
                    "Fps",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleRight);

            DebugUi.PlaceTopLeft(
                _fpsText.rectTransform,
                160f,
                10f,
                230f,
                36f);

            Text hint =
                UiFactory.CreateText(
                    _panel,
                    "Hint",
                    "F1",
                    UiTheme.FontTiny,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleCenter);

            DebugUi.PlaceTopLeft(
                hint.rectTransform,
                PanelWidth - 94f,
                10f,
                30f,
                36f);

            DebugUi.Button(
                _panel,
                "×",
                PanelWidth - Padding - 36f,
                10f,
                36f,
                36f,
                () => SetVisible(false));

            UiDecor.CreateDivider(
                _panel,
                "Divider",
                new Color(
                    UiTheme.Gold.r,
                    UiTheme.Gold.g,
                    UiTheme.Gold.b,
                    0.6f),
                52f,
                Padding);
        }

        private void BuildTabs()
        {
            const float tabTop = 62f;
            const float tabHeight = 38f;
            const float gap = 6f;
            float contentWidth = PanelWidth - Padding * 2f;

            float tabWidth =
                (contentWidth - gap * (_tabs.Length - 1)) / _tabs.Length;

            _tabButtons = new UiButton[_tabs.Length];
            _tabUnderlines = new Image[_tabs.Length];
            _tabRoots = new RectTransform[_tabs.Length];

            for (int i = 0;
                 i < _tabs.Length;
                 i++)
            {
                int index = i;
                float x = Padding + i * (tabWidth + gap);

                _tabButtons[i] =
                    DebugUi.Button(
                        _panel,
                        _tabs[i].Title,
                        x,
                        tabTop,
                        tabWidth,
                        tabHeight,
                        () => SelectTab(index));

                Image underline =
                    UiFactory.CreateImage(
                        _panel,
                        "Underline",
                        UiTheme.Gold);

                DebugUi.PlaceTopLeft(
                    underline.rectTransform,
                    x,
                    tabTop + tabHeight,
                    tabWidth,
                    3f);

                _tabUnderlines[i] = underline;

                RectTransform root =
                    UiFactory.CreateRect(
                        _panel,
                        $"Tab_{_tabs[i].Title}");

                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = new Vector2(Padding, Padding);
                root.offsetMax = new Vector2(-Padding, -(tabTop + tabHeight + 16f));

                _tabs[i].Build(
                    root);

                _tabRoots[i] = root;
            }
        }

        private void SelectTab(
            int index)
        {
            _activeTab =
                Mathf.Clamp(
                    index,
                    0,
                    _tabs.Length - 1);

            for (int i = 0;
                 i < _tabs.Length;
                 i++)
            {
                bool active = i == _activeTab;

                _tabRoots[i].gameObject.SetActive(active);
                _tabUnderlines[i].enabled = active;

                _tabButtons[i].Background.color =
                    active
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal;

                _tabButtons[i].Label.color =
                    active
                        ? UiTheme.Gold
                        : UiTheme.TextMuted;
            }

            _refreshRemaining = 0f;
        }
    }
}
