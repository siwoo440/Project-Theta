using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 루프탑 클럽 — 야경과 네온 (27일차).
    /// 도시 야경 파노라마 · 유리 난간 · 네온 사인 · 서치라이트 · 스피커 · 라운지 소파, LED 댄스플로어, VIP 소파 · 칵테일 테이블.
    /// DJ 부스 · 바 · 드롭 빛은 클럽 규칙이 그린다.
    /// </summary>
    public static class ClubDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(-11.6f, 0.5f),   // 스피커
            new MapFeature(-4.0f, 1.2f),    // 라운지 소파
            new MapFeature(2.0f, 0.4f),     // 화분
            new MapFeature(11.8f, 0.5f)     // 스피커
        };

        public static void Draw(
            MapPainter p)
        {
            // 도시 야경 파노라마.
            for (int i = 0; i < 20; i++)
            {
                float x = -18f + i * 1.9f;
                float height = 1.0f + MapPainter.Hash(i, 71) * 2.6f;

                p.Rect($"Skyline_{i}", x, 1.0f + height * 0.5f, 1.6f, height, new Color(0.10f, 0.06f, 0.20f), -130);

                for (int w = 0; w < 3; w++)
                {
                    if (MapPainter.Hash(i, 80 + w) < 0.5f)
                    {
                        continue;
                    }

                    p.Rect($"SkylineLight_{i}_{w}", x - 0.3f + w * 0.3f, 1.3f + w * 0.6f, 0.15f, 0.15f,
                        w % 2 == 0 ? new Color(1f, 0.80f, 0.45f) : new Color(0.60f, 0.85f, 1f), -129);
                }
            }

            // 서치라이트 · 별.
            p.Rect("SearchLight_A", -6f, 3.8f, 0.25f, 2.2f, new Color(0.80f, 0.60f, 1f, 0.12f), -128);
            p.Rect("SearchLight_B", 6f, 3.8f, 0.25f, 2.2f, new Color(0.40f, 0.90f, 1f, 0.12f), -128);

            for (int i = 0; i < 12; i++)
            {
                p.Rect($"Star_{i}", -17f + i * 3f, 4.1f + MapPainter.Hash(i, 91) * 0.5f, 0.06f, 0.06f, new Color(1f, 1f, 1f, 0.8f), -131);
            }

            // 유리 난간.
            p.Rect("GlassRail", 0f, 1.3f, 36f, 0.9f, new Color(0.60f, 0.80f, 1f, 0.12f), -120);
            p.Rect("RailTop", 0f, 1.78f, 36f, 0.05f, new Color(0.90f, 0.95f, 1f, 0.6f), -119);

            // 네온 사인.
            p.Label("NeonRooftop", p.Floor == 0 ? "ROOFTOP BAR" : "VIP LOUNGE", 0f, 3.6f, 26,
                p.Floor == 0 ? new Color(1f, 0.40f, 0.90f) : new Color(0.40f, 0.95f, 1f));
            p.Rect("NeonUnderline", 0f, 3.25f, 5f, 0.05f, new Color(1f, 0.40f, 0.90f, 0.8f), -44);

            // 스피커 · 라운지 소파 · 화분.
            Speaker(p, -11.6f);
            Speaker(p, 11.8f);

            p.Rect("LoungeSofa", -4.0f, 1.1f, 2.4f, 0.5f, new Color(0.45f, 0.15f, 0.40f), -48);
            p.Rect("LoungeSofaBack", -4.0f, 1.5f, 2.4f, 0.45f, new Color(0.38f, 0.12f, 0.34f), -49);
            p.Rect("ClubPlantPot", 2.0f, 1.05f, 0.5f, 0.5f, new Color(0.20f, 0.20f, 0.22f), -49);
            p.Rect("ClubPlant", 2.0f, 1.8f, 0.6f, 1.0f, new Color(0.20f, 0.50f, 0.35f), -50);

            // 기존 벤치 · 자판기 자리 — VIP 소파 · 칵테일 테이블.
            p.Prop("VipSofa", 5.2f, -0.95f, 2.8f, 0.42f, new Color(0.55f, 0.20f, 0.50f));
            p.Prop("VipSofaBack", 5.2f, -0.5f, 2.8f, 0.5f, new Color(0.45f, 0.15f, 0.42f), 1);
            p.Prop("CocktailTable", -10.2f, 0.12f, 0.9f, 1.2f, new Color(0.25f, 0.25f, 0.30f));
            p.Prop("CocktailGlow", -10.2f, 0.75f, 1.1f, 0.12f, new Color(0.40f, 0.90f, 1f), 2);
        }

        private static void Speaker(
            MapPainter p,
            float x)
        {
            p.Rect($"Speaker_{x}", x, 1.5f, 0.95f, 1.8f, new Color(0.08f, 0.08f, 0.10f), -48);
            p.Rect($"SpeakerWoofer_{x}", x, 1.1f, 0.6f, 0.6f, new Color(0.22f, 0.22f, 0.26f), -47);
            p.Rect($"SpeakerTweeter_{x}", x, 2.0f, 0.3f, 0.3f, new Color(0.22f, 0.22f, 0.26f), -47);
        }
    }
}
