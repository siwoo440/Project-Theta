using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Presentation;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 쇼핑몰 보안 무전이다 (25일차, 부록 C.3 [5]).
    ///
    /// 보안요원이나 CCTV가 발견하면 같은 층 보안요원 전원이 발견 지점으로 달려간다.
    /// 같은 층 요원 한 명이라도 멍해지면 그 층 무전이 5초 끊긴다.
    /// CCTV는 무전을 보내기만 하고 달려가지 않는다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class RadioLink : MonoBehaviour
    {
        private static readonly List<RadioLink> Active =
            new List<RadioLink>();

        private static readonly Dictionary<int, float> CutUntil =
            new Dictionary<int, float>();

        private static float _clock;

        private DisruptorBase _body;
        private WatcherRole _watcher;
        private bool _responder;
        private float _respondRemaining;

        /// <summary>무전을 받고 달려가는 중인지다. 머리 위 표시에 쓴다.</summary>
        public bool IsResponding =>
            _respondRemaining > 0f;

        public static bool IsCut(
            int floor)
        {
            return CutUntil.TryGetValue(floor, out float until) &&
                   until > _clock;
        }

        public void Configure(
            bool responder)
        {
            _body = GetComponent<DisruptorBase>();
            _watcher = GetComponent<WatcherRole>();
            _responder = responder;

            if (_watcher != null)
            {
                _watcher.Spotted += HandleSpotted;
            }
        }

        private void OnDestroy()
        {
            if (_watcher != null)
            {
                _watcher.Spotted -= HandleSpotted;
            }
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                _body == null)
            {
                return;
            }

            // 시계는 한 프레임에 한 번만 흐르게 첫 요원이 맡는다.
            if (Active.Count > 0 &&
                Active[0] == this)
            {
                _clock += Time.deltaTime;
            }

            if (_respondRemaining > 0f)
            {
                _respondRemaining -= Time.deltaTime;
            }

            // 요원이 멍해지면 그 층 무전이 끊긴다.
            if (_responder &&
                _body.IsStunned &&
                !IsCut(_body.Floor))
            {
                CutUntil[_body.Floor] = _clock + RadioLogic.CutSeconds;

                GameVfx.FloatText(
                    "무전 끊김",
                    (Vector2)transform.position + new Vector2(0f, 2.8f),
                    new Color(0.65f, 0.80f, 1.00f),
                    UI.Framework.UiTheme.FontSmall);
            }
        }

        private void HandleSpotted(
            Vector2 seenAt)
        {
            if (_body == null)
            {
                return;
            }

            float cut =
                CutUntil.TryGetValue(_body.Floor, out float until)
                    ? until - _clock
                    : 0f;

            int relayed = 0;

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                RadioLink other = Active[i];

                if (other == null ||
                    other == this ||
                    !other._responder ||
                    other._body == null ||
                    !other._body.CanAct ||
                    !RadioLogic.CanRelay(_body.Floor, other._body.Floor, cut))
                {
                    continue;
                }

                other._respondRemaining = RadioLogic.RespondSeconds;

                other._body.MoveToward(
                    seenAt,
                    RadioLogic.RespondSeconds);

                relayed++;
            }

            if (relayed > 0)
            {
                GameVfx.FloatText(
                    $"무전: 요원 {relayed}명 출동",
                    (Vector2)transform.position + new Vector2(0f, 2.8f),
                    new Color(1.00f, 0.60f, 0.35f),
                    UI.Framework.UiTheme.FontSmall);
            }
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active.Clear();
            CutUntil.Clear();
            _clock = 0f;
        }

        /// <summary>새 구역을 지을 때 지난 구역의 무전 끊김을 지운다.</summary>
        public static void ResetState()
        {
            CutUntil.Clear();
            _clock = 0f;
        }
    }
}
