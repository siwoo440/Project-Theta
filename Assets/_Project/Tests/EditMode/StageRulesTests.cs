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
        public void ComputeProductionPerSecond_MultipliesFollowers()
        {
            Assert.AreEqual(
                7,
                StageRules.ComputeProductionPerSecond(
                    7,
                    1));
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
    }
}
