using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 해변가 — 탁 트인 백사장 (27일차).
    /// 하늘 · 해 · 구름 · 수평선 바다 · 야자수 · 매점 오두막, 모래 · 젖은 모래 · 물결, 선베드 · 아이스박스.
    /// 파라솔 · 망루 · 밀물은 해변 규칙이 그린다.
    /// </summary>
    public static class BeachDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(-12.1f, 0.35f),  // 야자수
            new MapFeature(-6.6f, 1.45f),   // 매점 오두막
            new MapFeature(7.4f, 0.35f),    // 야자수
            new MapFeature(11.5f, 0.35f)    // 야자수
        };

        private static readonly Color Trunk = new Color(0.55f, 0.38f, 0.22f);
        private static readonly Color Leaf = new Color(0.20f, 0.62f, 0.35f);

        public static void Draw(
            MapPainter p)
        {
            // 해 · 구름.
            p.Rect("SunGlow", 12.5f, 3.9f, 1.6f, 1.6f, new Color(1f, 0.95f, 0.70f, 0.45f), -133);
            p.Rect("Sun", 12.5f, 3.9f, 0.9f, 0.9f, new Color(1f, 0.92f, 0.55f), -132);

            for (int i = 0; i < 5; i++)
            {
                float x = -15f + i * 6.5f;
                float y = 3.3f + MapPainter.Hash(i, 5) * 0.9f;

                p.Rect($"Cloud_{i}", x, y, 2.4f, 0.35f, new Color(1f, 1f, 1f, 0.85f), -131);
                p.Rect($"CloudTop_{i}", x + 0.3f, y + 0.22f, 1.3f, 0.3f, new Color(1f, 1f, 1f, 0.85f), -131);
            }

            // 먼 바다와 잔물결 · 요트.
            p.Rect("SeaFar", 0f, 1.35f, MapBaseLayers.Width, 0.6f, new Color(0.18f, 0.50f, 0.78f), -129);

            for (int i = 0; i < 12; i++)
            {
                p.Rect($"SeaGlint_{i}", -17f + i * 3.1f, 1.2f + MapPainter.Hash(i, 9) * 0.3f, 0.9f, 0.04f, new Color(1f, 1f, 1f, 0.55f), -128);
            }

            p.Rect("YachtHull", 3.5f, 1.55f, 1.2f, 0.18f, new Color(0.95f, 0.95f, 0.95f), -127);
            p.Rect("YachtSail", 3.5f, 1.95f, 0.08f, 0.6f, new Color(0.95f, 0.95f, 0.95f), -127);

            // 모래 언덕 경계.
            p.Rect("Dune", 0f, 0.72f, MapBaseLayers.Width, 0.3f, new Color(0.98f, 0.90f, 0.70f), -125);

            Palm(p, -12.1f, 2.6f);
            Palm(p, 7.4f, 2.3f);
            Palm(p, 11.5f, 2.8f);

            // 매점 오두막.
            p.Rect("HutBody", -6.6f, 1.35f, 2.8f, 1.3f, new Color(0.80f, 0.62f, 0.40f), -48);
            p.Rect("HutRoof", -6.6f, 2.25f, 3.2f, 0.5f, new Color(0.95f, 0.45f, 0.35f), -47);
            p.Rect("HutRoofStripe", -6.6f, 2.25f, 3.2f, 0.12f, new Color(1f, 1f, 1f, 0.8f), -46);
            p.Rect("HutWindow", -6.6f, 1.45f, 1.8f, 0.5f, new Color(0.30f, 0.20f, 0.12f), -46);
            p.Label("HutSign", "SNACK", -6.6f, 2.8f, 18, new Color(1f, 0.95f, 0.80f));

            // 젖은 모래 · 파도 거품 (밀물에 잠기는 줄).
            p.Rect("WetSand", 0f, -4.6f, MapBaseLayers.Width, 2.0f, new Color(0.84f, 0.74f, 0.54f), -85);
            p.Rect("FoamLine", 0f, -5.55f, MapBaseLayers.Width, 0.12f, new Color(1f, 1f, 1f, 0.7f), -84);
            p.Rect("ShoreWater", 0f, -6.6f, MapBaseLayers.Width, 1.9f, new Color(0.30f, 0.65f, 0.90f), -84);

            // 비치타월 · 조개 · 발자국.
            p.Rect("Towel_A", -1.8f, -2.9f, 1.4f, 0.5f, new Color(0.95f, 0.40f, 0.55f, 0.9f), -66);
            p.Rect("TowelStripe_A", -1.8f, -2.9f, 1.4f, 0.1f, new Color(1f, 1f, 1f, 0.8f), -65);
            p.Rect("Towel_B", 8.8f, -1.6f, 1.4f, 0.5f, new Color(0.35f, 0.70f, 0.95f, 0.9f), -66);

            for (int i = 0; i < 8; i++)
            {
                p.Rect($"Footprint_{i}", -14f + i * 1.1f, -3.2f + (i % 2) * 0.25f, 0.14f, 0.08f, new Color(0.80f, 0.68f, 0.46f), -67);
            }

            // 기존 벤치 · 자판기 자리 — 선베드 · 아이스박스.
            p.Prop("SunLounger", 5.2f, -0.95f, 2.8f, 0.3f, new Color(1f, 1f, 1f));
            p.Prop("SunLoungerBack", 6.3f, -0.6f, 0.7f, 0.6f, new Color(0.95f, 0.95f, 0.95f), 1);
            p.Prop("IceBox", -10.2f, 0.12f, 1.15f, 0.8f, new Color(0.30f, 0.60f, 0.90f));
            p.Prop("IceBoxLid", -10.2f, 0.58f, 1.2f, 0.16f, new Color(1f, 1f, 1f), 2);
        }

        private static void Palm(
            MapPainter p,
            float x,
            float height)
        {
            p.Rect($"PalmTrunk_{x}", x, 0.8f + height * 0.5f, 0.28f, height, Trunk, -49);

            float top = 0.8f + height;

            p.Rect($"PalmLeafL_{x}", x - 0.6f, top - 0.05f, 1.3f, 0.22f, Leaf, -48);
            p.Rect($"PalmLeafR_{x}", x + 0.6f, top - 0.05f, 1.3f, 0.22f, Leaf, -48);
            p.Rect($"PalmLeafT_{x}", x, top + 0.2f, 0.9f, 0.25f, Leaf, -48);
            p.Rect($"Coconut_{x}", x + 0.1f, top - 0.25f, 0.22f, 0.22f, new Color(0.40f, 0.28f, 0.16f), -47);
        }
    }
}
