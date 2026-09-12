using NUnit.Framework;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcTraitEffectTests
    {
        [Test]
        public void Plain_Trait_Changes_Nothing()
        {
            NpcTrait[] plain =
                { NpcTrait.Plain };

            Assert.AreEqual(
                1f,
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    plain),
                0.0001f);

            Assert.AreEqual(
                1f,
                NpcTraitTable.AggregateEssenceMultiplier(
                    plain),
                0.0001f);
        }

        [Test]
        public void Stubborn_Slows_Hypnosis_And_Raises_Essence()
        {
            NpcTrait[] stubborn =
                { NpcTrait.Stubborn };

            Assert.AreEqual(
                0.60f,
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    stubborn),
                0.0001f);

            Assert.AreEqual(
                1.50f,
                NpcTraitTable.AggregateEssenceMultiplier(
                    stubborn),
                0.0001f);
        }

        [Test]
        public void Multiple_Traits_Multiply_Together()
        {
            NpcTrait[] traits =
            {
                NpcTrait.Stubborn,
                NpcTrait.Fleer
            };

            Assert.AreEqual(
                0.60f,
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    traits),
                0.0001f);

            Assert.AreEqual(
                1.50f * 1.15f,
                NpcTraitTable.AggregateEssenceMultiplier(
                    traits),
                0.0001f);
        }

        [Test]
        public void Empty_Or_Null_Trait_List_Is_Neutral()
        {
            Assert.AreEqual(
                1f,
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    null),
                0.0001f);

            Assert.AreEqual(
                1f,
                NpcTraitTable.AggregateEssenceMultiplier(
                    new NpcTrait[0]),
                0.0001f);
        }

        [Test]
        public void Every_Trait_Has_A_Profile()
        {
            // 특성이 추가돼도 테이블이 함께 갱신되는지 열거형 기준으로 확인한다.
            Assert.AreEqual(
                System.Enum.GetValues(
                    typeof(NpcTrait)).Length,
                NpcTraitTable.Count);

            foreach (NpcTrait trait in
                     System.Enum.GetValues(
                         typeof(NpcTrait)))
            {
                Assert.AreEqual(
                    trait,
                    NpcTraitTable.Get(
                        trait).Trait);
            }
        }

        [Test]
        public void Gaze_Averter_Blocks_Only_The_Front_Of_Each_Cycle()
        {
            // 주기 2.4초 중 앞 0.9초만 차단된다.
            Assert.IsTrue(
                NpcGazeAverterLogic.IsBlocking(
                    0.0f,
                    2.4f,
                    0.9f));

            Assert.IsTrue(
                NpcGazeAverterLogic.IsBlocking(
                    0.85f,
                    2.4f,
                    0.9f));

            Assert.IsFalse(
                NpcGazeAverterLogic.IsBlocking(
                    0.9f,
                    2.4f,
                    0.9f));

            Assert.IsFalse(
                NpcGazeAverterLogic.IsBlocking(
                    2.3f,
                    2.4f,
                    0.9f));
        }

        [Test]
        public void Gaze_Averter_Cycle_Repeats()
        {
            Assert.IsTrue(
                NpcGazeAverterLogic.IsBlocking(
                    2.4f,
                    2.4f,
                    0.9f));

            Assert.IsTrue(
                NpcGazeAverterLogic.IsBlocking(
                    4.8f + 0.5f,
                    2.4f,
                    0.9f));

            Assert.IsFalse(
                NpcGazeAverterLogic.IsBlocking(
                    4.8f + 1.5f,
                    2.4f,
                    0.9f));
        }

        [Test]
        public void Gaze_Averter_With_Zero_Block_Never_Blocks()
        {
            Assert.IsFalse(
                NpcGazeAverterLogic.IsBlocking(
                    0f,
                    2.4f,
                    0f));
        }

        [Test]
        public void Rare_Stubborn_Npc_Is_Slower_But_Worth_More_Than_Common()
        {
            // 등급과 특성이 함께 적용된 최종 결과를 확인한다.
            NpcTrait[] rareTraits =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Rare,
                    0f);

            float rareSpeed =
                NpcGradeTable.Get(
                    NpcGrade.Rare).HypnosisBuildPerSecond *
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    rareTraits);

            float rareEssence =
                NpcGradeTable.Get(
                    NpcGrade.Rare).EssenceValue *
                NpcTraitTable.AggregateEssenceMultiplier(
                    rareTraits);

            NpcTrait[] commonTraits =
                NpcTraitAssignmentLogic.Resolve(
                    NpcGrade.Common,
                    0f);

            float commonSpeed =
                NpcGradeTable.Get(
                    NpcGrade.Common).HypnosisBuildPerSecond *
                NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                    commonTraits);

            float commonEssence =
                NpcGradeTable.Get(
                    NpcGrade.Common).EssenceValue *
                NpcTraitTable.AggregateEssenceMultiplier(
                    commonTraits);

            Assert.Less(
                rareSpeed,
                commonSpeed);

            Assert.Greater(
                rareEssence,
                commonEssence);
        }
    }
}
