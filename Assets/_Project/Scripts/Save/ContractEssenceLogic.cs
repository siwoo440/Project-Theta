using System;
using ProjectTheta.Balance;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 기획서 10.1절·13.3절. 스테이지 내부 정기를 영구 재화인 계약 정기로 환산한다.
    ///
    /// 두 재화를 분리하는 이유는 클리어 조건(스테이지 정기)과 성장 속도(계약 정기)를
    /// 따로 조정할 수 있어야 하기 때문이다.
    /// </summary>
    public static class ContractEssenceLogic
    {
        public static int GetRankBonus(
            string rankLabel)
        {
            StageBalanceValues values =
                BalanceOverrides.StageOrDefault;

            switch (rankLabel)
            {
                case "S":
                    return values.ContractRankBonusS;

                case "A":
                    return values.ContractRankBonusA;

                case "B":
                    return values.ContractRankBonusB;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 한 판의 결과를 계약 정기로 환산한다.
        ///
        /// 실패해도 회수분은 그대로 인정한다.
        /// 기획서 24장 "이미 확정 회수한 정기의 일부는 보존해 반복 실패의 피로도를 줄인다".
        /// 랭크 보너스와 초과 달성분은 클리어한 판에만 준다.
        /// </summary>
        public static int Compute(
            int recoveredEssence,
            int targetEssence,
            bool cleared,
            string rankLabel)
        {
            StageBalanceValues values =
                BalanceOverrides.StageOrDefault;

            int safeRecovered =
                Math.Max(
                    0,
                    recoveredEssence);

            double total =
                safeRecovered *
                (double)Math.Max(
                    0f,
                    values.ContractEssenceRatio);

            if (cleared)
            {
                total +=
                    GetRankBonus(
                        rankLabel);

                int overflow =
                    Math.Max(
                        0,
                        safeRecovered -
                        Math.Max(
                            0,
                            targetEssence));

                total +=
                    overflow *
                    (double)Math.Max(
                        0f,
                        values.ContractOverflowRatio);
            }

            return Math.Max(
                0,
                (int)Math.Round(
                    total,
                    MidpointRounding.AwayFromZero));
        }
    }
}
