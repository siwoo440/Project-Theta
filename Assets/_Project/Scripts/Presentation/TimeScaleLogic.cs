using System;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 게임 시간 흐름을 정하는 규칙이다.
    ///
    /// 시간을 건드리는 것이 두 가지다.
    ///   완전 정지  카드 선택 화면 (17일차 GameplayPause)
    ///   멈칫      레벨업·힘겨루기 승리 순간의 짧은 슬로 (19일차 히트스톱)
    ///
    /// 레벨업하면 멈칫이 먼저 걸리고, 곧바로 카드 화면이 떠서 완전 정지가 걸린다.
    /// 멈칫이 끝나면서 시간을 1로 되돌리면 카드 화면이 떠 있는데 게임이 재개된다.
    /// 그래서 <b>완전 정지가 항상 이긴다.</b>
    /// </summary>
    public static class TimeScaleLogic
    {
        public const float DefaultHitStopScale = 0.05f;

        public static float Resolve(
            int pauseRequests,
            float hitStopRemaining,
            float hitStopScale)
        {
            if (pauseRequests > 0)
            {
                return 0f;
            }

            if (hitStopRemaining > 0f)
            {
                return Math.Max(
                    0f,
                    Math.Min(
                        1f,
                        hitStopScale));
            }

            return 1f;
        }

        public const float MinimumDebugSpeed = 0.1f;
        public const float MaximumDebugSpeed = 4f;

        /// <summary>
        /// 디버그 패널의 게임 속도(20일차)까지 곱한 최종 시간 배율이다.
        /// 완전 정지는 속도를 올려도 0으로 남는다. 곱하기라 0 × 2 = 0이다.
        /// </summary>
        public static float Resolve(
            int pauseRequests,
            float hitStopRemaining,
            float hitStopScale,
            float debugSpeed)
        {
            return Resolve(
                       pauseRequests,
                       hitStopRemaining,
                       hitStopScale) *
                   ClampDebugSpeed(
                       debugSpeed);
        }

        public static float ClampDebugSpeed(
            float speed)
        {
            if (float.IsNaN(speed))
            {
                return 1f;
            }

            return Math.Max(
                MinimumDebugSpeed,
                Math.Min(
                    MaximumDebugSpeed,
                    speed));
        }

        /// <summary>
        /// 멈칫을 새로 걸 때의 남은 시간이다.
        /// 여러 번 겹쳐도 더하지 않고 긴 쪽만 남긴다. 더하면 연속 체인 최면에서 멈칫이 길게 늘어진다.
        /// </summary>
        public static float Extend(
            float remaining,
            float duration)
        {
            return Math.Max(
                Math.Max(
                    0f,
                    remaining),
                Math.Max(
                    0f,
                    duration));
        }

        /// <summary>멈칫 남은 시간을 줄인다. 멈칫은 실제 시간으로 흐른다(느려진 게임 시간이 아니라).</summary>
        public static float Tick(
            float remaining,
            float unscaledDeltaTime)
        {
            return Math.Max(
                0f,
                remaining -
                Math.Max(
                    0f,
                    unscaledDeltaTime));
        }
    }
}
