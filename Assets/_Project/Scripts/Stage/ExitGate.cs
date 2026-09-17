using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 1F 회수 지점의 탈출구다 (33일차).
    ///
    /// 목표를 채워 "탈출 가능"이 되면 회수 지점 위에 금빛 기둥과 "EXIT [F]" 글자가 켜진다.
    /// 플레이어가 그 안에 서서 상호작용 키를 누르면 <see cref="StageSessionController.RequestExit"/>로 클리어한다.
    /// </summary>
    public sealed class ExitGate : MonoBehaviour
    {
        private static readonly Color GlowColor = new Color(1.00f, 0.82f, 0.36f, 0.30f);

        private StageSessionController _stage;
        private FloorTransitionController _floors;
        private Transform _player;
        private Rect _zone;
        private SpriteRenderer _glow;
        private Text _label;
        private bool _shown;

        public void Configure(
            StageSessionController stage,
            FloorTransitionController floors,
            Transform player,
            Vector2 center,
            Vector2 size)
        {
            _stage = stage;
            _floors = floors;
            _player = player;

            transform.position = new Vector3(center.x, center.y, 0f);

            // 발밑 판정은 조금 넉넉하게 잡는다.
            _zone =
                new Rect(
                    center.x - size.x * 0.5f - 0.3f,
                    center.y - size.y * 0.5f,
                    size.x + 0.6f,
                    size.y);

            _glow =
                Locations.LocationProps.Box(
                    transform,
                    "ExitGlow",
                    center + new Vector2(0f, 0.5f),
                    new Vector2(size.x + 0.4f, size.y + 1f),
                    GlowColor,
                    -4);

            _label =
                WorldLabel.Create(
                    transform,
                    "ExitLabel",
                    new Vector2(0f, size.y * 0.5f + 1.4f),
                    30,
                    new Color(1f, 0.86f, 0.40f));

            SetShown(false);
        }

        private void Update()
        {
            if (_stage == null)
            {
                return;
            }

            bool ready = _stage.IsExitReady;

            if (ready != _shown)
            {
                SetShown(ready);

                if (ready)
                {
                    GameAudio.Play(GameSfx.UiStamp);
                }
            }

            if (!ready)
            {
                return;
            }

            // 숨쉬듯 밝아졌다 어두워진다(멈춘 동안에도).
            Color color = GlowColor;
            color.a = 0.18f + 0.16f * (Mathf.Sin(Time.unscaledTime * 3f) * 0.5f + 0.5f);
            _glow.color = color;

            _label.text = $"EXIT  [{GameInput.ShortLabel(GameAction.Interact)}]";

            bool canExit =
                StageExitLogic.CanExit(
                    _stage.State,
                    _floors == null ? 0 : _floors.CurrentFloor,
                    IsPlayerInside(),
                    GameplayPause.IsPaused);

            if (canExit &&
                GameInput.WasPressed(GameAction.Interact) &&
                _stage.RequestExit())
            {
                GameAudio.Play(GameSfx.Recovery);
            }
        }

        private bool IsPlayerInside()
        {
            return _player != null &&
                   _zone.Contains((Vector2)_player.position);
        }

        private void SetShown(
            bool shown)
        {
            _shown = shown;

            if (_glow != null)
            {
                _glow.enabled = shown;
            }

            if (_label != null)
            {
                _label.canvas.enabled = shown;
            }
        }
    }
}
