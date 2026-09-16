using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 <b>VIP 전용 구역</b>이다 (27일차, 부록 C.3 [7]).
    ///
    /// 1.5초 동안 플레이어 자리 양옆에 보라색 로프를 긋고, 15초 동안 그 구역(가로 5m)을 라이벌 영역으로 만든다.
    /// 구역 안 NPC는 플레이어 최면이 절반 속도다. DJ가 멍한 순간에 만들어지면 절반 시간만 간다.
    /// 대처: 구역 밖에서 공략, DJ에게 파동.
    /// </summary>
    public sealed class VipZoneAbility : SpecialAbility
    {
        private static VipZoneAbility _active;

        private readonly List<DisruptorBase> _buffer =
            new List<DisruptorBase>();

        private Transform _player;
        private BossBattle _battle;

        private SpriteRenderer _area;
        private SpriteRenderer _ropeLeft;
        private SpriteRenderer _ropeRight;

        private float _centerX;
        private float _remaining;

        public override string DisplayName =>
            "VIP 전용 구역";

        protected override float TelegraphSeconds =>
            LateBossSkillValues.ZoneTelegraphSeconds;

        protected override float CooldownSeconds =>
            BossBattleLogic.GetSkillCooldown(
                LateBossSkillValues.ZoneCooldownSeconds,
                _battle == null ? BossPhase.Contest : _battle.Phase);

        public bool IsActive =>
            _remaining > 0f;

        public float Remaining =>
            Mathf.Max(0f, _remaining);

        public static VipZoneAbility ActiveZone =>
            _active != null && _active.IsActive
                ? _active
                : null;

        /// <summary>그 자리 NPC의 최면 배율이다. 구역이 없거나 밖이면 1이다.</summary>
        public static float GetHypnosisMultiplier(
            Vector2 worldPosition)
        {
            VipZoneAbility zone = ActiveZone;

            if (zone == null)
            {
                return 1f;
            }

            return LateBossSkillValues.IsInZone(
                       worldPosition.x,
                       zone._centerX,
                       FloorSpace.FloorAt(worldPosition.y),
                       zone.Body.Floor)
                ? LateBossSkillValues.ZoneHypnosisMultiplier
                : 1f;
        }

        public void Configure(
            Transform player,
            BossBattle battle)
        {
            _player = player;
            _battle = battle;
            _active = this;

            SetInitialCooldown(35f);

            float midY = (FloorSpace.WalkMinY + FloorSpace.WalkMaxY) * 0.5f;
            float height = FloorSpace.WalkMaxY - FloorSpace.WalkMinY;

            _area = MakeBox("VipZoneArea", new Vector2(LateBossSkillValues.ZoneHalfWidth * 2f, height), new Color(0.6f, 0.2f, 0.9f, 0f), -56, midY);
            _ropeLeft = MakeBox("VipRopeLeft", new Vector2(0.12f, height), new Color(0.8f, 0.3f, 1f, 0f), -43, midY);
            _ropeRight = MakeBox("VipRopeRight", new Vector2(0.12f, height), new Color(0.8f, 0.3f, 1f, 0f), -43, midY);
        }

        private SpriteRenderer MakeBox(
            string name,
            Vector2 size,
            Color color,
            int sortingOrder,
            float localY)
        {
            SpriteRenderer box =
                LocationProps.Box(
                    transform,
                    name,
                    FloorSpace.ToWorld(Body.Floor, new Vector2(0f, localY)),
                    size,
                    color,
                    sortingOrder);

            box.transform.SetParent(null, true);

            return box;
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }

            DestroyBox(_area);
            DestroyBox(_ropeLeft);
            DestroyBox(_ropeRight);
        }

        private static void DestroyBox(
            SpriteRenderer box)
        {
            if (box != null)
            {
                Destroy(box.gameObject);
            }
        }

        protected override bool WantsToStart()
        {
            return !IsActive &&
                   _player != null &&
                   (_battle == null || !_battle.IsDefeated) &&
                   FloorSpace.FloorAt(_player.position.y) == Body.Floor;
        }

        protected override void OnTelegraph()
        {
            _centerX =
                Mathf.Clamp(
                    _player == null ? transform.position.x : _player.position.x,
                    FloorSpace.WalkMinX + 3f,
                    FloorSpace.WalkMaxX - 4f);
        }

        protected override void Fire()
        {
            _remaining = LateBossSkillValues.GetZoneSeconds(IsDjStunned());

            StageMoments.RaiseAbilityFired(
                FloorSpace.ToWorld(Body.Floor, new Vector2(_centerX, 0f)),
                _remaining < LateBossSkillValues.ZoneSeconds
                    ? "VIP 구역 (DJ 멍함 · 절반)"
                    : DisplayName);
        }

        private bool IsDjStunned()
        {
            DisruptorBase.CopyActive(_buffer);

            for (int i = 0;
                 i < _buffer.Count;
                 i++)
            {
                DisruptorBase body = _buffer[i];

                if (body != null &&
                    body.Profile != null &&
                    body.Profile.Kind == DisruptorKind.Dj &&
                    body.IsStunned)
                {
                    return true;
                }
            }

            return false;
        }

        private void LateUpdate()
        {
            if (_area == null ||
                Body == null)
            {
                return;
            }

            if (IsActive &&
                !GameplayPause.IsPaused)
            {
                _remaining -= Time.deltaTime;

                if (_battle != null &&
                    _battle.IsDefeated)
                {
                    _remaining = 0f;
                }
            }

            bool telegraph =
                Phase == AbilityPhase.Telegraph &&
                !Body.IsStunned;

            float ropeAlpha =
                IsActive
                    ? 0.9f
                    : telegraph
                        ? 0.3f + 0.4f * Mathf.PingPong(Time.time * 5f, 1f)
                        : 0f;

            float areaAlpha = IsActive ? 0.16f : 0f;

            Place(_area, _centerX, areaAlpha);
            Place(_ropeLeft, _centerX - LateBossSkillValues.ZoneHalfWidth, ropeAlpha);
            Place(_ropeRight, _centerX + LateBossSkillValues.ZoneHalfWidth, ropeAlpha);
        }

        private static void Place(
            SpriteRenderer box,
            float x,
            float alpha)
        {
            Vector3 position = box.transform.position;

            position.x = x;
            box.transform.position = position;

            Color color = box.color;

            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                box.color = color;
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _active = null;
        }
    }
}
