using UnityEngine;

namespace ProjectTheta.Capture
{
    public static class CaptureEscapeLogic
    {
        public static bool IsCorrectInput(
            CaptureInputSide expected,
            CaptureInputSide actual)
        {
            return expected ==
                   actual;
        }

        public static CaptureInputSide GetNextExpected(
            CaptureInputSide current)
        {
            return current ==
                   CaptureInputSide.Left
                ? CaptureInputSide.Right
                : CaptureInputSide.Left;
        }

        public static float AddEscapeProgress(
            float current,
            float maximum,
            float gain)
        {
            float safeMaximum =
                Mathf.Max(
                    1f,
                    maximum);

            float safeGain =
                Mathf.Max(
                    0f,
                    gain);

            return Mathf.Clamp(
                current +
                safeGain,
                0f,
                safeMaximum);
        }
    }
}
