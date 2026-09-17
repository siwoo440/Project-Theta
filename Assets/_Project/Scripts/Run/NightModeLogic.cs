using System;
using ProjectTheta.Save;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 엔딩 뒤 목표 "심야 모드"다 (36일차).
    ///
    /// 엔딩을 한 번 보면 모든 장소에서 켤 수 있다.
    ///   목표 정기 ×1.3 · 제한 시간 ×0.9 · 추격 층당 적 최대 7명 · 경계도 상승 ×1.2
    ///   대신 계약 정기 ×1.5, 클리어하면 장소에 ☾ 표시(<see cref="LocationStats.NightClears"/>)
    /// Unity 없이 도는 순수 계산이다. 스테이지 안에서 켜져 있는지는 <see cref="NightModeState"/>가 들고 있다.
    /// </summary>
    public static class NightModeLogic
    {
        public const float TargetScale = 1.3f;
        public const float TimeScale = 0.9f;
        public const int ChaseMaxPerFloor = 7;
        public const float AlertScale = 1.2f;
        public const float RewardScale = 1.5f;

        public const string Mark = "☾";
        public const string Label = "심야";

        public static bool IsUnlocked(
            SaveData save)
        {
            return MasteryLogic.HasEndingUnlock(save);
        }

        /// <summary>해금되지 않았으면 켤 수 없다.</summary>
        public static bool Resolve(
            bool requested,
            SaveData save)
        {
            return requested &&
                   IsUnlocked(save);
        }

        public static int GetTarget(
            int target,
            bool night)
        {
            return night
                ? (int)Math.Round(Math.Max(0, target) * (double)TargetScale)
                : target;
        }

        public static float GetTimeLimit(
            float seconds,
            bool night)
        {
            return night
                ? seconds * TimeScale
                : seconds;
        }

        public static int GetChaseMax(
            int normal,
            bool night)
        {
            return night
                ? Math.Max(normal, ChaseMaxPerFloor)
                : normal;
        }

        public static float GetAlertAmount(
            float amount,
            bool night)
        {
            // 경계도가 내려가는 쪽은 그대로 둔다.
            return night && amount > 0f
                ? amount * AlertScale
                : amount;
        }

        public static int GetReward(
            int contract,
            bool night)
        {
            return night
                ? (int)Math.Round(Math.Max(0, contract) * (double)RewardScale)
                : contract;
        }

        /// <summary>지도 패널 · 규칙 카드에 쓰는 변화 요약이다.</summary>
        public static string GetSummary()
        {
            return $"{Mark} 심야 모드  목표 ×{TargetScale:0.0} · 시간 ×{TimeScale:0.0} · 추격 최대 {ChaseMaxPerFloor}명 · 경계도 ×{AlertScale:0.0}  →  보상 ×{RewardScale:0.0}";
        }

        /// <summary>지도 패널 한 줄 요약이다.</summary>
        public static string GetShortSummary()
        {
            return $"{Mark} 심야  목표 ×{TargetScale:0.0} · 시간 ×{TimeScale:0.0} · 추격 {ChaseMaxPerFloor}명 · 보상 ×{RewardScale:0.0}";
        }

        public static string GetToggleLabel(
            bool unlocked,
            bool on)
        {
            if (!unlocked)
            {
                return $"{Mark} 심야 (잠김)";
            }

            return on
                ? $"{Mark} 심야 켜짐"
                : $"{Mark} 심야 꺼짐";
        }

        /// <summary>결과 화면 제목에 붙는다.</summary>
        public static string GetTitleSuffix(
            bool night)
        {
            return night
                ? $"  ·  {Mark} {Label}"
                : string.Empty;
        }
    }
}
