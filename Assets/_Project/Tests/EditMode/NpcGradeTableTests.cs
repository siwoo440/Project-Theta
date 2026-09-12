using NUnit.Framework;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcGradeTableTests
    {
        [Test]
        public void Table_Contains_All_Five_Grades()
        {
            Assert.AreEqual(
                5,
                NpcGradeTable.Count);

            Assert.AreEqual(
                NpcGrade.Common,
                NpcGradeTable.Get(
                    NpcGrade.Common).Grade);

            Assert.AreEqual(
                NpcGrade.Awakened,
                NpcGradeTable.Get(
                    NpcGrade.Awakened).Grade);
        }

        [Test]
        public void Higher_Grade_Hypnotizes_Slower()
        {
            float common =
                NpcGradeTable.Get(
                    NpcGrade.Common).HypnosisBuildPerSecond;

            float skilled =
                NpcGradeTable.Get(
                    NpcGrade.Skilled).HypnosisBuildPerSecond;

            float rare =
                NpcGradeTable.Get(
                    NpcGrade.Rare).HypnosisBuildPerSecond;

            float awakened =
                NpcGradeTable.Get(
                    NpcGrade.Awakened).HypnosisBuildPerSecond;

            Assert.Greater(
                common,
                skilled);

            Assert.Greater(
                skilled,
                rare);

            Assert.Greater(
                rare,
                awakened);
        }

        [Test]
        public void Hypnosis_Durations_Match_Design_Document_Ranges()
        {
            // 기획서 20장: 일반 2.0~3.0초 / 숙련 3.5~5.0초 / 희귀 6.0~9.0초
            float common =
                NpcGradeTable.Get(
                    NpcGrade.Common).FullHypnosisSeconds;

            float skilled =
                NpcGradeTable.Get(
                    NpcGrade.Skilled).FullHypnosisSeconds;

            float rare =
                NpcGradeTable.Get(
                    NpcGrade.Rare).FullHypnosisSeconds;

            Assert.That(
                common,
                Is.InRange(
                    2.0f,
                    3.0f));

            Assert.That(
                skilled,
                Is.InRange(
                    3.5f,
                    5.0f));

            Assert.That(
                rare,
                Is.InRange(
                    6.0f,
                    9.0f));
        }

        [Test]
        public void Essence_Values_Match_Design_Document()
        {
            // 기획서 20장: 일반 10 / 숙련 20 / 희귀 40
            Assert.AreEqual(
                10,
                NpcGradeTable.Get(
                    NpcGrade.Common).EssenceValue);

            Assert.AreEqual(
                20,
                NpcGradeTable.Get(
                    NpcGrade.Skilled).EssenceValue);

            Assert.AreEqual(
                40,
                NpcGradeTable.Get(
                    NpcGrade.Rare).EssenceValue);
        }

        [Test]
        public void Common_Grade_Is_The_Essence_Multiplier_Reference()
        {
            Assert.AreEqual(
                1f,
                NpcGradeTable.Get(
                    NpcGrade.Common).EssenceMultiplier,
                0.0001f);

            Assert.AreEqual(
                4f,
                NpcGradeTable.Get(
                    NpcGrade.Rare).EssenceMultiplier,
                0.0001f);
        }

        [Test]
        public void Higher_Grade_Builds_Impulse_Faster()
        {
            Assert.Greater(
                NpcGradeTable.Get(
                    NpcGrade.Rare).ImpulseBuildMultiplier,
                NpcGradeTable.Get(
                    NpcGrade.Common).ImpulseBuildMultiplier);
        }

        [Test]
        public void Unknown_Grade_Falls_Back_To_First_Profile()
        {
            NpcGradeProfile fallback =
                NpcGradeTable.Get(
                    (NpcGrade)999);

            Assert.AreEqual(
                NpcGrade.Common,
                fallback.Grade);
        }
    }
}
