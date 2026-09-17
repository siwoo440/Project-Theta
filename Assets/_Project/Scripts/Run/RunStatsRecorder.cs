using System;
using System.IO;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Player;
using ProjectTheta.Stage;

namespace ProjectTheta.Run
{
    /// <summary>판 기록 파일 한 개의 내용이다.</summary>
    [Serializable]
    public sealed class RunLogEntry
    {
        public string Date;

        /// <summary>21일차: 장소와 구역 번호다. 구역마다 기록 파일이 하나씩 생긴다.</summary>
        public string Location;
        public int Zone;

        public string Result;

        /// <summary>33일차: 끝난 방식(Escaped · TimeUp · None)과 목표 · 목표 뒤 머문 시간이다.</summary>
        public string Exit;
        public int TargetEssence;
        public float SecondsAfterGoal;

        /// <summary>36일차: 심야 모드 도전인지다. 보고서에서 따로 묶는다.</summary>
        public bool NightMode;

        public string Difficulty;
        public bool TuningModified;
        public float PlayerHypnosisSpeedScale;
        public float FocusHypnosisSpeedBonus;
        public float FocusHypnosisDrainPerSecond;
        public float ImpulseBuildScale;
        public RunStats Stats;
    }

    /// <summary>
    /// 한 판 동안 <see cref="RunStats"/>를 모으고, 판이 끝나면 파일로 남긴다 (20일차).
    ///
    /// 게임 규칙 코드는 건드리지 않는다. 19일차에 만든 알림(<see cref="StageMoments"/>)과
    /// 레벨·층 이동 알림을 듣기만 한다.
    /// </summary>
    public sealed class RunStatsRecorder : MonoBehaviour
    {
        public const string LogFolderName = "RunLogs";

        private StageSessionController _stage;
        private FloorTransitionController _floors;
        private PlayerFocus _focus;
        private RunProgression _run;
        private Companion.FollowerManager _followers;
        private bool _finished;

        public RunStats Stats { get; private set; } =
            new RunStats(1);

        /// <summary>마지막으로 쓴 기록 파일 경로다. 아직 없으면 빈 문자열이다.</summary>
        public string LastLogPath { get; private set; } =
            string.Empty;

        public static string LogFolder =>
            Path.Combine(
                Application.persistentDataPath,
                LogFolderName);

        public void Configure(
            StageSessionController stage,
            FloorTransitionController floors,
            PlayerFocus focus,
            RunProgression run)
        {
            _stage = stage;
            _floors = floors;
            _focus = focus;
            _run = run;
            _followers = FindFirstObjectByType<Companion.FollowerManager>();

            Stats =
                new RunStats(
                    floors == null
                        ? 1
                        : floors.FloorCount);

            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            StageMoments.HypnosisSucceeded += HandleHypnosis;
            StageMoments.RecoveryConfirmed += HandleRecovery;
            StageMoments.RampageWindup += HandleWindup;
            StageMoments.RampageSurvived += HandleSurvived;
            StageMoments.CaptureStarted += HandleCapture;
            StageMoments.DuelWon += HandleDuelWon;
            StageMoments.FollowerStolen += HandleStolen;

            if (_run != null)
            {
                _run.LevelChanged += HandleLevelChanged;
            }
        }

        private void Unsubscribe()
        {
            StageMoments.HypnosisSucceeded -= HandleHypnosis;
            StageMoments.RecoveryConfirmed -= HandleRecovery;
            StageMoments.RampageWindup -= HandleWindup;
            StageMoments.RampageSurvived -= HandleSurvived;
            StageMoments.CaptureStarted -= HandleCapture;
            StageMoments.DuelWon -= HandleDuelWon;
            StageMoments.FollowerStolen -= HandleStolen;

            if (_run != null)
            {
                _run.LevelChanged -= HandleLevelChanged;
            }
        }

        private void Update()
        {
            if (_finished ||
                _stage == null)
            {
                return;
            }

            if (DebugCheats.UsedThisRun)
            {
                Stats.Cheated = true;
            }

            if (!_stage.IsRunning)
            {
                Finish();

                return;
            }

            if (_followers != null)
            {
                Stats.NoteFollowers(_followers.Count);
            }

            // 카드 화면에서는 timeScale이 0이라 deltaTime도 0이다. 멈춘 시간은 자연히 빠진다.
            Stats.Tick(
                Time.deltaTime,
                _floors == null
                    ? 0
                    : _floors.CurrentFloor,
                _focus != null &&
                _focus.IsExhausted);
        }

        private void Finish()
        {
            _finished = true;

            // 기록 파일은 개발 중에만 남긴다. 정식 빌드 사용자의 저장 공간에 쌓이지 않게 한다.
            if (!Debug.isDebugBuild)
            {
                return;
            }

            try
            {
                StageBalanceValues values =
                    BalanceOverrides.StageOrDefault;

                RunLogEntry entry =
                    new RunLogEntry
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Location = Stage.Locations.LocationContext.Current.Id.ToString(),
                        Zone = Stage.Locations.LocationContext.Current == null
                            ? 0
                            : RunRouteLogic.GetTier(Stage.Locations.LocationContext.Current.Id),
                        Result = _stage.State.ToString(),
                        Exit = _stage.ExitKind.ToString(),
                        TargetEssence = _stage.TargetEssence,
                        SecondsAfterGoal = _stage.SecondsAfterGoal,
                        NightMode = NightModeState.Active,
                        Difficulty = BalanceOverrides.Difficulty == null
                            ? "-"
                            : BalanceOverrides.Difficulty.Level.ToString(),
                        TuningModified = BalanceTuningSession.CountModified() > 0,
                        PlayerHypnosisSpeedScale = values.PlayerHypnosisSpeedScale,
                        FocusHypnosisSpeedBonus = values.FocusHypnosisSpeedBonus,
                        FocusHypnosisDrainPerSecond = values.FocusHypnosisDrainPerSecond,
                        ImpulseBuildScale = values.ImpulseBuildScale,
                        Stats = Stats
                    };

                Directory.CreateDirectory(
                    LogFolder);

                string path =
                    Path.Combine(
                        LogFolder,
                        DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");

                File.WriteAllText(
                    path,
                    JsonUtility.ToJson(
                        entry,
                        true));

                LastLogPath = path;
            }
            catch (Exception exception)
            {
                // 기록 실패가 결과 화면을 막으면 안 된다.
                Debug.LogWarning(
                    $"도전 기록 파일을 쓰지 못했습니다: {exception.Message}");
            }
        }

        private void HandleHypnosis(
            Vector2 position,
            bool wasReclaim)
        {
            Stats.RecordHypnosis(
                wasReclaim);
        }

        private void HandleRecovery(
            Vector2 position,
            int count,
            int essence)
        {
            Stats.RecordRecovery(
                count,
                essence);
        }

        private void HandleWindup(
            Vector2 position)
        {
            Stats.RampageWindups++;
        }

        private void HandleSurvived(
            Vector2 position)
        {
            Stats.RampageSurvived++;
        }

        private void HandleCapture(
            Vector2 position)
        {
            Stats.CaptureCount++;
        }

        private void HandleDuelWon(
            Vector2 position)
        {
            Stats.DuelWins++;
        }

        private void HandleStolen(
            Vector2 position)
        {
            Stats.StolenCount++;
        }

        private void HandleLevelChanged(
            int level)
        {
            Stats.RecordLevel(
                level);
        }
    }
}
