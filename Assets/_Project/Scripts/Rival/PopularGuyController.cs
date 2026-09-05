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
    [RequireComponent(typeof(PopularGuyFollowerManager))]
    public sealed class PopularGuyController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 2.1f;
        [SerializeField] private float _stopDistance = 0.92f;

        [Header("Targeting")]
        [SerializeField] private float _reacquireInterval = 0.90f;
        [SerializeField] private float _abandonDistance = 13f;
        [SerializeField] private float _maximumPursuitDuration = 7.5f;

        [Header("Idle - Geumtaeyang x1.5")]
        [SerializeField] private float _minimumIdleDuration = 1.8f;
        [SerializeField] private float _maximumIdleDuration = 4.5f;
        [SerializeField] private float _lostTargetIdleMinimum = 1.2f;
        [SerializeField] private float _lostTargetIdleMaximum = 2.7f;
        [SerializeField] private float _postCaptureIdleMinimum = 1.5f;
        [SerializeField] private float _postCaptureIdleMaximum = 3.6f;

        [Header("Action")]
        [SerializeField] private float _actionDistance = 1.20f;
        [SerializeField] private float _contestDrainPerSecond = 12f;
        [SerializeField] private float _neutralClaimStepInterval = 0.75f;

        private Rigidbody2D _body;
        private RuntimeCharacterSpriteAnimator _animator;
        private StageSessionController _stage;
        private FollowerManager _playerFollowers;
        private PopularGuyFollowerManager _ownedFollowers;

        private HypnosisTarget _target;
        private PopularGuyTargetMode _targetMode;
        private float _reacquireTimer;
        private float _idleRemaining;
        private float _pursuitElapsed;
        private int _neutralClaimStep;
        private float _neutralClaimTimer;
        private bool _duelLocked;
        private float _duelStunRemaining;
        private AudioSource _claimTickSource;
        private AudioClip _claimTickClip;

        public PopularGuyState State { get; private set; } =
            PopularGuyState.Idle;

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
            PopularGuyDuelLogic.CanStart(
                _duelLocked,
                _duelStunRemaining,
                State,
                _targetMode,
                _target != null,
                _target == null
                    ? NpcOwner.PopularGuy
                    : _target.Owner);

        public string CurrentModeLabel
        {
            get
            {
                switch (_targetMode)
                {
                    case PopularGuyTargetMode.NeutralClaim:
                        return "중립 선점";

                    case PopularGuyTargetMode.Contest:
                        return "쟁탈";

                    case PopularGuyTargetMode.None:
                    default:
                        return "-";
                }
            }
        }

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _ownedFollowers =
                GetComponent<
                    PopularGuyFollowerManager>();

            _body.gravityScale =
                0f;

            _body.freezeRotation =
                true;

            _body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            _body.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            _claimTickSource =
                gameObject.AddComponent<
                    AudioSource>();

            _claimTickSource.playOnAwake =
                false;

            _claimTickSource.spatialBlend =
                0f;

            _claimTickSource.volume =
                0.45f;

            _claimTickClip =
                CreateClaimTickClip();
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
                    PopularGuyState.Idle;

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
                    PopularGuyState.Stunned;

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
                PopularGuyState.Idle)
            {
                UpdateIdle();

                return;
            }

            if (_target == null)
            {
                UpdateTargetSearch();

                return;
            }

            if (!IsTargetValid(
                    _target,
                    _targetMode))
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
                _actionDistance)
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
                    PopularGuyState.Approach;

                return;
            }

            _pursuitElapsed = 0f;

            if (_targetMode ==
                PopularGuyTargetMode.NeutralClaim)
            {
                State =
                    PopularGuyState.Claiming;

                StopMovement();

                ProcessNeutralClaimSteps();

                return;
            }

            State =
                PopularGuyState.Contest;

            StopMovement();

            bool depleted =
                _target.ApplyPopularGuyPressure(
                    _contestDrainPerSecond,
                    Time.deltaTime);

            if (depleted)
            {
                CaptureContestedTarget(
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
                PopularGuyState.Approach ||
                _target == null)
            {
                if (State !=
                    PopularGuyState.Approach)
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

            _body.linearVelocity =
                delta.normalized *
                _moveSpeed;
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
            ClearCurrentTargetVisuals();

            _target =
                null;

            _targetMode =
                PopularGuyTargetMode.None;

            _neutralClaimStep =
                0;

            _neutralClaimTimer =
                0f;

            _duelLocked =
                false;

            _duelStunRemaining =
                Mathf.Max(
                    0f,
                    duration);

            State =
                PopularGuyState.Stunned;

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

            HypnosisTarget neutral =
                FindNearestNeutralTarget();

            if (neutral != null)
            {
                AssignTarget(
                    neutral,
                    PopularGuyTargetMode.NeutralClaim);

                return;
            }

            HypnosisTarget contested =
                FindNearestContestTarget();

            if (contested != null)
            {
                AssignTarget(
                    contested,
                    PopularGuyTargetMode.Contest);

                return;
            }

            State =
                PopularGuyState.Search;
        }

        private HypnosisTarget FindNearestNeutralTarget()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best =
                null;

            float bestDistanceSquared =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (candidate == null ||
                    !candidate.isActiveAndEnabled ||
                    candidate.Owner !=
                    NpcOwner.Neutral)
                {
                    continue;
                }

                float distanceSquared =
                    ((Vector2)candidate.transform.position -
                     (Vector2)transform.position).
                    sqrMagnitude;

                if (distanceSquared >=
                    bestDistanceSquared)
                {
                    continue;
                }

                best =
                    candidate;

                bestDistanceSquared =
                    distanceSquared;
            }

            return best;
        }

        private HypnosisTarget FindNearestContestTarget()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best =
                null;

            float bestDistanceSquared =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (!IsTargetValid(
                        candidate,
                        PopularGuyTargetMode.Contest))
                {
                    continue;
                }

                float distanceSquared =
                    ((Vector2)candidate.transform.position -
                     (Vector2)transform.position).
                    sqrMagnitude;

                if (distanceSquared >=
                    bestDistanceSquared)
                {
                    continue;
                }

                best =
                    candidate;

                bestDistanceSquared =
                    distanceSquared;
            }

            return best;
        }

        private bool IsTargetValid(
            HypnosisTarget target,
            PopularGuyTargetMode mode)
        {
            if (target == null ||
                !target.isActiveAndEnabled)
            {
                return false;
            }

            if (mode ==
                PopularGuyTargetMode.NeutralClaim)
            {
                return target.Owner ==
                       NpcOwner.Neutral;
            }

            if (mode !=
                PopularGuyTargetMode.Contest ||
                !PopularGuyLogic.CanContest(
                    target.Owner))
            {
                return false;
            }

            if (target.Owner ==
                NpcOwner.Player)
            {
                if (!target.IsFollowing)
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
                }
            }

            return true;
        }

        private void AssignTarget(
            HypnosisTarget target,
            PopularGuyTargetMode mode)
        {
            _target =
                target;

            _targetMode =
                mode;

            _pursuitElapsed =
                0f;

            _neutralClaimStep =
                0;

            _neutralClaimTimer =
                0f;

            if (_targetMode ==
                PopularGuyTargetMode.NeutralClaim)
            {
                _target.SetPopularGuyClaimProgress(
                    0f);
            }

            _target.SetOpponentTargeted(
                NpcOwner.PopularGuy,
                true);

            State =
                PopularGuyState.Approach;
        }

        private void ProcessNeutralClaimSteps()
        {
            if (_target == null ||
                _target.Owner !=
                NpcOwner.Neutral)
            {
                ClearTarget();

                return;
            }

            _neutralClaimTimer +=
                Time.deltaTime;

            float interval =
                Mathf.Max(
                    0.05f,
                    _neutralClaimStepInterval);

            while (_neutralClaimTimer >=
                   interval)
            {
                _neutralClaimTimer -=
                    interval;

                _neutralClaimStep =
                    PopularGuyNeutralClaimLogic.
                        NextStep(
                            _neutralClaimStep);

                _target.SetPopularGuyClaimProgress(
                    PopularGuyNeutralClaimLogic.
                        Normalized(
                            _neutralClaimStep));

                PlayClaimTick();

                if (PopularGuyNeutralClaimLogic.
                        IsComplete(
                            _neutralClaimStep))
                {
                    CaptureNeutralTarget(
                        _target);

                    break;
                }
            }
        }

        private void PlayClaimTick()
        {
            if (_claimTickSource == null ||
                _claimTickClip == null)
            {
                return;
            }

            _claimTickSource.PlayOneShot(
                _claimTickClip);
        }

        private AudioClip CreateClaimTickClip()
        {
            const int sampleRate =
                44100;

            const float duration =
                0.055f;

            const float frequency =
                880f;

            int sampleCount =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        sampleRate *
                        duration));

            float[] samples =
                new float[
                    sampleCount];

            for (int i = 0;
                 i < samples.Length;
                 i++)
            {
                float t =
                    i /
                    (float)sampleRate;

                float fade =
                    1f -
                    (i /
                     (float)samples.Length);

                samples[i] =
                    Mathf.Sin(
                        Mathf.PI *
                        2f *
                        frequency *
                        t) *
                    0.16f *
                    fade;
            }

            AudioClip clip =
                AudioClip.Create(
                    "PopularGuyClaimTick",
                    sampleCount,
                    1,
                    sampleRate,
                    false);

            clip.SetData(
                samples,
                0);

            return clip;
        }

        private void CaptureNeutralTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Neutral)
            {
                ClearTarget();

                return;
            }

            target.ClaimByPopularGuy(
                this);

            _ownedFollowers?.TryAdd(
                target);

            FinishSuccessfulCapture();
        }

        private void CaptureContestedTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                !PopularGuyLogic.CanContest(
                    target.Owner))
            {
                ClearTarget();

                return;
            }

            if (target.Owner ==
                NpcOwner.Player)
            {
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
            }
            else if (target.Owner ==
                     NpcOwner.Geumtaeyang)
            {
                target.GeumtaeyangOwner?.
                    ReleaseOwnedTarget(
                        target);
            }

            target.ClaimByPopularGuy(
                this);

            _ownedFollowers?.TryAdd(
                target);

            FinishSuccessfulCapture();
        }

        private void FinishSuccessfulCapture()
        {
            _target =
                null;

            _targetMode =
                PopularGuyTargetMode.None;

            _pursuitElapsed =
                0f;

            _neutralClaimStep =
                0;

            _neutralClaimTimer =
                0f;

            EnterIdle(
                _postCaptureIdleMinimum,
                _postCaptureIdleMaximum);
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
                PopularGuyState.Search;

            _reacquireTimer =
                0f;
        }

        private void EnterIdle(
            float minimum,
            float maximum)
        {
            ClearCurrentTargetVisuals();

            State =
                PopularGuyState.Idle;

            _target =
                null;

            _targetMode =
                PopularGuyTargetMode.None;

            _pursuitElapsed =
                0f;

            _neutralClaimStep =
                0;

            _neutralClaimTimer =
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

        private void ClearCurrentTargetVisuals()
        {
            if (_target == null)
            {
                return;
            }

            if (_targetMode ==
                PopularGuyTargetMode.NeutralClaim)
            {
                _target.ClearPopularGuyClaimProgress();
            }

            _target.SetOpponentTargeted(
                NpcOwner.PopularGuy,
                false);
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
            ClearCurrentTargetVisuals();

            StopMovement();
        }

        private void OnDestroy()
        {
            if (_claimTickClip != null)
            {
                Destroy(
                    _claimTickClip);
            }
        }
    }
}
