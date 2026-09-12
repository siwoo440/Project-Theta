using System;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 기획서 10.3절 콤보다.
    /// 짧은 시간 안에 연속으로 성과를 내면 배율이 쌓이고, 일정 시간 아무 성과가 없으면 끊긴다.
    /// 콤보 배율은 정기가 아니라 점수에만 곱한다.
    /// (정기는 클리어 조건, 점수는 랭크 평가로 분리하기 위해서다.)
    /// </summary>
    public static class ComboLogic
    {
        public const float TimeoutSeconds = 6.0f;
        public const float StepMultiplier = 0.1f;
        public const float MinimumMultiplier = 1.0f;
        public const float MaximumMultiplier = 3.0f;

        /// <summary>배율 상한에 도달하는 콤보 수다.</summary>
        public const int MaximumCombo = 20;

        public static int AddCombo(
            int currentCombo)
        {
            int safeCombo =
                Math.Max(
                    0,
                    currentCombo);

            return Math.Min(
                MaximumCombo,
                safeCombo + 1);
        }

        /// <summary>마지막 성과 이후 흐른 시간을 보고 콤보가 유지되는지 판정한다.</summary>
        public static int Tick(
            int currentCombo,
            float secondsSinceLastEvent)
        {
            if (currentCombo <= 0)
            {
                return 0;
            }

            return secondsSinceLastEvent >=
                   TimeoutSeconds
                ? 0
                : currentCombo;
        }

        public static float GetMultiplier(
            int combo)
        {
            int safeCombo =
                Math.Max(
                    0,
                    combo);

            float multiplier =
                MinimumMultiplier +
                (safeCombo *
                 StepMultiplier);

            return multiplier >
                   MaximumMultiplier
                ? MaximumMultiplier
                : multiplier;
        }

        /// <summary>콤보가 끊기기까지 남은 시간이다. UI 표시에 사용한다.</summary>
        public static float GetRemainingSeconds(
            int currentCombo,
            float secondsSinceLastEvent)
        {
            if (currentCombo <= 0)
            {
                return 0f;
            }

            return Math.Max(
                0f,
                TimeoutSeconds -
                Math.Max(
                    0f,
                    secondsSinceLastEvent));
        }
    }
}
