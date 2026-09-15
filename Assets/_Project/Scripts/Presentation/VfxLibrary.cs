using UnityEngine;

namespace ProjectTheta.Presentation
{
    /// <summary>이펙트 스프라이트 종류다. 이름이 곧 Resources/Vfx 아래 파일 이름이다.</summary>
    public enum VfxSprite
    {
        SoftCircle = 0,
        Ring = 1,
        Spark = 2,
        Pillar = 3,
        Shard = 4,
        Heart = 5,
        EdgeVignette = 6
    }

    /// <summary>
    /// 이펙트 스프라이트를 불러온다.
    ///
    /// 14일차 <see cref="GameAudio"/>와 같은 규칙이다.
    ///   1. Resources/Vfx/이름 의 스프라이트가 있으면 그것을 쓴다
    ///   2. 스프라이트로 가져오지 못했으면 같은 이름의 텍스처로 스프라이트를 만든다
    ///   3. 둘 다 없으면 흰 사각형으로 대신한다 (화면이 깨지지는 않는다)
    ///
    /// 정식 에셋으로 바꿀 때는 같은 이름의 PNG를 덮어쓰기만 하면 된다.
    /// 19일차 임시 에셋은 Tools/generate_temp_assets.py가 만든다.
    /// </summary>
    public static class VfxLibrary
    {
        public const string ResourceFolder = "Vfx";

        private static readonly Sprite[] Cache =
            new Sprite[7];

        private static Sprite _fallback;

        public static Sprite Get(
            VfxSprite kind)
        {
            int index =
                (int)kind;

            if (index < 0 ||
                index >= Cache.Length)
            {
                return GetFallback();
            }

            if (Cache[index] != null)
            {
                return Cache[index];
            }

            string path =
                ResourceFolder + "/" + kind;

            Sprite sprite =
                Resources.Load<Sprite>(
                    path);

            if (sprite == null)
            {
                Texture2D texture =
                    Resources.Load<Texture2D>(
                        path);

                if (texture != null)
                {
                    // 가로세로 중 긴 쪽이 월드 1칸이 되게 한다. 크기는 코드에서 배율로 준다.
                    sprite =
                        Sprite.Create(
                            texture,
                            new Rect(
                                0f,
                                0f,
                                texture.width,
                                texture.height),
                            new Vector2(0.5f, 0.5f),
                            Mathf.Max(
                                texture.width,
                                texture.height));

                    sprite.name = kind.ToString();
                }
            }

            Cache[index] =
                sprite != null
                    ? sprite
                    : GetFallback();

            return Cache[index];
        }

        private static Sprite GetFallback()
        {
            if (_fallback != null)
            {
                return _fallback;
            }

            Texture2D texture =
                new Texture2D(
                    1,
                    1,
                    TextureFormat.RGBA32,
                    false);

            texture.SetPixel(
                0,
                0,
                Color.white);

            texture.Apply();

            _fallback =
                Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);

            return _fallback;
        }

        /// <summary>플레이를 끝내면 불러온 스프라이트가 파괴되므로 진입 시 비운다.</summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            System.Array.Clear(
                Cache,
                0,
                Cache.Length);

            _fallback = null;
        }
    }
}
