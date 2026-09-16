using System;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 쟁탈 역할(헌팅남) 계산이다 (23일차, 부록 C.3 [1]).
    /// 목표 동행자 곁에 붙어 있으면 게이지가 오르고, 가득 차면 그 동행자를 데려간다.
    /// 플레이어가 목표 곁을 지키면 게이지가 빠르게 빠진다.
    /// </summary>
    public static class ClaimLogic
    {
        public const float Maximum = 100f;
        public const float RisePerSecond = 15f;
        public const float GuardDrainPerSecond = 30f;
        public const float IdleDrainPerSecond = 10f;

        /// <summary>목표와 이 거리 안이면 말을 거는 중이다.</summary>
        public const float ReachDistance = 1.2f;

        /// <summary>플레이어가 목표와 이 거리 안이면 지키는 중이다.</summary>
        public const float GuardDistance = 1.6f;

        /// <summary>
        /// 게이지를 흘린다. 지키는 중이면 말을 걸어도 빠진다.
        /// <paramref name="riseScale"/>은 밸런스 배율이다.
        /// </summary>
        public static float Advance(
            float gauge,
            bool inReach,
            bool guarded,
            float riseScale,
            float deltaTime)
        {
            if (deltaTime <= 0f ||
                float.IsNaN(gauge))
            {
                return Clamp(gauge);
            }

            if (guarded)
            {
                return Clamp(gauge - GuardDrainPerSecond * deltaTime);
            }

            if (inReach)
            {
                return Clamp(gauge + RisePerSecond * Math.Max(0f, riseScale) * deltaTime);
            }

            return Clamp(gauge - IdleDrainPerSecond * deltaTime);
        }

        public static bool IsComplete(
            float gauge)
        {
            return gauge >= Maximum;
        }

        /// <summary>
        /// 2인 1조 중 몇 번째 짝이 노릴 동행자 순번이다. 무리 맨 뒤부터 한 명씩 나눠 맡는다.
        /// 맡을 동행자가 없으면 -1이다.
        /// </summary>
        public static int PickTargetIndex(
            int followerCount,
            int pairIndex)
        {
            int index =
                followerCount - 1 -
                Math.Max(0, pairIndex);

            return index >= 0
                ? index
                : -1;
        }

        private static float Clamp(
            float value)
        {
            return float.IsNaN(value)
                ? 0f
                : Math.Max(0f, Math.Min(Maximum, value));
        }
    }

    /// <summary>
    /// 호객꾼 붙잡기 계산이다 (23일차, 부록 C.3 [4]).
    /// 노점 앞을 지나는 동행자를 세우고, 6초 안에 되찾지 못하면 그 동행자는 떠난다.
    /// </summary>
    public static class StallHoldLogic
    {
        public const float PullRadius = 3f;
        public const float HoldSeconds = 6f;

        /// <summary>플레이어가 붙잡힌 동행자 곁에 이만큼 붙어 있으면 되찾는다.</summary>
        public const float RescueDistance = 1.3f;
        public const float RescueSeconds = 1f;

        /// <summary>한 명을 놓아준 뒤 다음 손님을 부르기까지의 간격이다.</summary>
        public const float CooldownSeconds = 5f;

        public static bool CanPull(
            float distance,
            float cooldownRemaining)
        {
            return cooldownRemaining <= 0f &&
                   distance <= PullRadius;
        }

        /// <summary>플레이어가 곁에 있으면 되찾기 진행이 오르고, 떨어지면 처음부터다.</summary>
        public static float AdvanceRescue(
            float progress,
            float playerDistance,
            float deltaTime)
        {
            if (playerDistance > RescueDistance ||
                float.IsNaN(progress))
            {
                return 0f;
            }

            return Math.Min(
                RescueSeconds,
                progress + Math.Max(0f, deltaTime));
        }

        public static bool IsRescued(
            float progress)
        {
            return progress >= RescueSeconds;
        }
    }

    /// <summary>
    /// 길막 역할(취객) 계산이다 (23일차, 부록 C.3 [4]).
    /// 동행자와 부딪치면 충동이 오르고, 대시로 밀쳐낼 수 있다.
    /// </summary>
    public static class BlockerLogic
    {
        public const float BumpDistance = 0.7f;
        public const float BumpImpulse = 15f;

        /// <summary>한 번 부딪친 뒤 다시 부딪칠 수 있기까지의 간격이다. 붙어 있다고 매 프레임 오르지 않게 한다.</summary>
        public const float BumpCooldownSeconds = 2f;

        public const float DashPushRadius = 1.2f;
        public const float DashPushDistance = 1f;
        public const float DashStunSeconds = 1f;

        public static bool CanBump(
            float distance,
            float cooldownRemaining)
        {
            return cooldownRemaining <= 0f &&
                   distance <= BumpDistance;
        }
    }

    /// <summary>
    /// 소매치기 계산이다 (23일차, 부록 C.3 [4]).
    /// 아직 회수하지 않은 정기의 일부를 훔쳐 달아나고, 제한 시간 안에 닿으면 되찾는다.
    /// </summary>
    public static class PickpocketLogic
    {
        public const float StealRatio = 0.3f;
        public const float EscapeSeconds = 20f;
        public const float CatchDistance = 1.0f;
        public const int CatchBonus = 50;

        /// <summary>이 거리 안에 들어오면 예고를 시작한다.</summary>
        public const float ApproachDistance = 3f;

        /// <summary>예고가 끝났을 때 이 거리 안이어야 실제로 훔친다. 대시로 벌리면 실패한다.</summary>
        public const float StealDistance = 2f;

        /// <summary>도난 표식이 붙은 동행자의 정기 배율이다.</summary>
        public static float RemainingMultiplier =>
            1f - StealRatio;

        /// <summary>훔친 양이다. 음수와 0은 0이다.</summary>
        public static int GetStolenAmount(
            int unrecoveredEssence,
            float ratio)
        {
            if (unrecoveredEssence <= 0 ||
                float.IsNaN(ratio))
            {
                return 0;
            }

            float safeRatio =
                Math.Max(0f, Math.Min(1f, ratio));

            return (int)Math.Round(
                unrecoveredEssence * safeRatio,
                MidpointRounding.AwayFromZero);
        }

        /// <summary>훔칠 게 있어야, 가까워야, 아직 훔친 적이 없어야 노린다 (구역당 1회).</summary>
        public static bool WantsToSteal(
            int unrecoveredEssence,
            float distance,
            bool alreadyStole)
        {
            return !alreadyStole &&
                   unrecoveredEssence > 0 &&
                   distance <= ApproachDistance;
        }
    }

    /// <summary>
    /// 해변가 · 야시장 특수 능력 수치다 (23일차, 부록 C.3 [1] · [4]).
    /// </summary>
    public static class BeachMarketAbilityValues
    {
        /// <summary>호루라기 경보: 이 반경 안 중립 NPC가 경계 상태가 된다.</summary>
        public const float WhistleRadius = 10f;
        public const float WhistleAlarmSeconds = 8f;
        public const float WhistleHypnosisMultiplier = 0.5f;
        public const float WhistleAlertRise = 25f;

        /// <summary>라이브 방송: 조명 반경이다. 조명 안은 등불처럼 최면 사거리가 온전하다.</summary>
        public const float LiveLightRadius = 3f;
        public const float LiveAlertRise = 30f;
        public const float LiveChaseSeconds = 8f;

        /// <summary>호루라기 경보를 맞은 NPC의 최면 배율이다. 경보가 끝났으면 1이다.</summary>
        public static float GetAlarmMultiplier(
            float alarmRemaining)
        {
            return alarmRemaining > 0f
                ? WhistleHypnosisMultiplier
                : 1f;
        }
    }
}
