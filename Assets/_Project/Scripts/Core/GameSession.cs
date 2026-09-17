using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Run;
using ProjectTheta.Save;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 씬을 넘어 살아남는 유일한 오브젝트다.
    ///
    /// 영속 오브젝트를 여러 개 두면 초기화 순서 문제가 바로 생기므로
    /// 세이브 · 스테이지 결과 · 씬 전환을 전부 여기 하나에 모은다.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        private static GameSession _instance;

        private SaveData _saveData;
        private StageResultSummary _pendingResult =
            StageResultSummary.Empty;

        private bool _hasPendingResult;

        public static GameSession Instance =>
            _instance;

        public SaveData Save =>
            _saveData;

        // --- 34일차: 저장 칸 ---

        /// <summary>
        /// 지금 플레이 중인 저장 칸이다. 메인 메뉴에서 칸을 고르기 전에는 -1이다.
        /// 그 동안 <see cref="Save"/>는 가장 최근 칸을 미리 읽어 둔 것이라 메인 메뉴 기록 · 업적 창이 보여 준다.
        /// </summary>
        public int ActiveSlot { get; private set; } = -1;

        public bool HasActiveSlot =>
            SaveSlotLogic.IsValid(
                ActiveSlot);

        // 칸을 고르지 않았을 때 쓸 칸(가장 최근 칸, 없으면 1번 칸)이다.
        private int _previewSlot;

        public bool HasPendingResult =>
            _hasPendingResult;

        public StageResultSummary PendingResult =>
            _pendingResult;

        /// <summary>
        /// 지금 도전 중인 장소다 (29일차: 판 대신 장소 한 번 도전).
        /// 지도에서 출발할 때 새로 만들어, 레벨 · 카드는 장소마다 처음부터 시작한다.
        /// </summary>
        public RunSession Run { get; private set; }

        /// <summary>장소 도전을 새로 시작한다. 이전 도전이 남아 있어도 버린다.</summary>
        public RunSession BeginLocation(
            ProjectTheta.Stage.Locations.LocationId location,
            bool nightMode = false)
        {
            Run =
                new RunSession(
                    System.Environment.TickCount,
                    location)
                {
                    // 30일차: 숙련도 · 엔딩 해금은 출발할 때의 세이브로 정한다.
                    Stars =
                        MasteryLogic.GetStars(
                            _saveData,
                            (int)location),
                    StartChoiceCount =
                        MasteryLogic.GetStartChoiceCount(
                            _saveData),
                    // 36일차: 엔딩을 보지 않은 칸은 켜지지 않는다.
                    NightMode =
                        NightModeLogic.Resolve(
                            nightMode,
                            _saveData)
                };

            return Run;
        }

        /// <summary>
        /// 에디터에서 스테이지 씬을 바로 재생한 경우처럼 도전 없이 스테이지에 들어왔을 때,
        /// 기업 연수원 도전을 만들어 준다. 이미 기록을 마친 도전이면 같은 장소로 새로 만든다.
        /// </summary>
        public RunSession EnsureRunForStage()
        {
            if (Run == null)
            {
                BeginLocation(
                    ProjectTheta.Stage.Locations.LocationCatalog.StartLocation);
            }
            else if (Run.IsRecorded)
            {
                BeginLocation(
                    Run.Location,
                    Run.NightMode);
            }

            return Run;
        }

        /// <summary>도전을 비운다. 허브로 돌아갈 때 쓴다.</summary>
        public void ClearRun()
        {
            Run = null;
        }

        /// <summary>
        /// 도메인 리로드가 꺼져 있으면 지난 플레이에서 파괴된 세션을 계속 가리킨다.
        /// Unity 오브젝트의 == null 덕분에 지금은 우연히 안전하지만,
        /// ?. 연산자는 그 규칙을 따르지 않으므로 명시적으로 비운다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject sessionObject =
                new GameObject(
                    "GameSession");

            _instance =
                sessionObject.AddComponent<
                    GameSession>();

            DontDestroyOnLoad(
                sessionObject);
        }

        private void Awake()
        {
            if (_instance != null &&
                _instance != this)
            {
                Destroy(
                    gameObject);

                return;
            }

            _instance =
                this;

            DontDestroyOnLoad(
                gameObject);

            if (_saveData == null)
            {
                LoadPreview();
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            // 31일차: 음량 · 화면 · 커서 설정을 게임 시작 때 적용한다.
            SettingsApplier.Apply(
                _saveData);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        /// <summary>
        /// 게임을 켤 때: 옛 세이브를 옮기고, 공용 설정과 가장 최근 칸을 읽는다.
        /// 칸은 아직 고르지 않은 상태다(메인 메뉴에서 이어하기 · 처음부터로 고른다).
        /// </summary>
        private void LoadPreview()
        {
            SaveSystem.MigrateLegacyIfNeeded();

            SaveSlotSummary[] summaries =
                SaveSystem.GetSummaries();

            _previewSlot =
                SaveSlotLogic.GetFallbackSlot(
                    summaries);

            _saveData =
                SaveSystem.LoadSlot(
                    _previewSlot);

            SaveSlotLogic.ApplySettings(
                SaveSystem.LoadSettings(),
                _saveData);

            ActiveSlot = -1;
        }

        /// <summary>
        /// 에디터에서 허브 · 지도 · 스테이지 씬을 바로 재생하면 칸을 고를 수 없다.
        /// 그때는 미리 읽어 둔 칸(가장 최근 · 없으면 1번)으로 이어서 저장한다.
        /// </summary>
        private void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (HasActiveSlot ||
                scene.name == SceneNames.MainMenu ||
                scene.name == SceneNames.Boot)
            {
                return;
            }

            ActiveSlot =
                _previewSlot;

            Debug.Log(
                $"[GameSession] 칸을 고르지 않고 들어와 {SaveSlotLogic.GetSlotLabel(ActiveSlot)}을 씁니다.");
        }

        /// <summary>저장 칸 요약 3개다. 메인 메뉴 칸 목록이 쓴다.</summary>
        public SaveSlotSummary[] GetSlotSummaries()
        {
            return SaveSystem.GetSummaries();
        }

        /// <summary>이어하기: 저장된 칸을 불러온다. 설정은 공용 설정을 그대로 쓴다.</summary>
        public bool LoadSlot(
            int slot)
        {
            SaveData loaded =
                SaveSystem.TryLoadSlot(
                    slot);

            if (loaded == null)
            {
                return false;
            }

            SaveSlotLogic.ApplySettings(
                SaveSlotLogic.ExtractSettings(
                    _saveData),
                loaded);

            BeginSlot(
                slot,
                loaded);

            return true;
        }

        /// <summary>처음부터: 고른 칸을 새 게임으로 덮어쓰고 바로 저장한다.</summary>
        public bool NewGame(
            int slot)
        {
            if (!SaveSlotLogic.IsValid(slot))
            {
                return false;
            }

            BeginSlot(
                slot,
                SaveSlotLogic.CreateNewGame(
                    SaveSlotLogic.ExtractSettings(
                        _saveData)));

            return WriteSave();
        }

        /// <summary>허브의 저장 버튼이다. 칸을 고르지 않았으면 저장하지 않는다.</summary>
        public bool SaveNow()
        {
            return HasActiveSlot &&
                   WriteSave();
        }

        private void BeginSlot(
            int slot,
            SaveData data)
        {
            _saveData = data;
            ActiveSlot = slot;
            _previewSlot = slot;

            // 다른 칸의 도전 · 결과 · 업적 알림이 섞이지 않게 비운다.
            Run = null;
            _hasPendingResult = false;
            _pendingResult = StageResultSummary.Empty;
            LastResult = StageResultSummary.Empty;
            HasLastResult = false;
            _newAchievements.Clear();

            SettingsApplier.Apply(
                _saveData);
        }

        /// <summary>스테이지가 끝났을 때 결과를 넘겨둔다. 허브가 이것을 세이브에 반영한다.</summary>
        public void SubmitStageResult(
            StageResultSummary result)
        {
            _pendingResult =
                result;

            _hasPendingResult =
                true;
        }

        /// <summary>마지막으로 세이브에 반영한 결과다 (30일차). 허브의 "직전 도전" 카드가 쓴다.</summary>
        public StageResultSummary LastResult { get; private set; } =
            StageResultSummary.Empty;

        public bool HasLastResult { get; private set; }

        private readonly System.Collections.Generic.List<AchievementDefinition> _newAchievements =
            new System.Collections.Generic.List<AchievementDefinition>();

        /// <summary>아직 알리지 않은 새 업적을 꺼낸다. 꺼내면 비워진다.</summary>
        public System.Collections.Generic.List<AchievementDefinition> TakeNewAchievements()
        {
            System.Collections.Generic.List<AchievementDefinition> result =
                new System.Collections.Generic.List<AchievementDefinition>(
                    _newAchievements);

            _newAchievements.Clear();

            return result;
        }

        /// <summary>넘겨받은 결과를 세이브에 반영하고 비운다.</summary>
        public bool ConsumePendingResult()
        {
            if (!_hasPendingResult)
            {
                return false;
            }

            string[] before =
                _saveData == null ||
                _saveData.UnlockedAchievements == null
                    ? new string[0]
                    : (string[])_saveData.UnlockedAchievements.Clone();

            _saveData =
                SaveDataLogic.ApplyStageResult(
                    _saveData,
                    _pendingResult);

            _newAchievements.AddRange(
                AchievementLogic.Diff(
                    before,
                    _saveData.UnlockedAchievements));

            LastResult =
                _pendingResult;

            HasLastResult =
                true;

            _hasPendingResult =
                false;

            _pendingResult =
                StageResultSummary.Empty;

            WriteSave();

            return true;
        }

        /// <summary>
        /// 공용 설정은 늘 저장하고, 칸을 골랐으면 칸도 저장한다 (34일차).
        /// 칸 저장에 성공했는지를 돌려준다.
        /// </summary>
        public bool WriteSave()
        {
            if (_saveData == null)
            {
                return false;
            }

            SaveSystem.SaveSettings(
                SaveSlotLogic.ExtractSettings(
                    _saveData));

            if (!HasActiveSlot)
            {
                return false;
            }

            SaveSlotLogic.Stamp(
                _saveData,
                System.DateTime.Now);

            return SaveSystem.SaveSlot(
                ActiveSlot,
                _saveData);
        }

        /// <summary>현재 씬에서 목적지로 이동한다. 규칙에 없는 전환은 무시한다.</summary>
        public bool GoTo(
            SceneDestination destination)
        {
            // 28일차: 로딩창이 떠 있는 동안 들어온 전환은 무시한다(버튼 연타 방지).
            if (UI.LoadingScreen.IsLoading)
            {
                return false;
            }

            string currentScene =
                SceneManager.GetActiveScene().name;

            if (!SceneFlowLogic.CanTransition(
                    currentScene,
                    destination))
            {
                Debug.LogWarning(
                    $"[GameSession] 허용되지 않은 씬 전환입니다: {currentScene} → {destination}");

                return false;
            }

            if (SceneFlowLogic.ShouldSaveOnTransition(
                    currentScene,
                    destination))
            {
                ConsumePendingResult();
            }

            // 28일차: 로딩창을 띄우고 그 뒤에서 불러온다.
            return UI.LoadingScreen.Load(
                SceneFlowLogic.GetSceneName(
                    destination));
        }

        private void OnApplicationQuit()
        {
            WriteSave();
        }
    }
}
