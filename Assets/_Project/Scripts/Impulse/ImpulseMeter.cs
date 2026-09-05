using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;

namespace ProjectTheta.Impulse
{
    [RequireComponent(typeof(HypnosisTarget))]
    [RequireComponent(typeof(FollowerController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ImpulseMeter : MonoBehaviour
    {
        [Header("Charge")]
        [SerializeField] private float _maximumImpulse = 100f;
        [SerializeField] private float _buildPerSecond = 3.0f;
        [SerializeField] private float _warningThreshold = 65f;
        [SerializeField] private float _dangerThreshold = 85f;

        [Header("Rampage")]
        [SerializeField] private float _prepareDuration = 1.0f;
        [SerializeField] private float _rampageDuration = 1.8f;
        [SerializeField] private float _recoveryDuration = 1.1f;
        [SerializeField] private float _rampageSpeed = 8.4f;
        [SerializeField] private float _catchDistance = 0.55f;
        [SerializeField] private float _postRecoveryImpulse = 20f;

        [Header("Variation")]
        [SerializeField] private float _buildRateVariation = 0.15f;

        private HypnosisTarget _target;
        private FollowerController _follower;
        private Rigidbody2D _body;
        private RuntimeCharacterSpriteAnimator _animator;

        private Transform _player;
        private RampageCoordinator _coordinator;

        private float _buildRateMultiplier = 1f;
        private float _phaseRemaining;

        public float CurrentImpulse { get; private set; }

        public float ImpulseNormalized =>
            Mathf.Clamp01(
                CurrentImpulse /
                Mathf.Max(
                    1f,
                    _maximumImpulse));

        public ImpulseState State { get; private set; } =
            ImpulseState.Calm;

        public bool IsWarningIconVisible =>
            State == ImpulseState.Preparing ||
            State == ImpulseState.Rampaging;

        public bool IsFollowingActive =>
            _target != null &&
            _target.IsFollowing;

        private void Awake()
        {
            _target =
                GetComponent<HypnosisTarget>();

            _follower =
                GetComponent<FollowerController>();

            _body =
                GetComponent<Rigidbody2D>();

            _animator =
                GetComponent<
                    RuntimeCharacterSpriteAnimator>();

            _buildRateMultiplier =
                Random.Range(
                    1f - _buildRateVariation,
                    1f + _buildRateVariation);
        }

        private void Start()
        {
            ResolveRuntimeReferences();
        }

        private void Update()
        {
            ResolveRuntimeReferences();

            if (!IsFollowingActive)
            {
                ResetWhenInactive();

                return;
            }

            switch (State)
            {
                case ImpulseState.Preparing:
                    UpdatePreparing();
                    break;

                case ImpulseState.Rampaging:
                    UpdateRampaging();
                    break;

                case ImpulseState.Recovering:
                    UpdateRecovering();
                    break;

                case ImpulseState.Calm:
                case ImpulseState.Warning:
                case ImpulseState.Danger:
                default:
                    UpdateCharge();
                    break;
            }
        }

        public string GetStateLabel()
        {
            return State.ToString();
        }

        private void UpdateCharge()
        {
            CurrentImpulse =
                ImpulseLogic.Build(
                    CurrentImpulse,
                    _maximumImpulse,
                    _buildPerSecond *
                    _buildRateMultiplier,
                    Time.deltaTime);

            State =
                ImpulseLogic.ClassifyBand(
                    CurrentImpulse,
                    _warningThreshold,
                    _dangerThreshold);

            if (CurrentImpulse <
                _maximumImpulse)
            {
                return;
            }

            CurrentImpulse =
                _maximumImpulse;

            if (_coordinator == null ||
                _coordinator.TryBegin(
                    this))
            {
                BeginPreparing();

                return;
            }

            State =
                ImpulseState.Danger;
        }

        private void BeginPreparing()
        {
            State =
                ImpulseState.Preparing;

            _phaseRemaining =
                _prepareDuration;

            TakeControl();

            FacePlayer();
        }

        private void UpdatePreparing()
        {
            _phaseRemaining -=
                Time.deltaTime;

            FacePlayer();

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            if (_phaseRemaining <= 0f)
            {
                BeginRampaging();
            }
        }

        private void BeginRampaging()
        {
            State =
                ImpulseState.Rampaging;

            _phaseRemaining =
                _rampageDuration;

            TakeControl();
        }

        private void UpdateRampaging()
        {
            _phaseRemaining -=
                Time.deltaTime;

            if (_body == null ||
                _player == null)
            {
                BeginRecovering();

                return;
            }

            Vector2 delta =
                (Vector2)_player.position -
                (Vector2)transform.position;

            float distance =
                delta.magnitude;

            FacePlayer();

            if (distance <=
                _catchDistance)
            {
                BeginRecovering();

                return;
            }

            if (distance <= 0.001f)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
            else
            {
                _body.linearVelocity =
                    delta.normalized *
                    _rampageSpeed;
            }

            if (_phaseRemaining <= 0f)
            {
                BeginRecovering();
            }
        }

        private void BeginRecovering()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            if (_coordinator != null)
            {
                _coordinator.End(
                    this);
            }

            State =
                ImpulseState.Recovering;

            _phaseRemaining =
                _recoveryDuration;

            CurrentImpulse =
                Mathf.Clamp(
                    _postRecoveryImpulse,
                    0f,
                    _maximumImpulse);
        }

        private void UpdateRecovering()
        {
            _phaseRemaining -=
                Time.deltaTime;

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            if (_phaseRemaining > 0f)
            {
                return;
            }

            ReleaseControl();

            State =
                ImpulseLogic.ClassifyBand(
                    CurrentImpulse,
                    _warningThreshold,
                    _dangerThreshold);
        }

        private void TakeControl()
        {
            _follower?.SetExternalControl(
                true);

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void ReleaseControl()
        {
            _follower?.SetExternalControl(
                false);
        }

        private void ResetWhenInactive()
        {
            if (_coordinator != null)
            {
                _coordinator.End(
                    this);
            }

            ReleaseControl();

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            CurrentImpulse = 0f;
            _phaseRemaining = 0f;
            State = ImpulseState.Calm;
        }

        private void FacePlayer()
        {
            if (_player == null ||
                _animator == null)
            {
                return;
            }

            _animator.FaceHorizontal(
                _player.position.x -
                transform.position.x);
        }

        private void ResolveRuntimeReferences()
        {
            if (_player == null)
            {
                FollowerManager manager =
                    FindFirstObjectByType<
                        FollowerManager>();

                if (manager != null)
                {
                    _player =
                        manager.transform;
                }
            }

            if (_coordinator == null)
            {
                _coordinator =
                    FindFirstObjectByType<
                        RampageCoordinator>();
            }
        }

        private void OnDisable()
        {
            if (_coordinator != null)
            {
                _coordinator.End(
                    this);
            }

            ReleaseControl();

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }
    }
}
