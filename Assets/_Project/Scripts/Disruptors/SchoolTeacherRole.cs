using UnityEngine; // 유니티 기능 참조
using ProjectTheta.Player; // 플레이어 이동 참조
using ProjectTheta.Stage; // 스테이지 참조

namespace ProjectTheta.Disruptors // 방해 세력 공간
{ // 공간 시작
    [RequireComponent(typeof(DisruptorBase))] // 공통 몸체 요구
    [RequireComponent(typeof(WatcherRole))] // 감시 역할 요구
    public sealed class SchoolTeacherRole : MonoBehaviour // 교사 전용 행동
    { // 클래스 시작
        private DisruptorBase _body; // 공통 몸체
        private WatcherRole _watcher; // 감시 역할
        private Transform _player; // 플레이어 위치
        private PlayerSideViewController _movement; // 플레이어 이동
        private StageSessionController _stage; // 스테이지 상태
        private float _chaseRemaining; // 추적 남은 시간
        private float _graceRemaining; // 재포획 유예 시간
        private float _blackoutRemaining; // 암전 남은 시간
        private Vector2 _lastSeenPosition; // 마지막 발견 위치

        public void Configure( // 의존성 연결
            Transform player, // 플레이어 위치
            StageSessionController stage) // 스테이지 상태
        { // 연결 시작
            _body = GetComponent<DisruptorBase>(); // 몸체 조회
            _watcher = GetComponent<WatcherRole>(); // 감시자 조회
            _player = player; // 플레이어 저장
            _stage = stage; // 스테이지 저장
            _movement = player == null // 플레이어 확인
                ? null // 이동 없음
                : player.GetComponent<PlayerSideViewController>(); // 이동 조회

            if (_watcher != null) // 감시자 확인
            { // 구독 시작
                _watcher.Spotted += HandleSpotted; // 발견 사건 구독
            } // 구독 끝
        } // 연결 끝

        private void Update() // 매 프레임 처리
        { // 처리 시작
            float deltaTime = Time.deltaTime; // 프레임 시간

            _graceRemaining = Mathf.Max( // 유예 시간 감소
                0f, // 최소 유예
                _graceRemaining - deltaTime); // 감소 계산
            _blackoutRemaining = Mathf.Max( // 암전 시간 감소
                0f, // 최소 암전
                _blackoutRemaining - deltaTime); // 감소 계산

            if (_chaseRemaining <= 0f || // 추적 여부 확인
                _body == null || // 몸체 확인
                _body.Profile == null || // 설정 확인
                _player == null || // 플레이어 확인
                !_body.CanAct) // 행동 가능 확인
            { // 중단 시작
                return; // 처리 중단
            } // 중단 끝

            _chaseRemaining = Mathf.Max( // 추적 시간 감소
                0f, // 최소 시간
                _chaseRemaining - deltaTime); // 감소 계산

            if (_watcher != null && _watcher.PlayerInSight) // 현재 시야 확인
            { // 위치 갱신 시작
                _lastSeenPosition = _player.position; // 마지막 위치 갱신
            } // 위치 갱신 끝

            _body.MoveToward( // 마지막 위치 추적
                _lastSeenPosition, // 이동 목표
                _chaseRemaining); // 이동 지속 시간

            float distance = Vector2.Distance( // 플레이어 거리 계산
                transform.position, // 교사 위치
                _player.position); // 플레이어 위치

            if (_graceRemaining <= 0f && // 유예 종료 확인
                distance <= _body.Profile.CaptureRange) // 포획 거리 확인
            { // 포획 시작
                CapturePlayer(); // 플레이어 포획
            } // 포획 끝
        } // 처리 끝

        private void HandleSpotted( // 발견 사건 처리
            Vector2 playerPosition) // 발견 위치
        { // 처리 시작
            if (_graceRemaining > 0f || // 유예 확인
                _body == null || // 몸체 확인
                _body.Profile == null) // 설정 확인
            { // 중단 시작
                return; // 발견 무시
            } // 중단 끝

            _lastSeenPosition = playerPosition; // 발견 위치 저장
            _chaseRemaining = _body.Profile.ChaseSeconds; // 추적 시간 시작
        } // 처리 끝

        private void CapturePlayer() // 포획 처리
        { // 처리 시작
            DisruptorProfile profile = _body.Profile; // 교사 설정 저장

            _stage?.ApplyTimePenalty( // 시간 벌점 적용
                profile.TimePenaltySeconds); // 벌점 시간 전달

            _movement?.ApplyStagger( // 암전 중 이동 차단
                profile.BlackoutSeconds); // 차단 시간 전달
            _blackoutRemaining = profile.BlackoutSeconds; // 암전 연출 시작

            float away = _player.position.x >= transform.position.x // 반대 방향 계산
                ? 1f // 오른쪽 이동
                : -1f; // 왼쪽 이동
            Vector3 position = _player.position; // 현재 위치 복사
            position.x = Mathf.Clamp( // 안전 위치 제한
                position.x + away * 3f, // 가까운 복도 위치
                FloorSpace.WalkMinX + 1f, // 왼쪽 한계
                FloorSpace.WalkMaxX - 1f); // 오른쪽 한계
            _player.position = position; // 플레이어 재배치

            _watcher?.ClearSuspicion(); // 발각 상태 해제
            _chaseRemaining = 0f; // 추적 종료
            _graceRemaining = profile.ReacquireGraceSeconds; // 재감지 유예 시작
        } // 처리 끝

        private void OnGUI() // 암전 화면 표시
        { // 표시 시작
            if (_blackoutRemaining <= 0f) // 암전 상태 확인
            { // 중단 시작
                return; // 표시 중단
            } // 중단 끝

            Color previous = GUI.color; // 기존 화면 색 저장
            GUI.color = Color.black; // 검은색 지정
            GUI.DrawTexture( // 화면 덮기
                new Rect(0f, 0f, Screen.width, Screen.height), // 전체 화면 영역
                Texture2D.whiteTexture); // 기본 흰색 질감
            GUI.color = previous; // 기존 화면 색 복원
        } // 표시 끝

        private void OnDisable() // 비활성 처리
        { // 처리 시작
            if (_watcher != null) // 감시자 확인
            { // 구독 해제 시작
                _watcher.Spotted -= HandleSpotted; // 발견 사건 해제
            } // 구독 해제 끝
        } // 처리 끝
    } // 클래스 끝
} // 공간 끝
