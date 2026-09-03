using NUnit.Framework;
using ProjectTheta.Core;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class StageQuotaTests
    {
        [Test]
        public void Add_CompletesQuota_WhenTargetReached()
        {
            StageQuota quota = new StageQuota(100); // 할당량 생성

            quota.Add(40); // 첫 회수
            quota.Add(60); // 두 번째 회수

            Assert.IsTrue(quota.IsComplete); // 완료 확인
            Assert.AreEqual(100, quota.Current); // 누적 확인
        }

        [Test]
        public void Add_ClampsAtTarget()
        {
            StageQuota quota = new StageQuota(100); // 할당량 생성

            quota.Add(150); // 초과 회수

            Assert.AreEqual(100, quota.Current); // 상한 확인
        }
    }
}
