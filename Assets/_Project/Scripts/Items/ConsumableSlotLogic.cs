using System;

namespace ProjectTheta.Items
{
    /// <summary>
    /// 소비 아이템 슬롯 규칙이다. 슬롯은 2개로 고정한다.
    /// </summary>
    public static class ConsumableSlotLogic
    {
        public const int SlotCount = 2;

        public static bool IsValidSlot(
            int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < SlotCount;
        }

        public static bool CanUse(
            ConsumableItem item)
        {
            return item !=
                   ConsumableItem.None;
        }

        /// <summary>0~1 난수 하나로 시작 아이템을 고른다.</summary>
        public static ConsumableItem PickStartingItem(
            float randomValue)
        {
            ConsumableItem[] pool =
                ConsumableItemTable.StartingPool;

            float clamped =
                randomValue < 0f
                    ? 0f
                    : randomValue > 1f
                        ? 1f
                        : randomValue;

            int index =
                (int)(clamped *
                      pool.Length);

            if (index >=
                pool.Length)
            {
                index =
                    pool.Length - 1;
            }

            return pool[index];
        }

        /// <summary>
        /// 두 슬롯에 서로 다른 아이템이 들어가도록 보정한다.
        /// 같은 아이템이 두 개 나오면 다음 후보로 밀어 선택지를 보장한다.
        /// </summary>
        public static ConsumableItem PickSecondItem(
            ConsumableItem first,
            float randomValue)
        {
            ConsumableItem[] pool =
                ConsumableItemTable.StartingPool;

            ConsumableItem picked =
                PickStartingItem(
                    randomValue);

            if (picked != first)
            {
                return picked;
            }

            int index =
                Array.IndexOf(
                    pool,
                    picked);

            return pool[
                (Math.Max(
                     0,
                     index) +
                 1) %
                pool.Length];
        }
    }
}
