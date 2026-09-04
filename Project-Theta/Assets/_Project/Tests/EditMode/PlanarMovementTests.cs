using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Player;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class PlanarMovementTests
    {
        [Test]
        public void CalculateVelocity_DiagonalInput_DoesNotExceedSpeed()
        {
            Vector2 velocity = PlanarMovement.CalculateVelocity(new Vector2(1f, 1f), 5f); // 대각 속도 계산
            Assert.That(velocity.magnitude, Is.EqualTo(5f).Within(0.001f)); // 속도 상한 확인
        }

        [Test]
        public void ClampPosition_OutsideVerticalBounds_ClampsY()
        {
            Vector2 clamped = PlanarMovement.ClampPosition(new Vector2(2f, 10f), -20f, 20f, -2.5f, 2.5f); // 위치 제한 계산
            Assert.That(clamped.y, Is.EqualTo(2.5f)); // 상단 제한 확인
        }

        [Test]
        public void ResolveDashDirection_UsesLastDirectionWhenInputIsZero()
        {
            Vector2 direction = PlanarMovement.ResolveDashDirection(Vector2.zero, new Vector2(-1f, 0.25f)); // 대시 방향 계산
            Assert.That(direction.x, Is.LessThan(0f)); // 마지막 방향 사용 확인
            Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.001f)); // 방향 정규화 확인
        }
    }
}
