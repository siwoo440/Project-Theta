using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 경쟁자 한 명이 확보한 NPC 대열을 관리한다.
    /// 소유자 구분은 같은 오브젝트의 <see cref="OpponentControllerBase.OwnerTag"/>를 사용하므로
    /// 금태양과 인기남이 같은 구현을 공유한다.
    /// </summary>
    public sealed class OpponentFollowerManager : MonoBehaviour
    {
        [SerializeField] private float _horizontalSpacing = 0.78f;
        [SerializeField] private float _rowSpacing = 0.46f;
        [SerializeField] private int _rowsPerColumn = 3;

        private readonly List<OpponentFollowerController> _followers =
            new List<OpponentFollowerController>();

        private OpponentControllerBase _controller;

        public int Count =>
            _followers.Count;

        public NpcOwner OwnerTag =>
            Controller == null
                ? NpcOwner.Neutral
                : Controller.OwnerTag;

        private OpponentControllerBase Controller
        {
            get
            {
                if (_controller == null)
                {
                    _controller =
                        GetComponent<
                            OpponentControllerBase>();
                }

                return _controller;
            }
        }

        public bool TryAdd(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                OwnerTag)
            {
                return false;
            }

            OpponentFollowerController follower =
                target.GetComponent<
                    OpponentFollowerController>();

            if (follower == null)
            {
                follower =
                    target.gameObject.AddComponent<
                        OpponentFollowerController>();
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

            OpponentFollowerController follower =
                target.GetComponent<
                    OpponentFollowerController>();

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
            return GetSlotWorldPosition(
                slotIndex,
                Vector2.zero);
        }

        public Vector2 GetSlotWorldPosition(
            int slotIndex,
            Vector2 personalOffset)
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
                Mathf.Max(
                    0.45f,
                    ((column + 1) *
                     _horizontalSpacing) +
                    personalOffset.x);

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
                    verticalOffset +
                    personalOffset.y,
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
