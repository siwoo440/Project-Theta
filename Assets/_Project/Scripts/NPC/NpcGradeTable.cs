using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>등급 하나가 가지는 기본 수치 묶음이다.</summary>
    public sealed class NpcGradeProfile
    {
        public NpcGradeProfile(
            NpcGrade grade,
            string displayName,
            float hypnosisBuildPerSecond,
            int essenceValue,
            float impulseBuildMultiplier,
            int fixedTraitCount,
            int randomTraitCount,
            Color markerColor)
        {
            Grade = grade;
            DisplayName = displayName;
            HypnosisBuildPerSecond = hypnosisBuildPerSecond;
            EssenceValue = essenceValue;
            ImpulseBuildMultiplier = impulseBuildMultiplier;
            FixedTraitCount = fixedTraitCount;
            RandomTraitCount = randomTraitCount;
            MarkerColor = markerColor;
        }

        public NpcGrade Grade { get; }

        public string DisplayName { get; }

        /// <summary>중립 상태에서 플레이어 최면 게이지가 초당 오르는 양이다.</summary>
        public float HypnosisBuildPerSecond { get; }

        /// <summary>회수 시 확정되는 기준 정기 가치다.</summary>
        public int EssenceValue { get; }

        /// <summary>동행 중 충동 상승 속도 배율이다.</summary>
        public float ImpulseBuildMultiplier { get; }

        public int FixedTraitCount { get; }

        public int RandomTraitCount { get; }

        public Color MarkerColor { get; }

        /// <summary>최면 게이지 100을 채우는 데 걸리는 이론 시간이다.</summary>
        public float FullHypnosisSeconds =>
            HypnosisBuildPerSecond <=
            0.0001f
                ? float.MaxValue
                : 100f /
                  HypnosisBuildPerSecond;

        /// <summary>일반 등급을 1.0으로 본 정기 가치 배율이다.</summary>
        public float EssenceMultiplier =>
            EssenceValue /
            (float)NpcGradeTable.ReferenceEssenceValue;
    }

    /// <summary>
    /// 등급별 기본 수치 표다.
    /// 14일차 ScriptableObject 데이터화 때 이 표를 그대로 자산으로 옮길 수 있도록
    /// 값만 담은 정적 테이블로 유지한다.
    /// </summary>
    public static class NpcGradeTable
    {
        /// <summary>정기 가치 배율의 기준이 되는 일반 등급 가치다.</summary>
        public const int ReferenceEssenceValue = 10;

        /// <summary>
        /// 자산에서 읽은 등급 표다. null이면 아래 기본 표를 쓴다.
        /// 테스트에서는 항상 null이므로 Unity 자산 없이 그대로 동작한다.
        /// </summary>
        public static NpcGradeProfile[] Override { get; set; }

        private static NpcGradeProfile[] Active =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Profiles;

        private static readonly NpcGradeProfile[] Profiles =
        {
            new NpcGradeProfile(
                NpcGrade.Common,
                "일반",
                40f,
                10,
                1.00f,
                1,
                1,
                new Color(0.86f, 0.88f, 0.92f, 1f)),

            new NpcGradeProfile(
                NpcGrade.Skilled,
                "숙련",
                24f,
                20,
                1.10f,
                1,
                1,
                new Color(0.40f, 0.72f, 1.00f, 1f)),

            new NpcGradeProfile(
                NpcGrade.Rare,
                "희귀",
                14f,
                40,
                1.25f,
                2,
                1,
                new Color(0.76f, 0.44f, 1.00f, 1f)),

            new NpcGradeProfile(
                NpcGrade.Special,
                "특수",
                12f,
                60,
                1.30f,
                2,
                1,
                new Color(1.00f, 0.82f, 0.26f, 1f)),

            new NpcGradeProfile(
                NpcGrade.Awakened,
                "각성",
                8f,
                50,
                1.15f,
                2,
                1,
                new Color(1.00f, 0.46f, 0.24f, 1f))
        };

        public static int Count =>
            Active.Length;

        public static NpcGradeProfile Get(
            NpcGrade grade)
        {
            NpcGradeProfile[] table =
                Active;

            for (int i = 0;
                 i < table.Length;
                 i++)
            {
                if (table[i].Grade ==
                    grade)
                {
                    return table[i];
                }
            }

            return table[0];
        }

        public static NpcGradeProfile GetAt(
            int index)
        {
            NpcGradeProfile[] table =
                Active;

            return table[
                Mathf.Clamp(
                    index,
                    0,
                    table.Length - 1)];
        }
    }
}
