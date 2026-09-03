using NUnit.Framework;
using ProjectTheta.Core;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ImpulseMeterTests
    {
        [Test]
        public void Following_IncreasesImpulse()
        {
            ImpulseMeter meter = new ImpulseMeter(100f, 70f, 10f, 20f); // 충동 생성

            meter.TickFollowing(2f, 1f); // 2초 동행

            Assert.AreEqual(20f, meter.Value, 0.001f); // 증가 확인
        }

        [Test]
        public void Warning_ActivatesAtThreshold()
        {
            ImpulseMeter meter = new ImpulseMeter(100f, 70f, 10f, 20f); // 충동 생성
            meter.SetValue(70f); // 경고값 설정

            Assert.IsTrue(meter.IsWarning); // 경고 확인
        }

        [Test]
        public void Rampage_ActivatesAtMaximum()
        {
            ImpulseMeter meter = new ImpulseMeter(100f, 70f, 10f, 20f); // 충동 생성
            meter.SetValue(100f); // 최대값 설정

            Assert.IsTrue(meter.IsRampaging); // 폭주 확인
        }
    }
}
