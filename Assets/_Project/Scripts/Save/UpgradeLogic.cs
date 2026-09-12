using System;

namespace ProjectTheta.Save
{
    /// <summary>기획서 14장 영구 성장 4계열이다.</summary>
    public enum UpgradeTrack
    {
        /// <summary>최면 - 최면 속도</summary>
        Hypnosis = 0,

        /// <summary>관리 - 안정 관리 한도, 유지도</summary>
        Control = 1,

        /// <summary>안정 - 충동 상승, 폭주 경고</summary>
        Stability = 2,

        /// <summary>기동 - 이동 속도, 대시 비용</summary>
        Mobility = 3
    }

    /// <summary>
    /// 계열별 레벨과 효과, 그리고 구매 비용을 계산한다.
    ///
    /// 각 시스템은 자기가 성장했는지 알 필요 없이
    /// <see cref="PlayerUpgrades"/>가 넘겨주는 배율만 곱한다.
    /// </summary>
    public static class UpgradeLogic
    {
        public const int MaximumLevel = 5;

        public const int TrackCount = 4;

        /// <summary>레벨 1부터 5까지의 누진 비용이다.</summary>
        private static readonly int[] LevelCosts =
            { 40, 70, 110, 160, 220 };

        /// <summary>한 계열을 만렙까지 올리는 총 비용이다.</summary>
        public static int TrackTotalCost
        {
            get
            {
                int total =
                    0;

                for (int i = 0;
                     i < LevelCosts.Length;
                     i++)
                {
                    total +=
                        LevelCosts[i];
                }

                return total;
            }
        }

        public static int ClampLevel(
            int level)
        {
            if (level < 0)
            {
                return 0;
            }

            return level >
                   MaximumLevel
                ? MaximumLevel
                : level;
        }

        public static bool IsMaxLevel(
            int level)
        {
            return ClampLevel(
                       level) >=
                   MaximumLevel;
        }

        /// <summary>현재 레벨에서 다음 레벨로 올리는 비용이다. 만렙이면 0을 반환한다.</summary>
        public static int GetNextLevelCost(
            int currentLevel)
        {
            int level =
                ClampLevel(
                    currentLevel);

            if (level >=
                MaximumLevel)
            {
                return 0;
            }

            return LevelCosts[level];
        }

        public static bool CanPurchase(
            int currentLevel,
            int availableEssence)
        {
            if (IsMaxLevel(
                    currentLevel))
            {
                return false;
            }

            return availableEssence >=
                   GetNextLevelCost(
                       currentLevel);
        }

        /// <summary>구매 후 남는 계약 정기다. 구매할 수 없으면 그대로 반환한다.</summary>
        public static int GetRemainingAfterPurchase(
            int currentLevel,
            int availableEssence)
        {
            if (!CanPurchase(
                    currentLevel,
                    availableEssence))
            {
                return availableEssence;
            }

            return availableEssence -
                   GetNextLevelCost(
                       currentLevel);
        }

        public static string GetTrackName(
            UpgradeTrack track)
        {
            switch (track)
            {
                case UpgradeTrack.Hypnosis:
                    return "최면";

                case UpgradeTrack.Control:
                    return "관리";

                case UpgradeTrack.Stability:
                    return "안정";

                case UpgradeTrack.Mobility:
                    return "기동";

                default:
                    return "-";
            }
        }

        // ---------------- 계열별 효과 ----------------

        /// <summary>최면 - 단계당 최면 속도 +6%.</summary>
        public static float GetHypnosisSpeedMultiplier(
            int level)
        {
            return 1f +
                   (0.06f *
                    ClampLevel(
                        level));
        }

        /// <summary>관리 - 단계당 안정 관리 한도 +1. 기본 4명에서 최대 8명.</summary>
        public static int GetStableFollowerLimit(
            int level,
            int baseLimit)
        {
            return Math.Max(
                       0,
                       baseLimit) +
                   Math.Min(
                       4,
                       ClampLevel(
                           level));
        }

        /// <summary>관리 - 단계당 유지도 감소 -7%.</summary>
        public static float GetStabilityDecayMultiplier(
            int level)
        {
            return Math.Max(
                0.1f,
                1f -
                (0.07f *
                 ClampLevel(
                     level)));
        }

        /// <summary>안정 - 단계당 충동 상승 -8%.</summary>
        public static float GetImpulseBuildMultiplier(
            int level)
        {
            return Math.Max(
                0.1f,
                1f -
                (0.08f *
                 ClampLevel(
                     level)));
        }

        /// <summary>안정 - 단계당 폭주 경고 시간 +0.1초.</summary>
        public static float GetRampageWarningBonus(
            int level)
        {
            return 0.1f *
                   ClampLevel(
                       level);
        }

        /// <summary>기동 - 단계당 이동 속도 +4%.</summary>
        public static float GetMoveSpeedMultiplier(
            int level)
        {
            return 1f +
                   (0.04f *
                    ClampLevel(
                        level));
        }

        /// <summary>기동 - 단계당 대시 집중력 소모 -10%.</summary>
        public static float GetDashCostMultiplier(
            int level)
        {
            return Math.Max(
                0.1f,
                1f -
                (0.1f *
                 ClampLevel(
                     level)));
        }
    }
}
