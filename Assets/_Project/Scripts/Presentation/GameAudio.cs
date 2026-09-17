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
        Purchase,

        // --- 19일차 ---

        /// <summary>최면 성공. 부드러운 종소리.</summary>
        HypnosisSuccess,

        /// <summary>회수 확정. 화음이 차례로 쌓인다.</summary>
        Recovery,

        /// <summary>레벨업. 빠른 상승 아르페지오.</summary>
        LevelUp,

        /// <summary>폭주 회피. 바람 가르는 소리.</summary>
        Dodge,

        /// <summary>힘겨루기 승리. 묵직한 타격음.</summary>
        DuelWin,

        /// <summary>층 도착. 계단 발소리.</summary>
        FloorArrive,

        // --- 37일차 (Tools/generate_temp_audio.py) ---

        /// <summary>목표 달성 뒤 추격 시작. 두 음 경보.</summary>
        ChaseStart,

        /// <summary>탈출 가능. 밝은 3화음.</summary>
        ExitOpen,

        /// <summary>탈출 성공. 짧은 팡파르.</summary>
        Escape,

        /// <summary>실패. 내려가는 음.</summary>
        Fail,

        /// <summary>대사 글자. 아주 짧은 "톡". 말하는 사람마다 높이가 다르다.</summary>
        DialogueBlip,

        /// <summary>허브 서큐버스 말풍선.</summary>
        Bubble,

        /// <summary>창 열기 · 닫기.</summary>
        WindowOpen,
        WindowClose,

        /// <summary>저장.</summary>
        Save,

        /// <summary>심야 모드 켜기. 낮은 종.</summary>
        NightToggle,

        /// <summary>위기 화면 효과가 켜질 때의 낮은 경고음.</summary>
        Warning,

        /// <summary>업적 달성. 반짝이는 상승음.</summary>
        Achievement
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

        /// <summary>효과음 종류 수만큼 칸을 둔다. 종류를 추가해도 여기를 고칠 필요가 없다.</summary>
        private static readonly AudioClip[] Clips =
            new AudioClip[
                System.Enum.GetValues(
                    typeof(GameSfx)).Length];

        private static AudioSource _source;

        // 37일차: 높이를 바꿔 내는 소리(대사 글자)는 따로 둔다. 공용 소스 높이를 바꾸면 다른 소리까지 바뀐다.
        private static AudioSource _pitchedSource;

        /// <summary>효과음 크기 배율이다 (31일차, 전체 × 효과음 설정).</summary>
        public static float SfxVolume { get; set; } = 1f;

        /// <summary>음악 크기 배율이다 (31일차). 배경음악이 생기면 쓴다.</summary>
        public static float MusicVolume { get; set; } = 1f;

        /// <summary>
        /// 플레이를 끝내면 소스와 런타임에 만든 클립이 파괴된다.
        /// 도메인 리로드가 꺼져 있으면 참조만 남으므로 진입 시 비운다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _source = null;
            _pitchedSource = null;
            SfxVolume = 1f;
            MusicVolume = 1f;

            System.Array.Clear(
                Clips,
                0,
                Clips.Length);
        }

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
                    1f) *
                Mathf.Clamp01(SfxVolume));
        }

        /// <summary>높이를 바꿔 낸다(37일차, 대사 글자 소리).</summary>
        public static void PlayPitched(
            GameSfx sfx,
            float volumeScale,
            float pitch)
        {
            if (ResolveSource() == null)
            {
                return;
            }

            if (_pitchedSource == null)
            {
                _pitchedSource =
                    _source.gameObject.AddComponent<AudioSource>();

                _pitchedSource.playOnAwake = false;
                _pitchedSource.spatialBlend = 0f;
            }

            AudioClip clip =
                ResolveClip(
                    sfx);

            if (clip == null)
            {
                return;
            }

            _pitchedSource.pitch =
                Mathf.Clamp(
                    pitch,
                    0.5f,
                    2f);

            _pitchedSource.PlayOneShot(
                clip,
                Mathf.Clamp(
                    volumeScale,
                    0f,
                    1f) *
                Mathf.Clamp01(SfxVolume));
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
                        1f) *
                    Mathf.Clamp01(SfxVolume));
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

                // 19일차 효과음: Resources/Audio에 파일이 있으면 위에서 이미 반환된다.
                // 파일이 빠졌을 때를 대비한 합성음이다.
                case GameSfx.HypnosisSuccess:
                    return RuntimeToneFactory.CreateTone(
                        "HypnosisSuccess",
                        660f,
                        0.35f,
                        0.24f,
                        1.6f);

                case GameSfx.Recovery:
                    return RuntimeToneFactory.CreateTone(
                        "Recovery",
                        784f,
                        0.45f,
                        0.24f,
                        1.2f);

                case GameSfx.LevelUp:
                    return RuntimeToneFactory.CreateTone(
                        "LevelUp",
                        1047f,
                        0.40f,
                        0.24f,
                        1.4f);

                case GameSfx.Dodge:
                    return RuntimeToneFactory.CreateTone(
                        "Dodge",
                        1480f,
                        0.12f,
                        0.16f,
                        3.0f);

                case GameSfx.DuelWin:
                    return RuntimeToneFactory.CreateTone(
                        "DuelWin",
                        110f,
                        0.30f,
                        0.34f,
                        2.0f);

                case GameSfx.FloorArrive:
                    return RuntimeToneFactory.CreateTone(
                        "FloorArrive",
                        180f,
                        0.10f,
                        0.26f,
                        4.0f);

                // 37일차 효과음: 파일이 빠졌을 때의 대체음이다.
                case GameSfx.ChaseStart:
                case GameSfx.Warning:
                    return RuntimeToneFactory.CreateTone(
                        sfx.ToString(),
                        240f,
                        0.30f,
                        0.30f,
                        2.0f);

                case GameSfx.ExitOpen:
                case GameSfx.Escape:
                case GameSfx.Achievement:
                case GameSfx.Save:
                    return RuntimeToneFactory.CreateTone(
                        sfx.ToString(),
                        988f,
                        0.30f,
                        0.24f,
                        1.8f);

                case GameSfx.Fail:
                case GameSfx.NightToggle:
                    return RuntimeToneFactory.CreateTone(
                        sfx.ToString(),
                        220f,
                        0.40f,
                        0.26f,
                        1.6f);

                case GameSfx.DialogueBlip:
                case GameSfx.Bubble:
                case GameSfx.WindowOpen:
                case GameSfx.WindowClose:
                    return RuntimeToneFactory.CreateTone(
                        sfx.ToString(),
                        620f,
                        0.04f,
                        0.16f,
                        4.0f);

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
