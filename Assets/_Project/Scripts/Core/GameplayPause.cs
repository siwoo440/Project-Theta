using UnityEngine;
using ProjectTheta.Presentation;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 강화 카드 선택처럼 게임을 완전히 멈춰야 할 때 쓰는 문이다.
    ///
    /// 기존 <c>PlayerSideViewController.SetInputLocked</c>는 포획·힘겨루기·종료가
    /// 함께 쓰는 bool 하나라서, 여기서 잠갔다가 포획이 풀면서 같이 풀어 버릴 수 있다.
    /// 그래서 멈춤은 별도로 두고, 여러 곳이 동시에 요청해도 전부 풀릴 때까지 유지한다.
    ///
    /// 입력을 읽는 컴포넌트는 Update 맨 앞에서 <see cref="IsPaused"/>를 보고 빠져나간다.
    /// 카드 연출처럼 멈춘 동안에도 움직여야 하는 것은 unscaledDeltaTime을 쓴다.
    /// </summary>
    public static class GameplayPause
    {
        private static int _requests;

        private static float _hitStopRemaining;

        private static float _hitStopScale =
            TimeScaleLogic.DefaultHitStopScale;

        /// <summary>
        /// 완전 정지 중인지다. 입력을 막는 쪽은 이것만 본다.
        /// 멈칫(히트스톱)은 0.06초 남짓이라 입력을 막지 않는다.
        /// </summary>
        public static bool IsPaused =>
            _requests > 0;

        public static bool IsHitStopping =>
            _hitStopRemaining > 0f;

        public static void Request()
        {
            _requests++;

            ApplyTimeScale();
        }

        public static void Release()
        {
            if (_requests <= 0)
            {
                return;
            }

            _requests--;

            ApplyTimeScale();
        }

        /// <summary>
        /// 짧은 멈칫을 건다 (19일차).
        /// 카드 화면 같은 완전 정지가 걸려 있으면 그쪽이 이긴다 (<see cref="TimeScaleLogic.Resolve"/>).
        /// </summary>
        public static void HitStop(
            float seconds,
            float scale)
        {
            _hitStopRemaining =
                TimeScaleLogic.Extend(
                    _hitStopRemaining,
                    seconds);

            _hitStopScale = scale;

            ApplyTimeScale();
        }

        /// <summary>멈칫 시간을 실제 시간으로 흘린다. 연출 담당이 매 프레임 호출한다.</summary>
        public static void TickHitStop(
            float unscaledDeltaTime)
        {
            if (_hitStopRemaining <= 0f)
            {
                return;
            }

            _hitStopRemaining =
                TimeScaleLogic.Tick(
                    _hitStopRemaining,
                    unscaledDeltaTime);

            if (_hitStopRemaining <= 0f)
            {
                ApplyTimeScale();
            }
        }

        /// <summary>진행 중인 멈칫만 끝낸다. 연출 담당이 사라질 때 멈칫이 남지 않게 한다.</summary>
        public static void CancelHitStop()
        {
            _hitStopRemaining = 0f;

            ApplyTimeScale();
        }

        private static void ApplyTimeScale()
        {
            Time.timeScale =
                TimeScaleLogic.Resolve(
                    _requests,
                    _hitStopRemaining,
                    _hitStopScale);
        }

        /// <summary>
        /// 모든 요청을 비우고 시간을 되돌린다.
        /// 멈춘 채로 씬을 나가면 다음 씬이 멈춘 상태로 시작하므로 반드시 필요하다.
        /// </summary>
        public static void ForceClear()
        {
            _requests = 0;
            _hitStopRemaining = 0f;

            Time.timeScale = 1f;
        }

        /// <summary>
        /// 이 프로젝트는 도메인 리로드를 끄고 있어서 static이 플레이 사이에 남는다.
        /// 멈춘 채로 플레이를 끝냈다면 다음 플레이가 멈춘 채로 시작되므로 진입 시 비운다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            ForceClear();
        }
    }
}
