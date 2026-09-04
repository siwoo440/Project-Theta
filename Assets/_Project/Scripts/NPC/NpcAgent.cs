using UnityEngine;
using ProjectTheta.Core;

namespace ProjectTheta.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NpcSoftSeparation))]
    public sealed class NpcAgent : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 1.65f;
        [SerializeField] private float _alertDistance = 3.2f;
        [SerializeField] private float _alertExitDistance = 4.2f;
        [SerializeField] private float _minimumIdleTime = 0.8f;
        [SerializeField] private float _maximumIdleTime = 2.2f;
        [SerializeField] private float _arrivalDistance = 0.18f;

        private Rigidbody2D _body;
        private Transform _player;
        private RuntimeCharacterSpriteAnimator _animator;
        private NpcSoftSeparation _separation;
        private Vector2 _moveTarget;
        private float _idleRemaining;
        private bool _configured;

        public NpcState State { get; private set; } =
            NpcState.Idle;

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _separation =
                GetComponent<NpcSoftSeparation>();

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

            Vector2 movementVelocity =
                direction.normalized *
                _moveSpeed;

            _body.linearVelocity =
                Vector2.ClampMagnitude(
                    movementVelocity +
                    separationVelocity,
                    _moveSpeed + 0.65f);
        }

        private void EnterIdle()
        {
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

            _moveTarget =
                new Vector2(
                    Random.Range(
                        SchoolHallwayPrototypeBuilder.WalkMinX + 1.0f,
                        SchoolHallwayPrototypeBuilder.WalkMaxX - 1.0f),
                    Random.Range(
                        SchoolHallwayPrototypeBuilder.WalkMinY + 0.7f,
                        SchoolHallwayPrototypeBuilder.WalkMaxY - 0.4f));
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
