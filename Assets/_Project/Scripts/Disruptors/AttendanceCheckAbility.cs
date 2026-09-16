using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 인사팀 평가관의 <b>근태 체크</b>다 (부록 C.3 [0]).
    ///
    /// 시야 안 플레이어 동행자 1명에게 체크 표식을 붙인다.
    ///   표식이 붙은 10초 동안 그 동행자의 유지도가 두 배로 줄어든다.
    ///   표식이 붙은 채 회수하면 그 동행자의 정기가 20% 깎인다.
    /// 대처: 표식이 붙은 동행자를 시야 밖으로 빼기, 파동으로 평가관을 멍하게 하기, 표식이 끝날 때까지 회수 보류.
    /// </summary>
    public sealed class AttendanceCheckAbility : SpecialAbility
    {
        public const float MarkSeconds = 10f;

        private FollowerManager _followers;

        public override string DisplayName =>
            "근태 체크";

        public void Configure(
            FollowerManager followers)
        {
            _followers = followers;
        }

        protected override bool WantsToStart()
        {
            return FindTarget() != null;
        }

        protected override void OnTelegraph()
        {
            FollowerController target =
                FindTarget();

            if (target != null)
            {
                Body.FaceToward(
                    target.transform.position.x);
            }
        }

        protected override void Fire()
        {
            FollowerController target =
                FindTarget();

            if (target == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            AttendanceMark mark =
                target.GetComponent<AttendanceMark>();

            if (mark == null)
            {
                mark =
                    target.gameObject.AddComponent<AttendanceMark>();
            }

            mark.Apply(
                MarkSeconds);

            StageMoments.RaiseAbilityFired(
                target.transform.position,
                DisplayName);
        }

        /// <summary>시야 안의 동행자 중 표식이 없는 가장 가까운 한 명이다.</summary>
        private FollowerController FindTarget()
        {
            if (_followers == null ||
                Body == null ||
                Body.Profile == null)
            {
                return null;
            }

            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            Vector2 self = transform.position;

            FollowerController best = null;
            float bestDistance = float.MaxValue;

            float range =
                Body.Profile.SightRange *
                Mathf.Max(
                    0f,
                    Balance.BalanceOverrides.StageOrDefault.WatcherSightScale);

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                FollowerController follower = list[i];

                if (follower == null ||
                    !follower.isActiveAndEnabled)
                {
                    continue;
                }

                Vector2 position = follower.transform.position;

                if (FloorSpace.FloorAt(position.y) != Body.Floor)
                {
                    continue;
                }

                // 평가관은 바라보는 쪽만 본다. 좌우를 번갈아 보므로 반대편에 있으면 잠시 안전하다.
                if (!DetectionLogic.IsInSight(
                        self.x,
                        self.y,
                        Body.Facing,
                        position.x,
                        position.y,
                        Body.Profile.SightHalfAngle,
                        range))
                {
                    continue;
                }

                AttendanceMark existing =
                    follower.GetComponent<AttendanceMark>();

                if (existing != null &&
                    existing.IsMarked)
                {
                    continue;
                }

                float distance =
                    (position - self).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = follower;
                }
            }

            return best;
        }
    }
}
