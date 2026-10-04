using System.Collections.Generic; // 목록 기능 참조
using UnityEngine; // 유니티 기능 참조
using ProjectTheta.Stage; // 스테이지 참조
using ProjectTheta.Stage.Locations; // 지역 위험 참조

namespace ProjectTheta.Disruptors // 방해 세력 공간
{ // 공간 시작
    public sealed class SchoolDisciplineHeadTarget : MonoBehaviour // 지도부장 최면 대상
    { // 클래스 시작
        private static readonly List<SchoolDisciplineHeadTarget> Active = // 활성 대상 목록
            new List<SchoolDisciplineHeadTarget>(); // 목록 생성
        private const float MaximumHypnosis = 100f; // 최대 최면 진행
        private const float BaseBuildPerSecond = 32f; // 일반 최면 속도

        private StageSessionController _stage; // 스테이지 상태
        private DisruptorProfile _profile; // 지도부장 설정
        private float _currentHypnosis; // 현재 최면 진행
        private int _lastFocusedFrame = -2; // 마지막 최면 프레임

        public float HypnosisNormalized => // 최면 비율
            Mathf.Clamp01( // 범위 제한
                _currentHypnosis / MaximumHypnosis); // 비율 계산

        public bool IsBeingFocused => // 현재 최면 대상 여부
            _lastFocusedFrame >= Time.frameCount - 1; // 갱신 순서 보정

        public void Configure( // 의존성 연결
            StageSessionController stage, // 스테이지 상태
            DisruptorProfile profile) // 지도부장 설정
        { // 연결 시작
            _stage = stage; // 스테이지 저장
            _profile = profile; // 설정 저장
        } // 연결 끝

        public static bool TryFocusNearest( // 가장 가까운 지도부장 최면
            Vector2 source, // 시전자 위치
            float range, // 최면 사거리
            float deltaTime, // 프레임 시간
            float speedMultiplier) // 집중력 배율
        { // 처리 시작
            SchoolDisciplineHeadTarget target = FindNearest( // 최근접 대상 조회
                source, // 시전자 위치
                range); // 최면 사거리

            if (target == null) // 대상 확인
            { // 대상 없음 시작
                return false; // 미처리 반환
            } // 대상 없음 끝

            target.ApplyFocus( // 최면 진행 적용
                deltaTime, // 프레임 시간
                speedMultiplier); // 집중력 배율

            return true; // 처리 완료 반환
        } // 처리 끝

        private static SchoolDisciplineHeadTarget FindNearest( // 최근접 대상 조회
            Vector2 source, // 시전자 위치
            float range) // 최면 사거리
        { // 조회 시작
            SchoolDisciplineHeadTarget best = null; // 최근접 대상
            float bestDistance = float.MaxValue; // 최근접 거리
            float rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range); // 사거리 제곱

            for (int i = 0; i < Active.Count; i++) // 활성 대상 순회
            { // 순회 시작
                SchoolDisciplineHeadTarget candidate = Active[i]; // 후보 조회

                if (candidate == null || !candidate.isActiveAndEnabled) // 후보 상태 확인
                { // 후보 제외 시작
                    continue; // 다음 후보 이동
                } // 후보 제외 끝

                Vector2 delta = (Vector2)candidate.transform.position - source; // 거리 벡터
                float distance = delta.sqrMagnitude; // 거리 제곱

                if (distance > rangeSquared || distance >= bestDistance) // 범위와 우선순위 확인
                { // 후보 제외 시작
                    continue; // 다음 후보 이동
                } // 후보 제외 끝

                best = candidate; // 최근접 대상 갱신
                bestDistance = distance; // 최근접 거리 갱신
            } // 순회 끝

            return best; // 최근접 대상 반환
        } // 조회 끝

        private void ApplyFocus( // 최면 진행 적용
            float deltaTime, // 프레임 시간
            float speedMultiplier) // 집중력 배율
        { // 적용 시작
            if (_profile == null || deltaTime <= 0f) // 설정과 시간 확인
            { // 중단 시작
                return; // 적용 중단
            } // 중단 끝

            _lastFocusedFrame = Time.frameCount; // 최면 프레임 기록

            _currentHypnosis += // 진행 증가
                BaseBuildPerSecond * // 일반 최면 속도
                Mathf.Max(0f, _profile.SelfHypnosisMultiplier) * // 지도부장 저항 배율
                Mathf.Max(0f, speedMultiplier) * // 집중력 배율
                RegionRiskLogic.GetHypnosisSpeedMultiplier( // 지역 저항 배율
                    RegionRiskState.CurrentMultiplier) * // 현재 위험 배율
                deltaTime; // 프레임 시간

            if (_currentHypnosis < MaximumHypnosis) // 완료 여부 확인
            { // 미완료 시작
                return; // 진행 유지
            } // 미완료 끝

            _stage?.AddEssence( // 정기 보상 지급
                _profile.EssenceReward); // 보상량 전달
            gameObject.SetActive(false); // 지도부장 퇴장
        } // 적용 끝

        private void OnEnable() // 활성 처리
        { // 처리 시작
            if (!Active.Contains(this)) // 중복 확인
            { // 등록 시작
                Active.Add(this); // 대상 등록
            } // 등록 끝
        } // 처리 끝

        private void OnDisable() // 비활성 처리
        { // 처리 시작
            Active.Remove(this); // 대상 해제
        } // 처리 끝

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 플레이 진입 초기화
        private static void ResetOnPlayModeEnter() // 정적 목록 초기화
        { // 초기화 시작
            Active.Clear(); // 목록 비우기
        } // 초기화 끝
    } // 클래스 끝
} // 공간 끝
