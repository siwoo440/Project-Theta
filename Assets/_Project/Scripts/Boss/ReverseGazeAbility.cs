using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 <b>역최면 시선</b>이다 (부록 C.3 [7]).
    ///
    /// 1.5초 동안 플레이어가 서 있던 줄에 붉은 선을 긋고, 그 줄을 따라 1.2초 동안 빔을 쏜다.
    /// 빔에 닿아 있는 동안 정신력이 초당 20 줄어든다.
    /// 대처: 줄을 바꾸거나 대시로 선 밖으로, 조명이 번쩍이는 순간은 무효.
    /// 분신 댄서(27일차)도 같은 기술을 피해 절반으로 쓴다.
    /// </summary>
    public sealed class ReverseGazeAbility : SpecialAbility
    {
        private Transform _player;
        private PlayerMind _mind;
        private BossBattle _battle;
        private float _damageScale = 1f;

        private SpriteRenderer _line;
        private float _laneY;
        private int _direction = 1;
        private float _beamRemaining;

        public override string DisplayName =>
            "역최면 시선";

        protected override float TelegraphSeconds =>
            BossSkillValues.GazeTelegraphSeconds;

        protected override float CooldownSeconds =>
            BossBattleLogic.GetSkillCooldown(
                BossSkillValues.GazeCooldownSeconds,
                _battle == null ? BossPhase.Contest : _battle.Phase);

        public bool IsBeaming =>
            _beamRemaining > 0f;

        public void Configure(
            Transform player,
            BossBattle battle,
            float damageScale)
        {
            _player = player;
            _mind = player == null ? null : player.GetComponent<PlayerMind>();
            _battle = battle;
            _damageScale = damageScale;

            SetInitialCooldown(4f);

            _line =
                LocationProps.Box(
                    transform,
                    "GazeLine",
                    transform.position,
                    Vector2.one,
                    new Color(1f, 0.2f, 0.3f, 0f),
                    14000);

            // 캐릭터 크기 배율과 상관없이 월드 크기로 그리기 위해 부모에서 떼어 둔다.
            _line.transform.SetParent(null, true);
        }

        private void OnDestroy()
        {
            if (_line != null)
            {
                Destroy(_line.gameObject);
            }
        }

        protected override bool WantsToStart()
        {
            if (_player == null ||
                (_battle != null && _battle.IsDefeated))
            {
                return false;
            }

            return FloorSpace.FloorAt(_player.position.y) == Body.Floor &&
                   Mathf.Abs(_player.position.x - transform.position.x) <= BossSkillValues.GazeRange;
        }

        protected override void OnTelegraph()
        {
            if (_player == null)
            {
                return;
            }

            _laneY = _player.position.y;

            _direction =
                _player.position.x >= transform.position.x
                    ? 1
                    : -1;

            Body.FaceToward(_player.position.x);
        }

        protected override void Fire()
        {
            _beamRemaining = BossSkillValues.GazeBeamSeconds;
        }

        private void LateUpdate()
        {
            if (_line == null ||
                Body == null)
            {
                return;
            }

            bool telegraph =
                Phase == AbilityPhase.Telegraph &&
                !Body.IsStunned;

            if (_beamRemaining > 0f &&
                !GameplayPause.IsPaused)
            {
                _beamRemaining -= Time.deltaTime;

                if (Body.IsStunned)
                {
                    _beamRemaining = 0f;
                }
                else
                {
                    ApplyBeam();
                }
            }

            float alpha =
                IsBeaming
                    ? (ClubBeat.FlashActive ? 0.2f : 0.75f)
                    : telegraph
                        ? 0.25f + 0.2f * Mathf.PingPong(Time.time * 6f, 1f)
                        : 0f;

            float thickness =
                IsBeaming
                    ? BossSkillValues.GazeLaneHalfHeight * 2f
                    : 0.08f;

            float startX = transform.position.x;
            float endX = startX + _direction * BossSkillValues.GazeRange;

            _line.transform.position =
                new Vector3((startX + endX) * 0.5f, _laneY, 0f);

            _line.transform.localScale =
                new Vector3(Mathf.Abs(endX - startX), thickness, 1f);

            Color color = _line.color;
            color.a = alpha;
            _line.color = color;
        }

        private void ApplyBeam()
        {
            if (_player == null ||
                _mind == null ||
                FloorSpace.FloorAt(_player.position.y) != Body.Floor)
            {
                return;
            }

            if (!BossSkillValues.GazeHits(
                    _laneY,
                    _player.position.y,
                    _direction,
                    transform.position.x,
                    _player.position.x,
                    ClubBeat.FlashActive))
            {
                return;
            }

            _mind.Damage(
                BossSkillValues.GazeDamagePerSecond *
                _damageScale *
                Time.deltaTime);
        }
    }
}
