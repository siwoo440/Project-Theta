using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 비서실장의 <b>긴급 회의 소집</b>이다 (부록 C.3 [6]).
    ///
    /// 동행자 중 직원(출입증 직원 포함)이 있으면 1.5초 예고(메신저 알림) 뒤,
    /// 최대 2명을 그 층 회의실로 12초 동안 끌고 간다 (재사용 35초).
    /// 회의 중에 회의실에 들어가면 구역 경계도 +40.
    /// 대처: 방문객 위주로 동행, 회의가 끝난 뒤 다시 합류, 파동으로 비서실장을 멍하게 하기.
    /// </summary>
    public sealed class EmergencyMeetingAbility : SpecialAbility
    {
        private const float FirstDelaySeconds = 20f;

        private readonly List<FollowerController> _candidates =
            new List<FollowerController>();

        private FollowerManager _followers;
        private Transform _player;

        public override string DisplayName =>
            "긴급 회의 소집";

        public void Configure(
            FollowerManager followers,
            Transform player)
        {
            _followers = followers;
            _player = player;

            SetInitialCooldown(FirstDelaySeconds);
        }

        protected override bool WantsToStart()
        {
            return CollectEmployees() > 0;
        }

        protected override void Fire()
        {
            int count =
                MeetingLogic.GetSummonCount(
                    CollectEmployees());

            for (int i = 0;
                 i < count;
                 i++)
            {
                FollowerController follower = _candidates[i];

                MeetingSummons.Apply(
                    follower,
                    _player,
                    OfficeLayout.GetMeetingRoomX(
                        FloorSpace.FloorAt(follower.transform.position.y)));
            }

            if (count > 0)
            {
                StageMoments.RaiseAbilityFired(
                    _candidates[0].transform.position,
                    $"긴급 회의 소집 ({count}명)");
            }
        }

        private int CollectEmployees()
        {
            _candidates.Clear();

            if (_followers == null)
            {
                return 0;
            }

            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                FollowerController follower = list[i];

                if (follower == null ||
                    !follower.isActiveAndEnabled ||
                    MeetingSummons.IsInMeeting(follower))
                {
                    continue;
                }

                NpcRoleMark mark = NpcRoleMark.Get(follower);

                if (mark != null &&
                    mark.IsEmployee)
                {
                    _candidates.Add(follower);
                }
            }

            return _candidates.Count;
        }
    }
}
