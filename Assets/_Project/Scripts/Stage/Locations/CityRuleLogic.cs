using System;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 지하철 열차 도착 계산이다 (24일차, 부록 B.3 [2]).
    ///
    ///   한 주기 45초. 주기 끝에 문이 열리고 5초 동안 인파가 쏟아진다.
    ///   생존 목표는 열차 3대가 지나갈 때까지 버티기다.
    /// </summary>
    public static class TrainLogic
    {
        public const float PeriodSeconds = 45f;
        public const float DoorOpenSeconds = 5f;
        public const float WarningSeconds = 4f;
        public const int DefaultRequiredTrains = 3;
        public const int MinimumCommuters = 3;
        public const int MaximumCommuters = 6;

        /// <summary>지금까지 도착한 열차 수다. 문이 열리는 순간 한 대로 센다.</summary>
        public static int GetArrivedCount(
            float elapsed)
        {
            if (elapsed < PeriodSeconds ||
                float.IsNaN(elapsed))
            {
                return 0;
            }

            return (int)(elapsed / PeriodSeconds);
        }

        /// <summary>문이 열려 인파가 쏟아지는 중인지다.</summary>
        public static bool IsDoorOpen(
            float elapsed)
        {
            if (GetArrivedCount(elapsed) == 0)
            {
                return false;
            }

            return elapsed % PeriodSeconds < DoorOpenSeconds;
        }

        public static bool IsArriving(
            float elapsed)
        {
            if (elapsed < 0f ||
                float.IsNaN(elapsed))
            {
                return false;
            }

            return elapsed % PeriodSeconds >= PeriodSeconds - WarningSeconds;
        }

        public static float SecondsUntilNext(
            float elapsed)
        {
            float safe =
                float.IsNaN(elapsed)
                    ? 0f
                    : Math.Max(0f, elapsed);

            return PeriodSeconds - safe % PeriodSeconds;
        }

        /// <summary>열차마다 내리는 인원이다. 뒤 열차일수록 붐빈다.</summary>
        public static int GetCommuterCount(
            int trainIndex)
        {
            return Math.Min(
                MaximumCommuters,
                MinimumCommuters + Math.Max(0, trainIndex));
        }

        /// <summary>필요한 열차 수가 제한 시간 안에 모두 도착하는지다.</summary>
        public static bool FitsTimeLimit(
            int requiredTrains,
            float timeLimitSeconds)
        {
            return requiredTrains * PeriodSeconds < timeLimitSeconds;
        }
    }

    /// <summary>헬스장 구역 종류다.</summary>
    public enum GymZoneKind
    {
        Workout = 0,
        Yoga = 1,
        Pool = 2
    }

    /// <summary>
    /// 헬스장 심박 규칙 계산이다 (24일차, 부록 B.3 [3]).
    ///
    ///   운동 구역  최면 ×1.4, 충동 ×1.5 (단체 PT 중 최면 ×1.3 추가)
    ///   요가실     최면 ×0.8, 충동 ×0 (단체 PT의 충동 가속도 막음)
    ///   수영장     ×1 (코치 능력 범위)
    /// </summary>
    public static class HeartRateLogic
    {
        public const float WorkoutHypnosisMultiplier = 1.4f;
        public const float WorkoutImpulseMultiplier = 1.5f;
        public const float YogaHypnosisMultiplier = 0.8f;
        public const float YogaImpulseMultiplier = 0f;

        public static float GetHypnosisMultiplier(
            GymZoneKind? zone,
            bool ptActive,
            float ptWorkoutMultiplier)
        {
            switch (zone)
            {
                case GymZoneKind.Workout:
                    return WorkoutHypnosisMultiplier *
                           (ptActive ? Math.Max(0f, ptWorkoutMultiplier) : 1f);

                case GymZoneKind.Yoga:
                    return YogaHypnosisMultiplier;

                default:
                    return 1f;
            }
        }

        /// <summary>
        /// 동행자 충동 배율이다. 서 있는 구역과 단체 PT 여부로 정한다.
        /// <paramref name="heartRateScale"/>는 운동 구역 배율에 곱하는 밸런스 값이다.
        /// </summary>
        public static float GetImpulseMultiplier(
            GymZoneKind? zone,
            bool ptActive,
            float ptImpulseMultiplier,
            float heartRateScale)
        {
            if (zone == GymZoneKind.Yoga)
            {
                return YogaImpulseMultiplier;
            }

            float result =
                zone == GymZoneKind.Workout
                    ? 1f + (WorkoutImpulseMultiplier - 1f) * Math.Max(0f, heartRateScale)
                    : 1f;

            return ptActive
                ? result * Math.Max(0f, ptImpulseMultiplier)
                : result;
        }
    }

    /// <summary>
    /// 장소 목표별 클리어 판정이다 (24일차).
    ///
    ///   생존(지하철)  열차를 모두 버티고 동행자가 1명 이상 남아 있으면 클리어. 정기 목표로는 끝나지 않는다.
    ///   그 외         기존 규칙(정기 목표 달성)
    /// </summary>
    public static class ObjectiveStateLogic
    {
        /// <summary>
        /// 보스 함락 목표까지 판정한다 (26일차).
        ///   보스(루프탑 클럽)  보스를 함락하면 클리어. 정기 목표로는 끝나지 않는다
        /// </summary>
        public static StageState Resolve(
            LocationObjective objective,
            float remainingTime,
            int currentEssence,
            int targetEssence,
            int currentHealth,
            int trainsArrived,
            int trainsRequired,
            int followerCount,
            bool bossDefeated)
        {
            if (objective != LocationObjective.Boss)
            {
                return Resolve(
                    objective,
                    remainingTime,
                    currentEssence,
                    targetEssence,
                    currentHealth,
                    trainsArrived,
                    trainsRequired,
                    followerCount);
            }

            if (currentHealth <= 0)
            {
                return StageState.FailedByHealth;
            }

            if (bossDefeated)
            {
                return StageState.Cleared;
            }

            return remainingTime <= 0f
                ? StageState.FailedByTime
                : StageState.Running;
        }

        public static StageState Resolve(
            LocationObjective objective,
            float remainingTime,
            int currentEssence,
            int targetEssence,
            int currentHealth,
            int trainsArrived,
            int trainsRequired,
            int followerCount)
        {
            if (objective != LocationObjective.Survival)
            {
                return StageRules.ResolveState(
                    remainingTime,
                    currentEssence,
                    targetEssence,
                    currentHealth);
            }

            if (currentHealth <= 0)
            {
                return StageState.FailedByHealth;
            }

            if (trainsArrived >= Math.Max(1, trainsRequired) &&
                followerCount > 0)
            {
                return StageState.Cleared;
            }

            if (remainingTime <= 0f)
            {
                return StageState.FailedByTime;
            }

            return StageState.Running;
        }
    }
}
