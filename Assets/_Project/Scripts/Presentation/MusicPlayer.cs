using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 배경음악 재생기다 (37일차). 씬을 넘어 하나만 산다.
    ///
    ///   소스 A · B : 곡을 바꿀 때 서로 크로스페이드(1.2초)
    ///   긴장 소스  : 같은 곡의 1마디 타악기 겹. 추격 · 위기일 때 페이드 인, 곡 박자에 맞춰 시작
    ///
    /// 어떤 곡 · 얼마나 긴장인지는 <see cref="MusicLogic"/>이 정한다.
    /// 음량은 설정의 음악 음량(<see cref="GameAudio.MusicVolume"/>)을 따르고, 대사 중에는 줄인다.
    /// 파일이 없는 곡은 조용히 넘어간다.
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        private static MusicPlayer _instance;

        private AudioSource _current;
        private AudioSource _previous;
        private AudioSource _tension;

        private MusicTrack _track = MusicTrack.None;
        private float _currentFade;
        private float _previousFade;
        private float _tensionLevel;

        private StageSessionController _stage;
        private PlayerHealth _health;
        private float _findCooldown;

        /// <summary>지금 틀고 있는 곡이다(확인 · 디버그용).</summary>
        public static MusicTrack CurrentTrack =>
            _instance == null
                ? MusicTrack.None
                : _instance._track;

        public static MusicIntensity CurrentIntensity { get; private set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _instance = null;
            CurrentIntensity = MusicIntensity.Calm;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject host = new GameObject("MusicPlayer");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<MusicPlayer>();
        }

        private void Awake()
        {
            _current = CreateSource("MusicA");
            _previous = CreateSource("MusicB");
            _tension = CreateSource("MusicTension");

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (_instance == this)
            {
                _instance = null;
            }
        }

        private AudioSource CreateSource(
            string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);

            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;

            return source;
        }

        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            _stage = null;
            _health = null;
            _findCooldown = 0f;
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;

            UpdateTrack();
            UpdateStageRefs(delta);

            bool running = _stage != null && _stage.IsRunning;
            bool night = running && Run.NightModeState.Active;

            MusicIntensity intensity =
                MusicLogic.GetIntensity(
                    running,
                    Disruptors.DisruptorSpawner.Current != null &&
                    Disruptors.DisruptorSpawner.Current.IsChasing,
                    _stage == null ? 999f : _stage.RemainingTime,
                    _health == null ? 1f : _health.HealthNormalized);

            CurrentIntensity = intensity;

            // 크로스페이드
            _currentFade = MusicLogic.Step(_currentFade, 1f, delta, MusicLogic.CrossfadeSeconds);
            _previousFade = MusicLogic.Step(_previousFade, 0f, delta, MusicLogic.CrossfadeSeconds);
            _tensionLevel = MusicLogic.Step(_tensionLevel, MusicLogic.GetTensionVolume(intensity), delta, MusicLogic.TensionFadeSeconds);

            bool dialogue = UI.DialogueOverlay.Busy;
            float setting = GameAudio.MusicVolume;
            float pitch = MusicLogic.GetPitch(night, intensity);

            _current.volume = MusicLogic.GetVolume(setting, dialogue, _currentFade);
            _previous.volume = MusicLogic.GetVolume(setting, dialogue, _previousFade);
            _tension.volume = MusicLogic.GetVolume(setting, dialogue, _tensionLevel);

            _current.pitch = pitch;
            _tension.pitch = pitch;

            if (_previousFade <= 0f &&
                _previous.isPlaying)
            {
                _previous.Stop();
            }

            KeepTensionInStep();
        }

        private void UpdateTrack()
        {
            MusicTrack wanted =
                MusicLogic.GetTrack(
                    SceneManager.GetActiveScene().name,
                    LocationContext.Current == null
                        ? LocationCatalog.StartLocation
                        : LocationContext.Current.Id,
                    Boss.EndingSequence.IsPlaying);

            if (!MusicLogic.ShouldSwitch(_track, wanted))
            {
                return;
            }

            _track = wanted;

            // 지금 곡을 "이전"으로 넘기고 새 곡을 페이드 인한다.
            AudioSource swap = _previous;
            _previous = _current;
            _current = swap;

            _previousFade = _currentFade;
            _currentFade = 0f;

            AudioClip clip = Resources.Load<AudioClip>(MusicLogic.GetPath(wanted));

            _current.Stop();
            _current.clip = clip;

            if (clip != null)
            {
                _current.Play();
            }

            AudioClip tension = Resources.Load<AudioClip>(MusicLogic.GetTensionPath(wanted));

            _tension.Stop();
            _tension.clip = tension;

            if (tension != null)
            {
                _tension.Play();
                _tensionLevel = 0f;
            }
        }

        /// <summary>긴장 겹이 곡 박자에서 어긋나면 다시 맞춘다(한 마디 기준).</summary>
        private void KeepTensionInStep()
        {
            if (_tension.clip == null ||
                _current.clip == null ||
                !_current.isPlaying ||
                !_tension.isPlaying)
            {
                return;
            }

            int expected =
                MusicLogic.GetTensionSample(
                    _current.timeSamples,
                    _tension.clip.samples);

            int drift = Mathf.Abs(_tension.timeSamples - expected);

            // 반복 경계에서 튀는 값은 무시하고, 50ms 넘게 어긋났을 때만 맞춘다.
            if (drift > _tension.clip.frequency / 20 &&
                drift < _tension.clip.samples - _tension.clip.frequency / 20)
            {
                _tension.timeSamples = expected;
            }
        }

        private void UpdateStageRefs(
            float delta)
        {
            if (_stage != null ||
                !SceneFlowLogicIsStage())
            {
                return;
            }

            _findCooldown -= delta;

            if (_findCooldown > 0f)
            {
                return;
            }

            _findCooldown = 1f;
            _stage = FindFirstObjectByType<StageSessionController>();
            _health = _stage == null ? null : _stage.GetComponent<PlayerHealth>();
        }

        private static bool SceneFlowLogicIsStage()
        {
            return Core.SceneFlowLogic.IsStageScene(
                SceneManager.GetActiveScene().name);
        }
    }
}
