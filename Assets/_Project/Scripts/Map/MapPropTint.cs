using UnityEngine;

namespace ProjectTheta.Map
{
    /// <summary>
    /// 장소 규칙 소품(노점 · 망루 · 게이트 · 탕비실 · DJ 부스 · 바 …) 색을 맵 톤에 맞춘다 (28일차).
    ///
    ///   1. 원래 색을 장소 강조색 쪽으로 조금 섞는다.  → 맵과 같은 분위기
    ///   2. 바닥과 밝기 차이가 작으면 밝히거나 어둡게 한다. → 바닥에 묻히지 않음
    ///
    /// 경고등 · 어둠 · 보스 효과처럼 "의미가 있는 색"에는 쓰지 않는다. 구조물에만 쓴다.
    /// </summary>
    public static class MapPropTint
    {
        public const float AccentBlend = 0.18f;

        public const float MinFloorGap = 0.12f;

        private const float ContrastStep = 0.08f;
        private const int MaxContrastSteps = 12;

        public static Color Apply(
            Color color,
            MapTheme theme)
        {
            if (theme == null)
            {
                return color;
            }

            float r = Mix(color.r, theme.Accent.r);
            float g = Mix(color.g, theme.Accent.g);
            float b = Mix(color.b, theme.Accent.b);

            float floor = MapLayoutRules.Luminance(theme.Floor.r, theme.Floor.g, theme.Floor.b);

            // 어두운 바닥이면 밝게, 밝은 바닥이면 어둡게 민다.
            bool lighten = floor < 0.5f;

            for (int i = 0;
                 i < MaxContrastSteps &&
                 System.Math.Abs(MapLayoutRules.Luminance(r, g, b) - floor) < MinFloorGap;
                 i++)
            {
                float target = lighten ? 1f : 0f;

                r += (target - r) * ContrastStep * 2f;
                g += (target - g) * ContrastStep * 2f;
                b += (target - b) * ContrastStep * 2f;
            }

            return new Color(r, g, b, color.a);
        }

        private static float Mix(
            float from,
            float to)
        {
            return from + (to - from) * AccentBlend;
        }
    }
}
