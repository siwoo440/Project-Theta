using UnityEngine;

namespace ProjectTheta.Player
{
    public static class PlayerMovementMath
    {
        public static Vector2 NormalizeInput(Vector2 input)
        {
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        public static Vector2 ResolveDashDirection(Vector2 currentInput, Vector2 lastDirection)
        {
            Vector2 normalizedCurrent = NormalizeInput(currentInput);
            if (normalizedCurrent.sqrMagnitude > 0.0001f)
            {
                return normalizedCurrent;
            }

            Vector2 normalizedLast = NormalizeInput(lastDirection);
            return normalizedLast.sqrMagnitude > 0.0001f ? normalizedLast : Vector2.right;
        }
    }
}
