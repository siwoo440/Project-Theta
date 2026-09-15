using ProjectTheta.Balance;
using ProjectTheta.Run;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 성장·난이도·런 강화 배율에 대한 전역 접근 지점이다.
    ///
    /// 17일차부터 최종 값은 "난이도 × 영구 성장 × 런 강화"다.
    /// 런 강화는 <see cref="RunUpgradeMultipliers"/>에서 곱해 들어오므로
    /// 이 값을 읽던 기존 코드는 손대지 않아도 효과가 반영된다.
    ///
    /// NPC나 최면 대상처럼 플레이어를 직접 참조하지 않는 컴포넌트가
    /// 매 프레임 탐색하지 않고 배율을 읽을 수 있게 한다.
    /// 플레이어가 없는 상황(테스트·타이틀 화면)에서는 난이도 배율만 적용된다.
    /// </summary>
    public static class PlayerUpgradeMultipliers
    {
        /// <summary>현재 스테이지의 플레이어 성장 정보다. <see cref="PlayerUpgrades"/>가 등록한다.</summary>
        public static PlayerUpgrades Active { get; set; }

        private static DifficultyMultipliers Difficulty =>
            BalanceOverrides.Difficulty ??
            DifficultyTable.Normal;

        public static float HypnosisSpeed =>
            (Active == null
                ? Difficulty.HypnosisSpeed
                : Active.HypnosisSpeedMultiplier) *
            RunUpgradeMultipliers.HypnosisSpeed;

        public static float ImpulseBuild =>
            (Active == null
                ? Difficulty.ImpulseBuild
                : Active.ImpulseBuildMultiplier) *
            RunUpgradeMultipliers.ImpulseBuild;

        public static float MoveSpeed =>
            (Active == null
                ? 1f
                : Active.MoveSpeedMultiplier) *
            RunUpgradeMultipliers.MoveSpeed;

        public static float DashCost =>
            (Active == null
                ? 1f
                : Active.DashCostMultiplier) *
            RunUpgradeMultipliers.DashCost;

        public static float FollowerStabilityDecay =>
            Active == null
                ? 1f
                : Active.FollowerStabilityDecayMultiplier;

        public static float RampageWarningBonus =>
            Active == null
                ? 0f
                : Active.RampageWarningBonus;

        public static float RampageWarningMultiplier =>
            Difficulty.RampageWarning;

        public static float OpponentPressure =>
            Difficulty.OpponentPressure;

        public static float TimeLimit =>
            Difficulty.TimeLimit;

        public static float TargetEssence =>
            Difficulty.TargetEssence;

        public static int GetStableFollowerLimit(
            int baseLimit)
        {
            int limit =
                Active == null
                    ? baseLimit
                    : Active.GetStableFollowerLimit(
                        baseLimit);

            return limit +
                   RunUpgradeMultipliers.FollowerLimitBonus;
        }

        public static void Clear()
        {
            Active =
                null;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Clear();
        }
    }
}
