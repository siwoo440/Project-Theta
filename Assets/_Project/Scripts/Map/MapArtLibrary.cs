using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Map
{
    /// <summary>맵 그림으로 바꿀 수 있는 층위다.</summary>
    public enum MapArtLayer
    {
        Background = 0,
        Floor = 1
    }

    /// <summary>
    /// 맵 그림 교체 자리다 (27일차). 효과 이미지(<c>VfxLibrary</c>)와 같은 규칙이다.
    ///
    ///   Resources/Maps/{장소}/{층위} 스프라이트(또는 텍스처)가 있으면 그 그림을 늘려 깐다.
    ///   없으면 null — 맵 빌더가 도형으로 그린다.
    /// 예: Resources/Maps/Beach/Background.png
    /// </summary>
    public static class MapArtLibrary
    {
        private const string ResourceFolder = "Maps";

        private static readonly Dictionary<string, Sprite> Cache =
            new Dictionary<string, Sprite>();

        public static string GetPath(
            LocationId location,
            MapArtLayer layer)
        {
            return $"{ResourceFolder}/{location}/{layer}";
        }

        public static Sprite TryGet(
            LocationId location,
            MapArtLayer layer)
        {
            string path = GetPath(location, layer);

            if (Cache.TryGetValue(path, out Sprite cached))
            {
                return cached;
            }

            Sprite sprite = Resources.Load<Sprite>(path);

            if (sprite == null)
            {
                Texture2D texture = Resources.Load<Texture2D>(path);

                if (texture != null)
                {
                    // 가로세로 중 긴 쪽이 월드 1칸이 되게 한다. 크기는 빌더가 배율로 준다.
                    sprite =
                        Sprite.Create(
                            texture,
                            new Rect(0f, 0f, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f),
                            Mathf.Max(texture.width, texture.height));

                    sprite.name = path;
                }
            }

            Cache[path] = sprite;

            return sprite;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Cache.Clear();
        }
    }
}
