using UnityEngine;
using UnityEngine.SceneManagement;
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
