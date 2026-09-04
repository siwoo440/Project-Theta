using UnityEngine;
using ProjectTheta.Core;

namespace ProjectTheta.Stage
{
    public sealed class StageGoalManager : MonoBehaviour
    {
        [SerializeField] private int _targetEssence = 100; // 목표 정기
        [SerializeField] private float _timeLimitSeconds = 180f; // 제한 시간
        private StageQuota _quota; // 할당량

        public static StageGoalManager Instance { get; private set; } // 전역 참조
        public int CurrentEssence => _quota?.Current ?? 0; // 현재 정기
        public int TargetEssence => _quota?.Target ?? _targetEssence; // 목표 정기
        public float TimeRemaining { get; private set; } // 남은 시간
        public bool IsFinished { get; private set; } // 종료 여부
        public bool IsCleared { get; private set; } // 클리어 여부

        private void Awake()
        {
            Instance = this; // 전역 등록
            _quota = new StageQuota(_targetEssence); // 할당량 생성
            TimeRemaining = Mathf.Max(1f, _timeLimitSeconds); // 시간 설정
        }

        private void Update()
        {
            if (IsFinished) // 종료 확인
            {
                return; // 시간 중단
            }

            TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime); // 시간 감소
            if (_quota.IsComplete) // 목표 달성 확인
            {
                Finish(true); // 성공 종료
            }
            else if (TimeRemaining <= 0f) // 시간 종료 확인
            {
                Finish(false); // 실패 종료
            }
        }

        public void AddEssence(int amount)
        {
            if (IsFinished) // 종료 확인
            {
                return; // 회수 차단
            }

            _quota.Add(amount); // 정기 누적
            if (_quota.IsComplete) // 목표 달성 확인
            {
                Finish(true); // 성공 종료
            }
        }

        private void Finish(bool cleared)
        {
            IsFinished = true; // 종료 설정
            IsCleared = cleared; // 결과 설정
            Debug.Log(cleared ? "Project Theta Prototype: CLEAR" : "Project Theta Prototype: FAILED"); // 결과 출력
        }
    }
}
