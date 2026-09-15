using System;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 기획서 13.1절 집중력 계산이다.
    ///
    /// 19일차에 역할을 바꿨다.
    ///   전: 집중력이 0이 되면 최면이 끊기고, 30까지 회복될 때까지 다시 걸 수 없었다
    ///   후: 집중력이 남아 있으면 최면이 빨라지고, 0이어도 기본 속도로 계속 걸린다
    ///
    /// 이전 방식은 최면 몇 번이면 집중력이 바닥나 한동안 아무것도 못 해서 흐름이 늘어졌다.
    /// 이제 집중력은 "막는 장치"가 아니라 "가속 자원"이다.
    /// 대시·최면 파동·체인 최면은 여전히 집중력을 써서, 가속과 기술 사이의 선택은 남는다.
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

        /// <summary>
        /// 집중력에 따른 최면 속도 배율이다.
        /// 조금이라도 남아 있으면 가속이 붙고, 0이면 기본 속도(1배)다.
        /// 0이어도 최면이 멈추지는 않는다.
        /// </summary>
        public static float GetHypnosisSpeedMultiplier(
            float current,
            float bonus)
        {
            return current > 0f
                ? 1f +
                  Math.Max(
                      0f,
                      bonus)
                : 1f;
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
