using System;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 시선 회피형 NPC가 최면 연결을 끊는 주기를 계산한다.
    /// 한 주기 안에서 앞쪽 일정 구간 동안만 차단 상태가 된다.
    /// </summary>
    public static class NpcGazeAverterLogic
    {
        public const float DefaultCycleDuration = 2.4f;
        public const float DefaultBlockDuration = 0.9f;

        public static bool IsBlocking(
            float elapsedSeconds,
            float cycleDuration,
            float blockDuration)
        {
            float safeCycle =
                Math.Max(
                    0.01f,
                    cycleDuration);

            float safeBlock =
                Math.Max(
                    0f,
                    Math.Min(
                        blockDuration,
                        safeCycle));

            if (safeBlock <= 0f)
            {
                return false;
            }

            float safeElapsed =
                Math.Max(
                    0f,
                    elapsedSeconds);

            float phase =
                safeElapsed -
                ((float)Math.Floor(
                     safeElapsed /
                     safeCycle) *
                 safeCycle);

            return phase <
                   safeBlock;
        }
    }
}
