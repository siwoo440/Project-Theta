using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Presentation;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 월드 공간 글자 조각이다 (22일차). 캐릭터 머리 위 이름표 · `?` `!` · 능력 예고에 쓴다.
    ///
    /// 월드 캔버스 하나에 글자 하나다. 0.01배로 줄여 월드 1칸 = UI 100픽셀로 맞춘다.
    /// 캐릭터를 따라 움직이도록 캐릭터의 자식으로 붙인다.
    /// </summary>
    public static class WorldLabel
    {
        public const float Scale = 0.01f;

        /// <summary>캐릭터 계층(10000±4000)과 NPC 머리 위 계층(20000대)보다 위다. 화면 UI 캔버스와는 따로 그려진다.</summary>
        public const int SortOrder = 24000;

        public static Text Create(
            Transform parent,
            string name,
            Vector2 localOffset,
            int fontSize,
            Color color,
            float width = 400f)
        {
            GameObject root =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Canvas));

            root.transform.SetParent(
                parent,
                false);

            Canvas canvas =
                root.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.WorldSpace;

            canvas.sortingOrder =
                SortOrder;

            RectTransform rect =
                root.GetComponent<RectTransform>();

            rect.sizeDelta =
                new Vector2(width, 60f);

            rect.localPosition =
                new Vector3(
                    localOffset.x,
                    localOffset.y,
                    0f);

            rect.localScale =
                new Vector3(
                    Scale,
                    Scale,
                    1f);

            Text text =
                UiFactory.CreateText(
                    rect,
                    "Text",
                    string.Empty,
                    fontSize,
                    color,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                text.rectTransform);

            // 밝은 바닥 위에서도 읽히게 외곽선을 준다.
            Outline outline =
                text.gameObject.AddComponent<Outline>();

            outline.effectColor =
                new Color(0f, 0f, 0f, 0.85f);

            outline.effectDistance =
                new Vector2(2f, -2f);

            return text;
        }
    }

    /// <summary>
    /// 방해 세력의 머리 위 표시와 바닥 시야 부채꼴이다 (22일차, 부록 C.8).
    ///
    ///   이름표   특수 개체만 금색으로 항상 표시
    ///   `?` `!`  감시 역할의 발각 진행
    ///   능력     예고 중 "근태 체크!"와 진행 막대
    ///   `zZ`     멍한 동안
    ///   시야     바닥에 옅은 부채꼴. 플레이어가 안에 있으면 진해지고, 발각되면 붉어진다
    /// </summary>
    public sealed class DisruptorStatusView : MonoBehaviour
    {
        private const float HeadHeight = 2.05f;

        private static Sprite _coneSprite;

        private DisruptorBase _body;
        private WatcherRole _watcher;
        private SpecialAbility _ability;

        private Text _nameText;
        private Text _stateText;
        private Text _abilityText;

        private SpriteRenderer _cone;

        public void Configure(
            DisruptorBase body)
        {
            _body = body;
            _watcher = GetComponent<WatcherRole>();
            _ability = GetComponent<SpecialAbility>();

            DisruptorProfile profile =
                body.Profile;

            bool special =
                profile != null &&
                profile.IsSpecial;

            _nameText =
                WorldLabel.Create(
                    transform,
                    "NameTag",
                    new Vector2(0f, HeadHeight),
                    special ? 22 : 18,
                    special ? UiTheme.Gold : new Color(0.80f, 0.84f, 0.95f, 0.85f));

            _nameText.text =
                profile == null
                    ? string.Empty
                    : special
                        ? $"◆ {profile.DisplayName}"
                        : profile.DisplayName;

            _stateText =
                WorldLabel.Create(
                    transform,
                    "State",
                    new Vector2(0f, HeadHeight + 0.42f),
                    40,
                    UiTheme.Gold);

            _abilityText =
                WorldLabel.Create(
                    transform,
                    "Ability",
                    new Vector2(0f, HeadHeight + 0.85f),
                    24,
                    UiTheme.Danger,
                    520f);

            if (_watcher != null)
            {
                CreateCone();
            }
        }

        private void CreateCone()
        {
            GameObject cone =
                new GameObject(
                    "SightCone");

            cone.transform.SetParent(
                transform,
                false);

            _cone =
                cone.AddComponent<SpriteRenderer>();

            _cone.sprite =
                GetConeSprite();

            // 바닥 위, 캐릭터 아래에 깐다.
            _cone.sortingOrder = -40;
        }

        private void LateUpdate()
        {
            if (_body == null)
            {
                return;
            }

            UpdateState();
            UpdateAbility();
            UpdateCone();
        }

        private void UpdateState()
        {
            string state = string.Empty;
            Color color = UiTheme.Gold;

            if (_body.IsStunned)
            {
                state = "zZ";
                color = new Color(0.60f, 0.80f, 1.00f);
            }
            else if (_watcher != null &&
                     _watcher.IsSpotting)
            {
                state = "!";
                color = UiTheme.Danger;
            }
            else if (_watcher != null &&
                     _watcher.SuspicionProgress > 0.01f)
            {
                state = "?";

                // 발각에 가까울수록 노랑 → 주황으로 짙어진다.
                color =
                    Color.Lerp(
                        UiTheme.Gold,
                        new Color(1f, 0.55f, 0.20f),
                        _watcher.SuspicionProgress);
            }

            if (_stateText.text != state)
            {
                _stateText.text = state;
            }

            _stateText.color = color;
        }

        private void UpdateAbility()
        {
            if (_ability == null)
            {
                return;
            }

            string text = string.Empty;

            if (!_body.IsStunned &&
                _ability.Phase == AbilityPhase.Telegraph)
            {
                // 진행 막대를 글자로 그린다. 이미지 하나 더 만드는 것보다 가볍고 읽기 쉽다.
                int filled =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            _ability.TelegraphProgress * 8f),
                        0,
                        8);

                text =
                    $"{_ability.DisplayName}!  {new string('■', filled)}{new string('□', 8 - filled)}";
            }

            if (_abilityText.text != text)
            {
                _abilityText.text = text;
            }
        }

        private void UpdateCone()
        {
            if (_cone == null)
            {
                return;
            }

            bool visible =
                !_body.IsStunned;

            if (_cone.enabled != visible)
            {
                _cone.enabled = visible;
            }

            if (!visible)
            {
                return;
            }

            float range =
                _watcher.SightRange;

            float halfWidth =
                range *
                Mathf.Tan(
                    _watcher.SightHalfAngle *
                    Mathf.Deg2Rad);

            // 스프라이트는 왼쪽 가운데가 원점인 1×1 칸 부채꼴이다. 바라보는 쪽으로 늘린다.
            _cone.transform.localScale =
                new Vector3(
                    range * _body.Facing,
                    halfWidth * 2f,
                    1f);

            Color color =
                _watcher.IsSpotting
                    ? new Color(1.00f, 0.25f, 0.25f, 0.30f)
                    : _watcher.PlayerInSight
                        ? new Color(1.00f, 0.85f, 0.35f, 0.22f)
                        : new Color(1.00f, 1.00f, 1.00f, 0.08f);

            _cone.color = color;
        }

        /// <summary>
        /// 부채꼴 모양 스프라이트를 코드로 한 번 만든다.
        /// 왼쪽 가운데가 꼭짓점이고 오른쪽으로 갈수록 넓어지며, 끝으로 갈수록 흐려진다.
        /// </summary>
        private static Sprite GetConeSprite()
        {
            if (_coneSprite != null)
            {
                return _coneSprite;
            }

            const int width = 96;
            const int height = 96;

            Texture2D texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false)
                {
                    name = "SightCone",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

            Color32[] pixels =
                new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (x + 0.5f) / width;
                    float v = ((y + 0.5f) / height) * 2f - 1f;

                    // 가로 위치 u에서 부채꼴의 세로 반폭은 u다.
                    float edge = Mathf.Abs(v) - u;

                    float alpha =
                        edge > 0f
                            ? 0f
                            : Mathf.Clamp01(-edge * 6f) * (1f - u * 0.7f);

                    pixels[y * width + x] =
                        new Color32(
                            255,
                            255,
                            255,
                            (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            _coneSprite =
                Sprite.Create(
                    texture,
                    new Rect(0f, 0f, width, height),
                    new Vector2(0f, 0.5f),
                    width);

            _coneSprite.hideFlags = HideFlags.DontSave;

            return _coneSprite;
        }
    }
}
