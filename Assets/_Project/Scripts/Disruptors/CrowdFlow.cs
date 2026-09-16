using UnityEngine;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 지하철 승강장 변경 안내로 생긴 인파 흐름이다 (24일차).
    /// 흐르는 동안 동행자가 한쪽으로 끌려가고 느려진다. 파동을 쓰면 잠깐 버틴다.
    /// 한 스테이지에 하나뿐이라 정적 상태로 둔다. 새 구역을 지을 때 스포너가 지운다.
    /// </summary>
    public static class CrowdFlow
    {
        private static float _remaining;
        private static float _anchorRemaining;
        private static int _direction;

        public static bool IsActive =>
            _remaining > 0f;

        public static bool IsAnchored =>
            _anchorRemaining > 0f;

        public static int Direction =>
            _direction;

        public static float Remaining =>
            Mathf.Max(0f, _remaining);

        public static float Drift =>
            CrowdFlowLogic.GetDrift(IsActive, IsAnchored, _direction);

        public static float SpeedMultiplier =>
            CrowdFlowLogic.GetSpeedMultiplier(IsActive, IsAnchored);

        public static void Begin(
            int direction,
            float seconds)
        {
            _direction = direction >= 0 ? 1 : -1;
            _remaining = Mathf.Max(_remaining, seconds);
        }

        /// <summary>파동을 쓰면 흐름 중에도 동행자가 버틴다.</summary>
        public static void Anchor(
            float seconds)
        {
            if (!IsActive)
            {
                return;
            }

            _anchorRemaining = Mathf.Max(_anchorRemaining, seconds);
        }

        public static void Tick(
            float deltaTime)
        {
            if (_remaining > 0f)
            {
                _remaining -= deltaTime;
            }

            if (_anchorRemaining > 0f)
            {
                _anchorRemaining -= deltaTime;
            }
        }

        public static void ResetState()
        {
            _remaining = 0f;
            _anchorRemaining = 0f;
            _direction = 0;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            ResetState();
        }
    }
}
