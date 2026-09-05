using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    public sealed class RivalFollowerManager : MonoBehaviour
    {
        [SerializeField] private float _horizontalSpacing = 0.78f;
        [SerializeField] private float _rowSpacing = 0.46f;
        [SerializeField] private int _rowsPerColumn = 3;

        private readonly List<RivalFollowerController> _followers =
            new List<RivalFollowerController>();

        private RivalController _controller;

        public int Count =>
            _followers.Count;

        private RivalController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller =
                        GetComponent<RivalController>();
                }

                return _controller;
            }
        }

        public bool TryAdd(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Rival)
            {
                return false;
            }

            RivalFollowerController follower =
                target.GetComponent<
                    RivalFollowerController>();

            if (follower == null)
            {
                follower =
                    target.gameObject.AddComponent<
                        RivalFollowerController>();
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

        public bool RemoveTarget(
            HypnosisTarget target)
        {
            if (target == null)
            {
                return false;
            }

            RivalFollowerController follower =
                target.GetComponent<
                    RivalFollowerController>();

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
            int slotIndex)
        {
            int rows =
                Mathf.Max(
                    1,
                    _rowsPerColumn);

            int safeSlot =
                Mathf.Max(
                    0,
                    slotIndex);

            int column =
                safeSlot /
                rows;

            int row =
                safeSlot %
                rows;

            float center =
                (rows - 1) *
                0.5f;

            float horizontalDistance =
                (column + 1) *
                _horizontalSpacing;

            float verticalOffset =
                (row - center) *
                _rowSpacing;

            int facing =
                Controller == null
                    ? 1
                    : Controller.FacingDirection;

            float x =
                transform.position.x -
                facing *
                horizontalDistance;

            float y =
                Mathf.Clamp(
                    transform.position.y +
                    verticalOffset,
                    SchoolHallwayPrototypeBuilder.WalkMinY + 0.35f,
                    SchoolHallwayPrototypeBuilder.WalkMaxY - 0.25f);

            return new Vector2(
                x,
                y);
        }

        private void ReindexFollowers()
        {
            for (int i = 0;
                 i < _followers.Count;
                 i++)
            {
                if (_followers[i] != null)
                {
                    _followers[i].SetSlotIndex(
                        i);
                }
            }
        }
    }
}
