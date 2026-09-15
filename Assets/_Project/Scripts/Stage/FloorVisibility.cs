using UnityEngine;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 지금 화면에 보이는 층이다.
    ///
    /// 층은 한 씬에 세로로 쌓여 있어서 화면에는 항상 한 층만 보인다.
    /// 머리 위 게이지처럼 "보일 때만 의미 있는" 표현은 이 값을 보고
    /// 다른 층의 계산을 건너뛴다.
    ///
    /// <see cref="FloorTransitionController"/>가 판 시작과 층 이동 때 갱신한다.
    /// </summary>
    public static class FloorVisibility
    {
        public static int ViewFloor { get; set; }

        /// <summary>도메인 리로드가 꺼져 있으면 지난 판의 층이 남으므로 진입 시 1층으로 되돌린다.</summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            ViewFloor = 0;
        }
    }
}
