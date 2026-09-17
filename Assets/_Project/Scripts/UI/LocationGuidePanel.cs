using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 스테이지 안의 장소 규칙 카드다 (35일차).
    ///
    ///   왼쪽: 목표 · 이 장소의 규칙 · 공략 팁      오른쪽: 적 정보
    ///
    /// 이 칸에서 처음 도전하는 장소면 로딩 · 시작 카드 선택이 끝난 뒤 한 번 자동으로 뜬다.
    /// 일시정지 메뉴의 [장소 규칙]으로 언제든 다시 연다. 열려 있는 동안 게임은 멈춘다.
    /// Enter · Esc · 버튼으로 닫는다(Space는 대시와 겹쳐 쓰지 않는다).
    /// </summary>
    public sealed class LocationGuidePanel : MonoBehaviour
    {
        /// <summary>일시정지 메뉴(260)보다 위다.</summary>
        private const int SortOrder = 270;

        private const float WindowWidth = 1180f;
        private const float WindowHeight = 780f;
        private const float ColumnWidth = 530f;

        private StageSessionController _stage;
        private RunUpgradeChoicePanel _cards;
        private UiOverlayParts _parts;
        private bool _autoShowPending;
        private bool _pauseHeld;

        public bool IsOpen =>
            _parts.Root != null &&
            _parts.Root.activeSelf;

        public void Configure(
            StageSessionController stage,
            bool showOnStart)
        {
            _stage = stage;
            _autoShowPending = showOnStart;

            Build();
        }

        private void Build()
        {
            LocationDefinition location = LocationContext.Current;
            LocationGuide guide = LocationGuideCatalog.Get(location.Id);

            Canvas canvas =
                UiFactory.CreateCanvas(
                    "LocationGuideCanvas",
                    SortOrder,
                    transform);

            _parts =
                UiOverlay.Create(
                    canvas.transform,
                    "LocationGuideOverlay",
                    SortOrder,
                    new Vector2(WindowWidth, WindowHeight),
                    $"장소 규칙  ·  {location.DisplayName}",
                    Hide);

            RectTransform w = _parts.Window;

            UiOverlay.Label(
                w,
                $"난이도 {LocationGuideLogic.FormatDifficulty(guide.Difficulty)}   ·   {LocationCatalog.GetTimeLabel(location.TimeOfDay)}   ·   {location.Summary}",
                40f,
                80f,
                WindowWidth - 80f,
                UiTheme.FontSmall,
                UiTheme.Gold);

            Column(w, "Rules", 40f, LocationGuideLogic.BuildRuleText(location, guide));
            Column(w, "Enemies", 40f + ColumnWidth + 40f, LocationGuideLogic.BuildEnemyText(location, guide));

            UiButton start =
                UiFactory.CreateButton(
                    w,
                    "Start",
                    "확인  (Enter)",
                    UiTheme.FontHeading,
                    true);

            UiFactory.Place(
                start.Background.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 30f),
                new Vector2(320f, 64f));

            start.Button.onClick.AddListener(Hide);

            UiOverlay.Label(
                w,
                "이 카드는 일시정지 메뉴(Esc) → [장소 규칙]에서 다시 볼 수 있습니다.",
                40f,
                WindowHeight - 120f,
                WindowWidth - 80f,
                UiTheme.FontTiny,
                UiTheme.TextDisabled);
        }

        private static void Column(
            RectTransform window,
            string name,
            float x,
            string content)
        {
            Image background =
                UiFactory.CreateImage(
                    window,
                    name,
                    UiTheme.RowFill);

            UiFactory.Place(
                background.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(x, -118f),
                new Vector2(ColumnWidth, WindowHeight - 260f));

            Text text =
                UiFactory.CreateText(
                    background.transform,
                    "Text",
                    content,
                    UiTheme.FontSmall + 1,
                    UiTheme.TextPrimary,
                    TextAnchor.UpperLeft);

            UiFactory.Stretch(text.rectTransform, 18f);

            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.2f;
            text.supportRichText = true;
        }

        /// <summary>일시정지 메뉴에서 연다.</summary>
        public void Open()
        {
            if (_parts.Root == null ||
                IsOpen)
            {
                return;
            }

            _autoShowPending = false;

            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);

            if (!_pauseHeld)
            {
                _pauseHeld = true;
                GameplayPause.Request();
            }

            GameAudio.Play(GameSfx.UiTick);
        }

        private void Hide()
        {
            if (!IsOpen)
            {
                return;
            }

            _parts.Root.SetActive(false);
            UiEscapeStack.Remove(this);

            ReleasePause();

            GameAudio.Play(GameSfx.UiTick);
        }

        private void ReleasePause()
        {
            if (!_pauseHeld)
            {
                return;
            }

            _pauseHeld = false;
            GameplayPause.Release();
        }

        private void Update()
        {
            if (_cards == null)
            {
                _cards = FindFirstObjectByType<RunUpgradeChoicePanel>();
            }

            bool running =
                _stage != null &&
                _stage.IsRunning;

            // 첫 입장: 로딩 · 시작 카드 선택 · 엔딩이 끝나고 판이 흐를 때 한 번 띄운다.
            if (_autoShowPending &&
                running &&
                PauseMenuLogic.CanOpen(
                    true,
                    _cards != null && _cards.IsOpen,
                    Boss.EndingSequence.IsPlaying,
                    LoadingScreen.IsLoading) &&
                UiEscapeStack.IsEmpty)
            {
                Open();

                return;
            }

            if (!IsOpen)
            {
                return;
            }

            if (!running)
            {
                Hide();

                return;
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                Hide();

                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                Hide();
            }
#endif
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);

            // 열린 채로 씬을 나가면 다음 씬이 멈춘 채로 시작한다.
            ReleasePause();
        }
    }
}
