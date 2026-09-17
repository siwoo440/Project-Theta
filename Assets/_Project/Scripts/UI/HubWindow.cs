using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 허브의 반투명 창 하나다 (36일차). 강화 · 통계 · 난이도 · 일기장이 같이 쓴다.
    ///
    /// 뒤는 <see cref="HubRoomLogic.WindowDim"/>만큼만 어둡게 해 방이 계속 보인다.
    /// 어두운 곳이나 [×]를 누르면 <c>onClose</c>를 부른다(열고 닫는 판단은 허브가 한다).
    /// </summary>
    public sealed class HubWindow
    {
        public GameObject Root { get; private set; }

        /// <summary>내용을 넣는 창이다. 위쪽 60px은 제목 자리다.</summary>
        public RectTransform Body { get; private set; }

        public Vector2 Size { get; private set; }

        public bool IsOpen =>
            Root != null &&
            Root.activeSelf;

        public static HubWindow Create(
            Transform parent,
            string name,
            string title,
            Vector2 size,
            Action onClose)
        {
            HubWindow window = new HubWindow { Size = size };

            window.Root = new GameObject(name, typeof(RectTransform));
            window.Root.transform.SetParent(parent, false);
            UiFactory.Stretch((RectTransform)window.Root.transform);

            UiButton dim = UiFactory.CreateButton(window.Root.transform, "Dim", string.Empty);
            UiFactory.Stretch(dim.Background.rectTransform);
            dim.Background.color = new Color(0f, 0f, 0f, HubRoomLogic.WindowDim);
            dim.Button.transition = Selectable.Transition.None;

            // 어두운 곳을 눌러도 닫힌다. 확대 효과는 빼서 화면이 흔들리지 않게 한다.
            UnityEngine.Object.Destroy(dim.Background.GetComponent<UiHoverEffect>());

            if (onClose != null)
            {
                dim.Button.onClick.AddListener(() => onClose());
            }

            RectTransform body =
                UiFactory.CreatePanel(
                    window.Root.transform,
                    "Window",
                    new Color(0.06f, 0.045f, 0.10f, HubRoomLogic.WindowAlpha),
                    new Color(UiTheme.AccentSoft.r, UiTheme.AccentSoft.g, UiTheme.AccentSoft.b, 0.9f));

            UiFactory.Place(body, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), size);

            // 창 자체를 눌렀을 때 뒤의 어두운 곳으로 눌림이 넘어가지 않게 한다.
            body.GetComponent<Image>().raycastTarget = true;

            window.Body = body;

            Text heading = UiFactory.CreateText(body, "Title", title, UiTheme.FontHeading, UiTheme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Place(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -14f), new Vector2(size.x - 120f, 40f));

            UiButton close = UiFactory.CreateButton(body, "Close", "×", UiTheme.FontHeading);
            UiFactory.Place(close.Background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -14f), new Vector2(44f, 44f));

            if (onClose != null)
            {
                close.Button.onClick.AddListener(() => onClose());
            }

            UiDecor.CreateDivider(body, "Divider", new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.45f), 64f, 28f);

            window.Root.SetActive(false);

            return window;
        }

        public void SetOpen(
            bool open)
        {
            if (Root != null)
            {
                Root.SetActive(open);
            }
        }
    }
}
