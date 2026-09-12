using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 세이브 데이터에 대한 판정과 갱신 규칙이다.
    ///
    /// 파일 입출력은 <see cref="SaveSystem"/>이 담당하고,
    /// 여기서는 Unity에 의존하지 않는 순수 계산만 다뤄 테스트로 고정한다.
    /// </summary>
    public static class SaveDataLogic
    {
        public const int CurrentVersion = 1;

        /// <summary>성장 계열 수다. 최면 / 관리 / 안정 / 기동.</summary>
        public const int UpgradeTrackCount = 4;

        public static SaveData CreateDefault()
        {
            return new SaveData
            {
                Version = CurrentVersion,
                ClearCount = 0,
                PlayCount = 0,
                BestScore = 0,
                BestRankLabel = "-",
                ContractEssence = 0,
                UpgradeLevels =
                    new int[UpgradeTrackCount]
            };
        }

        /// <summary>
        /// 읽어들인 데이터를 안전한 상태로 보정한다.
        /// 손상되었거나 필드가 비어 있어도 게임이 시작되지 않는 일이 없어야 한다.
        /// </summary>
        public static SaveData Normalize(
            SaveData data)
        {
            if (data == null)
            {
                return CreateDefault();
            }

            if (data.UpgradeLevels == null ||
                data.UpgradeLevels.Length !=
                UpgradeTrackCount)
            {
                int[] levels =
                    new int[UpgradeTrackCount];

                if (data.UpgradeLevels != null)
                {
                    int copyCount =
                        Math.Min(
                            data.UpgradeLevels.Length,
                            UpgradeTrackCount);

                    Array.Copy(
                        data.UpgradeLevels,
                        levels,
                        copyCount);
                }

                data.UpgradeLevels =
                    levels;
            }

            for (int i = 0;
                 i < data.UpgradeLevels.Length;
                 i++)
            {
                data.UpgradeLevels[i] =
                    Math.Max(
                        0,
                        data.UpgradeLevels[i]);
            }

            data.ClearCount =
                Math.Max(
                    0,
                    data.ClearCount);

            data.PlayCount =
                Math.Max(
                    data.ClearCount,
                    Math.Max(
                        0,
                        data.PlayCount));

            data.BestScore =
                Math.Max(
                    0,
                    data.BestScore);

            data.ContractEssence =
                Math.Max(
                    0,
                    data.ContractEssence);

            if (string.IsNullOrEmpty(
                    data.BestRankLabel))
            {
                data.BestRankLabel =
                    "-";
            }

            data.Version =
                Migrate(
                    data.Version);

            return data;
        }

        /// <summary>
        /// 구버전 저장을 현재 버전으로 올린다.
        /// 아직 구버전이 없으므로 버전 번호만 맞추지만, 경로를 미리 만들어 둔다.
        /// </summary>
        public static int Migrate(
            int version)
        {
            if (version <= 0)
            {
                return CurrentVersion;
            }

            return version >
                   CurrentVersion
                ? CurrentVersion
                : CurrentVersion;
        }

        /// <summary>랭크 라벨의 우열을 비교한다. 높을수록 좋은 랭크다.</summary>
        public static int GetRankOrder(
            string rankLabel)
        {
            switch (rankLabel)
            {
                case "S":
                    return 4;

                case "A":
                    return 3;

                case "B":
                    return 2;

                case "C":
                    return 1;

                default:
                    return 0;
            }
        }

        public static bool IsBetterRank(
            string candidate,
            string current)
        {
            return GetRankOrder(
                       candidate) >
                   GetRankOrder(
                       current);
        }

        /// <summary>
        /// 한 판의 결과를 세이브에 반영한다.
        /// 최고 기록은 더 좋을 때만 갱신하고, 낮은 기록은 무시한다.
        /// </summary>
        public static SaveData ApplyStageResult(
            SaveData data,
            StageResultSummary result)
        {
            SaveData target =
                Normalize(
                    data);

            target.PlayCount++;

            if (result.Cleared)
            {
                target.ClearCount++;
            }

            if (result.TotalScore >
                target.BestScore)
            {
                target.BestScore =
                    result.TotalScore;
            }

            // 랭크는 클리어한 판에서만 의미가 있다.
            if (result.Cleared &&
                IsBetterRank(
                    result.RankLabel,
                    target.BestRankLabel))
            {
                target.BestRankLabel =
                    result.RankLabel;
            }

            return target;
        }
    }
}
