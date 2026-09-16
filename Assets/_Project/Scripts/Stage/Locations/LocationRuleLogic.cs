using System;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>밀물 · 썰물 단계다.</summary>
    public enum TidePhase
    {
        Low = 0,
        Warning = 1,
        High = 2
    }

    /// <summary>
    /// 해변가 밀물 · 썰물 계산이다 (23일차, 부록 B.3 [1]).
    ///
    ///   한 주기 30초 = 썰물 19초 → 예고 3초 → 밀물 8초
    ///   밀물 동안 바닷가 쪽(화면 아래쪽) 줄이 물에 잠겨 서 있을 수 없다.
    /// </summary>
    public static class TideLogic
    {
        public const float PeriodSeconds = 30f;
        public const float WarningSeconds = 3f;
        public const float HighSeconds = 8f;

        /// <summary>층 기준 세로 좌표가 이보다 아래면 밀물 때 물에 잠긴다. 걷는 폭(-5.2 ~ 0.9)의 아래쪽 약 1/4이다.</summary>
        public const float FloodTopLocalY = -3.6f;

        public static TidePhase GetPhase(
            float elapsed)
        {
            if (elapsed < 0f ||
                float.IsNaN(elapsed))
            {
                return TidePhase.Low;
            }

            float position =
                elapsed % PeriodSeconds;

            if (position >= PeriodSeconds - HighSeconds)
            {
                return TidePhase.High;
            }

            if (position >= PeriodSeconds - HighSeconds - WarningSeconds)
            {
                return TidePhase.Warning;
            }

            return TidePhase.Low;
        }

        /// <summary>다음 밀물까지 남은 시간이다. 밀물 중이면 0이다.</summary>
        public static float SecondsUntilHigh(
            float elapsed)
        {
            if (GetPhase(elapsed) == TidePhase.High)
            {
                return 0f;
            }

            float safe =
                Math.Max(0f, float.IsNaN(elapsed) ? 0f : elapsed);

            float position =
                safe % PeriodSeconds;

            return PeriodSeconds - HighSeconds - position;
        }

        /// <summary>밀물이 빠지기까지 남은 시간이다. 밀물이 아니면 0이다.</summary>
        public static float SecondsUntilLow(
            float elapsed)
        {
            if (GetPhase(elapsed) != TidePhase.High)
            {
                return 0f;
            }

            return PeriodSeconds - elapsed % PeriodSeconds;
        }

        public static bool IsFlooded(
            TidePhase phase,
            float localY)
        {
            return phase == TidePhase.High &&
                   localY < FloodTopLocalY;
        }
    }

    /// <summary>
    /// 둥근 영역 판정이다 (23일차). 파라솔 그늘 · 등불 빛에 쓴다.
    /// 사이드뷰라 바닥 영역은 가로로 긴 타원이다.
    /// </summary>
    public static class AreaLogic
    {
        public static bool IsInsideEllipse(
            float dx,
            float dy,
            float radiusX,
            float radiusY)
        {
            if (radiusX <= 0f ||
                radiusY <= 0f)
            {
                return false;
            }

            float nx = dx / radiusX;
            float ny = dy / radiusY;

            return nx * nx + ny * ny <= 1f;
        }
    }

    /// <summary>
    /// 야시장 어둠과 등불 계산이다 (23일차, 부록 B.3 [4]).
    /// 불빛 안에서는 최면 사거리가 온전하고, 어두운 곳에서는 줄어든다.
    /// </summary>
    public static class LanternLogic
    {
        public const float DefaultDarkRangeScale = 0.7f;

        /// <summary>빛의 세로 반경은 가로의 이 비율이다.</summary>
        public const float VerticalRatio = 0.6f;

        public static float GetRangeMultiplier(
            bool darknessActive,
            bool lit,
            float darkRangeScale)
        {
            if (!darknessActive ||
                lit)
            {
                return 1f;
            }

            return float.IsNaN(darkRangeScale)
                ? DefaultDarkRangeScale
                : Math.Max(0.1f, Math.Min(1f, darkRangeScale));
        }
    }

    /// <summary>
    /// 장소 대표 목표가 회수 정산에 주는 배율이다 (23일차).
    ///
    ///   동시 운반(해변가)  한 번에 3명 이상 회수하면 ×1.2, 1~2명씩 나눠 오면 ×0.5
    ///   그 외             ×1
    /// </summary>
    public static class LocationObjectiveLogic
    {
        public const int GroupCarryMinimum = 3;
        public const float GroupCarryBonusMultiplier = 1.2f;
        public const float GroupCarrySmallBatchMultiplier = 0.5f;

        public static float GetBatchMultiplier(
            LocationObjective objective,
            int batchCount)
        {
            if (objective != LocationObjective.GroupCarry ||
                batchCount <= 0)
            {
                return 1f;
            }

            return batchCount >= GroupCarryMinimum
                ? GroupCarryBonusMultiplier
                : GroupCarrySmallBatchMultiplier;
        }

        /// <summary>HUD에 띄울 목표 안내다. 안내할 게 없으면 빈 글자다.</summary>
        public static string GetHint(
            LocationObjective objective)
        {
            return objective == LocationObjective.GroupCarry
                ? $"동시 운반: {GroupCarryMinimum}명 이상 한 번에 회수하면 정기 ×{GroupCarryBonusMultiplier:0.0}, 적으면 ×{GroupCarrySmallBatchMultiplier:0.0}"
                : string.Empty;
        }
    }
}
