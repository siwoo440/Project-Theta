using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.Story;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 계약 서큐버스 허브다.
    ///
    /// 스테이지에서 돌아온 결과를 세이브에 반영하고,
    /// 계약 정기로 4계열 영구 성장을 구매한다.
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
    ///
    /// 36일차: 배경을 "밤의 학생 방"(<see cref="HubRoomBackdrop"/>)으로 바꾸고 UI를 가장자리 반투명 띠로 줄였다.
    ///   위 띠   : 칸 · 계약 정기 · 도시 지배도 · [업적] [조작법] [설정]
    ///   아래 띠 : [강화] [통계] [난이도] [일기장] [저장] [타이틀로] ········ [출격 ▶]
    ///   창      : 강화 · 통계 · 난이도 · 일기장은 버튼(또는 방 물건)으로 켜고 끈다. 한 번에 하나.
    ///   서큐버스 : 들어올 때마다 방 안 무작위 자리. 누르면 머리 위 말풍선(<see cref="HubSuccubus"/>).
    /// </summary>
    public sealed class HubScreen : MonoBehaviour
    {
        private static readonly UpgradeTrack[] Tracks =
        {
            UpgradeTrack.Hypnosis,
            UpgradeTrack.Control,
            UpgradeTrack.Stability,
            UpgradeTrack.Mobility
        };

        private static readonly DifficultyLevel[] Difficulties =
        {
            DifficultyLevel.Story,
            DifficultyLevel.Normal,
            DifficultyLevel.Challenge
        };

        private const float TopBarHeight = 76f;
        private const float BottomBarHeight = 96f;

        /// <summary>업그레이드 한 줄을 이루는 조각 묶음이다.</summary>
        private sealed class UpgradeRow
        {
            public UpgradeTrack Track;
            public Text Pips;
            public Text Effect;
            public UiButton Buy;
            public Text MaxLabel;
            public Image Background;
        }

        /// <summary>창이 열려 있는 동안 Esc 순서에 올리는 표식이다.</summary>
        private sealed class WindowToken
        {
        }

        private readonly UpgradeRow[] _rows =
            new UpgradeRow[4];

        private readonly UiButton[] _difficultyButtons =
            new UiButton[3];

        private Text _slotText;
        private Text _essenceText;
        private Text _dominionText;
        private UiBar _dominionBar;

        private GameObject _resultCard;
        private Text _resultText;
        private Text _resultDetailText;
        private Image _resultAccent;

        private Text _upgradeEssence;
        private Text _statsText;
        private Text _dominionDetail;
        private Text _assetText;
        private Text _difficultyHintText;
        private Text _diaryHint;
        private readonly List<UiButton> _diaryButtons = new List<UiButton>();

        // 34일차: 저장 버튼 · 결과 문구
        private Text _saveMessage;
        private float _saveMessageRemaining;

        private bool _resultApplied;

        private StageResultSummary _lastResult =
            StageResultSummary.Empty;

        private bool _hasLastResult;

        private AchievementPanel _achievements;
        private StatsPanel _statsPanel;
        private SettingsPanel _settings;
        private ControlsPanel _controls;
        private Canvas _canvas;

        // 36일차: 방 · 창 · 이야기
        private HubRoomBackdrop _room;
        private readonly Dictionary<HubPanel, HubWindow> _windows = new Dictionary<HubPanel, HubWindow>();
        private readonly Dictionary<HubPanel, UiButton> _panelButtons = new Dictionary<HubPanel, UiButton>();
        private readonly WindowToken _windowToken = new WindowToken();
        private HubPanel _openPanel;
        private DialogueOverlay _story;

        private void Start()
        {
            GameSession game =
                GameSession.Instance;

            if (game != null)
            {
                _resultApplied =
                    game.ConsumePendingResult() ||
                    game.HasLastResult;

                // 30일차: 결과는 지도로 갈 때도 반영되므로, 세션이 기억한 마지막 결과를 보여 준다.
                _hasLastResult =
                    game.HasLastResult;

                if (_hasLastResult)
                {
                    _lastResult =
                        game.LastResult;
                }
            }

            Build();
            Refresh();

            if (game != null)
            {
                AchievementToast.Show(
                    _canvas.transform,
                    game.TakeNewAchievements());
            }

            // 36일차: 첫 클리어 · 라이벌 도발 · 후일담
            StoryPlayback.PlayPending(_story, Refresh);
        }

        private void Update()
        {
            if (_saveMessageRemaining > 0f &&
                _saveMessage != null)
            {
                _saveMessageRemaining -= Time.unscaledDeltaTime;

                if (_saveMessageRemaining <= 0f)
                {
                    _saveMessage.text = string.Empty;
                }
            }

#if ENABLE_INPUT_SYSTEM
            if (_openPanel == HubPanel.None ||
                (_story != null && _story.IsPlaying))
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(_windowToken, Time.frameCount))
            {
                SetPanel(HubPanel.None);
            }
#endif
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(_windowToken);
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "HubCanvas",
                    0,
                    transform);

            _canvas = canvas;

            // 36일차: 배경은 밤의 학생 방이다.
            _room = gameObject.AddComponent<HubRoomBackdrop>();
            _room.Build(canvas.transform);
            _room.ObjectClicked += HandleRoomObject;

            // 36일차: 방 안의 서큐버스. 매번 다른 자리에 있고 누르면 말풍선으로 말한다.
            gameObject.AddComponent<HubSuccubus>().Build(
                canvas.transform,
                () => CurrentSave,
                () => _openPanel != HubPanel.None ||
                      (_story != null && _story.IsPlaying));

            RectTransform motes =
                UiFactory.CreateRect(
                    canvas.transform,
                    "Motes");

            UiFactory.Stretch(
                motes);

            motes.gameObject.AddComponent<UiFloatingMotes>().Build(
                new Color(
                    UiTheme.Accent.r,
                    UiTheme.Accent.g,
                    UiTheme.Accent.b,
                    0.16f));

            BuildTopBar(canvas.transform);
            BuildResultCard(canvas.transform);
            BuildBottomBar(canvas.transform);

            BuildUpgradeWindow(canvas.transform);
            BuildStatsWindow(canvas.transform);
            BuildDifficultyWindow(canvas.transform);
            BuildDiaryWindow(canvas.transform);

            // 30일차: 업적 목록 창.
            _achievements =
                gameObject.AddComponent<AchievementPanel>();

            _achievements.Build(
                canvas.transform);

            // 36일차: 자세한 통계 창(지도와 같은 창).
            _statsPanel =
                gameObject.AddComponent<StatsPanel>();

            _statsPanel.Build(
                canvas.transform);

            // 31일차: 공용 설정 · 조작법 창.
            _settings =
                SettingsPanel.Create(
                    canvas.transform,
                    60);

            _settings.Closed += Refresh;

            _controls =
                ControlsPanel.Create(
                    canvas.transform,
                    60);

            _story = DialogueOverlay.Create(canvas.transform, StoryPlayback.SortOrder);
        }

        private static RectTransform CreateBar(
            Transform parent,
            string name,
            bool top,
            float height)
        {
            RectTransform bar = UiFactory.CreateRect(parent, name);

            bar.anchorMin = new Vector2(0f, top ? 1f : 0f);
            bar.anchorMax = new Vector2(1f, top ? 1f : 0f);
            bar.pivot = new Vector2(0.5f, top ? 1f : 0f);
            bar.offsetMin = Vector2.zero;
            bar.offsetMax = Vector2.zero;
            bar.sizeDelta = new Vector2(0f, height);
            bar.anchoredPosition = Vector2.zero;

            Image fill = UiFactory.CreateImage(bar, "Fill", new Color(0.04f, 0.03f, 0.07f, HubRoomLogic.BarAlpha));
            UiFactory.Stretch(fill.rectTransform);

            Image line = UiFactory.CreateImage(bar, "Line", new Color(UiTheme.AccentSoft.r, UiTheme.AccentSoft.g, UiTheme.AccentSoft.b, 0.5f));
            line.rectTransform.anchorMin = new Vector2(0f, top ? 0f : 1f);
            line.rectTransform.anchorMax = new Vector2(1f, top ? 0f : 1f);
            line.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            line.rectTransform.sizeDelta = new Vector2(0f, 2f);
            line.rectTransform.anchoredPosition = Vector2.zero;

            return bar;
        }

        private void BuildTopBar(
            Transform parent)
        {
            RectTransform bar = CreateBar(parent, "TopBar", true, TopBarHeight);

            _slotText = UiFactory.CreateText(bar, "Slot", string.Empty, UiTheme.FontSmall, UiTheme.TextMuted, TextAnchor.MiddleLeft);
            UiFactory.Place(_slotText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 14f), new Vector2(360f, 24f));

            Text caption = UiFactory.CreateText(bar, "EssenceCaption", "계약 정기", UiTheme.FontSmall, UiTheme.TextMuted, TextAnchor.MiddleLeft);
            UiFactory.Place(caption.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, -14f), new Vector2(100f, 24f));

            _essenceText = UiFactory.CreateText(bar, "Essence", "-", UiTheme.FontHeading + 4, UiTheme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Place(_essenceText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(130f, -12f), new Vector2(260f, 34f));

            // 도시 지배도 막대
            _dominionText = UiFactory.CreateText(bar, "Dominion", string.Empty, UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Place(_dominionText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(620f, 26f));

            _dominionBar = UiFactory.CreateBar(bar, "DominionBar", new Color(0.78f, 0.45f, 1f, 1f), UiTheme.TrackFill);
            UiFactory.Place(_dominionBar.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(480f, 12f));

            TopButton(bar, "AchievementsButton", "업  적", -330f, () => _achievements.Open(CurrentSave));
            TopButton(bar, "ControlsButton", "조작법", -190f, () => _controls.Open());
            TopButton(bar, "SettingsButton", "설  정", -50f, () => _settings.Open());
        }

        private static void TopButton(
            RectTransform bar,
            string name,
            string label,
            float x,
            System.Action onClick)
        {
            UiButton button = UiFactory.CreateButton(bar, name, label, UiTheme.FontBody);
            UiFactory.Place(button.Background.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(x - 40f, 0f), new Vector2(124f, 44f));

            button.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    onClick();
                });
        }

        /// <summary>직전 도전 결과다. 결과가 있을 때만 왼쪽 아래에 작게 뜬다.</summary>
        private void BuildResultCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "ResultCard",
                    new Color(0.05f, 0.04f, 0.09f, 0.62f),
                    new Color(UiTheme.PanelEdge.r, UiTheme.PanelEdge.g, UiTheme.PanelEdge.b, 0.6f));

            UiFactory.Place(card, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(32f, BottomBarHeight + 18f), new Vector2(420f, 112f));

            _resultCard = card.gameObject;

            _resultAccent = UiFactory.CreateImage(card, "Accent", UiTheme.AccentSoft);
            UiFactory.Place(_resultAccent.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(6f, 108f));

            Text caption = UiFactory.CreateText(card, "Caption", "직전 도전", UiTheme.FontSmall, UiTheme.TextMuted, TextAnchor.UpperLeft);
            UiFactory.Place(caption.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -10f), new Vector2(380f, 22f));

            _resultText = UiFactory.CreateText(card, "Result", string.Empty, UiTheme.FontSubheading, UiTheme.TextPrimary, TextAnchor.UpperLeft, FontStyle.Bold);
            UiFactory.Place(_resultText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -34f), new Vector2(390f, 28f));

            _resultDetailText = UiFactory.CreateText(card, "Detail", string.Empty, UiTheme.FontSmall, UiTheme.TextMuted, TextAnchor.UpperLeft);
            UiFactory.Place(_resultDetailText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -64f), new Vector2(390f, 44f));
        }

        private void BuildBottomBar(
            Transform parent)
        {
            RectTransform bar = CreateBar(parent, "BottomBar", false, BottomBarHeight);

            float x = 40f;

            foreach (HubPanel panel in HubRoomLogic.BottomPanels)
            {
                HubPanel captured = panel;
                UiButton button = BottomButton(bar, $"Panel_{panel}", HubRoomLogic.GetButtonLabel(panel), x, () => TogglePanel(captured));
                _panelButtons[panel] = button;
                x += 150f;
            }

            x += 30f;

            // 34일차: 지금 칸에 바로 저장한다(장소를 마칠 때도 자동 저장된다).
            BottomButton(bar, "SaveNow", "저  장", x, SaveNow);
            x += 150f;

            BottomButton(
                bar,
                "BackToTitle",
                "타이틀로",
                x,
                () => GameSession.Instance?.GoTo(SceneDestination.MainMenu));

            x += 160f;

            _saveMessage = UiFactory.CreateText(bar, "SaveMessage", string.Empty, UiTheme.FontBody, UiTheme.Gold, TextAnchor.MiddleLeft);
            UiFactory.Place(_saveMessage.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(460f, 40f));

            UiButton sortie = UiFactory.CreateButton(bar, "Sortie", "출    격  ▶", UiTheme.FontHeading, true);
            UiFactory.Place(sortie.Background.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(320f, 62f));
            sortie.Button.onClick.AddListener(Sortie);
        }

        private static UiButton BottomButton(
            RectTransform bar,
            string name,
            string label,
            float x,
            UnityEngine.Events.UnityAction onClick)
        {
            UiButton button = UiFactory.CreateButton(bar, name, label, UiTheme.FontBody);
            UiFactory.Place(button.Background.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(136f, 52f));
            button.Button.onClick.AddListener(onClick);

            return button;
        }

        // 창 ------------------------------------------------------------

        private HubWindow CreateWindow(
            Transform parent,
            HubPanel panel,
            Vector2 size)
        {
            HubWindow window =
                HubWindow.Create(
                    parent,
                    $"Window_{panel}",
                    HubRoomLogic.GetPanelTitle(panel),
                    size,
                    () => SetPanel(HubPanel.None));

            _windows[panel] = window;

            return window;
        }

        private void BuildUpgradeWindow(
            Transform parent)
        {
            HubWindow window = CreateWindow(parent, HubPanel.Upgrades, new Vector2(1060f, 440f));
            RectTransform body = window.Body;

            _upgradeEssence = UiFactory.CreateText(body, "Essence", string.Empty, UiTheme.FontBody, UiTheme.Gold, TextAnchor.MiddleRight, FontStyle.Bold);
            UiFactory.Place(_upgradeEssence.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -18f), new Vector2(420f, 34f));

            Text hint = UiFactory.CreateText(body, "Hint", "계열당 5레벨 · 총 600 정기 · 구매하면 바로 저장됩니다", UiTheme.FontSmall, UiTheme.TextMuted, TextAnchor.MiddleLeft);
            UiFactory.Place(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -76f), new Vector2(700f, 24f));

            const float rowHeight = 62f;
            const float firstRowTop = 110f;

            for (int i = 0;
                 i < Tracks.Length;
                 i++)
            {
                _rows[i] =
                    BuildUpgradeRow(
                        body,
                        Tracks[i],
                        firstRowTop +
                        (rowHeight + 6f) * i,
                        rowHeight);
            }
        }

        private UpgradeRow BuildUpgradeRow(
            Transform parent,
            UpgradeTrack track,
            float topOffset,
            float height)
        {
            Image background =
                UiFactory.CreateImage(
                    parent,
                    $"Row_{track}",
                    UiTheme.RowFill);

            UiFactory.PlaceRow(
                background.rectTransform,
                topOffset,
                height,
                24f);

            Text name =
                UiFactory.CreateText(
                    background.transform,
                    "Name",
                    UpgradeLogic.GetTrackName(
                        track),
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                name.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(20f, 11f),
                new Vector2(200f, 24f));

            Text pips =
                UiFactory.CreateText(
                    background.transform,
                    "Pips",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.Accent,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(
                pips.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(20f, -13f),
                new Vector2(200f, 22f));

            Text effect =
                UiFactory.CreateText(
                    background.transform,
                    "Effect",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(
                effect.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(240f, 0f),
                new Vector2(520f, 24f));

            UiButton buy =
                UiFactory.CreateButton(
                    background.transform,
                    "Buy",
                    string.Empty,
                    UiTheme.FontBody);

            UiFactory.Place(
                buy.Background.rectTransform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-20f, 0f),
                new Vector2(150f, 40f));

            UpgradeTrack captured =
                track;

            buy.Button.onClick.AddListener(
                () =>
                {
                    Purchase(
                        captured);
                });

            Text maxLabel =
                UiFactory.CreateText(
                    background.transform,
                    "MaxLabel",
                    "최대",
                    UiTheme.FontBody,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                maxLabel.rectTransform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-20f, 0f),
                new Vector2(150f, 40f));

            maxLabel.gameObject.SetActive(
                false);

            return new UpgradeRow
            {
                Track = track,
                Pips = pips,
                Effect = effect,
                Buy = buy,
                MaxLabel = maxLabel,
                Background = background
            };
        }

        private void BuildStatsWindow(
            Transform parent)
        {
            HubWindow window = CreateWindow(parent, HubPanel.Stats, new Vector2(1000f, 640f));
            RectTransform body = window.Body;

            _statsText = UiFactory.CreateText(body, "Stats", string.Empty, UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.UpperLeft);
            UiFactory.Place(_statsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -84f), new Vector2(940f, 90f));
            _statsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _statsText.lineSpacing = 1.3f;

            Text caption = UiFactory.CreateText(body, "DominionCaption", "도시 지배도 · 장소마다 클리어 1 + 숙련 ★3 + 심야 ☾ 1 = 5점", UiTheme.FontSmall, UiTheme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Place(caption.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -186f), new Vector2(940f, 26f));

            _dominionDetail = UiFactory.CreateText(body, "DominionDetail", string.Empty, UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.UpperLeft);
            UiFactory.Place(_dominionDetail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -218f), new Vector2(940f, 300f));
            _dominionDetail.lineSpacing = 1.25f;
            _dominionDetail.supportRichText = true;

            UiButton detail = UiFactory.CreateButton(body, "DetailStats", "자세한 통계", UiTheme.FontBody, true);
            UiFactory.Place(detail.Background.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 24f), new Vector2(200f, 50f));
            detail.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _statsPanel.Open(CurrentSave);
                });

            UiButton achievements = UiFactory.CreateButton(body, "Achievements", "업적 보기", UiTheme.FontBody);
            UiFactory.Place(achievements.Background.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-246f, 24f), new Vector2(170f, 50f));
            achievements.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _achievements.Open(CurrentSave);
                });

            _assetText = UiFactory.CreateText(body, "AssetState", string.Empty, UiTheme.FontTiny, UiTheme.TextDisabled, TextAnchor.LowerLeft);
            UiFactory.Place(_assetText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 30f), new Vector2(500f, 20f));
        }

        private void BuildDifficultyWindow(
            Transform parent)
        {
            HubWindow window = CreateWindow(parent, HubPanel.Difficulty, new Vector2(860f, 260f));
            RectTransform body = window.Body;

            for (int i = 0;
                 i < Difficulties.Length;
                 i++)
            {
                DifficultyLevel level =
                    Difficulties[i];

                DifficultyLevel captured =
                    level;

                _difficultyButtons[i] =
                    UiOverlay.Button(
                        body,
                        $"Difficulty_{level}",
                        DifficultyTable.Get(level).DisplayName,
                        30f + i * 270f,
                        90f,
                        250f,
                        60f,
                        () =>
                        {
                            BalanceOverrides.SetDifficulty(
                                captured);

                            GameAudio.Play(
                                GameSfx.UiTick);

                            Refresh();
                        });
            }

            _difficultyHintText = UiOverlay.Label(body, string.Empty, 30f, 190f, 800f, UiTheme.FontSmall, UiTheme.TextMuted);
        }

        private void BuildDiaryWindow(
            Transform parent)
        {
            HubWindow window = CreateWindow(parent, HubPanel.Diary, new Vector2(1040f, 660f));
            RectTransform body = window.Body;

            _diaryHint = UiOverlay.Label(body, string.Empty, 30f, 82f, 980f, UiTheme.FontSmall, UiTheme.TextMuted);

            const int perColumn = 10;
            const float columnWidth = 480f;
            const float rowHeight = 46f;

            for (int i = 0; i < StoryCatalog.All.Length; i++)
            {
                StoryScene scene = StoryCatalog.All[i];
                int column = i / perColumn;
                int row = i % perColumn;

                UiButton button =
                    UiOverlay.Button(
                        body,
                        $"Story_{scene.Id}",
                        scene.Title,
                        30f + column * (columnWidth + 20f),
                        124f + row * (rowHeight + 6f),
                        columnWidth,
                        rowHeight,
                        () => ReplayStory(scene));

                if (button.Label != null)
                {
                    button.Label.alignment = TextAnchor.MiddleLeft;
                    button.Label.rectTransform.offsetMin = new Vector2(18f, 0f);
                }

                _diaryButtons.Add(button);
            }
        }

        // 창 열고 닫기 --------------------------------------------------

        private void TogglePanel(
            HubPanel panel)
        {
            GameAudio.Play(GameSfx.UiTick);

            SetPanel(
                HubRoomLogic.Toggle(
                    _openPanel,
                    panel));
        }

        private void SetPanel(
            HubPanel panel)
        {
            _openPanel = panel;

            foreach (KeyValuePair<HubPanel, HubWindow> pair in _windows)
            {
                pair.Value.SetOpen(pair.Key == panel);
            }

            foreach (KeyValuePair<HubPanel, UiButton> pair in _panelButtons)
            {
                pair.Value.Background.color =
                    pair.Key == panel
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal;
            }

            if (panel == HubPanel.None)
            {
                UiEscapeStack.Remove(_windowToken);
            }
            else if (!UiEscapeStack.IsTop(_windowToken))
            {
                UiEscapeStack.Remove(_windowToken);
                UiEscapeStack.Push(_windowToken);
            }

            Refresh();
        }

        private void HandleRoomObject(
            HubRoomObject item)
        {
            if (_story != null &&
                _story.IsPlaying)
            {
                return;
            }

            if (HubRoomLogic.IsSortie(item))
            {
                Sortie();

                return;
            }

            HubPanel panel = HubRoomLogic.GetPanel(item);

            if (panel != HubPanel.None)
            {
                TogglePanel(panel);
            }
        }

        private void ReplayStory(
            StoryScene scene)
        {
            if (!StoryLogic.IsSeen(CurrentSave, scene.Id))
            {
                return;
            }

            GameAudio.Play(GameSfx.UiTick);
            StoryPlayback.Play(_story, new[] { scene }, false, null);
        }

        // 표시 갱신 ------------------------------------------------------

        private SaveData CurrentSave =>
            GameSession.Instance == null
                ? null
                : GameSession.Instance.Save;

        private void Refresh()
        {
            SaveData save = CurrentSave;

            RefreshTopBar(save);
            RefreshResult();
            RefreshUpgrades(save);
            RefreshDifficulty();
            RefreshStats(save);
            RefreshDiary(save);
        }

        private void RefreshTopBar(
            SaveData save)
        {
            GameSession session = GameSession.Instance;

            _slotText.text =
                session != null && session.HasActiveSlot
                    ? $"학생의 방  ·  {SaveSlotLogic.GetSlotLabel(session.ActiveSlot)}"
                    : "학생의 방";

            _essenceText.text =
                save == null
                    ? "-"
                    : save.ContractEssence.ToString("N0");

            _upgradeEssence.text =
                save == null
                    ? string.Empty
                    : $"보유 계약 정기  {save.ContractEssence:N0}";

            float ratio = DominionLogic.GetRatio(save);

            _dominionText.text = DominionLogic.GetSummary(save);
            _dominionBar.SetValue(ratio);
            _room.SetDominion(ratio);
        }

        private void RefreshResult()
        {
            _resultCard.SetActive(_hasLastResult);

            if (!_hasLastResult)
            {
                return;
            }

            string place =
                _lastResult.HasLocation
                    ? LocationCatalog.Get((LocationId)_lastResult.LocationId).DisplayName + "  "
                    : string.Empty;

            string night =
                _lastResult.NightMode
                    ? "☾ "
                    : string.Empty;

            _resultText.text =
                _lastResult.Cleared
                    ? $"{night}{place}클리어   랭크 {_lastResult.RankLabel}"
                    : $"{night}{place}실패";

            _resultText.color =
                _lastResult.Cleared
                    ? UiTheme.Positive
                    : UiTheme.Danger;

            _resultAccent.color =
                _lastResult.Cleared
                    ? UiTheme.Positive
                    : UiTheme.Danger;

            string detail =
                $"{_lastResult.TotalScore:N0}점  ·  계약 정기 +{_lastResult.ContractEssence}";

            if (!_resultApplied)
            {
                detail += "\n(반영할 결과가 없습니다)";
            }

            _resultDetailText.text = detail;
        }

        private void RefreshUpgrades(
            SaveData save)
        {
            for (int i = 0;
                 i < _rows.Length;
                 i++)
            {
                UpgradeRow row =
                    _rows[i];

                if (row == null)
                {
                    continue;
                }

                if (save == null)
                {
                    row.Buy.SetInteractable(false);
                    row.Buy.SetText("-");

                    continue;
                }

                int level =
                    SaveDataLogic.GetUpgradeLevel(
                        save,
                        row.Track);

                row.Pips.text =
                    BuildPips(
                        level);

                row.Effect.text =
                    GetTrackEffect(
                        row.Track,
                        level);

                bool maxed =
                    UpgradeLogic.IsMaxLevel(
                        level);

                row.MaxLabel.gameObject.SetActive(
                    maxed);

                row.Buy.Button.gameObject.SetActive(
                    !maxed);

                if (maxed)
                {
                    row.Background.color =
                        new Color(
                            UiTheme.Accent.r * 0.30f,
                            UiTheme.Accent.g * 0.22f,
                            UiTheme.Accent.b * 0.34f,
                            0.92f);

                    continue;
                }

                row.Background.color =
                    UiTheme.RowFill;

                int cost =
                    UpgradeLogic.GetNextLevelCost(
                        level);

                row.Buy.SetText(
                    $"{cost} 정기");

                row.Buy.SetInteractable(
                    UpgradeLogic.CanPurchase(
                        level,
                        save.ContractEssence));
            }
        }

        private void RefreshDifficulty()
        {
            for (int i = 0;
                 i < Difficulties.Length;
                 i++)
            {
                UiButton button =
                    _difficultyButtons[i];

                if (button.Button == null)
                {
                    continue;
                }

                bool selected =
                    BalanceOverrides.Difficulty != null &&
                    BalanceOverrides.Difficulty.Level ==
                    Difficulties[i];

                // 고른 난이도는 보라색으로 채워 한눈에 구분되게 한다.
                button.Background.color =
                    selected
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal;

                button.Label.color =
                    selected
                        ? UiTheme.TextPrimary
                        : UiTheme.TextMuted;

                button.Label.text =
                    DifficultyTable.Get(
                        Difficulties[i]).DisplayName +
                    (selected
                        ? "  ●"
                        : string.Empty);
            }

            if (_difficultyHintText != null)
            {
                _difficultyHintText.text =
                    "목표 정기 · 제한 시간 · 충동 · 쟁탈 속도에 반영됩니다";
            }
        }

        private void RefreshStats(
            SaveData save)
        {
            if (save == null)
            {
                _statsText.text =
                    "저장 정보를 불러오지 못했습니다";

                _dominionDetail.text = string.Empty;
                _assetText.text = string.Empty;

                return;
            }

            // 30일차: 29일차 누적 통계 요약 · 업적 · 엔딩 해금.
            PlayStats stats =
                save.Stats ?? new PlayStats();

            string unlock =
                MasteryLogic.HasEndingUnlock(save)
                    ? $"엔딩 {stats.Endings}회 · 해금: 시작 계약 카드 {MasteryLogic.EndingStartChoices}장 · ☾ 심야 모드"
                    : "엔딩 전 · 루프탑 클럽 보스를 함락하면 시작 카드 4장 · 심야 모드 해금";

            _statsText.text =
                $"플레이 {PlayStatsLogic.FormatDuration(stats.TotalSeconds)} · 도전 {stats.Attempts} · 클리어 {stats.Clears} ({PlayStatsLogic.FormatPercent(PlayStatsLogic.GetClearRate(stats.Attempts, stats.Clears))})\n" +
                $"업적 {AchievementLogic.CountUnlocked(save)}/{AchievementLogic.All.Length} · 최고 랭크 {save.BestRankLabel} · 최고 점수 {save.BestScore:N0}\n" +
                unlock;

            // 36일차: 장소별 지배도 내역
            System.Text.StringBuilder detail = new System.Text.StringBuilder();

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                detail.Append(
                    $"<color=#C9A2FF>{location.DisplayName}</color>   {DominionLogic.DescribeLocation(PlayStatsLogic.Get(save, (int)location.Id))}\n");
            }

            detail.Append($"\n<b>{DominionLogic.GetSummary(save)}</b>");

            _dominionDetail.text = detail.ToString();

            _assetText.text =
                BalanceBootstrap.StageAssetApplied
                    ? "밸런스 자산 적용됨"
                    : "밸런스 자산 없음 - 코드 기본값 사용 중";

            _assetText.color =
                BalanceBootstrap.StageAssetApplied
                    ? UiTheme.TextDisabled
                    : UiTheme.Danger;
        }

        private void RefreshDiary(
            SaveData save)
        {
            int seen = 0;

            for (int i = 0; i < _diaryButtons.Count; i++)
            {
                StoryScene scene = StoryCatalog.All[i];
                bool unlocked = StoryLogic.IsSeen(save, scene.Id);

                if (unlocked)
                {
                    seen++;
                }

                _diaryButtons[i].SetText(unlocked ? $"「{scene.Title}」" : "？？？");
                _diaryButtons[i].SetInteractable(unlocked);
            }

            _diaryHint.text = $"본 이야기 {seen} / {StoryCatalog.All.Length}  ·  누르면 다시 봅니다";
        }

        // 동작 ----------------------------------------------------------

        private void Sortie()
        {
            // 29일차: 판이 없다. 출격하면 도시 지도에서 아무 장소나 고른다.
            if (GameSession.Instance == null)
            {
                return;
            }

            GameSession.Instance.GoTo(
                SceneDestination.Map);
        }

        private void SaveNow()
        {
            GameSession session =
                GameSession.Instance;

            if (session == null)
            {
                return;
            }

            bool saved =
                session.SaveNow();

            GameAudio.Play(
                saved
                    ? GameSfx.UiStamp
                    : GameSfx.UiTick);

            _saveMessage.color =
                saved
                    ? UiTheme.Gold
                    : UiTheme.Danger;

            _saveMessage.text =
                saved
                    ? $"{SaveSlotLogic.GetSlotLabel(session.ActiveSlot)}에 저장했습니다  ·  {session.Save.SavedAt}"
                    : "저장하지 못했습니다";

            _saveMessageRemaining = 4f;
        }

        private void Purchase(
            UpgradeTrack track)
        {
            if (GameSession.Instance == null)
            {
                return;
            }

            if (SaveDataLogic.TryPurchaseUpgrade(
                    GameSession.Instance.Save,
                    track))
            {
                GameAudio.Play(
                    GameSfx.Purchase);

                // 구매 즉시 저장한다. 허브에서 나가기 전에 껐을 때 손실되지 않도록.
                GameSession.Instance.WriteSave();
            }

            Refresh();
        }

        private static string BuildPips(
            int level)
        {
            int clamped =
                UpgradeLogic.ClampLevel(
                    level);

            string pips =
                string.Empty;

            for (int i = 0;
                 i < UpgradeLogic.MaximumLevel;
                 i++)
            {
                pips +=
                    i < clamped
                        ? "●"
                        : "○";
            }

            return pips;
        }

        private static string GetTrackEffect(
            UpgradeTrack track,
            int level)
        {
            switch (track)
            {
                case UpgradeTrack.Hypnosis:
                    return $"최면 속도 ×{UpgradeLogic.GetHypnosisSpeedMultiplier(level):0.00}";

                case UpgradeTrack.Control:
                    return $"관리 한도 {UpgradeLogic.GetStableFollowerLimit(level, 4)}명";

                case UpgradeTrack.Stability:
                    return $"충동 ×{UpgradeLogic.GetImpulseBuildMultiplier(level):0.00}   경고 +{UpgradeLogic.GetRampageWarningBonus(level):0.0}초";

                case UpgradeTrack.Mobility:
                    return $"이동 ×{UpgradeLogic.GetMoveSpeedMultiplier(level):0.00}   대시 비용 ×{UpgradeLogic.GetDashCostMultiplier(level):0.00}";

                default:
                    return string.Empty;
            }
        }
    }
}
