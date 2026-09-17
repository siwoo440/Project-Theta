using System;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Presentation;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 첫 소개 화면이다 (35일차). 메인 메뉴 위에 화면 전체를 덮는다.
    ///
    ///   제목 · 두 줄 글 · "2 / 5" · [건너뛰기] [다음 ▶]
    ///
    /// 화면 클릭 · Space · Enter로 다음 장, Esc · 건너뛰기로 끝낸다.
    /// 끝나면 <see cref="Play"/>에 넘긴 콜백을 부른다(새 게임이면 허브로 간다).
    /// 그림은 아직 없어서 배경색 · 빛 알갱이 · 큰 글자로만 보여 준다. 그림 자리는 <see cref="_art"/>다.
    /// </summary>
    public sealed class IntroSequence : MonoBehaviour
    {
        private GameObject _root;
        private Image _art;
        private Text _title;
        private Text _body;
        private Text _counter;
        private UiButton _advance;

        private int _page;
        private float _elapsed;
        private Action _onFinished;

        public bool IsPlaying =>
            _root != null &&
            _root.activeSelf;

        public static IntroSequence Create(
            Transform canvas,
            int sortingOrder)
        {
            GameObject host = new GameObject("IntroSequence");
            host.transform.SetParent(canvas, false);

            IntroSequence intro = host.AddComponent<IntroSequence>();
            intro.Build(canvas, sortingOrder);

            return intro;
        }

        private void Build(
            Transform parent,
            int sortingOrder)
        {
            _root =
                new GameObject(
                    "IntroOverlay",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(GraphicRaycaster));

            _root.transform.SetParent(parent, false);
            UiFactory.Stretch((RectTransform)_root.transform);

            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            Transform root = _root.transform;

            // 배경 전체가 "다음" 버튼이다.
            UiButton backdrop =
                UiFactory.CreateButton(
                    root,
                    "Backdrop",
                    string.Empty);

            UiFactory.Stretch(backdrop.Background.rectTransform);
            backdrop.Background.color = new Color(0.035f, 0.025f, 0.060f, 1f);
            backdrop.Button.transition = Selectable.Transition.None;
            backdrop.Button.onClick.AddListener(Advance);

            RectTransform motes = UiFactory.CreateRect(root, "Motes");
            UiFactory.Stretch(motes);
            motes.gameObject.AddComponent<UiFloatingMotes>().Build(
                new Color(UiTheme.Accent.r, UiTheme.Accent.g, UiTheme.Accent.b, 0.35f));

            // 나중에 장마다 그림을 넣을 자리. 지금은 옅은 보라 빛만 둔다.
            _art = UiDecor.CreateGlow(root, "Art", UiTheme.Accent, new Vector2(900f, 420f));
            _art.raycastTarget = false;
            UiFactory.Place(_art.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(900f, 420f));

            _title =
                UiFactory.CreateText(root, "Title", string.Empty, UiTheme.FontTitle, UiTheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            UiFactory.Place(_title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1400f, 70f));

            _body =
                UiFactory.CreateText(root, "Body", string.Empty, UiTheme.FontHeading, UiTheme.TextMuted, TextAnchor.UpperCenter);

            UiFactory.Place(_body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(1400f, 120f));
            _body.lineSpacing = 1.4f;

            _counter =
                UiFactory.CreateText(root, "Counter", string.Empty, UiTheme.FontBody, UiTheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);

            UiFactory.Place(_counter.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(300f, 30f));

            UiButton skip = UiFactory.CreateButton(root, "Skip", "건너뛰기  (Esc)", UiTheme.FontBody);
            UiFactory.Place(skip.Background.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-180f, 70f), new Vector2(240f, 56f));
            skip.Button.onClick.AddListener(Finish);

            _advance = UiFactory.CreateButton(root, "Advance", "다음  ▶", UiTheme.FontHeading, true);
            UiFactory.Place(_advance.Background.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, 70f), new Vector2(300f, 56f));
            _advance.Button.onClick.AddListener(Advance);

            Text hint =
                UiFactory.CreateText(root, "Hint", "클릭 · Space · Enter 다음", UiTheme.FontTiny, UiTheme.TextDisabled, TextAnchor.MiddleCenter);

            UiFactory.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(400f, 24f));

            _root.SetActive(false);
        }

        /// <summary>처음 장부터 보여 준다. 끝나면(건너뛰기 포함) <paramref name="onFinished"/>를 부른다.</summary>
        public void Play(
            Action onFinished)
        {
            if (_root == null)
            {
                onFinished?.Invoke();

                return;
            }

            _onFinished = onFinished;
            _root.SetActive(true);
            UiEscapeStack.Push(this);

            ShowPage(0);
        }

        private void ShowPage(
            int page)
        {
            _page = page;
            _elapsed = 0f;

            IntroPage content = IntroLogic.Pages[page];

            _title.text = content.Title;
            _body.text = content.Body;
            _counter.text = IntroLogic.GetCounter(page);
            _advance.SetText(IntroLogic.GetAdvanceLabel(page));

            ApplyFade();
        }

        private void Advance()
        {
            if (!IsPlaying)
            {
                return;
            }

            // 글자가 떠오르는 중이면 먼저 다 보여 준다.
            if (IntroLogic.GetFade(_elapsed) < 1f)
            {
                _elapsed = IntroLogic.FadeSeconds;
                ApplyFade();

                return;
            }

            int next = IntroLogic.Next(_page);

            if (IntroLogic.IsFinished(next))
            {
                Finish();

                return;
            }

            GameAudio.Play(GameSfx.UiTick);
            ShowPage(next);
        }

        private void Finish()
        {
            if (!IsPlaying)
            {
                return;
            }

            _root.SetActive(false);
            UiEscapeStack.Remove(this);

            GameAudio.Play(GameSfx.UiStamp);

            Action callback = _onFinished;
            _onFinished = null;
            callback?.Invoke();
        }

        private void ApplyFade()
        {
            float alpha = IntroLogic.GetFade(_elapsed);

            SetAlpha(_title, UiTheme.TextPrimary, alpha);
            SetAlpha(_body, UiTheme.TextMuted, alpha);

            // 글자가 아래에서 살짝 떠오른다.
            _title.rectTransform.anchoredPosition = new Vector2(0f, 110f - (1f - alpha) * 16f);
        }

        private static void SetAlpha(
            Text text,
            Color color,
            float alpha)
        {
            color.a *= alpha;
            text.color = color;
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            ApplyFade();

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                Finish();

                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                Advance();
            }
#endif
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);
        }
    }
}
