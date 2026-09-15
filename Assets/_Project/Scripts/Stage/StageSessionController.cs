using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Run;

namespace ProjectTheta.Stage
{
    public sealed class StageSessionController : MonoBehaviour
    {
        [Header("Stage")]
        [SerializeField] private float _timeLimitSeconds = 180f;
        [SerializeField] private int _targetEssence = 140;

        [Header("Essence Rewards")]
        [SerializeField] private int _rampageCaughtReward = 10;

        [Header("Recovery Batch")]
        [SerializeField] private float _recoveryBatchWindow =
            EssenceRecoveryLogic.BatchWindowSeconds;

        [Header("Capture Damage")]
        [SerializeField] private int _captureTickDamage = 1;
        [SerializeField] private int _captureMaxDamage = 10;

        private PlayerHealth _playerHealth;
        private FollowerManager _followers;
        private StageScoreTracker _scoreTracker;

        private int _pendingEssence;
        private int _pendingCount;
        private int _pendingRiskyCount;
        private int _pendingHighGradeCount;

        /// <summary>마지막으로 회수된 NPC의 자리다. 회수 연출(빛기둥)을 띄울 위치로 쓴다.</summary>
        private Vector2 _pendingPosition;
        private float _pendingElapsed;
        private bool _hasPendingBatch;

        public StageState State { get; private set; } =
            StageState.Running;

        public float RemainingTime { get; private set; }

        public int CurrentEssence { get; private set; }

        public int RampageCaptureCount { get; private set; }

        public int RecoveredFollowerCount { get; private set; }

        public int TargetEssence =>
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    _targetEssence *
                    PlayerUpgradeMultipliers.TargetEssence));

        /// <summary>정산 대기 중인 회수 인원이다. HUD가 현재 배율을 미리 보여주는 데 사용한다.</summary>
        public int PendingRecoveryCount =>
            _pendingCount;

        public int PendingRecoveryEssence =>
            _pendingEssence;

        public float PendingRecoveryMultiplier =>
            EssenceRecoveryLogic.GetSimultaneousMultiplier(
                _pendingCount);

        public bool HasPendingRecovery =>
            _hasPendingBatch;

        public int RampageCaughtReward =>
            Mathf.Max(
                0,
                _rampageCaughtReward);

        public int CaptureTickDamage =>
            Mathf.Max(
                1,
                _captureTickDamage);

        public int CaptureMaxDamage =>
            Mathf.Max(
                1,
                _captureMaxDamage);

        public bool IsRunning =>
            State == StageState.Running;

        public float EssenceNormalized =>
            Mathf.Clamp01(
                CurrentEssence /
                (float)TargetEssence);

        public float ElapsedTime =>
            Mathf.Max(
                0f,
                _timeLimitSeconds -
                RemainingTime);

        private void Awake()
        {
            RemainingTime =
                Mathf.Max(
                    0f,
                    _timeLimitSeconds *
                    PlayerUpgradeMultipliers.TimeLimit);

            CurrentEssence = 0;
            RampageCaptureCount = 0;
            RecoveredFollowerCount = 0;
            ClearPendingBatch();
            State = StageState.Running;

            _playerHealth =
                GetComponent<PlayerHealth>();

            _followers =
                GetComponent<FollowerManager>();

            _scoreTracker =
                GetComponent<StageScoreTracker>();
        }

        /// <summary>
        /// 장소별 제한 시간과 목표 정기를 적용한다 (21일차).
        /// 부트스트랩이 컴포넌트를 붙인 직후, 판이 흐르기 전에 부른다.
        /// </summary>
        public void ApplyObjective(
            float timeLimitSeconds,
            int targetEssence)
        {
            _timeLimitSeconds =
                Mathf.Max(
                    1f,
                    timeLimitSeconds);

            _targetEssence =
                Mathf.Max(
                    1,
                    targetEssence);

            RemainingTime =
                _timeLimitSeconds *
                PlayerUpgradeMultipliers.TimeLimit;
        }

        public void Configure(
            PlayerHealth playerHealth,
            FollowerManager followers)
        {
            _playerHealth =
                playerHealth;

            _followers =
                followers;

            if (_scoreTracker == null)
            {
                _scoreTracker =
                    GetComponent<StageScoreTracker>();
            }

            EvaluateState();
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            RemainingTime =
                StageRules.TickTime(
                    RemainingTime,
                    Time.deltaTime);

            UpdateRecoveryBatch();

            if (RemainingTime <= 0f)
            {
                // 제한 시간이 끝나는 순간 남은 묶음을 먼저 확정한다.
                FlushPendingBatch();
            }

            EvaluateState();
        }

        public void AddEssence(
            int amount)
        {
            if (!IsRunning)
            {
                return;
            }

            CurrentEssence =
                StageRules.AddEssence(
                    CurrentEssence,
                    amount,
                    TargetEssence);

            EvaluateState();
        }

        public void GrantRampageCaptureReward()
        {
            if (!IsRunning)
            {
                return;
            }

            RampageCaptureCount++;

            _scoreTracker?.ReportCapture();

            AddEssence(
                RampageCaughtReward);
        }

        public void RefreshState()
        {
            EvaluateState();
        }

        public bool TryRecoverFollower(
            FollowerController follower,
            FollowerManager followerManager)
        {
            if (!IsRunning ||
                follower == null ||
                followerManager == null)
            {
                return false;
            }

            ImpulseMeter impulse =
                follower.GetComponent<ImpulseMeter>();

            // 폭주 직전까지 끌고 온 NPC를 무사히 회수하면 위험 보너스를 준다.
            bool wasHighImpulse =
                impulse != null &&
                impulse.ImpulseNormalized >=
                    0.70f;

            impulse?.CancelForRecovery();

            if (!followerManager.ConsumeFollower(
                    follower))
            {
                return false;
            }

            RecoveredFollowerCount++;

            _pendingPosition =
                follower.transform.position;

            NpcProfile profile =
                follower.GetComponent<NpcProfile>();

            AddToPendingBatch(
                profile == null
                    ? NpcGradeTable.ReferenceEssenceValue
                    : profile.EssenceValue,
                wasHighImpulse,
                profile != null &&
                IsHighGrade(
                    profile.Grade));

            follower.gameObject.SetActive(
                false);

            EvaluateState();

            return true;
        }

        /// <summary>회수 지점에 들어온 NPC를 정산 묶음에 적립한다.</summary>
        private void AddToPendingBatch(
            int essenceValue,
            bool wasHighImpulse,
            bool isHighGrade)
        {
            if (!_hasPendingBatch)
            {
                _hasPendingBatch = true;
                _pendingElapsed = 0f;
            }

            _pendingEssence +=
                Mathf.Max(
                    0,
                    essenceValue);

            _pendingCount++;

            if (wasHighImpulse)
            {
                _pendingRiskyCount++;
            }

            if (isHighGrade)
            {
                _pendingHighGradeCount++;
            }
        }

        private void UpdateRecoveryBatch()
        {
            if (!_hasPendingBatch)
            {
                return;
            }

            _pendingElapsed +=
                Time.deltaTime;

            if (EssenceRecoveryLogic.IsBatchWindowClosed(
                    _pendingElapsed,
                    _recoveryBatchWindow))
            {
                FlushPendingBatch();
            }
        }

        /// <summary>정산 창을 닫고 동시 회수 배율을 적용해 정기를 확정한다.</summary>
        public void FlushPendingBatch()
        {
            if (!_hasPendingBatch ||
                _pendingCount <= 0)
            {
                ClearPendingBatch();

                return;
            }

            // 동시 회수 배율을 먼저 적용한 뒤, 런 강화 "정기 흡수"를 곱한다.
            int confirmed =
                Mathf.RoundToInt(
                    EssenceRecoveryLogic.ComputeBatchEssence(
                        _pendingEssence,
                        _pendingCount) *
                    RunUpgradeMultipliers.RecoveryEssence);

            int count =
                _pendingCount;

            int risky =
                _pendingRiskyCount;

            int highGrade =
                _pendingHighGradeCount;

            ClearPendingBatch();

            AddEssence(
                confirmed);

            if (_scoreTracker != null)
            {
                _scoreTracker.ReportRecovery(
                    confirmed,
                    count,
                    risky,
                    highGrade);
            }

            StageMoments.RaiseRecoveryConfirmed(
                _pendingPosition,
                count,
                confirmed);
        }

        private void ClearPendingBatch()
        {
            _hasPendingBatch = false;
            _pendingEssence = 0;
            _pendingCount = 0;
            _pendingRiskyCount = 0;
            _pendingHighGradeCount = 0;
            _pendingElapsed = 0f;
        }

        private static bool IsHighGrade(
            NpcGrade grade)
        {
            return grade == NpcGrade.Rare ||
                   grade == NpcGrade.Special ||
                   grade == NpcGrade.Awakened;
        }

        public string GetStateLabel()
        {
            switch (State)
            {
                case StageState.Cleared:
                    return "CLEAR";

                case StageState.FailedByTime:
                    return "FAILED - TIME";

                case StageState.FailedByHealth:
                    return "FAILED - HP";

                case StageState.Running:
                default:
                    return "RUNNING";
            }
        }

        private void EvaluateState()
        {
            int health =
                _playerHealth == null
                    ? 1
                    : _playerHealth.CurrentHealth;

            State =
                StageRules.ResolveState(
                    RemainingTime,
                    CurrentEssence,
                    TargetEssence,
                    health);
        }
    }
}
