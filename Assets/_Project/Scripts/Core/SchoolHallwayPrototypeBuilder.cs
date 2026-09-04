using UnityEngine;

namespace ProjectTheta.Core
{
    public static class SchoolHallwayPrototypeBuilder
    {
        public const float WalkMinX = -17.4f;
        public const float WalkMaxX = 17.4f;
        public const float WalkMinY = -5.2f;
        public const float WalkMaxY = 0.9f;

        private static Sprite _squareSprite;

        public static void Build()
        {
            if (GameObject.Find("Day02_SchoolHallway") != null)
            {
                return;
            }

            GameObject root = new GameObject("Day02_SchoolHallway");

            CreateVisual(root.transform, "BackWall", new Vector2(0f, 2.0f), new Vector2(38f, 5.1f), new Color(0.92f, 0.89f, 0.80f), -120);
            CreateVisual(root.transform, "LowerWallPanel", new Vector2(0f, 0.55f), new Vector2(38f, 1.1f), new Color(0.49f, 0.63f, 0.62f), -110);
            CreateVisual(root.transform, "WallDivider", new Vector2(0f, 1.08f), new Vector2(38f, 0.12f), new Color(0.22f, 0.33f, 0.33f), -100);
            CreateVisual(root.transform, "Ceiling", new Vector2(0f, 4.25f), new Vector2(38f, 0.7f), new Color(0.82f, 0.82f, 0.78f), -115);

            // Day 02 추가 수정: 기존 4.1 높이의 바닥을 약 2배(8.2)로 확장.
            // 상단 위치는 유지하고 아래쪽(화면 전방)으로 확장한다.
            CreateVisual(root.transform, "Floor", new Vector2(0f, -3.40f), new Vector2(38f, 8.2f), new Color(0.76f, 0.73f, 0.66f), -90);
            CreateVisual(root.transform, "FloorBackBand", new Vector2(0f, 0.73f), new Vector2(38f, 0.18f), new Color(0.25f, 0.34f, 0.34f), -80);
            CreateVisual(root.transform, "FloorFrontBand", new Vector2(0f, -7.55f), new Vector2(38f, 0.24f), new Color(0.26f, 0.28f, 0.27f), 2200);

            CreateFloorTiles(root.transform);
            CreateWindows(root.transform);
            CreateDoors(root.transform);
            CreateLockers(root.transform);
            CreateNoticeBoard(root.transform);
            CreateCeilingLights(root.transform);
            CreatePillars(root.transform);
            CreateBench(root.transform);
            CreateVendingMachine(root.transform);
            CreateBoundaries(root.transform);
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
            float[] xs = { -13.2f, -7.8f, 7.8f, 13.2f };

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
            boundary.transform.position = position;
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
            visual.transform.position = new Vector3(position.x, position.y, 0f);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSquareSprite();
            renderer.color = color;
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
