using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Story;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 이야기를 틀고 본 것으로 저장하는 공용 순서다 (36일차).
    /// 지도 · 허브 도착, 허브 일기장 · 장소 입장이 같이 쓴다.
    /// </summary>
    public static class StoryPlayback
    {
        /// <summary>대사 창 정렬 순서다. 업적 알림 · 창들보다 위다.</summary>
        public const int SortOrder = 120;

        /// <summary>조건을 채웠는데 아직 안 본 장면을 모두 튼다. 없으면 바로 끝낸다.</summary>
        public static bool PlayPending(
            DialogueOverlay overlay,
            Action onFinished)
        {
            return PlayPending( // 안전 구역 재생 호출
                overlay, // 대사창
                StoryEventType.SafeArea, // 안전 구역 사건
                onFinished); // 완료 알림
        }

        public static bool PlayPending( // 사건별 대기 장면 재생
            DialogueOverlay overlay, // 대사창
            StoryEventType eventType, // 발생 사건
            Action onFinished) // 완료 알림
        { // 재생 요청 시작
            GameSession session = GameSession.Instance; // 게임 세션 조회

            if (session == null || // 게임 세션 확인
                session.Save == null || // 저장 자료 확인
                overlay == null) // 대사창 확인
            { // 재생 불가 시작
                onFinished?.Invoke(); // 완료 알림 호출

                return false; // 재생 실패 반환
            } // 재생 불가 끝

            List<StoryScene> resultScenes = new List<StoryScene>(); // 결과 장면 목록

            AddResultScenes( // 직전 결과 장면 우선 추가
                session, // 게임 세션
                resultScenes); // 결과 장면 목록

            if (resultScenes.Count > 0) // 결과 장면 존재 확인
            { // 결과 우선 재생 시작
                Play( // 결과 장면 재생
                    overlay, // 대사창
                    resultScenes, // 결과 장면 목록
                    false, // 화면 흐름 유지
                    () => PlayEventScenes(overlay, eventType, onFinished)); // 완료 후 현재 화면 재조회

                return true; // 재생 시작 반환
            } // 결과 우선 재생 끝

            return PlayEventScenes( // 현재 화면 장면 재생
                overlay, // 대사창
                eventType, // 발생 사건
                onFinished); // 완료 알림
        } // 재생 요청 끝

        private static bool PlayEventScenes( // 현재 화면 장면 재생
            DialogueOverlay overlay, // 대사창
            StoryEventType eventType, // 발생 사건
            Action onFinished) // 완료 알림
        { // 재생 시작
            GameSession session = GameSession.Instance; // 게임 세션 조회

            if (session == null || session.Save == null || overlay == null) // 재생 환경 확인
            { // 재생 불가 시작
                onFinished?.Invoke(); // 완료 알림 호출

                return false; // 재생 실패 반환
            } // 재생 불가 끝

            List<StoryScene> eventScenes = StoryQueueLogic.GetAvailable( // 현재 화면 장면 조회
                session.Save, // 현재 저장
                StoryCatalog.All, // 전체 장면
                new StoryContext( // 현재 사건 생성
                    eventType, // 발생 사건
                    session.HasLastResult && session.LastResult.HasLocation // 직전 장소 확인
                        ? (Stage.Locations.LocationId)session.LastResult.LocationId // 직전 장소 변환
                        : Stage.Locations.LocationId.TrainingCenter)); // 기본 장소

            if (eventScenes.Count == 0) // 재생 장면 확인
            { // 빈 장면 시작
                onFinished?.Invoke(); // 완료 알림 호출

                return false; // 재생 없음 반환
            } // 빈 장면 끝

            Play( // 현재 화면 장면 재생
                overlay, // 대사창
                eventScenes, // 현재 화면 장면 목록
                false, // 화면 흐름 유지
                onFinished); // 완료 알림

            return true; // 재생 시작 반환
        } // 재생 끝

        /// <summary>장면들을 틀고, 시작할 때마다 본 것으로 남기고, 끝나면 저장한다.</summary>
        public static void Play(
            DialogueOverlay overlay,
            IList<StoryScene> scenes,
            bool pauseGame,
            Action onFinished)
        {
            PlayInternal( // 자동 재생 호출
                overlay, // 대사창
                scenes, // 장면 목록
                pauseGame, // 일시정지 여부
                false, // 진행 저장 허용
                onFinished); // 완료 알림
        }

        public static void PlayReplay( // 일기장 다시 보기
            DialogueOverlay overlay, // 대사창
            IList<StoryScene> scenes, // 장면 목록
            bool pauseGame, // 일시정지 여부
            Action onFinished) // 완료 알림
        { // 재생 시작
            PlayInternal( // 다시 보기 호출
                overlay, // 대사창
                scenes, // 장면 목록
                pauseGame, // 일시정지 여부
                true, // 저장 변경 차단
                onFinished); // 완료 알림
        } // 재생 끝

        private static void PlayInternal( // 공용 이야기 재생
            DialogueOverlay overlay, // 대사창
            IList<StoryScene> scenes, // 장면 목록
            bool pauseGame, // 일시정지 여부
            bool replay, // 다시 보기 여부
            Action onFinished) // 완료 알림
        {
            bool changed = false;

            overlay.Play(
                scenes,
                pauseGame,
                scene =>
                {
                    GameSession session = GameSession.Instance;

                    if (session != null &&
                        StoryProgressLogic.Begin( // 장면 시작 저장
                            session.Save, // 현재 저장
                            scene.Id, // 장면 ID
                            replay)) // 다시 보기 여부
                    {
                        changed = !replay; // 저장 변경 기록

                        if (!replay) // 자동 재생 확인
                        { // 즉시 저장 시작
                            session.WriteSave(); // 진행 장면 저장
                        } // 즉시 저장 끝
                    }
                },
                scene => // 장면 완료 처리
                { // 완료 시작
                    GameSession session = GameSession.Instance; // 게임 세션 조회

                    if (session != null && StoryProgressLogic.Complete(session.Save, scene.Id, replay)) // 완료 저장
                    { // 변경 기록 시작
                        changed = changed || !replay; // 저장 변경 기록
                    } // 변경 기록 끝
                }, // 완료 끝
                (scene, choice) => // 선택 결과 처리
                { // 선택 시작
                    GameSession session = GameSession.Instance; // 게임 세션 조회

                    if (session != null && StoryProgressLogic.SetChoice(session.Save, scene.Id, choice.Id, replay)) // 선택 저장
                    { // 변경 기록 시작
                        changed = changed || !replay; // 저장 변경 기록
                    } // 변경 기록 끝
                }, // 선택 끝
                () =>
                {
                    if (changed)
                    {
                        GameSession.Instance?.WriteSave();
                    }

                    onFinished?.Invoke();
                });
        }

        private static void AddResultScenes( // 직전 결과 사건 추가
            GameSession session, // 게임 세션
            List<StoryScene> pending) // 대기 목록
        { // 추가 시작
            if (session == null || !session.HasLastResult || !session.LastResult.HasLocation) // 결과 존재 확인
            { // 중단 시작
                return; // 추가 중단
            } // 중단 끝

            Stage.Locations.LocationId location = // 직전 장소
                (Stage.Locations.LocationId)session.LastResult.LocationId; // 장소 번호 변환
            List<StoryContext> contexts = new List<StoryContext>(); // 결과 사건 목록

            if (session.LastResult.Cleared) // 클리어 여부 확인
            { // 클리어 사건 시작
                contexts.Add(new StoryContext(StoryEventType.LocationClear, location)); // 장소 클리어 추가
            } // 클리어 사건 끝

            if (session.LastResult.BossDefeated) // 보스 승리 확인
            { // 승리 사건 시작
                contexts.Add(new StoryContext(StoryEventType.BossVictory, location)); // 보스 승리 추가
            } // 승리 사건 끝
            else if (location == Stage.Locations.LocationId.RooftopClub && !session.LastResult.Cleared) // 보스 패배 확인
            { // 패배 사건 시작
                contexts.Add(new StoryContext(StoryEventType.BossDefeat, location)); // 보스 패배 추가
            } // 패배 사건 끝

            foreach (StoryContext context in contexts) // 결과 사건 순회
            { // 순회 시작
                List<StoryScene> resultScenes = StoryQueueLogic.GetAvailable( // 결과 장면 조회
                    session.Save, // 현재 저장
                    StoryCatalog.All, // 전체 장면
                    context); // 결과 사건

                foreach (StoryScene scene in resultScenes) // 결과 장면 순회
                { // 장면 순회 시작
                    if (!pending.Contains(scene)) // 중복 확인
                    { // 추가 시작
                        pending.Add(scene); // 결과 장면 추가
                    } // 추가 끝
                } // 장면 순회 끝
            } // 순회 끝
        } // 추가 끝
    }
}
