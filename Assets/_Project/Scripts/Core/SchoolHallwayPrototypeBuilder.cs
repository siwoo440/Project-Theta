using UnityEngine;
using ProjectTheta.Stage;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 학교 복도를 런타임에 짓는다.
    ///
    /// 16일차부터 한 판은 여러 층으로 이뤄진다.
    /// 층마다 씬을 따로 두지 않고 <b>같은 씬 안에 세로로 쌓아</b> 미리 전부 지어 둔다.
    /// 그래야 계단에서 로딩 없이 즉시 넘어갈 수 있고, 동행 NPC를 다시 만들 필요도 없다.
    ///
    /// 층 사이 간격(<see cref="FloorSpace.FloorHeight"/>)이 카메라 시야보다 넓어
    /// 위아래 층이 화면에 함께 보이지 않는다.
    /// </summary>
    public static class SchoolHallwayPrototypeBuilder
    {
        public const string RootName = "Day02_SchoolHallway";

        public const float WalkMinX = FloorSpace.WalkMinX;
        public const float WalkMaxX = FloorSpace.WalkMaxX;
        public const float WalkMinY = FloorSpace.WalkMinY;
        public const float WalkMaxY = FloorSpace.WalkMaxY;

        private static Sprite _squareSprite;

        /// <summary>지금 짓고 있는 층의 세로 원점이다. 모든 배치 좌표에 더해진다.</summary>
        private static float _originY;

        /// <summary>지금 짓고 있는 층의 색조다. 층이 올라갈수록 차가워진다.</summary>
        private static Color _floorTint = Color.white;

        /// <summary>기본 층 수로 건물 전체를 짓는다.</summary>
        public static void Build()
        {
            Build(
                FloorPlanLogic.DefaultFloorCount);
        }

        /// <summary>지금 짓는 장소의 색이다 (21일차). 층 색조에 곱해 장소마다 분위기를 다르게 한다.</summary>
        private static Color _locationTint = Color.white;

        /// <summary>장소 맵 색 묶음이다 (27일차). null이면 예전 학교 복도로 짓는다.</summary>
        private static Map.MapTheme _theme;

        /// <summary>마지막으로 지은 장소 건물에서 가장 많은 층의 맵 오브젝트 수다 (28일차). 예산 테스트가 읽는다.</summary>
        public static int MaxThemedFloorObjectCount { get; private set; }

        /// <summary>건물 전체를 짓는다. 이미 지어져 있으면 아무것도 하지 않는다.</summary>
        public static void Build(
            int floorCount)
        {
            Build(
                floorCount,
                Color.white);
        }

        /// <summary>
        /// 장소 맵으로 건물 전체를 짓는다 (27일차).
        /// 계단 · 이동 범위 벽 · 층 표지 · 벤치 · 자판기 충돌체는 학교 복도와 같은 자리이고,
        /// 벽 · 바닥 · 소품 · 계단 모양만 장소 테마(<see cref="Map.MapThemeCatalog"/>)로 바뀐다.
        /// </summary>
        public static void Build(
            int floorCount,
            Stage.Locations.LocationId location)
        {
            _theme = Map.MapThemeCatalog.Get(location);
            MaxThemedFloorObjectCount = 0;

            try
            {
                Build(
                    floorCount,
                    Color.white);
            }
            finally
            {
                _theme = null;
            }
        }

        /// <summary>
        /// 장소 색을 입혀 건물 전체를 짓는다 (21일차).
        /// 연수원이 아닌 장소는 아직 같은 복도 구조에 색만 다른 임시 장소다. 장소별 지형은 23일차부터 만든다.
        /// </summary>
        public static void Build(
            int floorCount,
            Color locationTint)
        {
            _locationTint = locationTint;

            if (GameObject.Find(RootName) != null)
            {
                return;
            }

            int safeCount =
                Mathf.Max(
                    1,
                    floorCount);

            GameObject building =
                new GameObject(RootName);

            for (int floor = 0;
                 floor < safeCount;
                 floor++)
            {
                BuildFloor(
                    building.transform,
                    floor,
                    safeCount);
            }

            _originY = 0f;
            _floorTint = Color.white;
            _locationTint = Color.white;
        }

        private static void BuildFloor(
            Transform building,
            int floorIndex,
            int floorCount)
        {
            _originY =
                FloorSpace.OriginY(
                    floorIndex);

            _floorTint =
                _theme == null
                    ? FloorPlanLogic.GetWallTint(
                          floorIndex) *
                      _locationTint
                    : Color.white;

            GameObject root =
                new GameObject(
                    $"Floor_{floorIndex + 1:00}");

            root.transform.SetParent(
                building,
                false);

            Transform parent =
                root.transform;

            if (_theme != null)
            {
                BuildThemedFloor(
                    parent,
                    floorIndex,
                    floorCount);

                return;
            }

            CreateVisual(parent, "BackWall", new Vector2(0f, 2.0f), new Vector2(38f, 5.1f), new Color(0.92f, 0.89f, 0.80f), -120);
            CreateVisual(parent, "LowerWallPanel", new Vector2(0f, 0.55f), new Vector2(38f, 1.1f), new Color(0.49f, 0.63f, 0.62f), -110);
            CreateVisual(parent, "WallDivider", new Vector2(0f, 1.08f), new Vector2(38f, 0.12f), new Color(0.22f, 0.33f, 0.33f), -100);
            CreateVisual(parent, "Ceiling", new Vector2(0f, 4.25f), new Vector2(38f, 0.7f), new Color(0.82f, 0.82f, 0.78f), -115);

            CreateVisual(parent, "Floor", new Vector2(0f, -3.40f), new Vector2(38f, 8.2f), new Color(0.76f, 0.73f, 0.66f), -90);
            CreateVisual(parent, "FloorBackBand", new Vector2(0f, 0.73f), new Vector2(38f, 0.18f), new Color(0.25f, 0.34f, 0.34f), -80);
            CreateVisual(parent, "FloorFrontBand", new Vector2(0f, -7.55f), new Vector2(38f, 0.24f), new Color(0.26f, 0.28f, 0.27f), 2200);

            CreateFloorTiles(parent);
            CreateWindows(parent);
            CreateDoors(parent);
            CreateLockers(parent);
            CreateNoticeBoard(parent);
            CreateCeilingLights(parent);
            CreatePillars(parent);
            CreateBench(parent);
            CreateVendingMachine(parent);
            CreateBoundaries(parent);

            CreateStairway(
                parent,
                FloorStairDirection.Up,
                floorIndex,
                floorIndex + 1,
                FloorPlanLogic.HasUpStair(
                    floorIndex,
                    floorCount));

            CreateStairway(
                parent,
                FloorStairDirection.Down,
                floorIndex,
                floorIndex - 1,
                FloorPlanLogic.HasDownStair(
                    floorIndex));
        }

        /// <summary>테마 장소의 한 층이다 (27일차). 꾸미기는 장소마다, 뼈대는 학교 복도와 같다.</summary>
        private static void BuildThemedFloor(
            Transform parent,
            int floorIndex,
            int floorCount)
        {
            Map.MapPainter painter =
                new Map.MapPainter(
                    parent,
                    floorIndex,
                    floorCount,
                    _originY,
                    _theme.GetShade(floorIndex));

            Map.MapBaseLayers.Draw(
                painter,
                _theme);

            Map.Decor.MapDecorCatalog.Draw(
                _theme.Location,
                painter);

            // 28일차: 한 층 오브젝트 수가 예산을 넘으면 알린다.
            MaxThemedFloorObjectCount =
                Mathf.Max(
                    MaxThemedFloorObjectCount,
                    painter.CreatedCount);

            if (Map.MapBudget.IsOver(
                    painter.CreatedCount))
            {
                Debug.LogWarning(
                    $"[Map] {_theme.Location} {floorIndex + 1}F 오브젝트 {painter.CreatedCount}개 — 예산 {Map.MapBudget.MaxObjectsPerFloor}개 초과");
            }

            CreateBoundaries(parent);

            CreateStairway(
                parent,
                FloorStairDirection.Up,
                floorIndex,
                floorIndex + 1,
                FloorPlanLogic.HasUpStair(
                    floorIndex,
                    floorCount));

            CreateStairway(
                parent,
                FloorStairDirection.Down,
                floorIndex,
                floorIndex - 1,
                FloorPlanLogic.HasDownStair(
                    floorIndex));
        }

        private static void CreateFloorTiles(Transform parent)
        {
            for (int x = -16; x <= 16; x += 2)
            {
                CreateVisual(
                    parent,
                    $"FloorSeam_X_{x}",
                    new Vector2(x, -3.35f),
                    new Vector2(0.035f, 7.9f),
                    new Color(0.57f, 0.55f, 0.50f, 0.65f),
                    -70);
            }

            for (int i = 0; i < 7; i++)
            {
                float y = -0.55f - i;
                CreateVisual(
                    parent,
                    $"FloorSeam_Y_{i + 1:00}",
                    new Vector2(0f, y),
                    new Vector2(38f, 0.035f),
                    new Color(0.57f, 0.55f, 0.50f, 0.55f),
                    -70);
            }
        }

        private static void CreateWindows(Transform parent)
        {
            // 13.2 자리는 위층 계단이 쓴다.
            float[] xs = { -7.8f, 7.8f };

            for (int i = 0; i < xs.Length; i++)
            {
                float x = xs[i];
                CreateVisual(parent, $"WindowFrame_{i}", new Vector2(x, 2.65f), new Vector2(3.65f, 2.15f), new Color(0.22f, 0.30f, 0.31f), -60);
                CreateVisual(parent, $"WindowGlass_{i}", new Vector2(x, 2.65f), new Vector2(3.3f, 1.82f), new Color(0.53f, 0.74f, 0.80f), -55);
                CreateVisual(parent, $"WindowSky_{i}", new Vector2(x, 3.0f), new Vector2(3.1f, 0.85f), new Color(0.66f, 0.82f, 0.88f), -54);
                CreateVisual(parent, $"WindowCrossV_{i}", new Vector2(x, 2.65f), new Vector2(0.09f, 1.82f), new Color(0.27f, 0.35f, 0.36f), -50);
                CreateVisual(parent, $"WindowCrossH_{i}", new Vector2(x, 2.65f), new Vector2(3.3f, 0.09f), new Color(0.27f, 0.35f, 0.36f), -50);
                CreateVisual(parent, $"WindowSill_{i}", new Vector2(x, 1.55f), new Vector2(3.9f, 0.18f), new Color(0.75f, 0.74f, 0.69f), -45);
            }
        }

        private static void CreateDoors(Transform parent)
        {
            CreateDoor(parent, -3.3f, "ClassroomDoor_A", new Color(0.35f, 0.23f, 0.17f));
            CreateDoor(parent, 16.0f, "ClassroomDoor_B", new Color(0.30f, 0.22f, 0.16f));
        }

        private static void CreateDoor(Transform parent, float x, string name, Color doorColor)
        {
            CreateVisual(parent, name + "_Frame", new Vector2(x, 2.0f), new Vector2(2.65f, 4.25f), new Color(0.19f, 0.24f, 0.24f), -40);
            CreateVisual(parent, name, new Vector2(x, 1.95f), new Vector2(2.32f, 3.95f), doorColor, -35);
            CreateVisual(parent, name + "_Glass", new Vector2(x, 2.75f), new Vector2(1.4f, 1.1f), new Color(0.43f, 0.62f, 0.66f), -30);
            CreateVisual(parent, name + "_Plate", new Vector2(x, 4.18f), new Vector2(1.7f, 0.38f), new Color(0.24f, 0.34f, 0.35f), -25);
            CreateVisual(parent, name + "_Handle", new Vector2(x + 0.83f, 1.35f), new Vector2(0.15f, 0.15f), new Color(0.88f, 0.72f, 0.30f), -20);
        }

        private static void CreateLockers(Transform parent)
        {
            float startX = 0.1f;

            for (int i = 0; i < 8; i++)
            {
                float x = startX + (i * 0.62f);
                Color lockerColor = i % 2 == 0
                    ? new Color(0.39f, 0.53f, 0.55f)
                    : new Color(0.34f, 0.47f, 0.50f);

                CreateVisual(parent, $"Locker_{i:00}", new Vector2(x, 2.05f), new Vector2(0.58f, 2.75f), lockerColor, -32);
                CreateVisual(parent, $"LockerVent_{i:00}", new Vector2(x, 2.85f), new Vector2(0.32f, 0.07f), new Color(0.18f, 0.27f, 0.28f), -28);
                CreateVisual(parent, $"LockerHandle_{i:00}", new Vector2(x + 0.18f, 1.95f), new Vector2(0.05f, 0.28f), new Color(0.78f, 0.77f, 0.70f), -27);
            }
        }

        private static void CreateNoticeBoard(Transform parent)
        {
            CreateVisual(parent, "NoticeBoardFrame", new Vector2(10.3f, 2.45f), new Vector2(3.25f, 1.95f), new Color(0.30f, 0.20f, 0.14f), -32);
            CreateVisual(parent, "NoticeBoardCork", new Vector2(10.3f, 2.45f), new Vector2(2.92f, 1.62f), new Color(0.66f, 0.47f, 0.30f), -30);
            CreateVisual(parent, "NoticePaper_A", new Vector2(9.55f, 2.65f), new Vector2(0.65f, 0.85f), new Color(0.92f, 0.91f, 0.80f), -25);
            CreateVisual(parent, "NoticePaper_B", new Vector2(10.35f, 2.25f), new Vector2(0.8f, 0.65f), new Color(0.81f, 0.89f, 0.91f), -25);
            CreateVisual(parent, "NoticePaper_C", new Vector2(11.1f, 2.68f), new Vector2(0.65f, 0.75f), new Color(0.90f, 0.78f, 0.76f), -25);
        }

        private static void CreateCeilingLights(Transform parent)
        {
            float[] xs = { -13f, -6.5f, 0f, 6.5f, 13f };

            for (int i = 0; i < xs.Length; i++)
            {
                CreateVisual(parent, $"CeilingLightHousing_{i}", new Vector2(xs[i], 4.0f), new Vector2(2.4f, 0.28f), new Color(0.48f, 0.49f, 0.47f), -10);
                CreateVisual(parent, $"CeilingLight_{i}", new Vector2(xs[i], 3.94f), new Vector2(2.1f, 0.14f), new Color(1f, 0.94f, 0.68f), -8);
            }
        }

        private static void CreatePillars(Transform parent)
        {
            float[] xs = { -17.9f, -5.45f, 5.45f, 17.9f };

            for (int i = 0; i < xs.Length; i++)
            {
                CreateVisual(parent, $"Pillar_{i}", new Vector2(xs[i], 1.8f), new Vector2(0.38f, 5.2f), new Color(0.70f, 0.69f, 0.64f), -5);
            }
        }

        private static void CreateBench(Transform parent)
        {
            GameObject bench = CreateVisual(parent, "HallwayBench", new Vector2(5.2f, -0.95f), new Vector2(2.8f, 0.42f), new Color(0.39f, 0.25f, 0.16f), CharacterOrderForY(-0.95f));
            bench.AddComponent<BoxCollider2D>().size = Vector2.one;

            CreateVisual(parent, "HallwayBenchBack", new Vector2(5.2f, -0.48f), new Vector2(2.8f, 0.55f), new Color(0.45f, 0.29f, 0.18f), CharacterOrderForY(-0.48f));
            CreateVisual(parent, "HallwayBenchLegL", new Vector2(4.35f, -1.35f), new Vector2(0.14f, 0.55f), new Color(0.22f, 0.20f, 0.18f), CharacterOrderForY(-1.35f));
            CreateVisual(parent, "HallwayBenchLegR", new Vector2(6.05f, -1.35f), new Vector2(0.14f, 0.55f), new Color(0.22f, 0.20f, 0.18f), CharacterOrderForY(-1.35f));
        }

        private static void CreateVendingMachine(Transform parent)
        {
            GameObject vending = CreateVisual(parent, "VendingMachine", new Vector2(-10.2f, 0.12f), new Vector2(1.15f, 1.45f), new Color(0.24f, 0.43f, 0.56f), CharacterOrderForY(0.12f));
            vending.AddComponent<BoxCollider2D>().size = Vector2.one;

            CreateVisual(parent, "VendingDisplay", new Vector2(-10.2f, 0.38f), new Vector2(0.72f, 0.48f), new Color(0.67f, 0.84f, 0.86f), CharacterOrderForY(0.12f) + 2);
            CreateVisual(parent, "VendingSlot", new Vector2(-10.2f, -0.28f), new Vector2(0.62f, 0.15f), new Color(0.11f, 0.17f, 0.20f), CharacterOrderForY(0.12f) + 2);
        }

        /// <summary>
        /// 벽면 계단이다. 갈 수 없는 방향이면 막힌 계단실로 그린다.
        /// 막힌 쪽도 그려 두어야 층마다 계단 자리가 일정하게 보인다.
        ///
        /// 17일차에 크기를 절반으로 줄였다. 바닥선에 붙여 두어서
        /// 복도 벽에 난 작은 계단 입구처럼 보이게 했다.
        /// </summary>
        private static void CreateStairway(
            Transform parent,
            FloorStairDirection direction,
            int sourceFloor,
            int targetFloor,
            bool enabled)
        {
            bool up =
                direction == FloorStairDirection.Up;

            string name =
                up
                    ? "StairUp"
                    : "StairDown";

            float x =
                up
                    ? FloorLayout.UpStairX
                    : FloorLayout.DownStairX;

            Color openingColor =
                enabled
                    ? _theme == null ? new Color(0.11f, 0.12f, 0.16f) : _theme.StairOpening
                    : new Color(0.34f, 0.33f, 0.31f);

            Color frameColor =
                _theme == null
                    ? new Color(0.21f, 0.26f, 0.27f)
                    : _theme.StairFrame;

            // 문틀 아래 끝을 교실 문과 같은 높이(-0.12)에 맞춘다.
            CreateVisual(parent, name + "_Frame", new Vector2(x, 0.97f), new Vector2(1.48f, 2.18f), frameColor, -42);
            CreateVisual(parent, name + "_Opening", new Vector2(x, 0.92f), new Vector2(1.28f, 1.98f), openingColor, -38);

            // 27일차: 장소마다 입구 모양을 다르게 그린다 (엘리베이터 문 · 에스컬레이터 난간 · 나무 데크).
            DrawStairStyle(parent, name, x, up, enabled);

            // 계단참을 층계 모양으로 쌓아 올라가는지 내려가는지 형태로 구분한다.
            for (int i = 0; i < 5; i++)
            {
                float stepWidth = 1.12f - (i * 0.135f);

                float stepY =
                    up
                        ? 0.12f + (i * 0.27f)
                        : 1.45f - (i * 0.27f);

                float offsetX =
                    up
                        ? -0.06f + (i * 0.04f)
                        : 0.06f - (i * 0.04f);

                Color stepColor =
                    enabled
                        ? new Color(0.60f, 0.62f, 0.64f, 1f - (i * 0.14f))
                        : new Color(0.40f, 0.40f, 0.39f, 0.5f - (i * 0.07f));

                CreateVisual(
                    parent,
                    $"{name}_Step_{i}",
                    new Vector2(x + offsetX, stepY),
                    new Vector2(stepWidth, 0.11f),
                    stepColor,
                    -36);
            }

            CreateVisual(parent, name + "_SignPlate", new Vector2(x, 2.34f), new Vector2(0.98f, 0.24f), new Color(0.20f, 0.30f, 0.32f), -24);

            Color arrowColor =
                enabled
                    ? new Color(0.98f, 0.86f, 0.42f)
                    : new Color(0.45f, 0.45f, 0.44f);

            // 화살표는 사각형 세 개로 삼각형 느낌만 낸다.
            for (int i = 0; i < 3; i++)
            {
                float width = 0.23f - (i * 0.075f);

                float y =
                    up
                        ? 2.29f + (i * 0.045f)
                        : 2.39f - (i * 0.045f);

                CreateVisual(
                    parent,
                    $"{name}_Arrow_{i}",
                    new Vector2(x, y),
                    new Vector2(width, 0.045f),
                    arrowColor,
                    -22);
            }

            // 계단이 좌우로 떨어져 있으므로 층 표지도 계단마다 붙인다.
            CreateFloorSign(
                parent,
                name,
                x,
                sourceFloor);

            if (!enabled)
            {
                return;
            }

            GameObject trigger =
                new GameObject(
                    name + "_Trigger");

            trigger.transform.SetParent(
                parent,
                false);

            trigger.transform.position =
                new Vector3(
                    x,
                    FloorLayout.StairStandY + _originY,
                    0f);

            trigger.AddComponent<FloorStairway>().Configure(
                direction,
                sourceFloor,
                targetFloor);
        }

        private static void DrawStairStyle(
            Transform parent,
            string name,
            float x,
            bool up,
            bool enabled)
        {
            if (_theme == null)
            {
                return;
            }

            switch (_theme.Stairs)
            {
                case Map.StairStyle.Elevator:
                    // 계단참 위에 닫힌 엘리베이터 문 두 짝을 덮는다.
                    CreateVisual(parent, name + "_ElevatorDoorL", new Vector2(x - 0.32f, 0.92f), new Vector2(0.6f, 1.95f), new Color(0.70f, 0.72f, 0.76f), -35);
                    CreateVisual(parent, name + "_ElevatorDoorR", new Vector2(x + 0.32f, 0.92f), new Vector2(0.6f, 1.95f), new Color(0.66f, 0.68f, 0.72f), -35);
                    CreateVisual(parent, name + "_ElevatorLamp", new Vector2(x, 2.1f), new Vector2(0.3f, 0.12f), enabled ? _theme.Accent : new Color(0.3f, 0.3f, 0.3f), -34);
                    break;

                case Map.StairStyle.Escalator:
                    // 비스듬한 난간.
                    for (int i = 0; i < 6; i++)
                    {
                        float step = i * 0.28f;

                        CreateVisual(
                            parent,
                            $"{name}_EscalatorRail_{i}",
                            new Vector2(x + (up ? -0.55f + step * 0.8f : 0.55f - step * 0.8f), 0.2f + step),
                            new Vector2(0.2f, 0.06f),
                            _theme.Accent,
                            -33);
                    }
                    break;

                case Map.StairStyle.Deck:
                    for (int i = 0; i < 4; i++)
                    {
                        CreateVisual(parent, $"{name}_DeckPlank_{i}", new Vector2(x, 0.1f + i * 0.5f), new Vector2(1.3f, 0.08f), new Color(0.40f, 0.28f, 0.16f), -33);
                    }
                    break;
            }
        }

        /// <summary>계단 위에 붙는 층 표지다. 층수만큼 금색 눈금을 긋는다.</summary>
        private static void CreateFloorSign(
            Transform parent,
            string owner,
            float x,
            int floorIndex)
        {
            CreateVisual(parent, owner + "_FloorSign_Plate", new Vector2(x, 2.78f), new Vector2(0.82f, 0.48f), new Color(0.15f, 0.19f, 0.21f), -20);

            int marks =
                Mathf.Clamp(
                    floorIndex + 1,
                    1,
                    6);

            float start =
                -0.06f * (marks - 1);

            for (int i = 0; i < marks; i++)
            {
                CreateVisual(
                    parent,
                    $"{owner}_FloorSign_Mark_{i}",
                    new Vector2(x + start + (i * 0.12f), 2.78f),
                    new Vector2(0.055f, 0.27f),
                    new Color(0.98f, 0.86f, 0.42f),
                    -18);
            }
        }

        private static void CreateBoundaries(Transform parent)
        {
            float verticalCenter = (WalkMinY + WalkMaxY) * 0.5f;
            float verticalHeight = (WalkMaxY - WalkMinY) + 0.7f;

            CreateBoundary(parent, "Boundary_Left", new Vector2(WalkMinX - 0.2f, verticalCenter), new Vector2(0.35f, verticalHeight));
            CreateBoundary(parent, "Boundary_Right", new Vector2(WalkMaxX + 0.2f, verticalCenter), new Vector2(0.35f, verticalHeight));
            CreateBoundary(parent, "Boundary_Back", new Vector2(0f, WalkMaxY + 0.18f), new Vector2(35.2f, 0.28f));
            CreateBoundary(parent, "Boundary_Front", new Vector2(0f, WalkMinY - 0.18f), new Vector2(35.2f, 0.28f));
        }

        private static void CreateBoundary(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject boundary = new GameObject(name);
            boundary.transform.SetParent(parent, false);
            boundary.transform.position = new Vector3(position.x, position.y + _originY, 0f);
            boundary.AddComponent<BoxCollider2D>().size = size;
        }

        private static int CharacterOrderForY(float y)
        {
            return 1000 - Mathf.RoundToInt(y * 100f);
        }

        private static GameObject CreateVisual(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.transform.position = new Vector3(position.x, position.y + _originY, 0f);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();

            // 층 색조를 곱해 층마다 분위기를 다르게 한다.
            renderer.color = new Color(
                color.r * _floorTint.r,
                color.g * _floorTint.g,
                color.b * _floorTint.b,
                color.a);

            renderer.sortingOrder = sortingOrder;
            return visual;
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            Texture2D texture = new Texture2D(1, 1)
            {
                name = "Day02_RuntimeSquare",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            _squareSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            _squareSprite.name = "Day02_RuntimeSquareSprite";
            return _squareSprite;
        }
    }
}
