using System;

namespace ProjectTheta.Disruptors
{
    /// <summary>구역 경계도의 단계다 (기획서 부록 C.2.1).</summary>
    public enum AlertLevel
    {
        Calm = 0,
        Caution = 1,
        Alert = 2,
        Emergency = 3
    }

    /// <summary>
    /// 구역 경계도 계산이다 (22일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   평온 0~29   효과 없음
    ///   주의 30~59  감시자 순찰 속도 +20%, 중립 NPC 최면 저항 +10%
    ///   경계 60~89  감시자가 마지막 발견 지점으로 이동, 특수 능력 재사용 -30%
    ///   비상 90~100 증원 1명(구역당 최대 2회), 회수 지점 10초 잠김
    /// </summary>
    public static class ZoneAlertLogic
    {
        public const float Maximum = 100f;
        public const float CautionThreshold = 30f;
        public const float AlertThreshold = 60f;
        public const float EmergencyThreshold = 90f;

        /// <summary>비상이 터진 뒤 경계도를 이 값으로 내린다. 그래야 다시 올라 두 번째 비상이 가능하다.</summary>
        public const float AfterEmergencyValue = 60f;

        public const int MaximumReinforcements = 2;

        public const float RecoveryLockSeconds = 10f;

        public const float CautionPatrolSpeedMultiplier = 1.2f;
        public const float CautionHypnosisMultiplier = 0.9f;
        public const float AlertAbilityCooldownMultiplier = 0.7f;

        /// <summary>파동을 감시자에게 맞히면 내려가는 양이다.</summary>
        public const float WaveRelief = 15f;

        /// <summary>파동을 맞은 방해 세력이 멍해지는 시간이다. 멍한 동안 특수 능력 시간도 멈춘다.</summary>
        public const float WaveStunSeconds = 3f;

        public static AlertLevel GetLevel(
            float value)
        {
            if (value >= EmergencyThreshold)
            {
                return AlertLevel.Emergency;
            }

            if (value >= AlertThreshold)
            {
                return AlertLevel.Alert;
            }

            if (value >= CautionThreshold)
            {
                return AlertLevel.Caution;
            }

            return AlertLevel.Calm;
        }

        public static float Add(
            float value,
            float amount)
        {
            if (float.IsNaN(amount))
            {
                return Clamp(value);
            }

            return Clamp(value + amount);
        }

        /// <summary>발각이 없으면 초당 일정량씩 내려간다.</summary>
        public static float Decay(
            float value,
            float perSecond,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return Clamp(value);
            }

            return Clamp(
                value -
                Math.Max(0f, perSecond) *
                deltaTime);
        }

        public static bool CanReinforce(
            int used)
        {
            return used < MaximumReinforcements;
        }

        /// <summary>주의 단계부터 플레이어 최면이 조금 느려진다(중립 NPC가 경계한다).</summary>
        public static float GetHypnosisMultiplier(
            AlertLevel level)
        {
            return level >= AlertLevel.Caution
                ? CautionHypnosisMultiplier
                : 1f;
        }

        public static float GetPatrolSpeedMultiplier(
            AlertLevel level)
        {
            return level >= AlertLevel.Caution
                ? CautionPatrolSpeedMultiplier
                : 1f;
        }

        public static float GetAbilityCooldownMultiplier(
            AlertLevel level)
        {
            return level >= AlertLevel.Alert
                ? AlertAbilityCooldownMultiplier
                : 1f;
        }

        public static string GetLabel(
            AlertLevel level)
        {
            switch (level)
            {
                case AlertLevel.Caution:
                    return "주의";

                case AlertLevel.Alert:
                    return "경계";

                case AlertLevel.Emergency:
                    return "비상";

                default:
                    return "평온";
            }
        }

        private static float Clamp(
            float value)
        {
            if (float.IsNaN(value))
            {
                return 0f;
            }

            return Math.Max(
                0f,
                Math.Min(
                    Maximum,
                    value));
        }
    }

    /// <summary>
    /// 감시 시야 판정이다 (22일차, 부록 C.2.2).
    /// 방해 세력은 사이드뷰 캐릭터라 왼쪽 · 오른쪽 중 한 방향만 본다.
    /// </summary>
    public static class DetectionLogic
    {
        /// <summary>시야 안에서 이만큼 계속 보이면 `?`가 `!`로 바뀐다.</summary>
        public const float SpotSeconds = 0.8f;

        /// <summary>동행자 한 명마다 발각이 이만큼 빨라진다. 많이 끌고 다닐수록 눈에 띈다.</summary>
        public const float FollowerSpotBonus = 0.05f;

        /// <summary>
        /// 대상이 시야 부채꼴 안인지다.
        /// <paramref name="facing"/>은 +1(오른쪽) 또는 -1(왼쪽), 각도는 부채꼴 전체 각도의 절반이다.
        /// </summary>
        public static bool IsInSight(
            float originX,
            float originY,
            int facing,
            float targetX,
            float targetY,
            float halfAngleDegrees,
            float range)
        {
            float dx = targetX - originX;
            float dy = targetY - originY;

            float distanceSquared = dx * dx + dy * dy;

            if (range <= 0f ||
                distanceSquared > range * range)
            {
                return false;
            }

            // 발밑에 겹쳐 서 있으면 방향과 상관없이 보인다.
            if (distanceSquared < 0.0001f)
            {
                return true;
            }

            float forward =
                dx * (facing >= 0 ? 1f : -1f);

            if (forward <= 0f)
            {
                return false;
            }

            double angle =
                Math.Atan2(
                    Math.Abs(dy),
                    forward) *
                180.0 /
                Math.PI;

            return angle <= Math.Max(0f, halfAngleDegrees);
        }

        public static float GetSpotRate(
            int followerCount)
        {
            return 1f +
                   FollowerSpotBonus *
                   Math.Max(0, followerCount);
        }

        /// <summary>
        /// 발각 진행도(0~1)를 흘린다. 수상한 행동이 보이면 오르고, 아니면 그 두 배 속도로 내려간다.
        /// 1에 닿으면 발각이다.
        /// </summary>
        public static float Advance(
            float progress,
            bool suspiciousInSight,
            int followerCount,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return Clamp01(progress);
            }

            float step =
                deltaTime /
                SpotSeconds;

            return suspiciousInSight
                ? Clamp01(progress + step * GetSpotRate(followerCount))
                : Clamp01(progress - step * 2f);
        }

        private static float Clamp01(
            float value)
        {
            return float.IsNaN(value)
                ? 0f
                : Math.Max(0f, Math.Min(1f, value));
        }
    }

    /// <summary>특수 능력의 진행 단계다.</summary>
    public enum AbilityPhase
    {
        Ready = 0,
        Telegraph = 1,
        Cooldown = 2
    }

    /// <summary>특수 능력의 시간 상태다. 값 복사로 다뤄 테스트하기 쉽게 했다.</summary>
    public struct AbilityTimer
    {
        public AbilityPhase Phase;
        public float Remaining;
    }

    /// <summary>
    /// 특수 능력 시간 흐름이다 (22일차, 부록 C.2.4).
    ///
    ///   대기 → (조건 충족) → 예고 → (예고 끝) 발동 → 재사용 대기 → 대기
    ///
    /// <b>예고 없는 능력은 만들 수 없다.</b> 예고 시간은 설정값과 상관없이 최소 1초다.
    /// 멍한 동안은 예고도 재사용 대기도 멈춘다.
    /// </summary>
    public static class SpecialAbilityLogic
    {
        public const float MinimumTelegraphSeconds = 1f;

        public static float SanitizeTelegraph(
            float seconds)
        {
            return float.IsNaN(seconds)
                ? MinimumTelegraphSeconds
                : Math.Max(MinimumTelegraphSeconds, seconds);
        }

        /// <summary>
        /// 한 프레임을 흘리고, 이번 프레임에 발동했으면 true를 돌려준다.
        /// 예고 중에 조건이 사라져도 예고는 끝까지 간다. 발동 시점에 효과가 대상을 다시 찾는다.
        /// </summary>
        public static bool Tick(
            ref AbilityTimer timer,
            float deltaTime,
            bool stunned,
            bool wantsToStart,
            float telegraphSeconds,
            float cooldownSeconds,
            AlertLevel alertLevel)
        {
            if (stunned ||
                deltaTime <= 0f)
            {
                return false;
            }

            switch (timer.Phase)
            {
                case AbilityPhase.Ready:
                    if (wantsToStart)
                    {
                        timer.Phase = AbilityPhase.Telegraph;
                        timer.Remaining = SanitizeTelegraph(telegraphSeconds);
                    }

                    return false;

                case AbilityPhase.Telegraph:
                    timer.Remaining -= deltaTime;

                    if (timer.Remaining > 0f)
                    {
                        return false;
                    }

                    timer.Phase = AbilityPhase.Cooldown;
                    timer.Remaining =
                        Math.Max(0f, cooldownSeconds) *
                        ZoneAlertLogic.GetAbilityCooldownMultiplier(alertLevel);

                    return true;

                default:
                    timer.Remaining -= deltaTime;

                    if (timer.Remaining <= 0f)
                    {
                        timer.Phase = AbilityPhase.Ready;
                        timer.Remaining = 0f;
                    }

                    return false;
            }
        }

        /// <summary>예고 진행률(0~1)이다. 머리 위 원형 표시에 쓴다.</summary>
        public static float GetTelegraphProgress(
            AbilityTimer timer,
            float telegraphSeconds)
        {
            if (timer.Phase != AbilityPhase.Telegraph)
            {
                return 0f;
            }

            float total =
                SanitizeTelegraph(
                    telegraphSeconds);

            return Math.Max(
                0f,
                Math.Min(
                    1f,
                    1f - timer.Remaining / total));
        }
    }

    /// <summary>
    /// 기업 연수원의 쉬는 시간 종이다 (22일차, 부록 B.3 [0]).
    /// 주기마다 짧은 쉬는 시간이 오고, 그동안 복도 NPC가 빨라진다.
    /// </summary>
    public static class BreakTimeLogic
    {
        public const float PeriodSeconds = 40f;
        public const float BreakSeconds = 8f;
        public const float NpcSpeedMultiplier = 1.6f;

        /// <summary>판 시작 직후에는 종이 울리지 않는다. 첫 종은 한 주기 뒤다.</summary>
        public static bool IsBreak(
            float elapsed)
        {
            if (elapsed < PeriodSeconds)
            {
                return false;
            }

            float phase =
                (elapsed - PeriodSeconds) %
                PeriodSeconds;

            return phase < BreakSeconds;
        }

        public static float GetNpcSpeedMultiplier(
            float elapsed)
        {
            return IsBreak(elapsed)
                ? NpcSpeedMultiplier
                : 1f;
        }
    }
}
