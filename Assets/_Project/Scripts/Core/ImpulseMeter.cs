using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class ImpulseMeter
    {
        private readonly float _maxValue; // 최대 충동
        private readonly float _warningThreshold; // 경고 기준
        private readonly float _growthRatePerSecond; // 증가 속도
        private readonly float _recoveryRatePerSecond; // 회복 속도

        public ImpulseMeter(float maxValue, float warningThreshold, float growthRatePerSecond, float recoveryRatePerSecond)
        {
            _maxValue = Mathf.Max(1f, maxValue); // 최대값 보정
            _warningThreshold = Mathf.Clamp(warningThreshold, 0f, _maxValue); // 경고값 보정
            _growthRatePerSecond = Mathf.Max(0f, growthRatePerSecond); // 증가값 보정
            _recoveryRatePerSecond = Mathf.Max(0f, recoveryRatePerSecond); // 회복값 보정
            Value = 0f; // 초기 충동
        }

        public float Value { get; private set; } // 현재 충동
        public float Normalized => Value / _maxValue; // 정규화 충동
        public bool IsWarning => Value >= _warningThreshold; // 경고 여부
        public bool IsRampaging => Value >= _maxValue; // 폭주 여부

        public void TickFollowing(float deltaTime, float growthMultiplier)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 시간 보정
            float safeMultiplier = Mathf.Max(0f, growthMultiplier); // 배율 보정
            Value = Mathf.Min(_maxValue, Value + (_growthRatePerSecond * safeMultiplier * safeDeltaTime)); // 충동 증가
        }

        public void TickRecovery(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 시간 보정
            Value = Mathf.Max(0f, Value - (_recoveryRatePerSecond * safeDeltaTime)); // 충동 감소
        }

        public void SetValue(float value)
        {
            Value = Mathf.Clamp(value, 0f, _maxValue); // 충동 지정
        }

        public void Reset()
        {
            Value = 0f; // 충동 초기화
        }
    }
}
