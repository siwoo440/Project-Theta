using System;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 결과 화면 연출 타이밍을 계산한다.
    ///
    /// 각 항목이 순서대로 "딱" 하고 튀어나오고, 모두 나온 뒤 랭크가 도장처럼 찍힌다.
    /// 시간만 넣으면 상태가 나오는 순수 함수로 두어, 스킵 처리는 경과 시간을
    /// 전체 길이로 바꿔주기만 하면 되도록 했다.
    /// </summary>
    public static class StageResultRevealLogic
    {
        public const float DefaultRowInterval = 0.26f;
        public const float DefaultPopDuration = 0.18f;
        public const float DefaultRankDelay = 0.40f;
        public const float DefaultRankDuration = 0.26f;

        /// <summary>항목이 튀어나올 때의 초기 확대량이다.</summary>
        public const float RowPopOvershoot = 0.45f;

        /// <summary>랭크 도장이 찍힐 때의 초기 확대량이다.</summary>
        public const float RankStampOvershoot = 1.60f;

        public static float GetRowAppearTime(
            int rowIndex,
            float rowInterval)
        {
            return Math.Max(
                       0,
                       rowIndex) *
                   Math.Max(
                       0f,
                       rowInterval);
        }

        public static bool IsRowVisible(
            int rowIndex,
            float elapsed,
            float rowInterval)
        {
            return elapsed >=
                   GetRowAppearTime(
                       rowIndex,
                       rowInterval);
        }

        public static int GetVisibleRowCount(
            float elapsed,
            int rowCount,
            float rowInterval)
        {
            int safeRowCount =
                Math.Max(
                    0,
                    rowCount);

            float safeInterval =
                Math.Max(
                    0.0001f,
                    rowInterval);

            if (elapsed < 0f)
            {
                return 0;
            }

            int visible =
                (int)Math.Floor(
                    elapsed /
                    safeInterval) +
                1;

            return Math.Min(
                safeRowCount,
                Math.Max(
                    0,
                    visible));
        }

        /// <summary>
        /// 항목의 현재 확대 배율이다.
        /// 아직 등장 전이면 0을 반환하므로 호출부는 그리지 않으면 된다.
        /// </summary>
        public static float GetRowScale(
            int rowIndex,
            float elapsed,
            float rowInterval,
            float popDuration)
        {
            float appearTime =
                GetRowAppearTime(
                    rowIndex,
                    rowInterval);

            if (elapsed < appearTime)
            {
                return 0f;
            }

            float safePop =
                Math.Max(
                    0.0001f,
                    popDuration);

            float t =
                Clamp01(
                    (elapsed - appearTime) /
                    safePop);

            float remaining =
                1f - t;

            return 1f +
                   (RowPopOvershoot *
                    remaining *
                    remaining);
        }

        public static float GetRankStartTime(
            int rowCount,
            float rowInterval,
            float rankDelay)
        {
            return GetRowAppearTime(
                       Math.Max(
                           0,
                           rowCount - 1),
                       rowInterval) +
                   Math.Max(
                       0f,
                       rankDelay);
        }

        /// <summary>랭크 도장 진행도다. 0이면 아직 등장 전, 1이면 완전히 찍힌 상태다.</summary>
        public static float GetRankProgress(
            float elapsed,
            int rowCount,
            float rowInterval,
            float rankDelay,
            float rankDuration)
        {
            float start =
                GetRankStartTime(
                    rowCount,
                    rowInterval,
                    rankDelay);

            if (elapsed < start)
            {
                return 0f;
            }

            float safeDuration =
                Math.Max(
                    0.0001f,
                    rankDuration);

            return Clamp01(
                (elapsed - start) /
                safeDuration);
        }

        /// <summary>도장이 크게 시작해 빠르게 내려앉는 곡선이다.</summary>
        public static float GetRankScale(
            float progress)
        {
            float t =
                Clamp01(
                    progress);

            float remaining =
                1f - t;

            return 1f +
                   (RankStampOvershoot *
                    remaining *
                    remaining *
                    remaining);
        }

        public static float GetRankAlpha(
            float progress)
        {
            return Clamp01(
                Clamp01(
                    progress) *
                2.5f);
        }

        public static float GetTotalDuration(
            int rowCount,
            float rowInterval,
            float rankDelay,
            float rankDuration)
        {
            return GetRankStartTime(
                       rowCount,
                       rowInterval,
                       rankDelay) +
                   Math.Max(
                       0f,
                       rankDuration);
        }

        public static bool IsComplete(
            float elapsed,
            int rowCount,
            float rowInterval,
            float rankDelay,
            float rankDuration)
        {
            return elapsed >=
                   GetTotalDuration(
                       rowCount,
                       rowInterval,
                       rankDelay,
                       rankDuration);
        }

        private static float Clamp01(
            float value)
        {
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
