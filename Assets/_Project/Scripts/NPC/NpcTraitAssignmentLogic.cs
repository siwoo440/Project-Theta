using System.Collections.Generic;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 등급별 고정 특성과 런마다 달라지는 보조 특성 1개를 결정한다.
    /// 기획서 A.5: "핵심 특성은 유형별로 고정하고 재도전 시 보조 특성 1개를 무작위로 부여할 수 있다."
    /// </summary>
    public static class NpcTraitAssignmentLogic
    {
        private static readonly NpcTrait[] CommonFixed =
            { NpcTrait.Plain };

        private static readonly NpcTrait[] CommonPool =
        {
            NpcTrait.Plain,
            NpcTrait.Plain,
            NpcTrait.GazeAverter,
            NpcTrait.Fleer
        };

        private static readonly NpcTrait[] SkilledFixed =
            { NpcTrait.GazeAverter };

        private static readonly NpcTrait[] SkilledPool =
        {
            NpcTrait.Plain,
            NpcTrait.Fleer,
            NpcTrait.Stubborn,
            NpcTrait.Counter
        };

        private static readonly NpcTrait[] RareFixed =
            { NpcTrait.GazeAverter, NpcTrait.Stubborn };

        private static readonly NpcTrait[] RarePool =
        {
            NpcTrait.Plain,
            NpcTrait.Fleer,
            NpcTrait.Counter
        };

        private static readonly NpcTrait[] SpecialFixed =
            { NpcTrait.Stubborn, NpcTrait.Counter };

        private static readonly NpcTrait[] SpecialPool =
        {
            NpcTrait.Plain,
            NpcTrait.GazeAverter,
            NpcTrait.Fleer
        };

        private static readonly NpcTrait[] AwakenedFixed =
            { NpcTrait.AwakeningAura, NpcTrait.Stubborn };

        private static readonly NpcTrait[] AwakenedPool =
            { NpcTrait.Plain };

        public static NpcTrait[] GetFixedTraits(
            NpcGrade grade)
        {
            switch (grade)
            {
                case NpcGrade.Skilled:
                    return SkilledFixed;

                case NpcGrade.Rare:
                    return RareFixed;

                case NpcGrade.Special:
                    return SpecialFixed;

                case NpcGrade.Awakened:
                    return AwakenedFixed;

                case NpcGrade.Common:
                default:
                    return CommonFixed;
            }
        }

        public static NpcTrait[] GetRandomPool(
            NpcGrade grade)
        {
            switch (grade)
            {
                case NpcGrade.Skilled:
                    return SkilledPool;

                case NpcGrade.Rare:
                    return RarePool;

                case NpcGrade.Special:
                    return SpecialPool;

                case NpcGrade.Awakened:
                    return AwakenedPool;

                case NpcGrade.Common:
                default:
                    return CommonPool;
            }
        }

        /// <summary>0~1 난수 하나로 보조 특성을 고른다.</summary>
        public static NpcTrait PickRandomTrait(
            NpcGrade grade,
            float randomValue)
        {
            NpcTrait[] pool =
                GetRandomPool(
                    grade);

            float clamped =
                randomValue < 0f
                    ? 0f
                    : randomValue > 1f
                        ? 1f
                        : randomValue;

            int index =
                (int)(clamped *
                      pool.Length);

            if (index >=
                pool.Length)
            {
                index =
                    pool.Length - 1;
            }

            return pool[index];
        }

        /// <summary>
        /// 고정 특성 + 보조 특성 1개를 합쳐 최종 특성 목록을 만든다.
        /// 일반형은 "특별한 저항이 없음"을 뜻하므로 다른 특성이 하나라도 있으면 제거한다.
        /// 남는 특성이 없으면 일반형 하나만 남는다.
        /// </summary>
        public static NpcTrait[] Resolve(
            NpcGrade grade,
            float randomValue)
        {
            List<NpcTrait> result =
                new List<NpcTrait>();

            NpcTrait[] fixedTraits =
                GetFixedTraits(
                    grade);

            for (int i = 0;
                 i < fixedTraits.Length;
                 i++)
            {
                if (!result.Contains(
                        fixedTraits[i]))
                {
                    result.Add(
                        fixedTraits[i]);
                }
            }

            NpcTrait randomTrait =
                PickRandomTrait(
                    grade,
                    randomValue);

            if (!result.Contains(
                    randomTrait))
            {
                result.Add(
                    randomTrait);
            }

            bool hasSpecialTrait =
                false;

            for (int i = 0;
                 i < result.Count;
                 i++)
            {
                if (result[i] !=
                    NpcTrait.Plain)
                {
                    hasSpecialTrait =
                        true;

                    break;
                }
            }

            if (hasSpecialTrait)
            {
                result.Remove(
                    NpcTrait.Plain);
            }

            if (result.Count == 0)
            {
                result.Add(
                    NpcTrait.Plain);
            }

            return result.ToArray();
        }
    }
}
