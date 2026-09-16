using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NpcSoftSeparation))]
    [RequireComponent(typeof(NpcWanderMotion))]
    public sealed class NpcAgent : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 1.65f;

        /// <summary>22일차: 기업 연수원 쉬는 시간에는 복도 NPC가 빨라진다.</summary>
        private float CurrentMoveSpeed =>
            _moveSpeed *
            Stage.Locations.BreakTimeBell.NpcSpeedMultiplier;
        [SerializeField] private float _alertDistance = 3.2f;
        [SerializeField] private float _alertExitDistance = 4.2f;
        [SerializeField] private float _minimumIdleTime = 0.55f;
        [SerializeField] private float _maximumIdleTime = 1.60f;
        [SerializeField] private float _arrivalDistance = 0.18f;

        [Header("Wander - 제자리 방황")]
        [SerializeField] private float _idleWanderSpeedMultiplier = 0.55f;
        [SerializeField] private float _alertWanderAmplitudeScale = 0.35f;
        [SerializeField] private float _wanderArrivalDistance = 0.05f;

        [Header("Flee - 도주형 특성")]
        [SerializeField] private float _fleeSpeedMultiplier = 1.45f;
        [SerializeField] private float _fleeRetargetInterval = 0.45f;
        [SerializeField] private float _fleeDistance = 4.2f;

        private Rigidbody2D _body;
        private Transform _player;
        private RuntimeCharacterSpriteAnimator _animator;
        private NpcSoftSeparation _separation;
        private Vector2 _moveTarget;
        private float _idleRemaining;
        private bool _configured;

        private NpcWanderMotion _wander;
        private Vector2 _wanderAnchor;

        private NpcProfile _profile;
        private HypnosisTarget _hypnosis;
        private bool _fleeResolved;
        private bool _isFleeing;
        private float _fleeRetargetTimer;

        public NpcState State { get; private set; } =
            NpcState.Idle;

        /// <summary>도주형 NPC가 지금 플레이어에게서 달아나는 중인지 여부다.</summary>
        public bool IsFleeing =>
            _isFleeing;

        /// <summary>
        /// 도주형 특성을 가진 중립 NPC가 플레이어의 최면 대상으로 지정된 상태인지 판정한다.
        /// </summary>
        private bool ShouldFlee
        {
            get
            {
                if (!_fleeResolved)
                {
                    _fleeResolved =
                        true;

                    _profile =
                        GetComponent<NpcProfile>();

                    _hypnosis =
                        GetComponent<HypnosisTarget>();
                }

                return _profile != null &&
                       _profile.HasTrait(
                           NpcTrait.Fleer) &&
                       _hypnosis != null &&
                       _hypnosis.IsTargeted &&
                       _hypnosis.Owner ==
                       NpcOwner.Neutral;
            }
        }

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _separation =
                GetComponent<NpcSoftSeparation>();

            _wander =
                GetComponent<NpcWanderMotion>();

            _wanderAnchor =
                transform.position;

            _body.gravityScale = 0f;
            _body.freezeRotation = true;

            _body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            _body.interpolation =
                RigidbodyInterpolation2D.Interpolate;
        }

        public void Configure(
            Transform player,
            RuntimeCharacterSpriteAnimator animator)
        {
            _player = player;
            _animator = animator;
            _configured = true;

            EnterIdle();
        }

        public void EnterFollowing()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            SetState(
                NpcState.Following);
        }

        public void ReturnToRoaming()
        {
            if (!_configured)
            {
                return;
            }

            EnterIdle();
        }

        private void Update()
        {
            if (!_configured ||
                State == NpcState.Following)
            {
                return;
            }

            if (ShouldFlee)
            {
                UpdateFlee();

                return;
            }

            _isFleeing =
                false;

            float playerDistance =
                _player == null
                    ? float.MaxValue
                    : Vector2.Distance(
                        transform.position,
                        _player.position);

            if (State != NpcState.Alert &&
                NpcAiLogic.ShouldEnterAlert(
                    playerDistance,
                    _alertDistance))
            {
                _wanderAnchor =
                    transform.position;

                SetState(
                    NpcState.Alert);
            }
            else if (
                State == NpcState.Alert &&
                NpcAiLogic.ShouldLeaveAlert(
                    playerDistance,
                    _alertExitDistance))
            {
                EnterMove();
            }

            if (State ==
                NpcState.Alert)
            {
                if (_player != null)
                {
                    _animator?.FaceHorizontal(
                        _player.position.x -
                        transform.position.x);
                }

                return;
            }

            if (State ==
                NpcState.Idle)
            {
                _idleRemaining -=
                    Time.deltaTime;

                if (_idleRemaining <= 0f)
                {
                    EnterMove();
                }

                return;
            }

            if (State ==
                NpcState.Move)
            {
                if (Vector2.Distance(
                        transform.position,
                        _moveTarget) <=
                    _arrivalDistance)
                {
                    EnterIdle();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_configured)
            {
                _body.linearVelocity =
                    Vector2.zero;

                return;
            }

            if (State ==
                NpcState.Following)
            {
                return;
            }

            Vector2 separationVelocity =
                _separation == null
                    ? Vector2.zero
                    : _separation.GetCorrectionVelocity();

            if (State !=
                NpcState.Move)
            {
                _body.linearVelocity =
                    GetWanderVelocity() +
                    separationVelocity;

                return;
            }

            Vector2 direction =
                _moveTarget -
                (Vector2)transform.position;

            if (direction.sqrMagnitude <=
                _arrivalDistance *
                _arrivalDistance)
            {
                _body.linearVelocity =
                    separationVelocity;

                return;
            }

            float speed =
                _isFleeing
                    ? CurrentMoveSpeed *
                      Mathf.Max(
                          1f,
                          _fleeSpeedMultiplier)
                    : CurrentMoveSpeed;

            Vector2 movementVelocity =
                direction.normalized *
                speed;

            _body.linearVelocity =
                Vector2.ClampMagnitude(
                    movementVelocity +
                    separationVelocity,
                    speed + 0.65f);
        }

        /// <summary>
        /// Idle·Alert 상태에서 제자리에 굳지 않도록 기준점 주변을 천천히 서성이게 한다.
        /// Alert에서는 진폭을 줄여 플레이어를 의식하는 느낌을 유지한다.
        /// </summary>
        private Vector2 GetWanderVelocity()
        {
            if (_wander == null)
            {
                return Vector2.zero;
            }

            float amplitudeScale =
                State ==
                NpcState.Alert
                    ? _alertWanderAmplitudeScale
                    : 1f;

            Vector2 destination =
                _wanderAnchor +
                _wander.GetOffset(
                    amplitudeScale);

            destination =
                new Vector2(
                    Mathf.Clamp(
                        destination.x,
                        FloorSpace.WalkMinX + 0.8f,
                        FloorSpace.WalkMaxX - 0.8f),
                    FloorSpace.ClampYOn(
                        CurrentFloor,
                        destination.y,
                        0.55f,
                        0.35f));

            Vector2 delta =
                destination -
                (Vector2)transform.position;

            if (delta.magnitude <=
                _wanderArrivalDistance)
            {
                return Vector2.zero;
            }

            return delta.normalized *
                   (CurrentMoveSpeed *
                    Mathf.Max(
                        0f,
                        _idleWanderSpeedMultiplier));
        }

        /// <summary>플레이어 반대 방향의 이동 목표를 주기적으로 갱신한다.</summary>
        private void UpdateFlee()
        {
            _isFleeing =
                true;

            SetState(
                NpcState.Move);

            _fleeRetargetTimer -=
                Time.deltaTime;

            if (_fleeRetargetTimer >
                0f)
            {
                return;
            }

            _fleeRetargetTimer =
                Mathf.Max(
                    0.05f,
                    _fleeRetargetInterval);

            Vector2 away =
                _player == null
                    ? Vector2.right
                    : (Vector2)transform.position -
                      (Vector2)_player.position;

            if (away.sqrMagnitude <=
                0.0001f)
            {
                away =
                    Vector2.right;
            }

            Vector2 destination =
                (Vector2)transform.position +
                (away.normalized *
                 Mathf.Max(
                     0.5f,
                     _fleeDistance));

            _moveTarget =
                new Vector2(
                    Mathf.Clamp(
                        destination.x,
                        FloorSpace.WalkMinX + 1.0f,
                        FloorSpace.WalkMaxX - 1.0f),
                    FloorSpace.ClampYOn(
                        CurrentFloor,
                        destination.y,
                        0.7f,
                        0.4f));
        }

        /// <summary>이 NPC가 서 있는 층이다. 층은 세로 좌표로 결정된다.</summary>
        public int CurrentFloor =>
            FloorSpace.FloorAt(
                transform.position.y);

        private void EnterIdle()
        {
            _isFleeing =
                false;

            _wanderAnchor =
                transform.position;

            SetState(
                NpcState.Idle);

            _idleRemaining =
                Random.Range(
                    _minimumIdleTime,
                    _maximumIdleTime);
        }

        private void EnterMove()
        {
            SetState(
                NpcState.Move);

            int floor =
                CurrentFloor;

            _moveTarget =
                new Vector2(
                    Random.Range(
                        FloorSpace.WalkMinX + 1.0f,
                        FloorSpace.WalkMaxX - 1.0f),
                    Random.Range(
                        FloorSpace.MinYOn(
                            floor) + 0.7f,
                        FloorSpace.MaxYOn(
                            floor) - 0.4f));
        }

        private void SetState(
            NpcState state)
        {
            State = state;

            if (_animator == null)
            {
                return;
            }

            switch (state)
            {
                case NpcState.Alert:
                    _animator.SetBaseTint(
                        new Color(
                            1f,
                            0.78f,
                            0.60f,
                            1f));
                    break;

                case NpcState.Following:
                case NpcState.Idle:
                case NpcState.Move:
                default:
                    _animator.SetBaseTint(
                        Color.white);
                    break;
            }
        }

        private void OnDisable()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }
    }
}
