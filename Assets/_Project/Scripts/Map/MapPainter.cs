using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Map
{
    /// <summary>
    /// 맵 꾸미기가 한 층에 도형을 그리는 도구다 (27일차).
    /// 좌표는 1층 기준(층 바닥 원점)이고, 층 높이와 층 명암은 여기서 입힌다.
    /// </summary>
    public sealed class MapPainter
    {
        private readonly Transform _parent;
        private readonly float _originY;
        private readonly float _shade;

        public MapPainter(
            Transform parent,
            int floor,
            int floorCount,
            float originY,
            float shade)
        {
            _parent = parent;
            _originY = originY;
            _shade = shade;
            Floor = floor;
            FloorCount = floorCount;
        }

        public int Floor { get; }

        public int FloorCount { get; }

        public bool IsTopFloor =>
            Floor == FloorCount - 1;

        /// <summary>바닥선 근처 물건의 정렬 값이다. 캐릭터와 같은 규칙(아래일수록 앞)이다.</summary>
        public static int CharacterOrder(
            float y)
        {
            return 1000 - Mathf.RoundToInt(y * 100f);
        }

        public SpriteRenderer Rect(
            string name,
            float x,
            float y,
            float width,
            float height,
            Color color,
            int sortingOrder)
        {
            GameObject visual = new GameObject(name);

            visual.transform.SetParent(_parent, false);
            visual.transform.position = new Vector3(x, y + _originY, 0f);
            visual.transform.localScale = new Vector3(width, height, 1f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();

            renderer.sprite = Stage.Locations.LocationProps.Solid;
            renderer.color = new Color(color.r * _shade, color.g * _shade, color.b * _shade, color.a);
            renderer.sortingOrder = sortingOrder;

            return renderer;
        }

        /// <summary>그림이 있으면 그 그림을 사각형 크기에 맞춰 깐다.</summary>
        public SpriteRenderer Art(
            string name,
            Sprite sprite,
            float x,
            float y,
            float width,
            float height,
            int sortingOrder)
        {
            SpriteRenderer renderer =
                Rect(name, x, y, width, height, Color.white, sortingOrder);

            renderer.sprite = sprite;

            Vector2 size = sprite.bounds.size;

            renderer.transform.localScale =
                new Vector3(
                    width / Mathf.Max(0.01f, size.x),
                    height / Mathf.Max(0.01f, size.y),
                    1f);

            return renderer;
        }

        /// <summary>바닥선 근처에 서는 물건이다. 캐릭터와 앞뒤가 맞게 정렬한다.</summary>
        public SpriteRenderer Prop(
            string name,
            float x,
            float y,
            float width,
            float height,
            Color color,
            int offset = 0)
        {
            return Rect(name, x, y, width, height, color, CharacterOrder(y - height * 0.5f) + offset);
        }

        /// <summary>
        /// 보이지 않는 충돌체다. 학교 복도의 벤치 · 자판기 자리와 크기를 그대로 지켜
        /// 장소가 바뀌어도 이동 판정이 같게 한다. 모양은 장소 꾸미기가 그린다.
        /// </summary>
        public void Collider(
            string name,
            float x,
            float y,
            float width,
            float height)
        {
            GameObject blocker = new GameObject(name);

            blocker.transform.SetParent(_parent, false);
            blocker.transform.position = new Vector3(x, y + _originY, 0f);
            blocker.transform.localScale = new Vector3(width, height, 1f);
            blocker.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        /// <summary>그라데이션 띠다. 아래에서 위로 색이 바뀐다.</summary>
        public void Gradient(
            string name,
            float centerX,
            float bottomY,
            float topY,
            float width,
            Color bottom,
            Color top,
            int bands,
            int sortingOrder)
        {
            int count = Mathf.Max(1, bands);
            float height = (topY - bottomY) / count;

            for (int i = 0;
                 i < count;
                 i++)
            {
                float t = count == 1 ? 0f : i / (float)(count - 1);

                Rect(
                    $"{name}_{i}",
                    centerX,
                    bottomY + height * (i + 0.5f),
                    width,
                    height + 0.02f,
                    Color.Lerp(bottom, top, t),
                    sortingOrder);
            }
        }

        public Text Label(
            string name,
            string text,
            float x,
            float y,
            int fontSize,
            Color color)
        {
            Text label =
                WorldLabel.Create(
                    _parent,
                    name,
                    new Vector2(x, y + _originY),
                    fontSize,
                    color);

            // 머리 위 글자보다 뒤, 캐릭터보다 뒤에 그린다. 간판은 배경이다.
            label.canvas.sortingOrder = -15;
            label.text = text;

            return label;
        }

        /// <summary>항상 같은 결과가 나오는 난수다. 같은 층은 매번 같은 무늬가 된다.</summary>
        public static float Hash(
            int a,
            int b)
        {
            unchecked
            {
                int h = a * 73856093 ^ b * 19349663;
                h ^= h >> 13;
                h *= 1274126177;

                return ((h & 0x7fffffff) % 10000) / 10000f;
            }
        }
    }
}
