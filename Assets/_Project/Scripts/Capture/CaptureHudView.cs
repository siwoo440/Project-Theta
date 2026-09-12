using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Capture
{
    /// <summary>
    /// 포획당했을 때 화면을 덮는 탈출 UI다.
    ///
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
    /// 포획 중에만 캔버스를 켜고, 평소에는 꺼 두어 그리기 비용을 없앤다.
    /// </summary>
    public sealed class CaptureHudView : MonoBehaviour
    {
        private PlayerCaptureController _controller;

        private Canvas _canvas;
        private RectTransform _overlayRect;
        private RectTransform _panel;
        private Text _expectedText;
        private Text _escapeText;
        private Text _damageText;
        private UiBar _escapeBar;

        public void Configure(
            PlayerCaptureController controller)
        {
            _controller =
                controller;
        }

        private void Start()
        {
            Build();

            SetVisible(
                false);
        }

        private void Update()
        {
            bool capturing =
                _controller != null &&
                _controller.IsCapturing;

            SetVisible(
                capturing);

            if (!capturing)
            {
                return;
            }

            Refresh();
        }

        private void SetVisible(
            bool value)
        {
            if (_canvas == null ||
                _canvas.gameObject.activeSelf == value)
            {
                return;
            }

            _canvas.gameObject.SetActive(
                value);
        }

        private void Refresh()
        {
            // 흔들림은 가로 위치로만 준다. 세로로 흔들면 글자가 읽기 어려워진다.
            if (_overlayRect != null)
            {
                _overlayRect.anchoredPosition =
                    new Vector2(
                        _controller.VisualJoltOffsetX,
                        _overlayRect.anchoredPosition.y);
            }

            _expectedText.text =
                $"다음 입력   {_controller.ExpectedInputLabel}";

            float escape =
                _controller.EscapeNormalized;

            _escapeBar.SetValue(
                escape);

            _escapeText.text =
                $"탈출 게이지  {escape * 100f:0}%";

            _damageText.text =
                $"피해 진행  {_controller.DamageTaken} / {_controller.DamageCap}";

            // 피해가 한계에 가까워지면 붉게 경고한다.
            _damageText.color =
                _controller.DamageCap > 0 &&
                _controller.DamageTaken >=
                _controller.DamageCap * 0.6f
                    ? UiTheme.Danger
                    : UiTheme.TextMuted;
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            // 결과창(200)보다는 아래, 일반 HUD(50)보다는 위에 둔다.
            _canvas =
                UiFactory.CreateCanvas(
                    "CaptureHudCanvas",
                    120,
                    transform);

            Image dim =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Dim",
                    new Color(0f, 0f, 0f, 0.42f));

            UiFactory.Stretch(
                dim.rectTransform);

            BuildOverlayImage();
            BuildEscapePanel();
        }

        private void BuildOverlayImage()
        {
            Texture2D texture =
                Resources.Load<Texture2D>(
                    "Capture/PlayerGrabOverlay");

            if (texture == null)
            {
                return;
            }

            Image overlay =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Overlay",
                    Color.white);

            overlay.sprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        texture.width,
                        texture.height),
                    new Vector2(0.5f, 0.5f));

            overlay.preserveAspect = true;

            _overlayRect =
                overlay.rectTransform;

            // 기준 해상도 기준으로 세로 절반 정도를 차지하게 둔다.
            float height = 460f;

            float width =
                height *
                texture.width /
                Mathf.Max(
                    1,
                    texture.height);

            UiFactory.Place(
                _overlayRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 40f),
                new Vector2(width, height));
        }

        private void BuildEscapePanel()
        {
            _panel =
                UiFactory.CreatePanel(
                    _canvas.transform,
                    "EscapePanel",
                    UiTheme.PanelFill,
                    UiTheme.Danger);

            UiFactory.Place(
                _panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -20f),
                new Vector2(700f, 268f));

            Text title =
                UiFactory.CreateText(
                    _panel,
                    "Title",
                    "붙잡힘 - 교대로 클릭해 탈출",
                    UiTheme.FontHeading,
                    UiTheme.Danger,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                title.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(660f, 34f));

            _expectedText =
                UiFactory.CreateText(
                    _panel,
                    "Expected",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _expectedText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -72f),
                new Vector2(660f, 28f));

            _escapeBar =
                UiFactory.CreateBar(
                    _panel,
                    "EscapeBar",
                    new Color(0.93f, 0.68f, 0.21f, 1f),
                    UiTheme.TrackFill);

            UiFactory.Place(
                _escapeBar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -112f),
                new Vector2(620f, 28f));

            _escapeText =
                UiFactory.CreateText(
                    _panel,
                    "EscapeText",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _escapeText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -156f),
                new Vector2(660f, 24f));

            _damageText =
                UiFactory.CreateText(
                    _panel,
                    "DamageText",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _damageText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -186f),
                new Vector2(660f, 24f));

            Text order =
                UiFactory.CreateText(
                    _panel,
                    "Order",
                    "입력 순서   좌클릭 → 우클릭 → 좌클릭 → 우클릭",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                order.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(660f, 22f));
        }
    }
}
