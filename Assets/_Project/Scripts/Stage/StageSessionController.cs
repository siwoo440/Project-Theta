using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Impulse;
using ProjectTheta.Player;

namespace ProjectTheta.Stage
{
    public sealed class StageSessionController : MonoBehaviour
    {
        [Header("Stage")]
        [SerializeField] private float _timeLimitSeconds = 180f;
        [SerializeField] private int _targetEssence = 200;

        [Header("Essence Rewards")]
        [SerializeField] private int _recoveryReward = 5;
        [SerializeField] private int _rampageCaughtReward = 10;
        [SerializeField] private int _passiveEssencePerFollower = 1;
        [SerializeField] private float _passiveTickInterval = 1.0f;

        [Header("Capture Damage")]
        [SerializeField] private int _captureTickDamage = 1;
        [SerializeField] private int _captureMaxDamage = 10;

        private PlayerHealth _playerHealth;
        private FollowerManager _followers;
        private float _passiveTickTimer;

        public StageState State { get; private set; } =
            StageState.Running;

        public float RemainingTime { get; private set; }

        public int CurrentEssence { get; private set; }

        public int RampageCaptureCount { get; private set; }

        public int RecoveredFollowerCount { get; private set; }

        public int TargetEssence =>
            Mathf.Max(
                1,
                _targetEssence);

        public int PassiveEssencePerFollower =>
            Mathf.Max(
                0,
                _passiveEssencePerFollower);

        public int RecoveryReward =>
            Mathf.Max(
                0,
                _recoveryReward);

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

        public int PassiveProductionPerSecond =>
            StageRules.ComputeProductionPerSecond(
                _followers == null
                    ? 0
                    : _followers.Count,
                PassiveEssencePerFollower);

        private void Awake()
        {
            RemainingTime =
                Mathf.Max(
                    0f,
                    _timeLimitSeconds);

            CurrentEssence = 0;
            RampageCaptureCount = 0;
            RecoveredFollowerCount = 0;
            _passiveTickTimer = 0f;
            State = StageState.Running;

            _playerHealth =
                GetComponent<PlayerHealth>();

            _followers =
                GetComponent<FollowerManager>();
        }

        public void Configure(
            PlayerHealth playerHealth,
            FollowerManager followers)
        {
            _playerHealth =
                playerHealth;

            _followers =
                followers;

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

            EvaluateState();

            if (!IsRunning)
            {
                return;
            }

            UpdatePassiveProduction();
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

            impulse?.CancelForRecovery();

            if (!followerManager.ConsumeFollower(
                    follower))
            {
                return false;
            }

            RecoveredFollowerCount++;

            AddEssence(
                RecoveryReward);

            follower.gameObject.SetActive(
                false);

            EvaluateState();

            return true;
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

        private void UpdatePassiveProduction()
        {
            float interval =
                Mathf.Max(
                    0.05f,
                    _passiveTickInterval);

            _passiveTickTimer +=
                Time.deltaTime;

            while (_passiveTickTimer >=
                   interval)
            {
                _passiveTickTimer -=
                    interval;

                int production =
                    PassiveProductionPerSecond;

                if (production > 0)
                {
                    AddEssence(
                        production);
                }

                if (!IsRunning)
                {
                    break;
                }
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
