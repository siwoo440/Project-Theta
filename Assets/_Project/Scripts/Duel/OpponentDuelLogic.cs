using UnityEngine;

namespace ProjectTheta.Duel
{
    public enum OpponentDuelInputSide
    {
        Left,
        Right
    }

    public enum OpponentDuelResult
    {
        None,
        PlayerWin,
        OpponentWin
    }

    public static class OpponentDuelLogic
    {
        public static bool IsCorrectInput(
            OpponentDuelInputSide expected,
            OpponentDuelInputSide actual)
        {
            return expected ==
                   actual;
        }

        public static OpponentDuelInputSide GetNextExpected(
            OpponentDuelInputSide current)
        {
            return current ==
                   OpponentDuelInputSide.Left
                ? OpponentDuelInputSide.Right
                : OpponentDuelInputSide.Left;
        }

        public static float AddPlayerPush(
            float current,
            float maximum,
            float gain)
        {
            float safeMaximum =
                Mathf.Max(
                    1f,
                    maximum);

            return Mathf.Clamp(
                current +
                Mathf.Max(
                    0f,
                    gain),
                0f,
                safeMaximum);
        }

        public static float ApplyOpponentPressure(
            float current,
            float pressurePerSecond,
            float deltaTime)
        {
            return Mathf.Max(
                0f,
                current -
                Mathf.Max(
                    0f,
                    pressurePerSecond) *
                Mathf.Max(
                    0f,
                    deltaTime));
        }

        public static OpponentDuelResult Resolve(
            float current,
            float maximum,
            float opponentWinThresholdNormalized,
            float playerWinThresholdNormalized)
        {
            float safeMaximum =
                Mathf.Max(
                    1f,
                    maximum);

            float opponentThreshold =
                safeMaximum *
                Mathf.Clamp01(
                    opponentWinThresholdNormalized);

            float playerThreshold =
                safeMaximum *
                Mathf.Clamp01(
                    playerWinThresholdNormalized);

            if (playerThreshold <
                opponentThreshold)
            {
                float swap =
                    playerThreshold;

                playerThreshold =
                    opponentThreshold;

                opponentThreshold =
                    swap;
            }

            if (current <=
                opponentThreshold)
            {
                return OpponentDuelResult.OpponentWin;
            }

            if (current >=
                playerThreshold)
            {
                return OpponentDuelResult.PlayerWin;
            }

            return OpponentDuelResult.None;
        }
    }
}
