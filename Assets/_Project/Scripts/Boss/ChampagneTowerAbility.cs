using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 <b>샴페인 타워</b>다 (27일차, 부록 C.3 [7]).
    ///
    /// 1.5초(바텐더 벨) 예고 뒤 바에 샴페인 타워를 세운다. 10초 동안 그 층 중립 손님이 바 쪽으로 모이고,
    /// 라이벌의 손님 확보 속도가 두 배가 된다.
    /// 대처: 타워 근처 손님을 먼저 확보, "[바텐더]"를 최면하면 타워가 즉시 무너진다.
    /// </summary>
    public sealed class ChampagneTowerAbility : SpecialAbility
    {
        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private BossBattle _battle;
        private SpriteRenderer _tower;
        private float _remaining;

        public override string DisplayName =>
            "샴페인 타워";

        protected override float TelegraphSeconds =>
            LateBossSkillValues.TowerTelegraphSeconds;

        protected override float CooldownSeconds =>
            BossBattleLogic.GetSkillCooldown(
                LateBossSkillValues.TowerCooldownSeconds,
                _battle == null ? BossPhase.Contest : _battle.Phase);

        public bool IsUp =>
            _remaining > 0f;

        public void Configure(
            BossBattle battle)
        {
            _battle = battle;

            SetInitialCooldown(18f);

            _tower =
                LocationProps.Box(
                    transform,
                    "ChampagneTower",
                    FloorSpace.ToWorld(Body.Floor, new Vector2(ClubLayout.BarX, FloorSpace.WalkMaxY + 1f)),
                    new Vector2(0.9f, 1.8f),
                    new Color(1.00f, 0.90f, 0.55f, 0.9f),
                    -44);

            _tower.transform.SetParent(null, true);
            _tower.enabled = false;
        }

        private void OnDestroy()
        {
            if (_tower != null)
            {
                Destroy(_tower.gameObject);
            }
        }

        protected override bool WantsToStart()
        {
            return !IsUp &&
                   (_battle == null || !_battle.IsDefeated);
        }

        protected override void Fire()
        {
            _remaining = LateBossSkillValues.TowerSeconds;

            _tower.enabled = true;

            StageMoments.RaiseAbilityFired(
                _tower.transform.position,
                DisplayName);
        }

        private void LateUpdate()
        {
            if (!IsUp ||
                GameplayPause.IsPaused)
            {
                return;
            }

            _remaining -= Time.deltaTime;

            if (IsBartenderTaken() ||
                (_battle != null && _battle.IsDefeated))
            {
                Collapse("바텐더를 빼앗아 샴페인 타워가 무너졌다!");

                return;
            }

            if (_remaining <= 0f)
            {
                Collapse(null);
            }
        }

        private void FixedUpdate()
        {
            if (!IsUp)
            {
                return;
            }

            if (_battle != null)
            {
                _battle.RivalClaimRate = LateBossSkillValues.GetClaimRate(true);
            }

            HypnosisTarget.CopyActive(_targetBuffer);

            float barX = ClubLayout.BarX;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner != NpcOwner.Neutral)
                {
                    continue;
                }

                Rigidbody2D body = target.GetComponent<Rigidbody2D>();

                if (body == null ||
                    FloorSpace.FloorAt(body.position.y) != Body.Floor ||
                    Mathf.Abs(body.position.x - barX) > LateBossSkillValues.TowerPullRange)
                {
                    continue;
                }

                body.position =
                    new Vector2(
                        Mathf.MoveTowards(
                            body.position.x,
                            barX - 1f,
                            LateBossSkillValues.TowerPullSpeed * Time.fixedDeltaTime),
                        body.position.y);
            }
        }

        private bool IsBartenderTaken()
        {
            HypnosisTarget.CopyActive(_targetBuffer);

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null ||
                    target.Owner != NpcOwner.Player)
                {
                    continue;
                }

                NpcRoleMark mark = NpcRoleMark.Get(target);

                if (mark != null &&
                    mark.Role == NpcRole.Bartender &&
                    FloorSpace.FloorAt(target.transform.position.y) == Body.Floor)
                {
                    return true;
                }
            }

            return false;
        }

        private void Collapse(
            string message)
        {
            _remaining = 0f;
            _tower.enabled = false;

            if (_battle != null)
            {
                _battle.RivalClaimRate = 1f;
            }

            if (!string.IsNullOrEmpty(message))
            {
                GameVfx.Shards(
                    _tower.transform.position,
                    new Color(1.00f, 0.90f, 0.55f),
                    12);

                GameVfx.FloatText(
                    message,
                    (Vector2)_tower.transform.position + new Vector2(0f, 1.5f),
                    UiTheme.Positive,
                    UiTheme.FontBody);
            }
        }
    }
}
