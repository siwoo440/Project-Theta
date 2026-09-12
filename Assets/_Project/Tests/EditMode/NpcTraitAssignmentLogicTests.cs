using NUnit.Framework;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcTraitAssignmentLogicTests
    {
        [Test]
        public void Common_Grade_Can_Resolve_To_A_Plain_Npc()
        {
            // 일반 등급은 보조 특성 풀의 절반이 일반형이므로
            // 아무 저항 특성도 없는 평범한 NPC가 실제로 등장해야 한다.
            NpcTrait[] traits =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Common,
                    0f);

            Assert.AreEqual(
                1,
                traits.Length);

            Assert.AreEqual(
                NpcTrait.Plain,
                traits[0]);
        }

        [Test]
        public void Plain_Is_Dropped_When_A_Real_Trait_Is_Present()
        {
            // 일반 등급 고정 특성은 일반형이지만 보조로 도주형이 붙으면 일반형은 사라진다.
            NpcTrait[] traits =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Common,
                    1f);

            Assert.IsFalse(
                NpcTraitTable.Contains(
                    traits,
                    NpcTrait.Plain));

            Assert.IsTrue(
                NpcTraitTable.Contains(
                    traits,
                    NpcTrait.Fleer));
        }

        [Test]
        public void Fixed_Traits_Are_Always_Present()
        {
            for (float r = 0f;
                 r <= 1f;
                 r += 0.1f)
            {
                NpcTrait[] rare =
                    NpcTraitAssignmentLogic.Resolve(
                        NpcGrade.Rare,
                        r);

                Assert.IsTrue(
                    NpcTraitTable.Contains(
                        rare,
                        NpcTrait.GazeAverter));

                Assert.IsTrue(
                    NpcTraitTable.Contains(
                        rare,
                        NpcTrait.Stubborn));
            }
        }

        [Test]
        public void Awakened_Always_Has_The_Awakening_Aura()
        {
            NpcTrait[] low =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Awakened,
                    0f);

            NpcTrait[] high =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Awakened,
                    1f);

            Assert.IsTrue(
                NpcTraitTable.Contains(
                    low,
                    NpcTrait.AwakeningAura));

            Assert.IsTrue(
                NpcTraitTable.Contains(
                    high,
                    NpcTrait.AwakeningAura));

            // 각성은 저항 특성을 가지므로 일반형이 섞이지 않는다.
            Assert.IsFalse(
                NpcTraitTable.Contains(
                    low,
                    NpcTrait.Plain));
        }

        [Test]
        public void Traits_Never_Duplicate()
        {
            for (float r = 0f;
                 r <= 1f;
                 r += 0.05f)
            {
                NpcTrait[] traits =
                    NpcTraitAssignmentLogic.Resolve(
                        NpcGrade.Skilled,
                        r);

                for (int i = 0;
                     i < traits.Length;
                     i++)
                {
                    for (int j = i + 1;
                         j < traits.Length;
                         j++)
                    {
                        Assert.AreNotEqual(
                            traits[i],
                            traits[j]);
                    }
                }
            }
        }

        [Test]
        public void Random_Value_Out_Of_Range_Is_Clamped()
        {
            Assert.AreEqual(
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Common,
                    0f),
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Common,
                    -5f));

            Assert.AreEqual(
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Common,
                    1f),
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Common,
                    5f));
        }

        [Test]
        public void Random_Value_Changes_The_Support_Trait()
        {
            // 재도전마다 보조 특성이 달라지는지 확인한다.
            NpcTrait first =
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Skilled,
                    0f);

            NpcTrait last =
                NpcTraitAssignmentLogic.PickRandomTrait(
                    NpcGrade.Skilled,
                    1f);

            Assert.AreNotEqual(
                first,
                last);
        }
    }
}
