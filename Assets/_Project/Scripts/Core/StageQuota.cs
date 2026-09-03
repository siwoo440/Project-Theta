using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class StageQuota
    {
        public StageQuota(int target)
        {
            Target = Mathf.Max(1, target); // 목표 보정
            Current = 0; // 현재값 초기화
        }

        public int Target { get; } // 목표 정기
        public int Current { get; private set; } // 현재 정기
        public bool IsComplete => Current >= Target; // 완료 여부

        public void Add(int amount)
        {
            int safeAmount = Mathf.Max(0, amount); // 회수량 보정
            Current = Mathf.Min(Target, Current + safeAmount); // 정기 누적
        }
    }
}
