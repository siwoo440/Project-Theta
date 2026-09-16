using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 <b>VIP 매혹 파동</b>이다 (부록 C.3 [7]).
    ///
    /// 플레이어 무리 자리에 1.2초 동안 보라색 원(반경 5m)을 그린 뒤,
    /// 원 안에 있는 동행자에게 매혹 게이지 +40을 준다. 게이지가 가득 찬 동행자는 라이벌에게 넘어간다.
    /// 대처: 동행자를 원 밖으로 빼기, 최면 파동으로 매혹 게이지 모두 지우기.
    /// </summary>
    public sealed class CharmWaveAbility : SpecialAbility
    {
        private const float WantRange = 10f;

        private Transform _player;
        private FollowerManager _followers;
        private BossBattle _battle;

        private SpriteRenderer _circle;
        private Vector2 _center;
        private float _flashRemaining;

        public override string DisplayName =>
            "VIP 매혹 파동";

        protected override float TelegraphSeconds =>
            BossSkillValues.CharmTelegraphSeconds;

        protected override float CooldownSeconds =>
            BossBattleLogic.GetSkillCooldown(
                BossSkillValues.CharmCooldownSeconds,
                _battle == null ? BossPhase.Contest : _battle.Phase);

        public void Configure(
            Transform player,
            FollowerManager followers,
            BossBattle battle)
        {
            _player = player;
            _followers = followers;
            _battle = battle;

            SetInitialCooldown(8f);

            _circle =
                LocationProps.Blob(
                    transform,
                    "CharmCircle",
                    transform.position,
                    new Vector2(
                        BossSkillValues.CharmRadius * 2f,
                        BossSkillValues.CharmRadius * 2f * LanternLogic.VerticalRatio),
                    new Color(0.75f, 0.30f, 1.00f, 0f),
                    -51);

            _circle.transform.SetParent(null, true);
        }

        private void OnDestroy()
        {
            if (_circle != null)
            {
                Destroy(_circle.gameObject);
            }
        }

        protected override bool WantsToStart()
        {
            return _player != null &&
                   _followers != null &&
                   _followers.Count > 0 &&
                   (_battle == null || !_battle.IsDefeated) &&
                   FloorSpace.FloorAt(_player.position.y) == Body.Floor &&
                   Vector2.Distance(_player.position, transform.position) <= WantRange;
        }

        protected override void OnTelegraph()
        {
            _center = _player == null ? (Vector2)transform.position : (Vector2)_player.position;
        }

        protected override void Fire()
        {
            _flashRemaining = 0.3f;

            if (_followers == null)
            {
                return;
            }

            IReadOnlyList<FollowerController> list = _followers.Followers;

            int hit = 0;

            for (int i = list.Count - 1;
                 i >= 0;
                 i--)
            {
                FollowerController follower = list[i];

                if (follower == null)
                {
                    continue;
                }

                Vector2 position = follower.transform.position;

                if (!AreaLogic.IsInsideEllipse(
                        position.x - _center.x,
                        position.y - _center.y,
                        BossSkillValues.CharmRadius,
                        BossSkillValues.CharmRadius * LanternLogic.VerticalRatio))
                {
                    continue;
                }

                RivalCharm.Add(follower, _followers, BossSkillValues.CharmGain);
                hit++;
            }

            StageMoments.RaiseAbilityFired(
                _center,
                $"매혹 파동 ({hit}명)");
        }

        private void LateUpdate()
        {
            if (_circle == null ||
                Body == null)
            {
                return;
            }

            if (_flashRemaining > 0f)
            {
                _flashRemaining -= Time.deltaTime;
            }

            bool telegraph =
                Phase == AbilityPhase.Telegraph &&
                !Body.IsStunned;

            float alpha =
                _flashRemaining > 0f
                    ? 0.45f
                    : telegraph
                        ? 0.15f + 0.15f * Mathf.PingPong(Time.time * 4f, 1f)
                        : 0f;

            _circle.transform.position = new Vector3(_center.x, _center.y, 0f);

            Color color = _circle.color;
            color.a = alpha;
            _circle.color = color;
        }
    }
}
