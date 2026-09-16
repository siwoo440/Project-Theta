using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 도시 지도 화면이다 (21일차).
    ///
    /// 29일차: 판이 없다. 장소 8곳 중 어디든 몇 번이든 골라 들어간다.
    ///   허브 ⇄ [지도] ⇄ 스테이지
    ///
    /// 왼쪽은 장소 8곳과 길, 오른쪽은 고른 장소의 정보 · 내 기록 · 출발 버튼이다.
    /// 오른쪽 위 [통계] 버튼은 지금까지의 누적 기록 창(<see cref="StatsPanel"/>)을 연다.
    /// </summary>
    public sealed class MapScreen : MonoBehaviour
    {
        private const float MapWidth = 1180f;
        private const float MapHeight = 800f;
        private const float NodeWidth = 236f;
        private const float NodeHeight = 96f;

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
            Open,
            Cleared,
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

        private List<LocationId> _candidates = new List<LocationId>();
        private LocationId? _selected;
        private StatsPanel _stats;
        private Canvas _canvas;

        private readonly Dictionary<LocationId, NodeView> _nodes =
            new Dictionary<LocationId, NodeView>();

        private Text _subtitle;

        private Text _infoTime;
        private Text _infoName;
        private Text _infoSummary;
        private Text _infoRows;
        private Text _infoDisruptors;
        private Text _runStatus;
        private UiButton _departButton;

        private void Start()
        {
            _candidates =
                RunRouteLogic.GetLocations();

            // 마지막으로 도전한 장소를 미리 골라 둔다. 바로 다시 도전하기 쉽다.
            GameSession game =
                GameSession.Instance;

            if (game != null &&
                game.Run != null &&
                _candidates.Contains(game.Run.Location))
            {
                _selected = game.Run.Location;
            }

            Build();
            Refresh();

            // 30일차: 방금 끝낸 장소에서 달성한 업적을 알린다.
            if (game != null)
            {
                AchievementToast.Show(
                    _canvas.transform,
                    game.TakeNewAchievements());
            }
        }

        private void Update()
        {
            // 통계 창이 열려 있으면 지도 단축키를 받지 않는다.
            if (_stats != null &&
                _stats.IsOpen)
            {
                return;
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

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                OpenStats();
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
            GameSession game =
                GameSession.Instance;

            if (_selected == null ||
                game == null)
            {
                return;
            }

            // 29일차: 출발할 때마다 새 도전을 만든다. 레벨 · 카드는 장소마다 처음부터다.
            game.BeginLocation(
                _selected.Value);

            GameAudio.Play(
                GameSfx.UiStamp);

            game.GoTo(
                SceneDestination.Stage);
        }

        private void OpenStats()
        {
            if (_stats == null)
            {
                return;
            }

            GameAudio.Play(
                GameSfx.UiTick);

            _stats.Open(
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save);
        }

        private void GoToHub()
        {
            GameSession game =
                GameSession.Instance;

            if (game == null)
            {
                return;
            }

            game.ClearRun();

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

            _canvas = canvas;

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

            BuildTopButtons(
                canvas.transform);

            BuildMap(
                canvas.transform);

            BuildInfoPanel(
                canvas.transform);

            // 통계 창은 지도 위에 덮는다.
            _stats =
                gameObject.AddComponent<StatsPanel>();

            _stats.Build(
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

        /// <summary>오른쪽 위의 [통계] · [허브로] 버튼이다 (29일차).</summary>
        private void BuildTopButtons(
            Transform parent)
        {
            UiButton stats =
                UiFactory.CreateButton(
                    parent,
                    "StatsButton",
                    "통계  (Tab)",
                    UiTheme.FontSubheading,
                    true);

            UiFactory.Place(
                stats.Background.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-260f, -48f),
                new Vector2(200f, 52f));

            stats.Button.onClick.AddListener(
                OpenStats);

            UiButton hub =
                UiFactory.CreateButton(
                    parent,
                    "HubButton",
                    "허브로",
                    UiTheme.FontSubheading);

            UiFactory.Place(
                hub.Background.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-60f, -48f),
                new Vector2(180f, 52f));

            hub.Button.onClick.AddListener(
                GoToHub);
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

            // 고른 장소에만 보이는 맥동 빛. 노드보다 먼저 만들어 뒤에 깐다.
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
                CreateInfoText(panel, padding, 526f, inner, 120f, UiTheme.FontBody, UiTheme.TextPrimary, false);

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
                new Vector2(0f, 40f),
                new Vector2(inner, 70f));

            _departButton.Button.onClick.AddListener(
                Depart);
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
            _subtitle.text =
                $"가고 싶은 장소를 고르세요  (숫자 키 1~{_candidates.Count} · Enter 출발)   ·   몇 번이든 다시 도전할 수 있습니다";

            RefreshNodes();
            RefreshInfo();
            RefreshRunStatus();
        }

        private SaveData CurrentSave =>
            GameSession.Instance == null
                ? null
                : GameSession.Instance.Save;

        private void RefreshNodes()
        {
            SaveData save = CurrentSave;

            foreach (KeyValuePair<LocationId, NodeView> pair in _nodes)
            {
                NodeView view = pair.Value;
                LocationStats record = PlayStatsLogic.Get(save, (int)pair.Key);
                NodeState state = GetState(pair.Key, record);

                view.Button.Button.interactable = true;
                view.Glow.gameObject.SetActive(state == NodeState.Selected);

                Color timeColor =
                    LocationCatalog.GetTimeColor(
                        view.Location.TimeOfDay);

                // 30일차: 숙련도(★)를 번호 옆에 붙인다.
                string number =
                    $"[{_candidates.IndexOf(pair.Key) + 1}] {MasteryLogic.FormatStars(MasteryLogic.GetStars(record.Clears))}";

                switch (state)
                {
                    case NodeState.Selected:
                        view.Edge.color = UiTheme.Gold;
                        view.Name.color = UiTheme.Gold;
                        view.Detail.color = UiTheme.TextPrimary;
                        view.Badge.text = $"{number} 선택됨";
                        view.Badge.color = UiTheme.Gold;
                        break;

                    case NodeState.Cleared:
                        view.Edge.color = timeColor;
                        view.Name.color = UiTheme.TextPrimary;
                        view.Detail.color = UiTheme.TextMuted;
                        view.Badge.text = $"{number} 클리어 {record.Clears} · {record.BestRank}";
                        view.Badge.color = UiTheme.Gold;
                        break;

                    default:
                        view.Edge.color = new Color(timeColor.r, timeColor.g, timeColor.b, 0.7f);
                        view.Name.color = UiTheme.TextPrimary;
                        view.Detail.color = UiTheme.TextMuted;
                        view.Badge.text =
                            record.Attempts > 0
                                ? $"{number} 도전 {record.Attempts}회"
                                : pair.Key == LocationCatalog.FinalLocation
                                    ? $"{number} 보스"
                                    : $"{number} 새 장소";
                        view.Badge.color =
                            pair.Key == LocationCatalog.FinalLocation
                                ? UiTheme.Danger
                                : timeColor;
                        break;
                }
            }
        }

        private NodeState GetState(
            LocationId id,
            LocationStats record)
        {
            if (_selected != null &&
                _selected.Value == id)
            {
                return NodeState.Selected;
            }

            return record.Clears > 0
                ? NodeState.Cleared
                : NodeState.Open;
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
                _infoSummary.text = "지도의 장소를 누르거나 숫자 키로 고르세요. 클리어할수록 ★이 오르고 목표와 보상이 커집니다.";
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

            // 30일차: 숙련도(★)만큼 목표 정기가 오른다.
            int stars =
                MasteryLogic.GetStars(
                    CurrentSave,
                    (int)location.Id);

            int target =
                MasteryLogic.GetTargetEssence(
                    location.TargetEssence,
                    stars);

            string targetBonus =
                stars > 0
                    ? $"   (★{stars} · 보상 +{Mathf.RoundToInt(MasteryLogic.RewardBonusPerStar * stars * 100f)}%)"
                    : string.Empty;

            _infoRows.text =
                $"층 수            {location.FloorCount}개 층\n" +
                $"제한 시간      {minutes}:{seconds:00}\n" +
                $"목표 정기      {target}{targetBonus}\n" +
                $"경쟁자          {(location.HasRivals ? "금태양 · 인기남" : "없음")}";

            _infoDisruptors.text = location.DisruptorPreview;
        }

        private void RefreshRunStatus()
        {
            SaveData save = CurrentSave;
            int essence = save == null ? 0 : save.ContractEssence;

            if (_selected == null)
            {
                _runStatus.text =
                    $"보유 계약 정기  {essence}\n" +
                    "레벨 · 강화 카드는 장소마다 처음부터 시작합니다.";

                return;
            }

            LocationStats record =
                PlayStatsLogic.Get(
                    save,
                    (int)_selected.Value);

            int next =
                MasteryLogic.GetClearsToNextStar(
                    record.Clears);

            string mastery =
                next > 0
                    ? $"숙련 {MasteryLogic.FormatStars(MasteryLogic.GetStars(record.Clears))}  (다음 ★까지 클리어 {next}회)"
                    : $"숙련 {MasteryLogic.FormatStars(MasteryLogic.MaxStars)}  (최고)";

            _runStatus.text =
                $"{mastery}\n" +
                $"내 기록   도전 {record.Attempts} · 클리어 {record.Clears} · 최고 등급 {record.BestRank}\n" +
                $"최고 정기 {record.BestEssence}   ·   최단 클리어 {PlayStatsLogic.FormatClock(record.BestClearSeconds)}\n" +
                $"보유 계약 정기  {essence}";
        }
    }
}
