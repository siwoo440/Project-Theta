using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Stage;

namespace ProjectTheta.Companion
{
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class FollowerManager : MonoBehaviour
    {
        [SerializeField] private float _horizontalSpacing = 0.92f;
        [SerializeField] private float _rowSpacing = 0.48f;
        [SerializeField] private int _rowsPerColumn = 3;

        private readonly List<FollowerController> _followers =
            new List<FollowerController>();

        private PlayerSideViewController _playerController;

        private float _contestWardRemaining;

        /// <summary>차단 부적으로 플레이어 동행 NPC가 쟁탈 면역인 상태다.</summary>
        public bool IsContestWarded =>
            _contestWardRemaining > 0f;

        public float ContestWardRemaining =>
            Mathf.Max(
                0f,
                _contestWardRemaining);

        public void ApplyContestWard(
            float seconds)
        {
            _contestWardRemaining =
                Mathf.Max(
                    _contestWardRemaining,
                    Mathf.Max(
                        0f,
                        seconds));
        }

        private void Update()
        {
            if (_contestWardRemaining > 0f)
            {
                _contestWardRemaining =
                    Mathf.Max(
                        0f,
                        _contestWardRemaining -
                        Time.deltaTime);
            }
        }

        public int Count =>
            _followers.Count;

        /// <summary>
        /// 지금 따라오는 동행자다.
        /// 층 이동 때 함께 옮기기 위해 <see cref="ProjectTheta.Stage.FloorTransitionController"/>가 읽는다.
        /// </summary>
        public IReadOnlyList<FollowerController> Followers =>
            _followers;

        public float LowestStabilityNormalized
        {
            get
            {
                if (_followers.Count == 0)
                {
                    return 1f;
                }

                float lowest = 1f;

                for (int i = 0;
                     i < _followers.Count;
                     i++)
                {
                    FollowerController follower =
                        _followers[i];

                    if (follower == null)
                    {
                        continue;
                    }

                    lowest =
                        Mathf.Min(
                            lowest,
                            follower.StabilityNormalized);
                }

                return lowest;
            }
        }

        private void Awake()
        {
            _playerController =
                GetComponent<PlayerSideViewController>();
        }

        public bool TryAdd(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Player ||
                target.IsFollowing)
            {
                return false;
            }

            FollowerController follower =
                target.GetComponent<FollowerController>();

            if (follower == null)
            {
                return false;
            }

            if (_followers.Contains(
                    follower))
            {
                return true;
            }

            _followers.Add(
                follower);

            follower.BeginFollowing(
                this,
                transform,
                _followers.Count - 1);

            return true;
        }

        public bool TransferOutFollower(
            FollowerController follower)
        {
            if (follower == null)
            {
                return false;
            }

            int index =
                _followers.IndexOf(
                    follower);

            if (index >= 0)
            {
                _followers.RemoveAt(
                    index);

                ReindexFollowers();
            }

            follower.StopFollowingForOwnershipTransfer();

            return true;
        }

        public Vector2 GetSlotWorldPosition(
            int slotIndex,
            Vector2 personalOffset)
        {
            int facingDirection =
                _playerController == null
                    ? 1
                    : _playerController.FacingDirection;

            int rows =
                Mathf.Max(
                    1,
                    _rowsPerColumn);

            float horizontalDistance =
                FollowerFormationLogic.
                    GetCompactHorizontalDistance(
                        slotIndex,
                        _horizontalSpacing,
                        rows);

            float verticalOffset =
                FollowerFormationLogic.
                    GetCompactVerticalOffset(
                        slotIndex,
                        _rowSpacing,
                        rows);

            float trailingDistance =
                Mathf.Max(
                    0.45f,
                    horizontalDistance +
                    personalOffset.x);

            float x =
                transform.position.x -
                (facingDirection *
                 trailingDistance);

            // 플레이어가 선 층 안에서만 줄을 세운다.
            float y =
                FloorSpace.ClampYNear(
                    transform.position.y,
                    transform.position.y +
                    verticalOffset +
                    personalOffset.y,
                    0.35f,
                    0.25f);

            return new Vector2(
                x,
                y);
        }

        public void RequestRelease(
            FollowerController follower)
        {
            RemoveFollower(
                follower,
                true);
        }

        public bool ConsumeFollower(
            FollowerController follower)
        {
            return RemoveFollower(
                follower,
                true);
        }

        private bool RemoveFollower(
            FollowerController follower,
            bool stopFollowing)
        {
            if (follower == null)
            {
                return false;
            }

            int index =
                _followers.IndexOf(
                    follower);

            if (index < 0)
            {
                return false;
            }

            _followers.RemoveAt(
                index);

            if (stopFollowing)
            {
                follower.StopFollowing();
            }

            ReindexFollowers();

            return true;
        }

        private void ReindexFollowers()
        {
            for (int i = 0;
                 i < _followers.Count;
                 i++)
            {
                FollowerController follower =
                    _followers[i];

                if (follower != null)
                {
                    follower.SetSlotIndex(
                        i);
                }
            }
        }
    }
}
