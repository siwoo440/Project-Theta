using System;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 기획서 13.1절 집중력 계산이다.
    ///
    /// 집중력은 최면 유지·대시·스킬에 쓰이는 자원으로,
    /// "무한정 쳐다보기"를 막는 유일한 제동 장치다.
    /// 0이 되면 최면이 강제로 끊기고, 일정량 이상 회복될 때까지 재시전이 막힌다.
    /// </summary>
    public static class FocusLogic
    {
        public static float Drain(
            float current,
            float amount)
        {
            return Math.Max(
                0f,
                current -
                Math.Max(
                    0f,
                    amount));
        }

        public static float DrainPerSecond(
            float current,
            float perSecond,
            float deltaTime)
        {
            return Drain(
                current,
                Math.Max(
                    0f,
                    perSecond) *
                Math.Max(
                    0f,
                    deltaTime));
        }

        public static float Recover(
            float current,
            float maximum,
            float perSecond,
            float deltaTime)
        {
            float safeMaximum =
                Math.Max(
                    1f,
                    maximum);

            float recovered =
                current +
                (Math.Max(
                     0f,
                     perSecond) *
                 Math.Max(
                     0f,
                     deltaTime));

            return Math.Min(
                safeMaximum,
                Math.Max(
                    0f,
                    recovered));
        }

        /// <summary>마지막 소모로부터 지연 시간이 지나야 회복이 시작된다.</summary>
        public static bool ShouldRecover(
            float secondsSinceSpend,
            float recoveryDelay)
        {
            return secondsSinceSpend >=
                   Math.Max(
                       0f,
                       recoveryDelay);
        }

        public static bool IsDepleted(
            float current)
        {
            return current <= 0f;
        }

        /// <summary>고갈 상태에서 다시 최면을 걸 수 있는 수준까지 회복됐는지 판정한다.</summary>
        public static bool CanResume(
            float current,
            float resumeThreshold)
        {
            return current >=
                   Math.Max(
                       0f,
                       resumeThreshold);
        }

        public static bool CanAfford(
            float current,
            float cost)
        {
            return current >=
                   Math.Max(
                       0f,
                       cost);
        }

        public static float Normalized(
            float current,
            float maximum)
        {
            float safeMaximum =
                Math.Max(
                    1f,
                    maximum);

            float value =
                current /
                safeMaximum;

            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f
                ? 1f
                : value;
        }
    }
}
