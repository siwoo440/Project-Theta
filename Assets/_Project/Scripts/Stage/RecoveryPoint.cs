using UnityEngine;
using ProjectTheta.Companion;

namespace ProjectTheta.Stage
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RecoveryPoint : MonoBehaviour
    {
        private StageSessionController _stage;
        private FollowerManager _followers;
        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;
        private static Sprite _squareSprite;

        private static readonly Color NormalColor =
            new Color(0.72f, 0.25f, 1.00f, 0.24f);

        private static readonly Color LockedColor =
            new Color(0.95f, 0.30f, 0.30f, 0.30f);

        /// <summary>
        /// 모든 회수 지점이 잠겨 있는 끝 시각이다 (22일차). 구역 경계도가 비상에 닿으면 잠긴다.
        /// 게임 시간 기준이라 카드 화면으로 멈춘 동안에는 잠김도 멈춘다.
        /// </summary>
        private static float _lockedUntil = -1f;

        public static bool IsLocked =>
            Time.time < _lockedUntil;

        public static float LockRemaining =>
            Mathf.Max(
                0f,
                _lockedUntil - Time.time);

        public static void LockAll(
            float seconds)
        {
            _lockedUntil =
                Mathf.Max(
                    _lockedUntil,
                    Time.time + Mathf.Max(0f, seconds));
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _lockedUntil = -1f;
        }

        /// <summary>잠긴 동안 붉게 칠해 "지금은 못 넣는다"를 보여준다.</summary>
        private void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            Color color =
                IsLocked
                    ? LockedColor
                    : NormalColor;

            if (_renderer.color != color)
            {
                _renderer.color = color;
            }
        }

        private void Awake()
        {
            _collider =
                GetComponent<BoxCollider2D>();

            _collider.isTrigger =
                true;

            _renderer =
                GetComponent<SpriteRenderer>();

            if (_renderer == null)
            {
                _renderer =
                    gameObject.AddComponent<
                        SpriteRenderer>();
            }

            _renderer.sprite =
                GetSquareSprite();

            _renderer.color =
                new Color(
                    0.72f,
                    0.25f,
                    1.00f,
                    0.24f);

            _renderer.sortingOrder =
                -5;
        }

        public void Configure(
            StageSessionController stage,
            FollowerManager followers,
            Vector2 position,
            Vector2 size)
        {
            _stage = stage;
            _followers = followers;

            transform.position =
                new Vector3(
                    position.x,
                    position.y,
                    0f);

            transform.localScale =
                new Vector3(
                    size.x,
                    size.y,
                    1f);

            _collider.size =
                Vector2.one;
        }

        /// <summary>잠김이 풀렸을 때 이미 안에 서 있던 동행자도 회수되게 머무는 동안에도 확인한다.</summary>
        private void OnTriggerStay2D(
            Collider2D other)
        {
            OnTriggerEnter2D(
                other);
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (IsLocked ||
                _stage == null ||
                !_stage.IsRunning ||
                other == null)
            {
                return;
            }

            FollowerController follower =
                other.GetComponent<
                    FollowerController>();

            if (follower == null)
            {
                return;
            }

            _stage.TryRecoverFollower(
                follower,
                _followers);
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            Texture2D texture =
                new Texture2D(1, 1)
                {
                    name = "RecoveryPointSquare",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

            texture.SetPixel(
                0,
                0,
                Color.white);

            texture.Apply();

            _squareSprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        1f,
                        1f),
                    new Vector2(
                        0.5f,
                        0.5f),
                    1f);

            return _squareSprite;
        }
    }
}
