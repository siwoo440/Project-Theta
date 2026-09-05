using UnityEngine;
using ProjectTheta.Companion;
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

        [Header("Rampage Penalty")]
        [SerializeField] private int _rampageCaughtDamage = 25;

        private PlayerHealth _playerHealth;

        public StageState State { get; private set; } =
            StageState.Running;

        public float RemainingTime { get; private set; }

        public int CurrentEssence { get; private set; }

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

        public int RampageCaughtDamage =>
            Mathf.Max(
                0,
                _rampageCaughtDamage);

        public bool IsRunning =>
            State == StageState.Running;

        public float EssenceNormalized =>
            Mathf.Clamp01(
                CurrentEssence /
                (float)TargetEssence);

        private void Awake()
        {
            RemainingTime =
                Mathf.Max(
                    0f,
                    _timeLimitSeconds);

            CurrentEssence = 0;
            State = StageState.Running;

            _playerHealth =
                GetComponent<PlayerHealth>();
        }

        public void Configure(
            PlayerHealth playerHealth)
        {
            _playerHealth =
                playerHealth;

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

        public void HandleRampageCatch()
        {
            if (!IsRunning)
            {
                return;
            }

            CurrentEssence =
                StageRules.AddEssence(
                    CurrentEssence,
                    RampageCaughtReward,
                    TargetEssence);

            if (_playerHealth != null)
            {
                _playerHealth.TakeDamage(
                    RampageCaughtDamage);
            }

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

            if (!followerManager.ConsumeFollower(
                    follower))
            {
                return false;
            }

            CurrentEssence =
                StageRules.AddEssence(
                    CurrentEssence,
                    RecoveryReward,
                    TargetEssence);

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
