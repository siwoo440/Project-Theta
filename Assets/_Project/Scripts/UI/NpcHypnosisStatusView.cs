using UnityEngine;
using ProjectTheta.Hypnosis;

namespace ProjectTheta.UI
{
    [RequireComponent(typeof(HypnosisTarget))]
    public sealed class NpcHypnosisStatusView : MonoBehaviour
    {
        [SerializeField] private float _gaugeWidth = 0.92f;
        [SerializeField] private float _gaugeHeight = 0.075f;
        [SerializeField] private Vector2 _gaugeOffset =
            new Vector2(0f, -0.18f);

        [SerializeField] private Vector2 _heartOffset =
            new Vector2(0.43f, 1.67f);

        private HypnosisTarget _target;
        private Transform _fillTransform;
        private SpriteRenderer _heartRenderer;

        private static Sprite _squareSprite;
        private Sprite _heartSprite;

        private void Awake()
        {
            _target = GetComponent<HypnosisTarget>();
            CreateGauge();
            CreateHeart();
            UpdateVisuals();
        }

        private void LateUpdate()
        {
            UpdateVisuals();
        }

        private void CreateGauge()
        {
            GameObject root =
                new GameObject("HypnosisGauge");

            root.transform.SetParent(
                transform,
                false);

            root.transform.localPosition =
                new Vector3(
                    _gaugeOffset.x,
                    _gaugeOffset.y,
                    0f);

            CreateBar(
                root.transform,
                "Outline",
                Vector2.zero,
                new Vector2(
                    _gaugeWidth + 0.08f,
                    _gaugeHeight + 0.07f),
                new Color(
                    0.08f,
                    0.05f,
                    0.10f,
                    0.96f),
                8);

            CreateBar(
                root.transform,
                "Track",
                Vector2.zero,
                new Vector2(
                    _gaugeWidth,
                    _gaugeHeight),
                new Color(
                    0.16f,
                    0.13f,
                    0.20f,
                    0.98f),
                9);

            GameObject fill =
                CreateBar(
                    root.transform,
                    "PurpleFill",
                    new Vector2(
                        -_gaugeWidth * 0.5f,
                        0f),
                    new Vector2(
                        0f,
                        _gaugeHeight),
                    new Color(
                        0.69f,
                        0.20f,
                        1.00f,
                        1f),
                    10);

            _fillTransform =
                fill.transform;
        }

        private void CreateHeart()
        {
            GameObject heart =
                new GameObject("HypnosisHeart");

            heart.transform.SetParent(
                transform,
                false);

            heart.transform.localPosition =
                new Vector3(
                    _heartOffset.x,
                    _heartOffset.y,
                    0f);

            _heartRenderer =
                heart.AddComponent<SpriteRenderer>();

            Texture2D texture =
                Resources.Load<Texture2D>(
                    "UI/Hypnosis/Heart");

            if (texture != null)
            {
                _heartSprite =
                    Sprite.Create(
                        texture,
                        new Rect(
                            0f,
                            0f,
                            texture.width,
                            texture.height),
                        new Vector2(0.5f, 0.5f),
                        320f);

                _heartSprite.name =
                    "HypnosisHeart_RuntimeSprite";

                _heartRenderer.sprite =
                    _heartSprite;
            }

            _heartRenderer.sortingOrder = 12;
            _heartRenderer.enabled = false;
        }

        private void UpdateVisuals()
        {
            if (_target == null ||
                _fillTransform == null)
            {
                return;
            }

            float progress =
                Mathf.Clamp01(
                    _target.HypnosisNormalized);

            float width =
                _gaugeWidth * progress;

            _fillTransform.localScale =
                new Vector3(
                    width,
                    _gaugeHeight,
                    1f);

            _fillTransform.localPosition =
                new Vector3(
                    (-_gaugeWidth * 0.5f) +
                    (width * 0.5f),
                    0f,
                    0f);

            if (_heartRenderer != null)
            {
                _heartRenderer.enabled =
                    _target.IsHypnotized;
            }
        }

        private static GameObject CreateBar(
            Transform parent,
            string name,
            Vector2 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            GameObject bar =
                new GameObject(name);

            bar.transform.SetParent(
                parent,
                false);

            bar.transform.localPosition =
                new Vector3(
                    localPosition.x,
                    localPosition.y,
                    0f);

            bar.transform.localScale =
                new Vector3(
                    size.x,
                    size.y,
                    1f);

            SpriteRenderer renderer =
                bar.AddComponent<SpriteRenderer>();

            renderer.sprite =
                GetSquareSprite();

            renderer.color = color;
            renderer.sortingOrder =
                sortingOrder;

            return bar;
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
                    name =
                        "HypnosisGaugeSquare",
                    filterMode =
                        FilterMode.Point,
                    wrapMode =
                        TextureWrapMode.Clamp
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

        private void OnDestroy()
        {
            if (_heartSprite != null)
            {
                Destroy(_heartSprite);
            }
        }
    }
}
