using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    public sealed class PopularGuyFollowerManager : MonoBehaviour
    {
        [SerializeField] private float _horizontalSpacing = 0.78f;
        [SerializeField] private float _rowSpacing = 0.46f;
        [SerializeField] private int _rowsPerColumn = 3;

        private readonly List<PopularGuyFollowerController> _followers =
            new List<PopularGuyFollowerController>();

        private PopularGuyController _controller;

        public int Count =>
            _followers.Count;

        private PopularGuyController Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller =
                        GetComponent<
                            PopularGuyController>();
                }

                return _controller;
            }
        }

        public bool TryAdd(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.PopularGuy)
            {
                return false;
            }

            PopularGuyFollowerController follower =
                target.GetComponent<
                    PopularGuyFollowerController>();

            if (follower == null)
            {
                follower =
                    target.gameObject.AddComponent<
                        PopularGuyFollowerController>();
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

            PopularGuyFollowerController follower =
                target.GetComponent<
                    PopularGuyFollowerController>();

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
