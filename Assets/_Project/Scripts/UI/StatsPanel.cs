using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 지도의 [통계] 창이다 (29일차). 세이브에 쌓인 누적 기록을 숫자로 보여 준다.
    ///
    ///   ┌ 플레이 통계 ─────────────────────────────── [닫기] ┐
    ///   │ [전체] [최면 · 동행] [정기] [위기] [성장]           │
    ///   │ 장소        도전 클리어 최고 정기 최단 클리어 최고 등급 │
    ///   │ 기업 연수원   3     2      180      2:41       A     │
    ///   │ …                                                    │
    ///   └──────────────────────────────────────────────────────┘
    ///
    /// Esc · Tab · 닫기 버튼으로 닫는다.
    /// </summary>
    public sealed class StatsPanel : MonoBehaviour
    {
        private const float Width = 1560f;
        private const float Height = 900f;
        private const float CardWidth = 284f;
        private const float CardGap = 12f;
        private const float CardHeight = 262f;
        private const float TableTop = 350f;
        private const float RowHeight = 44f;
        private const float NameColumnWidth = 300f;
        private const float ValueColumnWidth = 220f;

        private GameObject _root;
        private readonly List<Text[]> _cardValues = new List<Text[]>();
        private readonly Dictionary<LocationId, Text[]> _tableValues = new Dictionary<LocationId, Text[]>();
        private Text _footer;

        public bool IsOpen =>
            _root != null &&
            _root.activeSelf;

        /// <summary>마지막으로 열거나 닫은 프레임이다. 같은 키로 닫자마자 다시 열리지 않게 한다.</summary>
        public int LastToggleFrame { get; private set; } = -1;

        public void Build(
            Transform canvas)
        {
            // 지도 위에 덮는 캔버스. 뒤의 지도 클릭을 막는다.
            GameObject overlay =
                new GameObject(
                    "StatsOverlay",
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
                    "StatsWindow",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            UiFactory.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, Height));

            Text title =
                UiFactory.CreateText(
                    panel,
                    "Title",
                    "플레이 통계",
                    UiTheme.FontTitle - 6,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -22f), new Vector2(600f, 56f));

            UiButton close =
                UiFactory.CreateButton(
                    panel,
                    "Close",
                    "닫기  (Esc)",
                    UiTheme.FontBody);

            UiFactory.Place(close.Background.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -26f), new Vector2(170f, 48f));
            close.Button.onClick.AddListener(Close);

            BuildCards(panel);
            BuildTable(panel);

            _footer =
                UiFactory.CreateText(
                    panel,
                    "Footer",
                    "치트를 쓴 도전은 통계에 넣지 않습니다.",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(_footer.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 16f), new Vector2(900f, 28f));

            overlay.SetActive(false);
        }

        private void BuildCards(
            RectTransform panel)
        {
            List<StatsSection> sections =
                PlayStatsLogic.BuildSections(null);

            for (int i = 0; i < sections.Count; i++)
            {
                StatsSection section = sections[i];

                RectTransform card =
                    UiFactory.CreatePanel(
                        panel,
                        $"Card_{i}",
                        UiTheme.RowFill,
                        new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.35f),
                        1f);

                UiFactory.Place(card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f + i * (CardWidth + CardGap), -92f), new Vector2(CardWidth, CardHeight));

                Text heading =
                    UiFactory.CreateText(
                        card,
                        "Heading",
                        section.Title,
                        UiTheme.FontSubheading,
                        UiTheme.Gold,
                        TextAnchor.MiddleLeft,
                        FontStyle.Bold);

                UiFactory.Place(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(CardWidth - 36f, 34f));

                Text[] values = new Text[section.Rows.Length];

                for (int r = 0; r < section.Rows.Length; r++)
                {
                    float top = -52f - r * 40f;

                    Text label =
                        UiFactory.CreateText(
                            card,
                            "Label",
                            section.Rows[r].Label,
                            UiTheme.FontSmall,
                            UiTheme.TextMuted,
                            TextAnchor.MiddleLeft);

                    UiFactory.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, top), new Vector2(150f, 34f));

                    values[r] =
                        UiFactory.CreateText(
                            card,
                            "Value",
                            "-",
                            UiTheme.FontSubheading,
                            UiTheme.TextPrimary,
                            TextAnchor.MiddleRight,
                            FontStyle.Bold);

                    UiFactory.Place(values[r].rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, top), new Vector2(CardWidth - 170f, 34f));
                }

                _cardValues.Add(values);
            }
        }

        private void BuildTable(
            RectTransform panel)
        {
            Text heading =
                UiFactory.CreateText(
                    panel,
                    "TableHeading",
                    "장소별 기록",
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -TableTop - 20f), new Vector2(400f, 34f));

            float headerTop = TableTop + 20f;

            CreateCell(panel, "장소", 0, headerTop, UiTheme.TextMuted, true, TextAnchor.MiddleLeft, NameColumnWidth);

            for (int c = 0; c < PlayStatsLogic.LocationColumns.Length; c++)
            {
                CreateCell(panel, PlayStatsLogic.LocationColumns[c], NameColumnWidth + c * ValueColumnWidth, headerTop, UiTheme.TextMuted, true, TextAnchor.MiddleCenter, ValueColumnWidth);
            }

            IReadOnlyList<LocationDefinition> all = LocationCatalog.All;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null)
                {
                    continue;
                }

                float top = headerTop + RowHeight * (i + 1);

                // 줄마다 옅은 띠를 번갈아 깐다.
                if (i % 2 == 0)
                {
                    Image band =
                        UiFactory.CreateImage(
                            panel,
                            "Band",
                            new Color(1f, 1f, 1f, 0.04f));

                    UiFactory.Place(band.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -top), new Vector2(Width - 60f, RowHeight));
                }

                Image time =
                    UiFactory.CreateImage(
                        panel,
                        "TimeDot",
                        LocationCatalog.GetTimeColor(all[i].TimeOfDay));

                UiFactory.Place(time.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -top - RowHeight * 0.5f + 6f), new Vector2(6f, 12f));

                CreateCell(panel, all[i].DisplayName, 12f, top, UiTheme.TextPrimary, true, TextAnchor.MiddleLeft, NameColumnWidth - 12f);

                Text[] values = new Text[PlayStatsLogic.LocationColumns.Length];

                for (int c = 0; c < values.Length; c++)
                {
                    values[c] = CreateCell(panel, "-", NameColumnWidth + c * ValueColumnWidth, top, UiTheme.TextPrimary, false, TextAnchor.MiddleCenter, ValueColumnWidth);
                }

                _tableValues[all[i].Id] = values;
            }
        }

        private static Text CreateCell(
            RectTransform panel,
            string content,
            float x,
            float top,
            Color color,
            bool bold,
            TextAnchor anchor,
            float width)
        {
            Text text =
                UiFactory.CreateText(
                    panel,
                    "Cell",
                    content,
                    UiTheme.FontBody,
                    color,
                    anchor,
                    bold ? FontStyle.Bold : FontStyle.Normal);

            UiFactory.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f + x, -top), new Vector2(width, RowHeight));

            return text;
        }

        public void Open(
            SaveData save)
        {
            if (_root == null ||
                LastToggleFrame == Time.frameCount)
            {
                return;
            }

            Fill(save);

            _root.SetActive(true);
            LastToggleFrame = Time.frameCount;
        }

        public void Close()
        {
            if (_root == null ||
                !_root.activeSelf ||
                LastToggleFrame == Time.frameCount)
            {
                return;
            }

            _root.SetActive(false);
            LastToggleFrame = Time.frameCount;
        }

        private void Fill(
            SaveData save)
        {
            List<StatsSection> sections =
                PlayStatsLogic.BuildSections(
                    save == null ? null : save.Stats);

            for (int i = 0; i < sections.Count && i < _cardValues.Count; i++)
            {
                Text[] values = _cardValues[i];

                for (int r = 0; r < values.Length && r < sections[i].Rows.Length; r++)
                {
                    values[r].text = sections[i].Rows[r].Value;
                }
            }

            foreach (KeyValuePair<LocationId, Text[]> pair in _tableValues)
            {
                LocationStats record = PlayStatsLogic.Get(save, (int)pair.Key);
                string[] row = PlayStatsLogic.BuildLocationRow(record);

                for (int c = 0; c < pair.Value.Length && c < row.Length; c++)
                {
                    pair.Value[c].text = row[c];
                    pair.Value[c].color = record.Attempts > 0 ? UiTheme.TextPrimary : UiTheme.TextDisabled;
                }
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
                (keyboard.escapeKey.wasPressedThisFrame ||
                 keyboard.tabKey.wasPressedThisFrame))
            {
                Close();
            }
#endif
        }
    }
}
