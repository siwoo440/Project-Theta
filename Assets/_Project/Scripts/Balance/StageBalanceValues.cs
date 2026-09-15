using System;

namespace ProjectTheta.Balance
{
    /// <summary>
    /// 코드에 흩어져 있던 스테이지 관련 밸런스 수치를 한 곳에 모은 값 묶음이다.
    ///
    /// 계산식은 각 로직 클래스에 그대로 두고 숫자만 여기로 뺐다.
    /// 계산식까지 자산으로 옮기면 Unity 자산 없이 도는 EditMode 테스트가 전부 깨진다.
    /// </summary>
    [Serializable]
    public sealed class StageBalanceValues
    {
        // --- 랭크 구간 ---
        public int RankBThreshold = 6000;
        public int RankAThreshold = 11000;
        public int RankSThreshold = 16000;

        // --- 콤보 ---
        public float ComboTimeoutSeconds = 6.0f;
        public float ComboStepMultiplier = 0.1f;
        public float ComboMaximumMultiplier = 3.0f;

        // --- 회수 ---
        public float RecoveryBatchWindowSeconds = 1.5f;
        public float[] SimultaneousMultipliers =
            { 1.0f, 1.2f, 1.4f, 1.7f, 2.0f };

        // --- 최면 파동 ---
        public float WaveChargeSeconds = 0.40f;
        public float WaveRadius = 3.5f;
        public float WaveImpulseRelief = 35f;
        public float WaveOpponentStunSeconds = 1.2f;
        public float WaveAuraSuppressSeconds = 2.5f;
        public float WaveFocusCost = 30f;
        public float WaveCooldownSeconds = 4.0f;

        // --- 체인 최면 ---
        public float ChainRadius = 2.2f;
        public float[] ChainSpeedMultipliers =
            { 1.0f, 0.7f, 0.5f };
        public float[] ChainFocusCosts =
            { 0f, 15f, 25f };

        // --- 계약 정기 환산 ---
        public float ContractEssenceRatio = 0.5f;
        public float ContractOverflowRatio = 0.3f;
        public int ContractRankBonusB = 20;
        public int ContractRankBonusA = 50;
        public int ContractRankBonusS = 100;

        // --- 런 경험치 (17일차) ---
        // 행동마다 주는 경험치다. 모두 정수이며 줄어들지 않는다.
        public int XpHypnosisSuccess = 10;
        public int XpReclaimBonus = 15;
        public int XpRecoveryPerFollower = 12;
        public int XpBatchBonusPerExtra = 6;
        public int XpRiskyRecovery = 10;
        public int XpHighGradeRecovery = 15;
        public int XpDuelWin = 30;
        public int XpFloorFirstVisit = 25;
        public int XpRampageSurvived = 20;

        // --- 런 레벨 곡선 ---
        // 1→2 필요 경험치 = LevelBaseXp, 이후 레벨마다 LevelXpGrowth씩 늘어난다.
        public int LevelBaseXp = 60;
        public int LevelXpGrowth = 35;
        public int LevelMaximum = 10;

        public StageBalanceValues Clone()
        {
            return new StageBalanceValues
            {
                RankBThreshold = RankBThreshold,
                RankAThreshold = RankAThreshold,
                RankSThreshold = RankSThreshold,
                ComboTimeoutSeconds = ComboTimeoutSeconds,
                ComboStepMultiplier = ComboStepMultiplier,
                ComboMaximumMultiplier = ComboMaximumMultiplier,
                RecoveryBatchWindowSeconds = RecoveryBatchWindowSeconds,
                SimultaneousMultipliers =
                    (float[])SimultaneousMultipliers.Clone(),
                WaveChargeSeconds = WaveChargeSeconds,
                WaveRadius = WaveRadius,
                WaveImpulseRelief = WaveImpulseRelief,
                WaveOpponentStunSeconds = WaveOpponentStunSeconds,
                WaveAuraSuppressSeconds = WaveAuraSuppressSeconds,
                WaveFocusCost = WaveFocusCost,
                WaveCooldownSeconds = WaveCooldownSeconds,
                ChainRadius = ChainRadius,
                ChainSpeedMultipliers =
                    (float[])ChainSpeedMultipliers.Clone(),
                ChainFocusCosts =
                    (float[])ChainFocusCosts.Clone(),
                ContractEssenceRatio = ContractEssenceRatio,
                ContractOverflowRatio = ContractOverflowRatio,
                ContractRankBonusB = ContractRankBonusB,
                ContractRankBonusA = ContractRankBonusA,
                ContractRankBonusS = ContractRankBonusS,
                XpHypnosisSuccess = XpHypnosisSuccess,
                XpReclaimBonus = XpReclaimBonus,
                XpRecoveryPerFollower = XpRecoveryPerFollower,
                XpBatchBonusPerExtra = XpBatchBonusPerExtra,
                XpRiskyRecovery = XpRiskyRecovery,
                XpHighGradeRecovery = XpHighGradeRecovery,
                XpDuelWin = XpDuelWin,
                XpFloorFirstVisit = XpFloorFirstVisit,
                XpRampageSurvived = XpRampageSurvived,
                LevelBaseXp = LevelBaseXp,
                LevelXpGrowth = LevelXpGrowth,
                LevelMaximum = LevelMaximum
            };
        }

        /// <summary>배열 항목을 안전하게 읽는다. 범위를 벗어나면 마지막 값을 쓴다.</summary>
        public static float ReadClamped(
            float[] values,
            int index,
            float fallback)
        {
            if (values == null ||
                values.Length == 0)
            {
                return fallback;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >=
                values.Length)
            {
                index =
                    values.Length - 1;
            }

            return values[index];
        }
    }
}
