using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class HypnosisMeter
    {
        private readonly float _maxValue; // 최대 게이지
        private readonly float _buildRatePerSecond; // 상승 속도
        private readonly float _decayRatePerSecond; // 감소 속도
        private readonly float _gracePeriod; // 이탈 유예
        private float _graceRemaining; // 남은 유예

        public HypnosisMeter(float maxValue, float buildRatePerSecond, float decayRatePerSecond, float gracePeriod)
        {
            _maxValue = Mathf.Max(1f, maxValue); // 최대값 보정
            _buildRatePerSecond = Mathf.Max(0f, buildRatePerSecond); // 상승값 보정
            _decayRatePerSecond = Mathf.Max(0f, decayRatePerSecond); // 감소값 보정
            _gracePeriod = Mathf.Max(0f, gracePeriod); // 유예값 보정
            Value = 0f; // 초기 게이지
            _graceRemaining = 0f; // 초기 유예
        }

        public float Value { get; private set; } // 현재 게이지
        public float Normalized => Value / _maxValue; // 정규화 게이지
        public bool IsComplete => Value >= _maxValue; // 완료 여부

        public void Build(float deltaTime, float gainMultiplier)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 시간 보정
            float safeMultiplier = Mathf.Max(0f, gainMultiplier); // 배율 보정
            Value = Mathf.Min(_maxValue, Value + (_buildRatePerSecond * safeMultiplier * safeDeltaTime)); // 게이지 상승
            _graceRemaining = _gracePeriod; // 유예 갱신
        }

        public void BeginGrace()
        {
            _graceRemaining = _gracePeriod; // 유예 시작
        }

        public void Decay(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 시간 보정
            if (_graceRemaining > 0f) // 유예 확인
            {
                _graceRemaining = Mathf.Max(0f, _graceRemaining - safeDeltaTime); // 유예 감소
                return; // 감소 중단
            }

            Value = Mathf.Max(0f, Value - (_decayRatePerSecond * safeDeltaTime)); // 게이지 감소
        }

        public void Reset()
        {
            Value = 0f; // 게이지 초기화
            _graceRemaining = 0f; // 유예 초기화
        }
    }
}
