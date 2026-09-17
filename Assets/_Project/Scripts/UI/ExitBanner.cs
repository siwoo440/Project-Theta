using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 목표를 채우면 화면 위쪽에 뜨는 탈출 안내다 (33일차).
    /// "목표 달성! 1F 회수 지점에서 [F] 탈출 · 남아서 더 모으면 보상 ↑ (초과 +N · 남은 N초)"
    /// </summary>
    public sealed class ExitBanner : MonoBehaviour
    {
        private const int SortOrder = 55;

        private StageSessionController _stage;
        private StageScoreTracker _tracker;
        private FloorTransitionController _floors;
        private GameObject _root;
        private Image _background;
        private Text _text;

        public void Configure(
            StageSessionController stage,
            StageScoreTracker tracker,
            FloorTransitionController floors)
        {
            _stage = stage;
            _tracker = tracker;
            _floors = floors;

            Canvas canvas =
                UiFactory.CreateCanvas(
                    "ExitBannerCanvas",
                    SortOrder,
                    transform);

            _background =
                UiFactory.CreateImage(
                    canvas.transform,
                    "ExitBanner",
                    new Color(0.20f, 0.14f, 0.04f, 0.88f));

            UiFactory.Place(_background.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1180f, 54f));

            _text =
                UiFactory.CreateText(
                    _background.transform,
                    "Text",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Stretch(_text.rectTransform);

            _root = _background.gameObject;
            _root.SetActive(false);
        }

        private void Update()
        {
            if (_stage == null ||
                _root == null)
            {
                return;
            }

            bool ready = _stage.IsExitReady;

            if (_root.activeSelf != ready)
            {
                _root.SetActive(ready);

                // 37일차: 탈출 가능해지는 순간
                if (ready)
                {
                    Presentation.GameAudio.Play(
                        Presentation.GameSfx.ExitOpen);
                }
            }

            if (!ready)
            {
                return;
            }

            int overflow =
                StageExitLogic.GetOverflow(
                    _tracker == null ? _stage.CurrentEssence : _tracker.RecoveredEssence,
                    _stage.TargetEssence);

            _text.text =
                StageExitLogic.GetBanner(
                    GameInput.ShortLabel(GameAction.Interact),
                    _floors == null ? 0 : _floors.CurrentFloor,
                    overflow,
                    _stage.RemainingTime);

            // 남은 시간이 15초 아래면 붉게 깜빡여 알린다.
            bool hurry = _stage.RemainingTime <= 15f;
            float pulse = Mathf.Sin(Time.unscaledTime * 6f) * 0.5f + 0.5f;

            _background.color =
                hurry
                    ? Color.Lerp(new Color(0.30f, 0.06f, 0.06f, 0.90f), new Color(0.45f, 0.10f, 0.10f, 0.95f), pulse)
                    : new Color(0.20f, 0.14f, 0.04f, 0.88f);
        }
    }
}
