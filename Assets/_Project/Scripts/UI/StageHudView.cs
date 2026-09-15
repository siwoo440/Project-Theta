using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Items;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 인게임 정식 HUD다.
    ///
    /// 14일차까지 플레이어에게 필요한 정보가 IMGUI 디버그 출력과 뒤섞여 있었다.
    /// 15일차에 Canvas로 옮기면서 "플레이에 필요한 것"만 여기로 모으고,
    /// 개발용 수치는 <see cref="PrototypeHud"/>에 남겨 F1로 켜고 끄게 했다.
    ///
    /// 배치 원칙
    ///   좌상단  생존 - 체력 · 집중력
    ///   중상단  목표 - 남은 시간 · 정기 게이지
    ///   좌하단  수단 - 파동 · 소비 아이템
    ///   중하단  자산 - 동행 인원 · 최저 안정도
    ///   우하단  보상 - 콤보
    /// </summary>
    public sealed class StageHudView : MonoBehaviour
    {
        private StageSessionController _stage;
        private PlayerHealth _health;
        private PlayerFocus _focus;
        private HypnosisWaveCaster _wave;
        private PlayerConsumables _consumables;
        private FollowerManager _followers;
        private StageScoreTracker _tracker;
        private FloorTransitionController _floors;
        private RunProgression _run;

        // 레벨 (좌상단, 집중력 아래)
        private Text _levelText;
        private UiBar _xpBar;
        private Text _xpGainText;
        private float _xpGainRemaining;
        private int _xpGainAccumulated;
        private float _levelFlashRemaining;

        private const float XpGainShowSeconds = 1.1f;
        private const float LevelFlashSeconds = 0.9f;

        // 좌상단
        private UiBar _healthBar;
        private Text _healthText;
        private UiBar _focusBar;
        private Text _focusText;

        // 중상단
        private Text _timeText;
        private UiBar _essenceBar;
        private Text _essenceText;
        private Text _noticeText;
        private Text _tutorialText;

        private TutorialStep _tutorialStep =
            TutorialStep.Hypnosis;

        private bool _tutorialLoaded;

        // 좌하단
        private Text _waveText;
        private UiBar _waveBar;
        private readonly Text[] _slotKeyTexts = new Text[2];
        private readonly Text[] _slotNameTexts = new Text[2];
        private readonly Text[] _slotDescTexts = new Text[2];
        private readonly Image[] _slotBackgrounds = new Image[2];

        // 중하단
        private Text _followerText;
        private UiBar _stabilityBar;

        // 우상단
        private Image[] _floorMarks;
        private Text _floorLabel;

        // 계단 안내
        private RectTransform _stairPrompt;
        private Text _stairPromptText;

        // 우하단
        private RectTransform _comboGroup;
        private Text _comboText;

        private float _comboPulse;

        // 마지막으로 그린 값의 키다. 같으면 문자열을 다시 만들지 않는다 (18일차 최적화).
        private long _healthKey = UiChangeKey.Unset;
        private long _focusKey = UiChangeKey.Unset;
        private long _timeKey = UiChangeKey.Unset;
        private long _essenceKey = UiChangeKey.Unset;
        private long _noticeKey = UiChangeKey.Unset;
        private long _waveKey = UiChangeKey.Unset;
        private long _followerKey = UiChangeKey.Unset;
        private long _comboKey = UiChangeKey.Unset;
        private long _floorKey = UiChangeKey.Unset;
        private long _stairKey = UiChangeKey.Unset;
        private long _levelKey = UiChangeKey.Unset;
        private long _xpGainKey = UiChangeKey.Unset;

        public void Configure(
            HypnosisCaster caster,
            StageSessionController stage,
            PlayerHealth health)
        {
            _stage = stage;
            _health = health;

            _tracker =
                FindFirstObjectByType<
                    StageScoreTracker>();

            _floors =
                FindFirstObjectByType<
                    FloorTransitionController>();

            if (caster != null)
            {
                _run =
                    caster.GetComponent<RunProgression>();
            }

            if (caster != null)
            {
                _focus =
                    caster.GetComponent<PlayerFocus>();

                _wave =
                    caster.GetComponent<
                        HypnosisWaveCaster>();

                _consumables =
                    caster.GetComponent<
                        PlayerConsumables>();

                _followers =
                    caster.FollowerManager;
            }
        }

        private void Start()
        {
            Build();

            if (_run != null)
            {
                _run.XpGained += HandleXpGained;
                _run.LevelChanged += HandleLevelChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameVfx.EssenceTarget == _essenceBar.Root)
            {
                GameVfx.EssenceTarget = null;
            }

            if (_run != null)
            {
                _run.XpGained -= HandleXpGained;
                _run.LevelChanged -= HandleLevelChanged;
            }
        }

        /// <summary>짧은 시간에 연달아 얻으면 합쳐서 보여준다. 숫자가 깜빡이며 바뀌면 읽을 수 없다.</summary>
        private void HandleXpGained(
            int amount)
        {
            _xpGainAccumulated =
                _xpGainRemaining > 0f
                    ? _xpGainAccumulated + amount
                    : amount;

            _xpGainRemaining =
                XpGainShowSeconds;
        }

        private void HandleLevelChanged(
            int level)
        {
            _levelFlashRemaining =
                LevelFlashSeconds;
        }

        private void Update()
        {
            RefreshSurvival();
            RefreshObjective();
            RefreshTools();
            RefreshFollowers();
            RefreshCombo();
            RefreshTutorial();
            RefreshFloor();
            RefreshLevel();
        }

        private void RefreshLevel()
        {
            if (_run == null ||
                _levelText == null)
            {
                return;
            }

            RunLevelState level =
                _run.Level;

            if (UiChangeKey.Changed(
                    ref _levelKey,
                    UiChangeKey.Of(
                        level.Level,
                        level.IsMaxLevel
                            ? 1
                            : 0)))
            {
                _levelText.text =
                    level.IsMaxLevel
                        ? "MAX"
                        : $"Lv {level.Level}";
            }

            _xpBar.SetValue(
                level.Progress);

            // 레벨이 오른 직후에는 게이지와 글자를 흰색으로 번쩍여 알린다.
            // 카드 화면이 뜨면 시간이 멈추므로 unscaled로 흘린다.
            if (_levelFlashRemaining > 0f)
            {
                _levelFlashRemaining -=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        _levelFlashRemaining /
                        LevelFlashSeconds);

                Color flash =
                    Color.Lerp(
                        UiTheme.Gold,
                        Color.white,
                        t);

                _levelText.color = flash;
                _xpBar.SetColor(flash);
            }
            else
            {
                _levelText.color = UiTheme.Gold;
                _xpBar.SetColor(UiTheme.Gold);
            }

            if (_xpGainRemaining > 0f)
            {
                _xpGainRemaining -=
                    Time.unscaledDeltaTime;

                Color color =
                    UiTheme.Gold;

                color.a =
                    Mathf.Clamp01(
                        _xpGainRemaining /
                        (XpGainShowSeconds * 0.4f));

                _xpGainText.color = color;

                if (UiChangeKey.Changed(
                        ref _xpGainKey,
                        _xpGainAccumulated))
                {
                    _xpGainText.text =
                        $"+{_xpGainAccumulated}";
                }
            }
            else if (_xpGainKey != UiChangeKey.Unset)
            {
                _xpGainKey = UiChangeKey.Unset;

                _xpGainText.text = string.Empty;
            }
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "StageHudCanvas",
                    50,
                    transform);

            BuildSurvival(
                canvas.transform);

            BuildObjective(
                canvas.transform);

            BuildTools(
                canvas.transform);

            BuildFollowers(
                canvas.transform);

            BuildCombo(
                canvas.transform);

            BuildFloorIndicator(
                canvas.transform);

            BuildStairPrompt(
                canvas.transform);
        }

        /// <summary>
        /// 층 표시다. 아래가 1층이고 위로 쌓인다.
        /// 지금 층은 보라로 채우고, 가 본 층은 흐리게, 안 가 본 층은 테두리만 남긴다.
        /// </summary>
        private void BuildFloorIndicator(
            Transform parent)
        {
            int floorCount =
                _floors == null
                    ? 1
                    : _floors.FloorCount;

            RectTransform group =
                UiFactory.CreateRect(
                    parent,
                    "FloorIndicator");

            UiFactory.Place(
                group,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-44f, -36f),
                new Vector2(180f, 40f + (floorCount * 34f)));

            _floorLabel =
                UiFactory.CreateText(
                    group,
                    "FloorLabel",
                    string.Empty,
                    UiTheme.FontHeading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(
                _floorLabel.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0f),
                new Vector2(170f, 30f));

            _floorMarks =
                new Image[floorCount];

            for (int i = 0;
                 i < floorCount;
                 i++)
            {
                Image mark =
                    UiFactory.CreateImage(
                        group,
                        $"FloorMark_{i}",
                        UiTheme.TrackFill);

                // 1층이 맨 아래에 오도록 역순으로 쌓는다.
                UiFactory.Place(
                    mark.rectTransform,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(
                        0f,
                        -40f - ((floorCount - 1 - i) * 34f)),
                    new Vector2(84f, 26f));

                _floorMarks[i] = mark;
            }
        }

        /// <summary>계단 앞에 섰을 때만 뜨는 안내다.</summary>
        private void BuildStairPrompt(
            Transform parent)
        {
            _stairPrompt =
                UiFactory.CreatePanel(
                    parent,
                    "StairPrompt",
                    UiTheme.PanelFill,
                    UiTheme.Accent);

            UiFactory.Place(
                _stairPrompt,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 132f),
                new Vector2(460f, 62f));

            _stairPromptText =
                UiFactory.CreateText(
                    _stairPrompt,
                    "Text",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                _stairPromptText.rectTransform);

            _stairPrompt.gameObject.SetActive(
                false);
        }

        private void BuildSurvival(
            Transform parent)
        {
            RectTransform group =
                UiFactory.CreateRect(
                    parent,
                    "Survival");

            UiFactory.Place(
                group,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(40f, -36f),
                new Vector2(420f, 150f));

            _healthText =
                UiFactory.CreateText(
                    group,
                    "HealthText",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.LowerLeft);

            UiFactory.Place(
                _healthText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(2f, 0f),
                new Vector2(420f, 22f));

            _healthBar =
                UiFactory.CreateBar(
                    group,
                    "HealthBar",
                    UiTheme.Health,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _healthBar.Root,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, -24f),
                new Vector2(400f, 22f));

            _focusText =
                UiFactory.CreateText(
                    group,
                    "FocusText",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.LowerLeft);

            UiFactory.Place(
                _focusText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(2f, -52f),
                new Vector2(420f, 22f));

            _focusBar =
                UiFactory.CreateBar(
                    group,
                    "FocusBar",
                    UiTheme.Focus,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _focusBar.Root,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, -76f),
                new Vector2(400f, 22f));

            BuildLevel(
                group);
        }

        /// <summary>
        /// 레벨과 경험치 게이지다.
        /// 레벨 글자를 게이지 왼쪽에 크게 두고, 얻은 경험치는 오른쪽에 잠깐 띄운다.
        /// </summary>
        private void BuildLevel(
            Transform group)
        {
            _levelText =
                UiFactory.CreateText(
                    group,
                    "Level",
                    "Lv 1",
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                _levelText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, -106f),
                new Vector2(80f, 28f));

            _xpBar =
                UiFactory.CreateBar(
                    group,
                    "XpBar",
                    UiTheme.Gold,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _xpBar.Root,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(76f, -114f),
                new Vector2(324f, 12f));

            _xpGainText =
                UiFactory.CreateText(
                    group,
                    "XpGain",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                _xpGainText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(410f, -106f),
                new Vector2(120f, 28f));
        }

        private void BuildObjective(
            Transform parent)
        {
            RectTransform group =
                UiFactory.CreateRect(
                    parent,
                    "Objective");

            UiFactory.Place(
                group,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -28f),
                new Vector2(720f, 150f));

            _timeText =
                UiFactory.CreateText(
                    group,
                    "Time",
                    "00:00",
                    UiTheme.FontTitle - 6,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _timeText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -4f),
                new Vector2(400f, 50f));

            _essenceBar =
                UiFactory.CreateBar(
                    group,
                    "EssenceBar",
                    UiTheme.Accent,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _essenceBar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -62f),
                new Vector2(640f, 30f));

            // 회수한 정기 숫자가 이 게이지로 날아온다 (19일차).
            GameVfx.EssenceTarget =
                _essenceBar.Root;

            // 수치는 게이지 위에 겹쳐 적어 시선 이동을 줄인다.
            _essenceText =
                UiFactory.CreateText(
                    _essenceBar.Root,
                    "EssenceText",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                _essenceText.rectTransform);

            _noticeText =
                UiFactory.CreateText(
                    group,
                    "Notice",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _noticeText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -100f),
                new Vector2(760f, 26f));

            _tutorialText =
                UiFactory.CreateText(
                    group,
                    "Tutorial",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Focus,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _tutorialText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -132f),
                new Vector2(900f, 28f));
        }

        private void BuildTools(
            Transform parent)
        {
            RectTransform group =
                UiFactory.CreateRect(
                    parent,
                    "Tools");

            UiFactory.Place(
                group,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(40f, 36f),
                new Vector2(460f, 180f));

            for (int i = 0;
                 i < 2;
                 i++)
            {
                RectTransform slot =
                    UiFactory.CreatePanel(
                        group,
                        $"Slot{i}",
                        UiTheme.PanelFill,
                        UiTheme.PanelEdge);

                UiFactory.Place(
                    slot,
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    new Vector2(0f, 64f * i),
                    new Vector2(440f, 58f));

                _slotBackgrounds[i] =
                    slot.GetComponent<Image>();

                // 키 번호는 어두운 사각형 안에 넣어 눈에 먼저 들어오게 한다.
                Image keyBox =
                    UiFactory.CreateImage(
                        slot,
                        "KeyBox",
                        UiTheme.TrackFill);

                UiFactory.Place(
                    keyBox.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(10f, 0f),
                    new Vector2(40f, 40f));

                _slotKeyTexts[i] =
                    UiFactory.CreateText(
                        keyBox.transform,
                        "Key",
                        (i + 1).ToString(),
                        UiTheme.FontSubheading,
                        UiTheme.Accent,
                        TextAnchor.MiddleCenter,
                        FontStyle.Bold);

                UiFactory.Stretch(
                    _slotKeyTexts[i].rectTransform);

                _slotNameTexts[i] =
                    UiFactory.CreateText(
                        slot,
                        "Name",
                        string.Empty,
                        UiTheme.FontBody,
                        UiTheme.TextPrimary,
                        TextAnchor.MiddleLeft,
                        FontStyle.Bold);

                UiFactory.Place(
                    _slotNameTexts[i].rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(60f, 10f),
                    new Vector2(370f, 22f));

                _slotDescTexts[i] =
                    UiFactory.CreateText(
                        slot,
                        "Desc",
                        string.Empty,
                        UiTheme.FontTiny,
                        UiTheme.TextMuted,
                        TextAnchor.MiddleLeft);

                UiFactory.Place(
                    _slotDescTexts[i].rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(60f, -10f),
                    new Vector2(370f, 20f));
            }

            _waveText =
                UiFactory.CreateText(
                    group,
                    "WaveText",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.LowerLeft);

            UiFactory.Place(
                _waveText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(2f, 140f),
                new Vector2(460f, 22f));

            _waveBar =
                UiFactory.CreateBar(
                    group,
                    "WaveBar",
                    UiTheme.AccentSoft,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _waveBar.Root,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 132f),
                new Vector2(440f, 8f));
        }

        private void BuildFollowers(
            Transform parent)
        {
            RectTransform group =
                UiFactory.CreateRect(
                    parent,
                    "Followers");

            UiFactory.Place(
                group,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 36f),
                new Vector2(420f, 60f));

            _followerText =
                UiFactory.CreateText(
                    group,
                    "FollowerText",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _followerText.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(420f, 26f));

            // 가장 불안한 동행자의 안정도. 이게 바닥나면 폭주한다.
            _stabilityBar =
                UiFactory.CreateBar(
                    group,
                    "StabilityBar",
                    UiTheme.Positive,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _stabilityBar.Root,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 4f),
                new Vector2(300f, 12f));
        }

        private void BuildCombo(
            Transform parent)
        {
            _comboGroup =
                UiFactory.CreatePanel(
                    parent,
                    "Combo",
                    UiTheme.PanelFill,
                    UiTheme.Gold);

            UiFactory.Place(
                _comboGroup,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-40f, 36f),
                new Vector2(230f, 74f));

            _comboText =
                UiFactory.CreateText(
                    _comboGroup,
                    "ComboText",
                    string.Empty,
                    UiTheme.FontHeading + 4,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                _comboText.rectTransform);

            _comboGroup.gameObject.SetActive(
                false);
        }

        // 갱신 ----------------------------------------------------------

        private void RefreshSurvival()
        {
            if (_health != null)
            {
                _healthBar.SetValue(
                    _health.HealthNormalized);

                if (UiChangeKey.Changed(
                        ref _healthKey,
                        UiChangeKey.Of(
                            _health.CurrentHealth,
                            _health.MaximumHealth)))
                {
                    _healthText.text =
                        $"체력  {_health.CurrentHealth} / {_health.MaximumHealth}";
                }
            }

            if (_focus == null)
            {
                return;
            }

            _focusBar.SetValue(
                _focus.FocusNormalized);

            _focusBar.SetColor(
                _focus.IsExhausted
                    ? UiTheme.FocusExhausted
                    : UiTheme.Focus);

            if (UiChangeKey.Changed(
                    ref _focusKey,
                    UiChangeKey.Of(
                        UiChangeKey.Whole(
                            _focus.CurrentFocus),
                        UiChangeKey.Whole(
                            _focus.MaximumFocus),
                        _focus.IsExhausted
                            ? 1
                            : 0)))
            {
                // 19일차: 집중력은 최면 가속이다. 바닥나도 최면은 계속된다는 것을 글자로 알린다.
                _focusText.text =
                    _focus.IsExhausted
                        ? $"집중력  0 / {_focus.MaximumFocus:0}   기본 속도"
                        : $"집중력  {_focus.CurrentFocus:0} / {_focus.MaximumFocus:0}   최면 가속 ×{_focus.HypnosisSpeedMultiplier:0.0}";
            }

            // 바닥나도 막히는 게 아니므로 경고색(빨강)을 쓰지 않는다.
            _focusText.color =
                _focus.IsExhausted
                    ? UiTheme.TextDisabled
                    : UiTheme.Focus;
        }

        private void RefreshObjective()
        {
            if (_stage == null)
            {
                return;
            }

            // 시간은 초 단위로만 보이므로 1초에 한 번만 문자열을 만든다.
            if (UiChangeKey.Changed(
                    ref _timeKey,
                    Mathf.Max(
                        0,
                        Mathf.FloorToInt(
                            _stage.RemainingTime))))
            {
                _timeText.text =
                    FormatTime(
                        _stage.RemainingTime);
            }

            // 30초를 끊으면 시간 표시를 붉게 바꿔 마감을 알린다.
            _timeText.color =
                _stage.RemainingTime <= 30f
                    ? UiTheme.Danger
                    : UiTheme.TextPrimary;

            _essenceBar.SetValue(
                _stage.EssenceNormalized);

            if (UiChangeKey.Changed(
                    ref _essenceKey,
                    UiChangeKey.Of(
                        _stage.CurrentEssence,
                        _stage.TargetEssence)))
            {
                _essenceText.text =
                    $"정기  {_stage.CurrentEssence} / {_stage.TargetEssence}";
            }

            if (_followers != null &&
                _followers.IsContestWarded)
            {
                if (UiChangeKey.Changed(
                        ref _noticeKey,
                        UiChangeKey.Of(
                            UiChangeKey.Tenths(
                                _followers.ContestWardRemaining),
                            0,
                            1)))
                {
                    _noticeText.text =
                        $"차단 부적 활성  {_followers.ContestWardRemaining:0.0}초";
                }

                _noticeText.color = UiTheme.Focus;

                return;
            }

            if (_stage.HasPendingRecovery)
            {
                if (UiChangeKey.Changed(
                        ref _noticeKey,
                        UiChangeKey.Of(
                            _stage.PendingRecoveryCount,
                            UiChangeKey.Tenths(
                                _stage.PendingRecoveryMultiplier),
                            2)))
                {
                    _noticeText.text =
                        $"회수 정산 중  {_stage.PendingRecoveryCount}명  ×{_stage.PendingRecoveryMultiplier:0.0}";
                }

                _noticeText.color = UiTheme.Gold;

                return;
            }

            if (UiChangeKey.Changed(
                    ref _noticeKey,
                    UiChangeKey.Of(
                        0,
                        0,
                        0)))
            {
                _noticeText.text =
                    "회수 지점에 도착해야 정기가 확정됩니다";
            }

            _noticeText.color = UiTheme.TextMuted;
        }

        private void RefreshTools()
        {
            if (_wave != null)
            {
                if (_wave.CooldownRemaining > 0f)
                {
                    if (UiChangeKey.Changed(
                            ref _waveKey,
                            UiChangeKey.Of(
                                UiChangeKey.Tenths(
                                    _wave.CooldownRemaining),
                                0,
                                1)))
                    {
                        _waveText.text =
                            $"파동(우클릭 유지)  재사용 {_wave.CooldownRemaining:0.0}초";
                    }

                    _waveText.color = UiTheme.TextDisabled;

                    _waveBar.SetColor(
                        UiTheme.TextDisabled);

                    _waveBar.SetValue(
                        1f -
                        Mathf.Clamp01(
                            _wave.CooldownRemaining /
                            Mathf.Max(
                                0.01f,
                                HypnosisWaveLogic.CooldownSeconds)));
                }
                else if (_wave.IsCharging)
                {
                    if (UiChangeKey.Changed(
                            ref _waveKey,
                            UiChangeKey.Of(
                                UiChangeKey.Whole(
                                    _wave.ChargeNormalized * 100f),
                                0,
                                2)))
                    {
                        _waveText.text =
                            $"파동 충전  {_wave.ChargeNormalized * 100f:0}%";
                    }

                    _waveText.color = UiTheme.Accent;

                    _waveBar.SetColor(
                        UiTheme.Accent);

                    _waveBar.SetValue(
                        _wave.ChargeNormalized);
                }
                else
                {
                    if (UiChangeKey.Changed(
                            ref _waveKey,
                            UiChangeKey.Of(
                                UiChangeKey.Whole(
                                    _wave.FocusCost),
                                0,
                                3)))
                    {
                        _waveText.text =
                            $"파동(우클릭 유지)  준비됨   집중력 -{_wave.FocusCost:0}";
                    }

                    _waveText.color = UiTheme.TextMuted;

                    _waveBar.SetColor(
                        UiTheme.AccentSoft);

                    _waveBar.SetValue(1f);
                }
            }

            if (_consumables == null)
            {
                return;
            }

            for (int i = 0;
                 i < 2;
                 i++)
            {
                _slotNameTexts[i].text =
                    _consumables.GetSlotLabel(i);

                _slotDescTexts[i].text =
                    _consumables.GetSlotDescription(i);
            }
        }

        private void RefreshFollowers()
        {
            if (_followers == null)
            {
                return;
            }

            int count =
                _followers.Count;

            if (UiChangeKey.Changed(
                    ref _followerKey,
                    count))
            {
                _followerText.text =
                    $"동행  {count}명";
            }

            float stability =
                count <= 0
                    ? 0f
                    : _followers.LowestStabilityNormalized;

            _stabilityBar.SetValue(
                stability);

            // 초록 → 금색 → 빨강으로 옮겨가며 폭주 임박을 알린다.
            _stabilityBar.SetColor(
                stability > 0.5f
                    ? UiTheme.Positive
                    : stability > 0.25f
                        ? UiTheme.Gold
                        : UiTheme.Danger);

            if (_stabilityBar.Root.gameObject.activeSelf != count > 0)
            {
                _stabilityBar.Root.gameObject.SetActive(
                    count > 0);
            }
        }

        private void RefreshCombo()
        {
            bool active =
                _tracker != null &&
                _tracker.CurrentCombo > 0;

            if (_comboGroup.gameObject.activeSelf != active)
            {
                _comboGroup.gameObject.SetActive(
                    active);

                _comboPulse = 0f;
            }

            if (!active)
            {
                return;
            }

            if (UiChangeKey.Changed(
                    ref _comboKey,
                    UiChangeKey.Tenths(
                        _tracker.CurrentComboMultiplier)))
            {
                _comboText.text =
                    $"콤보 ×{_tracker.CurrentComboMultiplier:0.0}";
            }

            // 콤보가 살아 있는 동안 아주 약하게 맥동시켜 시선을 끈다.
            _comboPulse += Time.deltaTime * 4.2f;

            float scale =
                1f +
                Mathf.Sin(
                    _comboPulse) *
                0.025f;

            _comboGroup.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f);
        }

        /// <summary>
        /// 기획서 A.11절: 별도 튜토리얼 스테이지 없이 기능을 순서대로 연다.
        ///
        /// 18일차에 폭주 생존 신호가 생겨 다섯 단계를 끝까지 닫는다.
        /// 끝까지 마치면 세이브에 남기고, 다음 판부터는 안내를 띄우지 않는다.
        /// </summary>
        private void RefreshTutorial()
        {
            if (_tutorialText == null)
            {
                return;
            }

            if (!_tutorialLoaded)
            {
                _tutorialLoaded = true;

                if (GameSession.Instance != null &&
                    GameSession.Instance.Save != null &&
                    GameSession.Instance.Save.TutorialCompleted)
                {
                    _tutorialStep =
                        TutorialStep.Completed;
                }
            }

            if (TutorialFlowLogic.IsCompleted(
                    _tutorialStep))
            {
                if (_tutorialText.gameObject.activeSelf)
                {
                    _tutorialText.gameObject.SetActive(
                        false);
                }

                return;
            }

            if (_tracker != null)
            {
                _tutorialStep =
                    TutorialFlowLogic.Advance(
                        _tutorialStep,
                        new TutorialProgress
                        {
                            HypnosisCount =
                                _tracker.HypnosisCount,
                            MaximumFollowers =
                                _tracker.MaximumFollowers,
                            RecoveryCount =
                                _tracker.RecoveryCount,
                            ReclaimCount =
                                _tracker.ReclaimCount,
                            RampageSurvivedCount =
                                _tracker.RampageSurvivedCount
                        });
            }

            if (TutorialFlowLogic.IsCompleted(
                    _tutorialStep))
            {
                SaveTutorialCompleted();

                return;
            }

            if (!_tutorialText.gameObject.activeSelf)
            {
                _tutorialText.gameObject.SetActive(
                    true);
            }

            _tutorialText.text =
                TutorialFlowLogic.GetHint(
                    _tutorialStep);
        }

        /// <summary>튜토리얼을 마친 순간 한 번만 세이브에 기록한다.</summary>
        private void SaveTutorialCompleted()
        {
            GameSession session =
                GameSession.Instance;

            if (session == null ||
                session.Save == null ||
                session.Save.TutorialCompleted)
            {
                return;
            }

            session.Save.TutorialCompleted = true;

            session.WriteSave();
        }

        /// <summary>층 표시와 계단 안내를 갱신한다.</summary>
        private void RefreshFloor()
        {
            if (_floors == null ||
                _floorMarks == null)
            {
                return;
            }

            int current =
                _floors.CurrentFloor;

            // 층 표시는 층을 옮기거나 새 층을 처음 밟을 때만 바뀐다.
            if (UiChangeKey.Changed(
                    ref _floorKey,
                    UiChangeKey.Of(
                        current,
                        _floors.Run == null
                            ? 0
                            : _floors.Run.VisitedCount)))
            {
                RedrawFloorMarks(
                    current);
            }

            FloorStairway stairway =
                _floors.ActiveStairway;

            bool show =
                stairway != null;

            if (_stairPrompt.gameObject.activeSelf != show)
            {
                _stairPrompt.gameObject.SetActive(
                    show);
            }

            if (show &&
                UiChangeKey.Changed(
                    ref _stairKey,
                    UiChangeKey.Of(
                        stairway.TargetFloor,
                        (int)stairway.Direction)))
            {
                _stairPromptText.text =
                    stairway.GetPromptText();
            }
        }

        private void RedrawFloorMarks(
            int current)
        {
            _floorLabel.text =
                FloorPlanLogic.GetLabel(
                    current);

            for (int i = 0;
                 i < _floorMarks.Length;
                 i++)
            {
                if (_floorMarks[i] == null)
                {
                    continue;
                }

                bool isCurrent =
                    i == current;

                bool visited =
                    _floors.Run != null &&
                    _floors.Run.IsVisited(
                        i);

                _floorMarks[i].color =
                    isCurrent
                        ? UiTheme.Accent
                        : visited
                            ? UiTheme.AccentSoft * 0.55f
                            : UiTheme.TrackFill;
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
