using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 오피스 엘리베이터에 타지 못하고 층에 남은 동행자다 (27일차).
    /// 20초 동안 그 자리에서 기다리고, 플레이어가 그 층으로 돌아오면 다시 따라온다. 시간이 지나면 떠난다.
    /// </summary>
    [RequireComponent(typeof(FollowerController))]
    public sealed class ElevatorWait : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private FollowerController _follower;
        private FollowerManager _manager;
        private Transform _player;
        private Text _label;

        private int _floor;
        private float _remaining;

        public bool IsWaiting =>
            _remaining > 0f;

        public static bool IsWaitingFollower(
            Component follower)
        {
            if (follower == null)
            {
                return false;
            }

            ElevatorWait wait = follower.GetComponent<ElevatorWait>();

            return wait != null &&
                   wait.IsWaiting;
        }

        public static void Begin(
            FollowerController follower,
            FollowerManager manager,
            Transform player)
        {
            if (follower == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            ElevatorWait wait = follower.GetComponent<ElevatorWait>();

            if (wait == null)
            {
                wait = follower.gameObject.AddComponent<ElevatorWait>();
            }

            wait._manager = manager;
            wait._player = player;
            wait._floor = FloorSpace.FloorAt(follower.transform.position.y);
            wait._remaining = ElevatorLogic.WaitSeconds;

            follower.SetExternalControl(true);

            if (wait._label == null)
            {
                wait._label =
                    WorldLabel.Create(
                        follower.transform,
                        "ElevatorLabel",
                        new Vector2(0f, LabelHeight),
                        20,
                        new Color(0.70f, 0.85f, 1.00f));
            }
        }

        private void Awake()
        {
            _follower = GetComponent<FollowerController>();
        }

        private void Update()
        {
            if (!IsWaiting ||
                GameplayPause.IsPaused)
            {
                return;
            }

            _remaining -= Time.deltaTime;

            if (_player != null &&
                FloorSpace.FloorAt(_player.position.y) == _floor)
            {
                End();

                return;
            }

            if (_remaining <= 0f)
            {
                End();

                if (_manager != null)
                {
                    _manager.RequestRelease(_follower);
                }

                GameVfx.FloatText(
                    "기다리다 떠났다",
                    (Vector2)transform.position + new Vector2(0f, 1.9f),
                    UiTheme.Danger,
                    UiTheme.FontSmall);

                return;
            }

            if (_label != null)
            {
                _label.text = $"엘리베이터 대기 {Mathf.CeilToInt(_remaining)}";
            }
        }

        private void End()
        {
            _remaining = 0f;

            if (_follower != null)
            {
                _follower.SetExternalControl(false);
            }

            if (_label != null)
            {
                _label.text = string.Empty;
            }
        }

        private void OnDisable()
        {
            if (IsWaiting)
            {
                End();
            }
        }
    }
}
