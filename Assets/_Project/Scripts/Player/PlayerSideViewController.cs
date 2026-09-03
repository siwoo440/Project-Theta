using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectTheta.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerSideViewController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f; // 이동 속도
        [SerializeField] private float _dashSpeed = 11f; // 대시 속도
        [SerializeField] private float _dashDuration = 0.16f; // 대시 시간
        [SerializeField] private float _dashCooldown = 0.6f; // 대시 재사용
        [SerializeField] private float _minX = -14.5f; // 왼쪽 한계
        [SerializeField] private float _maxX = 14.5f; // 오른쪽 한계
        [SerializeField] private float _minY = -2.35f; // 아래 한계
        [SerializeField] private float _maxY = 2.15f; // 위 한계
        private Rigidbody2D _rigidbody; // 물리 본체
        private Vector2 _moveInput; // 이동 입력
        private Vector2 _lastMoveDirection = Vector2.right; // 마지막 이동 방향
        private Vector2 _dashDirection = Vector2.right; // 대시 방향
        private float _dashRemaining; // 남은 대시
        private float _dashCooldownRemaining; // 남은 재사용

        public int FacingDirection { get; private set; } = 1; // 좌우 시선 방향
        public Vector2 MoveInput => _moveInput; // 현재 이동 입력
        public bool IsDashing => _dashRemaining > 0f; // 대시 여부

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>(); // 물리 참조
            _rigidbody.gravityScale = 0f; // 중력 제거
            _rigidbody.freezeRotation = true; // 회전 고정
        }

        private void Update()
        {
            _moveInput = ReadMovement(); // 평면 입력
            if (_moveInput.sqrMagnitude > 0.01f) // 이동 입력 확인
            {
                _lastMoveDirection = _moveInput.normalized; // 마지막 방향 갱신
            }

            if (Mathf.Abs(_moveInput.x) > 0.01f) // 좌우 입력 확인
            {
                FacingDirection = _moveInput.x > 0f ? 1 : -1; // 시선 방향 갱신
            }

            _dashRemaining = Mathf.Max(0f, _dashRemaining - Time.deltaTime); // 대시 시간 감소
            _dashCooldownRemaining = Mathf.Max(0f, _dashCooldownRemaining - Time.deltaTime); // 재사용 감소
            if (ReadDashPressed() && _dashCooldownRemaining <= 0f) // 대시 입력 확인
            {
                _dashDirection = PlanarMovement.ResolveDashDirection(_moveInput, _lastMoveDirection); // 대시 방향 결정
                _dashRemaining = _dashDuration; // 대시 시작
                _dashCooldownRemaining = _dashCooldown; // 재사용 시작
            }
        }

        private void FixedUpdate()
        {
            Vector2 direction = IsDashing ? _dashDirection : _moveInput; // 현재 이동 방향
            float speed = IsDashing ? _dashSpeed : _moveSpeed; // 현재 속도
            _rigidbody.linearVelocity = PlanarMovement.CalculateVelocity(direction, speed); // 평면 이동 적용
            Vector2 clamped = PlanarMovement.ClampPosition(_rigidbody.position, _minX, _maxX, _minY, _maxY); // 이동 범위 제한
            _rigidbody.position = clamped; // 제한 위치 적용
        }

        private Vector2 ReadMovement()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) // 키보드 확인
            {
                return Vector2.zero; // 입력 없음
            }

            float left = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? -1f : 0f; // 왼쪽 입력
            float right = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f; // 오른쪽 입력
            float down = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? -1f : 0f; // 아래 입력
            float up = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f; // 위 입력
            Vector2 input = new Vector2(left + right, down + up); // 평면 입력 조합
            return input.sqrMagnitude > 1f ? input.normalized : input; // 대각 입력 제한
#else
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // 구 입력 반환
            return input.sqrMagnitude > 1f ? input.normalized : input; // 대각 입력 제한
#endif
        }

        private bool ReadDashPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame); // 대시 입력 반환
#else
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space); // 구 대시 입력 반환
#endif
        }
    }
}
