using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.UI
{
    [RequireComponent(typeof(HypnosisTarget))]
    public sealed class NpcHypnosisStatusView : MonoBehaviour
    {
        [SerializeField] private float _gaugeWidth = 0.92f;
        [SerializeField] private float _gaugeHeight = 0.075f;

        [SerializeField] private Vector2 _gaugeOffset =
            new Vector2(0f, -0.18f);

        [SerializeField] private Vector2 _ownershipGaugeOffset =
            new Vector2(0f, -0.32f);

        [SerializeField] private Vector2 _iconOffset =
            new Vector2(0.43f, 1.67f);

        [SerializeField] private Vector2 _gradeMarkerOffset =
            new Vector2(-0.43f, 1.67f);

        [SerializeField] private float _gradeMarkerSize = 0.17f;

        [SerializeField] private float _traitPipSize = 0.11f;
        [SerializeField] private float _traitPipSpacing = 0.15f;

        private HypnosisTarget _target;
        private ImpulseMeter _impulse;
        private NpcProfile _profile;

        private SpriteRenderer _gradeMarkerRenderer;

        private GameObject _gaugeRoot;
        private Transform _fillTransform;
        private SpriteRenderer _fillRenderer;

        private GameObject _ownershipGaugeRoot;
        private Transform _ownershipFillTransform;
        private SpriteRenderer _ownershipFillRenderer;

        private SpriteRenderer _heartRenderer;
        private SpriteRenderer _exclamationRenderer;

        private static Sprite _squareSprite;

        // 마지막으로 적용한 값이다. 같으면 렌더러에 다시 쓰지 않는다.
        private static readonly Color UnsetColor =
            new Color(-1f, -1f, -1f, -1f);

        private float _appliedPrimaryProgress = -1f;
        private float _appliedOwnershipProgress = -1f;
        private Color _appliedPrimaryColor = UnsetColor;
        private Color _appliedOwnershipColor = UnsetColor;
        private Color _appliedHeartColor = UnsetColor;
        private Color _appliedExclamationColor = UnsetColor;
        private Color _appliedGradeColor = UnsetColor;
        private Sprite _heartSprite;
        private Sprite _exclamationSprite;

        private void Awake()
        {
            _target =
                GetComponent<HypnosisTarget>();

            _impulse =
                GetComponent<ImpulseMeter>();

            _profile =
                GetComponent<NpcProfile>();

            CreatePrimaryGauge();
            CreateOwnershipGauge();
            CreateGradeMarker();
            CreateIcons();
            MatchIconVisualSize();
            UpdateVisuals();
        }

        private void LateUpdate()
        {
            UpdateVisuals();
        }

        private void CreatePrimaryGauge()
        {
            CreateGauge(
                "StatusGauge",
                _gaugeOffset,
                out _gaugeRoot,
                out _fillTransform,
                out _fillRenderer);
        }

        private void CreateOwnershipGauge()
        {
            CreateGauge(
                "OwnershipGauge",
                _ownershipGaugeOffset,
                out _ownershipGaugeRoot,
                out _ownershipFillTransform,
                out _ownershipFillRenderer);
        }

        private void CreateGauge(
            string name,
            Vector2 offset,
            out GameObject root,
            out Transform fillTransform,
            out SpriteRenderer fillRenderer)
        {
            root =
                new GameObject(
                    name);

            root.transform.SetParent(
                transform,
                false);

            root.transform.localPosition =
                new Vector3(
                    offset.x,
                    offset.y,
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
                    "Fill",
                    new Vector2(
                        -_gaugeWidth *
                        0.5f,
                        0f),
                    new Vector2(
                        0f,
                        _gaugeHeight),
                    Color.white,
                    10);

            fillTransform =
                fill.transform;

            fillRenderer =
                fill.GetComponent<
                    SpriteRenderer>();
        }

        /// <summary>NPC 머리 위에 등급 색상 표식을 만든다.</summary>
        private void CreateGradeMarker()
        {
            if (_profile == null)
            {
                return;
            }

            GameObject marker =
                CreateBar(
                    transform,
                    "GradeMarker",
                    _gradeMarkerOffset,
                    new Vector2(
                        _gradeMarkerSize,
                        _gradeMarkerSize),
                    _profile.GradeMarkerColor,
                    12);

            _gradeMarkerRenderer =
                marker.GetComponent<
                    SpriteRenderer>();

            CreateTraitPips();
        }

        /// <summary>등급 표식 아래에 보유 특성 개수만큼 작은 색상 핍을 만든다.</summary>
        private void CreateTraitPips()
        {
            NpcTrait[] traits =
                _profile.Traits;

            if (traits == null ||
                _profile.IsPlain)
            {
                return;
            }

            for (int i = 0;
                 i < traits.Length;
                 i++)
            {
                CreateBar(
                    transform,
                    "TraitPip_" + traits[i],
                    new Vector2(
                        _gradeMarkerOffset.x,
                        _gradeMarkerOffset.y -
                        (_traitPipSpacing *
                         (i + 1))),
                    new Vector2(
                        _traitPipSize,
                        _traitPipSize),
                    NpcTraitTable.Get(
                        traits[i]).MarkerColor,
                    12);
            }
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
            if (_target == null)
            {
                return;
            }

            // 16일차부터 NPC 52명 중 화면에 보이는 건 지금 층의 한 무리뿐이다.
            // 다른 층 NPC의 게이지는 어차피 안 보이므로 계산하지 않는다.
            // 층을 옮기면 FloorTransitionController가 Update에서 층을 바꾸므로
            // 새 층 NPC는 같은 프레임 LateUpdate에서 바로 갱신된다.
            if (FloorSpace.FloorAt(
                    transform.position.y) !=
                FloorVisibility.ViewFloor)
            {
                return;
            }

            bool playerOwned =
                _target.Owner ==
                NpcOwner.Player;

            bool geumtaeyangOwned =
                _target.Owner ==
                NpcOwner.Geumtaeyang;

            bool popularGuyOwned =
                _target.Owner ==
                NpcOwner.PopularGuy;

            bool isHypnotized =
                _target.Owner !=
                NpcOwner.Neutral;

            bool popularGuyNeutralClaim =
                _target.Owner ==
                    NpcOwner.Neutral &&
                _target.OpponentClaimNormalized >
                    0f;

            bool hasImpulse =
                playerOwned &&
                _impulse != null &&
                _target.IsFollowing;

            bool showImpulseWarning =
                hasImpulse &&
                _impulse.IsWarningIconVisible;

            bool showOpponentWarning =
                _target.IsOpponentTargeted;

            bool showPrimaryGauge =
                _target.Owner ==
                NpcOwner.Neutral ||
                hasImpulse;

            float primaryProgress =
                _target.Owner ==
                NpcOwner.Neutral
                    ? _target.HypnosisNormalized
                    : hasImpulse
                        ? _impulse.ImpulseNormalized
                        : 0f;

            SetActiveIfChanged(
                _gaugeRoot,
                showPrimaryGauge);

            if (showPrimaryGauge)
            {
                SetGaugeProgressIfChanged(
                    _fillTransform,
                    ref _appliedPrimaryProgress,
                    primaryProgress);

                SetColorIfChanged(
                    _fillRenderer,
                    ref _appliedPrimaryColor,
                    GetPrimaryGaugeColor(
                        hasImpulse));
            }

            bool showOwnership =
                isHypnotized ||
                popularGuyNeutralClaim;

            SetActiveIfChanged(
                _ownershipGaugeRoot,
                showOwnership);

            if (showOwnership)
            {
                SetGaugeProgressIfChanged(
                    _ownershipFillTransform,
                    ref _appliedOwnershipProgress,
                    popularGuyNeutralClaim
                        ? _target.OpponentClaimNormalized
                        : _target.HypnosisNormalized);

                SetColorIfChanged(
                    _ownershipFillRenderer,
                    ref _appliedOwnershipColor,
                    popularGuyNeutralClaim
                        ? GetOwnershipColor(
                            NpcOwner.PopularGuy)
                        : GetOwnershipColor(
                            _target.Owner));
            }

            // 등급은 판 도중에 바뀌지 않으므로 사실상 한 번만 쓰인다.
            if (_profile != null)
            {
                SetColorIfChanged(
                    _gradeMarkerRenderer,
                    ref _appliedGradeColor,
                    _profile.GradeMarkerColor);
            }

            SetEnabledIfChanged(
                _heartRenderer,
                isHypnotized &&
                !showImpulseWarning &&
                !showOpponentWarning);

            SetColorIfChanged(
                _heartRenderer,
                ref _appliedHeartColor,
                GetOwnershipColor(
                    _target.Owner));

            SetEnabledIfChanged(
                _exclamationRenderer,
                showImpulseWarning ||
                showOpponentWarning);

            SetColorIfChanged(
                _exclamationRenderer,
                ref _appliedExclamationColor,
                showOpponentWarning
                    ? GetOwnershipColor(
                        _target.PrimaryThreatOwner)
                    : Color.white);
        }

        // 바뀔 때만 쓰기 -----------------------------------------------

        private static void SetActiveIfChanged(
            GameObject target,
            bool active)
        {
            if (target != null &&
                target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void SetEnabledIfChanged(
            SpriteRenderer renderer,
            bool enabled)
        {
            if (renderer != null &&
                renderer.enabled != enabled)
            {
                renderer.enabled = enabled;
            }
        }

        private static void SetColorIfChanged(
            SpriteRenderer renderer,
            ref Color applied,
            Color color)
        {
            if (renderer == null ||
                applied == color)
            {
                return;
            }

            applied = color;

            renderer.color = color;
        }

        /// <summary>게이지 폭이 눈에 띄게(0.5% 이상) 바뀔 때만 transform을 건드린다.</summary>
        private void SetGaugeProgressIfChanged(
            Transform fillTransform,
            ref float applied,
            float progress)
        {
            float clamped =
                Mathf.Clamp01(
                    progress);

            if (Mathf.Abs(
                    clamped -
                    applied) <
                0.005f)
            {
                return;
            }

            applied = clamped;

            SetGaugeProgress(
                fillTransform,
                clamped);
        }

        private Color GetOwnershipColor(
            NpcOwner owner)
        {
            switch (owner)
            {
                case NpcOwner.Geumtaeyang:
                    return new Color(
                        0.96f,
                        0.22f,
                        0.25f,
                        1f);

                case NpcOwner.PopularGuy:
                    return new Color(
                        0.20f,
                        0.84f,
                        0.92f,
                        1f);

                case NpcOwner.Player:
                    return new Color(
                        1.00f,
                        0.42f,
                        0.78f,
                        1f);

                case NpcOwner.Rival:
                    return new Color(
                        0.65f,
                        0.35f,
                        1.00f,
                        1f);

                case NpcOwner.Neutral:
                default:
                    return new Color(
                        1.00f,
                        0.76f,
                        0.35f,
                        1f);
            }
        }

        private void SetGaugeProgress(
            Transform fillTransform,
            float progress)
        {
            if (fillTransform == null)
            {
                return;
            }

            float width =
                _gaugeWidth *
                Mathf.Clamp01(
                    progress);

            fillTransform.localScale =
                new Vector3(
                    width,
                    _gaugeHeight,
                    1f);

            fillTransform.localPosition =
                new Vector3(
                    (-_gaugeWidth *
                     0.5f) +
                    (width *
                     0.5f),
                    0f,
                    0f);
        }

        private Color GetPrimaryGaugeColor(
            bool hasImpulse)
        {
            if (_target.Owner ==
                NpcOwner.Neutral)
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
                case ImpulseState.Capturing:
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
                new GameObject(
                    name);

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
