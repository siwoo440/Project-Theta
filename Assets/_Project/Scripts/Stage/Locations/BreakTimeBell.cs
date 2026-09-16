using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Player;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 기업 연수원의 쉬는 시간 종이다 (22일차, 부록 B.3 [0]).
    ///
    /// 40초마다 종이 울리고 8초 동안 복도 NPC가 1.6배 빨라진다.
    /// 붐빌 때 최면 대상은 가까이 오지만, 동행자와 뒤섞이고 감시 시야를 가리기도 한다.
    /// </summary>
    public sealed class BreakTimeBell : MonoBehaviour
    {
        private static BreakTimeBell _current;

        private StageSessionController _stage;
        private float _elapsed;
        private bool _wasBreak;

        /// <summary>NPC 이동 속도에 곱한다. 연수원이 아니거나 쉬는 시간이 아니면 1이다.</summary>
        public static float NpcSpeedMultiplier =>
            _current == null
                ? 1f
                : BreakTimeLogic.GetNpcSpeedMultiplier(
                    _current._elapsed);

        public static bool IsBreak =>
            _current != null &&
            BreakTimeLogic.IsBreak(
                _current._elapsed);

        public void Configure(
            StageSessionController stage)
        {
            _stage = stage;
            _current = this;
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            _elapsed +=
                Time.deltaTime;

            bool isBreak =
                BreakTimeLogic.IsBreak(
                    _elapsed);

            if (isBreak &&
                !_wasBreak)
            {
                StageMoments.RaiseBreakTimeStarted();
            }

            _wasBreak = isBreak;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
