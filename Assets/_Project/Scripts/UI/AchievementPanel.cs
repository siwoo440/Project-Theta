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
    /// 허브의 업적 목록 창이다 (30일차). 두 줄로 업적을 늘어놓고 달성 · 진행도 · 보상을 보여 준다.
    /// Esc · 닫기로 닫는다.
    /// </summary>
    public sealed class AchievementPanel : MonoBehaviour
    {
        private const float Width = 1560f;
        private const float Height = 920f;
        private const int Columns = 2;
        private const float RowHeight = 62f;
        private const float RowGap = 6f;

        private sealed class RowView
        {
            public AchievementDefinition Definition;
            public Image Background;
            public Text Name;
            public Text Detail;
            public Text State;
            public UiBar Bar;
        }

        private GameObject _root;
        private Text _summary;
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

            UiFactory.Place(_summary.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200f, -30f), new Vector2(900f, 40f));

            UiButton close =
                UiFactory.CreateButton(
                    panel,
                    "Close",
                    "닫기  (Esc)",
                    UiTheme.FontBody);

            UiFactory.Place(close.Background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -26f), new Vector2(170f, 48f));
            close.Button.onClick.AddListener(Close);

            float columnWidth = (Width - 80f - 20f) / Columns;
            int perColumn = Mathf.CeilToInt(AchievementLogic.All.Length / (float)Columns);

            for (int i = 0; i < AchievementLogic.All.Length; i++)
            {
                int column = i / perColumn;
                int row = i % perColumn;

                _rows.Add(
                    BuildRow(
                        panel,
                        AchievementLogic.All[i],
                        40f + column * (columnWidth + 20f),
                        96f + row * (RowHeight + RowGap),
                        columnWidth));
            }

            overlay.SetActive(false);
        }

        private static RowView BuildRow(
            RectTransform panel,
            AchievementDefinition definition,
            float x,
            float top,
            float width)
        {
            Image background =
                UiFactory.CreateImage(
                    panel,
                    $"Achievement_{definition.Id}",
                    UiTheme.RowFill);

            UiFactory.Place(background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -top), new Vector2(width, RowHeight));

            Text name =
                UiFactory.CreateText(
                    background.transform,
                    "Name",
                    definition.Name,
                    UiTheme.FontBody,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -6f), new Vector2(width * 0.5f, 26f));

            Text detail =
                UiFactory.CreateText(
                    background.transform,
                    "Detail",
                    $"{definition.Description}  ·  +{definition.Reward}",
                    UiTheme.FontTiny,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -34f), new Vector2(width * 0.6f, 22f));

            UiBar bar =
                UiFactory.CreateBar(
                    background.transform,
                    "Progress",
                    UiTheme.Gold,
                    UiTheme.TrackFill);

            UiFactory.Place(bar.Root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, -12f), new Vector2(width * 0.3f, 12f));

            Text state =
                UiFactory.CreateText(
                    background.transform,
                    "State",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(state.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 12f), new Vector2(width * 0.35f, 24f));

            return new RowView
            {
                Definition = definition,
                Background = background,
                Name = name,
                Detail = detail,
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
                int value = AchievementLogic.GetValue(save, row.Definition.Stat);

                row.Bar.SetValue(progress);
                row.Bar.Root.gameObject.SetActive(!unlocked);

                row.State.text =
                    unlocked
                        ? "달성"
                        : $"{Mathf.Min(value, row.Definition.Target)} / {row.Definition.Target}";

                row.State.color = unlocked ? UiTheme.Gold : UiTheme.TextMuted;
                row.Name.color = unlocked ? UiTheme.Gold : UiTheme.TextPrimary;
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
