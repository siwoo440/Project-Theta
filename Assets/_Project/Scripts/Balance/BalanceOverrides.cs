namespace ProjectTheta.Balance
{
    /// <summary>
    /// 자산에서 읽어온 밸런스 값을 담아두는 주입 지점이다.
    ///
    /// 각 로직 클래스는 여기를 먼저 보고, 값이 없으면 자기 안의 기본 표를 쓴다.
    /// 이 구조 덕분에 EditMode 테스트는 Unity 자산이나 Resources.Load 없이 그대로 돈다.
    /// (테스트에서는 여기가 항상 비어 있으므로 기본 표가 쓰인다.)
    /// </summary>
    public static class BalanceOverrides
    {
        /// <summary>자산에서 읽은 스테이지 수치다. null이면 코드 기본값을 쓴다.</summary>
        public static StageBalanceValues Stage { get; set; }

        /// <summary>현재 난이도 배율이다. 항상 값이 있으며 기본은 노멀이다.</summary>
        public static DifficultyMultipliers Difficulty { get; set; } =
            DifficultyTable.Normal;

        /// <summary>자산이 실제로 적용되었는지 여부다. 디버그 표시에 사용한다.</summary>
        public static bool HasStageAsset =>
            Stage != null;

        public static StageBalanceValues StageOrDefault =>
            Stage ?? DefaultStage;

        private static readonly StageBalanceValues DefaultStage =
            new StageBalanceValues();

        public static void ResetToDefaults()
        {
            Stage = null;

            Difficulty =
                DifficultyTable.Normal;
        }

        public static void SetDifficulty(
            DifficultyLevel level)
        {
            Difficulty =
                DifficultyTable.Get(
                    level);
        }
    }
}
