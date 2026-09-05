using UnityEngine;

namespace ProjectTheta.Capture
{
    public sealed class CaptureHudView : MonoBehaviour
    {
        private PlayerCaptureController _controller;
        private Texture2D _overlayTexture;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;

        public void Configure(
            PlayerCaptureController controller)
        {
            _controller =
                controller;
        }

        private void Awake()
        {
            _overlayTexture =
                Resources.Load<Texture2D>(
                    "Capture/PlayerGrabOverlay");
        }

        private void OnGUI()
        {
            if (_controller == null ||
                !_controller.IsCapturing)
            {
                return;
            }

            EnsureStyles();

            DrawBackdrop();
            DrawOverlayImage();
            DrawEscapeHud();
        }

        private void DrawBackdrop()
        {
            Color previousColor =
                GUI.color;

            GUI.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.36f);

            GUI.DrawTexture(
                new Rect(
                    0f,
                    0f,
                    Screen.width,
                    Screen.height),
                Texture2D.whiteTexture);

            GUI.color =
                previousColor;
        }

        private void DrawOverlayImage()
        {
            if (_overlayTexture == null)
            {
                return;
            }

            float maxWidth =
                Screen.width *
                0.48f;

            float maxHeight =
                Screen.height *
                0.48f;

            float widthRatio =
                maxWidth /
                _overlayTexture.width;

            float heightRatio =
                maxHeight /
                _overlayTexture.height;

            float scale =
                Mathf.Min(
                    widthRatio,
                    heightRatio);

            float drawWidth =
                _overlayTexture.width *
                scale;

            float drawHeight =
                _overlayTexture.height *
                scale;

            float x =
                ((Screen.width -
                  drawWidth) *
                 0.5f) +
                _controller.VisualJoltOffsetX;

            float y =
                (Screen.height *
                 0.5f) -
                drawHeight -
                22f;

            GUI.DrawTexture(
                new Rect(
                    x,
                    y,
                    drawWidth,
                    drawHeight),
                _overlayTexture,
                ScaleMode.ScaleToFit,
                true);
        }

        private void DrawEscapeHud()
        {
            float panelWidth =
                Mathf.Min(
                    460f,
                    Screen.width *
                    0.54f);

            float panelX =
                (Screen.width -
                 panelWidth) *
                0.5f;

            float panelY =
                Screen.height *
                0.63f;

            GUI.Label(
                new Rect(
                    panelX,
                    panelY,
                    panelWidth,
                    32f),
                "붙잡힘 상태 - 교대로 클릭해 탈출",
                _titleStyle);

            GUI.Label(
                new Rect(
                    panelX,
                    panelY + 34f,
                    panelWidth,
                    24f),
                $"다음 입력: {_controller.ExpectedInputLabel}",
                _labelStyle);

            DrawProgressBar(
                new Rect(
                    panelX,
                    panelY + 68f,
                    panelWidth,
                    24f),
                _controller.EscapeNormalized,
                new Color(
                    0.93f,
                    0.68f,
                    0.21f,
                    1f),
                new Color(
                    0.19f,
                    0.13f,
                    0.07f,
                    0.94f));

            GUI.Label(
                new Rect(
                    panelX,
                    panelY + 95f,
                    panelWidth,
                    22f),
                $"탈출 게이지: {_controller.EscapeNormalized * 100f:0}%",
                _labelStyle);

            GUI.Label(
                new Rect(
                    panelX,
                    panelY + 119f,
                    panelWidth,
                    22f),
                $"피해 진행: {_controller.DamageTaken} / {_controller.DamageCap}",
                _labelStyle);

            GUI.Label(
                new Rect(
                    panelX,
                    panelY + 143f,
                    panelWidth,
                    22f),
                "입력 순서: 좌클릭 → 우클릭 → 좌클릭 → 우클릭",
                _labelStyle);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 22,
                    fontStyle =
                        FontStyle.Bold
                };

            _labelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 15,
                    fontStyle =
                        FontStyle.Bold
                };
        }

        private static void DrawProgressBar(
            Rect rect,
            float normalized,
            Color fillColor,
            Color backgroundColor)
        {
            float value =
                Mathf.Clamp01(
                    normalized);

            Color previousColor =
                GUI.color;

            GUI.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.82f);

            GUI.DrawTexture(
                new Rect(
                    rect.x - 2f,
                    rect.y - 2f,
                    rect.width + 4f,
                    rect.height + 4f),
                Texture2D.whiteTexture);

            GUI.color =
                backgroundColor;

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture);

            GUI.color =
                fillColor;

            GUI.DrawTexture(
                new Rect(
                    rect.x,
                    rect.y,
                    rect.width *
                    value,
                    rect.height),
                Texture2D.whiteTexture);

            GUI.color =
                previousColor;
        }
    }
}
