using NUnit.Framework;
using ProjectTheta.Stage;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class StageRulesTests
    {
        [Test]
        public void AddEssence_ClampsToTarget()
        {
            Assert.AreEqual(
                200,
                StageRules.AddEssence(
                    198,
                    5,
                    200));
        }


        [Test]
        public void TickTime_DoesNotGoBelowZero()
        {
            Assert.AreEqual(
                0f,
                StageRules.TickTime(
                    0.25f,
                    1f),
                0.001f);
        }

        [Test]
        public void ResolveState_TargetReached_Clears()
        {
            Assert.AreEqual(
                StageState.Cleared,
                StageRules.ResolveState(
                    30f,
                    200,
                    200,
                    50));
        }

        [Test]
        public void ResolveState_TimeExpired_FailsByTime()
        {
            Assert.AreEqual(
                StageState.FailedByTime,
                StageRules.ResolveState(
                    0f,
                    199,
                    200,
                    50));
        }

        [Test]
        public void ResolveState_HealthDepleted_FailsByHealth()
        {
            Assert.AreEqual(
                StageState.FailedByHealth,
                StageRules.ResolveState(
                    30f,
                    200,
                    200,
                    0));
        }
    
        [Test]
        public void ScaleEssenceReward_Applies_Grade_Multiplier()
        {
            Assert.AreEqual(
                5,
                StageRules.ScaleEssenceReward(
                    5,
                    1f));

            Assert.AreEqual(
                20,
                StageRules.ScaleEssenceReward(
                    5,
                    4f));
        }

        [Test]
        public void ScaleEssenceReward_Keeps_At_Least_One_For_Positive_Base()
        {
            Assert.AreEqual(
                1,
                StageRules.ScaleEssenceReward(
                    1,
                    0f));

            Assert.AreEqual(
                1,
                StageRules.ScaleEssenceReward(
                    1,
                    0.2f));
        }

        [Test]
        public void ScaleEssenceReward_Keeps_Zero_Base_At_Zero()
        {
            Assert.AreEqual(
                0,
                StageRules.ScaleEssenceReward(
                    0,
                    4f));
        }
}
}
