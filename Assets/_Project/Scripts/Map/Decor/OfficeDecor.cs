using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 오피스 타워 — 야근 중인 사무실 (27일차).
    /// 창밖 야경 · 파티션 · 모니터 책상 · 복사기 · 회사 로고, 네이비 카펫, 라운지 소파 · 정수기. 조명은 절반 꺼져 있다.
    /// 탕비실 · 회의실 · 출입증 게이트는 오피스 규칙이 그린다.
    /// </summary>
    public static class OfficeDecor
    {
        public static readonly float[] DeskX =
        {
            -11.2f, -3.2f, -0.4f, 2.0f
        };

        public const float DeskHalfWidth = 0.9f;

        public const float CopierX = 10.4f;

        public static MapFeature[] LowWall
        {
            get
            {
                MapFeature[] result = new MapFeature[DeskX.Length + 1];

                for (int i = 0; i < DeskX.Length; i++)
                {
                    result[i] = new MapFeature(DeskX[i], DeskHalfWidth);
                }

                result[DeskX.Length] = new MapFeature(CopierX, 0.45f);

                return result;
            }
        }

        public static void Draw(
            MapPainter p)
        {
            // 창밖 야경 띠.
            p.Rect("NightWindow", 0f, 3.15f, 36f, 1.7f, new Color(0.06f, 0.08f, 0.18f), -105);

            for (int i = 0; i < 24; i++)
            {
                float x = -17f + i * 1.5f;
                float height = 0.4f + MapPainter.Hash(i, 41 + p.Floor) * 1.1f;

                p.Rect($"CityBlock_{i}", x, 2.3f + height * 0.5f, 1.2f, height, new Color(0.12f, 0.14f, 0.26f), -104);

                if (MapPainter.Hash(i, 51 + p.Floor) > 0.4f)
                {
                    p.Rect($"CityLight_{i}", x, 2.35f + height * 0.6f, 0.2f, 0.15f, new Color(1f, 0.85f, 0.50f), -103);
                }
            }

            for (float x = -18f; x <= 18f; x += 3f)
            {
                p.Rect($"WindowMullion_{x}", x, 3.15f, 0.08f, 1.7f, new Color(0.35f, 0.38f, 0.45f), -102);
            }

            p.Label("CompanyLogo", "THETA CORP.", -7.5f, 4.2f, 18, new Color(0.70f, 0.80f, 1.00f));

            // 파티션 · 모니터 책상.
            for (int i = 0; i < DeskX.Length; i++)
            {
                Desk(p, i, DeskX[i]);
            }

            // 복사기.
            p.Rect("Copier", CopierX, 1.2f, 0.85f, 0.9f, new Color(0.85f, 0.86f, 0.88f), -48);
            p.Rect("CopierTop", CopierX, 1.72f, 0.9f, 0.15f, new Color(0.30f, 0.32f, 0.36f), -47);

            // 형광등 절반만 켜진 야근 층.
            MapBaseLayers.CeilingLights(p, new Color(0.30f, 0.32f, 0.36f), new Color(0.88f, 0.94f, 1.00f), true);

            // 기존 벤치 · 자판기 자리 — 라운지 소파 · 정수기.
            p.Prop("LoungeSofa", 5.2f, -0.95f, 2.8f, 0.42f, new Color(0.30f, 0.34f, 0.48f));
            p.Prop("LoungeSofaBack", 5.2f, -0.5f, 2.8f, 0.5f, new Color(0.26f, 0.30f, 0.42f), 1);
            p.Prop("OfficeWater", -10.2f, 0.12f, 0.6f, 1.45f, new Color(0.85f, 0.88f, 0.92f));
            p.Prop("OfficeWaterBottle", -10.2f, 0.95f, 0.45f, 0.5f, new Color(0.55f, 0.78f, 0.95f, 0.85f), 2);
        }

        private static void Desk(
            MapPainter p,
            int index,
            float x)
        {
            float width = DeskHalfWidth * 2f;

            p.Rect($"Partition_{index}", x, 1.55f, width, 1.1f, new Color(0.62f, 0.66f, 0.72f), -50);
            p.Rect($"DeskTop_{index}", x, 1.05f, width, 0.12f, new Color(0.45f, 0.35f, 0.28f), -49);
            p.Rect($"Monitor_{index}", x, 1.45f, 0.7f, 0.45f, new Color(0.10f, 0.10f, 0.12f), -48);

            // 켜진 모니터는 야근 중인 자리다.
            bool on = MapPainter.Hash(index, 61 + p.Floor) > 0.35f;

            p.Rect($"MonitorScreen_{index}", x, 1.47f, 0.6f, 0.35f,
                on ? new Color(0.45f, 0.70f, 1.00f) : new Color(0.18f, 0.20f, 0.24f), -47);
        }
    }
}
