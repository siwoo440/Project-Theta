using UnityEngine;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 지금 스테이지가 심야 모드인지다 (36일차).
    /// 부트스트랩이 도전 정보(<see cref="RunSession.NightMode"/>)를 보고 켠다.
    /// 적 증가 · 경계도 · 결과 화면이 여기를 읽는다.
    /// </summary>
    public static class NightModeState
    {
        public static bool Active { get; set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active = false;
        }
    }
}
