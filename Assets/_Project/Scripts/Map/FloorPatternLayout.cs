using System.Collections.Generic;
using UnityEngine;

namespace ProjectTheta.Map
{
    /// <summary>바닥 무늬를 이루는 사각형 하나다.</summary>
    public struct PatternRect
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public Color Color;

        public PatternRect(
            float x,
            float y,
            float width,
            float height,
            Color color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Color = color;
        }
    }

    /// <summary>
    /// 바닥 무늬의 사각형 목록이다 (28일차).
    ///
    /// 27일차에는 줄눈 · 모래 알갱이 · LED 칸을 하나하나 오브젝트로 만들어, 한 층에 200개가 넘게 생겼다.
    /// 이제 목록만 만들고 <see cref="FloorPatternBaker"/>가 텍스처 한 장에 구워 오브젝트 1개로 깐다.
    /// 목록은 화면 없이 테스트할 수 있다.
    /// </summary>
    public static class FloorPatternLayout
    {
        /// <summary>구울 영역이다. 바닥 전체(앞 테두리 포함)를 덮는다.</summary>
        public const float AreaMinX = -MapBaseLayers.Width * 0.5f;
        public const float AreaMaxX = MapBaseLayers.Width * 0.5f;
        public const float AreaMinY = -7.6f;
        public const float AreaMaxY = 0.7f;

        public static float AreaCenterY =>
            (AreaMinY + AreaMaxY) * 0.5f;

        public static float AreaHeight =>
            AreaMaxY - AreaMinY;

        public static List<PatternRect> Build(
            MapTheme theme,
            int floor)
        {
            List<PatternRect> result = new List<PatternRect>();

            if (theme == null)
            {
                return result;
            }

            switch (theme.Pattern)
            {
                case FloorPattern.Carpet:
                    Grid(result, theme.FloorLine, 2f, 1f, 0.03f);
                    break;

                case FloorPattern.Tile:
                    Grid(result, theme.FloorLine, 1f, 1f, 0.04f);
                    break;

                case FloorPattern.Marble:
                    Grid(result, theme.FloorLine, 3f, 2f, 0.03f);
                    Sheen(result);
                    break;

                case FloorPattern.Rubber:
                    Grid(result, theme.FloorLine, 1.5f, 1.5f, 0.05f);
                    Speckle(result, new Color(0.40f, 0.40f, 0.44f, 0.5f), 90, floor);
                    break;

                case FloorPattern.Sand:
                    Speckle(result, theme.FloorLine, 140, floor);
                    break;

                case FloorPattern.Paving:
                    Bricks(result, theme.FloorLine);
                    break;

                case FloorPattern.LedGrid:
                    LedGrid(result, theme);
                    break;
            }

            AddLocationExtras(result, theme);

            return result;
        }

        /// <summary>층마다 무늬가 달라지는 무늬인지다. 같으면 한 장을 여러 층이 나눠 쓴다.</summary>
        public static bool DependsOnFloor(
            FloorPattern pattern)
        {
            return pattern == FloorPattern.Rubber ||
                   pattern == FloorPattern.Sand;
        }

        /// <summary>장소 고유의 바닥 표시다. 점이 많아 오브젝트 대신 함께 굽는다.</summary>
        private static void AddLocationExtras(
            List<PatternRect> result,
            MapTheme theme)
        {
            if (theme.Location == Stage.Locations.LocationId.SubwayStation)
            {
                // 노란 점자 블록 · 승강장 경고선.
                result.Add(new PatternRect(0f, TactileY, MapBaseLayers.Width, 0.35f, new Color(0.95f, 0.80f, 0.20f)));

                for (float x = -18f; x <= 18f; x += 0.35f)
                {
                    result.Add(new PatternRect(x, TactileY, 0.08f, 0.08f, new Color(0.80f, 0.65f, 0.10f)));
                }
            }
        }

        public const float TactileY = 0.3f;

        private static void Grid(
            List<PatternRect> result,
            Color line,
            float stepX,
            float stepY,
            float thickness)
        {
            for (float x = -18f; x <= 18f; x += stepX)
            {
                result.Add(new PatternRect(x, -3.35f, thickness, 7.9f, line));
            }

            for (float y = -0.4f; y > MapBaseLayers.FloorBottom; y -= stepY)
            {
                result.Add(new PatternRect(0f, y, MapBaseLayers.Width, thickness, line));
            }
        }

        private static void Sheen(
            List<PatternRect> result)
        {
            // 광택 바닥 — 천장 조명이 비친 옅은 띠.
            for (int i = 0; i < 5; i++)
            {
                result.Add(new PatternRect(-13f + i * 6.5f, -1.2f, 2.2f, 1.6f, new Color(1f, 1f, 1f, 0.12f)));
            }
        }

        private static void Speckle(
            List<PatternRect> result,
            Color color,
            int count,
            int floor)
        {
            for (int i = 0; i < count; i++)
            {
                float x = -18f + MapPainter.Hash(i, 11 + floor) * 36f;
                float y = MapBaseLayers.FloorBottom + 0.3f + MapPainter.Hash(i, 23 + floor) * 7.6f;
                float size = 0.05f + MapPainter.Hash(i, 37) * 0.07f;

                result.Add(new PatternRect(x, y, size, size, color));
            }
        }

        private static void Bricks(
            List<PatternRect> result,
            Color line)
        {
            int row = 0;

            for (float y = -0.2f; y > MapBaseLayers.FloorBottom; y -= 0.8f)
            {
                result.Add(new PatternRect(0f, y, MapBaseLayers.Width, 0.05f, line));

                float offset = row % 2 == 0 ? 0f : 0.8f;

                for (float x = -18f + offset; x <= 18f; x += 1.6f)
                {
                    result.Add(new PatternRect(x, y - 0.4f, 0.05f, 0.8f, line));
                }

                row++;
            }

            // 비 온 뒤 물웅덩이.
            result.Add(new PatternRect(-7f, -4.6f, 1.8f, 0.35f, new Color(0.25f, 0.30f, 0.50f, 0.55f)));
            result.Add(new PatternRect(7.5f, -2.8f, 1.2f, 0.28f, new Color(0.25f, 0.30f, 0.50f, 0.55f)));
        }

        private static void LedGrid(
            List<PatternRect> result,
            MapTheme theme)
        {
            // 어두운 바닥에 옅은 LED 칸. 드롭 빛(클럽 규칙)이 이 위에 번쩍인다.
            for (float y = -0.2f; y > -5.4f; y -= 1f)
            {
                for (float x = -17.5f; x <= 17.5f; x += 1f)
                {
                    bool lit = ((int)(x + 18f) + (int)(-y)) % 2 == 0;

                    result.Add(
                        new PatternRect(
                            x,
                            y - 0.5f,
                            0.92f,
                            0.92f,
                            lit
                                ? new Color(theme.Accent.r, theme.Accent.g, theme.Accent.b, 0.07f)
                                : new Color(theme.Divider.r, theme.Divider.g, theme.Divider.b, 0.05f)));
                }
            }
        }
    }
}
