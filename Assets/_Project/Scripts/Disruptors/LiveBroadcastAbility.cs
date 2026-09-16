using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 촬영팀의 <b>라이브 방송</b>이다 (부록 C.3 [4]).
    ///
    /// 촬영팀은 앞쪽 바닥에 밝은 조명(반경 3m)을 비추며 골목을 걷는다.
    /// 조명 안에서 최면하면 1초 예고 뒤 "라이브 중!" — 구역 경계도 +30, 8초 동안 촬영팀이 따라온다.
    /// 조명은 등불과 같아서 안에서는 최면 사거리가 온전하다. 위험과 기회를 함께 준다.
    /// 대처: 조명 경로 밖에서 활동, 파동으로 촬영팀을 멍하게 해 조명 끄기.
    /// </summary>
    public sealed class LiveBroadcastAbility : SpecialAbility
    {
        /// <summary>조명은 촬영팀 발밑이 아니라 바라보는 쪽 앞을 비춘다.</summary>
        private const float LightForward = 1.2f;

        private const float ChaseRepathSeconds = 0.5f;

        private static readonly Color IdleLight = new Color(1.00f, 0.95f, 0.80f, 0.30f);
        private static readonly Color TelegraphLight = new Color(1.00f, 1.00f, 1.00f, 0.55f);
        private static readonly Color LiveLight = new Color(1.00f, 0.45f, 0.45f, 0.40f);

        private Transform _player;
        private HypnosisCaster _caster;
        private LanternLight _light;

        private float _liveRemaining;
        private float _repathRemaining;

        public override string DisplayName =>
            "라이브 방송";

        public bool IsLive =>
            _liveRemaining > 0f;

        public float LiveRemaining =>
            Mathf.Max(0f, _liveRemaining);

        public void Configure(
            Transform player)
        {
            _player = player;
            _caster = player == null ? null : player.GetComponent<HypnosisCaster>();

            _light =
                LanternLight.Create(
                    transform,
                    "FilmCrewLight",
                    transform.position,
                    BeachMarketAbilityValues.LiveLightRadius,
                    IdleLight,
                    false);
        }

        private bool PlayerInLight =>
            _player != null &&
            _light != null &&
            _light.isActiveAndEnabled &&
            FloorSpace.FloorAt(_player.position.y) == Body.Floor &&
            _light.Contains(_player.position);

        protected override bool WantsToStart()
        {
            return PlayerInLight &&
                   _caster != null &&
                   _caster.CurrentTarget != null;
        }

        protected override void OnTelegraph()
        {
            Body.FaceToward(
                _player.position.x);
        }

        protected override void Fire()
        {
            Vector2 self = transform.position;

            // 예고 동안 조명 밖으로 빠져나갔으면 방송은 헛돈다.
            if (!PlayerInLight)
            {
                GameVfx.FloatText(
                    "방송 놓침",
                    self + new Vector2(0f, 2.9f),
                    UI.Framework.UiTheme.TextMuted,
                    UI.Framework.UiTheme.FontSmall);

                return;
            }

            Vector2 player = _player.position;

            if (ZoneAlert.Current != null)
            {
                ZoneAlert.Current.Add(
                    BeachMarketAbilityValues.LiveAlertRise,
                    player);
            }

            _liveRemaining = BeachMarketAbilityValues.LiveChaseSeconds;
            _repathRemaining = 0f;

            StageMoments.RaiseAbilityFired(
                player,
                "라이브 중");
        }

        private void LateUpdate()
        {
            if (_light == null ||
                Body == null)
            {
                return;
            }

            // 멍한 동안은 조명이 꺼진다. 파동이 조명을 끄는 유일한 방법이다.
            bool lit =
                !Body.IsStunned;

            if (_light.gameObject.activeSelf != lit)
            {
                _light.gameObject.SetActive(lit);
            }

            _light.transform.position =
                (Vector2)transform.position +
                new Vector2(Body.Facing * LightForward, 0f);

            _light.SetColor(
                IsLive
                    ? LiveLight
                    : Phase == AbilityPhase.Telegraph
                        ? TelegraphLight
                        : IdleLight);

            UpdateChase();
        }

        private void UpdateChase()
        {
            if (_liveRemaining <= 0f ||
                GameplayPause.IsPaused)
            {
                return;
            }

            if (!Body.CanAct)
            {
                // 멍해지면 방송이 끊긴다.
                if (Body.IsStunned)
                {
                    _liveRemaining = 0f;
                }

                return;
            }

            _liveRemaining -= Time.deltaTime;
            _repathRemaining -= Time.deltaTime;

            if (_repathRemaining > 0f ||
                _player == null)
            {
                return;
            }

            _repathRemaining = ChaseRepathSeconds;

            Body.MoveToward(
                _player.position,
                ChaseRepathSeconds + 0.2f);
        }
    }
}
