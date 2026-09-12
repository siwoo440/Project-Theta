using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ProjectTheta.UI.Framework
{
    /// <summary>게이지 하나를 이루는 조각 묶음이다.</summary>
    public struct UiBar
    {
        public RectTransform Root;
        public Image Track;
        public Image Fill;

        public void SetValue(
            float normalized)
        {
            if (Fill != null)
            {
                Fill.fillAmount =
                    Mathf.Clamp01(
                        normalized);
            }
        }

        public void SetColor(
            Color color)
        {
            if (Fill != null)
            {
                Fill.color = color;
            }
        }
    }

    /// <summary>버튼 하나를 이루는 조각 묶음이다.</summary>
    public struct UiButton
    {
        public Button Button;
        public Image Background;
        public Text Label;

        public void SetInteractable(
            bool value)
        {
            if (Button == null)
            {
                return;
            }

            Button.interactable = value;

            if (Label != null)
            {
                Label.color =
                    value
                        ? UiTheme.TextPrimary
                        : UiTheme.TextDisabled;
            }
        }

        public void SetText(
            string value)
        {
            if (Label != null)
            {
                Label.text = value;
            }
        }
    }

    /// <summary>
    /// 코드로 uGUI 화면을 조립하는 도구다.
    ///
    /// 이 프로젝트는 씬 파일을 비워 두고 런타임에 모든 것을 만든다.
    /// 프리팹 대신 이 팩터리가 그 역할을 한다.
    /// </summary>
    public static class UiFactory
    {
        private static Sprite _whiteSprite;

        /// <summary>1x1 흰색 스프라이트다. 색만 바꿔 사각형 패널로 쓴다.</summary>
        public static Sprite WhiteSprite
        {
            get
            {
                if (_whiteSprite != null)
                {
                    return _whiteSprite;
                }

                Texture2D texture =
                    new Texture2D(
                        1,
                        1,
                        TextureFormat.RGBA32,
                        false)
                    {
                        name = "UiWhite",
                        hideFlags = HideFlags.DontSave,
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp
                    };

                texture.SetPixel(
                    0,
                    0,
                    Color.white);

                texture.Apply();

                _whiteSprite =
                    Sprite.Create(
                        texture,
                        new Rect(0f, 0f, 1f, 1f),
                        new Vector2(0.5f, 0.5f),
                        1f);

                _whiteSprite.name = "UiWhite";
                _whiteSprite.hideFlags = HideFlags.DontSave;

                return _whiteSprite;
            }
        }

        // 캔버스 ---------------------------------------------------------

        /// <summary>
        /// 화면 전체를 덮는 캔버스를 만든다.
        /// 기준 해상도로 배치하면 어떤 창 크기에서도 같은 비율로 보인다.
        /// </summary>
        public static Canvas CreateCanvas(
            string name,
            int sortOrder,
            Transform parent = null)
        {
            GameObject root =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

            if (parent != null)
            {
                root.transform.SetParent(
                    parent,
                    false);
            }

            Canvas canvas =
                root.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler =
                root.GetComponent<CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            scaler.referenceResolution =
                UiTheme.ReferenceResolution;

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            // 0.5 = 가로세로 변화를 똑같이 반영한다. 세로가 짧아도 글자가 넘치지 않는다.
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            return canvas;
        }

        /// <summary>버튼 클릭을 받으려면 씬에 EventSystem이 하나 있어야 한다.</summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem =
                new GameObject(
                    "EventSystem",
                    typeof(EventSystem));

#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<
                InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<
                StandaloneInputModule>();
#endif
        }

        // 기본 조각 ------------------------------------------------------

        public static RectTransform CreateRect(
            Transform parent,
            string name)
        {
            GameObject go =
                new GameObject(
                    name,
                    typeof(RectTransform));

            RectTransform rect =
                go.GetComponent<RectTransform>();

            rect.SetParent(
                parent,
                false);

            return rect;
        }

        public static Image CreateImage(
            Transform parent,
            string name,
            Color color)
        {
            GameObject go =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Image));

            go.transform.SetParent(
                parent,
                false);

            Image image =
                go.GetComponent<Image>();

            image.sprite = WhiteSprite;
            image.type = Image.Type.Simple;
            image.color = color;

            // 장식용 그림이 클릭을 가로채지 않게 한다.
            // 클릭을 받아야 하는 버튼과 가림막에서만 따로 켠다.
            image.raycastTarget = false;

            return image;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            FontStyle style = FontStyle.Normal)
        {
            GameObject go =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Text));

            go.transform.SetParent(
                parent,
                false);

            Text text =
                go.GetComponent<Text>();

            text.font = UiFontProvider.Get();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.fontStyle = style;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;

            return text;
        }

        /// <summary>테두리가 있는 패널을 만든다. 테두리는 바깥쪽 이미지로 표현한다.</summary>
        public static RectTransform CreatePanel(
            Transform parent,
            string name,
            Color fill,
            Color edge,
            float edgeThickness = 2f)
        {
            Image border =
                CreateImage(
                    parent,
                    name,
                    edge);

            Image inner =
                CreateImage(
                    border.transform,
                    "Fill",
                    fill);

            RectTransform innerRect =
                inner.rectTransform;

            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;

            innerRect.offsetMin =
                new Vector2(
                    edgeThickness,
                    edgeThickness);

            innerRect.offsetMax =
                new Vector2(
                    -edgeThickness,
                    -edgeThickness);

            return border.rectTransform;
        }

        /// <summary>가로로 차오르는 게이지를 만든다.</summary>
        public static UiBar CreateBar(
            Transform parent,
            string name,
            Color fillColor,
            Color trackColor)
        {
            Image track =
                CreateImage(
                    parent,
                    name,
                    trackColor);

            Image fill =
                CreateImage(
                    track.transform,
                    "Fill",
                    fillColor);

            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            RectTransform fillRect =
                fill.rectTransform;

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;

            fillRect.offsetMin =
                new Vector2(2f, 2f);

            fillRect.offsetMax =
                new Vector2(-2f, -2f);

            return new UiBar
            {
                Root = track.rectTransform,
                Track = track,
                Fill = fill
            };
        }

        public static UiButton CreateButton(
            Transform parent,
            string name,
            string label,
            int fontSize = UiTheme.FontBody,
            bool primary = false)
        {
            Image background =
                CreateImage(
                    parent,
                    name,
                    primary
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal);

            background.raycastTarget = true;

            Button button =
                background.gameObject.AddComponent<Button>();

            button.targetGraphic = background;

            ColorBlock colors =
                button.colors;

            colors.normalColor = Color.white;

            colors.highlightedColor =
                Divide(
                    primary
                        ? UiTheme.PrimaryButtonHighlight
                        : UiTheme.ButtonHighlight,
                    primary
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal);

            colors.pressedColor =
                Divide(
                    UiTheme.ButtonPressed,
                    primary
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal);

            colors.disabledColor =
                Divide(
                    UiTheme.ButtonDisabled,
                    primary
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal);

            colors.fadeDuration = 0.08f;

            button.colors = colors;

            Text text =
                CreateText(
                    background.transform,
                    "Label",
                    label,
                    fontSize,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            Stretch(
                text.rectTransform);

            return new UiButton
            {
                Button = button,
                Background = background,
                Label = text
            };
        }

        // 배치 도우미 ----------------------------------------------------

        /// <summary>부모를 가득 채운다.</summary>
        public static void Stretch(
            RectTransform rect,
            float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            rect.offsetMin =
                new Vector2(padding, padding);

            rect.offsetMax =
                new Vector2(-padding, -padding);
        }

        /// <summary>
        /// 한 지점에 고정하고 크기를 지정한다.
        /// anchor는 (0,0)이 좌하단, (1,1)이 우상단이다.
        /// </summary>
        public static void Place(
            RectTransform rect,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 offset,
            Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }

        /// <summary>가로로는 부모를 채우고 세로 위치와 높이만 지정한다.</summary>
        public static void PlaceRow(
            RectTransform rect,
            float topOffset,
            float height,
            float horizontalPadding = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);

            rect.offsetMin =
                new Vector2(
                    horizontalPadding,
                    0f);

            rect.offsetMax =
                new Vector2(
                    -horizontalPadding,
                    0f);

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    -topOffset);

            rect.sizeDelta =
                new Vector2(
                    rect.sizeDelta.x,
                    height);
        }

        /// <summary>
        /// Button.colors는 targetGraphic 색에 곱해진다.
        /// 그래서 원하는 최종 색을 얻으려면 바탕색으로 나눈 값을 넣어야 한다.
        /// </summary>
        private static Color Divide(
            Color target,
            Color basis)
        {
            return new Color(
                Safe(target.r, basis.r),
                Safe(target.g, basis.g),
                Safe(target.b, basis.b),
                1f);
        }

        private static float Safe(
            float target,
            float basis)
        {
            return basis <= 0.0001f
                ? 1f
                : Mathf.Clamp(
                    target / basis,
                    0f,
                    2f);
        }
    }
}
