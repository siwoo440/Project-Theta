using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Impulse;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 호객꾼이다 (23일차, 부록 C.3 [4]). 쟁탈 역할의 "붙잡기" 형태다.
    ///
    /// 자기 노점 앞 3m를 지나는 동행자를 불러 세운다. 세워진 동행자는 따라오지 않는다.
    /// 6초 안에 플레이어가 곁에 1초 붙어 있으면 되찾고, 못 하면 그 동행자는 무리를 떠난다.
    /// 대처: 노점 앞을 빠르게 지나기, 노점 반대편 줄로 걷기, 붙잡힌 동행자에게 돌아가기, 파동.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class StallToutRole : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;

        private FollowerController _held;
        private float _holdRemaining;
        private float _rescueProgress;
        private float _cooldownRemaining;
        private Text _heldLabel;

        /// <summary>전단지 알바 모드다 (24일차). 짧게 세우기만 하고 동행자를 빼앗지 않는다.</summary>
        private bool _flyer;

        public bool IsHolding =>
            _held != null;

        public float HoldRemaining =>
            _held == null
                ? 0f
                : _holdRemaining;

        public void Configure(
            FollowerManager followers,
            Transform player,
            bool flyer = false)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;
            _flyer = flyer;
        }

        private float PullRadius =>
            _flyer ? StallHoldLogic.FlyerPullRadius : StallHoldLogic.PullRadius;

        private float HoldSeconds =>
            _flyer ? StallHoldLogic.FlyerHoldSeconds : StallHoldLogic.HoldSeconds;

        private float CooldownSeconds =>
            _flyer ? StallHoldLogic.FlyerCooldownSeconds : StallHoldLogic.CooldownSeconds;

        private void OnDisable()
        {
            // 판이 끝나거나 씬이 내려갈 때 동행자를 붙잡힌 채로 두지 않는다.
            if (_held != null)
            {
                _held.SetExternalControl(false);
                _held = null;
            }

            DestroyLabel();
        }

        private void Update()
        {
            if (_body == null ||
                _followers == null ||
                GameplayPause.IsPaused)
            {
                return;
            }

            float deltaTime =
                Time.deltaTime;

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            if (_held != null)
            {
                UpdateHold(deltaTime);

                return;
            }

            if (!_body.CanAct)
            {
                return;
            }

            FollowerController candidate =
                FindCandidate();

            if (candidate != null)
            {
                Hold(candidate);
            }
        }

        private FollowerController FindCandidate()
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            Vector2 self = transform.position;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                FollowerController follower = list[i];

                if (follower == null ||
                    !follower.isActiveAndEnabled ||
                    !IsCalm(follower))
                {
                    continue;
                }

                Vector2 position = follower.transform.position;

                if (FloorSpace.FloorAt(position.y) != _body.Floor)
                {
                    continue;
                }

                if (_cooldownRemaining <= 0f &&
                    Vector2.Distance(self, position) <= PullRadius)
                {
                    return follower;
                }
            }

            return null;
        }

        /// <summary>폭주 흐름에 들어간 동행자는 붙잡지 않는다. 폭주 쪽이 이미 몸을 쥐고 있다.</summary>
        private static bool IsCalm(
            FollowerController follower)
        {
            ImpulseMeter impulse =
                follower.GetComponent<ImpulseMeter>();

            if (impulse == null)
            {
                return true;
            }

            switch (impulse.State)
            {
                case ImpulseState.Preparing:
                case ImpulseState.Rampaging:
                case ImpulseState.Capturing:
                case ImpulseState.Recovering:
                    return false;

                default:
                    return true;
            }
        }

        private void Hold(
            FollowerController follower)
        {
            _held = follower;
            _holdRemaining = HoldSeconds;
            _rescueProgress = 0f;

            follower.SetExternalControl(true);

            _body.FaceToward(
                follower.transform.position.x);

            _heldLabel =
                WorldLabel.Create(
                    follower.transform,
                    "StallHoldLabel",
                    new Vector2(0f, LabelHeight),
                    22,
                    new Color(1.00f, 0.70f, 0.35f));

            GameVfx.FloatText(
                _flyer ? "헬스장 3개월 반값이에요~" : "시식하고 가세요~",
                (Vector2)transform.position + new Vector2(0f, 2.4f),
                new Color(1.00f, 0.80f, 0.45f),
                UiTheme.FontSmall);
        }

        private void UpdateHold(
            float deltaTime)
        {
            if (_held == null ||
                !_held.isActiveAndEnabled ||
                !IsStillFollowing(_held) ||
                !IsCalm(_held))
            {
                // 회수 · 폭주 · 다른 이유로 무리를 떠났다. 몸은 건드리지 않고 놓는다.
                _held = null;
                DestroyLabel();
                _cooldownRemaining = CooldownSeconds;

                return;
            }

            // 파동에 맞아 멍해지거나, 동행자가 다른 층으로 옮겨지면 풀려난다.
            if (_body.IsStunned ||
                FloorSpace.FloorAt(_held.transform.position.y) != _body.Floor)
            {
                Release(true);

                return;
            }

            // 판이 멈춘 동안(결과 화면 등)은 시간을 흘리지 않는다.
            if (!_body.CanAct)
            {
                return;
            }

            _holdRemaining -= deltaTime;

            float playerDistance =
                _player == null
                    ? float.MaxValue
                    : Vector2.Distance(
                        _player.position,
                        _held.transform.position);

            _rescueProgress =
                StallHoldLogic.AdvanceRescue(
                    _rescueProgress,
                    playerDistance,
                    deltaTime);

            if (StallHoldLogic.IsRescued(_rescueProgress))
            {
                Release(true);

                return;
            }

            if (_holdRemaining <= 0f)
            {
                // 전단지 알바는 시간이 지나면 그냥 놓아준다.
                Release(_flyer);

                return;
            }

            if (_heldLabel != null)
            {
                _heldLabel.text =
                    _rescueProgress > 0f
                        ? "되찾는 중…"
                        : _flyer
                            ? "전단지 받는 중…"
                            : $"호객 중 {Mathf.CeilToInt(_holdRemaining)}";
            }
        }

        private bool IsStillFollowing(
            FollowerController follower)
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                if (list[i] == follower)
                {
                    return true;
                }
            }

            return false;
        }

        private void Release(
            bool rescued)
        {
            FollowerController follower = _held;
            Vector2 position = follower.transform.position;

            _held = null;
            _cooldownRemaining = CooldownSeconds;

            follower.SetExternalControl(false);

            DestroyLabel();

            if (rescued)
            {
                GameVfx.FloatText(
                    _flyer ? "다시 출발" : "되찾았다!",
                    position + new Vector2(0f, 1.8f),
                    UiTheme.Positive,
                    UiTheme.FontBody);

                return;
            }

            _followers.RequestRelease(
                follower);

            StageMoments.RaiseFollowerStolen(
                position);

            GameVfx.FloatText(
                "노점에 붙잡혔다!",
                position + new Vector2(0f, 1.8f),
                UiTheme.Danger,
                UiTheme.FontBody);
        }

        private void DestroyLabel()
        {
            if (_heldLabel == null)
            {
                return;
            }

            // 글자는 월드 캔버스의 자식이다. 캔버스째 지운다.
            Destroy(
                _heldLabel.canvas != null
                    ? _heldLabel.canvas.gameObject
                    : _heldLabel.gameObject);

            _heldLabel = null;
        }
    }
}
