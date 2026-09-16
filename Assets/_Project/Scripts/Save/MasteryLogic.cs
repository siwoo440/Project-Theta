using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 장소 숙련도(★)와 엔딩 해금 규칙이다 (30일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   ★ 수   클리어 1 · 3 · 6회에서 하나씩 오른다 (최대 3)
    ///   목표    별마다 목표 정기 +5%   (익숙한 장소일수록 조금 어렵게)
    ///   보상    별마다 계약 정기 +10%  (대신 보상도 크게)
    ///   엔딩    한 번이라도 보면 시작 계약 카드가 3장 → 4장
    /// </summary>
    public static class MasteryLogic
    {
        public const int MaxStars = 3;

        public static readonly int[] StarClears = { 1, 3, 6 };

        public const float TargetBonusPerStar = 0.05f;
        public const float RewardBonusPerStar = 0.10f;

        public const int BaseStartChoices = 3;
        public const int EndingStartChoices = 4;

        public static int GetStars(
            int clears)
        {
            int stars = 0;

            for (int i = 0; i < StarClears.Length; i++)
            {
                if (clears >= StarClears[i])
                {
                    stars++;
                }
            }

            return Math.Min(MaxStars, stars);
        }

        public static int GetStars(
            SaveData save,
            int location)
        {
            return GetStars(PlayStatsLogic.Get(save, location).Clears);
        }

        /// <summary>다음 별까지 남은 클리어 수다. 다 모았으면 0이다.</summary>
        public static int GetClearsToNextStar(
            int clears)
        {
            for (int i = 0; i < StarClears.Length; i++)
            {
                if (clears < StarClears[i])
                {
                    return StarClears[i] - clears;
                }
            }

            return 0;
        }

        public static int GetTargetEssence(
            int baseTarget,
            int stars)
        {
            return (int)Math.Round(
                Math.Max(0, baseTarget) *
                (1.0 + TargetBonusPerStar * Clamp(stars)));
        }

        public static int GetContractEssence(
            int baseContract,
            int stars)
        {
            return (int)Math.Round(
                Math.Max(0, baseContract) *
                (1.0 + RewardBonusPerStar * Clamp(stars)));
        }

        /// <summary>"★★☆"다.</summary>
        public static string FormatStars(
            int stars)
        {
            int clamped = Clamp(stars);

            return new string('★', clamped) + new string('☆', MaxStars - clamped);
        }

        public static bool HasEndingUnlock(
            SaveData save)
        {
            return save != null &&
                   save.Stats != null &&
                   save.Stats.Endings > 0;
        }

        /// <summary>시작 계약에서 제시하는 카드 수다.</summary>
        public static int GetStartChoiceCount(
            SaveData save)
        {
            return HasEndingUnlock(save)
                ? EndingStartChoices
                : BaseStartChoices;
        }

        private static int Clamp(
            int stars)
        {
            return Math.Max(0, Math.Min(MaxStars, stars));
        }
    }
}
