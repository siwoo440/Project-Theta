using System;

namespace ProjectTheta.Companion
{
    public static class FollowerFormationLogic
    {
        public static float GetHorizontalDistance(
            int slotIndex,
            float spacing)
        {
            int safeSlot =
                Math.Max(0, slotIndex);

            float safeSpacing =
                Math.Max(0f, spacing);

            int column =
                safeSlot / 2;

            return (column + 1) *
                   safeSpacing;
        }

        public static float GetVerticalOffset(
            int slotIndex,
            float rowSpacing)
        {
            int safeSlot =
                Math.Max(0, slotIndex);

            float halfSpacing =
                Math.Max(0f, rowSpacing) *
                0.5f;

            return safeSlot % 2 == 0
                ? -halfSpacing
                : halfSpacing;
        }
    }
}
