using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 계약 서큐버스 허브다.
    ///
    /// 스테이지에서 돌아온 결과를 세이브에 반영하고,
    /// 계약 정기로 4계열 영구 성장을 구매한다.
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
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

        private readonly UpgradeRow[] _rows =
            new UpgradeRow[4];

        private readonly UiButton[] _difficultyButtons =
            new UiButton[3];

        private Text _essenceText;
        private Text _resultText;
        private Text _resultDetailText;
        private Image _resultAccent;
        private Text _statsText;
        private Text _assetText;
        private Text _difficultyHintText;
        private UiButton _shakeButton;

        private bool _resultApplied;

        private StageResultSummary _lastResult =
            StageResultSummary.Empty;

        private bool _hasLastResult;

        private AchievementPanel _achievements;
        private SettingsPanel _settings;
        private ControlsPanel _controls;
        private Canvas _canvas;

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

            Image backdrop =
                UiFactory.CreateImage(
                    canvas.transform,
                    "Backdrop",
                    UiTheme.Backdrop);

            UiFactory.Stretch(
                backdrop.rectTransform);

            // 20일차 꾸미기: 배경에 느리게 떠오르는 보라 빛 알갱이
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
                    0.22f));

            BuildHeader(
                canvas.transform);

            // 본문은 좌(정보) · 우(성장)로 나눈다.
            RectTransform body =
                UiFactory.CreateRect(
                    canvas.transform,
                    "Body");

            body.anchorMin = new Vector2(0f, 0f);
            body.anchorMax = new Vector2(1f, 1f);
            body.offsetMin = new Vector2(80f, 132f);
            body.offsetMax = new Vector2(-80f, -128f);

            RectTransform left =
                UiFactory.CreateRect(
                    body,
                    "LeftColumn");

            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0.34f, 1f);
            left.offsetMin = Vector2.zero;
            left.offsetMax = new Vector2(-16f, 0f);

            RectTransform right =
                UiFactory.CreateRect(
                    body,
                    "RightColumn");

            right.anchorMin = new Vector2(0.34f, 0f);
            right.anchorMax = new Vector2(1f, 1f);
            right.offsetMin = new Vector2(16f, 0f);
            right.offsetMax = Vector2.zero;

            BuildResultCard(
                left);

            BuildStatsCard(
                left);

            BuildUpgradeCard(
                right);

            BuildDifficultyCard(
                right);

            BuildSettingsCard(
                right);

            BuildFooter(
                canvas.transform);

            // 30일차: 업적 목록 창.
            _achievements =
                gameObject.AddComponent<AchievementPanel>();

            _achievements.Build(
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
        }

        private void BuildHeader(
            Transform parent)
        {
            RectTransform header =
                UiFactory.CreateRect(
                    parent,
                    "Header");

            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = new Vector2(0f, 0f);
            header.offsetMax = new Vector2(0f, 0f);
            header.sizeDelta = new Vector2(0f, 112f);

            Image fill =
                UiFactory.CreateImage(
                    header,
                    "Fill",
                    new Color(
                        0.085f,
                        0.065f,
                        0.125f,
                        1f));

            UiFactory.Stretch(
                fill.rectTransform);

            Image underline =
                UiFactory.CreateImage(
                    header,
                    "Underline",
                    UiTheme.PanelEdge);

            RectTransform underlineRect =
                underline.rectTransform;

            underlineRect.anchorMin = new Vector2(0f, 0f);
            underlineRect.anchorMax = new Vector2(1f, 0f);
            underlineRect.pivot = new Vector2(0.5f, 0f);
            underlineRect.offsetMin = Vector2.zero;
            underlineRect.offsetMax = Vector2.zero;
            underlineRect.sizeDelta = new Vector2(0f, 2f);

            Text title =
                UiFactory.CreateText(
                    header,
                    "Title",
                    "계약 서큐버스 허브",
                    UiTheme.FontHeading + 4,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                title.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(80f, 8f),
                new Vector2(700f, 36f));

            Text caption =
                UiFactory.CreateText(
                    header,
                    "Caption",
                    "출격 준비 · 영구 성장 · 난이도 선택",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(80f, -22f),
                new Vector2(700f, 22f));

            Text essenceCaption =
                UiFactory.CreateText(
                    header,
                    "EssenceCaption",
                    "계약 정기",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleRight);

            UiFactory.Place(
                essenceCaption.rectTransform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-80f, 18f),
                new Vector2(400f, 22f));

            _essenceText =
                UiFactory.CreateText(
                    header,
                    "Essence",
                    "-",
                    UiTheme.FontTitle - 8,
                    UiTheme.Gold,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(
                _essenceText.rectTransform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-80f, -16f),
                new Vector2(400f, 46f));
        }

        private void BuildResultCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "ResultCard",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.offsetMin = new Vector2(0f, 0f);
            card.offsetMax = new Vector2(0f, 0f);
            card.sizeDelta = new Vector2(0f, 168f);

            // 왼쪽 세로 막대로 클리어 여부를 색으로 알린다.
            _resultAccent =
                UiFactory.CreateImage(
                    card,
                    "Accent",
                    UiTheme.AccentSoft);

            RectTransform accentRect =
                _resultAccent.rectTransform;

            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.offsetMin = new Vector2(0f, 2f);
            accentRect.offsetMax = new Vector2(0f, -2f);
            accentRect.sizeDelta = new Vector2(6f, 0f);

            Text caption =
                UiFactory.CreateText(
                    card,
                    "Caption",
                    "직전 도전",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.UpperLeft);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -20f),
                new Vector2(380f, 22f));

            _resultText =
                UiFactory.CreateText(
                    card,
                    "Result",
                    "출격 준비 완료",
                    UiTheme.FontHeading,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                _resultText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -50f),
                new Vector2(420f, 34f));

            _resultDetailText =
                UiFactory.CreateText(
                    card,
                    "Detail",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.UpperLeft);

            _resultDetailText.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            UiFactory.Place(
                _resultDetailText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -94f),
                new Vector2(420f, 60f));
        }

        private void BuildStatsCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "StatsCard",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.offsetMin = new Vector2(0f, 0f);
            card.offsetMax = new Vector2(0f, 0f);

            card.anchoredPosition =
                new Vector2(0f, -184f);

            card.sizeDelta = new Vector2(0f, 150f);

            Text caption =
                UiFactory.CreateText(
                    card,
                    "Caption",
                    "누적 기록",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.UpperLeft);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -20f),
                new Vector2(380f, 22f));

            _statsText =
                UiFactory.CreateText(
                    card,
                    "Stats",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft);

            _statsText.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            UiFactory.Place(
                _statsText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -48f),
                new Vector2(400f, 70f));

            // 30일차: 업적 목록 버튼.
            UiButton achievements =
                UiFactory.CreateButton(
                    card,
                    "AchievementsButton",
                    "업적 보기",
                    UiTheme.FontSmall,
                    true);

            UiFactory.Place(
                achievements.Background.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-20f, -14f),
                new Vector2(130f, 36f));

            achievements.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(
                        GameSfx.UiTick);

                    _achievements.Open(
                        GameSession.Instance == null
                            ? null
                            : GameSession.Instance.Save);
                });

            _assetText =
                UiFactory.CreateText(
                    card,
                    "AssetState",
                    string.Empty,
                    UiTheme.FontTiny,
                    UiTheme.TextDisabled,
                    TextAnchor.LowerLeft);

            UiFactory.Place(
                _assetText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(24f, 16f),
                new Vector2(420f, 20f));
        }

        private void BuildUpgradeCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "UpgradeCard",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.offsetMin = new Vector2(0f, 0f);
            card.offsetMax = new Vector2(0f, 0f);
            card.sizeDelta = new Vector2(0f, 352f);

            Text caption =
                UiFactory.CreateText(
                    card,
                    "Caption",
                    "영구 성장",
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -18f),
                new Vector2(300f, 26f));

            Text hint =
                UiFactory.CreateText(
                    card,
                    "Hint",
                    "계열당 5레벨 · 총 600 정기",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.UpperRight);

            UiFactory.Place(
                hint.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-28f, -20f),
                new Vector2(400f, 22f));

            const float rowHeight = 62f;
            const float firstRowTop = 58f;

            for (int i = 0;
                 i < Tracks.Length;
                 i++)
            {
                _rows[i] =
                    BuildUpgradeRow(
                        card,
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

        /// <summary>
        /// 설정 카드다. 지금은 화면 흔들림 켜기·끄기 하나뿐이다 (19일차).
        /// 흔들림과 번쩍임은 사람에 따라 멀미를 일으킬 수 있어 끌 수 있게 했다.
        /// 끄면 흔들림만 빠지고 나머지 연출은 그대로다.
        /// </summary>
        private void BuildSettingsCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "SettingsCard",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.offsetMin = new Vector2(0f, 0f);
            card.offsetMax = new Vector2(0f, 0f);

            card.anchoredPosition =
                new Vector2(0f, -518f);

            card.sizeDelta = new Vector2(0f, 84f);

            Text caption =
                UiFactory.CreateText(
                    card,
                    "Caption",
                    "설정",
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(28f, 0f),
                new Vector2(160f, 30f));

            _shakeButton =
                UiFactory.CreateButton(
                    card,
                    "ScreenShake",
                    string.Empty,
                    UiTheme.FontBody);

            UiFactory.Place(
                _shakeButton.Background.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(190f, 0f),
                new Vector2(260f, 44f));

            _shakeButton.Button.onClick.AddListener(
                ToggleScreenShake);

            // 31일차: 음량 · 화면 · 커서는 공용 설정 창에서.
            UiButton more =
                UiFactory.CreateButton(
                    card,
                    "MoreSettings",
                    "설정 더 보기",
                    UiTheme.FontBody,
                    true);

            UiFactory.Place(
                more.Background.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(476f, 0f),
                new Vector2(200f, 44f));

            more.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _settings.Open();
                });

            UiButton controls =
                UiFactory.CreateButton(
                    card,
                    "Controls",
                    "조작법",
                    UiTheme.FontBody);

            UiFactory.Place(
                controls.Background.rectTransform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(692f, 0f),
                new Vector2(160f, 44f));

            controls.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _controls.Open();
                });
        }

        private void ToggleScreenShake()
        {
            GameSession session =
                GameSession.Instance;

            if (session == null ||
                session.Save == null)
            {
                return;
            }

            session.Save.ScreenShakeDisabled =
                !session.Save.ScreenShakeDisabled;

            CameraShake.Enabled =
                !session.Save.ScreenShakeDisabled;

            GameAudio.Play(
                GameSfx.UiTick);

            session.WriteSave();

            Refresh();
        }

        private void RefreshSettings(
            SaveData save)
        {
            if (_shakeButton.Button == null)
            {
                return;
            }

            bool enabled =
                save == null ||
                !save.ScreenShakeDisabled;

            _shakeButton.SetText(
                enabled
                    ? "화면 흔들림  켜짐"
                    : "화면 흔들림  꺼짐");

            _shakeButton.Background.color =
                enabled
                    ? UiTheme.PrimaryButtonNormal
                    : UiTheme.ButtonNormal;

            _shakeButton.Label.color =
                enabled
                    ? UiTheme.TextPrimary
                    : UiTheme.TextMuted;
        }

        private void BuildDifficultyCard(
            Transform parent)
        {
            RectTransform card =
                UiFactory.CreatePanel(
                    parent,
                    "DifficultyCard",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            card.anchorMin = new Vector2(0f, 1f);
            card.anchorMax = new Vector2(1f, 1f);
            card.pivot = new Vector2(0.5f, 1f);
            card.offsetMin = new Vector2(0f, 0f);
            card.offsetMax = new Vector2(0f, 0f);

            card.anchoredPosition =
                new Vector2(0f, -368f);

            card.sizeDelta = new Vector2(0f, 134f);

            Text caption =
                UiFactory.CreateText(
                    card,
                    "Caption",
                    "난이도",
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                caption.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(28f, -16f),
                new Vector2(300f, 26f));

            RectTransform buttonRow =
                UiFactory.CreateRect(
                    card,
                    "Buttons");

            UiFactory.PlaceRow(
                buttonRow,
                48f,
                44f,
                24f);

            for (int i = 0;
                 i < Difficulties.Length;
                 i++)
            {
                DifficultyLevel level =
                    Difficulties[i];

                UiButton button =
                    UiFactory.CreateButton(
                        buttonRow,
                        $"Difficulty_{level}",
                        DifficultyTable.Get(
                            level).DisplayName,
                        UiTheme.FontBody);

                RectTransform rect =
                    button.Background.rectTransform;

                float step =
                    1f / Difficulties.Length;

                rect.anchorMin =
                    new Vector2(step * i, 0f);

                rect.anchorMax =
                    new Vector2(step * (i + 1), 1f);

                rect.offsetMin = new Vector2(4f, 0f);
                rect.offsetMax = new Vector2(-4f, 0f);

                DifficultyLevel captured =
                    level;

                button.Button.onClick.AddListener(
                    () =>
                    {
                        BalanceOverrides.SetDifficulty(
                            captured);

                        GameAudio.Play(
                            GameSfx.UiTick);

                        Refresh();
                    });

                _difficultyButtons[i] = button;
            }

            _difficultyHintText =
                UiFactory.CreateText(
                    card,
                    "Hint",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.LowerLeft);

            UiFactory.Place(
                _difficultyHintText.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(28f, 12f),
                new Vector2(900f, 22f));
        }

        private void BuildFooter(
            Transform parent)
        {
            RectTransform footer =
                UiFactory.CreateRect(
                    parent,
                    "Footer");

            footer.anchorMin = new Vector2(0f, 0f);
            footer.anchorMax = new Vector2(1f, 0f);
            footer.pivot = new Vector2(0.5f, 0f);
            footer.offsetMin = Vector2.zero;
            footer.offsetMax = Vector2.zero;
            footer.sizeDelta = new Vector2(0f, 116f);

            Image fill =
                UiFactory.CreateImage(
                    footer,
                    "Fill",
                    new Color(
                        0.085f,
                        0.065f,
                        0.125f,
                        1f));

            UiFactory.Stretch(
                fill.rectTransform);

            UiButton sortie =
                UiFactory.CreateButton(
                    footer,
                    "Sortie",
                    "출  격",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(
                sortie.Background.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(110f, 0f),
                new Vector2(340f, 62f));

            sortie.Button.onClick.AddListener(
                () =>
                {
                    // 29일차: 판이 없다. 출격하면 도시 지도에서 아무 장소나 고른다.
                    if (GameSession.Instance == null)
                    {
                        return;
                    }

                    GameSession.Instance.GoTo(
                        SceneDestination.Map);
                });

            UiButton back =
                UiFactory.CreateButton(
                    footer,
                    "BackToTitle",
                    "타이틀로",
                    UiTheme.FontBody);

            UiFactory.Place(
                back.Background.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-180f, 0f),
                new Vector2(200f, 54f));

            back.Button.onClick.AddListener(
                () =>
                {
                    GameSession.Instance?.GoTo(
                        SceneDestination.MainMenu);
                });
        }

        // 표시 갱신 ------------------------------------------------------

        private void Refresh()
        {
            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            RefreshEssence(
                save);

            RefreshResult();

            RefreshUpgrades(
                save);

            RefreshDifficulty();

            RefreshStats(
                save);

            RefreshSettings(
                save);
        }

        private void RefreshEssence(
            SaveData save)
        {
            if (_essenceText == null)
            {
                return;
            }

            _essenceText.text =
                save == null
                    ? "-"
                    : save.ContractEssence.ToString("N0");
        }

        private void RefreshResult()
        {
            if (_resultText == null)
            {
                return;
            }

            if (!_hasLastResult)
            {
                _resultText.text = "출격 준비 완료";
                _resultText.color = UiTheme.TextPrimary;

                _resultDetailText.text =
                    "지도에서 장소를 골라 정기를 회수하세요";

                _resultAccent.color =
                    UiTheme.AccentSoft;

                return;
            }

            string place =
                _lastResult.HasLocation
                    ? LocationCatalog.Get((LocationId)_lastResult.LocationId).DisplayName + "  "
                    : string.Empty;

            _resultText.text =
                _lastResult.Cleared
                    ? $"{place}클리어   랭크 {_lastResult.RankLabel}"
                    : $"{place}실패";

            _resultText.color =
                _lastResult.Cleared
                    ? UiTheme.Positive
                    : UiTheme.Danger;

            _resultAccent.color =
                _lastResult.Cleared
                    ? UiTheme.Positive
                    : UiTheme.Danger;

            string detail =
                $"{_lastResult.TotalScore:N0}점\n계약 정기 +{_lastResult.ContractEssence}";

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
            if (_statsText == null)
            {
                return;
            }

            if (save == null)
            {
                _statsText.text =
                    "저장 정보를 불러오지 못했습니다";

                _assetText.text = string.Empty;

                return;
            }

            // 30일차: 29일차 누적 통계 요약 · 업적 · 엔딩 해금.
            PlayStats stats =
                save.Stats ?? new PlayStats();

            string unlock =
                MasteryLogic.HasEndingUnlock(save)
                    ? $"엔딩 {stats.Endings}회 · 해금: 시작 계약 카드 {MasteryLogic.EndingStartChoices}장"
                    : "엔딩 전 · 루프탑 클럽 보스를 함락하면 해금";

            _statsText.text =
                $"플레이 {PlayStatsLogic.FormatDuration(stats.TotalSeconds)} · 도전 {stats.Attempts} · 클리어 {stats.Clears} ({PlayStatsLogic.FormatPercent(PlayStatsLogic.GetClearRate(stats.Attempts, stats.Clears))})\n" +
                $"업적 {AchievementLogic.CountUnlocked(save)}/{AchievementLogic.All.Length} · 최고 랭크 {save.BestRankLabel} · 최고 점수 {save.BestScore:N0}\n" +
                unlock;

            _assetText.text =
                BalanceBootstrap.StageAssetApplied
                    ? "밸런스 자산 적용됨"
                    : "밸런스 자산 없음 - 코드 기본값 사용 중";

            _assetText.color =
                BalanceBootstrap.StageAssetApplied
                    ? UiTheme.TextDisabled
                    : UiTheme.Danger;
        }

        // 동작 ----------------------------------------------------------

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
