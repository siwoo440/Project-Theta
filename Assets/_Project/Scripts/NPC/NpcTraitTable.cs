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
                new Color(1.00f, 0.38f, 0.46f, 1f))
        };

        public static int Count =>
            Profiles.Length;

        public static NpcTraitProfile Get(
            NpcTrait trait)
        {
            for (int i = 0;
                 i < Profiles.Length;
                 i++)
            {
                if (Profiles[i].Trait ==
                    trait)
                {
                    return Profiles[i];
                }
            }

            return Profiles[0];
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
