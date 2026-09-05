using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Player;

namespace ProjectTheta.Companion
{
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class FollowerManager : MonoBehaviour
    {
        [SerializeField] private float _horizontalSpacing = 0.92f;
        [SerializeField] private float _rowSpacing = 0.48f;

        private readonly List<FollowerController> _followers =
            new List<FollowerController>();

        private PlayerSideViewController _playerController;

        public int Count =>
            _followers.Count;

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

            if (_followers.Contains(follower))
            {
                return true;
            }

            _followers.Add(follower);

            follower.BeginFollowing(
                this,
                transform,
                _followers.Count - 1);

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

            float horizontalDistance =
                FollowerFormationLogic.
                    GetHorizontalDistance(
                        slotIndex,
                        _horizontalSpacing);

            float verticalOffset =
                FollowerFormationLogic.
                    GetVerticalOffset(
                        slotIndex,
                        _rowSpacing);

            float trailingDistance =
                Mathf.Max(
                    0.45f,
                    horizontalDistance +
                    personalOffset.x);

            float x =
                transform.position.x -
                (facingDirection *
                 trailingDistance);

            float y =
                Mathf.Clamp(
                    transform.position.y +
                    verticalOffset +
                    personalOffset.y,
                    SchoolHallwayPrototypeBuilder.WalkMinY + 0.35f,
                    SchoolHallwayPrototypeBuilder.WalkMaxY - 0.25f);

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

            _followers.RemoveAt(index);

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
                    follower.SetSlotIndex(i);
                }
            }
        }
    }
}
