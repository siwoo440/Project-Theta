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
        private Sprite _portraitFallbackSprite; // 임시 초상 그림
        private Color _portraitFallbackColor; // 임시 초상 색상
        private RectTransform _choiceRoot; // 선택지 영역

        private readonly List<StoryScene> _queue = new List<StoryScene>();
        private readonly List<UiButton> _choiceButtons = new List<UiButton>(); // 선택지 버튼 목록
        private readonly Dictionary<string, Sprite> _portraitCache = new Dictionary<string, Sprite>(); // 초상 캐시
        private readonly HashSet<string> _missingPortraitPaths = new HashSet<string>(); // 누락 초상 경로 캐시
        private int _sceneIndex;
        private int _lineIndex;
        private float _elapsed;
        private bool _pauseGame;
        private bool _pauseHeld;
        private bool _reserved;
        private int _lastVisible;
        private bool _waitingForChoice; // 선택 대기 여부
        private Action<StoryScene> _onSceneShown;
        private Action<StoryScene> _onSceneCompleted; // 장면 완료 알림
        private Action<StoryScene, StoryChoice> _onChoiceSelected; // 선택 결과 알림
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
            _portraitFallbackSprite = _portrait.sprite; // 임시 그림 보관
            _portraitFallbackColor = _portrait.color; // 임시 색상 보관
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

            _choiceRoot = // 선택지 영역 생성
                new GameObject( // 선택지 오브젝트 생성
                    "Choices", // 오브젝트 이름
                    typeof(RectTransform)) // 위치 구성 요소
                .GetComponent<RectTransform>(); // 위치 구성 요소 조회
            _choiceRoot.SetParent(root, false); // 대사창 아래 배치
            UiFactory.Place( // 선택지 영역 배치
                _choiceRoot, // 선택지 위치
                new Vector2(0.5f, 0f), // 아래 가운데 기준
                new Vector2(0.5f, 0f), // 아래 가운데 축
                new Vector2(0f, 350f), // 대사창 위 위치
                new Vector2(1000f, 60f)); // 선택지 영역 크기
            _choiceRoot.gameObject.SetActive(false); // 초기 숨김

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
            Play( // 확장 재생 호출
                scenes, // 장면 목록
                pauseGame, // 일시정지 여부
                onSceneShown, // 시작 알림
                null, // 완료 알림 없음
                null, // 선택 알림 없음
                onFinished); // 전체 완료 알림
        }

        public void Play( // 진행 상태 지원 재생
            IList<StoryScene> scenes, // 장면 목록
            bool pauseGame, // 일시정지 여부
            Action<StoryScene> onSceneShown, // 시작 알림
            Action<StoryScene> onSceneCompleted, // 완료 알림
            Action<StoryScene, StoryChoice> onChoiceSelected, // 선택 알림
            Action onFinished) // 전체 완료 알림
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
            _onSceneCompleted = onSceneCompleted; // 완료 알림 저장
            _onChoiceSelected = onChoiceSelected; // 선택 알림 저장
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
            _waitingForChoice = false; // 선택 대기 초기화
            HideChoices(); // 이전 선택지 숨김

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
            _waitingForChoice = false; // 선택 대기 해제
            HideChoices(); // 선택지 숨김

            StoryLine line = _queue[_sceneIndex].Lines[index];
            bool narration = string.IsNullOrEmpty(line.Speaker);

            _speakerTag.gameObject.SetActive(!narration);
            _speaker.text = line.Speaker;
            RefreshPortrait(line); // 화자 초상 갱신

            if (ColorUtility.TryParseHtmlString(StoryLogic.GetSpeakerColor(line.Speaker), out Color color))
            {
                _speakerTag.color = new Color(color.r * 0.45f, color.g * 0.35f, color.b * 0.5f, 0.95f);
            }

            RefreshLine();
        }

        private void RefreshPortrait( // 대화 초상 갱신
            StoryLine line) // 현재 대사
        { // 갱신 시작
            if (_portrait == null) // 초상 영역 확인
            { // 영역 없음 시작
                return; // 갱신 종료
            } // 영역 없음 끝

            if (!CharacterArtCatalog.TryGetCharacterBySpeaker(line.Speaker, out CharacterArtId id)) // 주요 화자 확인
            { // 주요 화자 아님 시작
                _portrait.gameObject.SetActive(false); // 초상 숨김

                return; // 갱신 종료
            } // 주요 화자 아님 끝

            _portrait.gameObject.SetActive(true); // 초상 표시
            Sprite art = LoadPortraitSprite( // 초상 이미지 조회
                CharacterArtCatalog.GetPortraitPaths(id, line.Expression)); // 표정별 후보 경로

            if (art != null) // 정식 초상 확인
            { // 정식 초상 시작
                _portrait.sprite = art; // 정식 그림 적용
                _portrait.color = Color.white; // 원본 색상 적용
                _portrait.preserveAspect = true; // 원본 비율 유지

                return; // 갱신 종료
            } // 정식 초상 끝

            _portrait.sprite = _portraitFallbackSprite; // 임시 그림 복원
            _portrait.color = _portraitFallbackColor; // 임시 색상 복원
            _portrait.preserveAspect = false; // 임시 그림 채우기
        } // 갱신 끝

        private Sprite LoadPortraitSprite( // 첫 유효 초상 조회
            string[] paths) // 후보 경로 목록
        { // 조회 시작
            if (paths == null) // 후보 목록 확인
            { // 목록 없음 시작
                return null; // 초상 없음 반환
            } // 목록 없음 끝

            for (int i = 0; i < paths.Length; i++) // 후보 경로 순회
            { // 순회 시작
                string path = paths[i]; // 현재 경로 조회

                if (_portraitCache.TryGetValue(path, out Sprite cached)) // 캐시 확인
                { // 캐시 존재 시작
                    return cached; // 캐시 초상 반환
                } // 캐시 존재 끝

                if (_missingPortraitPaths.Contains(path)) // 누락 경로 캐시 확인
                { // 누락 경로 시작
                    continue; // 다음 후보 이동
                } // 누락 경로 끝

                Texture2D texture = Resources.Load<Texture2D>(path); // 초상 텍스처 조회

                if (texture == null) // 텍스처 없음 확인
                { // 없음 시작
                    _missingPortraitPaths.Add(path); // 누락 경로 저장

                    continue; // 다음 후보 이동
                } // 없음 끝

                Sprite sprite = Sprite.Create( // 초상 스프라이트 생성
                    texture, // 원본 텍스처
                    new Rect(0f, 0f, texture.width, texture.height), // 전체 영역
                    new Vector2(0.5f, 0f), // 아래 중앙 기준점
                    100f); // 픽셀 단위

                sprite.name = path.Replace('/', '_') + "_Dialogue"; // 스프라이트 이름 설정
                _portraitCache[path] = sprite; // 초상 캐시 저장

                return sprite; // 초상 반환
            } // 순회 끝

            return null; // 초상 없음 반환
        } // 조회 끝

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

            if (_waitingForChoice) // 선택 대기 확인
            { // 입력 차단 시작
                return; // 일반 진행 차단
            } // 입력 차단 끝

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

            StoryScene scene = _queue[_sceneIndex]; // 현재 장면 조회

            if (scene.Choices.Length > 0) // 선택지 존재 확인
            { // 선택 시작
                ShowChoices(scene); // 선택지 표시

                return; // 선택 대기
            } // 선택 끝

            CompleteCurrentScene(); // 현재 장면 완료
        }

        private void ShowChoices(StoryScene scene) // 선택지 표시
        { // 표시 시작
            HideChoices(); // 이전 선택지 정리
            _waitingForChoice = true; // 선택 대기 설정
            _next.enabled = false; // 다음 표시 숨김
            _choiceRoot.gameObject.SetActive(true); // 선택지 영역 표시

            float width = Mathf.Min(300f, 960f / Mathf.Max(1, scene.Choices.Length)); // 버튼 너비 계산

            for (int i = 0; i < scene.Choices.Length; i++) // 선택지 순회
            { // 순회 시작
                int choiceIndex = i; // 콜백 번호 복사
                StoryChoice choice = scene.Choices[i]; // 선택지 조회
                UiButton button = UiFactory.CreateButton( // 선택 버튼 생성
                    _choiceRoot, // 선택지 영역
                    $"Choice{i}", // 버튼 이름
                    choice.Text, // 표시 문구
                    UiTheme.FontSmall, // 글자 크기
                    true); // 강조 버튼
                float totalWidth = width * scene.Choices.Length; // 전체 버튼 너비
                float x = -totalWidth * 0.5f + width * 0.5f + width * i; // 버튼 가로 위치
                UiFactory.Place( // 버튼 배치
                    button.Background.rectTransform, // 버튼 위치
                    new Vector2(0.5f, 0.5f), // 가운데 기준
                    new Vector2(0.5f, 0.5f), // 가운데 축
                    new Vector2(x, 0f), // 가로 위치
                    new Vector2(width - 12f, 52f)); // 버튼 크기
                button.Button.onClick.AddListener(() => SelectChoice(choiceIndex)); // 선택 처리 연결
                _choiceButtons.Add(button); // 버튼 목록 추가
            } // 순회 끝
        } // 표시 끝

        private void SelectChoice(int index) // 선택지 처리
        { // 처리 시작
            StoryScene scene = _queue[_sceneIndex]; // 현재 장면 조회

            if (!_waitingForChoice || index < 0 || index >= scene.Choices.Length) // 선택 유효성 확인
            { // 거부 시작
                return; // 선택 거부
            } // 거부 끝

            _onChoiceSelected?.Invoke(scene, scene.Choices[index]); // 선택 결과 알림
            CompleteCurrentScene(); // 현재 장면 완료
        } // 처리 끝

        private void CompleteCurrentScene() // 현재 장면 완료
        { // 완료 시작
            StoryScene scene = _queue[_sceneIndex]; // 현재 장면 조회
            _onSceneCompleted?.Invoke(scene); // 완료 알림
            HideChoices(); // 선택지 숨김

            if (_sceneIndex + 1 < _queue.Count) // 다음 장면 확인
            { // 다음 장면 시작
                ShowScene(_sceneIndex + 1); // 다음 장면 표시

                return; // 완료 처리 종료
            } // 다음 장면 끝

            Finish(); // 전체 재생 종료
        } // 완료 끝

        private void HideChoices() // 선택지 숨김
        { // 숨김 시작
            _waitingForChoice = false; // 선택 대기 해제

            foreach (UiButton button in _choiceButtons) // 버튼 순회
            { // 순회 시작
                if (button.Background != null) // 버튼 존재 확인
                { // 제거 시작
                    Destroy(button.Background.gameObject); // 버튼 제거
                } // 제거 끝
            } // 순회 끝

            _choiceButtons.Clear(); // 버튼 목록 비우기

            if (_choiceRoot != null) // 영역 존재 확인
            { // 숨김 시작
                _choiceRoot.gameObject.SetActive(false); // 선택지 영역 숨김
            } // 숨김 끝
        } // 숨김 끝

        /// <summary>남은 장면을 모두 본 것으로 남기고 닫는다.</summary>
        private void SkipAll()
        {
            if (!IsPlaying)
            {
                return;
            }

            int choiceSceneIndex = StoryPlaybackLogic.GetSkipStopIndex( // 다음 선택 장면 조회
                _queue, // 남은 장면 목록
                _sceneIndex); // 현재 장면 번호
            int completeThrough = choiceSceneIndex >= 0 // 선택 장면 존재 확인
                ? choiceSceneIndex - 1 // 선택 장면 직전까지 완료
                : _queue.Count - 1; // 모든 장면 완료

            for (int i = _sceneIndex; i <= completeThrough; i++)
            {
                if (i > _sceneIndex) // 아직 시작하지 않은 장면 확인
                { // 시작 기록 시작
                    _onSceneShown?.Invoke(_queue[i]); // 장면 시작 알림
                } // 시작 기록 끝

                _onSceneCompleted?.Invoke(_queue[i]); // 장면 완료 알림
            }

            if (choiceSceneIndex >= 0) // 선택 장면 존재 확인
            { // 선택 이동 시작
                if (choiceSceneIndex != _sceneIndex) // 다른 장면 확인
                { // 장면 시작
                    ShowScene(choiceSceneIndex); // 선택 장면 표시
                } // 장면 시작 끝

                StoryScene choiceScene = _queue[choiceSceneIndex]; // 선택 장면 조회
                int lastLine = choiceScene.Lines.Length - 1; // 마지막 대사 번호
                ShowLine(lastLine); // 마지막 대사 표시
                _elapsed = StoryLogic.GetLineSeconds(choiceScene.Lines[lastLine].Text.Length); // 전체 대사 시간 적용
                RefreshLine(); // 전체 대사 표시
                ShowChoices(choiceScene); // 선택지 표시

                return; // 선택 대기
            } // 선택 이동 끝

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
            HideChoices(); // 선택지 정리

            _queue.Clear();
            _onSceneShown = null;
            _onSceneCompleted = null; // 완료 알림 해제
            _onChoiceSelected = null; // 선택 알림 해제

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

            foreach (Sprite sprite in _portraitCache.Values) // 생성 초상 순회
            { // 순회 시작
                if (sprite != null) // 초상 존재 확인
                { // 존재 시작
                    Destroy(sprite); // 런타임 초상 정리
                } // 존재 끝
            } // 순회 끝

            _portraitCache.Clear(); // 초상 캐시 비우기
            _missingPortraitPaths.Clear(); // 누락 경로 캐시 비우기
        }
    }
}
