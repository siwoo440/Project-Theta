using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 헬스장 — 거울과 고무 매트 (27일차).
    /// 노출 콘크리트 · 전신 거울 · 동기부여 문구 · 덤벨 랙 · 정수기 · 수건 걸이, 검정 고무 매트, 벤치 프레스 · 단백질 음료 기계.
    /// 러닝머신 · 요가 매트 · 구역 색은 헬스장 규칙이 그린다. 2F 수영장 칸은 벽 위쪽에 파란 타일을 더한다.
    /// </summary>
    public static class FitnessDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(1.0f, 0.3f),     // 정수기 (운동 구역 사이)
            new MapFeature(11.6f, 0.4f)     // 수건 걸이
        };

        public static void Draw(
            MapPainter p)
        {
            // 콘크리트 줄눈.
            for (float x = -18f; x <= 18f; x += 3f)
            {
                p.Rect($"ConcreteJoint_{x}", x, 2.8f, 0.04f, 3.0f, new Color(0.52f, 0.53f, 0.54f), -118);
            }

            // 전신 거울 (벽 위쪽, 러닝머신보다 높이).
            foreach (float x in new[] { -8f, 0f, 8f })
            {
                p.Rect($"MirrorFrame_{x}", x, 2.95f, 4.4f, 1.9f, new Color(0.18f, 0.18f, 0.20f), -60);
                p.Rect($"Mirror_{x}", x, 2.95f, 4.2f, 1.7f, new Color(0.72f, 0.80f, 0.86f), -58);
                p.Rect($"MirrorShine_{x}", x - 1.2f, 3.2f, 0.25f, 1.2f, new Color(1f, 1f, 1f, 0.35f), -57);
            }

            p.Label("Slogan", "NO PAIN · NO GAIN", 0f, 4.2f, 20, new Color(0.95f, 0.45f, 0.20f));

            // 2F 수영장 칸 — 파란 타일 벽.
            if (p.Floor == 1)
            {
                p.Rect("PoolWall", -4.75f, 2.8f, 10.5f, 3.2f, new Color(0.35f, 0.62f, 0.85f), -59);

                for (float x = -10f; x <= 0.5f; x += 0.7f)
                {
                    p.Rect($"PoolTile_{x:0.#}", x, 2.8f, 0.03f, 3.2f, new Color(0.60f, 0.80f, 0.95f), -58);
                }

                p.Rect("PoolLane", -4.75f, -2.6f, 10.5f, 0.08f, new Color(1f, 1f, 1f, 0.4f), -56);
                p.Rect("PoolLane2", -4.75f, -4.0f, 10.5f, 0.08f, new Color(1f, 0.3f, 0.3f, 0.4f), -56);
            }

            // 정수기 · 수건 걸이.
            p.Rect("GymWaterCooler", 1.0f, 1.35f, 0.5f, 1.1f, new Color(0.85f, 0.88f, 0.92f), -48);
            p.Rect("TowelRack", 11.6f, 1.6f, 0.8f, 0.08f, new Color(0.70f, 0.70f, 0.72f), -48);
            p.Rect("Towel_1", 11.35f, 1.3f, 0.25f, 0.5f, new Color(1f, 1f, 1f), -47);
            p.Rect("Towel_2", 11.85f, 1.3f, 0.25f, 0.5f, new Color(0.95f, 0.45f, 0.20f), -47);

            // 바닥 구역 경계 테이프.
            p.Rect("FloorTape", 0f, -0.2f, 34f, 0.06f, new Color(0.95f, 0.80f, 0.20f, 0.6f), -67);

            MapBaseLayers.CeilingLights(p, new Color(0.22f, 0.22f, 0.24f), new Color(1.00f, 0.95f, 0.85f), false);

            // 기존 벤치 · 자판기 자리 — 벤치 프레스 · 단백질 음료 기계.
            p.Prop("BenchPress", 5.2f, -0.95f, 2.2f, 0.3f, new Color(0.15f, 0.15f, 0.17f));
            p.Prop("BenchPressBar", 5.2f, -0.35f, 2.8f, 0.07f, new Color(0.75f, 0.75f, 0.78f), 2);
            p.Prop("BenchPressPlateL", 3.85f, -0.35f, 0.14f, 0.55f, new Color(0.10f, 0.10f, 0.10f), 3);
            p.Prop("BenchPressPlateR", 6.55f, -0.35f, 0.14f, 0.55f, new Color(0.10f, 0.10f, 0.10f), 3);
            p.Prop("ShakeMachine", -10.2f, 0.12f, 1.15f, 1.45f, new Color(0.95f, 0.45f, 0.20f));
            p.Prop("ShakeMachineScreen", -10.2f, 0.45f, 0.75f, 0.4f, new Color(0.15f, 0.15f, 0.17f), 2);
        }
    }
}
