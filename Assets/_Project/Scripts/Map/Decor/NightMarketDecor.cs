using UnityEngine;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 야시장 — 전구줄 아래 골목 (27일차).
    /// 밤하늘 · 달 · 건물 실루엣과 창 불빛 · 간판 · 전구줄 · 김, 보도블록 · 물웅덩이, 플라스틱 테이블 · 음식 수레.
    /// 노점 · 등불 빛은 야시장 규칙이 그리고, 전구줄 알맹이는 노점 자리에 맞춘다.
    /// </summary>
    public static class NightMarketDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(-6.0f, 0.5f),    // 박스 더미
            new MapFeature(0f, 0.5f),       // 드럼통 화로
            new MapFeature(6.0f, 0.5f),     // 박스 더미
            new MapFeature(11.6f, 0.4f)     // 입간판
        };

        public static void Draw(
            MapPainter p)
        {
            p.Rect("MoonGlow", -12.5f, 4.0f, 1.3f, 1.3f, new Color(1f, 0.95f, 0.80f, 0.25f), -133);
            p.Rect("Moon", -12.5f, 4.0f, 0.7f, 0.7f, new Color(1f, 0.97f, 0.85f), -132);

            // 건물 실루엣 · 창 불빛.
            for (int i = 0; i < 14; i++)
            {
                float x = -17.5f + i * 2.7f;
                float height = 1.6f + MapPainter.Hash(i, 3) * 2.4f;
                float width = 2.1f + MapPainter.Hash(i, 4) * 0.5f;

                p.Rect($"Building_{i}", x, 1.0f + height * 0.5f, width, height, new Color(0.10f, 0.09f, 0.16f), -130);

                for (int w = 0; w < 4; w++)
                {
                    if (MapPainter.Hash(i, 20 + w) < 0.45f)
                    {
                        continue;
                    }

                    p.Rect(
                        $"BuildingWindow_{i}_{w}",
                        x - 0.5f + (w % 2) * 1.0f,
                        1.4f + (w / 2) * 0.8f,
                        0.3f,
                        0.3f,
                        new Color(1f, 0.82f, 0.45f, 0.85f),
                        -129);
                }
            }

            // 네온 간판.
            p.Label("NeonSign_A", "포차", -14.5f, 3.4f, 22, new Color(1f, 0.35f, 0.45f));
            p.Label("NeonSign_B", "24시", 13.0f, 3.9f, 20, new Color(0.35f, 0.95f, 1f));

            // 전구줄 — 노점 사이로 늘어진다.
            p.Rect("StringWire", 0f, 3.55f, 34f, 0.03f, new Color(0.10f, 0.10f, 0.10f), -44);

            for (float x = -16.5f; x <= 16.5f; x += 1.1f)
            {
                float sag = Mathf.Abs(Mathf.Sin((x + 16.5f) * 0.45f)) * 0.18f;

                p.Rect($"Bulb_{x:0.#}", x, 3.45f - sag, 0.14f, 0.18f, new Color(1f, 0.78f, 0.40f), -43);
            }

            // 김 · 연기 (노점 위).
            foreach (float x in DisruptorCatalog.NightMarketStallX)
            {
                for (int s = 0; s < 3; s++)
                {
                    p.Rect($"Steam_{x}_{s}", x + (s - 1) * 0.35f, 3.0f + s * 0.25f, 0.28f, 0.28f, new Color(1f, 1f, 1f, 0.12f), -42);
                }
            }

            // 박스 더미 · 드럼통 화로 · 입간판.
            Boxes(p, -6.0f);
            Boxes(p, 6.0f);

            p.Rect("DrumFire", 0f, 1.2f, 0.7f, 0.9f, new Color(0.30f, 0.30f, 0.32f), -48);
            p.Rect("DrumFlame", 0f, 1.8f, 0.45f, 0.35f, new Color(1f, 0.55f, 0.20f, 0.9f), -47);

            p.Rect("StandSign", 11.6f, 1.3f, 0.7f, 1.0f, new Color(0.95f, 0.90f, 0.75f), -48);
            p.Label("StandSignText", "OPEN", 11.6f, 1.35f, 12, new Color(0.80f, 0.25f, 0.20f));

            // 기존 벤치 · 자판기 자리 — 플라스틱 테이블 · 음식 수레.
            p.Prop("PlasticTable", 5.2f, -0.95f, 1.8f, 0.3f, new Color(0.90f, 0.30f, 0.30f));
            p.Prop("PlasticStoolL", 3.8f, -1.15f, 0.45f, 0.35f, new Color(0.20f, 0.45f, 0.85f), -1);
            p.Prop("PlasticStoolR", 6.6f, -1.15f, 0.45f, 0.35f, new Color(0.20f, 0.45f, 0.85f), -1);
            p.Prop("FoodCart", -10.2f, 0.12f, 1.15f, 1.0f, new Color(0.75f, 0.55f, 0.35f));
            p.Prop("FoodCartAwning", -10.2f, 0.85f, 1.4f, 0.25f, new Color(0.95f, 0.75f, 0.25f), 2);
        }

        private static void Boxes(
            MapPainter p,
            float x)
        {
            p.Rect($"Box_{x}_A", x - 0.2f, 1.0f, 0.55f, 0.45f, new Color(0.60f, 0.45f, 0.28f), -48);
            p.Rect($"Box_{x}_B", x + 0.25f, 1.05f, 0.45f, 0.5f, new Color(0.55f, 0.40f, 0.25f), -48);
            p.Rect($"Box_{x}_C", x, 1.45f, 0.45f, 0.35f, new Color(0.65f, 0.50f, 0.32f), -47);
        }
    }
}
