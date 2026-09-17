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
        /// <summary>
        /// 실패했을 때 받는 비율이다 (34일차). 욕심내다 쓰러지면 손해가 분명해야 한다.
        /// </summary>
        public const float FailureMultiplier = 0.5f;

        /// <summary>결과 화면 문구에 쓰는 실패 보상 퍼센트다.</summary>
        public static int FailurePercent =>
            (int)Math.Round(FailureMultiplier * 100f);

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
        /// 실패해도 회수분의 일부는 인정한다.
        /// 기획서 24장 "이미 확정 회수한 정기의 일부는 보존해 반복 실패의 피로도를 줄인다".
        /// 랭크 보너스와 초과 달성분은 클리어한 판에만 준다.
        ///
        /// 34일차: 실패하면 <see cref="FailureMultiplier"/>만큼만 받고,
        /// 목표를 넘긴 정기(탈출 가능 상태에서 쓰러진 경우)는 인정하지 않는다.
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

            if (!cleared)
            {
                int counted =
                    targetEssence > 0
                        ? Math.Min(
                            safeRecovered,
                            targetEssence)
                        : safeRecovered;

                return Round(
                    counted *
                    (double)Math.Max(
                        0f,
                        values.ContractEssenceRatio) *
                    FailureMultiplier);
            }

            double total =
                safeRecovered *
                (double)Math.Max(
                    0f,
                    values.ContractEssenceRatio);

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

            return Round(
                total);
        }

        /// <summary>
        /// 실패 페널티 없이 회수분 비율만 적용했을 때의 값이다.
        /// 결과 화면이 "실패로 잃은 정기"를 보여 주는 데 쓴다.
        /// </summary>
        public static int GetFullRate(
            int recoveredEssence)
        {
            return Round(
                Math.Max(
                    0,
                    recoveredEssence) *
                (double)Math.Max(
                    0f,
                    BalanceOverrides.StageOrDefault.ContractEssenceRatio));
        }

        /// <summary>결과 화면 계약 정기 줄의 실패 표시다. 잃은 양이 없으면 퍼센트만 적는다.</summary>
        public static string GetFailureNote(
            int lost)
        {
            return lost > 0
                ? $"실패 · 보상 {FailurePercent}%  (−{lost:N0})"
                : $"실패 · 보상 {FailurePercent}%";
        }

        private static int Round(
            double value)
        {
            return Math.Max(
                0,
                (int)Math.Round(
                    value,
                    MidpointRounding.AwayFromZero));
        }
    }
}
