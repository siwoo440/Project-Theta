using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 루프탑 클럽 VIP 라운지 입구다 (26일차, 부록 C.3 [7] 바운서).
    ///
    /// 1F 위층 계단을 바운서가 지킨다. 동행자가 6명을 넘으면 들여보내지 않고, 비상 우회도 없다.
    /// "[VIP]" 게스트를 데리고 있거나, 바운서가 멍하거나 아직 없으면 통과한다.
    /// </summary>
    public sealed class VipEntrance : MonoBehaviour
    {
        private const int EntranceFloor = 0;

        private static VipEntrance _current;

        private readonly List<DisruptorBase> _buffer =
            new List<DisruptorBase>();

        private FollowerManager _followers;

        public static bool IsLocked(
            int sourceFloor)
        {
            return _current != null &&
                   sourceFloor == EntranceFloor &&
                   _current.IsBlocked;
        }

        public bool IsBlocked =>
            BouncerLogic.IsBlocked(
                _followers == null ? 0 : _followers.Count,
                HasVipGuest(),
                IsBouncerAway());

        public void Configure(
            FollowerManager followers)
        {
            _followers = followers;
            _current = this;

            LocationProps.Sign(
                transform,
                EntranceFloor,
                new Vector2(FloorLayout.UpStairX - 1.5f, FloorSpace.WalkMaxY + 1.3f),
                $"VIP 라운지 · 동행 {BouncerLogic.MaximumGuests}명까지",
                new Color(0.85f, 0.60f, 1.00f),
                20);
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        private bool HasVipGuest()
        {
            if (_followers == null)
            {
                return false;
            }

            IReadOnlyList<FollowerController> list = _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                NpcRoleMark mark = NpcRoleMark.Get(list[i]);

                if (mark != null &&
                    mark.Role == NpcRole.VipGuest)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBouncerAway()
        {
            DisruptorBase.CopyActive(_buffer);

            for (int i = 0;
                 i < _buffer.Count;
                 i++)
            {
                DisruptorBase body = _buffer[i];

                if (body != null &&
                    body.Floor == EntranceFloor &&
                    body.Profile != null &&
                    body.Profile.Kind == DisruptorKind.Bouncer &&
                    !body.IsStunned)
                {
                    return false;
                }
            }

            return true;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
