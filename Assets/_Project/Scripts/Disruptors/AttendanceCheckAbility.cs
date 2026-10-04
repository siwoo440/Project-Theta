using UnityEngine; // 유니티 기능 참조
using ProjectTheta.Hypnosis; // 최면 시전자 참조
using ProjectTheta.Player; // 플레이어 이동 참조
using ProjectTheta.Stage; // 층 계산 참조

namespace ProjectTheta.Disruptors // 방해 세력 공간
{ // 공간 시작
    /// <summary>생활지도부장이 최면을 발견해 돌진하고 시전을 끊는 능력이다 (40일차).</summary>
    public sealed class AttendanceCheckAbility : SpecialAbility // 생활지도 개입 능력
    { // 클래스 시작
        private Transform _player; // 플레이어 위치
        private HypnosisCaster _caster; // 최면 시전자
        private PlayerSideViewController _movement; // 플레이어 이동
        private SchoolDisciplineHeadTarget _selfTarget; // 본인 최면 대상
        private float _detectionProgress; // 감지 진행 시간

        public override string DisplayName => // 능력 표시 이름
            "최면 단속"; // 사용자 표시

        public void Configure( // 의존성 연결
            Transform player) // 플레이어 위치
        { // 연결 시작
            _player = player; // 플레이어 저장
            _caster = player == null // 플레이어 확인
                ? null // 시전자 없음
                : player.GetComponent<HypnosisCaster>(); // 시전자 조회
            _movement = player == null // 플레이어 확인
                ? null // 이동 없음
                : player.GetComponent<PlayerSideViewController>(); // 이동 조회
            _selfTarget = GetComponent<SchoolDisciplineHeadTarget>(); // 본인 대상 조회
        } // 연결 끝

        protected override bool WantsToStart() // 발동 조건 확인
        { // 확인 시작
            if (!CanSeeHypnosis()) // 최면 목격 확인
            { // 미목격 시작
                _detectionProgress = 0f; // 감지 초기화
                return false; // 발동 보류
            } // 미목격 끝

            _detectionProgress += Time.deltaTime; // 감지 시간 증가

            return _detectionProgress >= // 확정 시간 비교
                   Mathf.Max( // 최소 시간 적용
                       0.01f, // 최소 확정 시간
                       Body.Profile.DetectionSeconds); // 설정 확정 시간
        } // 확인 끝

        protected override void OnTelegraph() // 예고 시작 처리
        { // 처리 시작
            _detectionProgress = 0f; // 감지 초기화

            if (_player == null || Body == null) // 대상 확인
            { // 중단 시작
                return; // 처리 중단
            } // 중단 끝

            Body.FaceToward( // 플레이어 방향 전환
                _player.position.x); // 플레이어 가로 위치
            Body.MoveToward( // 플레이어에게 돌진
                _player.position, // 돌진 목표
                TelegraphSeconds); // 돌진 시간
        } // 처리 끝

        protected override void Fire() // 능력 발동
        { // 발동 시작
            if (_player == null || // 플레이어 확인
                Body == null || // 몸체 확인
                Body.Profile == null) // 설정 확인
            { // 중단 시작
                return; // 발동 중단
            } // 중단 끝

            DisruptorProfile profile = Body.Profile; // 능력 설정 저장

            _caster?.Interrupt( // 최면 시전 차단
                profile.HypnosisBlockSeconds); // 차단 시간 전달
            _movement?.ApplyStagger( // 플레이어 경직
                profile.StunSeconds); // 경직 시간 전달

            float direction = _player.position.x >= transform.position.x // 밀치기 방향 계산
                ? 1f // 오른쪽 방향
                : -1f; // 왼쪽 방향
            Vector3 position = _player.position; // 현재 위치 복사
            position.x = Mathf.Clamp( // 복도 범위 제한
                position.x + direction * profile.PushDistance, // 밀치기 적용
                FloorSpace.WalkMinX + 1f, // 왼쪽 한계
                FloorSpace.WalkMaxX - 1f); // 오른쪽 한계
            _player.position = position; // 밀친 위치 적용

            StageMoments.RaiseAbilityFired( // 능력 발동 알림
                _player.position, // 효과 위치
                DisplayName); // 능력 이름
        } // 발동 끝

        private bool CanSeeHypnosis() // 최면 목격 판정
        { // 판정 시작
            if (_player == null || // 플레이어 확인
                _caster == null || // 시전자 확인
                !_caster.IsHolding || // 최면 입력 확인
                (_caster.CurrentTarget == null && // 일반 대상 확인
                 (_selfTarget == null || !_selfTarget.IsBeingFocused)) || // 본인 대상 확인
                Body == null || // 몸체 확인
                Body.Profile == null) // 설정 확인
            { // 실패 시작
                return false; // 미목격 반환
            } // 실패 끝

            Vector2 self = transform.position; // 지도부장 위치
            Vector2 player = _player.position; // 플레이어 위치

            if (FloorSpace.FloorAt(player.y) != Body.Floor) // 같은 층 확인
            { // 다른 층 시작
                return false; // 미목격 반환
            } // 다른 층 끝

            return DetectionLogic.IsInSight( // 시야 판정 호출
                self.x, // 시야 원점 가로
                self.y, // 시야 원점 세로
                Body.Facing, // 바라보는 방향
                player.x, // 대상 가로
                player.y, // 대상 세로
                Body.Profile.SightHalfAngle, // 시야 반각
                Body.Profile.SightRange); // 시야 거리
        } // 판정 끝
    } // 클래스 끝
} // 공간 끝
