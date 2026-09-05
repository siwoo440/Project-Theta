using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.Rival
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(RivalFollowerManager))]
    public sealed class RivalController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 4.2f;
        [SerializeField] private float _stopDistance = 0.92f;

        [Header("Targeting")]
        [SerializeField] private float _searchRange = 10f;
        [SerializeField] private float _reacquireInterval = 0.30f;
        [SerializeField] private float _abandonDistance = 13f;
        [SerializeField] private float _maximumPursuitDuration = 5.0f;

        [Header("Idle")]
        [SerializeField] private float _minimumIdleDuration = 0.6f;
        [SerializeField] private float _maximumIdleDuration = 1.5f;
        [SerializeField] private float _lostTargetIdleMinimum = 0.4f;
        [SerializeField] private float _lostTargetIdleMaximum = 0.9f;
        [SerializeField] private float _postCaptureIdleMinimum = 0.5f;
        [SerializeField] private float _postCaptureIdleMaximum = 1.2f;

        [Header("Contest")]
        [SerializeField] private float _contestDistance = 1.20f;
        [SerializeField] private float _contestDrainPerSecond = 18f;

        private Rigidbody2D _body;
        private RuntimeCharacterSpriteAnimator _animator;
        private StageSessionController _stage;
        private FollowerManager _playerFollowers;
        private RivalFollowerManager _ownedFollowers;

        private HypnosisTarget _target;
        private float _reacquireTimer;
        private float _idleRemaining;
        private float _pursuitElapsed;
        private bool _duelLocked;
        private float _duelStunRemaining;

        public RivalState State { get; private set; } =
            RivalState.Idle;

        public int FacingDirection { get; private set; } =
            -1;

        public int OwnedFollowerCount =>
            _ownedFollowers == null
                ? 0
                : _ownedFollowers.Count;

        public string CurrentTargetName =>
            _target == null
                ? "-"
                : _target.name;

        public float CurrentTargetControlNormalized =>
            _target == null
                ? 0f
                : _target.HypnosisNormalized;

        public bool CanStartPlayerDuel =>
            !_duelLocked &&
            _duelStunRemaining <=
                0f &&
            State ==
                RivalState.Contest &&
            _target != null &&
            _target.Owner ==
                NpcOwner.Player;

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _ownedFollowers =
                GetComponent<
                    RivalFollowerManager>();

            _body.gravityScale =
                0f;

            _body.freezeRotation =
                true;

            _body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            _body.interpolation =
                RigidbodyInterpolation2D.Interpolate;
        }

        public void Configure(
            StageSessionController stage,
            FollowerManager playerFollowers,
            RuntimeCharacterSpriteAnimator animator)
        {
            _stage =
                stage;

            _playerFollowers =
                playerFollowers;

            _animator =
                animator;

            EnterIdle(
                _minimumIdleDuration,
                _maximumIdleDuration);
        }

        private void Update()
        {
            if (_stage == null ||
                !_stage.IsRunning)
            {
                StopMovement();

                State =
                    RivalState.Idle;

                return;
            }

            if (_duelLocked)
            {
                StopMovement();

                return;
            }

            if (_duelStunRemaining >
                0f)
            {
                _duelStunRemaining =
                    Mathf.Max(
                        0f,
                        _duelStunRemaining -
                        Time.deltaTime);

                State =
                    RivalState.Stunned;

                StopMovement();

                if (_duelStunRemaining <=
                    0f)
                {
                    EnterIdle(
                        _lostTargetIdleMinimum,
                        _lostTargetIdleMaximum);
                }

                return;
            }

            if (State ==
                RivalState.Idle)
            {
                UpdateIdle();

                return;
            }

            RivalTargetDecision targetDecision =
                RivalTargetDecisionLogic.Resolve(
                    _target != null,
                    _target != null &&
                    IsTargetValid(
                        _target));

            if (targetDecision ==
                RivalTargetDecision.Search)
            {
                UpdateTargetSearch();

                return;
            }

            if (targetDecision ==
                RivalTargetDecision.Wait)
            {
                ClearTarget();

                return;
            }

            Vector2 delta =
                (Vector2)_target.transform.position -
                (Vector2)transform.position;

            FaceHorizontal(
                delta.x);

            float distance =
                delta.magnitude;

            if (distance >
                _contestDistance)
            {
                _pursuitElapsed +=
                    Time.deltaTime;

                if (OpponentTargetingLogic.
                        ShouldAbandon(
                            distance,
                            _pursuitElapsed,
                            _abandonDistance,
                            _maximumPursuitDuration))
                {
                    ClearTarget();

                    return;
                }

                State =
                    RivalState.Approach;

                return;
            }

            _pursuitElapsed = 0f;

            State =
                RivalState.Contest;

            StopMovement();

            bool depleted =
                _target.ApplyGeumtaeyangPressure(
                    _contestDrainPerSecond,
                    Time.deltaTime);

            if (depleted)
            {
                CaptureTarget(
                    _target);
            }
        }

        private void FixedUpdate()
        {
            if (_stage == null ||
                !_stage.IsRunning ||
                _duelLocked ||
                _duelStunRemaining >
                    0f ||
                State !=
                RivalState.Approach ||
                _target == null)
            {
                if (State !=
                    RivalState.Approach)
                {
                    StopMovement();
                }

                return;
            }

            Vector2 delta =
                (Vector2)_target.transform.position -
                (Vector2)transform.position;

            float distance =
                delta.magnitude;

            if (distance <=
                _stopDistance)
            {
                StopMovement();

                return;
            }

            Vector2 velocity =
                delta.normalized *
                _moveSpeed;

            _body.linearVelocity =
                velocity;
        }

        public void SetDuelLocked(
            bool locked)
        {
            _duelLocked =
                locked;

            if (locked)
            {
                StopMovement();
            }
        }

        public void ApplyDuelStun(
            float duration)
        {
            if (_target != null)
            {
                _target.SetOpponentTargeted(
                    NpcOwner.Geumtaeyang,
                    false);
            }

            _target =
                null;

            _duelLocked =
                false;

            _duelStunRemaining =
                Mathf.Max(
                    0f,
                    duration);

            State =
                RivalState.Stunned;

            StopMovement();
        }

        public void ReleaseOwnedTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                _ownedFollowers == null)
            {
                return;
            }

            _ownedFollowers.RemoveTarget(
                target);
        }

        private void CaptureTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Player)
            {
                ClearTarget();
                return;
            }

            ImpulseMeter impulse =
                target.GetComponent<
                    ImpulseMeter>();

            impulse?.CancelForRecovery();

            FollowerController follower =
                target.GetComponent<
                    FollowerController>();

            if (_playerFollowers == null ||
                follower == null ||
                !_playerFollowers.TransferOutFollower(
                    follower))
            {
                ClearTarget();
                return;
            }

            target.ClaimByGeumtaeyang(
                this);

            _ownedFollowers?.TryAdd(
                target);

            _target =
                null;

            _pursuitElapsed = 0f;

            EnterIdle(
                _postCaptureIdleMinimum,
                _postCaptureIdleMaximum);
        }

        private void UpdateTargetSearch()
        {
            _reacquireTimer -=
                Time.deltaTime;

            if (_reacquireTimer >
                0f)
            {
                return;
            }

            _reacquireTimer =
                Mathf.Max(
                    0.05f,
                    _reacquireInterval);

            _target =
                FindBestPlayerTarget();

            if (_target != null)
            {
                _target.SetOpponentTargeted(
                    NpcOwner.Geumtaeyang,
                    true);

                _pursuitElapsed = 0f;
            }

            State =
                _target == null
                    ? RivalState.Search
                    : RivalState.Approach;
        }

        private HypnosisTarget FindBestPlayerTarget()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best =
                null;

            float bestScore =
                float.NegativeInfinity;

            float range =
                Mathf.Max(
                    0f,
                    _searchRange);

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (!IsTargetValid(
                        candidate))
                {
                    continue;
                }

                float opponentDistance =
                    Vector2.Distance(
                        transform.position,
                        candidate.transform.position);

                if (opponentDistance >
                    range)
                {
                    continue;
                }

                float playerDistance =
                    _playerFollowers == null
                        ? 0f
                        : Vector2.Distance(
                            _playerFollowers.transform.position,
                            candidate.transform.position);

                float score =
                    OpponentTargetingLogic.
                        ScoreGeumtaeyangTarget(
                            opponentDistance,
                            playerDistance);

                if (score <=
                    bestScore)
                {
                    continue;
                }

                best =
                    candidate;

                bestScore =
                    score;
            }

            return best;
        }

        private bool IsTargetValid(
            HypnosisTarget target)
        {
            if (target == null ||
                !target.isActiveAndEnabled ||
                target.Owner !=
                NpcOwner.Player ||
                !target.IsFollowing)
            {
                return false;
            }

            ImpulseMeter impulse =
                target.GetComponent<
                    ImpulseMeter>();

            if (impulse == null)
            {
                return true;
            }

            switch (impulse.State)
            {
                case ImpulseState.Preparing:
                case ImpulseState.Rampaging:
                case ImpulseState.Capturing:
                case ImpulseState.Recovering:
                    return false;

                case ImpulseState.Calm:
                case ImpulseState.Warning:
                case ImpulseState.Danger:
                default:
                    return true;
            }
        }

        private void ClearTarget()
        {
            EnterIdle(
                _lostTargetIdleMinimum,
                _lostTargetIdleMaximum);
        }

        private void UpdateIdle()
        {
            StopMovement();

            _idleRemaining -=
                Time.deltaTime;

            if (_idleRemaining >
                0f)
            {
                return;
            }

            State =
                RivalState.Search;

            _reacquireTimer =
                0f;
        }

        private void EnterIdle(
            float minimum,
            float maximum)
        {
            if (_target != null)
            {
                _target.SetOpponentTargeted(
                    NpcOwner.Geumtaeyang,
                    false);
            }

            State =
                RivalState.Idle;

            _target =
                null;

            _pursuitElapsed =
                0f;

            _idleRemaining =
                RivalIdleLogic.ResolveDuration(
                    minimum,
                    maximum,
                    Random.value);

            _reacquireTimer =
                0f;

            StopMovement();
        }

        private void FaceHorizontal(
            float deltaX)
        {
            if (Mathf.Abs(
                    deltaX) <=
                0.001f)
            {
                return;
            }

            FacingDirection =
                deltaX > 0f
                    ? 1
                    : -1;

            _animator?.FaceHorizontal(
                deltaX);
        }

        private void StopMovement()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void OnDisable()
        {
            if (_target != null)
            {
                _target.SetOpponentTargeted(
                    NpcOwner.Geumtaeyang,
                    false);
            }

            StopMovement();
        }
    }
}
