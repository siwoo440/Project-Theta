using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Player;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class HypnosisCaster : MonoBehaviour
    {
        [SerializeField] private float _scanRange = 4.5f; // 탐색 거리
        [SerializeField] private float _verticalTolerance = 2.2f; // 깊이 허용
        private readonly List<HypnosisTarget> _followers = new List<HypnosisTarget>(); // 동행 목록
        private PlayerSideViewController _playerController; // 플레이어 제어
        private HypnosisTarget _currentTarget; // 현재 대상
        private LineRenderer _lineRenderer; // 시선 효과

        public PlayerSideViewController PlayerController => _playerController; // 플레이어 참조
        public HypnosisTarget CurrentTarget => _currentTarget; // 대상 참조
        public int FollowersCount => _followers.Count; // 동행 수

        private void Awake()
        {
            _playerController = GetComponent<PlayerSideViewController>(); // 플레이어 참조
            _lineRenderer = GetComponent<LineRenderer>(); // 라인 참조
        }

        private void Update()
        {
            bool held = ReadHypnosisHeld(); // 최면 입력
            if (!held) // 입력 해제 확인
            {
                ClearTarget(); // 대상 해제
                return; // 최면 중단
            }

            HypnosisTarget candidate = FindBestTarget(); // 대상 탐색
            if (candidate != _currentTarget) // 대상 변경 확인
            {
                ClearTarget(); // 이전 대상 해제
                _currentTarget = candidate; // 새 대상 설정
            }

            if (_currentTarget == null) // 대상 확인
            {
                UpdateLine(false); // 라인 숨김
                return; // 최면 중단
            }

            bool completed = _currentTarget.ApplyFocus(Time.deltaTime); // 최면 진행
            UpdateLine(true); // 라인 표시
            if (completed) // 최면 완료 확인
            {
                AddFollower(_currentTarget); // 동행 등록
                _currentTarget = null; // 대상 초기화
                UpdateLine(false); // 라인 숨김
            }
        }

        public int IndexOf(HypnosisTarget target)
        {
            int index = _followers.IndexOf(target); // 위치 탐색
            return Mathf.Max(0, index); // 안전 인덱스 반환
        }

        public void RemoveFollower(HypnosisTarget target)
        {
            _followers.Remove(target); // 동행 제거
            ReindexFollowers(); // 슬롯 재정렬
        }

        private void AddFollower(HypnosisTarget target)
        {
            if (target == null || _followers.Contains(target)) // 중복 확인
            {
                return; // 등록 중단
            }

            _followers.Add(target); // 동행 추가
            target.BeginFollowing(this, transform, _playerController, _followers.Count - 1); // 추종 시작
        }

        private void ReindexFollowers()
        {
            for (int i = 0; i < _followers.Count; i++) // 동행 순회
            {
                if (_followers[i] != null) // 대상 확인
                {
                    _followers[i].SetFollowerSlot(i); // 슬롯 갱신
                }
            }
        }

        private HypnosisTarget FindBestTarget()
        {
            HypnosisTarget[] targets = FindObjectsByType<HypnosisTarget>(FindObjectsSortMode.None); // 전체 대상 탐색
            HypnosisTarget best = null; // 최적 대상
            float bestDistance = float.MaxValue; // 최적 거리
            for (int i = 0; i < targets.Length; i++) // 대상 순회
            {
                HypnosisTarget target = targets[i]; // 현재 대상
                if (target == null || target.IsHypnotized) // 유효성 확인
                {
                    continue; // 대상 제외
                }

                Vector3 offset = target.transform.position - transform.position; // 상대 위치
                if (Mathf.Abs(offset.y) > _verticalTolerance) // 깊이 확인
                {
                    continue; // 대상 제외
                }

                if (Mathf.Abs(offset.x) > 0.05f && Mathf.Sign(offset.x) != _playerController.FacingDirection) // 좌우 방향 확인
                {
                    continue; // 대상 제외
                }

                float distance = new Vector2(offset.x, offset.y).magnitude; // 평면 거리
                if (distance > _scanRange || distance >= bestDistance) // 거리 확인
                {
                    continue; // 대상 제외
                }

                best = target; // 대상 갱신
                bestDistance = distance; // 거리 갱신
            }

            return best; // 최적 대상 반환
        }

        private void ClearTarget()
        {
            if (_currentTarget != null) // 기존 대상 확인
            {
                _currentTarget.EndFocus(); // 시선 종료
            }

            _currentTarget = null; // 대상 초기화
            UpdateLine(false); // 라인 숨김
        }

        private void UpdateLine(bool visible)
        {
            if (_lineRenderer == null) // 라인 확인
            {
                return; // 표시 중단
            }

            _lineRenderer.enabled = visible && _currentTarget != null; // 표시 상태
            if (!_lineRenderer.enabled) // 표시 확인
            {
                return; // 좌표 갱신 중단
            }

            _lineRenderer.SetPosition(0, transform.position + Vector3.up * 0.4f); // 시작점 설정
            _lineRenderer.SetPosition(1, _currentTarget.transform.position + Vector3.up * 0.4f); // 끝점 설정
        }

        private bool ReadHypnosisHeld()
        {
#if ENABLE_INPUT_SYSTEM
            bool keyboard = Keyboard.current != null && Keyboard.current.eKey.isPressed; // 키보드 최면
            bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed; // 마우스 최면
            return keyboard || mouse; // 최면 입력 반환
#else
            return Input.GetKey(KeyCode.E) || Input.GetMouseButton(0); // 구 입력 반환
#endif
        }
    }
}
