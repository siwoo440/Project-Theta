using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 타이틀 화면이다.
    ///
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
    /// 씬 파일은 여전히 비어 있고, 이 스크립트가 런타임에 화면을 조립한다.
    ///
    /// 34일차: 시작 버튼을 이어하기(저장 칸 불러오기) · 처음부터(새 게임)로 나눴다.
    /// 35일차: 처음부터로 시작하면 소개 화면(<see cref="IntroSequence"/>)을 본 뒤 허브로 간다. [소개]로 다시 본다.
    /// </summary>
    public sealed class MainMenuScreen : MonoBehaviour
    {
        private Text _statsText;
        private Text _bestScoreText;
        private Text _titleGlow;

        private float _elapsed;

        // 31일차: 설정 · 종료 / 32일차: 조작법 대신 업적
        private SettingsPanel _settings;
        private AchievementPanel _achievements;
        private UiButton _quit;
        private float _quitConfirm;

        // 34일차: 저장 칸
        private SaveSlotPanel _slots;
        private UiButton _continue;

        // 35일차: 소개 화면
        private IntroSequence _intro;

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;

            if (_quitConfirm > 0f)
            {
                _quitConfirm -= Time.unscaledDeltaTime;

                if (_quitConfirm <= 0f)
                {
                    _quit.SetText("종  료");
                }
            }

            if (_titleGlow == null)
            {
                return;
            }

            // 제목 뒤 그림자를 아주 느리게 숨쉬게 해서 정적인 화면에 생기를 준다.
            float pulse =
                0.18f +
                (Mathf.Sin(
                     _elapsed * 1.4f) *
                 0.5f +
                 0.5f) *
                0.22f;

            Color color =
                UiTheme.Accent;

            color.a = pulse;

            _titleGlow.color = color;
        }

        /// <summary>설정 · 업적 · 종료 줄의 높이다. 34일차에 시작 버튼이 둘이 되어 아래로 내렸다.</summary>
        private const float MenuRowY = -96f;

        /// <summary>
        /// 이어하기 · 처음부터 버튼이다 (34일차).
        /// 이어하기는 저장된 칸이 없으면 누를 수 없다.
        /// </summary>
        private void BuildStartButtons(
            Transform canvas)
        {
            _slots = SaveSlotPanel.Create(canvas, 60);
            _slots.SlotChosen += HandleSlotChosen;

            _intro = IntroSequence.Create(canvas, 80);

            _continue =
                UiFactory.CreateButton(
                    canvas,
                    "ContinueButton",
                    "이어하기",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(_continue.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 36f), new Vector2(340f, 60f));

            _continue.Button.onClick.AddListener(
                () => OpenSlots(SaveSlotMode.Load));

            UiButton newGame =
                UiFactory.CreateButton(
                    canvas,
                    "NewGameButton",
                    "처음부터",
                    UiTheme.FontSubheading);

            UiFactory.Place(newGame.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(340f, 48f));

            newGame.Button.onClick.AddListener(
                () => OpenSlots(SaveSlotMode.NewGame));
        }

        private void OpenSlots(
            SaveSlotMode mode)
        {
            GameSession session = GameSession.Instance;

            if (session == null)
            {
                return;
            }

            GameAudio.Play(GameSfx.UiTick);
            _slots.Open(mode, session.GetSlotSummaries());
        }

        private void HandleSlotChosen(
            SaveSlotMode mode,
            int slot)
        {
            GameSession session = GameSession.Instance;

            if (session == null)
            {
                return;
            }

            bool ready =
                mode == SaveSlotMode.Load
                    ? session.LoadSlot(slot)
                    : session.NewGame(slot);

            if (!ready)
            {
                Debug.LogWarning($"[MainMenu] {SaveSlotLogic.GetSlotLabel(slot)}을 준비하지 못했습니다.");
                Refresh();

                return;
            }

            // 35일차: 새 게임은 소개를 보고 들어간다. 이어하기는 바로 허브로.
            if (mode == SaveSlotMode.NewGame)
            {
                Refresh();
                _intro.Play(() => GameSession.Instance?.GoTo(SceneDestination.Hub));

                return;
            }

            session.GoTo(SceneDestination.Hub);
        }

        /// <summary>설정 · 업적 · 소개 · 종료 버튼의 가로 위치다 (35일차에 넷이 되었다).</summary>
        private static readonly float[] MenuRowX = { -168f, -56f, 56f, 168f };

        private const float MenuButtonWidth = 104f;

        /// <summary>시작 버튼 아래 줄이다. 설정 · 업적 · 소개 · 종료.</summary>
        private void BuildMenuRow(
            Transform canvas)
        {
            _settings = SettingsPanel.Create(canvas, 60);

            // 32일차: 허브와 같은 업적 창을 메인 메뉴에서도 연다.
            _achievements = gameObject.AddComponent<AchievementPanel>();
            _achievements.Build(canvas);

            UiButton settings =
                UiFactory.CreateButton(canvas, "SettingsButton", "설  정", UiTheme.FontBody);

            UiFactory.Place(settings.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(MenuRowX[0], MenuRowY), new Vector2(MenuButtonWidth, 44f));

            settings.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _settings.Open();
                });

            UiButton achievements =
                UiFactory.CreateButton(canvas, "AchievementsButton", "업  적", UiTheme.FontBody);

            UiFactory.Place(achievements.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(MenuRowX[1], MenuRowY), new Vector2(MenuButtonWidth, 44f));

            achievements.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _achievements.Open(
                        GameSession.Instance == null
                            ? null
                            : GameSession.Instance.Save);
                });

            // 35일차: 소개 다시 보기
            UiButton intro =
                UiFactory.CreateButton(canvas, "IntroButton", "소  개", UiTheme.FontBody);

            UiFactory.Place(intro.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(MenuRowX[2], MenuRowY), new Vector2(MenuButtonWidth, 44f));

            intro.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _intro.Play(null);
                });

            _quit =
                UiFactory.CreateButton(canvas, "QuitButton", "종  료", UiTheme.FontBody);

            UiFactory.Place(_quit.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(MenuRowX[3], MenuRowY), new Vector2(MenuButtonWidth, 44f));

            _quit.Button.onClick.AddListener(Quit);
        }

        /// <summary>실수로 누르지 않게 두 번 눌러야 끈다. 끄기 전에 저장한다.</summary>
        private void Quit()
        {
            if (_quitConfirm <= 0f)
            {
                _quitConfirm = PauseMenuLogic.AbandonConfirmSeconds;
                _quit.SetText("한 번 더");
                GameAudio.Play(GameSfx.UiTick);

                return;
            }

            GameSession.Instance?.WriteSave();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "MainMenuCanvas",
                    0,
                    transform);

            // 배경 ------------------------------------------------------
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
                    0.30f));

            // 화면 아래쪽을 보라색으로 은은하게 물들이는 띠
            Image glowBand =
                UiFactory.CreateImage(
                    canvas.transform,
                    "GlowBand",
                    new Color(
                        UiTheme.Accent.r,
                        UiTheme.Accent.g,
                        UiTheme.Accent.b,
                        0.07f));

            RectTransform glowRect =
                glowBand.rectTransform;

            glowRect.anchorMin = new Vector2(0f, 0f);
            glowRect.anchorMax = new Vector2(1f, 0.42f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;

            // 제목 ------------------------------------------------------
            RectTransform titleGroup =
                UiFactory.CreateRect(
                    canvas.transform,
                    "TitleGroup");

            UiFactory.Place(
                titleGroup,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 210f),
                new Vector2(900f, 150f));

            _titleGlow =
                UiFactory.CreateText(
                    titleGroup,
                    "TitleGlow",
                    "프로젝트 θ",
                    UiTheme.FontTitle + 6,
                    UiTheme.Accent,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                _titleGlow.rectTransform);

            Text title =
                UiFactory.CreateText(
                    titleGroup,
                    "Title",
                    "프로젝트 θ",
                    UiTheme.FontTitle,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                title.rectTransform);

            Text subtitle =
                UiFactory.CreateText(
                    canvas.transform,
                    "Subtitle",
                    "최면 · 군중 관리 · 쟁탈",
                    UiTheme.FontSubheading,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                subtitle.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 128f),
                new Vector2(900f, 30f));

            // 제목과 버튼을 가르는 얇은 선
            Image divider =
                UiFactory.CreateImage(
                    canvas.transform,
                    "Divider",
                    new Color(
                        UiTheme.AccentSoft.r,
                        UiTheme.AccentSoft.g,
                        UiTheme.AccentSoft.b,
                        0.55f));

            UiFactory.Place(
                divider.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 88f),
                new Vector2(320f, 2f));

            // 시작 버튼 (34일차: 이어하기 · 처음부터) --------------------
            BuildStartButtons(
                canvas.transform);

            // 31일차: 설정 · 조작법 · 종료 -------------------------------
            BuildMenuRow(
                canvas.transform);

            // 기록 ------------------------------------------------------
            RectTransform recordPanel =
                UiFactory.CreatePanel(
                    canvas.transform,
                    "RecordPanel",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            UiFactory.Place(
                recordPanel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -190f),
                new Vector2(560f, 104f));

            _statsText =
                UiFactory.CreateText(
                    recordPanel,
                    "Stats",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _statsText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f),
                new Vector2(540f, 26f));

            _bestScoreText =
                UiFactory.CreateText(
                    recordPanel,
                    "BestScore",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _bestScoreText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -16f),
                new Vector2(540f, 30f));

            // 하단 표기 -------------------------------------------------
            Text footer =
                UiFactory.CreateText(
                    canvas.transform,
                    "Footer",
                    BuildInfo.GetFooter(Debug.isDebugBuild),
                    UiTheme.FontTiny,
                    UiTheme.TextDisabled,
                    TextAnchor.LowerCenter);

            UiFactory.Place(
                footer.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 18f),
                new Vector2(600f, 22f));

            Refresh();
        }

        private void Refresh()
        {
            GameSession session = GameSession.Instance;

            SaveSlotSummary[] summaries =
                session == null
                    ? new SaveSlotSummary[0]
                    : session.GetSlotSummaries();

            int latest =
                SaveSlotLogic.FindLatest(
                    summaries);

            // 34일차: 저장된 칸이 없으면 이어하기를 막고 기록 칸도 비운다.
            _continue.SetInteractable(latest >= 0);

            SaveData save =
                session == null ||
                latest < 0
                    ? null
                    : session.Save;

            if (_statsText != null)
            {
                _statsText.text =
                    save == null
                        ? "저장된 칸이 없습니다 · 처음부터 시작하세요"
                        : $"최근 {SaveSlotLogic.GetSlotLabel(latest)}   ·   플레이 {save.PlayCount}회   클리어 {save.ClearCount}회   최고 랭크 {save.BestRankLabel}";
            }

            if (_bestScoreText != null)
            {
                _bestScoreText.text =
                    save == null
                        ? string.Empty
                        : $"최고 점수 {save.BestScore:N0}";
            }
        }
    }
}
