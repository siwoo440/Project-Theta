using System.Collections.Generic; // 대기열 기능 참조
using ProjectTheta.Core; // 게임 세션 참조
using ProjectTheta.Stage; // 스테이지 사건 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.Story; // 이야기 자료 참조
using ProjectTheta.UI.Framework; // UI 생성 도구 참조
using UnityEngine; // 유니티 기능 참조

namespace ProjectTheta.UI // UI 공간
{ // 공간 시작
    public sealed class StageStoryEventDirector : MonoBehaviour // 스테이지 이야기 사건 감독
    { // 클래스 시작
        private const int SortOrder = 276; // 대화창 정렬 순서
        private readonly Queue<StoryEventType> _pending = new Queue<StoryEventType>(); // 사건 대기열
        private StageSessionController _stage; // 스테이지 진행 자료
        private LocationId _location; // 현재 장소
        private DialogueOverlay _overlay; // 이야기 대화창
        private bool _playing; // 이야기 재생 여부

        public void Configure( // 감독 설정
            StageSessionController stage, // 스테이지 진행 자료
            LocationId location) // 현재 장소
        { // 설정 시작
            _stage = stage; // 스테이지 저장
            _location = location; // 장소 저장

            Canvas canvas = UiFactory.CreateCanvas( // 전용 캔버스 생성
                "StageStoryEventCanvas", // 캔버스 이름
                SortOrder, // 정렬 순서
                transform); // 부모 오브젝트

            _overlay = DialogueOverlay.Create( // 대화창 생성
                canvas.transform, // 캔버스 부모
                SortOrder); // 정렬 순서
        } // 설정 끝

        private void OnEnable() // 사건 구독 시작
        { // 구독 시작
            StageMoments.HypnosisSucceeded += OnHypnosisSucceeded; // 최면 성공 구독
            StageMoments.RecoveryConfirmed += OnRecoveryConfirmed; // 회수 확정 구독
            StageMoments.FollowerStolen += OnFollowerStolen; // 동행자 탈취 구독
            StageMoments.DuelWon += OnDuelWon; // 힘겨루기 승리 구독
        } // 구독 끝

        private void OnDisable() // 사건 구독 해제
        { // 해제 시작
            StageMoments.HypnosisSucceeded -= OnHypnosisSucceeded; // 최면 성공 해제
            StageMoments.RecoveryConfirmed -= OnRecoveryConfirmed; // 회수 확정 해제
            StageMoments.FollowerStolen -= OnFollowerStolen; // 동행자 탈취 해제
            StageMoments.DuelWon -= OnDuelWon; // 힘겨루기 승리 해제
        } // 해제 끝

        private void Update() // 대기 사건 처리
        { // 처리 시작
            if (_playing || // 현재 재생 확인
                DialogueOverlay.Busy || // 다른 대화창 확인
                _stage == null || // 스테이지 확인
                !_stage.IsRunning || // 진행 상태 확인
                _overlay == null) // 전용 대화창 확인
            { // 대기 시작
                return; // 다음 화면까지 대기
            } // 대기 끝

            PlayNext(); // 다음 사건 재생
        } // 처리 끝

        private void OnHypnosisSucceeded( // 최면 성공 처리
            Vector2 position, // 발생 위치
            bool wasReclaim) // 재탈환 여부
        { // 처리 시작
            Enqueue(StageStoryEventLogic.GetHypnosisEvent(wasReclaim)); // 최면 사건 추가
        } // 처리 끝

        private void OnRecoveryConfirmed( // 회수 확정 처리
            Vector2 position, // 발생 위치
            int count, // 회수 인원
            int essence) // 확정 정기
        { // 처리 시작
            Enqueue(StoryEventType.RecoveryConfirmed); // 회수 사건 추가
        } // 처리 끝

        private void OnFollowerStolen(Vector2 position) // 동행자 탈취 처리
        { // 처리 시작
            Enqueue(StoryEventType.FollowerStolen); // 탈취 사건 추가
        } // 처리 끝

        private void OnDuelWon(Vector2 position) // 힘겨루기 승리 처리
        { // 처리 시작
            Enqueue(StageStoryEventLogic.GetDuelEvent()); // 소유권 미변경 사건 처리
        } // 처리 끝

        private void Enqueue(StoryEventType eventType) // 사건 대기열 추가
        { // 추가 시작
            if (eventType != StoryEventType.None && // 유효 사건 확인
                !_pending.Contains(eventType)) // 중복 사건 확인
            { // 추가 허용 시작
                _pending.Enqueue(eventType); // 사건 추가
            } // 추가 허용 끝
        } // 추가 끝

        private void PlayNext() // 다음 이야기 재생
        { // 재생 시작
            GameSession session = GameSession.Instance; // 게임 세션 조회

            while (_pending.Count > 0) // 대기 사건 순회
            { // 순회 시작
                StoryEventType eventType = _pending.Dequeue(); // 다음 사건 조회
                List<StoryScene> scenes = StoryQueueLogic.GetAvailable( // 재생 장면 조회
                    session == null ? null : session.Save, // 현재 저장
                    StoryCatalog.All, // 전체 장면
                    new StoryContext(eventType, _location)); // 현재 사건 자료

                if (scenes.Count == 0) // 재생 장면 확인
                { // 다음 사건 시작
                    continue; // 다음 사건 이동
                } // 다음 사건 끝

                _playing = true; // 재생 상태 설정
                StoryPlayback.Play( // 이야기 재생
                    _overlay, // 전용 대화창
                    scenes, // 발생 장면 목록
                    true, // 게임 일시정지
                    OnPlaybackFinished); // 완료 처리

                return; // 중복 재생 방지
            } // 순회 끝
        } // 재생 끝

        private void OnPlaybackFinished() // 이야기 완료 처리
        { // 처리 시작
            _playing = false; // 재생 상태 해제
        } // 처리 끝
    } // 클래스 끝
} // 공간 끝
