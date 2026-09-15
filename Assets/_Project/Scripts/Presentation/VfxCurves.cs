using System;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 연출이 시간에 따라 어떻게 변하는지의 곡선이다.
    ///
    /// 모든 함수는 진행률 t(0~1)를 받는다.
    /// 연출은 눈으로만 확인하기 쉬운데, "끝났는데 투명도가 조금 남아서 흔적이 쌓이는" 류의 버그는
    /// 눈으로 잘 안 보인다. 그래서 곡선을 순수 계산으로 떼어 내 테스트로 고정한다.
    ///
    /// 규칙: 투명도 곡선은 t = 1에서 반드시 정확히 0이다.
    /// </summary>
    public static class VfxCurves
    {
        public static float Clamp01(
            float value)
        {
            return value < 0f
                ? 0f
                : value > 1f
                    ? 1f
                    : value;
        }

        public static float EaseOutCubic(
            float t)
        {
            float u =
                1f -
                Clamp01(t);

            return 1f -
                   u * u * u;
        }

        public static float EaseInQuad(
            float t)
        {
            float c =
                Clamp01(t);

            return c * c;
        }

        /// <summary>약 110%까지 튀었다가 제자리로 돌아온다. 17일차 카드 등장과 같은 곡선이다.</summary>
        public static float EaseOutBack(
            float t)
        {
            const float back = 1.70158f;

            float u =
                Clamp01(t) -
                1f;

            return 1f +
                   (back + 1f) * u * u * u +
                   back * u * u;
        }

        // 파문 ---------------------------------------------------------

        /// <summary>파문은 빠르게 퍼지다가 느려진다.</summary>
        public static float RippleScale(
            float t)
        {
            return 0.15f +
                   0.85f *
                   EaseOutCubic(t);
        }

        /// <summary>파문은 퍼지는 동안 계속 옅어진다.</summary>
        public static float RippleAlpha(
            float t)
        {
            float u =
                1f -
                Clamp01(t);

            return u * u;
        }

        // 빛기둥 --------------------------------------------------------

        /// <summary>처음 25% 동안 솟아오르고 그 뒤로 유지한다.</summary>
        public static float PillarHeight(
            float t)
        {
            return EaseOutCubic(
                Clamp01(t) /
                0.25f);
        }

        /// <summary>마지막 40% 동안 가늘어진다.</summary>
        public static float PillarWidth(
            float t)
        {
            float c =
                Clamp01(t);

            return c < 0.6f
                ? 1f
                : 1f -
                  (c - 0.6f) /
                  0.4f;
        }

        public static float PillarAlpha(
            float t)
        {
            return FadeOutTail(
                t,
                0.6f);
        }

        // 톡 튀어나오기 (하트·원) ----------------------------------------

        public static float PopScale(
            float t)
        {
            float c =
                Clamp01(t);

            return c < 0.35f
                ? EaseOutBack(
                    c /
                    0.35f)
                : 1f;
        }

        public static float PopAlpha(
            float t)
        {
            return FadeOutTail(
                t,
                0.65f);
        }

        // 떠오르는 글자 ---------------------------------------------------

        /// <summary>처음엔 빠르게 뜨고 점점 느려진다. rise는 최종 상승 높이다.</summary>
        public static float FloatRise(
            float t,
            float rise)
        {
            float c =
                Clamp01(t);

            return rise *
                   (1f - (1f - c) * (1f - c));
        }

        public static float FloatAlpha(
            float t)
        {
            return FadeOutTail(
                t,
                0.6f);
        }

        // 날아가는 글자 ---------------------------------------------------

        /// <summary>
        /// 2차 베지어 곡선이다. 출발점에서 위로 휘었다가 목표로 빨려 들어간다.
        /// 진행률에 가속을 걸어 목표 근처에서 빨라지게 한다.
        /// </summary>
        public static void Bezier(
            float startX,
            float startY,
            float controlX,
            float controlY,
            float endX,
            float endY,
            float t,
            out float x,
            out float y)
        {
            float p =
                EaseInQuad(t);

            float u =
                1f -
                p;

            x = u * u * startX +
                2f * u * p * controlX +
                p * p * endX;

            y = u * u * startY +
                2f * u * p * controlY +
                p * p * endY;
        }

        /// <summary>날아가는 동안은 보이고, 목표에 닿기 직전에만 사라진다.</summary>
        public static float FlightAlpha(
            float t)
        {
            return FadeOutTail(
                t,
                0.85f);
        }

        // 화면 흔들림 ----------------------------------------------------

        /// <summary>흔들림 세기다. 처음이 가장 세고 제곱으로 줄어든다. 시간이 끝나면 정확히 0이다.</summary>
        public static float ShakeAmplitude(
            float elapsed,
            float duration,
            float strength)
        {
            if (duration <= 0f ||
                elapsed >= duration)
            {
                return 0f;
            }

            float u =
                1f -
                Clamp01(
                    elapsed /
                    duration);

            return Math.Max(
                0f,
                strength) *
                   u *
                   u;
        }

        /// <summary>
        /// 흔들림 방향이다. 사인파 두 개를 섞어 규칙적으로 보이지 않게 한다.
        /// 난수를 쓰지 않아 같은 시각이면 같은 값이 나온다.
        /// </summary>
        public static void ShakeDirection(
            float time,
            out float x,
            out float y)
        {
            x = (float)(Math.Sin(time * 47.0) * 0.6 +
                        Math.Sin(time * 83.0) * 0.4);

            y = (float)(Math.Sin(time * 59.0 + 1.3) * 0.6 +
                        Math.Sin(time * 71.0 + 0.4) * 0.4);
        }

        // 화면 가장자리 경고 ----------------------------------------------

        /// <summary>폭주 위험 테두리의 맥동이다. intensity가 0이면 완전히 사라진다.</summary>
        public static float DangerPulse(
            float time,
            float intensity)
        {
            float level =
                Clamp01(intensity);

            if (level <= 0f)
            {
                return 0f;
            }

            float wave =
                (float)(Math.Sin(time * 7.0) * 0.5 + 0.5);

            return level *
                   (0.35f + 0.4f * wave);
        }

        // 공통 ---------------------------------------------------------

        /// <summary>tailStart까지 완전히 보이다가 그 뒤로 선형으로 사라진다. t = 1에서 0이다.</summary>
        public static float FadeOutTail(
            float t,
            float tailStart)
        {
            float c =
                Clamp01(t);

            float start =
                Clamp01(tailStart);

            if (c <= start)
            {
                return 1f;
            }

            if (start >= 1f)
            {
                return c >= 1f
                    ? 0f
                    : 1f;
            }

            return 1f -
                   (c - start) /
                   (1f - start);
        }
    }
}
