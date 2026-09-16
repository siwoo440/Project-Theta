using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 쇼핑몰 — 쇼윈도와 대리석 (27일차).
    /// 매장 쇼윈도(마네킹 · 조명) · 브랜드 간판 · 층 안내판 · 화분, 광택 대리석, 몰 벤치 · 안내 키오스크.
    /// 셔터 · 기둥 · CCTV · 보안실 표지는 쇼핑몰 규칙이 그린다.
    /// </summary>
    public static class MallDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(-7.4f, 1.3f),    // 쇼윈도 A
            new MapFeature(0f, 1.6f),       // 쇼윈도 B
            new MapFeature(8f, 1.6f),       // 쇼윈도 C
            new MapFeature(11.6f, 0.4f)     // 화분
        };

        private static readonly string[] Brands =
        {
            "THETA", "MUSE", "LUNA"
        };

        private static readonly Color[] BrandColors =
        {
            new Color(0.85f, 0.35f, 0.55f),
            new Color(0.30f, 0.50f, 0.85f),
            new Color(0.25f, 0.65f, 0.50f)
        };

        public static void Draw(
            MapPainter p)
        {
            ShopWindow(p, 0, -7.4f, 1.3f);
            ShopWindow(p, 1, 0f, 1.6f);
            ShopWindow(p, 2, 8f, 1.6f);

            // 층 안내판 · 화분.
            p.Rect("FloorGuide", -12.2f, 3.0f, 0.9f, 1.3f, new Color(0.25f, 0.25f, 0.30f), -45);
            p.Label("FloorGuideText", $"{p.Floor + 1}F", -12.2f, 3.3f, 18, new Color(1f, 1f, 1f));

            p.Rect("MallPlantPot", 11.6f, 1.05f, 0.55f, 0.55f, new Color(0.95f, 0.95f, 0.92f), -49);
            p.Rect("MallPlant", 11.6f, 1.85f, 0.7f, 1.1f, new Color(0.30f, 0.62f, 0.38f), -50);

            // 천장 조명 · 난간 너머 아래층 빛.
            MapBaseLayers.CeilingLights(p, new Color(0.90f, 0.88f, 0.85f), new Color(1f, 0.98f, 0.90f), false);

            // 기존 벤치 · 자판기 자리 — 몰 벤치 · 안내 키오스크.
            p.Prop("MallBench", 5.2f, -0.95f, 2.8f, 0.35f, new Color(0.62f, 0.48f, 0.32f));
            p.Prop("MallBenchLegs", 5.2f, -1.25f, 2.4f, 0.25f, new Color(0.40f, 0.40f, 0.42f), -1);
            p.Prop("Kiosk", -10.2f, 0.12f, 0.9f, 1.45f, new Color(0.95f, 0.95f, 0.97f));
            p.Prop("KioskScreen", -10.2f, 0.45f, 0.65f, 0.6f, new Color(0.30f, 0.55f, 0.90f), 2);
        }

        private static void ShopWindow(
            MapPainter p,
            int index,
            float x,
            float halfWidth)
        {
            float width = halfWidth * 2f;
            Color brand = BrandColors[index % BrandColors.Length];

            p.Rect($"ShopFrame_{index}", x, 2.1f, width, 3.2f, new Color(0.30f, 0.28f, 0.28f), -62);
            p.Rect($"ShopGlass_{index}", x, 2.0f, width - 0.2f, 2.8f, new Color(0.85f, 0.90f, 0.95f), -61);
            p.Rect($"ShopLight_{index}", x, 3.25f, width - 0.3f, 0.12f, new Color(1f, 0.95f, 0.75f), -60);
            p.Rect($"ShopSign_{index}", x, 3.95f, width - 0.2f, 0.45f, brand, -60);
            p.Label($"ShopSignText_{index}", Brands[index % Brands.Length], x, 3.95f, 18, new Color(1f, 1f, 1f));

            // 마네킹 두 개.
            for (int m = 0; m < 2; m++)
            {
                float mx = x + (m == 0 ? -0.45f : 0.45f);

                p.Rect($"MannequinHead_{index}_{m}", mx, 2.55f, 0.22f, 0.22f, new Color(0.92f, 0.88f, 0.82f), -59);
                p.Rect($"MannequinBody_{index}_{m}", mx, 1.9f, 0.4f, 1.0f, Color.Lerp(brand, Color.white, 0.3f), -59);
            }
        }
    }
}
