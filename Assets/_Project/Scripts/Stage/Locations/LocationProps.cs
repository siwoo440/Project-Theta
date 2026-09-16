using UnityEngine;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 장소 소품을 코드로 만든다 (23일차). 노점 · 망루 · 표지판 같은 임시 그림이다.
    /// 전용 그림이 생기면 이 파일만 바꾸면 된다.
    ///
    /// 소품은 바닥 위, 캐릭터 아래(-45 ~ -30)에 깐다. 캐릭터 계층은 10000±4000이다.
    /// </summary>
    public static class LocationProps
    {
        private const int BackSortOrder = -45;
        private const int FrontSortOrder = -30;

        private static Sprite _solid;

        /// <summary>월드 1칸짜리 흰 사각형이다.</summary>
        public static Sprite Solid
        {
            get
            {
                if (_solid != null)
                {
                    return _solid;
                }

                Texture2D texture =
                    new Texture2D(4, 4, TextureFormat.RGBA32, false)
                    {
                        name = "LocationPropSolid",
                        hideFlags = HideFlags.DontSave,
                        filterMode = FilterMode.Point
                    };

                Color32[] pixels = new Color32[16];

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color32(255, 255, 255, 255);
                }

                texture.SetPixels32(pixels);
                texture.Apply();

                _solid =
                    Sprite.Create(
                        texture,
                        new Rect(0f, 0f, 4f, 4f),
                        new Vector2(0.5f, 0.5f),
                        4f);

                _solid.hideFlags = HideFlags.DontSave;

                return _solid;
            }
        }

        public static SpriteRenderer Box(
            Transform parent,
            string name,
            Vector2 worldCenter,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            return Create(parent, name, Solid, worldCenter, size, color, sortingOrder);
        }

        public static SpriteRenderer Blob(
            Transform parent,
            string name,
            Vector2 worldCenter,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            return Create(parent, name, VfxLibrary.Get(VfxSprite.SoftCircle), worldCenter, size, color, sortingOrder);
        }

        public static void Sign(
            Transform parent,
            int floor,
            Vector2 localPosition,
            string text,
            Color color,
            int fontSize = 22)
        {
            Vector2 world =
                FloorSpace.ToWorld(floor, localPosition);

            UnityEngine.UI.Text label =
                WorldLabel.Create(
                    parent,
                    "Sign",
                    world - (Vector2)parent.position,
                    fontSize,
                    color);

            label.text = text;
        }

        /// <summary>야시장 포장마차다. 복도 안쪽 벽에 붙어 선다.</summary>
        public static void Stall(
            Transform parent,
            int floor,
            float x,
            Color awning,
            string title)
        {
            float backY = FloorSpace.WalkMaxY;

            Box(parent, "StallCounter",
                FloorSpace.ToWorld(floor, new Vector2(x, backY + 0.35f)),
                new Vector2(2.6f, 0.9f),
                new Color(0.35f, 0.24f, 0.18f),
                BackSortOrder);

            Box(parent, "StallAwning",
                FloorSpace.ToWorld(floor, new Vector2(x, backY + 1.5f)),
                new Vector2(3.0f, 0.5f),
                awning,
                BackSortOrder + 1);

            Sign(parent, floor, new Vector2(x, backY + 2.2f), title, awning, 20);
        }

        /// <summary>해변가 망루다. 라이프가드 반장이 그 앞에 선다.</summary>
        public static void LifeguardTower(
            Transform parent,
            int floor,
            float x,
            float localY)
        {
            Color wood = new Color(0.85f, 0.70f, 0.50f);

            Box(parent, "TowerLegLeft",
                FloorSpace.ToWorld(floor, new Vector2(x - 0.6f, localY + 1.2f)),
                new Vector2(0.14f, 2.4f),
                wood,
                BackSortOrder);

            Box(parent, "TowerLegRight",
                FloorSpace.ToWorld(floor, new Vector2(x + 0.6f, localY + 1.2f)),
                new Vector2(0.14f, 2.4f),
                wood,
                BackSortOrder);

            Box(parent, "TowerPlatform",
                FloorSpace.ToWorld(floor, new Vector2(x, localY + 2.5f)),
                new Vector2(1.8f, 0.3f),
                new Color(0.95f, 0.30f, 0.28f),
                BackSortOrder + 1);

            Sign(parent, floor, new Vector2(x, localY + 3.1f), "망루", new Color(1.00f, 0.55f, 0.50f), 20);
        }

        /// <summary>파라솔 천이다. 그늘 판정은 <see cref="ParasolShade"/>가 맡는다.</summary>
        public static void ParasolCanopy(
            Transform parent,
            Vector2 worldShadeCenter,
            Color canopy)
        {
            Box(parent, "ParasolPole",
                worldShadeCenter + new Vector2(0f, 0.9f),
                new Vector2(0.08f, 1.8f),
                new Color(0.90f, 0.90f, 0.90f),
                FrontSortOrder);

            // 캐릭터보다 위에 그려 "밑에 숨었다"는 느낌을 준다. 반투명이라 가려도 보인다.
            Blob(parent, "ParasolCanopy",
                worldShadeCenter + new Vector2(0f, 1.9f),
                new Vector2(3.0f, 0.9f),
                canopy,
                15000);
        }

        private static SpriteRenderer Create(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 worldCenter,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            GameObject go = new GameObject(name);

            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(worldCenter.x, worldCenter.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();

            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            return renderer;
        }
    }
}
