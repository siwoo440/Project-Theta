using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;

namespace ProjectTheta.UI
{
    [RequireComponent(typeof(HypnosisTarget))]
    public sealed class NpcHypnosisStatusView : MonoBehaviour
    {
        [SerializeField] private float _gaugeWidth = 0.92f;
        [SerializeField] private float _gaugeHeight = 0.075f;

        [SerializeField] private Vector2 _gaugeOffset =
            new Vector2(0f, -0.18f);

        [SerializeField] private Vector2 _iconOffset =
            new Vector2(0.43f, 1.67f);

        private HypnosisTarget _target;
        private ImpulseMeter _impulse;

        private GameObject _gaugeRoot;
        private Transform _fillTransform;
        private SpriteRenderer _fillRenderer;
        private SpriteRenderer _heartRenderer;
        private SpriteRenderer _exclamationRenderer;

        private static Sprite _squareSprite;
        private Sprite _heartSprite;
        private Sprite _exclamationSprite;

        private void Awake()
        {
            _target =
                GetComponent<HypnosisTarget>();

            _impulse =
                GetComponent<ImpulseMeter>();

            CreateGauge();
            CreateIcons();
            MatchIconVisualSize();
            UpdateVisuals();
        }

        private void LateUpdate()
        {
            UpdateVisuals();
        }

        private void CreateGauge()
        {
            _gaugeRoot =
                new GameObject(
                    "StatusGauge");

            _gaugeRoot.transform.SetParent(
                transform,
                false);

            _gaugeRoot.transform.localPosition =
                new Vector3(
                    _gaugeOffset.x,
                    _gaugeOffset.y,
                    0f);

            CreateBar(
                _gaugeRoot.transform,
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
                _gaugeRoot.transform,
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
                    _gaugeRoot.transform,
                    "Fill",
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

            _fillRenderer =
                fill.GetComponent<SpriteRenderer>();
        }

        private void CreateIcons()
        {
            _heartRenderer =
                CreateIconRenderer(
                    "Heart",
                    "UI/Hypnosis/Heart",
                    out _heartSprite);

            _exclamationRenderer =
                CreateIconRenderer(
                    "Exclamation",
                    "UI/Hypnosis/Exclamation",
                    out _exclamationSprite);
        }

        private SpriteRenderer CreateIconRenderer(
            string objectName,
            string resourcePath,
            out Sprite runtimeSprite)
        {
            runtimeSprite = null;

            GameObject icon =
                new GameObject(
                    objectName);

            icon.transform.SetParent(
                transform,
                false);

            icon.transform.localPosition =
                new Vector3(
                    _iconOffset.x,
                    _iconOffset.y,
                    0f);

            SpriteRenderer renderer =
                icon.AddComponent<SpriteRenderer>();

            Texture2D texture =
                Resources.Load<Texture2D>(
                    resourcePath);

            if (texture != null)
            {
                runtimeSprite =
                    Sprite.Create(
                        texture,
                        new Rect(
                            0f,
                            0f,
                            texture.width,
                            texture.height),
                        new Vector2(
                            0.5f,
                            0.5f),
                        320f);

                runtimeSprite.name =
                    objectName +
                    "_RuntimeSprite";

                renderer.sprite =
                    runtimeSprite;
            }

            renderer.sortingOrder =
                12;

            renderer.enabled =
                false;

            return renderer;
        }

        private void MatchIconVisualSize()
        {
            if (_heartRenderer == null ||
                _exclamationRenderer == null ||
                _heartRenderer.sprite == null ||
                _exclamationRenderer.sprite == null)
            {
                return;
            }

            Vector2 heartSize =
                _heartRenderer.sprite.bounds.size;

            Vector2 exclamationSize =
                _exclamationRenderer.sprite.bounds.size;

            float scaleX =
                exclamationSize.x <= 0.0001f
                    ? 1f
                    : heartSize.x /
                      exclamationSize.x;

            float scaleY =
                exclamationSize.y <= 0.0001f
                    ? 1f
                    : heartSize.y /
                      exclamationSize.y;

            _exclamationRenderer.transform.localScale =
                new Vector3(
                    scaleX,
                    scaleY,
                    1f);
        }

        private void UpdateVisuals()
        {
            if (_target == null ||
                _fillTransform == null)
            {
                return;
            }

            bool isHypnotized =
                _target.IsHypnotized;

            bool hasImpulse =
                _impulse != null &&
                _target.IsFollowing;

            bool showWarningIcon =
                hasImpulse &&
                _impulse.IsWarningIconVisible;

            float progress =
                !isHypnotized
                    ? _target.HypnosisNormalized
                    : hasImpulse
                        ? _impulse.ImpulseNormalized
                        : 0f;

            bool showGauge =
                !isHypnotized ||
                hasImpulse;

            if (_gaugeRoot != null)
            {
                _gaugeRoot.SetActive(
                    showGauge);
            }

            if (showGauge)
            {
                float width =
                    _gaugeWidth *
                    Mathf.Clamp01(
                        progress);

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

                if (_fillRenderer != null)
                {
                    _fillRenderer.color =
                        GetGaugeColor(
                            isHypnotized,
                            hasImpulse);
                }
            }

            if (_heartRenderer != null)
            {
                _heartRenderer.enabled =
                    isHypnotized &&
                    !showWarningIcon;
            }

            if (_exclamationRenderer != null)
            {
                _exclamationRenderer.enabled =
                    isHypnotized &&
                    showWarningIcon;
            }
        }

        private Color GetGaugeColor(
            bool isHypnotized,
            bool hasImpulse)
        {
            if (!isHypnotized)
            {
                return new Color(
                    0.69f,
                    0.20f,
                    1.00f,
                    1f);
            }

            if (!hasImpulse ||
                _impulse == null)
            {
                return new Color(
                    1.00f,
                    0.52f,
                    0.85f,
                    1f);
            }

            switch (_impulse.State)
            {
                case ImpulseState.Danger:
                case ImpulseState.Preparing:
                case ImpulseState.Rampaging:
                    return new Color(
                        1.00f,
                        0.46f,
                        0.46f,
                        1f);

                case ImpulseState.Warning:
                    return new Color(
                        1.00f,
                        0.76f,
                        0.35f,
                        1f);

                case ImpulseState.Recovering:
                    return new Color(
                        1.00f,
                        0.56f,
                        0.72f,
                        1f);

                case ImpulseState.Calm:
                default:
                    return new Color(
                        1.00f,
                        0.52f,
                        0.85f,
                        1f);
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

            renderer.color =
                color;

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
                        "StatusGaugeSquare",
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
                Destroy(
                    _heartSprite);
            }

            if (_exclamationSprite != null)
            {
                Destroy(
                    _exclamationSprite);
            }
        }
    }
}
