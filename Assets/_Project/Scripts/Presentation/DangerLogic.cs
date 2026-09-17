using System;

namespace ProjectTheta.Presentation
{
    /// <summary>화면 가장자리 붉은 테두리를 정하는 재료다 (34일차).</summary>
    public struct DangerInputs
    {
        /// <summary>폭주 쪽 세기(0~1)다. 19일차부터 있던 값이다.</summary>
        public float Rampage;

        /// <summary>체력 비율(0~1)이다.</summary>
        public float HealthNormalized;

        /// <summary>경쟁자가 내 동행자를 빼앗는 게이지 중 가장 높은 값(0~1)이다.</summary>
        public float StealProgress;

        /// <summary>회수 지점이 잠겼는지다.</summary>
        public bool RecoveryLocked;

        /// <summary>장소가 아직 진행 중인지다(탈출 가능 포함).</summary>
        public bool Running;

        /// <summary>남은 시간(초)이다.</summary>
        public float RemainingSeconds;
    }

    /// <summary>
    /// 위기 화면 효과의 세기를 정한다 (34일차).
    ///
    ///   체력 30% 이하          0.35 → 0.8 (낮을수록 강하게)
    ///   동행자 빼앗기기 직전   게이지 50%부터 0.3 → 0.8
    ///   회수 지점 잠김         0.35
    ///   남은 시간 15초 이하    0.3 → 0.7
    ///   폭주                   기존 값(0.6 · 0.8 · 1)
    /// 여러 위기가 겹치면 가장 급한 것 하나만 따른다(더하면 화면이 너무 붉어진다).
    /// 설정에서 끄면 폭주를 포함해 모두 0이다.
    /// </summary>
    public static class DangerLogic
    {
        public const float LowHealth = 0.3f;
        public const float StealWarning = 0.5f;
        public const float TimeWarningSeconds = 15f;
        public const float LockedIntensity = 0.35f;

        public static float FromHealth(
            float normalized)
        {
            if (float.IsNaN(normalized) ||
                normalized <= 0f ||
                normalized > LowHealth)
            {
                return 0f;
            }

            return 0.35f + 0.45f * (1f - normalized / LowHealth);
        }

        public static float FromSteal(
            float progress)
        {
            if (float.IsNaN(progress) ||
                progress < StealWarning)
            {
                return 0f;
            }

            float t =
                (Math.Min(1f, progress) - StealWarning) /
                (1f - StealWarning);

            return 0.3f + 0.5f * t;
        }

        public static float FromTime(
            float remaining,
            bool running)
        {
            if (!running ||
                float.IsNaN(remaining) ||
                remaining <= 0f ||
                remaining > TimeWarningSeconds)
            {
                return 0f;
            }

            return 0.3f + 0.4f * (1f - remaining / TimeWarningSeconds);
        }

        public static float Resolve(
            DangerInputs inputs,
            bool enabled)
        {
            if (!enabled)
            {
                return 0f;
            }

            float rampage =
                float.IsNaN(inputs.Rampage)
                    ? 0f
                    : Math.Max(0f, Math.Min(1f, inputs.Rampage));

            if (!inputs.Running)
            {
                return rampage;
            }

            float result = rampage;

            result = Math.Max(result, FromHealth(inputs.HealthNormalized));
            result = Math.Max(result, FromSteal(inputs.StealProgress));
            result = Math.Max(result, FromTime(inputs.RemainingSeconds, true));

            if (inputs.RecoveryLocked)
            {
                result = Math.Max(result, LockedIntensity);
            }

            return result;
        }
    }
}
