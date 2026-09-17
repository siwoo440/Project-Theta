using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// F3 성능 표시 창이다 (38일차). 씬을 넘어 하나만 산다.
    ///
    ///   왼쪽 위 작은 창: FPS · 평균 ms · 1% 느린 FPS · 최장 프레임 · GC · 메모리 · (장소 안) NPC · 방해자 수
    ///   색: 1% 느린 FPS 30 미만 노랑, 20 미만 빨강
    ///
    /// 창을 끈 동안에도 프레임 시간은 모은다(켜자마자 값이 보이게). 글자는 켜져 있을 때만 쓴다.
    /// 개발 빌드에서는 도전 하나가 끝날 때마다 성능 요약 한 줄을 RunLogs/perf_log.txt에 남긴다.
    /// </summary>
    public sealed class PerfOverlay : MonoBehaviour
    {
        public const string PerfLogFileName = "perf_log.txt";

        private static PerfOverlay _instance;

        private readonly PerfSampler _sampler = new PerfSampler();
        private readonly PerfRunStats _run = new PerfRunStats();

        private GameObject _root;
        private Text _text;
        private Image _edge;
        private float _refresh;
        private float _countCooldown;
        private int _npcs = -1;
        private int _disruptors;
        private int _gcStart;
        private int _runGcStart;

        private StageSessionController _stage;
        private float _findCooldown;
        private bool _wasRunning;

        /// <summary>표시 창이 켜져 있는지다. 한 번 켜면 씬을 넘어도 유지된다.</summary>
        public static bool Visible { get; private set; }

        /// <summary>마지막으로 성능 기록을 쓴 경로다(확인용).</summary>
        public static string LastLogPath { get; private set; } = string.Empty;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _instance = null;
            Visible = false;
            LastLogPath = string.Empty;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject host = new GameObject("PerfOverlay");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<PerfOverlay>();
        }

        private void Awake()
        {
            _gcStart = GC.CollectionCount(0);
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

        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            // 로딩 프레임이 최장 프레임을 차지하지 않게 새로 모은다.
            _sampler.Clear();
            _stage = null;
            _findCooldown = 0f;
            _wasRunning = false;
            _npcs = -1;
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;

            _sampler.Add(delta);

            if (ReadTogglePressed())
            {
                SetVisible(!Visible);
            }

            UpdateRun(delta);

            if (!Visible)
            {
                return;
            }

            _countCooldown -= delta;

            if (_countCooldown <= 0f)
            {
                _countCooldown = 1f;
                CountActors();
            }

            _refresh -= delta;

            if (_refresh > 0f)
            {
                return;
            }

            _refresh = PerfLogic.RefreshSeconds;
            Redraw();
        }

        private void SetVisible(
            bool visible)
        {
            Visible = visible;

            if (visible &&
                _root == null)
            {
                Build();
            }

            if (_root != null)
            {
                _root.SetActive(visible);
            }

            _refresh = 0f;
            _countCooldown = 0f;
        }

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "PerfCanvas",
                    500,
                    transform);

            // 클릭을 받지 않는다. 게임 조작을 가로채지 않게 한다.
            Destroy(canvas.GetComponent<GraphicRaycaster>());

            _root = canvas.gameObject;

            RectTransform panel =
                UiFactory.CreatePanel(
                    canvas.transform,
                    "PerfPanel",
                    new Color(0.04f, 0.03f, 0.07f, 0.78f),
                    UiTheme.Positive,
                    1f);

            _edge = panel.GetComponent<Image>();

            UiFactory.Place(
                panel,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(12f, -12f),
                new Vector2(300f, 96f));

            Text title =
                UiFactory.CreateText(
                    panel,
                    "Title",
                    "성능 (F3)",
                    UiTheme.FontTiny,
                    UiTheme.TextDisabled,
                    TextAnchor.UpperRight);

            UiFactory.Place(
                title.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-8f, -5f),
                new Vector2(120f, 16f));

            _text =
                UiFactory.CreateText(
                    panel,
                    "Values",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft);

            _text.lineSpacing = 1.05f;

            UiFactory.Place(
                _text.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(10f, -6f),
                new Vector2(280f, 84f));
        }

        private void Redraw()
        {
            if (_text == null)
            {
                return;
            }

            PerfSnapshot snapshot = _sampler.Compute();

            snapshot.GcCount = GC.CollectionCount(0) - _gcStart;
            snapshot.ManagedBytes = GC.GetTotalMemory(false);
            snapshot.Npcs = _npcs;
            snapshot.Disruptors = _disruptors;

            _text.text = PerfLogic.Format(snapshot);

            Color color = GetGradeColor(PerfLogic.GetGrade(snapshot.LowFps));

            _text.color = color;

            if (_edge != null)
            {
                _edge.color = color;
            }

            // NPC 줄이 없으면 창을 한 줄 줄인다.
            RectTransform panel = (RectTransform)_text.transform.parent;
            panel.sizeDelta = new Vector2(300f, _npcs >= 0 ? 96f : 76f);
        }

        private static Color GetGradeColor(
            PerfGrade grade)
        {
            switch (grade)
            {
                case PerfGrade.Bad:
                    return UiTheme.Danger;

                case PerfGrade.Warn:
                    return UiTheme.Gold;

                default:
                    return UiTheme.Positive;
            }
        }

        private void CountActors()
        {
            if (!SceneFlowLogic.IsStageScene(SceneManager.GetActiveScene().name))
            {
                _npcs = -1;
                _disruptors = 0;
                return;
            }

            _npcs = FindObjectsByType<NPC.NpcAgent>(FindObjectsSortMode.None).Length;
            _disruptors = FindObjectsByType<Disruptors.DisruptorBase>(FindObjectsSortMode.None).Length;
        }

        // 개발 빌드 도전별 기록 --------------------------------------------

        private void UpdateRun(
            float delta)
        {
            if (!SceneFlowLogic.IsStageScene(SceneManager.GetActiveScene().name))
            {
                return;
            }

            if (_stage == null)
            {
                _findCooldown -= delta;

                if (_findCooldown > 0f)
                {
                    return;
                }

                _findCooldown = 1f;
                _stage = FindFirstObjectByType<StageSessionController>();

                if (_stage == null)
                {
                    return;
                }
            }

            bool running = _stage.IsRunning;

            if (running &&
                !_wasRunning)
            {
                _run.Reset();
                _runGcStart = GC.CollectionCount(0);
            }

            // 카드 · 대사 화면처럼 멈춘 시간은 빼고 실제 플레이만 잰다.
            if (running &&
                Time.timeScale > 0f)
            {
                _run.Add(delta);
            }

            if (!running &&
                _wasRunning)
            {
                _run.GcCount = GC.CollectionCount(0) - _runGcStart;
                WriteRunLog();
            }

            _wasRunning = running;
        }

        private void WriteRunLog()
        {
            // 도전 기록(RunStatsRecorder)과 같이 개발 중에만 남긴다.
            if (!Debug.isDebugBuild ||
                _run.Frames == 0)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(RunStatsRecorder.LogFolder);

                string path = Path.Combine(RunStatsRecorder.LogFolder, PerfLogFileName);

                File.AppendAllText(
                    path,
                    PerfLogic.FormatRunLine(
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Stage.Locations.LocationContext.Current == null
                            ? "-"
                            : Stage.Locations.LocationContext.Current.Id.ToString(),
                        NightModeState.Active,
                        _run) +
                    Environment.NewLine);

                LastLogPath = path;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"성능 기록을 쓰지 못했습니다: {exception.Message}");
            }
        }

        private static bool ReadTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null &&
                   Keyboard.current.f3Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F3);
#endif
        }
    }
}
