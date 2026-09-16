using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 관장의 <b>단체 PT 호출</b>이다 (부록 C.3 [3]).
    ///
    /// 1.5초 예고 뒤 "단체 PT 시작!" — 10초 동안
    ///   동행자 충동 상승 +60% (요가실 안에서는 충동이 오르지 않음)
    ///   운동 구역 NPC 최면 속도 +30%
    /// 위험과 기회를 함께 주는 능력이다. 요가실로 피신하거나, 이 시간에 운동 중인 NPC를 빠르게 최면한다.
    /// </summary>
    public sealed class GroupPtAbility : SpecialAbility
    {
        private const float FirstDelaySeconds = 15f;

        private StageSessionController _stage;
        private FollowerManager _followers;

        public override string DisplayName =>
            "단체 PT 호출";

        public void Configure(
            StageSessionController stage,
            FollowerManager followers)
        {
            _stage = stage;
            _followers = followers;

            SetInitialCooldown(FirstDelaySeconds);
        }

        /// <summary>동행자가 있어야 부른다. 아무도 데리고 있지 않으면 위험이 없어 의미가 없다.</summary>
        protected override bool WantsToStart()
        {
            return _followers != null &&
                   _followers.Count > 0;
        }

        protected override void Fire()
        {
            GymZone.BeginPt(
                CityAbilityValues.PtSeconds);

            StageMoments.RaiseAbilityFired(
                transform.position,
                "단체 PT 시작");
        }

        private void LateUpdate()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            GymZone.TickPt(Time.deltaTime);
        }
    }
}
