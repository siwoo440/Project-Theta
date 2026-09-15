using UnityEngine;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 화면 흔들림이다.
    ///
    /// 카메라 추적(<see cref="ProjectTheta.Core.CameraFollow2D"/>)이 매 프레임 <see cref="Offset"/>을 더한다.
    /// 흔들림은 실제 시간으로 흐른다. 멈칫 중에도 흔들려야 타격감이 살기 때문이다.
    ///
    /// 설정에서 끄면 요청을 받아도 흔들지 않는다 (멀미 대비).
    /// </summary>
    public static class CameraShake
    {
        private static float _elapsed;
        private static float _duration;
        private static float _strength;

        /// <summary>플레이어가 허브 설정에서 끌 수 있다.</summary>
        public static bool Enabled { get; set; } = true;

        public static Vector3 Offset { get; private set; }

        /// <summary>
        /// 흔들림을 건다. 진행 중인 흔들림보다 약하면 무시한다.
        /// 약한 흔들림이 강한 흔들림을 덮어써서 끊기면 어색하기 때문이다.
        /// </summary>
        public static void Add(
            float strength,
            float duration)
        {
            if (!Enabled ||
                strength <= 0f ||
                duration <= 0f)
            {
                return;
            }

            float current =
                VfxCurves.ShakeAmplitude(
                    _elapsed,
                    _duration,
                    _strength);

            if (strength < current)
            {
                return;
            }

            _strength = strength;
            _duration = duration;
            _elapsed = 0f;
        }

        public static void Tick(
            float unscaledDeltaTime)
        {
            if (_duration <= 0f)
            {
                Offset = Vector3.zero;

                return;
            }

            _elapsed += unscaledDeltaTime;

            float amplitude =
                Enabled
                    ? VfxCurves.ShakeAmplitude(
                        _elapsed,
                        _duration,
                        _strength)
                    : 0f;

            if (amplitude <= 0f)
            {
                Stop();

                return;
            }

            VfxCurves.ShakeDirection(
                Time.unscaledTime,
                out float x,
                out float y);

            Offset =
                new Vector3(
                    x * amplitude,
                    y * amplitude,
                    0f);
        }

        public static void Stop()
        {
            _elapsed = 0f;
            _duration = 0f;
            _strength = 0f;

            Offset = Vector3.zero;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Stop();

            Enabled = true;
        }
    }
}
