using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Save; // 저장 자료 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.Story; // 이야기 자료 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day42Tests // 42일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void Normalize_OldSeenStoriesBecomeCompleted() // 구버전 이야기 완료 변환 검증
        { // 테스트 시작
            SaveData save = new SaveData // 구버전 저장 생성
            { // 자료 시작
                Version = 2, // 구버전 지정
                SeenStories = new[] { "enter_beach" }, // 기존 본 장면
                CompletedStories = null // 신규 완료 자료 누락
            }; // 자료 끝

            SaveData normalized = SaveDataLogic.Normalize(save); // 저장 보정 실행

            Assert.AreEqual(3, normalized.Version); // 저장 버전 확인
            CollectionAssert.Contains(normalized.CompletedStories, "enter_beach"); // 완료 이관 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Progress_StartAndCompleteUseSeparateStates() // 시작과 완료 상태 분리 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsTrue(StoryProgressLogic.Begin(save, "story_a", false)); // 자동 재생 시작
            Assert.IsTrue(StoryProgressLogic.IsSeen(save, "story_a")); // 본 장면 확인
            Assert.IsFalse(StoryProgressLogic.IsCompleted(save, "story_a")); // 미완료 확인
            Assert.AreEqual("story_a", save.ActiveStoryId); // 진행 장면 확인

            Assert.IsTrue(StoryProgressLogic.Complete(save, "story_a", false)); // 장면 완료
            Assert.IsTrue(StoryProgressLogic.IsCompleted(save, "story_a")); // 완료 확인
            Assert.AreEqual(string.Empty, save.ActiveStoryId); // 진행 장면 해제 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Choice_ReplacesPreviousSelectionAndClonesSafely() // 선택 결과 교체와 복제 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsTrue(StoryProgressLogic.SetChoice(save, "story_a", "accept")); // 첫 선택 저장
            Assert.IsTrue(StoryProgressLogic.SetChoice(save, "story_a", "refuse")); // 선택 교체
            Assert.AreEqual("refuse", StoryProgressLogic.GetChoice(save, "story_a")); // 최신 선택 확인

            SaveData copy = save.Clone(); // 저장 복제
            StoryProgressLogic.SetChoice(copy, "story_a", "accept"); // 복제 선택 변경

            Assert.AreEqual("refuse", StoryProgressLogic.GetChoice(save, "story_a")); // 원본 분리 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Conditions_RequireCompletedStoryAndSavedChoice() // 선행 장면과 선택 조건 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryCondition[] conditions = new[] // 조건 목록 생성
            { // 목록 시작
                StoryCondition.StoryCompleted("intro"), // 선행 완료 조건
                StoryCondition.ChoiceEquals("intro", "trust") // 선행 선택 조건
            }; // 목록 끝

            Assert.IsFalse(StoryConditionLogic.AreMet(save, conditions)); // 초기 조건 실패 확인

            StoryProgressLogic.Complete(save, "intro", false); // 선행 장면 완료
            StoryProgressLogic.SetChoice(save, "intro", "trust"); // 선행 선택 저장

            Assert.IsTrue(StoryConditionLogic.AreMet(save, conditions)); // 전체 조건 충족 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Queue_UsesEventConditionsAndCatalogOrder() // 사건별 대기열 순서 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryScene first = CreateScene("first", StoryEventType.HubReturn); // 첫 장면 생성
            StoryScene wrongEvent = CreateScene("wrong", StoryEventType.BossVictory); // 다른 사건 장면 생성
            StoryScene second = CreateScene("second", StoryEventType.HubReturn); // 둘째 장면 생성

            System.Collections.Generic.List<StoryScene> pending = StoryQueueLogic.GetAvailable( // 대기열 조회
                save, // 현재 저장
                new[] { first, wrongEvent, second }, // 장면 표
                new StoryContext(StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 사건

            CollectionAssert.AreEqual(new[] { "first", "second" }, pending.ConvertAll(scene => scene.Id)); // 순서 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Queue_ExcludesCompletedAndResumesActiveStoryFirst() // 완료 제외와 진행 장면 우선 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryScene first = CreateScene("first", StoryEventType.HubReturn); // 첫 장면 생성
            StoryScene active = CreateScene("active", StoryEventType.HubReturn); // 진행 장면 생성
            StoryScene last = CreateScene("last", StoryEventType.HubReturn); // 마지막 장면 생성
            StoryProgressLogic.Complete(save, "first", false); // 첫 장면 완료
            StoryProgressLogic.Begin(save, "active", false); // 진행 장면 시작

            System.Collections.Generic.List<StoryScene> pending = StoryQueueLogic.GetAvailable( // 대기열 조회
                save, // 현재 저장
                new[] { first, active, last }, // 장면 표
                new StoryContext(StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 사건

            CollectionAssert.AreEqual(new[] { "active", "last" }, pending.ConvertAll(scene => scene.Id)); // 대기열 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Queue_SafeAreaSceneDoesNotLeakIntoLocationEvent() // 안전 구역 장면 유입 방지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryScene safeArea = CreateScene("safe", StoryEventType.SafeArea); // 안전 구역 장면 생성
            StoryScene location = CreateScene("location", StoryEventType.LocationEnter); // 장소 입장 장면 생성

            System.Collections.Generic.List<StoryScene> pending = StoryQueueLogic.GetAvailable( // 입장 대기열 조회
                save, // 현재 저장
                new[] { safeArea, location }, // 장면 표
                new StoryContext(StoryEventType.LocationEnter, LocationId.TrainingCenter)); // 장소 입장 사건

            CollectionAssert.AreEqual(new[] { "location" }, pending.ConvertAll(scene => scene.Id)); // 입장 장면만 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Queue_ActiveSceneWaitsForItsOwnEventContext() // 진행 장면 사건 유지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryScene active = CreateScene("active", StoryEventType.HubReturn); // 허브 진행 장면 생성
            StoryScene location = CreateScene("location", StoryEventType.LocationEnter); // 장소 입장 장면 생성
            StoryProgressLogic.Begin(save, active.Id, false); // 허브 장면 진행 저장

            System.Collections.Generic.List<StoryScene> pending = StoryQueueLogic.GetAvailable( // 입장 대기열 조회
                save, // 현재 저장
                new[] { active, location }, // 장면 표
                new StoryContext(StoryEventType.LocationEnter, LocationId.TrainingCenter)); // 장소 입장 사건

            CollectionAssert.AreEqual(new[] { "location" }, pending.ConvertAll(scene => scene.Id)); // 입장 장면만 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Replay_DoesNotChangeProgressOrChoice() // 다시 보기 무변경 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsTrue(StoryProgressLogic.Begin(save, "story_a", true)); // 다시 보기 시작
            Assert.IsTrue(StoryProgressLogic.SetChoice(save, "story_a", "accept", true)); // 다시 보기 선택 처리
            Assert.IsTrue(StoryProgressLogic.Complete(save, "story_a", true)); // 다시 보기 완료

            Assert.IsFalse(StoryProgressLogic.IsSeen(save, "story_a")); // 본 장면 미변경 확인
            Assert.IsFalse(StoryProgressLogic.IsCompleted(save, "story_a")); // 완료 미변경 확인
            Assert.AreEqual(string.Empty, StoryProgressLogic.GetChoice(save, "story_a")); // 선택 미변경 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void ReplayList_ExcludesScenesThatDisallowReplay() // 다시 보기 금지 장면 제외 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryScene allowed = CreateScene("allowed", StoryEventType.HubReturn); // 다시 보기 허용 장면
            StoryScene blocked = new StoryScene( // 다시 보기 금지 장면
                "blocked", // 장면 ID
                "blocked", // 장면 제목
                StoryEventType.HubReturn, // 허브 사건
                LocationId.TrainingCenter, // 기본 장소
                true, // 자동 재생 허용
                false, // 다시 보기 금지
                new StoryCondition[0], // 조건 없음
                new StoryChoice[0], // 선택지 없음
                new[] { new StoryLine("나", "테스트") }); // 대사 한 줄
            StoryProgressLogic.Complete(save, allowed.Id, false); // 허용 장면 완료
            StoryProgressLogic.Complete(save, blocked.Id, false); // 금지 장면 완료

            System.Collections.Generic.List<StoryScene> replayable = StoryQueueLogic.GetReplayable( // 다시 보기 목록 조회
                save, // 현재 저장
                new[] { allowed, blocked }); // 장면 표

            CollectionAssert.AreEqual(new[] { "allowed" }, replayable.ConvertAll(scene => scene.Id)); // 허용 장면만 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Skip_StopsAtFirstSceneThatRequiresChoice() // 건너뛰기 선택지 정지 검증
        { // 테스트 시작
            StoryScene first = CreateScene("first", StoryEventType.HubReturn); // 일반 장면 생성
            StoryScene choice = new StoryScene( // 선택 장면 생성
                "choice", // 장면 ID
                "choice", // 장면 제목
                StoryEventType.HubReturn, // 허브 사건
                LocationId.TrainingCenter, // 기본 장소
                true, // 자동 재생 허용
                true, // 다시 보기 허용
                new StoryCondition[0], // 조건 없음
                new[] { new StoryChoice("accept", "수락") }, // 선택지 한 개
                new[] { new StoryLine("나", "테스트") }); // 대사 한 줄
            StoryScene last = CreateScene("last", StoryEventType.HubReturn); // 마지막 장면 생성

            Assert.AreEqual(1, StoryPlaybackLogic.GetSkipStopIndex(new[] { first, choice, last }, 0)); // 선택 장면 위치 확인
            Assert.AreEqual(1, StoryPlaybackLogic.GetSkipStopIndex(new[] { first, choice, last }, 1)); // 현재 선택 장면 확인
            Assert.AreEqual(-1, StoryPlaybackLogic.GetSkipStopIndex(new[] { first, choice, last }, 2)); // 남은 선택 없음 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void InvalidChoice_IsRejected() // 잘못된 선택값 거부 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsFalse(StoryProgressLogic.SetChoice(save, string.Empty, "accept")); // 빈 장면 거부 확인
            Assert.IsFalse(StoryProgressLogic.SetChoice(save, "story_a", string.Empty)); // 빈 선택 거부 확인
            Assert.AreEqual(0, save.StoryChoices.Length); // 저장 없음 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Validation_FindsMissingAndCircularDependencies() // 누락과 순환 선행 조건 검증
        { // 테스트 시작
            StoryScene first = CreateScene( // 첫 장면 생성
                "first", // 첫 장면 ID
                StoryEventType.HubReturn, // 허브 사건
                StoryCondition.StoryCompleted("second")); // 둘째 선행 조건
            StoryScene second = CreateScene( // 둘째 장면 생성
                "second", // 둘째 장면 ID
                StoryEventType.HubReturn, // 허브 사건
                StoryCondition.StoryCompleted("first")); // 첫째 선행 조건
            StoryScene missing = CreateScene( // 누락 장면 생성
                "missing", // 누락 장면 ID
                StoryEventType.HubReturn, // 허브 사건
                StoryCondition.StoryCompleted("unknown")); // 없는 선행 조건

            string[] invalid = StoryCatalogValidation.GetInvalidSceneIds(new[] { first, second, missing }); // 무효 장면 조회

            CollectionAssert.AreEquivalent(new[] { "first", "second", "missing" }, invalid); // 무효 장면 확인
        } // 테스트 끝

        private static StoryScene CreateScene( // 테스트 장면 생성
            string id, // 장면 ID
            StoryEventType eventType, // 발생 사건
            params StoryCondition[] conditions) // 해금 조건
        { // 생성 시작
            return new StoryScene( // 장면 반환
                id, // 장면 ID
                id, // 장면 제목
                eventType, // 발생 사건
                LocationId.TrainingCenter, // 기본 장소
                true, // 자동 재생 허용
                true, // 다시 보기 허용
                conditions, // 해금 조건
                new StoryChoice[0], // 선택지 없음
                new[] { new StoryLine("나", "테스트") }); // 대사 한 줄
        } // 생성 끝
    } // 클래스 끝
} // 공간 끝
