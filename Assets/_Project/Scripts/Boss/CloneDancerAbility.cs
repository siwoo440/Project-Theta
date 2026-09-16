using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 <b>분신 댄서</b>다 (27일차, 부록 C.3 [7]).
    ///
    /// 1초 동안 조명이 어두워진 뒤 본체 양옆에 분신 2명이 나타난다 (15초).
    /// 분신은 이름표가 본체와 같고, 역최면 시선을 피해 절반으로 쏜다. <b>그림자가 있는 쪽이 본체</b>다.
    /// 분신은 파동 한 번에 사라진다.
    /// </summary>
    public sealed class CloneDancerAbility : SpecialAbility
    {
        private Transform _player;
        private BossBattle _battle;
        private SpriteRenderer _dim;

        public override string DisplayName =>
            "분신 댄서";

        protected override float TelegraphSeconds =>
            LateBossSkillValues.CloneTelegraphSeconds;

        protected override float CooldownSeconds =>
            BossBattleLogic.GetSkillCooldown(
                LateBossSkillValues.CloneCooldownSeconds,
                _battle == null ? BossPhase.Contest : _battle.Phase);

        public void Configure(
            Transform player,
            BossBattle battle)
        {
            _player = player;
            _battle = battle;

            SetInitialCooldown(25f);

            // 예고 동안 그 층을 살짝 어둡게 덮는다.
            _dim =
                LocationProps.Box(
                    transform,
                    "CloneDim",
                    FloorSpace.ToWorld(Body.Floor, new Vector2(0f, (FloorSpace.WalkMinY + FloorSpace.WalkMaxY) * 0.5f)),
                    new Vector2(40f, FloorSpace.FloorHeight),
                    new Color(0f, 0f, 0.05f, 0f),
                    18000);

            _dim.transform.SetParent(null, true);
        }

        private void OnDestroy()
        {
            if (_dim != null)
            {
                Destroy(_dim.gameObject);
            }
        }

        protected override bool WantsToStart()
        {
            return _player != null &&
                   (_battle == null || !_battle.IsDefeated) &&
                   CloneDancer.ActiveCount == 0 &&
                   FloorSpace.FloorAt(_player.position.y) == Body.Floor;
        }

        protected override void Fire()
        {
            for (int i = 0;
                 i < LateBossSkillValues.CloneCount;
                 i++)
            {
                float side = i % 2 == 0 ? -2.5f : 2.5f;

                CloneDancer.Spawn(
                    _battle,
                    _player,
                    Body.Floor,
                    transform.position.x + side);
            }

            StageMoments.RaiseAbilityFired(
                transform.position,
                DisplayName);
        }

        private void LateUpdate()
        {
            if (_dim == null ||
                Body == null)
            {
                return;
            }

            float alpha =
                Phase == AbilityPhase.Telegraph &&
                !Body.IsStunned &&
                !GameplayPause.IsPaused
                    ? 0.5f
                    : 0f;

            Color color = _dim.color;

            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                _dim.color = color;
            }
        }
    }
}
