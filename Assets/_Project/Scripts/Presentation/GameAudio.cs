using UnityEngine;

namespace ProjectTheta.Presentation
{
    /// <summary>게임에서 쓰는 효과음 종류다. 정식 사운드로 교체할 때의 키가 된다.</summary>
    public enum GameSfx
    {
        /// <summary>UI 항목이 튀어나올 때의 짧은 "딱".</summary>
        UiTick,

        /// <summary>랭크 도장이 찍힐 때의 묵직한 소리.</summary>
        UiStamp,

        /// <summary>인기남의 중립 선점 단계 진행음.</summary>
        ClaimTick,

        /// <summary>업그레이드 구매음.</summary>
        Purchase
    }

    /// <summary>
    /// 효과음 재생 창구다.
    ///
    /// 9일차 인기남 선점음과 11일차 결과 화면 효과음이 각자 클립 생성 코드를
    /// 따로 갖고 있던 것을 여기로 합쳤다.
    /// 정식 오디오 파일이 준비되면 <see cref="LoadClip"/>만 바꾸면 되고,
    /// 호출부는 전부 그대로 둔다.
    /// </summary>
    public static class GameAudio
    {
        /// <summary>정식 사운드를 넣을 Resources 경로다.</summary>
        public const string ResourceFolder = "Audio";

        private static readonly AudioClip[] Clips =
            new AudioClip[4];

        private static AudioSource _source;

        public static void Play(
            GameSfx sfx,
            float volumeScale = 1f)
        {
            AudioSource source =
                ResolveSource();

            if (source == null)
            {
                return;
            }

            AudioClip clip =
                ResolveClip(
                    sfx);

            if (clip == null)
            {
                return;
            }

            source.PlayOneShot(
                clip,
                Mathf.Clamp(
                    volumeScale,
                    0f,
                    1f));
        }

        /// <summary>특정 오브젝트에서 소리를 내야 할 때 쓴다(위치가 의미 있는 경우).</summary>
        public static void PlayAt(
            AudioSource source,
            GameSfx sfx,
            float volumeScale = 1f)
        {
            if (source == null)
            {
                Play(
                    sfx,
                    volumeScale);

                return;
            }

            AudioClip clip =
                ResolveClip(
                    sfx);

            if (clip != null)
            {
                source.PlayOneShot(
                    clip,
                    Mathf.Clamp(
                        volumeScale,
                        0f,
                        1f));
            }
        }

        private static AudioClip ResolveClip(
            GameSfx sfx)
        {
            int index =
                (int)sfx;

            if (index < 0 ||
                index >= Clips.Length)
            {
                return null;
            }

            if (Clips[index] == null)
            {
                Clips[index] =
                    LoadClip(
                        sfx);
            }

            return Clips[index];
        }

        /// <summary>
        /// 정식 오디오 파일이 있으면 그것을 쓰고, 없으면 런타임 합성음으로 대체한다.
        /// 프로토타입 단계에서 오디오 자산 없이도 동작하게 하기 위한 구조다.
        /// </summary>
        private static AudioClip LoadClip(
            GameSfx sfx)
        {
            AudioClip fromResources =
                Resources.Load<AudioClip>(
                    ResourceFolder + "/" + sfx);

            if (fromResources != null)
            {
                return fromResources;
            }

            switch (sfx)
            {
                case GameSfx.UiStamp:
                    return RuntimeToneFactory.CreateTone(
                        "UiStamp",
                        190f,
                        0.22f,
                        0.30f,
                        2.2f);

                case GameSfx.ClaimTick:
                    return RuntimeToneFactory.CreateTone(
                        "ClaimTick",
                        880f,
                        0.055f,
                        0.16f,
                        3.0f);

                case GameSfx.Purchase:
                    return RuntimeToneFactory.CreateTone(
                        "Purchase",
                        660f,
                        0.10f,
                        0.22f,
                        3.0f);

                case GameSfx.UiTick:
                default:
                    return RuntimeToneFactory.CreateTone(
                        "UiTick",
                        1180f,
                        0.045f,
                        0.18f,
                        4.0f);
            }
        }

        private static AudioSource ResolveSource()
        {
            if (_source != null)
            {
                return _source;
            }

            GameObject holder =
                new GameObject(
                    "GameAudio");

            Object.DontDestroyOnLoad(
                holder);

            _source =
                holder.AddComponent<
                    AudioSource>();

            _source.playOnAwake =
                false;

            _source.spatialBlend =
                0f;

            return _source;
        }
    }
}
