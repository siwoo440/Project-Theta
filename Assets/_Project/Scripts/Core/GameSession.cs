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

        public bool HasPendingResult =>
            _hasPendingResult;

        public StageResultSummary PendingResult =>
            _pendingResult;

        /// <summary>
        /// 진행 중인 한 판이다 (21일차). 허브에서 출격할 때 만들고, 판이 끝나면 허브로 돌아갈 때 비운다.
        /// 구역마다 스테이지 씬을 다시 불러오므로 레벨 · 카드 · 경로는 여기에 둔다.
        /// </summary>
        public RunSession Run { get; private set; }

        /// <summary>새 판을 시작한다. 이전 판이 남아 있어도 버린다.</summary>
        public RunSession BeginRun()
        {
            Run =
                new RunSession(
                    System.Environment.TickCount);

            return Run;
        }

        /// <summary>
        /// 에디터에서 스테이지 씬을 바로 재생한 경우처럼 판 없이 스테이지에 들어왔을 때,
        /// 첫 구역(기업 연수원)을 고른 판을 만들어 준다.
        /// </summary>
        public RunSession EnsureRunForStage()
        {
            if (Run == null ||
                Run.IsFinished)
            {
                BeginRun();
            }

            if (Run.SelectedLocation == null)
            {
                System.Collections.Generic.List<ProjectTheta.Stage.Locations.LocationId> candidates =
                    Run.GetCandidates();

                if (candidates.Count > 0)
                {
                    Run.Select(
                        candidates[0]);
                }
            }

            return Run;
        }

        public void EndRun()
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
                _saveData =
                    SaveSystem.Load();
            }
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

        /// <summary>넘겨받은 결과를 세이브에 반영하고 비운다.</summary>
        public bool ConsumePendingResult()
        {
            if (!_hasPendingResult)
            {
                return false;
            }

            _saveData =
                SaveDataLogic.ApplyStageResult(
                    _saveData,
                    _pendingResult);

            _hasPendingResult =
                false;

            _pendingResult =
                StageResultSummary.Empty;

            SaveSystem.Save(
                _saveData);

            return true;
        }

        public void WriteSave()
        {
            SaveSystem.Save(
                _saveData);
        }

        /// <summary>현재 씬에서 목적지로 이동한다. 규칙에 없는 전환은 무시한다.</summary>
        public bool GoTo(
            SceneDestination destination)
        {
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

            SceneManager.LoadScene(
                SceneFlowLogic.GetSceneName(
                    destination));

            return true;
        }

        private void OnApplicationQuit()
        {
            WriteSave();
        }
    }
}
