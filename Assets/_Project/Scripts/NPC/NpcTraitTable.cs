using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>특성 하나가 가지는 수치 보정과 표시 정보다.</summary>
    public sealed class NpcTraitProfile
    {
        public NpcTraitProfile(
            NpcTrait trait,
            string displayName,
            float hypnosisSpeedMultiplier,
            float essenceMultiplier,
            Color markerColor)
        {
            Trait = trait;
            DisplayName = displayName;
            HypnosisSpeedMultiplier = hypnosisSpeedMultiplier;
            EssenceMultiplier = essenceMultiplier;
            MarkerColor = markerColor;
        }

        public NpcTrait Trait { get; }

        public string DisplayName { get; }

        public float HypnosisSpeedMultiplier { get; }

        public float EssenceMultiplier { get; }

        public Color MarkerColor { get; }
    }

    public static class NpcTraitTable
    {
        /// <summary>자산에서 읽은 특성 표다. null이면 아래 기본 표를 쓴다.</summary>
        public static NpcTraitProfile[] Override { get; set; }

        private static NpcTraitProfile[] Active =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Profiles;

        private static readonly NpcTraitProfile[] Profiles =
        {
            new NpcTraitProfile(
                NpcTrait.Plain,
                "일반형",
                1.00f,
                1.00f,
                new Color(0.72f, 0.74f, 0.78f, 1f)),

            new NpcTraitProfile(
                NpcTrait.GazeAverter,
                "시선 회피형",
                1.00f,
                1.10f,
                new Color(0.42f, 0.86f, 0.72f, 1f)),

            new NpcTraitProfile(
                NpcTrait.Fleer,
                "도주형",
                1.00f,
                1.15f,
                new Color(0.98f, 0.72f, 0.34f, 1f)),

            new NpcTraitProfile(
                NpcTrait.Stubborn,
                "고저항형",
                0.60f,
                1.50f,
                new Color(0.62f, 0.58f, 0.98f, 1f)),

            new NpcTraitProfile(
                NpcTrait.AwakeningAura,
                "각성 지원형",
                0.85f,
                1.30f,
                new Color(1.00f, 0.38f, 0.46f, 1f)),

            new NpcTraitProfile(
                NpcTrait.Counter,
                "반격형",
                1.00f,
                1.25f,
                new Color(0.98f, 0.94f, 0.42f, 1f))
        };

        public static int Count =>
            Active.Length;

        public static NpcTraitProfile Get(
            NpcTrait trait)
        {
            NpcTraitProfile[] table =
                Active;

            for (int i = 0;
                 i < table.Length;
                 i++)
            {
                if (table[i].Trait ==
                    trait)
                {
                    return table[i];
                }
            }

            return table[0];
        }

        public static bool Contains(
            NpcTrait[] traits,
            NpcTrait trait)
        {
            if (traits == null)
            {
                return false;
            }

            for (int i = 0;
                 i < traits.Length;
                 i++)
            {
                if (traits[i] ==
                    trait)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>보유한 모든 특성의 최면 속도 배율을 곱한 값이다.</summary>
        public static float AggregateHypnosisSpeedMultiplier(
            NpcTrait[] traits)
        {
            if (traits == null ||
                traits.Length == 0)
            {
                return 1f;
            }

            float result =
                1f;

            for (int i = 0;
                 i < traits.Length;
                 i++)
            {
                result *=
                    Get(traits[i]).
                        HypnosisSpeedMultiplier;
            }

            return result;
        }

        /// <summary>보유한 모든 특성의 정기 가치 배율을 곱한 값이다.</summary>
        public static float AggregateEssenceMultiplier(
            NpcTrait[] traits)
        {
            if (traits == null ||
                traits.Length == 0)
            {
                return 1f;
            }

            float result =
                1f;

            for (int i = 0;
                 i < traits.Length;
                 i++)
            {
                result *=
                    Get(traits[i]).
                        EssenceMultiplier;
            }

            return result;
        }
    }
}
