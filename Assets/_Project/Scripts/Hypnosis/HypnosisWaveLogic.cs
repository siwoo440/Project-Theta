using System;
using ProjectTheta.Balance;

namespace ProjectTheta.Hypnosis
{
    /// <summary>
    /// 기획서 4.2절·7.3절 최면 파동이다.
    ///
    /// 플레이어가 "당하기만 하는" 상황을 끊어내는 핵심 대응 수단으로,
    /// 동시 폭주 위기·경쟁자 쟁탈 진행·각성 오라 통과를 한 번에 해결한다.
    /// </summary>
    public static class HypnosisWaveLogic
    {
        public const float DefaultChargeSeconds = 0.40f;
        public const float DefaultFocusCost = 30f;

        public static float ChargeSeconds =>
            BalanceOverrides.StageOrDefault.WaveChargeSeconds;

        public static float Radius =>
            BalanceOverrides.StageOrDefault.WaveRadius;

        /// <summary>내 동행 NPC의 충동을 이만큼 낮춘다.</summary>
        public static float ImpulseRelief =>
            BalanceOverrides.StageOrDefault.WaveImpulseRelief;

        /// <summary>경쟁자를 이 시간만큼 정지시키고 진행 중인 선점·쟁탈을 초기화한다.</summary>
        public static float OpponentStunSeconds =>
            BalanceOverrides.StageOrDefault.WaveOpponentStunSeconds;

        /// <summary>각성 지원형의 오라를 이 시간만큼 무력화한다.</summary>
        public static float AuraSuppressSeconds =>
            BalanceOverrides.StageOrDefault.WaveAuraSuppressSeconds;

        public static float FocusCost =>
            BalanceOverrides.StageOrDefault.WaveFocusCost;

        public static float CooldownSeconds =>
            BalanceOverrides.StageOrDefault.WaveCooldownSeconds;

        public static bool IsInRadius(
            float distance,
            float radius)
        {
            return distance <=
                   Math.Max(
                       0f,
                       radius);
        }

        public static float GetChargeNormalized(
            float heldSeconds,
            float chargeSeconds)
        {
            float safeCharge =
                Math.Max(
                    0.0001f,
                    chargeSeconds);

            float value =
                Math.Max(
                    0f,
                    heldSeconds) /
                safeCharge;

            return value > 1f
                ? 1f
                : value;
        }

        public static bool IsChargeComplete(
            float heldSeconds,
            float chargeSeconds)
        {
            return heldSeconds >=
                   Math.Max(
                       0f,
                       chargeSeconds);
        }

        /// <summary>
        /// 파동을 시전할 수 있는 조건이다.
        /// 힘겨루기·포획 미니게임도 우클릭을 쓰므로 그동안에는 차단한다.
        /// </summary>
        public static bool CanCast(
            bool stageRunning,
            bool blockedByOtherSystem,
            float cooldownRemaining,
            float currentFocus,
            float focusCost)
        {
            return stageRunning &&
                   !blockedByOtherSystem &&
                   cooldownRemaining <= 0f &&
                   currentFocus >=
                   Math.Max(
                       0f,
                       focusCost);
        }
    }
}
