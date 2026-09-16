using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 보스를 함락하면 결과 화면 전에 엔딩 장면을 보여 준다 (29일차).
    /// 시간표는 <see cref="EndingLogic"/>이 정한다. 게임이 멈춰 있어도 움직이게 unscaled 시간을 쓴다.
    /// 연출이 도는 동안 <see cref="IsPlaying"/>이 켜져 결과 화면이 기다린다.
    /// </summary>
    public sealed class EndingSequence : MonoBehaviour
    {
        private const int SortOrder = 250;

        private static bool _playing;

        private Canvas _canvas;
        private Image _backdrop;
        private Text _title;
        private Text[] _lines;
        private float _elapsed;
        private bool _started;

        /// <summary>엔딩이 도는 중인지다. 결과 화면이 이 동안 뜨지 않는다.</summary>
        public static bool IsPlaying =>
            _playing;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _playing = false;
        }

        private void OnEnable()
        {
            StageMoments.BossDefeated += HandleBossDefeated;
        }

        private void OnDisable()
        {
            StageMoments.BossDefeated -= HandleBossDefeated;
        }

        private void OnDestroy()
        {
            if (_started)
            {
                _playing = false;
            }
        }

        private void HandleBossDefeated(
            Vector2 position)
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _playing = true;
            _elapsed = 0f;

            Build();
            Apply();
        }

        private void Update()
        {
            if (!_started ||
                !_playing)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;

            if (ReadSkipPressed())
            {
                _elapsed = EndingLogic.Skip(_elapsed);
            }

            Apply();

            if (EndingLogic.IsFinished(_elapsed))
            {
                _playing = false;

                if (_canvas != null)
                {
                    Destroy(_canvas.gameObject);
                }
            }
        }

        private void Apply()
        {
            if (_backdrop == null)
            {
                return;
            }

            Color dark = _backdrop.color;
            dark.a = EndingLogic.GetBackdropAlpha(_elapsed);
            _backdrop.color = dark;

            SetAlpha(_title, EndingLogic.GetTextAlpha(_elapsed, -1));

            for (int i = 0; i < _lines.Length; i++)
            {
                SetAlpha(_lines[i], EndingLogic.GetTextAlpha(_elapsed, i));
            }
        }

        private static void SetAlpha(
            Text text,
            float alpha)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }

        private static bool ReadSkipPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return (Mouse.current != null &&
                    Mouse.current.leftButton.wasPressedThisFrame) ||
                   (Keyboard.current != null &&
                    (Keyboard.current.spaceKey.wasPressedThisFrame ||
                     Keyboard.current.enterKey.wasPressedThisFrame));
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private void Build()
        {
            _canvas =
                UiFactory.CreateCanvas(
                    "EndingCanvas",
                    SortOrder,
                    transform);

            _backdrop =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Backdrop",
                    new Color(0.02f, 0.01f, 0.05f, 0f));

            _backdrop.raycastTarget = true;
            UiFactory.Stretch(_backdrop.rectTransform);

            _title =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Title",
                    EndingLogic.Title,
                    UiTheme.FontTitle + 20,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(_title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(900f, 90f));

            _lines = new Text[EndingLogic.Lines.Length];

            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i] =
                    UiFactory.CreateText(
                        _canvas.transform,
                        $"Line_{i}",
                        EndingLogic.Lines[i],
                        UiTheme.FontHeading,
                        i == _lines.Length - 1 ? UiTheme.AccentSoft : UiTheme.TextPrimary,
                        TextAnchor.MiddleCenter);

                UiFactory.Place(_lines[i].rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f - i * 60f), new Vector2(1400f, 44f));
            }

            Text hint =
                UiFactory.CreateText(
                    _canvas.transform,
                    "Hint",
                    "클릭하면 넘어갑니다",
                    UiTheme.FontSmall,
                    new Color(1f, 1f, 1f, 0.35f),
                    TextAnchor.MiddleCenter);

            UiFactory.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(400f, 24f));
        }
    }
}
