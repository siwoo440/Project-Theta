using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;

namespace ProjectTheta.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerSideViewController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _dashSpeed = 11f;
        [SerializeField] private float _dashDuration = 0.16f;
        [SerializeField] private float _dashCooldown = 0.6f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _lastMoveDirection = Vector2.right;
        private PlayerFocus _focus;

        private float _sprintRemaining;
        private float _sprintSpeedMultiplier = 1f;
        private float _sprintCooldownMultiplier = 1f;

        private Vector2 _dashDirection = Vector2.right;
        private float _dashRemaining;
        private float _dashCooldownRemaining;
        private bool _inputLocked;

        /// <summary>헬스 고인물에게 밀려 휘청이는 남은 시간이다 (24일차). 입력 잠금과 따로 흐른다.</summary>
        private float _staggerRemaining;

        public int FacingDirection { get; private set; } = 1;
        public bool IsDashing => _dashRemaining > 0f;

        /// <summary>질주약이 적용 중인 상태다.</summary>
        public bool IsSprintBoosted => _sprintRemaining > 0f;

        public float SprintRemaining =>
            Mathf.Max(
                0f,
                _sprintRemaining);

        /// <summary>질주약이 이동 속도와 대시 재사용 대기를 일정 시간 강화한다.</summary>
        public void ApplySprintBoost(
            float durationSeconds,
            float speedMultiplier,
            float cooldownMultiplier)
        {
            _sprintRemaining =
                Mathf.Max(
                    _sprintRemaining,
                    Mathf.Max(
                        0f,
                        durationSeconds));

            _sprintSpeedMultiplier =
                Mathf.Max(
                    1f,
                    speedMultiplier);

            _sprintCooldownMultiplier =
                Mathf.Clamp(
                    cooldownMultiplier,
                    0.1f,
                    1f);
        }
        public bool IsStaggered => _staggerRemaining > 0f;

        /// <summary>잠깐 움직이지도 대시하지도 못하게 한다. 포획 · 힘겨루기의 입력 잠금과 겹쳐도 서로 풀지 않는다.</summary>
        public void ApplyStagger(
            float seconds)
        {
            _staggerRemaining =
                Mathf.Max(
                    _staggerRemaining,
                    seconds);

            _dashRemaining = 0f;
        }

        public Vector2 MoveInput => _moveInput;
        public bool IsInputLocked => _inputLocked;

        private void Awake()
        {
            _rigidbody =
                GetComponent<Rigidbody2D>();

            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;
            _rigidbody.interpolation =
                RigidbodyInterpolation2D.Interpolate;
        }

        private void Update()
        {
            if (_inputLocked ||
                GameplayPause.IsPaused)
            {
                _moveInput =
                    Vector2.zero;

                _dashRemaining = 0f;

                return;
            }

            if (_staggerRemaining > 0f)
            {
                _staggerRemaining -= Time.deltaTime;
                _moveInput = Vector2.zero;
                _dashRemaining = 0f;

                return;
            }

            _moveInput =
                ReadMovement();

            if (_moveInput.sqrMagnitude >
                0.0001f)
            {
                _lastMoveDirection =
                    _moveInput;

                if (Mathf.Abs(
                        _moveInput.x) >
                    0.01f)
                {
                    FacingDirection =
                        _moveInput.x > 0f
                            ? 1
                            : -1;
                }
            }

            _dashRemaining =
                Mathf.Max(
                    0f,
                    _dashRemaining -
                    Time.deltaTime);

            _dashCooldownRemaining =
                Mathf.Max(
                    0f,
                    _dashCooldownRemaining -
                    Time.deltaTime);

            _sprintRemaining =
                Mathf.Max(
                    0f,
                    _sprintRemaining -
                    Time.deltaTime);

            if (ReadDashPressed() &&
                _dashCooldownRemaining <= 0f &&
                TrySpendDashFocus())
            {
                _dashDirection =
                    PlayerMovementMath.ResolveDashDirection(
                        _moveInput,
                        _lastMoveDirection);

                _dashRemaining =
                    _dashDuration;

                _dashCooldownRemaining =
                    _dashCooldown *
                    (IsSprintBoosted
                        ? _sprintCooldownMultiplier
                        : 1f);
            }
        }

        private void FixedUpdate()
        {
            if (_inputLocked)
            {
                _rigidbody.linearVelocity =
                    Vector2.zero;

                return;
            }

            Vector2 direction =
                IsDashing
                    ? _dashDirection
                    : _moveInput;

            float speed =
                IsDashing
                    ? _dashSpeed
                    : _moveSpeed;

            if (IsSprintBoosted)
            {
                speed *=
                    _sprintSpeedMultiplier;
            }

            speed *=
                PlayerUpgradeMultipliers.MoveSpeed;

            _rigidbody.linearVelocity =
                direction *
                speed;
        }

        /// <summary>대시는 집중력을 소모한다. 집중력이 모자라면 대시가 나가지 않는다.</summary>
        private bool TrySpendDashFocus()
        {
            if (_focus == null)
            {
                _focus =
                    GetComponent<PlayerFocus>();
            }

            if (_focus == null)
            {
                return true;
            }

            return _focus.TrySpend(
                _focus.DashCost *
                PlayerUpgradeMultipliers.DashCost);
        }

        public void SetInputLocked(
            bool isLocked)
        {
            _inputLocked =
                isLocked;

            if (!isLocked)
            {
                return;
            }

            _moveInput =
                Vector2.zero;

            _dashRemaining = 0f;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity =
                    Vector2.zero;
            }
        }

        public void FaceToward(
            float worldX)
        {
            float deltaX =
                worldX -
                transform.position.x;

            if (Mathf.Abs(deltaX) <=
                0.001f)
            {
                return;
            }

            FacingDirection =
                deltaX > 0f
                    ? 1
                    : -1;
        }

        private void OnDisable()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity =
                    Vector2.zero;
            }
        }

        private Vector2 ReadMovement()
        {
            // 32일차: 키 설정(GameInput)을 따른다.
            return PlayerMovementMath.NormalizeInput(
                GameInput.GetMove());
        }

        private bool ReadDashPressed()
        {
            return GameInput.WasPressed(
                GameAction.Dash);
        }
    }
}
