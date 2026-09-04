using System.Collections.Generic;
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

            _idleSprite = LoadSprite(resourceRoot + "/Idle");

            _moveSprites = new Sprite[4];
            for (int i = 0; i < _moveSprites.Length; i++)
            {
                _moveSprites[i] =
                    LoadSprite(resourceRoot + $"/Move_{i}");
            }

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
        }

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

        private Sprite LoadSprite(string resourcePath)
        {
            Texture2D texture =
                Resources.Load<Texture2D>(resourcePath);

            if (texture == null)
            {
                Debug.LogWarning(
                    $"Project Theta: character texture not found: {resourcePath}");
                return null;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    texture.width,
                    texture.height),
                new Vector2(0.5f, 0.0625f),
                _pixelsPerUnit);

            sprite.name =
                resourcePath.Replace('/', '_') + "_RuntimeSprite";

            _createdSprites.Add(sprite);
            return sprite;
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
