using UnityEngine;

namespace ProjectTheta.UI.Framework
{
    /// <summary>
    /// 화면 전체가 공유하는 색·크기 규칙이다.
    ///
    /// 15일차에 IMGUI를 Canvas(uGUI)로 옮기면서, 화면마다 제각각이던
    /// 색과 여백을 한곳으로 모았다. 톤을 바꾸고 싶으면 여기만 고치면 된다.
    /// </summary>
    public static class UiTheme
    {
        // ── 배경 계열 ────────────────────────────────
        public static readonly Color Backdrop =
            new Color(0.055f, 0.043f, 0.086f, 1f);

        public static readonly Color PanelFill =
            new Color(0.105f, 0.082f, 0.145f, 0.96f);

        public static readonly Color PanelEdge =
            new Color(0.42f, 0.26f, 0.68f, 0.85f);

        public static readonly Color RowFill =
            new Color(0.145f, 0.115f, 0.200f, 0.92f);

        public static readonly Color TrackFill =
            new Color(0.070f, 0.055f, 0.100f, 0.94f);

        // ── 글자 계열 ────────────────────────────────
        public static readonly Color TextPrimary =
            new Color(0.94f, 0.92f, 0.98f, 1f);

        public static readonly Color TextMuted =
            new Color(0.62f, 0.58f, 0.72f, 1f);

        public static readonly Color TextDisabled =
            new Color(0.42f, 0.39f, 0.50f, 1f);

        // ── 강조 계열 ────────────────────────────────
        /// <summary>최면·정기를 상징하는 보라색이다.</summary>
        public static readonly Color Accent =
            new Color(0.70f, 0.30f, 1.00f, 1f);

        public static readonly Color AccentSoft =
            new Color(0.52f, 0.34f, 0.82f, 1f);

        /// <summary>계약 정기·보상을 상징하는 금색이다.</summary>
        public static readonly Color Gold =
            new Color(0.98f, 0.80f, 0.36f, 1f);

        public static readonly Color Focus =
            new Color(0.30f, 0.78f, 0.96f, 1f);

        public static readonly Color FocusExhausted =
            new Color(0.62f, 0.55f, 0.24f, 1f);

        public static readonly Color Health =
            new Color(0.88f, 0.20f, 0.27f, 1f);

        public static readonly Color Danger =
            new Color(0.95f, 0.32f, 0.34f, 1f);

        public static readonly Color Positive =
            new Color(0.42f, 0.88f, 0.56f, 1f);

        // ── 버튼 계열 ────────────────────────────────
        public static readonly Color ButtonNormal =
            new Color(0.22f, 0.16f, 0.32f, 1f);

        public static readonly Color ButtonHighlight =
            new Color(0.34f, 0.24f, 0.50f, 1f);

        public static readonly Color ButtonPressed =
            new Color(0.46f, 0.30f, 0.70f, 1f);

        public static readonly Color ButtonDisabled =
            new Color(0.16f, 0.14f, 0.19f, 1f);

        public static readonly Color PrimaryButtonNormal =
            new Color(0.44f, 0.20f, 0.72f, 1f);

        public static readonly Color PrimaryButtonHighlight =
            new Color(0.56f, 0.28f, 0.88f, 1f);

        // ── 글자 크기 ────────────────────────────────
        public const int FontTitle = 46;
        public const int FontHeading = 24;
        public const int FontSubheading = 19;
        public const int FontBody = 16;
        public const int FontSmall = 14;
        public const int FontTiny = 12;

        // ── 여백 ─────────────────────────────────────
        public const float PaddingLarge = 24f;
        public const float PaddingMedium = 14f;
        public const float PaddingSmall = 8f;

        /// <summary>모든 화면이 이 해상도를 기준으로 배치된 뒤 실제 화면에 맞게 늘어난다.</summary>
        public static readonly Vector2 ReferenceResolution =
            new Vector2(1920f, 1080f);
    }
}
