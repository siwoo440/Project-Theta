using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 도시 지도 화면이다 (21일차). 한 판 안에서 다음 장소를 고른다.
    ///
    ///   허브 → [지도] 연수원 → 스테이지 → [지도] 후보 2곳 중 선택 → 스테이지 → … → [지도] 루프탑 클럽
    ///
    /// 왼쪽은 장소 8곳과 길, 오른쪽은 고른 장소의 정보와 출발 버튼이다.
    /// 이번 구역 후보만 누를 수 있고, 간 곳은 "완료", 나머지는 잠겨 보인다.
    /// 후보는 판 시드로 정해져서 지도를 다시 열어도 바뀌지 않는다.
    /// </summary>
    public sealed class MapScreen : MonoBehaviour
    {
        private const float MapWidth = 1180f;
        private const float MapHeight = 800f;
        private const float NodeWidth = 236f;
        private const float NodeHeight = 96f;
        private const float AbandonConfirmSeconds = 3f;

        /// <summary>지도 위 길이다. 기획서 부록 B.2의 연결을 따른다. 지금은 보기용이며 이동 제약은 없다.</summary>
        private static readonly LocationId[,] Roads =
        {
            { LocationId.TrainingCenter, LocationId.SubwayStation },
            { LocationId.SubwayStation, LocationId.FitnessCenter },
            { LocationId.SubwayStation, LocationId.Beach },
            { LocationId.TrainingCenter, LocationId.OfficeTower },
            { LocationId.SubwayStation, LocationId.ShoppingMall },
            { LocationId.FitnessCenter, LocationId.NightMarket },
            { LocationId.OfficeTower, LocationId.ShoppingMall },
            { LocationId.ShoppingMall, LocationId.NightMarket },
            { LocationId.ShoppingMall, LocationId.RooftopClub }
        };

        private enum NodeState
        {
            Locked,
            Visited,
            Candidate,
            Selected
        }

        private sealed class NodeView
        {
            public LocationDefinition Location;
            public UiButton Button;
            public Image Edge;
            public Image Glow;
            public Text Name;
            public Text Detail;
            public Text Badge;
        }

        private RunSession _session;
        private List<LocationId> _candidates = new List<LocationId>();
        private LocationId? _selected;

        private readonly Dictionary<LocationId, NodeView> _nodes =
            new Dictionary<LocationId, NodeView>();

        private Text _subtitle;
        private readonly Text[] _routeSlots = new Text[RunRouteLogic.ZoneCount];
        private readonly Image[] _routeSlotBackgrounds = new Image[RunRouteLogic.ZoneCount];

        private Text _infoTime;
        private Text _infoName;
        private Text _infoSummary;
        private Text _infoRows;
        private Text _infoDisruptors;
        private Text _runStatus;
        private UiButton _departButton;
        private UiButton _abandonButton;

        private float _abandonConfirmRemaining;

        private void Start()
        {
            GameSession game =
                GameSession.Instance;

            // 지도 씬을 바로 재생한 경우처럼 판이 없으면 새 판을 만든다.
            if (game != null &&
                (game.Run == null ||
                 game.Run.IsFinished))
            {
                game.BeginRun();
            }

            _session =
                game == null
                    ? new RunSession(System.Environment.TickCount)
                    : game.Run;

            _candidates =
                _session.GetCandidates();

            // 후보가 하나뿐이면(첫 구역 · 마지막 구역) 미리 골라 둔다.
            if (_candidates.Count == 1)
            {
                _selected = _candidates[0];
            }

            Build();
            Refresh();
        }

        private void Update()
        {
            if (_abandonConfirmRemaining > 0f)
            {
                _abandonConfirmRemaining -=
                    Time.unscaledDeltaTime;

                if (_abandonConfirmRemaining <= 0f)
                {
                    _abandonButton.SetText("판 포기하고 허브로");
                }
            }

            ReadKeyboard();
        }

        /// <summary>숫자 키로 후보를 고르고, 엔터로 출발한다.</summary>
        private void ReadKeyboard()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SelectCandidateAt(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SelectCandidateAt(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SelectCandidateAt(2);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SelectCandidateAt(3);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                SelectCandidateAt(4);
            }
            else if (keyboard.digit6Key.wasPressedThisFrame)
            {
                SelectCandidateAt(5);
            }
            else if (keyboard.digit7Key.wasPressedThisFrame)
            {
                SelectCandidateAt(6);
            }
            else if (keyboard.digit8Key.wasPressedThisFrame)
            {
                SelectCandidateAt(7);
            }

            if (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                Depart();
            }
#endif
        }

        private void SelectCandidateAt(
            int index)
        {
            if (index < 0 ||
                index >= _candidates.Count)
            {
                return;
            }

            Select(
                _candidates[index]);
        }

        private void Select(
            LocationId id)
        {
            if (!_candidates.Contains(id))
            {
                return;
            }

            _selected = id;

            GameAudio.Play(
                GameSfx.UiTick);

            Refresh();
        }

        private void Depart()
        {
            if (_selected == null ||
                !_session.Select(_selected.Value))
            {
                return;
            }

            GameAudio.Play(
                GameSfx.UiStamp);

            GameSession.Instance?.GoTo(
                SceneDestination.Stage);
        }

        /// <summary>실수로 누르지 않게 두 번 눌러야 포기된다.</summary>
        private void Abandon()
        {
            if (_abandonConfirmRemaining <= 0f)
            {
                _abandonConfirmRemaining =
                    AbandonConfirmSeconds;

                _abandonButton.SetText("한 번 더 누르면 포기합니다");

                return;
            }

            GameSession game =
                GameSession.Instance;

            if (game == null)
            {
                return;
            }

            _session.Abandon();

            game.EndRun();

            game.GoTo(
                SceneDestination.Hub);
        }

        // 조립 ----------------------------------------------------------

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "MapCanvas",
                    0,
                    transform);

            Image backdrop =
                UiFactory.CreateImage(
                    canvas.transform,
                    "Backdrop",
                    new Color(0.040f, 0.050f, 0.085f, 1f));

            UiFactory.Stretch(
                backdrop.rectTransform);

            RectTransform motes =
                UiFactory.CreateRect(
                    canvas.transform,
                    "Motes");

            UiFactory.Stretch(
                motes);

            motes.gameObject.AddComponent<UiFloatingMotes>().Build(
                new Color(0.46f, 0.50f, 1.00f, 0.20f));

            BuildHeader(
                canvas.transform);

            BuildRouteStrip(
                canvas.transform);

            BuildMap(
                canvas.transform);

            BuildInfoPanel(
                canvas.transform);
        }

        private void BuildHeader(
            Transform parent)
        {
            Text title =
                UiFactory.CreateText(
                    parent,
                    "Title",
                    "도시 지도",
                    UiTheme.FontTitle,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                title.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(70f, -40f),
                new Vector2(600f, 60f));

            _subtitle =
                UiFactory.CreateText(
                    parent,
                    "Subtitle",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                _subtitle.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(72f, -100f),
                new Vector2(900f, 32f));
        }

        /// <summary>오른쪽 위의 "연수원 → 해변가 → ? → ? → 클럽" 경로 줄이다.</summary>
        private void BuildRouteStrip(
            Transform parent)
        {
            const float slotWidth = 150f;
            const float slotHeight = 44f;
            const float gap = 26f;

            float totalWidth =
                RunRouteLogic.ZoneCount * slotWidth +
                (RunRouteLogic.ZoneCount - 1) * gap;

            RectTransform strip =
                UiFactory.CreateRect(
                    parent,
                    "RouteStrip");

            UiFactory.Place(
                strip,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-60f, -52f),
                new Vector2(totalWidth, slotHeight));

            for (int i = 0;
                 i < RunRouteLogic.ZoneCount;
                 i++)
            {
                float x = i * (slotWidth + gap);

                Image background =
                    UiFactory.CreateImage(
                        strip,
                        $"Slot{i}",
                        UiTheme.RowFill);

                UiFactory.Place(
                    background.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f),
                    new Vector2(x, 0f),
                    new Vector2(slotWidth, slotHeight));

                Text label =
                    UiFactory.CreateText(
                        background.transform,
                        "Label",
                        "?",
                        UiTheme.FontSmall,
                        UiTheme.TextMuted,
                        TextAnchor.MiddleCenter,
                        FontStyle.Bold);

                UiFactory.Stretch(
                    label.rectTransform);

                _routeSlots[i] = label;
                _routeSlotBackgrounds[i] = background;

                if (i < RunRouteLogic.ZoneCount - 1)
                {
                    Text arrow =
                        UiFactory.CreateText(
                            strip,
                            "Arrow",
                            "›",
                            UiTheme.FontHeading,
                            UiTheme.TextDisabled,
                            TextAnchor.MiddleCenter);

                    UiFactory.Place(
                        arrow.rectTransform,
                        new Vector2(0f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(x + slotWidth + gap * 0.5f, 0f),
                        new Vector2(gap, slotHeight));
                }
            }
        }

        private void BuildMap(
            Transform parent)
        {
            RectTransform map =
                UiFactory.CreatePanel(
                    parent,
                    "Map",
                    new Color(0.070f, 0.085f, 0.130f, 0.92f),
                    new Color(0.30f, 0.36f, 0.58f, 0.60f));

            UiFactory.Place(
                map,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(70f, -60f),
                new Vector2(MapWidth, MapHeight));

            // 바다 · 강 느낌의 옅은 띠. 해변가 쪽을 푸르게 깐다.
            Image sea =
                UiFactory.CreateImage(
                    map,
                    "Sea",
                    new Color(0.18f, 0.36f, 0.58f, 0.22f));

            UiFactory.Place(
                sea.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 4f),
                new Vector2(MapWidth - 8f, 120f));

            // 길을 먼저 깔아 장소 표식 아래로 가게 한다.
            for (int i = 0;
                 i < Roads.GetLength(0);
                 i++)
            {
                BuildRoad(
                    map,
                    GetNodePosition(LocationCatalog.Get(Roads[i, 0])),
                    GetNodePosition(LocationCatalog.Get(Roads[i, 1])));
            }

            IReadOnlyList<LocationDefinition> all =
                LocationCatalog.All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (all[i] != null)
                {
                    BuildNode(
                        map,
                        all[i]);
                }
            }
        }

        private static Vector2 GetNodePosition(
            LocationDefinition location)
        {
            const float paddingX = 150f;
            const float paddingY = 90f;

            return new Vector2(
                paddingX + Mathf.Clamp01(location.MapPosition.x) * (MapWidth - paddingX * 2f),
                paddingY + Mathf.Clamp01(location.MapPosition.y) * (MapHeight - paddingY * 2f));
        }

        private static void BuildRoad(
            RectTransform map,
            Vector2 from,
            Vector2 to)
        {
            Vector2 delta = to - from;

            Image road =
                UiFactory.CreateImage(
                    map,
                    "Road",
                    new Color(0.42f, 0.46f, 0.66f, 0.45f));

            RectTransform rect =
                road.rectTransform;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = (from + to) * 0.5f;
            rect.sizeDelta = new Vector2(delta.magnitude, 6f);

            rect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void BuildNode(
            RectTransform map,
            LocationDefinition location)
        {
            Vector2 position =
                GetNodePosition(
                    location);

            // 후보일 때만 보이는 맥동 빛. 노드보다 먼저 만들어 뒤에 깐다.
            Image glow =
                UiDecor.CreateGlow(
                    map,
                    "Glow",
                    LocationCatalog.GetTimeColor(location.TimeOfDay),
                    new Vector2(NodeWidth + 130f, NodeHeight + 110f));

            glow.rectTransform.anchorMin = Vector2.zero;
            glow.rectTransform.anchorMax = Vector2.zero;
            glow.rectTransform.anchoredPosition = position;

            UiPulse pulse =
                glow.gameObject.AddComponent<UiPulse>();

            pulse.Target = glow;
            pulse.MinimumAlpha = 0.15f;
            pulse.MaximumAlpha = 0.55f;
            pulse.Period = 1.6f;

            UiButton button =
                UiFactory.CreateButton(
                    map,
                    $"Node_{location.Id}",
                    string.Empty);

            RectTransform rect =
                button.Background.rectTransform;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(NodeWidth, NodeHeight);

            LocationId id = location.Id;

            button.Button.onClick.AddListener(
                () => Select(id));

            // 버튼 바탕을 테두리로 쓰고 안쪽을 채운다.
            Image fill =
                UiFactory.CreateImage(
                    button.Background.transform,
                    "Fill",
                    new Color(0.085f, 0.075f, 0.130f, 1f));

            UiFactory.Stretch(
                fill.rectTransform,
                3f);

            Image timeBand =
                UiFactory.CreateImage(
                    fill.transform,
                    "TimeBand",
                    LocationCatalog.GetTimeColor(location.TimeOfDay));

            timeBand.rectTransform.anchorMin = new Vector2(0f, 0f);
            timeBand.rectTransform.anchorMax = new Vector2(0f, 1f);
            timeBand.rectTransform.pivot = new Vector2(0f, 0.5f);
            timeBand.rectTransform.offsetMin = Vector2.zero;
            timeBand.rectTransform.offsetMax = Vector2.zero;
            timeBand.rectTransform.sizeDelta = new Vector2(8f, 0f);

            Text name =
                UiFactory.CreateText(
                    fill.transform,
                    "Name",
                    location.DisplayName,
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                name.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(22f, -12f),
                new Vector2(NodeWidth - 40f, 32f));

            Text detail =
                UiFactory.CreateText(
                    fill.transform,
                    "Detail",
                    $"{LocationCatalog.GetTimeLabel(location.TimeOfDay)} · {LocationCatalog.GetObjectiveLabel(location.Objective)}",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(
                detail.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(22f, -46f),
                new Vector2(NodeWidth - 40f, 24f));

            Text badge =
                UiFactory.CreateText(
                    fill.transform,
                    "Badge",
                    string.Empty,
                    UiTheme.FontTiny,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(
                badge.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-12f, 6f),
                new Vector2(NodeWidth - 30f, 22f));

            _nodes[id] =
                new NodeView
                {
                    Location = location,
                    Button = button,
                    Edge = button.Background,
                    Glow = glow,
                    Name = name,
                    Detail = detail,
                    Badge = badge
                };
        }

        private void BuildInfoPanel(
            Transform parent)
        {
            const float width = 540f;
            const float padding = 34f;
            float inner = width - padding * 2f;

            RectTransform panel =
                UiFactory.CreatePanel(
                    parent,
                    "Info",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            UiFactory.Place(
                panel,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-60f, -60f),
                new Vector2(width, MapHeight));

            _infoTime =
                CreateInfoText(panel, padding, 30f, inner, 26f, UiTheme.FontBody, UiTheme.Gold, true);

            _infoName =
                CreateInfoText(panel, padding, 60f, inner, 52f, UiTheme.FontTitle - 6, UiTheme.TextPrimary, true);

            _infoSummary =
                CreateInfoText(panel, padding, 122f, inner, 60f, UiTheme.FontBody, UiTheme.TextMuted, false);

            _infoSummary.horizontalOverflow = HorizontalWrapMode.Wrap;
            _infoSummary.alignment = TextAnchor.UpperLeft;

            UiDecor.CreateDivider(
                panel,
                "Divider",
                new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.5f),
                200f,
                padding);

            _infoRows =
                CreateInfoText(panel, padding, 222f, inner, 150f, UiTheme.FontBody, UiTheme.TextPrimary, false);

            _infoRows.alignment = TextAnchor.UpperLeft;
            _infoRows.lineSpacing = 1.35f;

            CreateInfoText(panel, padding, 388f, inner, 26f, UiTheme.FontSmall, UiTheme.Gold, true)
                .text = "등장하는 방해 세력";

            _infoDisruptors =
                CreateInfoText(panel, padding, 416f, inner, 70f, UiTheme.FontBody, UiTheme.TextMuted, false);

            _infoDisruptors.horizontalOverflow = HorizontalWrapMode.Wrap;
            _infoDisruptors.alignment = TextAnchor.UpperLeft;

            UiDecor.CreateDivider(
                panel,
                "Divider2",
                new Color(UiTheme.PanelEdge.r, UiTheme.PanelEdge.g, UiTheme.PanelEdge.b, 0.6f),
                508f,
                padding);

            _runStatus =
                CreateInfoText(panel, padding, 526f, inner, 60f, UiTheme.FontBody, UiTheme.TextPrimary, false);

            _runStatus.alignment = TextAnchor.UpperLeft;
            _runStatus.lineSpacing = 1.3f;

            _departButton =
                UiFactory.CreateButton(
                    panel,
                    "Depart",
                    "출  발",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(
                _departButton.Background.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 104f),
                new Vector2(inner, 70f));

            _departButton.Button.onClick.AddListener(
                Depart);

            _abandonButton =
                UiFactory.CreateButton(
                    panel,
                    "Abandon",
                    "판 포기하고 허브로",
                    UiTheme.FontSmall);

            UiFactory.Place(
                _abandonButton.Background.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 36f),
                new Vector2(inner, 44f));

            _abandonButton.Button.onClick.AddListener(
                Abandon);
        }

        private static Text CreateInfoText(
            RectTransform panel,
            float x,
            float top,
            float width,
            float height,
            int fontSize,
            Color color,
            bool bold)
        {
            Text text =
                UiFactory.CreateText(
                    panel,
                    "Text",
                    string.Empty,
                    fontSize,
                    color,
                    TextAnchor.MiddleLeft,
                    bold
                        ? FontStyle.Bold
                        : FontStyle.Normal);

            UiFactory.Place(
                text.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(x, -top),
                new Vector2(width, height));

            return text;
        }

        // 갱신 ----------------------------------------------------------

        private void Refresh()
        {
            int step =
                _session.NextStep;

            _subtitle.text =
                _candidates.Count > 1
                    ? $"구역 {step + 1} / {RunRouteLogic.ZoneCount}   ·   다음 장소를 고르세요  (숫자 키 1~{_candidates.Count})"
                    : RunRouteLogic.IsFinalStep(step) ||
                      RunRouteLogic.OpenAllLocations
                        ? $"구역 {step + 1} / {RunRouteLogic.ZoneCount}   ·   마지막 장소입니다"
                        : $"구역 {step + 1} / {RunRouteLogic.ZoneCount}   ·   첫 장소에서 시작합니다";

            RefreshRouteStrip(step);
            RefreshNodes();
            RefreshInfo();
            RefreshRunStatus();
        }

        private void RefreshRouteStrip(
            int step)
        {
            for (int i = 0;
                 i < RunRouteLogic.ZoneCount;
                 i++)
            {
                Text label = _routeSlots[i];
                Image background = _routeSlotBackgrounds[i];

                if (i < _session.Records.Count)
                {
                    ZoneRecord record = _session.Records[i];

                    label.text = $"{LocationCatalog.Get(record.Location).DisplayName}  {record.RankLabel}";
                    label.color = UiTheme.Gold;
                    background.color = new Color(0.20f, 0.16f, 0.08f, 0.95f);

                    continue;
                }

                if (i == step)
                {
                    label.text =
                        _selected == null
                            ? "선택 중"
                            : LocationCatalog.Get(_selected.Value).DisplayName;

                    label.color = UiTheme.TextPrimary;
                    background.color = UiTheme.PrimaryButtonNormal;

                    continue;
                }

                // 모든 장소 열기에서는 마지막 구역도 정해져 있지 않다.
                label.text =
                    i == RunRouteLogic.ZoneCount - 1 &&
                    !RunRouteLogic.OpenAllLocations
                        ? LocationCatalog.Get(LocationCatalog.FinalLocation).DisplayName
                        : "?";

                label.color = UiTheme.TextDisabled;
                background.color = UiTheme.RowFill;
            }
        }

        private void RefreshNodes()
        {
            foreach (KeyValuePair<LocationId, NodeView> pair in _nodes)
            {
                NodeView view = pair.Value;
                NodeState state = GetState(pair.Key);

                bool clickable =
                    state == NodeState.Candidate ||
                    state == NodeState.Selected;

                view.Button.Button.interactable = clickable;
                view.Glow.gameObject.SetActive(clickable);

                Color timeColor =
                    LocationCatalog.GetTimeColor(
                        view.Location.TimeOfDay);

                switch (state)
                {
                    case NodeState.Selected:
                        view.Edge.color = UiTheme.Gold;
                        view.Name.color = UiTheme.Gold;
                        view.Detail.color = UiTheme.TextPrimary;
                        view.Badge.text = "선택됨";
                        view.Badge.color = UiTheme.Gold;
                        break;

                    case NodeState.Candidate:
                        view.Edge.color = timeColor;
                        view.Name.color = UiTheme.TextPrimary;
                        view.Detail.color = UiTheme.TextMuted;
                        view.Badge.text = $"[{_candidates.IndexOf(pair.Key) + 1}] 선택 가능";
                        view.Badge.color = timeColor;
                        break;

                    case NodeState.Visited:
                        view.Edge.color = new Color(0.46f, 0.38f, 0.18f, 1f);
                        view.Name.color = UiTheme.TextMuted;
                        view.Detail.color = UiTheme.TextDisabled;
                        view.Badge.text = "완료";
                        view.Badge.color = UiTheme.Gold;
                        break;

                    default:
                        view.Edge.color = new Color(0.20f, 0.20f, 0.28f, 1f);
                        view.Name.color = UiTheme.TextDisabled;
                        view.Detail.color = UiTheme.TextDisabled;
                        view.Badge.text =
                            pair.Key == LocationCatalog.FinalLocation
                                ? "최종"
                                : string.Empty;
                        view.Badge.color = UiTheme.Danger;
                        break;
                }
            }
        }

        private NodeState GetState(
            LocationId id)
        {
            if (_selected != null &&
                _selected.Value == id)
            {
                return NodeState.Selected;
            }

            if (_candidates.Contains(id))
            {
                return NodeState.Candidate;
            }

            for (int i = 0;
                 i < _session.Visited.Count;
                 i++)
            {
                if (_session.Visited[i] == id)
                {
                    return NodeState.Visited;
                }
            }

            return NodeState.Locked;
        }

        private void RefreshInfo()
        {
            bool hasSelection =
                _selected != null;

            _departButton.SetInteractable(
                hasSelection);

            if (!hasSelection)
            {
                _infoTime.text = string.Empty;
                _infoName.text = "장소를 고르세요";
                _infoSummary.text = "빛나는 장소가 이번 구역에서 갈 수 있는 곳입니다.";
                _infoRows.text = string.Empty;
                _infoDisruptors.text = "-";

                return;
            }

            LocationDefinition location =
                LocationCatalog.Get(
                    _selected.Value);

            _infoTime.text =
                $"{LocationCatalog.GetTimeLabel(location.TimeOfDay)}   ·   {LocationCatalog.GetObjectiveLabel(location.Objective)}";

            _infoTime.color =
                LocationCatalog.GetTimeColor(
                    location.TimeOfDay);

            _infoName.text = location.DisplayName;
            _infoSummary.text = location.Summary;

            int minutes = Mathf.FloorToInt(location.TimeLimitSeconds / 60f);
            int seconds = Mathf.FloorToInt(location.TimeLimitSeconds) % 60;

            _infoRows.text =
                $"층 수            {location.FloorCount}개 층\n" +
                $"제한 시간      {minutes}:{seconds:00}\n" +
                $"목표 정기      {location.TargetEssence}\n" +
                $"경쟁자          {(location.HasRivals ? "금태양 · 인기남" : "없음")}";

            _infoDisruptors.text = location.DisruptorPreview;
        }

        private void RefreshRunStatus()
        {
            _runStatus.text =
                $"Lv {_session.Level.Level}   ·   강화 카드 {_session.Upgrades.PickCount}장\n" +
                $"이번 판 계약 정기  +{_session.TotalContractEssence}";
        }
    }
}
