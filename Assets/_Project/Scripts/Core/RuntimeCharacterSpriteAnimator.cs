using ProjectTheta.Presentation; // 주요 캐릭터 리소스 규칙 참조
using UnityEngine;

namespace ProjectTheta.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RuntimeCharacterSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private float _framesPerSecond = 8f;
        [SerializeField] private float _pixelsPerUnit = 390f;
        [SerializeField] private float _movementThreshold = 0.00001f;

        private SpriteRenderer _renderer;
        private Sprite _idleSprite;
        private Sprite[] _moveSprites;
        private Vector3 _lastPosition;
        private float _animationTime;
        private Color _baseTint = Color.white;
        private bool _highlighted;

        public bool IsConfigured { get; private set; }
        public string LoadedIdlePath { get; private set; } = string.Empty; // 불러온 대기 경로

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _lastPosition = transform.position;
        }

        public void Configure(
            string resourceRoot,
            float framesPerSecond = 8f,
            float pixelsPerUnit = 390f)
        {
            Configure( // 후보 배열 설정 위임
                new[] { resourceRoot + "/Idle" }, // 기존 대기 후보
                new[] // 기존 이동 후보 목록
                { // 목록 시작
                    new[] { resourceRoot + "/Move_0" }, // 첫 이동 후보
                    new[] { resourceRoot + "/Move_1" }, // 둘째 이동 후보
                    new[] { resourceRoot + "/Move_2" }, // 셋째 이동 후보
                    new[] { resourceRoot + "/Move_3" } // 넷째 이동 후보
                }, // 목록 끝
                framesPerSecond, // 초당 프레임 전달
                pixelsPerUnit); // 픽셀 단위 전달
        }

        public void Configure( // 주요 캐릭터 설정
            CharacterArtId characterId, // 캐릭터 ID
            float framesPerSecond = 8f, // 초당 프레임
            float pixelsPerUnit = 390f) // 픽셀 단위
        { // 설정 시작
            Configure( // 후보 배열 설정 위임
                CharacterArtCatalog.GetIdlePaths(characterId), // 대기 후보 경로
                new[] // 이동 후보 목록
                { // 목록 시작
                    CharacterArtCatalog.GetMovementPaths(characterId, 0), // 첫 이동 후보
                    CharacterArtCatalog.GetMovementPaths(characterId, 1), // 둘째 이동 후보
                    CharacterArtCatalog.GetMovementPaths(characterId, 2), // 셋째 이동 후보
                    CharacterArtCatalog.GetMovementPaths(characterId, 3) // 넷째 이동 후보
                }, // 목록 끝
                framesPerSecond, // 초당 프레임 전달
                pixelsPerUnit); // 픽셀 단위 전달
        } // 설정 끝

        public void Configure( // 후보 배열 기반 설정
            string[] idlePaths, // 대기 후보 경로
            string[][] movementPaths, // 이동 프레임별 후보 경로
            float framesPerSecond = 8f, // 초당 프레임
            float pixelsPerUnit = 390f) // 픽셀 단위
        { // 설정 시작
            _framesPerSecond = Mathf.Max(1f, framesPerSecond); // 프레임 속도 보정
            _pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit); // 픽셀 단위 보정
            _idleSprite = RuntimeArtLoader.LoadFirst( // 대기 스프라이트 조회
                idlePaths, // 대기 후보 전달
                new Vector2(0.5f, 0.0625f), // 발 기준 피벗
                _pixelsPerUnit, // 픽셀 단위 전달
                out string loadedIdlePath); // 불러온 경로 수신
            LoadedIdlePath = loadedIdlePath; // 대기 경로 저장
            int movementCount = movementPaths == null ? 0 : movementPaths.Length; // 이동 프레임 수 계산
            _moveSprites = new Sprite[movementCount]; // 이동 프레임 배열 생성

            for (int i = 0; i < movementCount; i++) // 이동 프레임 순회
            { // 순회 시작
                _moveSprites[i] = RuntimeArtLoader.LoadFirst( // 이동 스프라이트 조회
                    movementPaths[i], // 현재 프레임 후보 전달
                    new Vector2(0.5f, 0.0625f), // 발 기준 피벗
                    _pixelsPerUnit, // 픽셀 단위 전달
                    out _); // 개별 경로 미보관
            } // 순회 끝

            CompleteConfiguration(); // 공통 설정 완료
        } // 설정 끝

        private void CompleteConfiguration() // 불러온 스프라이트 적용
        { // 적용 시작
            if (_idleSprite != null)
            {
                _renderer.sprite = _idleSprite;
            }
            else
            {
                for (int i = 0; i < _moveSprites.Length; i++)
                {
                    if (_moveSprites[i] != null)
                    {
                        _renderer.sprite = _moveSprites[i];
                        break;
                    }
                }
            }

            _lastPosition = transform.position;
            _animationTime = 0f;
            IsConfigured = true;
            ApplyTint();
        } // 적용 끝

        public void SetBaseTint(Color color)
        {
            _baseTint = color;
            ApplyTint();
        }

        public void SetHighlighted(bool highlighted)
        {
            _highlighted = highlighted;
            ApplyTint();
        }

        public void FaceHorizontal(float horizontalDirection)
        {
            if (Mathf.Abs(horizontalDirection) <= 0.001f)
            {
                return;
            }

            _renderer.flipX = horizontalDirection < 0f;
        }

        private void LateUpdate()
        {
            if (!IsConfigured)
            {
                return;
            }

            Vector3 delta = transform.position - _lastPosition;
            bool moving = delta.sqrMagnitude > _movementThreshold;

            if (Mathf.Abs(delta.x) > 0.0001f)
            {
                FaceHorizontal(delta.x);
            }

            if (moving && HasMoveFrames())
            {
                _animationTime += Time.deltaTime;
                int index =
                    Mathf.FloorToInt(
                        _animationTime * _framesPerSecond) %
                    _moveSprites.Length;

                if (_moveSprites[index] != null)
                {
                    _renderer.sprite = _moveSprites[index];
                }
            }
            else
            {
                _animationTime = 0f;
                if (_idleSprite != null)
                {
                    _renderer.sprite = _idleSprite;
                }
            }

            _lastPosition = transform.position;
        }

        private bool HasMoveFrames()
        {
            if (_moveSprites == null || _moveSprites.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _moveSprites.Length; i++)
            {
                if (_moveSprites[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyTint()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.color = _highlighted
                ? new Color(0.87f, 0.72f, 1f, 1f)
                : _baseTint;
        }

    }
}
