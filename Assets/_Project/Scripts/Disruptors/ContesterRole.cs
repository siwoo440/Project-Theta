using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Companion;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 쟁탈 역할이다 (23일차, 부록 C.3 [1] 헌팅남).
    ///
    /// 플레이어 무리 맨 뒤의 동행자 곁으로 다가가 말을 건다. 곁에 붙어 있는 동안 쟁탈 게이지가 오르고,
    /// 가득 차면 그 동행자는 무리를 떠난다.
    /// 대처: 플레이어가 그 동행자 곁을 지키기(무리를 촘촘하게), 파동으로 멍하게 하기, 차단 부적.
    ///
    /// 2인 1조라 두 번째 짝은 뒤에서 두 번째 동행자를 노린다. 한 명을 막는 동안 다른 한 명이 계속 쟁탈한다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class ContesterRole : MonoBehaviour
    {
        /// <summary>목표 곁 자리를 다시 잡는 간격이다. 매 프레임 이동 목표를 바꾸면 떨린다.</summary>
        private const float RepathSeconds = 0.4f;

        /// <summary>한 명을 데려간 뒤 다음 목표를 찾기까지 쉬는 시간이다.</summary>
        private const float RestSeconds = 6f;

        /// <summary>목표 동행자 옆 이만큼 떨어진 자리에 선다.</summary>
        private const float StandOffset = 0.7f;

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;
        private int _pairIndex;

        private FollowerController _target;
        private float _gauge;
        private float _repathRemaining;
        private float _restRemaining;

        /// <summary>쟁탈 게이지(0~1)다. 머리 위 표시에 쓴다.</summary>
        public float ClaimProgress =>
            _gauge / ClaimLogic.Maximum;

        public bool HasTarget =>
            _target != null;

        public void Configure(
            FollowerManager followers,
            Transform player,
            int pairIndex)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;
            _pairIndex = Mathf.Max(0, pairIndex);
        }

        private void Update()
        {
            if (_body == null ||
                _followers == null)
            {
                return;
            }

            if (!_body.CanAct)
            {
                // 멍해지면 하던 말이 끊긴다.
                if (_body.IsStunned)
                {
                    _gauge = 0f;
                }

                return;
            }

            float deltaTime =
                Time.deltaTime;

            if (_restRemaining > 0f)
            {
                _restRemaining -= deltaTime;
                _target = null;

                return;
            }

            _target =
                PickTarget();

            if (_target == null)
            {
                _gauge =
                    ClaimLogic.Advance(
                        _gauge,
                        false,
                        false,
                        1f,
                        deltaTime);

                return;
            }

            Vector2 self = transform.position;
            Vector2 target = _target.transform.position;

            ApproachTarget(
                self,
                target,
                deltaTime);

            bool inReach =
                Vector2.Distance(self, target) <=
                ClaimLogic.ReachDistance;

            bool guarded =
                _followers.IsContestWarded ||
                (_player != null &&
                 Vector2.Distance(_player.position, target) <=
                 ClaimLogic.GuardDistance);

            _gauge =
                ClaimLogic.Advance(
                    _gauge,
                    inReach,
                    guarded,
                    BalanceOverrides.StageOrDefault.ContestRiseScale *
                    // 25일차: 오피스 탕비실 근처에서는 사내 인기남이 두 배로 빠르다.
                    MallOfficeValues.GetPantryMultiplier(
                        target.x,
                        Stage.Locations.OfficeLayout.GetPantryX(_body.Floor),
                        Stage.Locations.OfficeLayout.Active) *
                    // 26일차: 클럽 드롭 직후에는 클럽 MD가 세 배로 빠르다.
                    Stage.Locations.ClubBeat.ContestMultiplier,
                    deltaTime);

            if (ClaimLogic.IsComplete(_gauge))
            {
                TakeTarget();
            }
        }

        private void ApproachTarget(
            Vector2 self,
            Vector2 target,
            float deltaTime)
        {
            _repathRemaining -= deltaTime;

            if (_repathRemaining > 0f)
            {
                return;
            }

            _repathRemaining = RepathSeconds;

            // 목표의 내 쪽 옆자리에 선다. 목표와 완전히 겹치면 누가 누군지 안 보인다.
            float side =
                self.x >= target.x
                    ? 1f
                    : -1f;

            _body.MoveToward(
                target + new Vector2(side * StandOffset, 0f),
                RepathSeconds + 0.2f);
        }

        private FollowerController PickTarget()
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            int index =
                ClaimLogic.PickTargetIndex(
                    list.Count,
                    _pairIndex);

            if (index < 0)
            {
                return null;
            }

            FollowerController follower =
                list[index];

            if (follower == null ||
                !follower.isActiveAndEnabled ||
                FloorSpace.FloorAt(follower.transform.position.y) != _body.Floor)
            {
                return null;
            }

            // 목표가 바뀌면 게이지는 처음부터다.
            if (follower != _target)
            {
                _gauge = 0f;
            }

            return follower;
        }

        private void TakeTarget()
        {
            Vector2 position =
                _target.transform.position;

            _followers.RequestRelease(
                _target);

            StageMoments.RaiseFollowerStolen(
                position);

            GameVfx.FloatText(
                _body.Profile == null
                    ? "빼앗겼다!"
                    : $"{_body.Profile.DisplayName}에게 넘어갔다!",
                position + new Vector2(0f, 1.8f),
                UiTheme.Danger,
                UiTheme.FontBody);

            _target = null;
            _gauge = 0f;
            _restRemaining = RestSeconds;
        }
    }
}
