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
    /// 가운데는 장소 8곳과 길이다. 35일차: 장소를 고르면 오른쪽에서 패널이 들어오고 지도는 왼쪽으로 비킨다.
    /// 패널의 [적 정보] [장소 규칙] [보상] [기록] 버튼이 패널 왼쪽에 상세 창을 연다.
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
            public Image Recommend;
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
        private Text _runStatus;
        private UiButton _departButton;

        // 35일차: 슬라이드 패널 · 상세 창 · 추천
        private RectTransform _mapRect;
        private RectTransform _panel;
        private float _panelShown;
        private Text _infoDifficulty;
        private Text _infoNumber;
        private Text _infoRecommend;
        private readonly UiButton[] _detailButtons = new UiButton[5];

        // 36일차: 심야 모드 · 이야기
        private bool _nightMode;
        private UiButton _nightButton;
        private Image _panelEdge;
        private DialogueOverlay _story;
        private LocationDetailKind _detail;
        private RectTransform _detailWindow;
        private Text _detailTitle;
        private Text _detailBody;
        private LocationId? _recommended;

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
            else
            {
                // 35일차: 처음 열면 추천 장소를 골라 둔다.
                _selected =
                    LocationGuideLogic.GetRecommended(
                        _candidates,
                        CurrentSave);
            }

            Build();
            Refresh();

            // 처음 열 때는 미끄러지지 않고 바로 제자리에 둔다.
            _panelShown = _selected != null ? 1f : 0f;
            AnimatePanel();

            // 30일차: 방금 끝낸 장소에서 달성한 업적을 알린다.
            if (game != null)
            {
                AchievementToast.Show(
                    _canvas.transform,
                    game.TakeNewAchievements());
            }

            // 36일차: 첫 클리어 · 라이벌 도발 · 후일담
            _story = DialogueOverlay.Create(_canvas.transform, StoryPlayback.SortOrder);
            StoryPlayback.PlayPending(_story, Refresh);
        }

        private void Update()
        {
            AnimatePanel();

            // 36일차: 대사가 나오는 동안 지도 단축키(Enter 출발 등)를 받지 않는다.
            if (_story != null &&
                _story.IsPlaying)
            {
                return;
            }

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

            // 35일차: 장소를 골랐으면 Tab은 장소 규칙, 아니면 통계다.
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                if (_selected != null)
                {
                    ToggleDetail(LocationDetailKind.Rules);
                }
                else
                {
                    OpenStats();
                }
            }

            // Esc: 상세 창 → 패널 순서로 닫는다.
            if (keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsumeWhenEmpty(Time.frameCount))
            {
                if (_detail != LocationDetailKind.None)
                {
                    SetDetail(LocationDetailKind.None);
                }
                else
                {
                    ClosePanel();
                }
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
            // 36일차: 심야 모드를 켰으면 함께 넘긴다.
            game.BeginLocation(
                _selected.Value,
                _nightMode);

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
                    "통  계",
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
                new Vector2(MapLeftCentered, -60f),
                new Vector2(MapWidth, MapHeight));

            _mapRect = map;

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

            // 35일차: 난이도(◆)를 시간대 앞에 둔다. 목표 방식은 패널에서 본다.
            Text detail =
                UiFactory.CreateText(
                    fill.transform,
                    "Detail",
                    $"난이도 {LocationGuideLogic.FormatDifficulty(LocationGuideCatalog.Get(location.Id).Difficulty)}  ·  {LocationCatalog.GetTimeLabel(location.TimeOfDay)}",
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

            // 35일차: 추천 장소 위에 뜨는 표시
            Image recommend =
                UiFactory.CreateImage(
                    map,
                    "Recommend",
                    UiTheme.Gold);

            recommend.rectTransform.anchorMin = Vector2.zero;
            recommend.rectTransform.anchorMax = Vector2.zero;
            recommend.rectTransform.pivot = new Vector2(0f, 0f);
            recommend.rectTransform.anchoredPosition = position + new Vector2(-NodeWidth * 0.5f + 12f, NodeHeight * 0.5f - 4f);
            recommend.rectTransform.sizeDelta = new Vector2(78f, 26f);

            Text recommendLabel =
                UiFactory.CreateText(
                    recommend.transform,
                    "Label",
                    "◆ 추천",
                    UiTheme.FontSmall,
                    new Color(0.10f, 0.07f, 0.12f, 1f),
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                recommendLabel.rectTransform);

            UiPulse recommendPulse = recommend.gameObject.AddComponent<UiPulse>();
            recommendPulse.Target = recommend;
            recommendPulse.MinimumAlpha = 0.6f;
            recommendPulse.MaximumAlpha = 1f;
            recommendPulse.Period = 1.2f;

            recommend.gameObject.SetActive(false);

            _nodes[id] =
                new NodeView
                {
                    Recommend = recommend,
                    Location = location,
                    Button = button,
                    Edge = button.Background,
                    Glow = glow,
                    Name = name,
                    Detail = detail,
                    Badge = badge
                };
        }

        // 35일차: 오른쪽 패널 --------------------------------------------

        private const float PanelWidth = 540f;
        private const float PanelMargin = 60f;
        private const float PanelPadding = 34f;
        private const float DetailWidth = 480f;
        private const float DetailGap = 16f;
        private const float SlideSeconds = 0.25f;

        /// <summary>패널이 없을 때 지도는 가운데, 패널이 열리면 왼쪽으로 비킨다.</summary>
        private const float MapLeftWithPanel = 70f;

        private static float MapLeftCentered =>
            (UiTheme.ReferenceResolution.x - MapWidth) * 0.5f;

        private static float PanelHiddenX =>
            PanelWidth + PanelMargin + 40f;

        /// <summary>
        /// 장소를 고르면 오른쪽에서 밀려 들어오는 패널이다 (35일차).
        ///   시간대 · 목표 · 난이도 / 큰 번호 · 이름 / 소개 / 핵심 수치 / 추천
        ///   [적 정보] [장소 규칙] [보상] [기록]
        ///   [닫기] [출발]
        /// 긴 정보는 버튼을 눌러 패널 왼쪽에 붙는 상세 창에서 본다.
        /// </summary>
        private void BuildInfoPanel(
            Transform parent)
        {
            float inner = PanelWidth - PanelPadding * 2f;

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
                new Vector2(PanelHiddenX, -60f),
                new Vector2(PanelWidth, MapHeight));

            _panel = panel;

            // 패널 왼쪽 가장자리의 금색 띠
            Image accent =
                UiFactory.CreateImage(
                    panel,
                    "Accent",
                    new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.8f));

            UiFactory.Place(accent.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(5f, MapHeight));

            _infoTime =
                CreateInfoText(panel, PanelPadding, 30f, inner, 26f, UiTheme.FontBody, UiTheme.Gold, true);

            _infoDifficulty =
                CreateInfoText(panel, PanelPadding, 30f, inner, 26f, UiTheme.FontBody, UiTheme.TextPrimary, true);

            _infoDifficulty.alignment = TextAnchor.MiddleRight;

            _infoNumber =
                CreateInfoText(panel, PanelPadding, 66f, 110f, 72f, UiTheme.FontTitle + 10, UiTheme.Gold, true);

            _infoName =
                CreateInfoText(panel, PanelPadding + 110f, 66f, inner - 110f, 72f, UiTheme.FontTitle - 8, UiTheme.TextPrimary, true);

            _infoSummary =
                CreateInfoText(panel, PanelPadding, 146f, inner, 56f, UiTheme.FontBody, UiTheme.TextMuted, false);

            _infoSummary.horizontalOverflow = HorizontalWrapMode.Wrap;
            _infoSummary.alignment = TextAnchor.UpperLeft;

            UiDecor.CreateDivider(
                panel,
                "Divider",
                new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.5f),
                212f,
                PanelPadding);

            _infoRows =
                CreateInfoText(panel, PanelPadding, 228f, inner, 100f, UiTheme.FontBody, UiTheme.TextPrimary, false);

            _infoRows.alignment = TextAnchor.UpperLeft;
            _infoRows.lineSpacing = 1.35f;

            _infoRecommend =
                CreateInfoText(panel, PanelPadding, 334f, inner, 42f, UiTheme.FontBody, UiTheme.Gold, true);

            UiPulse recommendPulse = _infoRecommend.gameObject.AddComponent<UiPulse>();
            recommendPulse.Target = _infoRecommend;
            recommendPulse.MinimumAlpha = 0.55f;
            recommendPulse.MaximumAlpha = 1f;
            recommendPulse.Period = 1.6f;

            _infoRecommend.horizontalOverflow = HorizontalWrapMode.Wrap;

            _panelEdge = panel.GetComponent<Image>();

            // 36일차: 버튼 3칸 × 2줄. 마지막 칸은 심야 모드 켜기/끄기다.
            float buttonWidth = (inner - 32f) / 3f;
            float Column(int index) => PanelPadding + index * (buttonWidth + 16f);

            _detailButtons[0] = UiOverlay.Button(panel, "EnemyButton", "적 정보", Column(0), 380f, buttonWidth, 64f, () => ToggleDetail(LocationDetailKind.Enemies));
            _detailButtons[1] = UiOverlay.Button(panel, "RuleButton", "규칙 (Tab)", Column(1), 380f, buttonWidth, 64f, () => ToggleDetail(LocationDetailKind.Rules));
            _detailButtons[2] = UiOverlay.Button(panel, "RewardButton", "보  상", Column(2), 380f, buttonWidth, 64f, () => ToggleDetail(LocationDetailKind.Rewards));
            _detailButtons[3] = UiOverlay.Button(panel, "RecordButton", "기  록", Column(0), 456f, buttonWidth, 64f, () => ToggleDetail(LocationDetailKind.Record));
            _detailButtons[4] = UiOverlay.Button(panel, "StoryButton", "이야기", Column(1), 456f, buttonWidth, 64f, () => ToggleDetail(LocationDetailKind.Story));
            _nightButton = UiOverlay.Button(panel, "NightButton", "-", Column(2), 456f, buttonWidth, 64f, ToggleNight);

            UiDecor.CreateDivider(
                panel,
                "Divider2",
                new Color(UiTheme.PanelEdge.r, UiTheme.PanelEdge.g, UiTheme.PanelEdge.b, 0.6f),
                544f,
                PanelPadding);

            _runStatus =
                CreateInfoText(panel, PanelPadding, 560f, inner, 60f, UiTheme.FontSmall, UiTheme.TextMuted, false);

            _runStatus.alignment = TextAnchor.UpperLeft;
            _runStatus.lineSpacing = 1.3f;

            UiButton close =
                UiFactory.CreateButton(
                    panel,
                    "ClosePanel",
                    "닫  기",
                    UiTheme.FontBody);

            UiFactory.Place(
                close.Background.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(PanelPadding, 40f),
                new Vector2(140f, 70f));

            close.Button.onClick.AddListener(
                ClosePanel);

            _departButton =
                UiFactory.CreateButton(
                    panel,
                    "Depart",
                    "출    발  ▶",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(
                _departButton.Background.rectTransform,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-PanelPadding, 40f),
                new Vector2(inner - 156f, 70f));

            _departButton.Button.onClick.AddListener(
                Depart);

            BuildDetailWindow(
                parent);
        }

        /// <summary>패널 왼쪽에 붙는 상세 창이다. 한 번에 하나만 뜬다.</summary>
        private void BuildDetailWindow(
            Transform parent)
        {
            RectTransform window =
                UiFactory.CreatePanel(
                    parent,
                    "Detail",
                    new Color(0.075f, 0.065f, 0.115f, 0.97f),
                    UiTheme.Gold);

            UiFactory.Place(
                window,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(-(PanelMargin + PanelWidth + DetailGap), -60f),
                new Vector2(DetailWidth, MapHeight));

            _detailWindow = window;

            _detailTitle =
                CreateInfoText(window, 28f, 26f, DetailWidth - 120f, 40f, UiTheme.FontHeading, UiTheme.Gold, true);

            UiButton close =
                UiFactory.CreateButton(
                    window,
                    "CloseDetail",
                    "×",
                    UiTheme.FontHeading);

            UiFactory.Place(
                close.Background.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-20f, -22f),
                new Vector2(48f, 48f));

            close.Button.onClick.AddListener(
                () => SetDetail(LocationDetailKind.None));

            UiDecor.CreateDivider(
                window,
                "Divider",
                new Color(UiTheme.Gold.r, UiTheme.Gold.g, UiTheme.Gold.b, 0.5f),
                82f,
                28f);

            _detailBody =
                CreateInfoText(window, 28f, 100f, DetailWidth - 56f, MapHeight - 130f, UiTheme.FontSmall + 1, UiTheme.TextPrimary, false);

            _detailBody.alignment = TextAnchor.UpperLeft;
            _detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailBody.verticalOverflow = VerticalWrapMode.Overflow;
            _detailBody.lineSpacing = 1.2f;
            _detailBody.supportRichText = true;

            window.gameObject.SetActive(false);
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
                $"가고 싶은 장소를 고르세요  (숫자 키 1~{_candidates.Count} · Enter 출발 · Tab 장소 규칙 · Esc 닫기)";

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

            // 35일차: 추천 장소(아직 깨지 않은 곳 중 가장 쉬운 곳)
            _recommended =
                LocationGuideLogic.GetRecommended(
                    _candidates,
                    save);

            foreach (KeyValuePair<LocationId, NodeView> pair in _nodes)
            {
                NodeView view = pair.Value;
                LocationStats record = PlayStatsLogic.Get(save, (int)pair.Key);
                NodeState state = GetState(pair.Key, record);

                view.Button.Button.interactable = true;
                view.Glow.gameObject.SetActive(state == NodeState.Selected);
                view.Recommend.gameObject.SetActive(_recommended != null && _recommended.Value == pair.Key);

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
                        view.Badge.text =
                            record.NightClears > 0
                                ? $"{number} 클리어 {record.Clears} · {record.BestRank} · ☾"
                                : $"{number} 클리어 {record.Clears} · {record.BestRank}";
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

            // 35일차: 고른 장소가 없으면 패널이 빠져나간다. 글자는 빠지는 동안 그대로 둔다.
            if (!hasSelection)
            {
                SetDetail(LocationDetailKind.None);

                return;
            }

            LocationDefinition location =
                LocationCatalog.Get(
                    _selected.Value);

            LocationGuide guide =
                LocationGuideCatalog.Get(
                    location.Id);

            int stars =
                MasteryLogic.GetStars(
                    CurrentSave,
                    (int)location.Id);

            _infoTime.text =
                $"{LocationCatalog.GetTimeLabel(location.TimeOfDay)}  ·  {LocationCatalog.GetObjectiveLabel(location.Objective)}";

            _infoTime.color =
                LocationCatalog.GetTimeColor(
                    location.TimeOfDay);

            _infoDifficulty.text = $"난이도 {LocationGuideLogic.FormatDifficulty(guide.Difficulty)}";
            _infoNumber.text = LocationGuideLogic.FormatNumber(_candidates.IndexOf(location.Id));
            _infoName.text = location.DisplayName;
            _infoSummary.text = location.Summary;
            _infoRows.text = LocationGuideLogic.BuildCoreRows(location, stars, _nightMode);

            _infoRecommend.text =
                _recommended != null &&
                _recommended.Value == location.Id
                    ? "◆ 추천 장소  ·  아직 깨지 않은 곳 중 가장 쉬워요"
                    : string.Empty;

            // 36일차: 심야 모드
            bool unlocked = NightModeLogic.IsUnlocked(CurrentSave);

            if (!unlocked)
            {
                _nightMode = false;
            }

            _nightButton.SetText(NightModeLogic.GetToggleLabel(unlocked, _nightMode));
            _nightButton.SetInteractable(unlocked);
            _nightButton.Background.color = _nightMode ? NightColor : UiTheme.ButtonNormal;
            _panelEdge.color = _nightMode ? NightColor : UiTheme.PanelEdge;

            if (_nightMode)
            {
                _infoRecommend.text = NightModeLogic.GetShortSummary();
                _infoRecommend.fontSize = UiTheme.FontSmall;
            }
            else
            {
                _infoRecommend.fontSize = UiTheme.FontBody;
            }

            RefreshDetail();
        }

        private void RefreshRunStatus()
        {
            SaveData save = CurrentSave;
            int essence = save == null ? 0 : save.ContractEssence;

            _runStatus.text =
                $"보유 계약 정기  {essence:N0}\n" +
                "레벨 · 강화 카드는 장소마다 처음부터 시작합니다.";
        }

        // 35일차: 패널 · 상세 창 ------------------------------------------

        private static readonly Color NightColor =
            new Color(0.28f, 0.34f, 0.78f, 1f);

        /// <summary>36일차: 엔딩을 본 칸에서만 켤 수 있다.</summary>
        private void ToggleNight()
        {
            if (!NightModeLogic.IsUnlocked(CurrentSave))
            {
                return;
            }

            _nightMode = !_nightMode;

            // 37일차: 켤 때는 낮은 종, 끌 때는 짧은 소리
            GameAudio.Play(
                _nightMode
                    ? GameSfx.NightToggle
                    : GameSfx.UiTick);

            Refresh();
        }

        private void ClosePanel()
        {
            if (_selected == null)
            {
                return;
            }

            _selected = null;

            GameAudio.Play(
                GameSfx.UiTick);

            Refresh();
        }

        private void ToggleDetail(
            LocationDetailKind kind)
        {
            if (_selected == null)
            {
                return;
            }

            LocationDetailKind next =
                LocationGuideLogic.Toggle(
                    _detail,
                    kind);

            GameAudio.Play(
                next == LocationDetailKind.None
                    ? GameSfx.WindowClose
                    : GameSfx.WindowOpen);

            SetDetail(next);
        }

        private void SetDetail(
            LocationDetailKind kind)
        {
            _detail = kind;

            RefreshDetail();
        }

        private void RefreshDetail()
        {
            bool open =
                _detail != LocationDetailKind.None &&
                _selected != null;

            _detailWindow.gameObject.SetActive(
                open);

            for (int i = 0; i < _detailButtons.Length; i++)
            {
                bool selected = open && (int)_detail == i + 1;

                _detailButtons[i].Background.color =
                    selected
                        ? UiTheme.PrimaryButtonNormal
                        : UiTheme.ButtonNormal;
            }

            if (!open)
            {
                return;
            }

            LocationDefinition location =
                LocationCatalog.Get(
                    _selected.Value);

            _detailTitle.text =
                $"{LocationGuideLogic.GetDetailTitle(_detail)}  ·  {location.DisplayName}";

            _detailBody.text =
                LocationGuideLogic.BuildDetail(
                    _detail,
                    location,
                    CurrentSave);
        }

        /// <summary>패널 · 지도를 부드럽게 옮긴다. 열릴 때 지도는 왼쪽으로 비킨다.</summary>
        private void AnimatePanel()
        {
            float target =
                _selected != null
                    ? 1f
                    : 0f;

            _panelShown =
                Mathf.MoveTowards(
                    _panelShown,
                    target,
                    Time.unscaledDeltaTime / SlideSeconds);

            float t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    _panelShown);

            if (_panel != null)
            {
                _panel.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(PanelHiddenX, -PanelMargin, t),
                        _panel.anchoredPosition.y);
            }

            if (_mapRect != null)
            {
                _mapRect.anchoredPosition =
                    new Vector2(
                        Mathf.Lerp(MapLeftCentered, MapLeftWithPanel, t),
                        _mapRect.anchoredPosition.y);
            }
        }
    }
}
