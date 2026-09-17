using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Story;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 이야기 대사 창이다 (36일차).
    ///
    ///   「장면 제목」                                         [건너뛰기 (Esc)]
    ///   ┌──────────────────────────────────────────────────────────────┐
    ///   │ [말하는 사람]                                                  │
    ///   │ 대사가 한 글자씩 나온다 …                                   ▼   │
    ///   └──────────────────────────────────────────────────────────────┘
    ///
    /// 화면 클릭 · Space · Enter: 글자가 다 나왔으면 다음 줄, 아니면 한 번에 보여 준다.
    /// Esc · 건너뛰기: 남은 장면을 모두 건너뛴다(건너뛴 장면도 본 것으로 남는다).
    /// 장소 안에서는 게임을 멈추고 연다. 인물 그림 자리는 <see cref="_portrait"/>다.
    /// </summary>
    public sealed class DialogueOverlay : MonoBehaviour
    {
        private const float BoxWidth = 1500f;
        private const float BoxHeight = 210f;

        private static int _busyCount;

        /// <summary>
        /// 대사 창이 떠 있거나 곧 뜰 예정인지다.
        /// 장소 규칙 카드가 대사가 끝날 때까지 기다리는 데 쓴다.
        /// </summary>
        public static bool Busy =>
            _busyCount > 0;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _busyCount = 0;
        }

        private GameObject _root;
        private Text _sceneTitle;
        private Text _speaker;
        private Image _speakerTag;
        private Text _line;
        private Text _next;
        private Image _portrait;

        private readonly List<StoryScene> _queue = new List<StoryScene>();
        private int _sceneIndex;
        private int _lineIndex;
        private float _elapsed;
        private bool _pauseGame;
        private bool _pauseHeld;
        private bool _reserved;
        private int _lastVisible;
        private Action<StoryScene> _onSceneShown;
        private Action _onFinished;

        public bool IsPlaying =>
            _root != null &&
            _root.activeSelf;

        public static DialogueOverlay Create(
            Transform canvas,
            int sortingOrder)
        {
            GameObject host = new GameObject("DialogueOverlay");
            host.transform.SetParent(canvas, false);

            DialogueOverlay overlay = host.AddComponent<DialogueOverlay>();
            overlay.Build(canvas, sortingOrder);

            return overlay;
        }

        /// <summary>곧 대사를 띄울 예정임을 알린다. <see cref="Play"/>가 끝나면 풀린다.</summary>
        public void Reserve()
        {
            if (_reserved)
            {
                return;
            }

            _reserved = true;
            _busyCount++;
        }

        private void ReleaseReserve()
        {
            if (!_reserved)
            {
                return;
            }

            _reserved = false;
            _busyCount = Mathf.Max(0, _busyCount - 1);
        }

        private void Build(
            Transform parent,
            int sortingOrder)
        {
            _root =
                new GameObject(
                    "DialogueRoot",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(GraphicRaycaster));

            _root.transform.SetParent(parent, false);
            UiFactory.Stretch((RectTransform)_root.transform);

            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            Transform root = _root.transform;

            // 화면 전체가 "다음" 버튼이다. 배경은 조금만 어둡게 해 뒤가 보이게 한다.
            UiButton dim = UiFactory.CreateButton(root, "Dim", string.Empty);
            UiFactory.Stretch(dim.Background.rectTransform);
            dim.Background.color = new Color(0f, 0f, 0f, 0.35f);
            dim.Button.transition = Selectable.Transition.None;
            dim.Button.onClick.AddListener(Advance);

            // 인물 그림 자리(지금은 옅은 빛)
            _portrait = UiDecor.CreateGlow(root, "Portrait", UiTheme.Accent, new Vector2(420f, 520f));
            _portrait.raycastTarget = false;
            UiFactory.Place(_portrait.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-520f, 220f), new Vector2(420f, 520f));

            RectTransform box =
                UiFactory.CreatePanel(
                    root,
                    "Box",
                    new Color(0.06f, 0.045f, 0.10f, 0.88f),
                    UiTheme.AccentSoft);

            UiFactory.Place(box, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(BoxWidth, BoxHeight));

            _speakerTag = UiFactory.CreateImage(box, "SpeakerTag", UiTheme.AccentSoft);
            UiFactory.Place(_speakerTag.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(40f, -8f), new Vector2(220f, 44f));

            _speaker = UiFactory.CreateText(_speakerTag.transform, "Speaker", string.Empty, UiTheme.FontSubheading, UiTheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Stretch(_speaker.rectTransform);

            _line = UiFactory.CreateText(box, "Line", string.Empty, UiTheme.FontHeading, UiTheme.TextPrimary, TextAnchor.UpperLeft);
            UiFactory.Place(_line.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -58f), new Vector2(BoxWidth - 140f, BoxHeight - 80f));
            _line.horizontalOverflow = HorizontalWrapMode.Wrap;
            _line.lineSpacing = 1.3f;
            _line.supportRichText = true;

            _next = UiFactory.CreateText(box, "Next", "▼", UiTheme.FontHeading, UiTheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Place(_next.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 20f), new Vector2(40f, 40f));

            UiPulse pulse = _next.gameObject.AddComponent<UiPulse>();
            pulse.Target = _next;
            pulse.MinimumAlpha = 0.2f;
            pulse.MaximumAlpha = 1f;
            pulse.Period = 0.9f;

            _sceneTitle = UiFactory.CreateText(root, "SceneTitle", string.Empty, UiTheme.FontSubheading, UiTheme.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Place(_sceneTitle.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-BoxWidth * 0.5f, 290f), new Vector2(900f, 32f));

            UiButton skip = UiFactory.CreateButton(root, "Skip", "건너뛰기  (Esc)", UiTheme.FontSmall);
            UiFactory.Place(skip.Background.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(BoxWidth * 0.5f, 284f), new Vector2(180f, 40f));
            skip.Button.onClick.AddListener(SkipAll);

            _root.SetActive(false);
        }

        /// <summary>
        /// 장면들을 차례로 보여 준다.
        /// <paramref name="onSceneShown"/>은 장면이 시작될 때마다(본 것으로 남길 때) 부른다.
        /// </summary>
        public void Play(
            IList<StoryScene> scenes,
            bool pauseGame,
            Action<StoryScene> onSceneShown,
            Action onFinished)
        {
            _queue.Clear();

            if (scenes != null)
            {
                foreach (StoryScene scene in scenes)
                {
                    if (scene != null &&
                        scene.Lines.Length > 0)
                    {
                        _queue.Add(scene);
                    }
                }
            }

            _onSceneShown = onSceneShown;
            _onFinished = onFinished;

            if (_queue.Count == 0 ||
                _root == null)
            {
                Finish();

                return;
            }

            Reserve();

            _pauseGame = pauseGame;

            if (_pauseGame &&
                !_pauseHeld)
            {
                _pauseHeld = true;
                GameplayPause.Request();
            }

            _root.SetActive(true);
            UiEscapeStack.Push(this);

            ShowScene(0);
        }

        private void ShowScene(
            int index)
        {
            _sceneIndex = index;

            StoryScene scene = _queue[index];

            _sceneTitle.text = $"「{scene.Title}」";
            _onSceneShown?.Invoke(scene);

            ShowLine(0);
        }

        private void ShowLine(
            int index)
        {
            _lineIndex = index;
            _elapsed = 0f;
            _lastVisible = 0;

            StoryLine line = _queue[_sceneIndex].Lines[index];
            bool narration = string.IsNullOrEmpty(line.Speaker);

            _speakerTag.gameObject.SetActive(!narration);
            _speaker.text = line.Speaker;

            if (ColorUtility.TryParseHtmlString(StoryLogic.GetSpeakerColor(line.Speaker), out Color color))
            {
                _speakerTag.color = new Color(color.r * 0.45f, color.g * 0.35f, color.b * 0.5f, 0.95f);
            }

            RefreshLine();
        }

        private StoryLine CurrentLine =>
            _queue[_sceneIndex].Lines[_lineIndex];

        private void RefreshLine()
        {
            StoryLine line = CurrentLine;
            int visible = StoryLogic.GetVisibleCharacters(_elapsed, line.Text.Length);
            string shown = line.Text.Substring(0, visible);

            // 37일차: 두 글자마다 작은 소리. 말하는 사람마다 높이가 다르다.
            if (MusicLogic.ShouldBlip(_lastVisible, visible, line.Text))
            {
                GameAudio.PlayPitched(GameSfx.DialogueBlip, 0.35f, MusicLogic.GetBlipPitch(line.Speaker));
            }

            _lastVisible = visible;

            _line.text =
                string.IsNullOrEmpty(line.Speaker)
                    ? $"<i>{shown}</i>"
                    : shown;

            _line.color =
                string.IsNullOrEmpty(line.Speaker)
                    ? UiTheme.TextMuted
                    : UiTheme.TextPrimary;

            _next.enabled = visible >= line.Text.Length;
        }

        private void Advance()
        {
            if (!IsPlaying)
            {
                return;
            }

            StoryLine line = CurrentLine;

            // 글자가 나오는 중이면 먼저 다 보여 준다.
            if (!StoryLogic.IsLineComplete(_elapsed, line.Text.Length))
            {
                _elapsed = StoryLogic.GetLineSeconds(line.Text.Length);
                RefreshLine();

                return;
            }

            GameAudio.Play(GameSfx.UiTick, 0.6f);

            if (_lineIndex + 1 < _queue[_sceneIndex].Lines.Length)
            {
                ShowLine(_lineIndex + 1);

                return;
            }

            if (_sceneIndex + 1 < _queue.Count)
            {
                ShowScene(_sceneIndex + 1);

                return;
            }

            Finish();
        }

        /// <summary>남은 장면을 모두 본 것으로 남기고 닫는다.</summary>
        private void SkipAll()
        {
            if (!IsPlaying)
            {
                return;
            }

            for (int i = _sceneIndex + 1; i < _queue.Count; i++)
            {
                _onSceneShown?.Invoke(_queue[i]);
            }

            Finish();
        }

        private void Finish()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            UiEscapeStack.Remove(this);
            ReleasePause();
            ReleaseReserve();

            _queue.Clear();
            _onSceneShown = null;

            Action callback = _onFinished;
            _onFinished = null;
            callback?.Invoke();
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
            if (!IsPlaying)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            RefreshLine();

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                SkipAll();

                return;
            }

            // 장소 안에서는 Space가 대시와 겹치므로 Enter · 클릭만 받는다.
            if ((!_pauseGame && keyboard.spaceKey.wasPressedThisFrame) ||
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
            ReleasePause();
            ReleaseReserve();
        }
    }
}
