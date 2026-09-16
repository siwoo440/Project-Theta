using System;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 보스 함락 뒤 엔딩 연출의 시간표다 (29일차). 화면 없이 테스트할 수 있게 숫자만 다룬다.
    ///
    ///   0.0  함락 연출(빛기둥 · 흔들림)을 그대로 보여 준다
    ///   1.2  화면이 어두워진다
    ///   2.0  "ENDING" 제목, 이어서 문장이 한 줄씩 떠오른다
    ///   …    다 뜬 뒤 잠깐 머문다
    ///   끝   어둠이 걷히고 결과 화면이 나온다
    ///
    /// 문장이 뜨기 시작한 뒤에 클릭하면 바로 걷히는 단계로 건너뛴다.
    /// </summary>
    public static class EndingLogic
    {
        public const float WaitSeconds = 1.2f;
        public const float FadeInSeconds = 0.8f;
        public const float LineSeconds = 1.4f;
        public const float LineFadeSeconds = 0.5f;
        public const float HoldSeconds = 1.6f;
        public const float FadeOutSeconds = 0.6f;
        public const float BackdropAlpha = 0.88f;

        public const string Title = "ENDING";

        public static readonly string[] Lines =
        {
            "라이벌 서큐버스가 무릎을 꿇었다.",
            "루프탑의 음악이 멈추고, 도시의 밤이 조용해졌다.",
            "이 도시의 정기는 이제 당신의 것.",
            "하지만 밤은 또 온다. 가고 싶은 곳으로 계속 나아가자."
        };

        public static float LinesStart =>
            WaitSeconds + FadeInSeconds;

        public static float FadeOutStart =>
            LinesStart + Lines.Length * LineSeconds + HoldSeconds;

        public static float TotalSeconds =>
            FadeOutStart + FadeOutSeconds;

        /// <summary>검은 막의 불투명도다.</summary>
        public static float GetBackdropAlpha(
            float elapsed)
        {
            if (elapsed <= WaitSeconds)
            {
                return 0f;
            }

            if (elapsed < LinesStart)
            {
                return BackdropAlpha * Clamp01((elapsed - WaitSeconds) / FadeInSeconds);
            }

            if (elapsed < FadeOutStart)
            {
                return BackdropAlpha;
            }

            return BackdropAlpha * (1f - Clamp01((elapsed - FadeOutStart) / FadeOutSeconds));
        }

        /// <summary>
        /// 글자의 불투명도다. index −1은 제목, 0부터는 문장이다.
        /// 제목은 문장 단계가 시작될 때 함께 뜬다.
        /// </summary>
        public static float GetTextAlpha(
            float elapsed,
            int index)
        {
            float appear =
                LinesStart +
                Math.Max(0, index) * LineSeconds;

            float fadeIn = Clamp01((elapsed - appear) / LineFadeSeconds);

            if (elapsed < FadeOutStart)
            {
                return fadeIn;
            }

            return fadeIn * (1f - Clamp01((elapsed - FadeOutStart) / FadeOutSeconds));
        }

        public static bool IsFinished(
            float elapsed)
        {
            return elapsed >= TotalSeconds;
        }

        /// <summary>건너뛸 수 있는 때인지다. 문장이 뜨기 시작한 뒤, 걷히기 전이다.</summary>
        public static bool CanSkip(
            float elapsed)
        {
            return elapsed >= LinesStart &&
                   elapsed < FadeOutStart;
        }

        /// <summary>건너뛰면 걷히는 단계 처음으로 간다. 이미 지났으면 그대로다.</summary>
        public static float Skip(
            float elapsed)
        {
            return CanSkip(elapsed)
                ? FadeOutStart
                : elapsed;
        }

        private static float Clamp01(
            float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
