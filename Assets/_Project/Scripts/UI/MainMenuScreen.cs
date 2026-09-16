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
    /// </summary>
    public sealed class MainMenuScreen : MonoBehaviour
    {
        private Text _statsText;
        private Text _bestScoreText;
        private Text _titleGlow;

        private float _elapsed;

        // 31일차: 설정 · 조작법 · 종료
        private SettingsPanel _settings;
        private ControlsPanel _controls;
        private UiButton _quit;
        private float _quitConfirm;

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

        private void BuildMenuRow(
            Transform canvas)
        {
            _settings = SettingsPanel.Create(canvas, 60);
            _controls = ControlsPanel.Create(canvas, 60);

            UiButton settings =
                UiFactory.CreateButton(canvas, "SettingsButton", "설  정", UiTheme.FontBody);

            UiFactory.Place(settings.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-115f, -62f), new Vector2(105f, 44f));

            settings.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _settings.Open();
                });

            UiButton controls =
                UiFactory.CreateButton(canvas, "ControlsButton", "조작법", UiTheme.FontBody);

            UiFactory.Place(controls.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -62f), new Vector2(105f, 44f));

            controls.Button.onClick.AddListener(
                () =>
                {
                    GameAudio.Play(GameSfx.UiTick);
                    _controls.Open();
                });

            _quit =
                UiFactory.CreateButton(canvas, "QuitButton", "종  료", UiTheme.FontBody);

            UiFactory.Place(_quit.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(115f, -62f), new Vector2(105f, 44f));

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

            // 시작 버튼 -------------------------------------------------
            UiButton start =
                UiFactory.CreateButton(
                    canvas.transform,
                    "StartButton",
                    "시  작",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(
                start.Background.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f),
                new Vector2(340f, 64f));

            start.Button.onClick.AddListener(
                () =>
                {
                    GameSession.Instance?.GoTo(
                        SceneDestination.Hub);
                });

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
                new Vector2(0f, -150f),
                new Vector2(460f, 104f));

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
                new Vector2(440f, 26f));

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
                new Vector2(440f, 30f));

            // 하단 표기 -------------------------------------------------
            Text footer =
                UiFactory.CreateText(
                    canvas.transform,
                    "Footer",
                    "31일차 · Esc 일시정지 · 설정",
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
            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            if (_statsText != null)
            {
                _statsText.text =
                    save == null
                        ? "저장 정보 없음"
                        : $"플레이 {save.PlayCount}회      클리어 {save.ClearCount}회      최고 랭크 {save.BestRankLabel}";
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
