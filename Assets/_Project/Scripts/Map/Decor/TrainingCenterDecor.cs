using UnityEngine;

namespace ProjectTheta.Map.Decor
{
    /// <summary>
    /// 기업 연수원 — 깔끔한 사내 교육동 (27일차).
    /// 흰 벽 · 강의실 문 · 화이트보드 · 교육 일정표 · 화분 · 정수기, 회색 카펫, 대기 의자 · 커피 머신.
    /// </summary>
    public static class TrainingCenterDecor
    {
        public static readonly MapFeature[] LowWall =
        {
            new MapFeature(-3.3f, 1.35f),   // 강의실 A
            new MapFeature(2.6f, 1.35f),    // 강의실 B
            new MapFeature(-11.2f, 0.4f),   // 화분
            new MapFeature(11.6f, 0.4f),    // 화분
            new MapFeature(0f, 0.35f)       // 정수기
        };

        private static readonly Color Door = new Color(0.62f, 0.66f, 0.72f);
        private static readonly Color Frame = new Color(0.30f, 0.42f, 0.58f);

        public static void Draw(
            MapPainter p)
        {
            // 높은 창 두 개 — 바깥은 맑은 낮.
            foreach (float x in new[] { -7.8f, 7.8f })
            {
                p.Rect($"Window_{x}", x, 2.95f, 3.6f, 1.6f, Frame, -60);
                p.Rect($"WindowGlass_{x}", x, 2.95f, 3.3f, 1.35f, new Color(0.70f, 0.86f, 0.98f), -55);
                p.Rect($"WindowMullion_{x}", x, 2.95f, 0.07f, 1.35f, Frame, -50);
            }

            Classroom(p, "LectureA", -3.3f, "강의실 A");
            Classroom(p, "LectureB", 2.6f, "강의실 B");

            // 화이트보드 · 교육 일정표 (높은 벽).
            p.Rect("Whiteboard", 10.3f, 2.8f, 2.6f, 1.3f, new Color(0.98f, 0.98f, 0.98f), -40);
            p.Rect("WhiteboardFrame", 10.3f, 2.1f, 2.7f, 0.08f, new Color(0.55f, 0.58f, 0.62f), -39);
            p.Rect("WhiteboardText_A", 9.9f, 3.1f, 1.4f, 0.06f, new Color(0.25f, 0.40f, 0.80f), -38);
            p.Rect("WhiteboardText_B", 10.1f, 2.85f, 1.8f, 0.06f, new Color(0.25f, 0.40f, 0.80f), -38);
            p.Rect("WhiteboardText_C", 9.8f, 2.6f, 1.2f, 0.06f, new Color(0.85f, 0.30f, 0.30f), -38);

            p.Rect("Schedule", -0.3f, 3.2f, 1.6f, 0.9f, new Color(0.90f, 0.93f, 0.97f), -40);
            p.Label("ScheduleLabel", "교육 일정", -0.3f, 3.2f, 16, new Color(0.25f, 0.35f, 0.55f));

            // 화분 · 정수기.
            Plant(p, -11.2f);
            Plant(p, 11.6f);

            p.Rect("WaterCooler", 0f, 1.35f, 0.5f, 1.1f, new Color(0.85f, 0.88f, 0.92f), -48);
            p.Rect("WaterBottle", 0f, 2.05f, 0.36f, 0.45f, new Color(0.55f, 0.78f, 0.95f, 0.85f), -47);

            // 복도 가운데 안내선.
            p.Rect("GuideLine", 0f, -2.1f, 34f, 0.08f, new Color(0.30f, 0.55f, 0.90f, 0.35f), -68);

            MapBaseLayers.CeilingLights(p, new Color(0.70f, 0.72f, 0.75f), new Color(0.98f, 0.98f, 1.00f), false);

            // 기존 벤치 · 자판기 자리 — 대기 의자 · 커피 머신.
            p.Prop("WaitingChairs", 5.2f, -0.95f, 2.8f, 0.42f, new Color(0.30f, 0.45f, 0.70f));
            p.Prop("WaitingChairsBack", 5.2f, -0.48f, 2.8f, 0.55f, new Color(0.26f, 0.40f, 0.64f), 1);
            p.Prop("CoffeeMachine", -10.2f, 0.12f, 1.15f, 1.45f, new Color(0.22f, 0.22f, 0.24f));
            p.Prop("CoffeeMachineScreen", -10.2f, 0.4f, 0.7f, 0.35f, new Color(0.55f, 0.85f, 0.70f), 2);
        }

        private static void Classroom(
            MapPainter p,
            string name,
            float x,
            string title)
        {
            p.Rect(name + "_Frame", x, 2.0f, 2.65f, 4.25f, Frame, -40);
            p.Rect(name + "_Door", x, 1.95f, 2.32f, 3.95f, Door, -35);
            p.Rect(name + "_Glass", x, 2.75f, 1.4f, 1.1f, new Color(0.75f, 0.88f, 0.96f), -30);
            p.Rect(name + "_Handle", x + 0.83f, 1.35f, 0.15f, 0.15f, new Color(0.85f, 0.85f, 0.88f), -20);
            p.Label(name + "_Plate", title, x, 4.2f, 16, new Color(0.95f, 0.97f, 1.00f));
        }

        private static void Plant(
            MapPainter p,
            float x)
        {
            p.Rect($"PlantPot_{x}", x, 1.05f, 0.55f, 0.55f, new Color(0.85f, 0.85f, 0.82f), -49);
            p.Rect($"PlantLeaves_{x}", x, 1.75f, 0.75f, 0.95f, new Color(0.30f, 0.60f, 0.35f), -50);
        }
    }
}
