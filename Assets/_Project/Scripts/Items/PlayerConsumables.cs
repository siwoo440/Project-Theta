using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Core;

namespace ProjectTheta.Items
{
    /// <summary>
    /// 기획서 A.8절의 2슬롯 소비 아이템이다. 1 · 2 키로 사용한다.
    ///
    /// 획득 방식(드롭·구매)은 메타 시스템 작업이므로 지금은 시작 시 두 개를 지급하기만 한다.
    /// </summary>
    public sealed class PlayerConsumables : MonoBehaviour
    {
        /// <summary>최면 대상 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<HypnosisTarget> _targetBuffer =
            new System.Collections.Generic.List<HypnosisTarget>();
        private readonly ConsumableItem[] _slots =
            new ConsumableItem[
                ConsumableSlotLogic.SlotCount];

        private PlayerFocus _focus;
        private FollowerManager _followers;
        private PlayerSideViewController _movement;
        private StageSessionController _stage;

        public int SlotCount =>
            ConsumableSlotLogic.SlotCount;

        private void Awake()
        {
            _focus =
                GetComponent<PlayerFocus>();

            _followers =
                GetComponent<FollowerManager>();

            _movement =
                GetComponent<
                    PlayerSideViewController>();

            _stage =
                GetComponent<StageSessionController>();

            GrantStartingItems();
        }

        public ConsumableItem GetSlot(
            int slotIndex)
        {
            return ConsumableSlotLogic.IsValidSlot(
                       slotIndex)
                ? _slots[slotIndex]
                : ConsumableItem.None;
        }

        public string GetSlotLabel(
            int slotIndex)
        {
            return ConsumableItemTable.Get(
                GetSlot(
                    slotIndex)).DisplayName;
        }

        public string GetSlotDescription(
            int slotIndex)
        {
            return ConsumableItemTable.Get(
                GetSlot(
                    slotIndex)).Description;
        }

        /// <summary>런 시작 시 서로 다른 아이템 두 개를 지급한다.</summary>
        public void GrantStartingItems()
        {
            _slots[0] =
                ConsumableSlotLogic.PickStartingItem(
                    Random.value);

            _slots[1] =
                ConsumableSlotLogic.PickSecondItem(
                    _slots[0],
                    Random.value);
        }

        public bool TryUse(
            int slotIndex)
        {
            if (_stage != null &&
                !_stage.IsRunning)
            {
                return false;
            }

            if (!ConsumableSlotLogic.IsValidSlot(
                    slotIndex))
            {
                return false;
            }

            ConsumableItem item =
                _slots[slotIndex];

            if (!ConsumableSlotLogic.CanUse(
                    item))
            {
                return false;
            }

            ApplyEffect(
                item);

            _slots[slotIndex] =
                ConsumableItem.None;

            return true;
        }

        private void Update()
        {
            // 카드 선택 키(1·2·3)와 아이템 키(1·2)가 겹치므로 멈춘 동안에는 읽지 않는다.
            if (GameplayPause.IsPaused)
            {
                return;
            }

            if (ReadSlotPressed(
                    0))
            {
                TryUse(
                    0);
            }
            else if (ReadSlotPressed(
                         1))
            {
                TryUse(
                    1);
            }
        }

        private void ApplyEffect(
            ConsumableItem item)
        {
            ConsumableItemProfile profile =
                ConsumableItemTable.Get(
                    item);

            switch (item)
            {
                case ConsumableItem.Sedative:
                    RelieveAllFollowers(
                        profile.Magnitude);
                    break;

                case ConsumableItem.FocusTonic:
                    _focus?.Refill(
                        profile.Magnitude);
                    break;

                case ConsumableItem.WardCharm:
                    _followers?.ApplyContestWard(
                        profile.DurationSeconds);
                    break;

                case ConsumableItem.SprintDraught:
                    _movement?.ApplySprintBoost(
                        profile.DurationSeconds,
                        1f + profile.Magnitude,
                        0.5f);
                    break;
            }
        }

        private void RelieveAllFollowers(
            float amount)
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner !=
                    NpcOwner.Player)
                {
                    continue;
                }

                target.GetComponent<ImpulseMeter>()?.
                    RelieveImpulse(
                        amount);
            }
        }

        private static bool ReadSlotPressed(
            int slotIndex)
        {
            return GameInput.WasPressed(
                slotIndex == 0
                    ? GameAction.Item1
                    : GameAction.Item2);
        }
    }
}
