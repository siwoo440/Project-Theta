using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 긴급 회의에 불려 간 동행자다 (25일차).
    /// 12초 동안 회의실로 걸어가 머물고, 끝나면 다시 따라온다. 그동안 플레이어가 회의실에 들어가면 경계도 +40 (한 번).
    /// </summary>
    [RequireComponent(typeof(FollowerController))]
    public sealed class MeetingSummons : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private FollowerController _follower;
        private Rigidbody2D _body;
        private Transform _player;
        private Text _label;

        private float _remaining;
        private float _roomX;
        private bool _intruded;

        public bool IsActive =>
            _remaining > 0f;

        public static bool IsInMeeting(
            Component follower)
        {
            if (follower == null)
            {
                return false;
            }

            MeetingSummons summons = follower.GetComponent<MeetingSummons>();

            return summons != null &&
                   summons.IsActive;
        }

        public static void Apply(
            FollowerController follower,
            Transform player,
            float roomX)
        {
            if (follower == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            MeetingSummons summons = follower.GetComponent<MeetingSummons>();

            if (summons == null)
            {
                summons = follower.gameObject.AddComponent<MeetingSummons>();
            }

            summons.Begin(player, roomX);
        }

        private void Awake()
        {
            _follower = GetComponent<FollowerController>();
            _body = GetComponent<Rigidbody2D>();
        }

        private void Begin(
            Transform player,
            float roomX)
        {
            _player = player;
            _roomX = roomX;
            _remaining = MeetingLogic.MeetingSeconds;
            _intruded = false;

            _follower.SetExternalControl(true);

            if (_label == null)
            {
                _label =
                    WorldLabel.Create(
                        transform,
                        "MeetingLabel",
                        new Vector2(0f, LabelHeight),
                        20,
                        new Color(0.70f, 0.80f, 1.00f));
            }
        }

        private void Update()
        {
            if (!IsActive ||
                GameplayPause.IsPaused)
            {
                return;
            }

            _remaining -= Time.deltaTime;

            if (_label != null)
            {
                _label.text = $"회의 중 {Mathf.CeilToInt(_remaining)}";
            }

            CheckIntrusion();

            if (_remaining <= 0f)
            {
                End();
            }
        }

        private void FixedUpdate()
        {
            if (!IsActive ||
                _body == null)
            {
                return;
            }

            Vector2 position = _body.position;

            _body.position =
                new Vector2(
                    Mathf.MoveTowards(
                        position.x,
                        _roomX,
                        MeetingLogic.WalkSpeed * Time.fixedDeltaTime),
                    position.y);
        }

        private void CheckIntrusion()
        {
            if (_intruded ||
                _player == null ||
                ZoneAlert.Current == null)
            {
                return;
            }

            int floor = FloorSpace.FloorAt(transform.position.y);

            if (FloorSpace.FloorAt(_player.position.y) != floor ||
                Mathf.Abs(_player.position.x - _roomX) > MeetingLogic.RoomHalfWidth)
            {
                return;
            }

            _intruded = true;

            ZoneAlert.Current.Add(
                MeetingLogic.IntrusionAlertRise,
                _player.position);

            GameVfx.FloatText(
                $"회의 중 난입! 경계도 +{MeetingLogic.IntrusionAlertRise:0}",
                (Vector2)_player.position + new Vector2(0f, 2.2f),
                UiTheme.Danger,
                UiTheme.FontBody);
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
            if (IsActive)
            {
                End();
            }
        }
    }
}
