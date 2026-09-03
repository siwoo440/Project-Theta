using NUnit.Framework;
using ProjectTheta.Core;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class HypnosisMeterTests
    {
        [Test]
        public void Build_IncreasesValue_ByConfiguredRate()
        {
            HypnosisMeter meter = new HypnosisMeter(100f, 20f, 10f, 1.5f); // 게이지 생성

            meter.Build(1f, 1f); // 1초 최면

            Assert.AreEqual(20f, meter.Value, 0.001f); // 상승 확인
        }

        [Test]
        public void FocusLoss_UsesGraceBeforeDecay()
        {
            HypnosisMeter meter = new HypnosisMeter(100f, 20f, 10f, 1.5f); // 게이지 생성
            meter.Build(1f, 1f); // 초기 충전
            meter.BeginGrace(); // 유예 시작

            meter.Decay(1f); // 유예 중 감소 시도

            Assert.AreEqual(20f, meter.Value, 0.001f); // 유예 유지
        }

        [Test]
        public void Decay_ReducesValue_AfterGraceEnds()
        {
            HypnosisMeter meter = new HypnosisMeter(100f, 20f, 10f, 1.5f); // 게이지 생성
            meter.Build(1f, 1f); // 초기 충전
            meter.BeginGrace(); // 유예 시작
            meter.Decay(1.5f); // 유예 소진
            meter.Decay(1f); // 감소 적용

            Assert.AreEqual(10f, meter.Value, 0.001f); // 감소 확인
        }
    }
}
