using System;

namespace ProjectTheta.Balance
{
    /// <summary>기획서 A.1절 난이도 3단계다. 기본 밸런스는 노멀 기준이다.</summary>
    public enum DifficultyLevel
    {
        Story,
        Normal,
        Challenge
    }

    /// <summary>
    /// 난이도별 배율이다.
    ///
    /// 밸런스를 자산으로 뺀 덕분에 난이도는 "그 자산에 곱하는 배율 세트"로 끝난다.
    /// 별도의 난이도 전용 수치표를 따로 만들지 않는다.
    /// </summary>
    [Serializable]
    public sealed class DifficultyMultipliers
    {
        public DifficultyLevel Level = DifficultyLevel.Normal;

        /// <summary>플레이어 최면 속도 배율이다.</summary>
        public float HypnosisSpeed = 1.0f;

        /// <summary>NPC 충동 상승 배율이다.</summary>
        public float ImpulseBuild = 1.0f;

        /// <summary>폭주 경고 시간 배율이다. 기획서 A.6: 스토리 2.0초 / 노멀 1.5초 / 챌린지 1.2초.</summary>
        public float RampageWarning = 1.0f;

        /// <summary>경쟁자 쟁탈 속도 배율이다.</summary>
        public float OpponentPressure = 1.0f;

        /// <summary>스테이지 제한 시간 배율이다.</summary>
        public float TimeLimit = 1.0f;

        /// <summary>목표 정기 배율이다.</summary>
        public float TargetEssence = 1.0f;

        public string DisplayName
        {
            get
            {
                switch (Level)
                {
                    case DifficultyLevel.Story:
                        return "스토리";

                    case DifficultyLevel.Challenge:
                        return "챌린지";

                    case DifficultyLevel.Normal:
                    default:
                        return "노멀";
                }
            }
        }
    }

    public static class DifficultyTable
    {
        private static readonly DifficultyMultipliers[] Presets =
        {
            new DifficultyMultipliers
            {
                Level = DifficultyLevel.Story,
                HypnosisSpeed = 1.25f,
                ImpulseBuild = 0.75f,
                RampageWarning = 1.33f,
                OpponentPressure = 0.75f,
                TimeLimit = 1.20f,
                TargetEssence = 0.80f
            },

            new DifficultyMultipliers
            {
                Level = DifficultyLevel.Normal,
                HypnosisSpeed = 1.00f,
                ImpulseBuild = 1.00f,
                RampageWarning = 1.00f,
                OpponentPressure = 1.00f,
                TimeLimit = 1.00f,
                TargetEssence = 1.00f
            },

            new DifficultyMultipliers
            {
                Level = DifficultyLevel.Challenge,
                HypnosisSpeed = 0.85f,
                ImpulseBuild = 1.25f,
                RampageWarning = 0.80f,
                OpponentPressure = 1.30f,
                TimeLimit = 0.90f,
                TargetEssence = 1.20f
            }
        };

        public static int Count =>
            Presets.Length;

        public static DifficultyMultipliers Get(
            DifficultyLevel level)
        {
            for (int i = 0;
                 i < Presets.Length;
                 i++)
            {
                if (Presets[i].Level ==
                    level)
                {
                    return Presets[i];
                }
            }

            return Presets[1];
        }

        public static DifficultyMultipliers Normal =>
            Get(DifficultyLevel.Normal);
    }
}
