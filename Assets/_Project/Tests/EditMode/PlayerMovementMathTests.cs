using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Player;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PlayerMovementMathTests
    {
        [Test]
        public void NormalizeInput_ClampsDiagonalMagnitudeToOne()
        {
            Vector2 normalized = PlayerMovementMath.NormalizeInput(new Vector2(1f, 1f));

            Assert.AreEqual(1f, normalized.magnitude, 0.001f);
        }

        [Test]
        public void NormalizeInput_PreservesCardinalMagnitude()
        {
            Vector2 normalized = PlayerMovementMath.NormalizeInput(new Vector2(1f, 0f));

            Assert.AreEqual(new Vector2(1f, 0f), normalized);
        }

        [Test]
        public void ResolveDashDirection_UsesCurrentInputWhenAvailable()
        {
            Vector2 direction = PlayerMovementMath.ResolveDashDirection(Vector2.up, Vector2.left);

            Assert.AreEqual(Vector2.up, direction);
        }

        [Test]
        public void ResolveDashDirection_UsesLastDirectionWhenIdle()
        {
            Vector2 direction = PlayerMovementMath.ResolveDashDirection(Vector2.zero, Vector2.left);

            Assert.AreEqual(Vector2.left, direction);
        }
    }
}
