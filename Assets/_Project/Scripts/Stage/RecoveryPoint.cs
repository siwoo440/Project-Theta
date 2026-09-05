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

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (_stage == null ||
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
