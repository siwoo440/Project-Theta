using NUnit.Framework;
using ProjectTheta.Balance;

namespace ProjectTheta.Tests.EditMode
{
    /// <summary>
    /// EditMode 테스트 전체가 시작되기 전에 밸런스 주입을 비운다.
    ///
    /// 이 프로젝트는 Enter Play Mode Options로 도메인 리로드를 끄고 있어서,
    /// 한 번 플레이하고 나면 자산에서 주입된 값이 static에 그대로 남는다.
    /// 그 상태로 테스트를 돌리면 "자산 없음 = 코드 기본값" 전제가 깨져
    /// 기본값을 검증하는 테스트들이 자산 값으로 실패한다.
    /// </summary>
    [SetUpFixture]
    public sealed class BalanceIsolationSetUp
    {
        [OneTimeSetUp]
        public void ClearInjectedBalance()
        {
            BalanceBootstrap.Reset();
        }

        [OneTimeTearDown]
        public void LeaveBalanceClean()
        {
            // 비워 둔 채로 끝낸다. 다음 플레이 진입 때 BalanceBootstrap이 다시 주입한다.
            BalanceBootstrap.Reset();
        }
    }
}
