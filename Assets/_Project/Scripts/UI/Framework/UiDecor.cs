using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectTheta.Presentation;

namespace ProjectTheta.UI.Framework
{
    /// <summary>
    /// 화면을 꾸미는 장식 조각이다 (20일차).
    ///
    /// 새 그림 파일 없이 19일차에 만든 연출 그림(<see cref="VfxLibrary"/>)을 재사용한다.
    /// 장식은 전부 클릭을 받지 않는다(raycastTarget = false).
    /// 카드 화면처럼 시간이 멈춘 곳에서도 움직여야 하므로 전부 실제 시간(unscaled)으로 움직인다.
    /// </summary>
    public static class UiDecor
    {
        /// <summary>가장자리가 흐린 원형 빛이다. 강조하고 싶은 것 뒤에 깐다.</summary>
        public static Image CreateGlow(
            Transform parent,
            string name,
            Color color,
            Vector2 size)
        {
            Image glow =
                UiFactory.CreateImage(
                    parent,
                    name,
                    color);

            glow.sprite =
                VfxLibrary.Get(
                    VfxSprite.SoftCircle);

            glow.rectTransform.sizeDelta = size;

            return glow;
        }

        /// <summary>제목 아래 한 줄 구분선이다. 가운데가 진하고 양끝이 흐리다.</summary>
        public static Image CreateDivider(
            Transform parent,
            string name,
            Color color,
            float topOffset,
            float horizontalPadding = 0f)
        {
            Image line =
                UiFactory.CreateImage(
                    parent,
                    name,
                    color);

            line.sprite =
                VfxLibrary.Get(
                    VfxSprite.Pillar);

            // 세로 빛기둥 그림을 눕혀 가운데가 밝은 선으로 쓴다.
            UiFactory.PlaceRow(
                line.rectTransform,
                topOffset,
                2f,
                horizontalPadding);

            return line;
        }

        /// <summary>작은 반짝임 장식이다. 카드 모서리 등에 붙인다.</summary>
        public static Image CreateSpark(
            Transform parent,
            string name,
            Color color,
            Vector2 anchor,
            Vector2 offset,
            float size)
        {
            Image spark =
                UiFactory.CreateImage(
                    parent,
                    name,
                    color);

            spark.sprite =
                VfxLibrary.Get(
                    VfxSprite.Spark);

            UiFactory.Place(
                spark.rectTransform,
                anchor,
                new Vector2(0.5f, 0.5f),
                offset,
                new Vector2(size, size));

            return spark;
        }

        /// <summary>패널 아래에 은은한 그림자를 단다.</summary>
        public static void AddShadow(
            Graphic graphic,
            float distance = 5f)
        {
            if (graphic == null ||
                graphic.GetComponent<Shadow>() != null)
            {
                return;
            }

            Shadow shadow =
                graphic.gameObject.AddComponent<Shadow>();

            shadow.effectColor =
                new Color(0f, 0f, 0f, 0.45f);

            shadow.effectDistance =
                new Vector2(
                    distance * 0.4f,
                    -distance);
        }
    }

    /// <summary>
    /// 마우스를 올리면 살짝 커지고, 빛이 있으면 밝아진다 (20일차).
    /// 모든 버튼에 붙는다(<see cref="UiFactory.CreateButton"/>).
    /// </summary>
    public sealed class UiHoverEffect :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private const float HoverScale = 1.035f;
        private const float Speed = 14f;

        /// <summary>마우스를 올렸을 때만 보이는 빛이다. 없어도 된다.</summary>
        public Image Glow;

        public float GlowAlpha = 0.35f;

        private bool _hovered;
        private float _amount;
        private Selectable _selectable;

        private void Awake()
        {
            _selectable =
                GetComponent<Selectable>();

            Apply();
        }

        private void OnDisable()
        {
            // 화면이 닫힐 때 커진 채로 남지 않게 한다. 다음에 열면 원래 크기로 시작한다.
            _hovered = false;
            _amount = 0f;

            Apply();
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            _hovered =
                _selectable == null ||
                _selectable.interactable;
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            _hovered = false;
        }

        private void Update()
        {
            if (_hovered &&
                _selectable != null &&
                !_selectable.interactable)
            {
                _hovered = false;
            }

            float target =
                _hovered
                    ? 1f
                    : 0f;

            if (Mathf.Approximately(
                    _amount,
                    target))
            {
                return;
            }

            _amount =
                Mathf.MoveTowards(
                    _amount,
                    target,
                    Speed *
                    Time.unscaledDeltaTime);

            Apply();
        }

        private void Apply()
        {
            float scale =
                Mathf.Lerp(
                    1f,
                    HoverScale,
                    _amount);

            transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f);

            if (Glow != null)
            {
                Color color =
                    Glow.color;

                color.a =
                    GlowAlpha *
                    _amount;

                Glow.color = color;
            }
        }
    }

    /// <summary>투명도를 천천히 오르내리게 한다. 켜져 있는 동안만 움직인다 (20일차).</summary>
    public sealed class UiPulse : MonoBehaviour
    {
        public Graphic Target;
        public float MinimumAlpha = 0.25f;
        public float MaximumAlpha = 0.8f;
        public float Period = 1.4f;

        private float _elapsed;

        private void Update()
        {
            if (Target == null)
            {
                return;
            }

            _elapsed +=
                Time.unscaledDeltaTime;

            float wave =
                0.5f +
                0.5f *
                Mathf.Sin(
                    _elapsed *
                    Mathf.PI *
                    2f /
                    Mathf.Max(
                        0.05f,
                        Period));

            Color color =
                Target.color;

            color.a =
                Mathf.Lerp(
                    MinimumAlpha,
                    MaximumAlpha,
                    wave);

            Target.color = color;
        }
    }

    /// <summary>
    /// 배경에 느리게 떠오르는 빛 알갱이다 (20일차). 허브·타이틀 배경에 쓴다.
    /// 위로 벗어나면 아래에서 다시 나온다. 알갱이 수가 고정이라 매 프레임 할당이 없다.
    /// </summary>
    public sealed class UiFloatingMotes : MonoBehaviour
    {
        private const int Count = 26;

        private RectTransform[] _motes;
        private Image[] _images;
        private float[] _speeds;
        private float[] _phases;
        private float[] _baseAlpha;
        private float _elapsed;

        public void Build(
            Color color)
        {
            if (_motes != null)
            {
                return;
            }

            _motes = new RectTransform[Count];
            _images = new Image[Count];
            _speeds = new float[Count];
            _phases = new float[Count];
            _baseAlpha = new float[Count];

            // 난수 대신 고정 순서를 쓴다. 화면을 열 때마다 배치가 같아 스크린샷 비교가 쉽다.
            for (int i = 0;
                 i < Count;
                 i++)
            {
                float t =
                    (i * 0.618034f) % 1f;

                float size =
                    10f +
                    ((i * 7) % 5) * 7f;

                Image mote =
                    UiFactory.CreateImage(
                        transform,
                        $"Mote{i}",
                        color);

                mote.sprite =
                    VfxLibrary.Get(
                        VfxSprite.SoftCircle);

                RectTransform rect =
                    mote.rectTransform;

                rect.anchorMin =
                    new Vector2(
                        t,
                        0f);

                rect.anchorMax =
                    rect.anchorMin;

                rect.sizeDelta =
                    new Vector2(
                        size,
                        size);

                _motes[i] = rect;
                _images[i] = mote;
                _speeds[i] = 18f + ((i * 13) % 7) * 6f;
                _phases[i] = ((i * 0.382f) % 1f) * UiTheme.ReferenceResolution.y;
                _baseAlpha[i] = color.a * (0.35f + ((i * 3) % 4) * 0.2f);
            }
        }

        private void Update()
        {
            if (_motes == null)
            {
                return;
            }

            _elapsed +=
                Time.unscaledDeltaTime;

            float height =
                UiTheme.ReferenceResolution.y + 80f;

            for (int i = 0;
                 i < _motes.Length;
                 i++)
            {
                float y =
                    ((_phases[i] + _elapsed * _speeds[i]) % height) - 40f;

                float sway =
                    Mathf.Sin(
                        _elapsed * 0.6f +
                        i) *
                    14f;

                _motes[i].anchoredPosition =
                    new Vector2(
                        sway,
                        y);

                // 위로 갈수록 흐려진다.
                Color color =
                    _images[i].color;

                color.a =
                    _baseAlpha[i] *
                    (1f - Mathf.Clamp01(y / height));

                _images[i].color = color;
            }
        }
    }
}
