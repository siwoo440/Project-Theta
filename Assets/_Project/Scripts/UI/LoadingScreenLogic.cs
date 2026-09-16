using System;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 로딩창의 계산이다 (28일차). 화면 없이 테스트할 수 있게 숫자만 다룬다.
    ///
    ///   주인공      가운데 로딩 바 위를 달린다. 바 왼쪽 끝에서 출발해, 차오르는 끝을 따라가다 100%에서 바 오른쪽 끝에 도착한다.
    ///   서큐버스    주인공 왼쪽 위 자리를 살짝 늦게 따라가며 위아래로 둥실거린다.
    ///   로딩 바     실제 진행률을 부드럽게 따라가며 절대 줄어들지 않는다.
    ///   Tip         몇 초마다 다른 문장으로 바뀐다. 같은 문장이 연달아 나오지 않는다.
    /// </summary>
    public static class LoadingScreenLogic
    {
        /// <summary>로딩이 아무리 빨라도 이만큼은 보여 깜빡임을 막는다.</summary>
        public const float MinVisibleSeconds = 1.6f;

        /// <summary>오른쪽 끝에 도착한 뒤 잠깐 서 있는 시간이다. 도착한 모습이 보이게 한다.</summary>
        public const float ArrivalHoldSeconds = 0.35f;

        public const float FadeSeconds = 0.25f;

        /// <summary>가운데 로딩 바의 가로 길이(기준 해상도 px)다.</summary>
        public const float BarWidth = 560f;

        /// <summary>주인공이 달리는 가로 폭이다. 로딩 바와 같아서, 주인공이 바가 차오르는 끝 위에 선다.</summary>
        public const float RunWidth = BarWidth;

        public const float RunFramesPerSecond = 10f;

        /// <summary>서큐버스가 서는 자리다. 주인공 기준 왼쪽 위.</summary>
        public const float CompanionOffsetX = -130f;

        public const float CompanionOffsetY = 110f;

        public const float CompanionFollowSharpness = 6f;

        public const float CompanionBobHeight = 12f;

        public const float CompanionBobSpeed = 3f;

        /// <summary>로딩 바가 목표를 따라가는 속도(초당 비율)다.</summary>
        public const float BarFillSpeed = 0.7f;

        public const float TipSeconds = 3f;

        /// <summary>
        /// 주인공의 가로 위치다. 가운데가 0이고, 로딩 바 0% = 왼쪽 끝(−폭/2), 100% = 오른쪽 끝(+폭/2)이다.
        /// 바가 줄어들지 않으므로 주인공도 뒤로 가지 않는다.
        /// </summary>
        public static float GetRunnerX(
            float displayedProgress)
        {
            return (Clamp01(displayedProgress) - 0.5f) * RunWidth;
        }

        /// <summary>도착했으면 달리기를 멈추고 서 있는다.</summary>
        public static bool HasArrived(
            float displayedProgress)
        {
            return IsBarFull(displayedProgress);
        }

        /// <summary>달리기 그림 번호다.</summary>
        public static int GetRunFrame(
            float elapsed,
            int frameCount)
        {
            if (frameCount <= 0)
            {
                return 0;
            }

            int frame = (int)(Math.Max(0f, elapsed) * RunFramesPerSecond);

            return frame % frameCount;
        }

        public static float GetCompanionTargetX(
            float runnerX)
        {
            return runnerX + CompanionOffsetX;
        }

        public static float GetCompanionTargetY(
            float runnerY,
            float elapsed)
        {
            return runnerY + CompanionOffsetY +
                   (float)Math.Sin(elapsed * CompanionBobSpeed) * CompanionBobHeight;
        }

        /// <summary>현재 자리에서 목표 자리로 부드럽게 다가간다. 프레임 수와 상관없이 같은 속도다.</summary>
        public static float Follow(
            float current,
            float target,
            float deltaTime)
        {
            float t = 1f - (float)Math.Exp(-CompanionFollowSharpness * Math.Max(0f, deltaTime));

            return current + (target - current) * t;
        }

        /// <summary>Unity의 비동기 로딩 진행률(0~0.9에서 멈춤)을 0~1로 바꾼다.</summary>
        public static float NormalizeProgress(
            float operationProgress,
            bool isDone)
        {
            if (isDone)
            {
                return 1f;
            }

            return Clamp01(operationProgress / 0.9f);
        }

        /// <summary>화면에 보이는 로딩 바 값이다. 목표를 따라 오르기만 한다.</summary>
        public static float StepDisplayed(
            float displayed,
            float target,
            float deltaTime)
        {
            float clampedTarget = Clamp01(target);

            if (clampedTarget <= displayed)
            {
                return Clamp01(displayed);
            }

            return Math.Min(clampedTarget, displayed + BarFillSpeed * Math.Max(0f, deltaTime));
        }

        /// <summary>새 화면으로 넘어가도 되는지다. 다 불러왔고 최소 시간이 지났어야 한다.</summary>
        public static bool CanActivate(
            float loadProgress,
            float elapsed)
        {
            return loadProgress >= 1f &&
                   elapsed >= MinVisibleSeconds;
        }

        /// <summary>로딩 바가 끝까지 찼는지다. 바가 100%를 보여 준 뒤에 사라지게 한다.</summary>
        public static bool IsBarFull(
            float displayed)
        {
            return displayed >= 0.999f;
        }

        public static string FormatPercent(
            float displayed)
        {
            return $"{(int)Math.Round(Clamp01(displayed) * 100f)}%";
        }

        /// <summary>다음 Tip 번호다. 지금 문장과 다른 번호를 고른다.</summary>
        public static int NextTipIndex(
            int current,
            int count,
            int randomValue)
        {
            if (count <= 1)
            {
                return 0;
            }

            // 처음 고를 때는 아무 문장이나 고른다.
            if (current < 0 || current >= count)
            {
                return Math.Abs(randomValue % count);
            }

            int offset = 1 + Math.Abs(randomValue % (count - 1));

            return (current + offset) % count;
        }

        private static float Clamp01(
            float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
