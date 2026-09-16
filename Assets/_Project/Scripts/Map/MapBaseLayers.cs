using UnityEngine;

namespace ProjectTheta.Map
{
    /// <summary>
    /// 모든 장소가 공유하는 한 층의 바탕이다 (27일차).
    ///
    ///   y  4.6 ┬ 천장 (하늘 장소는 없음)
    ///          │ 벽 · 하늘 배경
    ///   y  1.1 ┤ 걸레받이 · 경계선
    ///   y  0.7 ┼ 바닥선 ─ 여기서부터 캐릭터가 다니는 바닥
    ///          │ 바닥 · 바닥 무늬
    ///   y -7.5 ┴ 앞 테두리
    /// 학교 복도와 같은 자리 · 크기라 계단 · 이동 범위 판정은 그대로다.
    /// </summary>
    public static class MapBaseLayers
    {
        public const float Width = 38f;
        public const float FloorTop = 0.7f;
        public const float FloorBottom = -7.5f;
        public const float WallBottom = -0.55f;
        public const float WallTop = 4.55f;

        public const float BenchX = 5.2f;
        public const float BenchY = -0.95f;
        public const float VendingX = -10.2f;
        public const float VendingY = 0.12f;

        public static void Draw(
            MapPainter p,
            MapTheme theme)
        {
            DrawWall(p, theme);
            DrawFloor(p, theme);

            // 학교 복도의 벤치 · 자판기와 같은 자리 · 크기의 충돌체다. 모양은 장소마다 다르게 그린다.
            p.Collider("BenchBlocker", BenchX, BenchY, 2.8f, 0.42f);
            p.Collider("VendingBlocker", VendingX, VendingY, 1.15f, 1.45f);

            p.Rect("FrontBand", 0f, -7.55f, Width, 0.24f, theme.FrontBand, 2200);
        }

        private static void DrawWall(
            MapPainter p,
            MapTheme theme)
        {
            Sprite art = MapArtLibrary.TryGet(theme.Location, MapArtLayer.Background);

            if (art != null)
            {
                p.Art("BackgroundArt", art, 0f, (WallBottom + WallTop) * 0.5f, Width, WallTop - WallBottom, -130);

                return;
            }

            if (theme.OpenSky)
            {
                // 하늘은 벽 높이보다 조금 더 위까지 채워 층 사이 빈틈이 보이지 않게 한다.
                p.Gradient("Sky", 0f, 1.0f, WallTop + 0.4f, Width, theme.SkyBottom, theme.SkyTop, 6, -135);
                p.Rect("SkyBase", 0f, 0.35f, Width, 1.4f, theme.Wall, -134);
                p.Rect("Horizon", 0f, 1.02f, Width, 0.06f, theme.Divider, -126);

                return;
            }

            p.Rect("BackWall", 0f, 2.0f, Width, 5.1f, theme.Wall, -120);
            p.Rect("LowerWallPanel", 0f, 0.55f, Width, 1.1f, theme.LowerWall, -110);
            p.Rect("WallDivider", 0f, 1.08f, Width, 0.12f, theme.Divider, -100);
            p.Rect("Ceiling", 0f, 4.25f, Width, 0.7f, theme.Ceiling, -115);
        }

        private static void DrawFloor(
            MapPainter p,
            MapTheme theme)
        {
            Sprite art = MapArtLibrary.TryGet(theme.Location, MapArtLayer.Floor);

            if (art != null)
            {
                p.Art("FloorArt", art, 0f, (FloorTop + FloorBottom) * 0.5f, Width, FloorTop - FloorBottom, -90);

                return;
            }

            p.Rect("Floor", 0f, -3.40f, Width, 8.2f, theme.Floor, -90);
            p.Rect("FloorBackBand", 0f, 0.73f, Width, 0.18f, theme.FloorLine, -80);

            switch (theme.Pattern)
            {
                case FloorPattern.Carpet:
                    Grid(p, theme.FloorLine, 2f, 1f, 0.03f);
                    break;

                case FloorPattern.Tile:
                    Grid(p, theme.FloorLine, 1f, 1f, 0.04f);
                    break;

                case FloorPattern.Marble:
                    Grid(p, theme.FloorLine, 3f, 2f, 0.03f);
                    Sheen(p);
                    break;

                case FloorPattern.Rubber:
                    Grid(p, theme.FloorLine, 1.5f, 1.5f, 0.05f);
                    Speckle(p, new Color(0.40f, 0.40f, 0.44f, 0.5f), 90);
                    break;

                case FloorPattern.Sand:
                    Speckle(p, theme.FloorLine, 140);
                    break;

                case FloorPattern.Paving:
                    Bricks(p, theme.FloorLine);
                    break;

                case FloorPattern.LedGrid:
                    LedGrid(p, theme);
                    break;
            }
        }

        private static void Grid(
            MapPainter p,
            Color line,
            float stepX,
            float stepY,
            float thickness)
        {
            for (float x = -18f; x <= 18f; x += stepX)
            {
                p.Rect($"FloorSeamX_{x:0.#}", x, -3.35f, thickness, 7.9f, line, -70);
            }

            int row = 0;

            for (float y = -0.4f; y > FloorBottom; y -= stepY)
            {
                p.Rect($"FloorSeamY_{row++}", 0f, y, Width, thickness, line, -70);
            }
        }

        private static void Sheen(
            MapPainter p)
        {
            // 광택 바닥 — 천장 조명이 비친 옅은 띠.
            for (int i = 0; i < 5; i++)
            {
                p.Rect($"FloorSheen_{i}", -13f + i * 6.5f, -1.2f, 2.2f, 1.6f, new Color(1f, 1f, 1f, 0.12f), -69);
            }
        }

        private static void Speckle(
            MapPainter p,
            Color color,
            int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = -18f + MapPainter.Hash(i, 11 + p.Floor) * 36f;
                float y = FloorBottom + 0.3f + MapPainter.Hash(i, 23 + p.Floor) * 7.6f;
                float size = 0.05f + MapPainter.Hash(i, 37) * 0.07f;

                p.Rect($"Speck_{i}", x, y, size, size, color, -69);
            }
        }

        private static void Bricks(
            MapPainter p,
            Color line)
        {
            int row = 0;

            for (float y = -0.2f; y > FloorBottom; y -= 0.8f)
            {
                p.Rect($"BrickRow_{row}", 0f, y, Width, 0.05f, line, -70);

                float offset = row % 2 == 0 ? 0f : 0.8f;

                for (float x = -18f + offset; x <= 18f; x += 1.6f)
                {
                    p.Rect($"BrickJoint_{row}_{x:0.#}", x, y - 0.4f, 0.05f, 0.8f, line, -70);
                }

                row++;
            }

            // 비 온 뒤 물웅덩이.
            p.Rect("Puddle_A", -7f, -4.6f, 1.8f, 0.35f, new Color(0.25f, 0.30f, 0.50f, 0.55f), -68);
            p.Rect("Puddle_B", 7.5f, -2.8f, 1.2f, 0.28f, new Color(0.25f, 0.30f, 0.50f, 0.55f), -68);
        }

        private static void LedGrid(
            MapPainter p,
            MapTheme theme)
        {
            // 어두운 바닥에 옅은 LED 칸. 드롭 빛(클럽 규칙)이 이 위에 번쩍인다.
            int index = 0;

            for (float y = -0.2f; y > -5.4f; y -= 1f)
            {
                for (float x = -17.5f; x <= 17.5f; x += 1f)
                {
                    bool lit = ((int)(x + 18f) + (int)(-y)) % 2 == 0;

                    p.Rect(
                        $"Led_{index++}",
                        x,
                        y - 0.5f,
                        0.92f,
                        0.92f,
                        lit
                            ? new Color(theme.Accent.r, theme.Accent.g, theme.Accent.b, 0.07f)
                            : new Color(theme.Divider.r, theme.Divider.g, theme.Divider.b, 0.05f),
                        -69);
                }
            }
        }

        /// <summary>실내 장소 천장 조명이다.</summary>
        public static void CeilingLights(
            MapPainter p,
            Color housing,
            Color light,
            bool alternateOff)
        {
            float[] xs = { -13f, -6.5f, 0f, 6.5f, 13f };

            for (int i = 0; i < xs.Length; i++)
            {
                bool off = alternateOff && i % 2 == 1;

                p.Rect($"CeilingLightHousing_{i}", xs[i], 4.0f, 2.4f, 0.28f, housing, -10);
                p.Rect($"CeilingLight_{i}", xs[i], 3.94f, 2.1f, 0.14f, off ? new Color(0.30f, 0.30f, 0.32f) : light, -8);
            }
        }
    }
}
