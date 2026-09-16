using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Map
{
    /// <summary>
    /// 바닥 무늬 사각형 목록을 텍스처 한 장에 굽는다 (28일차).
    /// 한 번 구운 장은 장소(와 층) 단위로 캐시해, 구역을 다시 들어가도 새로 굽지 않는다.
    /// </summary>
    public static class FloorPatternBaker
    {
        public const int PixelsPerUnit = 24;

        private static readonly Dictionary<string, Sprite> Cache =
            new Dictionary<string, Sprite>();

        /// <summary>캐시 열쇠다. 층과 무관한 무늬는 모든 층이 한 장을 나눠 쓴다.</summary>
        public static string GetKey(
            LocationId location,
            FloorPattern pattern,
            int floor)
        {
            return FloorPatternLayout.DependsOnFloor(pattern)
                ? $"{location}/{floor}"
                : $"{location}";
        }

        public static int TextureWidth =>
            Mathf.CeilToInt((FloorPatternLayout.AreaMaxX - FloorPatternLayout.AreaMinX) * PixelsPerUnit);

        public static int TextureHeight =>
            Mathf.CeilToInt(FloorPatternLayout.AreaHeight * PixelsPerUnit);

        public static Sprite Get(
            MapTheme theme,
            int floor)
        {
            string key = GetKey(theme.Location, theme.Pattern, floor);

            if (Cache.TryGetValue(key, out Sprite cached) &&
                cached != null)
            {
                return cached;
            }

            List<PatternRect> rects = FloorPatternLayout.Build(theme, floor);

            if (rects.Count == 0)
            {
                return null;
            }

            Sprite sprite = Bake(rects, key);
            Cache[key] = sprite;

            return sprite;
        }

        private static Sprite Bake(
            List<PatternRect> rects,
            string key)
        {
            int width = TextureWidth;
            int height = TextureHeight;
            Color32[] pixels = new Color32[width * height];

            for (int i = 0; i < rects.Count; i++)
            {
                Paint(pixels, width, height, rects[i]);
            }

            Texture2D texture =
                new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = "FloorPattern_" + key,
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Sprite sprite =
                Sprite.Create(
                    texture,
                    new Rect(0f, 0f, width, height),
                    new Vector2(0.5f, 0.5f),
                    PixelsPerUnit);

            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.DontSave;

            return sprite;
        }

        /// <summary>사각형을 반투명 합성(over)으로 칠한다. 가는 선도 최소 1px은 남긴다.</summary>
        private static void Paint(
            Color32[] pixels,
            int width,
            int height,
            PatternRect rect)
        {
            float left = (rect.X - rect.Width * 0.5f - FloorPatternLayout.AreaMinX) * PixelsPerUnit;
            float bottom = (rect.Y - rect.Height * 0.5f - FloorPatternLayout.AreaMinY) * PixelsPerUnit;

            int x0 = Mathf.Clamp(Mathf.FloorToInt(left), 0, width);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(bottom), 0, height);
            int x1 = Mathf.Clamp(Mathf.Max(Mathf.FloorToInt(left) + 1, Mathf.RoundToInt(left + rect.Width * PixelsPerUnit)), 0, width);
            int y1 = Mathf.Clamp(Mathf.Max(Mathf.FloorToInt(bottom) + 1, Mathf.RoundToInt(bottom + rect.Height * PixelsPerUnit)), 0, height);

            Color source = rect.Color;
            float alpha = Mathf.Clamp01(source.a);

            for (int y = y0; y < y1; y++)
            {
                int row = y * width;

                for (int x = x0; x < x1; x++)
                {
                    Color destination = pixels[row + x];
                    float outAlpha = alpha + destination.a * (1f - alpha);

                    if (outAlpha <= 0f)
                    {
                        continue;
                    }

                    Color result =
                        new Color(
                            (source.r * alpha + destination.r * destination.a * (1f - alpha)) / outAlpha,
                            (source.g * alpha + destination.g * destination.a * (1f - alpha)) / outAlpha,
                            (source.b * alpha + destination.b * destination.a * (1f - alpha)) / outAlpha,
                            outAlpha);

                    pixels[row + x] = result;
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            foreach (Sprite sprite in Cache.Values)
            {
                if (sprite == null)
                {
                    continue;
                }

                Object.Destroy(sprite.texture);
                Object.Destroy(sprite);
            }

            Cache.Clear();
        }
    }
}
