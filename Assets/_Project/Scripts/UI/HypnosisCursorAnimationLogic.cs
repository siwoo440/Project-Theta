using UnityEngine;

namespace ProjectTheta.UI
{
    public static class HypnosisCursorAnimationLogic
    {
        public static int GetFrameIndex(
            float time,
            float frameDuration)
        {
            float safeDuration =
                Mathf.Max(0.01f, frameDuration);

            int frame =
                Mathf.FloorToInt(
                    Mathf.Max(0f, time) /
                    safeDuration);

            return frame % 2;
        }
    }
}
