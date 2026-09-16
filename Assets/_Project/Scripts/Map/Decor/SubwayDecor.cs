using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 지하철 환승역 — 타일과 스크린도어 (27일차).
    /// 흰 타일 벽 · 노선 띠 · 노선도 · 광고판 · 스크린도어 · 역명판, 회색 타일 · 노란 점자 블록, 역 벤치 · 승차권 발매기.
    /// 개찰구 · 승강장 선 · 전광판(방송실)은 지하철 규칙이 그린다.
    /// </summary>
    public static class SubwayDecor
    {
        public static readonly float[] ScreenDoorX =
        {
            -10.4f, -7.8f, -5.2f, -2.6f, 2.6f, 5.2f, 7.8f, 10.4f
        };

        public const float ScreenDoorHalfWidth = 1.05f;

        public static MapFeature[] LowWall
        {
            get
            {
                MapFeature[] result = new MapFeature[ScreenDoorX.Length];

                for (int i = 0; i < ScreenDoorX.Length; i++)
                {
                    result[i] = new MapFeature(ScreenDoorX[i], ScreenDoorHalfWidth);
                }

                return result;
            }
        }

        public static void Draw(
            MapPainter p)
        {
            // 벽 타일 줄눈.
            for (float x = -18f; x <= 18f; x += 0.8f)
            {
                p.Rect($"WallTileX_{x:0.#}", x, 2.8f, 0.03f, 3.1f, new Color(0.78f, 0.80f, 0.80f), -118);
            }

            for (float y = 1.4f; y < 4.3f; y += 0.5f)
            {
                p.Rect($"WallTileY_{y:0.#}", 0f, y, MapBaseLayers.Width, 0.03f, new Color(0.78f, 0.80f, 0.80f), -118);
            }

            // 노선 색 띠 · 역명판.
            Color line = p.Floor == 0
                ? new Color(0.20f, 0.45f, 0.85f)
                : new Color(0.25f, 0.70f, 0.40f);

            p.Rect("LineStripe", 0f, 3.75f, MapBaseLayers.Width, 0.22f, line, -105);
            p.Rect("StationSignPlate", -0.2f, 4.2f, 3.4f, 0.5f, new Color(0.15f, 0.17f, 0.20f), -45);
            p.Label("StationSign", p.Floor == 0 ? "시티 환승역 · 1호선" : "시티 환승역 · 2호선", -0.2f, 4.2f, 18, new Color(1f, 1f, 1f));

            // 스크린도어.
            for (int i = 0; i < ScreenDoorX.Length; i++)
            {
                float x = ScreenDoorX[i];

                p.Rect($"ScreenDoorFrame_{i}", x, 2.0f, ScreenDoorHalfWidth * 2f, 2.5f, new Color(0.55f, 0.58f, 0.62f), -62);
                p.Rect($"ScreenDoorGlassL_{i}", x - 0.48f, 1.95f, 0.9f, 2.2f, new Color(0.60f, 0.75f, 0.82f, 0.8f), -61);
                p.Rect($"ScreenDoorGlassR_{i}", x + 0.48f, 1.95f, 0.9f, 2.2f, new Color(0.60f, 0.75f, 0.82f, 0.8f), -61);
                p.Rect($"ScreenDoorTop_{i}", x, 3.1f, ScreenDoorHalfWidth * 2f, 0.22f, line, -60);
            }

            // 노선도 · 광고판 (높은 벽, 스크린도어 사이).
            p.Rect("RouteMap", 0f, 2.55f, 1.0f, 0.7f, new Color(0.95f, 0.95f, 0.95f), -58);

            for (int i = 0; i < 4; i++)
            {
                p.Rect($"RouteLine_{i}", 0f, 2.35f + i * 0.13f, 0.8f, 0.04f,
                    Color.HSVToRGB(i * 0.22f, 0.7f, 0.85f), -57);
            }

            p.Rect("AdBoard", 12.0f, 2.9f, 0.8f, 1.1f, new Color(0.95f, 0.75f, 0.35f), -58);
            p.Label("AdText", "SALE", 12.0f, 2.9f, 14, new Color(0.25f, 0.18f, 0.10f));

            // 노란 점자 블록은 바닥 무늬와 함께 구워 깐다 (28일차, FloorPatternLayout).

            MapBaseLayers.CeilingLights(p, new Color(0.45f, 0.47f, 0.50f), new Color(0.92f, 0.96f, 1.00f), false);

            // 기존 벤치 · 자판기 자리 — 역 벤치 · 승차권 발매기.
            p.Prop("StationBench", 5.2f, -0.95f, 2.8f, 0.3f, new Color(0.60f, 0.62f, 0.66f));
            p.Prop("StationBenchLegs", 5.2f, -1.3f, 2.4f, 0.35f, new Color(0.35f, 0.36f, 0.38f), -1);
            p.Prop("TicketMachine", -10.2f, 0.12f, 1.15f, 1.45f, new Color(0.25f, 0.45f, 0.75f));
            p.Prop("TicketScreen", -10.2f, 0.45f, 0.75f, 0.45f, new Color(0.70f, 0.90f, 0.98f), 2);
        }
    }
}
