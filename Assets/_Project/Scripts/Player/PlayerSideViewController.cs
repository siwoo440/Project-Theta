using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        private Vector2 _dashDirection = Vector2.right;
        private float _dashRemaining;
        private float _dashCooldownRemaining;

        public int FacingDirection { get; private set; } = 1;
        public bool IsDashing => _dashRemaining > 0f;
        public Vector2 MoveInput => _moveInput;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;
            _rigidbody.interpolation =
                RigidbodyInterpolation2D.Interpolate;
        }

        private void Update()
        {
            _moveInput = ReadMovement();

            if (_moveInput.sqrMagnitude > 0.0001f)
            {
                _lastMoveDirection = _moveInput;

                if (Mathf.Abs(_moveInput.x) > 0.01f)
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

            if (ReadDashPressed() &&
                _dashCooldownRemaining <= 0f)
            {
                _dashDirection =
                    PlayerMovementMath.ResolveDashDirection(
                        _moveInput,
                        _lastMoveDirection);

                _dashRemaining =
                    _dashDuration;

                _dashCooldownRemaining =
                    _dashCooldown;
            }
        }

        private void FixedUpdate()
        {
            Vector2 direction =
                IsDashing
                    ? _dashDirection
                    : _moveInput;

            float speed =
                IsDashing
                    ? _dashSpeed
                    : _moveSpeed;

            _rigidbody.linearVelocity =
                direction * speed;
        }

        public void FaceToward(
            float worldX)
        {
            float deltaX =
                worldX -
                transform.position.x;

            if (Mathf.Abs(deltaX) <= 0.001f)
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
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
            {
                return Vector2.zero;
            }

            float x = 0f;
            float y = 0f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                x += 1f;
            }

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                y += 1f;
            }

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
            {
                y -= 1f;
            }

            return PlayerMovementMath.NormalizeInput(
                new Vector2(x, y));
#else
            return PlayerMovementMath.NormalizeInput(
                new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")));
#endif
        }

        private bool ReadDashPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return
                Keyboard.current != null &&
                (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                 Keyboard.current.spaceKey.wasPressedThisFrame);
#else
            return
                Input.GetKeyDown(KeyCode.LeftShift) ||
                Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }
}
