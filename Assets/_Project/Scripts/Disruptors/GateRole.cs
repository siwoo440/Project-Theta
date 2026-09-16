using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 관문 역할이다 (24일차, 부록 C.3 [2] 역무원).
    ///
    /// 개찰구 앞에 선다. 동행자가 5명 이상이면 플레이어와 반대편에 남은 동행자를 세우고,
    /// 1초에 한 명씩만 통과시킨다. 4명 이하면 그대로 지나간다.
    /// 대처: 인원을 4명 이하로 나눠 통과, 열차가 도착해 인파가 쏟아질 때 섞여 지나가기, 파동으로 역무원을 멍하게 하기.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class GateRole : MonoBehaviour
    {
        private readonly List<FollowerController> _queue =
            new List<FollowerController>();

        private readonly Dictionary<FollowerController, float> _passedAt =
            new Dictionary<FollowerController, float>();

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;
        private float _gateX;
        private float _clock;
        private float _lastRelease = -99f;

        public int WaitingCount =>
            _queue.Count;

        public void Configure(
            FollowerManager followers,
            Transform player,
            float gateX)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;
            _gateX = gateX;
        }

        private void OnDisable()
        {
            ReleaseAll();
        }

        private void Update()
        {
            if (_body == null ||
                _followers == null ||
                _player == null ||
                GameplayPause.IsPaused)
            {
                return;
            }

            _clock += Time.deltaTime;

            DropInvalid();

            bool open =
                _body.IsStunned ||
                !_body.CanAct ||
                !GateLogic.NeedsCheck(
                    _followers.Count,
                    TrainArrival.IsCrowdRush);

            if (open)
            {
                ReleaseAll();

                return;
            }

            HoldNewArrivals();

            if (_queue.Count > 0 &&
                GateLogic.CanRelease(_clock - _lastRelease))
            {
                ReleaseAt(0);
            }
        }

        private void HoldNewArrivals()
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            float playerX = _player.position.x;

            if (FloorSpace.FloorAt(_player.position.y) != _body.Floor)
            {
                return;
            }

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                FollowerController follower = list[i];

                if (follower == null ||
                    !follower.isActiveAndEnabled ||
                    _queue.Contains(follower))
                {
                    continue;
                }

                Vector2 position = follower.transform.position;

                if (FloorSpace.FloorAt(position.y) != _body.Floor ||
                    Mathf.Abs(position.x - _gateX) > GateLogic.CheckHalfWidth ||
                    !GateLogic.IsOnOtherSide(_gateX, playerX, position.x))
                {
                    continue;
                }

                if (_passedAt.TryGetValue(follower, out float passed) &&
                    _clock - passed < GateLogic.PassedMemorySeconds)
                {
                    continue;
                }

                follower.SetExternalControl(true);
                _queue.Add(follower);
            }
        }

        private void DropInvalid()
        {
            for (int i = _queue.Count - 1;
                 i >= 0;
                 i--)
            {
                FollowerController follower = _queue[i];

                if (follower != null &&
                    follower.isActiveAndEnabled &&
                    IsStillFollowing(follower))
                {
                    continue;
                }

                if (follower != null)
                {
                    follower.SetExternalControl(false);
                }

                _queue.RemoveAt(i);
            }
        }

        private bool IsStillFollowing(
            FollowerController follower)
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                if (list[i] == follower)
                {
                    return true;
                }
            }

            return false;
        }

        private void ReleaseAt(
            int index)
        {
            FollowerController follower = _queue[index];

            _queue.RemoveAt(index);

            if (follower == null)
            {
                return;
            }

            follower.SetExternalControl(false);

            _passedAt[follower] = _clock;
            _lastRelease = _clock;
        }

        private void ReleaseAll()
        {
            for (int i = _queue.Count - 1;
                 i >= 0;
                 i--)
            {
                if (_queue[i] != null)
                {
                    _queue[i].SetExternalControl(false);
                    _passedAt[_queue[i]] = _clock;
                }
            }

            _queue.Clear();
        }
    }
}
