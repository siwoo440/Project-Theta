using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTheta.UI.Framework
{
    /// <summary>덮개 창 하나를 이루는 조각 묶음이다.</summary>
    public struct UiOverlayParts
    {
        public GameObject Root;
        public RectTransform Window;
        public UiButton Close;
    }

    /// <summary>
    /// 화면 위에 덮는 창의 공통 틀이다 (31일차).
    /// 어두운 막(뒤 클릭 차단) · 가운데 패널 · 제목 · 닫기 버튼을 만든다.
    /// 설정 · 조작법 · 일시정지 창이 같이 쓴다.
    /// </summary>
    public static class UiOverlay
    {
        public static UiOverlayParts Create(
            Transform parent,
            string name,
            int sortingOrder,
            Vector2 size,
            string title,
            Action onClose)
        {
            GameObject overlay =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(GraphicRaycaster));

            overlay.transform.SetParent(parent, false);
            UiFactory.Stretch((RectTransform)overlay.transform);

            Canvas canvas = overlay.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            Image dim =
                UiFactory.CreateImage(
                    overlay.transform,
                    "Dim",
                    new Color(0f, 0f, 0f, 0.72f));

            dim.raycastTarget = true;
            UiFactory.Stretch(dim.rectTransform);

            RectTransform window =
                UiFactory.CreatePanel(
                    overlay.transform,
                    "Window",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            UiFactory.Place(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);

            Text heading =
                UiFactory.CreateText(
                    window,
                    "Title",
                    title,
                    UiTheme.FontHeading + 6,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -22f), new Vector2(size.x - 260f, 48f));

            UiButton close =
                UiFactory.CreateButton(
                    window,
                    "Close",
                    "닫기  (Esc)",
                    UiTheme.FontBody);

            UiFactory.Place(close.Background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -24f), new Vector2(170f, 46f));

            if (onClose != null)
            {
                close.Button.onClick.AddListener(() => onClose());
            }

            overlay.SetActive(false);

            return new UiOverlayParts
            {
                Root = overlay,
                Window = window,
                Close = close
            };
        }

        /// <summary>창 안에 왼쪽 정렬 글자를 둔다. top은 창 위에서부터의 거리다.</summary>
        public static Text Label(
            RectTransform window,
            string content,
            float x,
            float top,
            float width,
            int fontSize,
            Color color,
            bool bold = false,
            TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            Text text =
                UiFactory.CreateText(
                    window,
                    "Label",
                    content,
                    fontSize,
                    color,
                    anchor,
                    bold ? FontStyle.Bold : FontStyle.Normal);

            UiFactory.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -top), new Vector2(width, 36f));

            return text;
        }

        public static UiButton Button(
            RectTransform window,
            string name,
            string label,
            float x,
            float top,
            float width,
            float height,
            Action onClick,
            bool primary = false)
        {
            UiButton button =
                UiFactory.CreateButton(
                    window,
                    name,
                    label,
                    UiTheme.FontBody,
                    primary);

            UiFactory.Place(button.Background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -top), new Vector2(width, height));

            if (onClick != null)
            {
                button.Button.onClick.AddListener(() => onClick());
            }

            return button;
        }
    }
}
