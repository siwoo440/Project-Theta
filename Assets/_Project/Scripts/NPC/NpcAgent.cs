using UnityEngine;

namespace ProjectTheta.NPC
{
    public sealed class NpcAgent : MonoBehaviour
    {
        [SerializeField] private Vector2 _patrolExtents = new Vector2(2.5f, 1.1f); // 순찰 범위
        [SerializeField] private float _moveSpeed = 1.4f; // 이동 속도
        [SerializeField] private float _arrivalDistance = 0.12f; // 도착 거리
        [SerializeField] private float _minY = -2.2f; // 아래 순찰 한계
        [SerializeField] private float _maxY = 2.0f; // 위 순찰 한계
        private Vector3 _origin; // 시작 위치
        private Vector3 _patrolTarget; // 순찰 목표

        public NpcState State { get; private set; } = NpcState.Move; // 현재 상태

        private void Awake()
        {
            _origin = transform.position; // 시작점 저장
            ChoosePatrolTarget(); // 첫 목표 선택
        }

        private void Update()
        {
            if (State != NpcState.Move) // 이동 상태 확인
            {
                return; // 순찰 중단
            }

            transform.position = Vector3.MoveTowards(transform.position, _patrolTarget, _moveSpeed * Time.deltaTime); // 평면 순찰 이동
            if (Vector2.Distance(transform.position, _patrolTarget) <= _arrivalDistance) // 목표 도착 확인
            {
                ChoosePatrolTarget(); // 다음 목표 선택
            }
        }

        public void SetState(NpcState state)
        {
            State = state; // 상태 변경
        }

        private void ChoosePatrolTarget()
        {
            float x = Random.Range(-_patrolExtents.x, _patrolExtents.x); // 가로 목표 선택
            float y = Random.Range(-_patrolExtents.y, _patrolExtents.y); // 세로 목표 선택
            float targetY = Mathf.Clamp(_origin.y + y, _minY, _maxY); // 세로 범위 제한
            _patrolTarget = new Vector3(_origin.x + x, targetY, _origin.z); // 순찰 목표 설정
        }
    }
}
