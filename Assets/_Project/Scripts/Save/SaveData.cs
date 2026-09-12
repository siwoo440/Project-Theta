using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 한 판의 결과를 스테이지에서 허브로 넘기는 묶음이다.
    /// </summary>
    public struct StageResultSummary
    {
        public bool Cleared;
        public int RecoveredEssence;
        public int TotalScore;
        public string RankLabel;

        /// <summary>이번 판에서 환산된 계약 정기다.</summary>
        public int ContractEssence;
        public int TargetEssence;

        public static StageResultSummary Empty =>
            new StageResultSummary
            {
                Cleared = false,
                RecoveredEssence = 0,
                TotalScore = 0,
                RankLabel = "-",
                ContractEssence = 0,
                TargetEssence = 0
            };
    }

    /// <summary>
    /// 저장 파일에 들어가는 내용이다.
    ///
    /// JsonUtility로 직렬화하므로 public 필드만 사용한다.
    /// 13일차에는 클리어 횟수·최고 기록만 실제로 채우고,
    /// 계약 정기와 성장 레벨은 필드만 준비해 14일차에 사용한다.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>저장 형식 버전이다. 이후 마이그레이션 경로의 기준이 된다.</summary>
        public int Version = SaveDataLogic.CurrentVersion;

        public int ClearCount;
        public int PlayCount;
        public int BestScore;
        public string BestRankLabel = "-";

        // --- 14일차에 사용 ---
        public int ContractEssence;
        public int[] UpgradeLevels = new int[4];

        public SaveData Clone()
        {
            SaveData copy =
                new SaveData
                {
                    Version = Version,
                    ClearCount = ClearCount,
                    PlayCount = PlayCount,
                    BestScore = BestScore,
                    BestRankLabel = BestRankLabel,
                    ContractEssence = ContractEssence,
                    UpgradeLevels = new int[UpgradeLevels?.Length ?? 4]
                };

            if (UpgradeLevels != null)
            {
                Array.Copy(
                    UpgradeLevels,
                    copy.UpgradeLevels,
                    UpgradeLevels.Length);
            }

            return copy;
        }
    }
}
