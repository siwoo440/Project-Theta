using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Save;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 허브 · 메인 메뉴의 업적 목록 창이다 (30일차).
    ///
    /// 32일차: 업적이 46개라 한 줄에 하나씩 세로로 늘어놓고, 마우스 휠 · 스크롤 막대로 내려 본다.
    ///   [이름]  [설명]                      [+보상]  [진행 막대]  [값 / 목표 · 달성]
    /// Esc · 닫기로 닫는다. 열 때마다 맨 위로 돌아간다.
    /// </summary>
    public sealed class AchievementPanel : MonoBehaviour
    {
        private const float Width = 1360f;
        private const float Height = 920f;
        private const float ListTop = 96f;
        private const float ListBottom = 28f;
        private const float ListSide = 36f;
        private const float ScrollbarWidth = 14f;
        private const float RowHeight = 54f;
        private const float RowGap = 6f;
        private const float WheelSensitivity = 40f;

        private sealed class RowView
        {
            public AchievementDefinition Definition;
            public Image Background;
            public Text Name;
            public Text Detail;
            public Text Reward;
            public Text State;
            public UiBar Bar;
        }

        private GameObject _root;
        private Text _summary;
        private ScrollRect _scroll;
        private readonly List<RowView> _rows = new List<RowView>();

        public bool IsOpen =>
            _root != null &&
            _root.activeSelf;

        public void Build(
            Transform canvas)
        {
            GameObject overlay =
                new GameObject(
                    "AchievementOverlay",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(GraphicRaycaster));

            overlay.transform.SetParent(canvas, false);
            UiFactory.Stretch((RectTransform)overlay.transform);

            Canvas overlayCanvas = overlay.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 50;

            _root = overlay;

            Image dim =
                UiFactory.CreateImage(
                    overlay.transform,
                    "Dim",
                    new Color(0f, 0f, 0f, 0.72f));

            dim.raycastTarget = true;
            UiFactory.Stretch(dim.rectTransform);

            RectTransform panel =
                UiFactory.CreatePanel(
                    overlay.transform,
                    "AchievementWindow",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            UiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, Height));

            Text title =
                UiFactory.CreateText(
                    panel,
                    "Title",
                    "업적",
                    UiTheme.FontTitle - 6,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -22f), new Vector2(300f, 56f));

            _summary =
                UiFactory.CreateText(
                    panel,
                    "Summary",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(_summary.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200f, -30f), new Vector2(700f, 40f));

            Text hint =
                UiFactory.CreateText(
                    panel,
                    "Hint",
                    "마우스 휠로 내려 보기",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleRight);

            UiFactory.Place(hint.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-220f, -32f), new Vector2(300f, 36f));

            UiButton close =
                UiFactory.CreateButton(
                    panel,
                    "Close",
                    "닫기  (Esc)",
                    UiTheme.FontBody);

            UiFactory.Place(close.Background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -26f), new Vector2(170f, 48f));
            close.Button.onClick.AddListener(Close);

            BuildList(panel);

            overlay.SetActive(false);
        }

        /// <summary>세로 스크롤 목록이다. 보이는 창(Viewport) 안에서 내용(Content)이 위아래로 움직인다.</summary>
        private void BuildList(
            RectTransform panel)
        {
            // 보이는 창. 밖으로 나간 줄은 잘라 낸다. 휠을 받으려면 클릭을 받는 그림이 있어야 한다.
            Image viewportImage =
                UiFactory.CreateImage(
                    panel,
                    "Viewport",
                    new Color(0f, 0f, 0f, 0.001f));

            viewportImage.raycastTarget = true;

            RectTransform viewport = viewportImage.rectTransform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(ListSide, ListBottom);
            viewport.offsetMax = new Vector2(-ListSide - ScrollbarWidth - 10f, -ListTop);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UiFactory.CreateRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta =
                new Vector2(
                    0f,
                    AchievementLogic.All.Length * (RowHeight + RowGap));

            for (int i = 0; i < AchievementLogic.All.Length; i++)
            {
                _rows.Add(
                    BuildRow(
                        content,
                        AchievementLogic.All[i],
                        i * (RowHeight + RowGap)));
            }

            _scroll = panel.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport;
            _scroll.content = content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = WheelSensitivity;
            _scroll.inertia = true;
            _scroll.decelerationRate = 0.12f;
            _scroll.verticalScrollbar = BuildScrollbar(panel);
            _scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }

        private static Scrollbar BuildScrollbar(
            RectTransform panel)
        {
            Image track =
                UiFactory.CreateImage(
                    panel,
                    "Scrollbar",
                    UiTheme.TrackFill);

            track.raycastTarget = true;

            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(1f, 0f);
            trackRect.anchorMax = new Vector2(1f, 1f);
            trackRect.pivot = new Vector2(1f, 0.5f);
            trackRect.offsetMin = new Vector2(-ListSide - ScrollbarWidth, ListBottom);
            trackRect.offsetMax = new Vector2(-ListSide, -ListTop);

            RectTransform slidingArea = UiFactory.CreateRect(trackRect, "SlidingArea");
            UiFactory.Stretch(slidingArea, 2f);

            Image handle =
                UiFactory.CreateImage(
                    slidingArea,
                    "Handle",
                    UiTheme.AccentSoft);

            handle.raycastTarget = true;
            handle.rectTransform.anchorMin = Vector2.zero;
            handle.rectTransform.anchorMax = Vector2.one;
            handle.rectTransform.offsetMin = Vector2.zero;
            handle.rectTransform.offsetMax = Vector2.zero;

            Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            ColorBlock colors = scrollbar.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = UiTheme.Gold;
            colors.pressedColor = UiTheme.Gold;
            scrollbar.colors = colors;

            // 방향키가 스크롤바로 가지 않게 한다(캐릭터 이동 키와 섞이지 않게).
            Navigation navigation = scrollbar.navigation;
            navigation.mode = Navigation.Mode.None;
            scrollbar.navigation = navigation;

            return scrollbar;
        }

        private static RowView BuildRow(
            RectTransform content,
            AchievementDefinition definition,
            float top)
        {
            Image background =
                UiFactory.CreateImage(
                    content,
                    $"Achievement_{definition.Id}",
                    UiTheme.RowFill);

            RectTransform rect = background.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(0f, RowHeight);

            Text name =
                UiFactory.CreateText(
                    background.transform,
                    "Name",
                    definition.Name,
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(240f, RowHeight));

            Text detail =
                UiFactory.CreateText(
                    background.transform,
                    "Detail",
                    definition.Description,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(detail.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(270f, 0f), new Vector2(480f, RowHeight));

            Text reward =
                UiFactory.CreateText(
                    background.transform,
                    "Reward",
                    $"+{definition.Reward}",
                    UiTheme.FontBody,
                    UiTheme.Gold,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(reward.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-460f, 0f), new Vector2(90f, RowHeight));

            UiBar bar =
                UiFactory.CreateBar(
                    background.transform,
                    "Progress",
                    UiTheme.Gold,
                    UiTheme.TrackFill);

            UiFactory.Place(bar.Root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(240f, 12f));

            Text state =
                UiFactory.CreateText(
                    background.transform,
                    "State",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(state.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(160f, RowHeight));

            return new RowView
            {
                Definition = definition,
                Background = background,
                Name = name,
                Detail = detail,
                Reward = reward,
                State = state,
                Bar = bar
            };
        }

        public void Open(
            SaveData save)
        {
            if (_root == null)
            {
                return;
            }

            Fill(save);
            _root.SetActive(true);
            UiEscapeStack.Push(this);

            // 열 때마다 맨 위부터 보여 준다.
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1f;
        }

        public void Close()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            UiEscapeStack.Remove(this);
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);
        }

        private void Fill(
            SaveData save)
        {
            _summary.text =
                $"{AchievementLogic.CountUnlocked(save)} / {AchievementLogic.All.Length} 달성";

            foreach (RowView row in _rows)
            {
                bool unlocked = AchievementLogic.IsUnlocked(save, row.Definition.Id);
                float progress = AchievementLogic.GetProgress(save, row.Definition);
                int value = AchievementLogic.GetValue(save, row.Definition);

                row.Bar.SetValue(progress);
                row.Bar.Root.gameObject.SetActive(!unlocked);

                row.State.text =
                    unlocked
                        ? "달성"
                        : $"{Mathf.Min(value, row.Definition.Target)} / {row.Definition.Target}";

                row.State.color = unlocked ? UiTheme.Gold : UiTheme.TextMuted;
                row.Name.color = unlocked ? UiTheme.Gold : UiTheme.TextPrimary;
                row.Detail.color = unlocked ? UiTheme.TextPrimary : UiTheme.TextMuted;
                row.Reward.color = unlocked ? UiTheme.TextDisabled : UiTheme.Gold;
                row.Background.color =
                    unlocked
                        ? new Color(0.20f, 0.16f, 0.08f, 0.95f)
                        : UiTheme.RowFill;
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                Close();
            }
#endif
        }
    }
}
