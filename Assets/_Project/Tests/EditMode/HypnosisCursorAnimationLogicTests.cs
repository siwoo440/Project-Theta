using NUnit.Framework;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class HypnosisCursorAnimationLogicTests
    {
        [Test]
        public void GetFrameIndex_AlternatesTwoFrames()
        {
            Assert.AreEqual(
                0,
                HypnosisCursorAnimationLogic.
                GetFrameIndex(0f, 0.14f));

            Assert.AreEqual(
                1,
                HypnosisCursorAnimationLogic.
                GetFrameIndex(0.14f, 0.14f));

            Assert.AreEqual(
                0,
                HypnosisCursorAnimationLogic.
                GetFrameIndex(0.28f, 0.14f));
        }
    }
}
