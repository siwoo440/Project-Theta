using System.Collections.Generic;
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

        private readonly List<Sprite> _createdSprites = new List<Sprite>();
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
            _framesPerSecond = Mathf.Max(1f, framesPerSecond);
            _pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit);

            _idleSprite = LoadFirstSprite( // 대기 스프라이트 조회
                new[] { resourceRoot + "/Idle" }, // 기존 단일 경로
                out string loadedIdlePath); // 불러온 경로
            LoadedIdlePath = loadedIdlePath; // 대기 경로 저장

            _moveSprites = new Sprite[4];
            for (int i = 0; i < _moveSprites.Length; i++)
            {
                _moveSprites[i] =
                    LoadFirstSprite( // 이동 스프라이트 조회
                        new[] { resourceRoot + $"/Move_{i}" }, // 기존 단일 경로
                        out _); // 개별 경로 미보관
            }

            CompleteConfiguration(); // 공통 설정 완료
        }

        public void Configure( // 주요 캐릭터 설정
            CharacterArtId characterId, // 캐릭터 ID
            float framesPerSecond = 8f, // 초당 프레임
            float pixelsPerUnit = 390f) // 픽셀 단위
        { // 설정 시작
            _framesPerSecond = Mathf.Max(1f, framesPerSecond); // 프레임 속도 보정
            _pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit); // 픽셀 단위 보정

            _idleSprite = LoadFirstSprite( // 대기 스프라이트 조회
                CharacterArtCatalog.GetIdlePaths(characterId), // 대기 후보 경로
                out string loadedIdlePath); // 불러온 경로
            LoadedIdlePath = loadedIdlePath; // 대기 경로 저장

            _moveSprites = new Sprite[4]; // 이동 프레임 배열 생성
            for (int i = 0; i < _moveSprites.Length; i++) // 이동 프레임 순회
            { // 순회 시작
                _moveSprites[i] = LoadFirstSprite( // 이동 스프라이트 조회
                    CharacterArtCatalog.GetMovementPaths(characterId, i), // 이동 후보 경로
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

        private Sprite LoadFirstSprite( // 첫 유효 스프라이트 조회
            string[] resourcePaths, // 후보 경로 목록
            out string loadedPath) // 불러온 경로
        { // 조회 시작
            loadedPath = string.Empty; // 경로 초기화

            if (resourcePaths == null) // 후보 목록 확인
            { // 목록 없음 시작
                return null; // 스프라이트 없음 반환
            } // 목록 없음 끝

            for (int i = 0; i < resourcePaths.Length; i++) // 후보 경로 순회
            { // 순회 시작
                string resourcePath = resourcePaths[i]; // 현재 경로 조회
                Texture2D texture = Resources.Load<Texture2D>(resourcePath); // 텍스처 조회

                if (texture == null) // 텍스처 없음 확인
                { // 없음 시작
                    continue; // 다음 후보 이동
                } // 없음 끝

                Sprite sprite = Sprite.Create( // 런타임 스프라이트 생성
                    texture, // 원본 텍스처
                    new Rect( // 전체 영역 생성
                        0f, // 왼쪽 좌표
                        0f, // 아래 좌표
                        texture.width, // 텍스처 너비
                        texture.height), // 텍스처 높이
                    new Vector2(0.5f, 0.0625f), // 발 기준점
                    _pixelsPerUnit); // 픽셀 단위

                sprite.name = resourcePath.Replace('/', '_') + "_RuntimeSprite"; // 스프라이트 이름 설정

                _createdSprites.Add(sprite); // 정리 목록 추가
                loadedPath = resourcePath; // 불러온 경로 저장

                return sprite; // 첫 스프라이트 반환
            } // 순회 끝

            return null; // 스프라이트 없음 반환
        } // 조회 끝

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

        private void OnDestroy()
        {
            for (int i = 0; i < _createdSprites.Count; i++)
            {
                if (_createdSprites[i] != null)
                {
                    Destroy(_createdSprites[i]);
                }
            }

            _createdSprites.Clear();
        }
    }
}
