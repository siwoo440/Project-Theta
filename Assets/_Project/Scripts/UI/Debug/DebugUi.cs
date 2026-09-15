using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>
    /// 디버그 패널 탭들이 쓰는 배치 도우미다 (20일차).
    ///
    /// 탭 안의 모든 조각은 "왼쪽 위 기준 (x, y)"로 놓는다. y는 아래로 갈수록 커진다.
    /// 목업의 줄 단위 배치를 그대로 옮기기 쉽게 하려고 이렇게 했다.
    /// </summary>
    public static class DebugUi
    {
        public const float ContentWidth = 440f;
        public const float RowHeight = 26f;
        public const float SectionGap = 12f;

        public static readonly Color Background =
            new Color(0.055f, 0.050f, 0.110f, 0.88f);

        public static readonly Color SameFloor =
            new Color(0.42f, 0.88f, 0.56f, 1f);

        /// <summary>왼쪽 위 기준으로 놓는다.</summary>
        public static void PlaceTopLeft(
            RectTransform rect,
            float x,
            float y,
            float width,
            float height)
        {
            UiFactory.Place(
                rect,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(x, -y),
                new Vector2(width, height));
        }

        public static Text Label(
            Transform parent,
            string text,
            float x,
            float y,
            float width,
            Color color,
            int fontSize = UiTheme.FontSmall,
            TextAnchor anchor = TextAnchor.MiddleLeft,
            bool bold = false)
        {
            Text label =
                UiFactory.CreateText(
                    parent,
                    "Label",
                    text,
                    fontSize,
                    color,
                    anchor,
                    bold
                        ? FontStyle.Bold
                        : FontStyle.Normal);

            PlaceTopLeft(
                label.rectTransform,
                x,
                y,
                width,
                RowHeight);

            return label;
        }

        public static UiBar Bar(
            Transform parent,
            float x,
            float y,
            float width,
            Color color)
        {
            UiBar bar =
                UiFactory.CreateBar(
                    parent,
                    "Bar",
                    color,
                    UiTheme.TrackFill);

            PlaceTopLeft(
                bar.Root,
                x,
                y + 6f,
                width,
                RowHeight - 12f);

            return bar;
        }

        /// <summary>
        /// 버튼을 만든다. 누른 뒤 선택을 풀어서, 스페이스·엔터가 같은 치트를 또 누르지 않게 한다.
        /// (스페이스는 대시 키다.)
        /// </summary>
        public static UiButton Button(
            Transform parent,
            string label,
            float x,
            float y,
            float width,
            float height,
            Action onClick,
            bool primary = false)
        {
            UiButton button =
                UiFactory.CreateButton(
                    parent,
                    "Button",
                    label,
                    UiTheme.FontSmall,
                    primary);

            PlaceTopLeft(
                button.Background.rectTransform,
                x,
                y,
                width,
                height);

            Navigation navigation =
                button.Button.navigation;

            navigation.mode =
                Navigation.Mode.None;

            button.Button.navigation = navigation;

            button.Button.onClick.AddListener(
                () =>
                {
                    onClick?.Invoke();

                    if (EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(
                            null);
                    }
                });

            return button;
        }

        /// <summary>
        /// 묶음 제목과 그 아래 구분선을 놓고, 다음 줄의 y를 돌려준다.
        /// </summary>
        public static float Section(
            Transform parent,
            string title,
            float y)
        {
            Label(
                parent,
                "▶ " + title,
                0f,
                y,
                ContentWidth,
                UiTheme.Gold,
                UiTheme.FontSmall,
                TextAnchor.MiddleLeft,
                true);

            Image line =
                UiFactory.CreateImage(
                    parent,
                    "Line",
                    new Color(
                        UiTheme.Gold.r,
                        UiTheme.Gold.g,
                        UiTheme.Gold.b,
                        0.25f));

            PlaceTopLeft(
                line.rectTransform,
                0f,
                y + RowHeight,
                ContentWidth,
                1f);

            return y + RowHeight + 6f;
        }

        /// <summary>켜진 버튼은 금색, 꺼진 버튼은 기본색으로 칠한다.</summary>
        public static void SetToggleLook(
            UiButton button,
            bool on,
            string label)
        {
            button.SetText(label);

            if (button.Background != null)
            {
                button.Background.color =
                    on
                        ? new Color(0.62f, 0.46f, 0.14f, 1f)
                        : UiTheme.ButtonNormal;
            }

            if (button.Label != null)
            {
                button.Label.color =
                    on
                        ? UiTheme.Gold
                        : UiTheme.TextPrimary;
            }
        }

        public static string Percent(
            float normalized)
        {
            return $"{Mathf.RoundToInt(Mathf.Clamp01(normalized) * 100f)}%";
        }
    }

    /// <summary>디버그 패널의 탭 한 장이다.</summary>
    public interface IDebugTab
    {
        string Title { get; }

        void Build(
            RectTransform root);

        /// <summary>보이는 동안 1초에 10번 불린다.</summary>
        void Refresh();
    }
}
