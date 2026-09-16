using System;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 쇼핑몰 폐점 방송 계산이다 (25일차).
    /// 제한 시간의 70%가 지나면 방송이 나오고, 이후 시간이 빨리 흐른다.
    /// </summary>
    public static class ClosingLogic
    {
        public const float AnnounceAt = 0.7f;
        public const float DefaultTimeScale = 1.3f;

        public static bool ShouldAnnounce(
            float elapsed,
            float timeLimit)
        {
            if (timeLimit <= 0f ||
                float.IsNaN(elapsed))
            {
                return false;
            }

            return elapsed >= timeLimit * AnnounceAt;
        }
    }

    /// <summary>
    /// 오피스 정전 계산이다 (25일차).
    /// 제한 시간의 35% · 70% 지점에 한 번씩, 3초 예고 뒤 8초 동안 꺼진다.
    /// </summary>
    public static class BlackoutLogic
    {
        public static readonly float[] StartAt =
        {
            0.35f, 0.70f
        };

        public const float WarningSeconds = 3f;
        public const float DarkSeconds = 8f;
        public const float RangeMultiplier = 0.7f;
        public const float ShadeAlpha = 0.55f;

        public static int TimesPerZone =>
            StartAt.Length;

        /// <summary><paramref name="nextIndex"/>번째 정전을 시작할 때인지다.</summary>
        public static bool ShouldStart(
            int nextIndex,
            float elapsed,
            float timeLimit)
        {
            if (nextIndex < 0 ||
                nextIndex >= StartAt.Length ||
                timeLimit <= 0f ||
                float.IsNaN(elapsed))
            {
                return false;
            }

            return elapsed >= timeLimit * StartAt[nextIndex];
        }

        /// <summary>정전 중에도 볼 수 있는 감시자만 본다.</summary>
        public static bool CanSee(
            bool blackout,
            bool seesInBlackout)
        {
            return !blackout || seesInBlackout;
        }
    }

    /// <summary>
    /// 쇼핑몰 잠입 목표 계산이다 (25일차).
    /// 이번 구역에서 한 번도 발각되지 않은 채 회수하면 그 묶음 정기에 보너스가 붙는다.
    /// </summary>
    public static class StealthLogic
    {
        public const float UndetectedMultiplier = 1.3f;

        public static float GetBatchMultiplier(
            LocationObjective objective,
            bool detected)
        {
            return objective == LocationObjective.Stealth &&
                   !detected
                ? UndetectedMultiplier
                : 1f;
        }
    }
}
