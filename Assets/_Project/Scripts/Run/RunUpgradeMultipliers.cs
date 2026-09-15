namespace ProjectTheta.Run
{
    /// <summary>
    /// 이번 판의 강화 효과에 대한 전역 접근 지점이다.
    ///
    /// <see cref="ProjectTheta.Player.PlayerUpgradeMultipliers"/>가 여기를 곱해서
    /// 난이도 × 영구 성장 × 런 강화를 한 통로로 내보낸다.
    /// 판이 없을 때(테스트·허브)는 모두 효과 없음(1배, +0)이다.
    /// </summary>
    public static class RunUpgradeMultipliers
    {
        public static RunUpgradeState Active { get; set; }

        public static float HypnosisSpeed =>
            Active == null
                ? 1f
                : Active.HypnosisSpeedMultiplier;

        public static float ImpulseBuild =>
            Active == null
                ? 1f
                : Active.ImpulseBuildMultiplier;

        public static float MoveSpeed =>
            Active == null
                ? 1f
                : Active.MoveSpeedMultiplier;

        public static float DashCost =>
            Active == null
                ? 1f
                : Active.DashCostMultiplier;

        public static float RecoveryEssence =>
            Active == null
                ? 1f
                : Active.RecoveryEssenceMultiplier;

        public static float XpGain =>
            Active == null
                ? 1f
                : Active.XpGainMultiplier;

        public static int ChainExtraTargets =>
            Active == null
                ? 0
                : Active.ChainExtraTargets;

        public static int FollowerLimitBonus =>
            Active == null
                ? 0
                : Active.FollowerLimitBonus;

        public static void Clear()
        {
            Active = null;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Clear();
        }
    }
}
