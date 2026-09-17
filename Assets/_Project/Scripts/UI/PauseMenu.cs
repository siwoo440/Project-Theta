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
    /// 스테이지 일시정지 메뉴다 (31일차). Esc로 열고 닫는다.
    ///
    ///   계속하기 · 장소 규칙(35일차) · 조작법 · 설정 · 포기하기
    ///
    /// 카드 선택 중 · 결과 화면 · 보스 엔딩 · 로딩 중에는 열지 않는다(<see cref="PauseMenuLogic.CanOpen"/>).
    /// 포기는 한 번 더 눌러야 되고, 실패로 끝나 계약 정기는 0이다. 결과 화면은 평소처럼 뜬다.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        private const int SortOrder = 260;

        private StageSessionController _stage;
        private RunUpgradeChoicePanel _cards;
        private Canvas _canvas;
        private UiOverlayParts _parts;
        private UiButton _abandon;
        private SettingsPanel _settings;
        private ControlsPanel _controls;
        private LocationGuidePanel _guide;
        private bool _pauseHeld;
        private float _abandonConfirm;

        public bool IsOpen =>
            _parts.Root != null &&
            _parts.Root.activeSelf;

        public void Configure(
            StageSessionController stage)
        {
            _stage = stage;
            _cards = FindFirstObjectByType<RunUpgradeChoicePanel>();
            _guide = FindFirstObjectByType<LocationGuidePanel>();

            Build();
        }

        private void Build()
        {
            _canvas =
                UiFactory.CreateCanvas(
                    "PauseCanvas",
                    SortOrder,
                    transform);

            _parts =
                UiOverlay.Create(
                    _canvas.transform,
                    "PauseOverlay",
                    SortOrder,
                    new Vector2(560f, 640f),
                    "일시정지",
                    Resume);

            RectTransform w = _parts.Window;
            const float x = 80f;
            const float width = 400f;

            Text place =
                UiOverlay.Label(
                    w,
                    $"{LocationContext.Current.DisplayName} · {LocationCatalog.GetObjectiveLabel(LocationContext.Current.Objective)}",
                    36f,
                    76f,
                    480f,
                    UiTheme.FontSmall,
                    UiTheme.Gold);

            place.alignment = TextAnchor.MiddleLeft;

            UiOverlay.Button(w, "Resume", "계속하기", x, 130f, width, 60f, Resume, true);
            UiOverlay.Button(w, "Guide", "장소 규칙", x, 206f, width, 60f, OpenGuide);
            UiOverlay.Button(w, "Controls", "조작법", x, 282f, width, 60f, OpenControls);
            UiOverlay.Button(w, "Settings", "설정", x, 358f, width, 60f, OpenSettings);
            _abandon = UiOverlay.Button(w, "Abandon", "포기하기  (보상 없음)", x, 462f, width, 60f, Abandon);

            _abandon.Background.color = new Color(0.36f, 0.12f, 0.16f, 1f);

            UiOverlay.Label(w, "포기하면 실패로 끝나고 계약 정기를 받지 못합니다.", 36f, 546f, 490f, UiTheme.FontSmall, UiTheme.TextMuted);

            _settings = SettingsPanel.Create(_canvas.transform, SortOrder + 10);
            _controls = ControlsPanel.Create(_canvas.transform, SortOrder + 10);
        }

        private void Update()
        {
            if (_abandonConfirm > 0f)
            {
                _abandonConfirm -= Time.unscaledDeltaTime;

                if (_abandonConfirm <= 0f)
                {
                    _abandon.SetText("포기하기  (보상 없음)");
                }
            }

            // 스테이지가 끝나면(포기 포함) 메뉴를 걷는다.
            if (IsOpen &&
                (_stage == null || !_stage.IsRunning))
            {
                Hide();

                return;
            }

            if (!ReadEscapePressed())
            {
                return;
            }

            int frame = Time.frameCount;

            if (IsOpen)
            {
                if (UiEscapeStack.TryConsume(this, frame))
                {
                    Resume();
                }

                return;
            }

            bool canOpen =
                PauseMenuLogic.CanOpen(
                    _stage != null && _stage.IsRunning,
                    _cards != null && _cards.IsOpen,
                    Boss.EndingSequence.IsPlaying,
                    LoadingScreen.IsLoading);

            if (canOpen &&
                UiEscapeStack.TryConsumeWhenEmpty(frame))
            {
                Show();
            }
        }

        private static bool ReadEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null &&
                   Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private void Show()
        {
            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);

            if (!_pauseHeld)
            {
                _pauseHeld = true;
                GameplayPause.Request();
            }

            GameAudio.Play(GameSfx.UiTick);
        }

        private void Resume()
        {
            if (!IsOpen)
            {
                return;
            }

            GameAudio.Play(GameSfx.UiTick);
            Hide();
        }

        private void Hide()
        {
            _settings.Close();
            _controls.Close();

            _parts.Root.SetActive(false);
            UiEscapeStack.Remove(this);

            _abandonConfirm = 0f;
            _abandon.SetText("포기하기  (보상 없음)");

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

        private void OpenControls()
        {
            GameAudio.Play(GameSfx.UiTick);

            LocationDefinition location = LocationContext.Current;

            _controls.Open(
                $"이번 장소 · {location.DisplayName}\n{location.Summary}\n방해 세력: {location.DisruptorPreview}");
        }

        /// <summary>35일차: 장소 규칙 카드를 일시정지 메뉴 위에 연다.</summary>
        private void OpenGuide()
        {
            if (_guide != null)
            {
                _guide.Open();
            }
        }

        private void OpenSettings()
        {
            GameAudio.Play(GameSfx.UiTick);
            _settings.Open();
        }

        /// <summary>실수로 누르지 않게 두 번 눌러야 포기한다.</summary>
        private void Abandon()
        {
            if (_abandonConfirm <= 0f)
            {
                _abandonConfirm = PauseMenuLogic.AbandonConfirmSeconds;
                _abandon.SetText("한 번 더 누르면 포기합니다");
                GameAudio.Play(GameSfx.UiTick);

                return;
            }

            GameAudio.Play(GameSfx.UiStamp);

            // 멈춤을 먼저 풀어야 결과 화면 연출이 흐른다.
            Hide();

            _stage?.Abandon();
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);

            // 열린 채로 씬을 나가면 다음 씬이 멈춘 채로 시작한다.
            ReleasePause();
        }
    }
}
