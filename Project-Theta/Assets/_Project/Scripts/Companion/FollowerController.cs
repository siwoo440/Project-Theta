using UnityEngine;
using ProjectTheta.Player;

namespace ProjectTheta.Companion
{
    public sealed class FollowerController : MonoBehaviour
    {
        [SerializeField] private float _followSpeed = 5.5f; // 추종 속도
        [SerializeField] private float _slotSpacing = 0.8f; // 슬롯 간격
        [SerializeField] private float _rowSpacing = 0.55f; // 열 간격
        [SerializeField] private float _minY = -2.3f; // 아래 추종 한계
        [SerializeField] private float _maxY = 2.1f; // 위 추종 한계
        private Transform _leader; // 추종 대상
        private PlayerSideViewController _leaderController; // 방향 참조
        private int _slotIndex; // 슬롯 번호
        private bool _isFollowing; // 추종 여부

        public bool IsFollowing => _isFollowing; // 추종 상태
        public int SlotIndex => _slotIndex; // 현재 슬롯

        public void BeginFollowing(Transform leader, PlayerSideViewController leaderController, int slotIndex)
        {
            _leader = leader; // 대상 설정
            _leaderController = leaderController; // 방향 설정
            _slotIndex = Mathf.Max(0, slotIndex); // 슬롯 설정
            _isFollowing = true; // 추종 시작
        }

        public void SetSlotIndex(int slotIndex)
        {
            _slotIndex = Mathf.Max(0, slotIndex); // 슬롯 갱신
        }

        public void StopFollowing()
        {
            _isFollowing = false; // 추종 종료
            _leader = null; // 대상 해제
            _leaderController = null; // 방향 해제
        }

        private void Update()
        {
            if (!_isFollowing || _leader == null || _leaderController == null) // 추종 가능 확인
            {
                return; // 추종 중단
            }

            float side = -_leaderController.FacingDirection; // 후방 방향
            int column = _slotIndex / 2; // 후방 열 계산
            int row = _slotIndex % 2; // 위아래 행 계산
            float horizontalDistance = (column + 1) * _slotSpacing; // 후방 거리 계산
            float verticalOffset = row == 0 ? -_rowSpacing * 0.5f : _rowSpacing * 0.5f; // 행 오프셋 계산
            Vector3 target = _leader.position + new Vector3(side * horizontalDistance, verticalOffset, 0f); // 평면 슬롯 위치
            target.y = Mathf.Clamp(target.y, _minY, _maxY); // 세로 위치 제한
            transform.position = Vector3.MoveTowards(transform.position, target, _followSpeed * Time.deltaTime); // 추종 이동
        }
    }
}
