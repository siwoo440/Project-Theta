using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 강화 카드 3장 중 하나를 고르는 화면이다.
    ///
    /// 열려 있는 동안 게임을 완전히 멈춘다. 시간 압박이 없는 유일한 순간이다.
    /// 카드 등장 연출은 멈춘 시간과 무관하게 흘러야 하므로 unscaledDeltaTime을 쓴다.
    ///
    /// 열리는 순간 플레이어는 최면 때문에 좌클릭을 누르고 있었을 가능성이 높다.
    /// 그 클릭이 곧바로 카드 선택으로 새지 않도록 잠깐 입력을 받지 않는다.
    /// </summary>
    public sealed class RunUpgradeChoicePanel : MonoBehaviour
    {
        private const int CardSlots = 3;

        /// <summary>열린 직후 입력을 무시하는 시간이다.</summary>
        private const float InputGuardSeconds = 0.35f;

        private const float CardInterval = 0.12f;
        private const float CardPopDuration = 0.22f;

        private Canvas _canvas;
        private Text _titleText;
        private Text _subtitleText;

        private readonly CardView[] _cards =
            new CardView[CardSlots];

        private readonly List<RunUpgradeCard> _offered =
            new List<RunUpgradeCard>();

        private Action<RunUpgradeCard> _onPicked;
        private float _openElapsed;
        private bool _pauseHeld;

        public bool IsOpen { get; private set; }

        private sealed class CardView
        {
            public RectTransform Root;
            public Image Edge;
            public Image Band;
            public Text Category;
            public Text Name;
            public Text Description;
            public Text Stacks;
            public Text Key;
            public UiButton Button;
        }

        private void Awake()
        {
            Build();

            _canvas.gameObject.SetActive(
                false);
        }

        private void OnDestroy()
        {
            // 열린 채로 씬을 나가면 다음 씬이 멈춘 상태로 시작한다.
            ReleasePause();
        }

        public void Show(
            string title,
            string subtitle,
            IReadOnlyList<RunUpgradeCard> cards,
            RunUpgradeState state,
            Action<RunUpgradeCard> onPicked)
        {
            _offered.Clear();

            for (int i = 0;
                 i < cards.Count &&
                 i < CardSlots;
                 i++)
            {
                _offered.Add(cards[i]);
            }

            _onPicked = onPicked;

            _titleText.text = title;
            _subtitleText.text = subtitle;

            for (int i = 0;
                 i < CardSlots;
                 i++)
            {
                bool used =
                    i < _offered.Count;

                _cards[i].Root.gameObject.SetActive(
                    used);

                if (used)
                {
                    FillCard(
                        _cards[i],
                        _offered[i],
                        state,
                        i);
                }
            }

            LayoutCards();

            _openElapsed = 0f;

            IsOpen = true;

            _canvas.gameObject.SetActive(
                true);

            if (!_pauseHeld)
            {
                GameplayPause.Request();

                _pauseHeld = true;
            }

            GameAudio.Play(
                GameSfx.UiStamp);

            ApplyReveal();
        }

        public void Hide()
        {
            IsOpen = false;

            _onPicked = null;

            _canvas.gameObject.SetActive(
                false);

            ReleasePause();
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
            if (!IsOpen)
            {
                return;
            }

            _openElapsed +=
                Time.unscaledDeltaTime;

            ApplyReveal();

            if (_openElapsed < InputGuardSeconds)
            {
                return;
            }

            int key =
                ReadNumberKey();

            if (key >= 0 &&
                key < _offered.Count)
            {
                Pick(key);
            }
        }

        private void Pick(
            int index)
        {
            if (!IsOpen ||
                index < 0 ||
                index >= _offered.Count ||
                _openElapsed < InputGuardSeconds)
            {
                return;
            }

            RunUpgradeCard card =
                _offered[index];

            Action<RunUpgradeCard> callback =
                _onPicked;

            GameAudio.Play(
                GameSfx.Purchase);

            Hide();

            callback?.Invoke(card);
        }

        private static int ReadNumberKey()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                return -1;
            }

            if (keyboard.digit1Key.wasPressedThisFrame ||
                keyboard.numpad1Key.wasPressedThisFrame)
            {
                return 0;
            }

            if (keyboard.digit2Key.wasPressedThisFrame ||
                keyboard.numpad2Key.wasPressedThisFrame)
            {
                return 1;
            }

            if (keyboard.digit3Key.wasPressedThisFrame ||
                keyboard.numpad3Key.wasPressedThisFrame)
            {
                return 2;
            }

            return -1;
#else
            if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
            return -1;
#endif
        }

        // 연출 ----------------------------------------------------------

        /// <summary>카드가 왼쪽부터 하나씩 "딱" 튀어나온다.</summary>
        private void ApplyReveal()
        {
            for (int i = 0;
                 i < _offered.Count;
                 i++)
            {
                float local =
                    _openElapsed -
                    (i * CardInterval);

                float scale;

                if (local <= 0f)
                {
                    scale = 0f;
                }
                else if (local >= CardPopDuration)
                {
                    scale = 1f;
                }
                else
                {
                    // ease-out-back: 약 110%까지 튀었다가 제자리로 돌아온다.
                    const float back = 1.70158f;

                    float u =
                        (local / CardPopDuration) - 1f;

                    scale =
                        1f +
                        ((back + 1f) * u * u * u) +
                        (back * u * u);
                }

                _cards[i].Root.localScale =
                    new Vector3(
                        scale,
                        scale,
                        1f);
            }
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            // HUD(50)와 포획(120)보다 위, 결과창(200)보다는 아래다.
            _canvas =
                UiFactory.CreateCanvas(
                    "RunUpgradeChoiceCanvas",
                    150,
                    transform);

            Image dim =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Dim",
                    new Color(0.02f, 0.01f, 0.04f, 0.80f));

            UiFactory.Stretch(
                dim.rectTransform);

            // 뒤쪽 게임 화면으로 클릭이 새지 않게 막는다.
            dim.raycastTarget = true;

            Image titleLine =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "TitleLine",
                    new Color(
                        UiTheme.Accent.r,
                        UiTheme.Accent.g,
                        UiTheme.Accent.b,
                        0.6f));

            UiFactory.Place(
                titleLine.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 330f),
                new Vector2(900f, 2f));

            _titleText =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Title",
                    string.Empty,
                    UiTheme.FontTitle - 4,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _titleText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 380f),
                new Vector2(1000f, 60f));

            _subtitleText =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Subtitle",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _subtitleText.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 296f),
                new Vector2(1000f, 30f));

            for (int i = 0;
                 i < CardSlots;
                 i++)
            {
                _cards[i] =
                    BuildCard(
                        _canvas.transform,
                        i);
            }

            Text hint =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Hint",
                    "클릭 또는 1 · 2 · 3 키로 선택",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                hint.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -330f),
                new Vector2(800f, 26f));
        }

        private CardView BuildCard(
            Transform parent,
            int index)
        {
            RectTransform root =
                UiFactory.CreateRect(
                    parent,
                    $"Card{index}");

            UiFactory.Place(
                root,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(340f, 480f));

            // 카드 전체가 버튼이다. 테두리 색이 계열을 나타낸다.
            UiButton button =
                UiFactory.CreateButton(
                    root,
                    "Button",
                    string.Empty);

            UiFactory.Stretch(
                button.Background.rectTransform);

            button.Background.color =
                UiTheme.PanelEdge;

            int captured =
                index;

            button.Button.onClick.AddListener(
                () =>
                {
                    Pick(captured);
                });

            Image fill =
                UiFactory.CreateImage(
                    button.Background.transform,
                    "Fill",
                    UiTheme.PanelFill);

            UiFactory.Stretch(
                fill.rectTransform,
                4f);

            // 윗부분 계열 띠
            Image band =
                UiFactory.CreateImage(
                    fill.transform,
                    "Band",
                    UiTheme.Accent);

            RectTransform bandRect =
                band.rectTransform;

            bandRect.anchorMin = new Vector2(0f, 1f);
            bandRect.anchorMax = new Vector2(1f, 1f);
            bandRect.pivot = new Vector2(0.5f, 1f);
            bandRect.offsetMin = Vector2.zero;
            bandRect.offsetMax = Vector2.zero;
            bandRect.sizeDelta = new Vector2(0f, 64f);

            Text category =
                UiFactory.CreateText(
                    band.transform,
                    "Category",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.Backdrop,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                category.rectTransform);

            Text name =
                UiFactory.CreateText(
                    fill.transform,
                    "Name",
                    string.Empty,
                    UiTheme.FontHeading + 2,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                name.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -120f),
                new Vector2(310f, 40f));

            Text description =
                UiFactory.CreateText(
                    fill.transform,
                    "Description",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextMuted,
                    TextAnchor.UpperCenter);

            description.horizontalOverflow =
                HorizontalWrapMode.Wrap;

            UiFactory.Place(
                description.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -180f),
                new Vector2(290f, 120f));

            Text stacks =
                UiFactory.CreateText(
                    fill.transform,
                    "Stacks",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                stacks.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 96f),
                new Vector2(310f, 30f));

            Image keyBox =
                UiFactory.CreateImage(
                    fill.transform,
                    "KeyBox",
                    UiTheme.TrackFill);

            UiFactory.Place(
                keyBox.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 26f),
                new Vector2(48f, 48f));

            Text key =
                UiFactory.CreateText(
                    keyBox.transform,
                    "Key",
                    (index + 1).ToString(),
                    UiTheme.FontHeading,
                    UiTheme.Accent,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(
                key.rectTransform);

            return new CardView
            {
                Root = root,
                Edge = button.Background,
                Band = band,
                Category = category,
                Name = name,
                Description = description,
                Stacks = stacks,
                Key = key,
                Button = button
            };
        }

        /// <summary>보여줄 카드 수에 맞춰 가운데 정렬한다.</summary>
        private void LayoutCards()
        {
            const float spacing = 380f;

            int count =
                _offered.Count;

            float start =
                -(count - 1) *
                spacing *
                0.5f;

            for (int i = 0;
                 i < count;
                 i++)
            {
                _cards[i].Root.anchoredPosition =
                    new Vector2(
                        start + (i * spacing),
                        -10f);
            }
        }

        private static void FillCard(
            CardView view,
            RunUpgradeCard card,
            RunUpgradeState state,
            int index)
        {
            RunUpgradeProfile profile =
                RunUpgradeTable.Get(card);

            Color color =
                GetCategoryColor(
                    profile.Category);

            view.Edge.color = color;
            view.Band.color = color;
            view.Key.color = color;

            view.Category.text =
                GetCategoryName(
                    profile.Category);

            view.Name.text =
                profile.DisplayName;

            view.Description.text =
                state == null
                    ? profile.DescriptionFormat
                    : state.DescribeNextPick(card);

            int current =
                state == null
                    ? 0
                    : state.GetStacks(card);

            view.Stacks.text =
                current <= 0
                    ? "NEW"
                    : BuildStars(
                        current + 1,
                        profile.MaximumStacks);

            view.Key.text =
                (index + 1).ToString();
        }

        /// <summary>고르면 몇 번째 스택이 되는지 별로 보여준다. ★ 채워질 칸, ☆ 남은 칸.</summary>
        private static string BuildStars(
            int afterPick,
            int maximum)
        {
            string stars = string.Empty;

            for (int i = 0;
                 i < maximum;
                 i++)
            {
                stars +=
                    i < afterPick
                        ? "★"
                        : "☆";
            }

            return stars;
        }

        public static Color GetCategoryColor(
            RunUpgradeCategory category)
        {
            switch (category)
            {
                case RunUpgradeCategory.Hypnosis:
                    return UiTheme.Accent;

                case RunUpgradeCategory.Command:
                    return UiTheme.Positive;

                case RunUpgradeCategory.Harvest:
                    return UiTheme.Gold;

                case RunUpgradeCategory.Mobility:
                default:
                    return UiTheme.Focus;
            }
        }

        public static string GetCategoryName(
            RunUpgradeCategory category)
        {
            switch (category)
            {
                case RunUpgradeCategory.Hypnosis:
                    return "최면";

                case RunUpgradeCategory.Command:
                    return "통솔";

                case RunUpgradeCategory.Harvest:
                    return "수급";

                case RunUpgradeCategory.Mobility:
                default:
                    return "기동";
            }
        }
    }
}
