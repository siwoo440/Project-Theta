using System;
using ProjectTheta.Balance;

namespace ProjectTheta.Run
{
    /// <summary>경험치를 주는 행동이다.</summary>
    public enum RunXpSource
    {
        /// <summary>최면에 성공했다. 체인으로 이어진 대상도 한 명씩 센다.</summary>
        HypnosisSuccess = 0,

        /// <summary>경쟁자에게 빼앗긴 NPC를 되찾았다. 최면 성공에 더해 준다.</summary>
        ReclaimBonus = 1,

        /// <summary>회수 지점에서 한 명을 확정했다.</summary>
        RecoveryPerFollower = 2,

        /// <summary>한 번에 두 명 이상 회수했을 때, 두 번째부터 한 명당 더 준다.</summary>
        BatchBonusPerExtra = 3,

        /// <summary>충동이 위험 구간인 동행자를 회수했다.</summary>
        RiskyRecovery = 4,

        /// <summary>희귀 이상 등급을 회수했다.</summary>
        HighGradeRecovery = 5,

        /// <summary>경쟁자와의 힘겨루기에서 이겼다.</summary>
        DuelWin = 6,

        /// <summary>새 층에 처음 올라갔다.</summary>
        FloorFirstVisit = 7
    }

    /// <summary>
    /// 한 판 안에서 쌓는 경험치와 레벨 규칙이다.
    ///
    /// 행동마다 주는 경험치와 레벨 곡선은 모두 <see cref="StageBalanceValues"/>에 있어
    /// 자산에서 재컴파일 없이 조정한다. 여기에는 계산식만 둔다.
    ///
    /// 경험치는 줄어들지 않는다. 포획당해도 깎지 않는다.
    /// 실패에 대한 벌은 이미 점수와 체력이 맡고 있고, 성장까지 깎으면 회복이 불가능해진다.
    /// </summary>
    public static class RunExperienceLogic
    {
        private static StageBalanceValues Values =>
            BalanceOverrides.StageOrDefault;

        public static int GetPoints(
            RunXpSource source)
        {
            StageBalanceValues v =
                Values;

            switch (source)
            {
                case RunXpSource.HypnosisSuccess:
                    return v.XpHypnosisSuccess;

                case RunXpSource.ReclaimBonus:
                    return v.XpReclaimBonus;

                case RunXpSource.RecoveryPerFollower:
                    return v.XpRecoveryPerFollower;

                case RunXpSource.BatchBonusPerExtra:
                    return v.XpBatchBonusPerExtra;

                case RunXpSource.RiskyRecovery:
                    return v.XpRiskyRecovery;

                case RunXpSource.HighGradeRecovery:
                    return v.XpHighGradeRecovery;

                case RunXpSource.DuelWin:
                    return v.XpDuelWin;

                case RunXpSource.FloorFirstVisit:
                    return v.XpFloorFirstVisit;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 회수 한 묶음이 주는 경험치다.
        /// 한 명씩 따로 회수하는 것보다 모아서 회수하는 쪽이 더 많이 받는다.
        /// </summary>
        public static int GetRecoveryPoints(
            int recoveredCount,
            int riskyCount,
            int highGradeCount)
        {
            int count =
                Math.Max(
                    0,
                    recoveredCount);

            if (count == 0)
            {
                return 0;
            }

            return count *
                   GetPoints(
                       RunXpSource.RecoveryPerFollower) +
                   (count - 1) *
                   GetPoints(
                       RunXpSource.BatchBonusPerExtra) +
                   Math.Max(
                       0,
                       riskyCount) *
                   GetPoints(
                       RunXpSource.RiskyRecovery) +
                   Math.Max(
                       0,
                       highGradeCount) *
                   GetPoints(
                       RunXpSource.HighGradeRecovery);
        }

        public static int GetHypnosisPoints(
            bool wasReclaim)
        {
            return GetPoints(
                       RunXpSource.HypnosisSuccess) +
                   (wasReclaim
                       ? GetPoints(
                           RunXpSource.ReclaimBonus)
                       : 0);
        }

        public static int MaximumLevel =>
            Math.Max(
                1,
                Values.LevelMaximum);

        /// <summary>
        /// 해당 레벨에서 다음 레벨로 가는 데 필요한 경험치다.
        /// 1→2가 기본값이고, 레벨마다 일정량씩 늘어난다.
        /// 최대 레벨이면 0이다.
        /// </summary>
        public static int GetRequiredXp(
            int level)
        {
            if (level >= MaximumLevel)
            {
                return 0;
            }

            int safeLevel =
                Math.Max(
                    1,
                    level);

            return Math.Max(
                1,
                Values.LevelBaseXp +
                Values.LevelXpGrowth *
                (safeLevel - 1));
        }

        /// <summary>1레벨에서 목표 레벨까지 필요한 경험치 총합이다.</summary>
        public static int GetTotalXpToReach(
            int targetLevel)
        {
            int total = 0;

            int target =
                Math.Min(
                    MaximumLevel,
                    targetLevel);

            for (int level = 1;
                 level < target;
                 level++)
            {
                total +=
                    GetRequiredXp(
                        level);
            }

            return total;
        }

        /// <summary>경험치 획득 배율을 적용한다. 반올림한다.</summary>
        public static int ApplyGainMultiplier(
            int points,
            float multiplier)
        {
            if (points <= 0)
            {
                return 0;
            }

            return Math.Max(
                1,
                (int)Math.Round(
                    points *
                    Math.Max(
                        0f,
                        multiplier),
                    MidpointRounding.AwayFromZero));
        }
    }
}
