using System; // 배열 기능 참조
using System.Linq; // 목록 조회 기능 참조
using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Save; // 저장 자료 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.Story; // 이야기 자료 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day43Tests // 43일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void FreshSave_QueuesOnlyContractPrologueInSafeArea() // 새 저장 프롤로그 단독 재생 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            string[] pending = GetIds( // 대기 장면 ID 조회
                save, // 현재 저장
                StoryEventType.SafeArea, // 안전 구역 사건
                LocationId.TrainingCenter); // 기본 장소

            CollectionAssert.AreEqual( // 프롤로그 단독 확인
                new[] { StoryCatalog.PrologueContractId }, // 기대 장면
                pending); // 실제 장면
        } // 테스트 끝

        [Test] // 테스트 표시
        public void FirstMission_StartsAtAnySelectedLocationAfterPrologue() // 임의 장소 첫 임무 시작 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 프롤로그 완료

            string[] pending = GetIds( // 대기 장면 ID 조회
                save, // 현재 저장
                StoryEventType.FirstStageEnter, // 첫 장소 진입 사건
                LocationId.Beach); // 해변가 선택

            CollectionAssert.AreEqual( // 첫 임무 안내 확인
                new[] { StoryCatalog.FirstMissionBriefingId }, // 기대 장면
                pending); // 실제 장면
        } // 테스트 끝

        [Test] // 테스트 표시
        public void FirstMission_UnlocksHypnosisRecoveryAndLumiaInOrder() // 첫 임무 튜토리얼 순서 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 프롤로그 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstMissionBriefingId, false); // 첫 임무 안내 완료

            CollectionAssert.AreEqual( // 첫 최면 안내 확인
                new[] { StoryCatalog.FirstHypnosisId }, // 기대 장면
                GetIds(save, StoryEventType.HypnosisSucceeded, LocationId.Beach)); // 최면 사건 조회
            Assert.AreEqual(0, GetIds(save, StoryEventType.RecoveryConfirmed, LocationId.Beach).Length); // 조기 회수 안내 차단

            StoryProgressLogic.Complete(save, StoryCatalog.FirstHypnosisId, false); // 첫 최면 완료

            CollectionAssert.AreEqual( // 첫 회수 안내 확인
                new[] { StoryCatalog.FirstRecoveryId }, // 기대 장면
                GetIds(save, StoryEventType.RecoveryConfirmed, LocationId.Beach)); // 회수 사건 조회

            StoryProgressLogic.Complete(save, StoryCatalog.FirstRecoveryId, false); // 첫 회수 완료

            CollectionAssert.AreEqual( // 르미아 조우 확인
                new[] { StoryCatalog.LumiaEncounterId }, // 기대 장면
                GetIds(save, StoryEventType.LocationClear, LocationId.Beach)); // 장소 클리어 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void CompetitionArc_UnlocksContestReclaimAndRiellaRelationInOrder() // 경쟁과 관계 변화 순서 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            CompleteThroughFirstMission(save); // 첫 임무 완료 처리

            CollectionAssert.AreEqual( // 경쟁 임무 안내 확인
                new[] { StoryCatalog.CompetitionBriefingId }, // 기대 장면
                GetIds(save, StoryEventType.SafeArea, LocationId.TrainingCenter)); // 안전 구역 조회

            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionBriefingId, false); // 경쟁 임무 안내 완료

            CollectionAssert.AreEqual( // 동행자 탈취 안내 확인
                new[] { StoryCatalog.FollowerStolenId }, // 기대 장면
                GetIds(save, StoryEventType.FollowerStolen, LocationId.SubwayStation)); // 탈취 사건 조회

            StoryProgressLogic.Complete(save, StoryCatalog.FollowerStolenId, false); // 탈취 안내 완료

            CollectionAssert.AreEqual( // 재탈환 안내 확인
                new[] { StoryCatalog.FollowerReclaimedId }, // 기대 장면
                GetIds(save, StoryEventType.FollowerReclaimed, LocationId.SubwayStation)); // 재탈환 사건 조회

            StoryProgressLogic.Complete(save, StoryCatalog.FollowerReclaimedId, false); // 재탈환 안내 완료

            CollectionAssert.AreEqual( // 경쟁 임무 정리 확인
                new[] { StoryCatalog.CompetitionAftermathId }, // 기대 장면
                GetIds(save, StoryEventType.LocationClear, LocationId.SubwayStation)); // 장소 클리어 조회

            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionAftermathId, false); // 경쟁 임무 정리 완료

            Assert.AreEqual( // 지도 관계 장면 차단 확인
                0, // 기대 장면 수
                GetIds(save, StoryEventType.MapEnter, LocationId.SubwayStation).Length); // 지도 진입 조회
            CollectionAssert.AreEqual( // 허브 관계 장면 확인
                new[] { StoryCatalog.RiellaJealousyId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.SubwayStation)); // 허브 복귀 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void StoryFour_MorningAfterUnlocksRegionalEpisodes() // 스토리 4 지역 개방 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            CompleteThroughCompetition(save); // 경쟁 임무 완료 처리
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionAftermathId, false); // 경쟁 정리 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaJealousyId, false); // 질투 장면 완료

            CollectionAssert.AreEqual( // 다음 날 아침 장면 확인
                new[] { StoryCatalog.RiellaMorningAfterId }, // 기대 장면
                GetIds(save, StoryEventType.MapEnter, LocationId.TrainingCenter)); // 지도 진입 조회
            Assert.IsFalse(StoryArcLogic.AreRegionalEpisodesUnlocked(save)); // 완료 전 지역 잠금 확인

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // 다음 날 장면 완료

            Assert.IsTrue(StoryArcLogic.AreRegionalEpisodesUnlocked(save)); // 완료 후 지역 개방 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RegionalPlaceholders_StayLockedUntilStoryFourCompletes() // 기존 지역 장면 잠금 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 프롤로그 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstMissionBriefingId, false); // 첫 임무 안내 완료

            Assert.AreEqual( // 스토리 4 이전 잠금 확인
                0, // 기대 장면 수
                GetIds(save, StoryEventType.LocationEnter, LocationId.Beach).Length); // 지역 입장 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // 스토리 4 완료

            CollectionAssert.AreEqual( // 스토리 4 이후 지역 장면 확인
                new[] { "enter_beach" }, // 기대 지역 장면
                GetIds(save, StoryEventType.LocationEnter, LocationId.Beach)); // 지역 입장 장면 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void GameplayMoment_HypnosisRoutesCaptureAndReclaimSeparately() // 최면 사건 분리 검증
        { // 테스트 시작
            Assert.AreEqual( // 첫 확보 사건 확인
                StoryEventType.HypnosisSucceeded, // 기대 첫 확보 사건
                StageStoryEventLogic.GetHypnosisEvent(false)); // 일반 최면 변환
            Assert.AreEqual( // 재탈환 사건 확인
                StoryEventType.FollowerReclaimed, // 기대 재탈환 사건
                StageStoryEventLogic.GetHypnosisEvent(true)); // 재최면 변환
        } // 테스트 끝

        [Test] // 테스트 표시
        public void GameplayMoment_DuelWinDoesNotCountAsFollowerReclaim() // 힘겨루기 재탈환 오판 방지 검증
        { // 테스트 시작
            Assert.AreEqual( // 힘겨루기 사건 확인
                StoryEventType.None, // 이야기 사건 없음
                StageStoryEventLogic.GetDuelEvent()); // 힘겨루기 변환
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Day43Catalog_HasUniqueValidDependenciesAndStableOrder() // 43일차 장면 표 유효성 검증
        { // 테스트 시작
            string[] expected = new[] // 기대 장면 순서
            { // 목록 시작
                StoryCatalog.PrologueContractId, // 프롤로그
                StoryCatalog.FirstMissionBriefingId, // 첫 임무 안내
                StoryCatalog.FirstHypnosisId, // 첫 최면
                StoryCatalog.FirstRecoveryId, // 첫 회수
                StoryCatalog.LumiaEncounterId, // 르미아 조우
                StoryCatalog.CompetitionBriefingId, // 경쟁 임무 안내
                StoryCatalog.FollowerStolenId, // 동행자 탈취
                StoryCatalog.FollowerReclaimedId, // 동행자 재탈환
                StoryCatalog.CompetitionAftermathId, // 경쟁 임무 정리
                StoryCatalog.RiellaJealousyId, // 리엘라 질투
                StoryCatalog.RiellaMorningAfterId // 다음 날 아침
            }; // 목록 끝
            string[] actual = StoryCatalog.All // 전체 장면 조회
                .Where(scene => expected.Contains(scene.Id)) // 43일차 장면 선별
                .Select(scene => scene.Id) // 장면 ID 변환
                .ToArray(); // 배열 생성

            CollectionAssert.AreEqual(expected, actual); // 장면 순서 확인
            CollectionAssert.IsEmpty(StoryCatalogValidation.GetInvalidSceneIds(StoryCatalog.All)); // 전체 의존성 확인
            Assert.AreEqual(expected.Length, expected.Distinct().Count()); // 기대 ID 중복 확인
        } // 테스트 끝

        private static void CompleteThroughFirstMission(SaveData save) // 첫 임무 완료 도우미
        { // 처리 시작
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 프롤로그 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstMissionBriefingId, false); // 임무 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstHypnosisId, false); // 첫 최면 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstRecoveryId, false); // 첫 회수 완료
            StoryProgressLogic.Complete(save, StoryCatalog.LumiaEncounterId, false); // 르미아 조우 완료
        } // 처리 끝

        private static void CompleteThroughCompetition(SaveData save) // 경쟁 임무 완료 도우미
        { // 처리 시작
            CompleteThroughFirstMission(save); // 첫 임무 완료 처리
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionBriefingId, false); // 경쟁 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerStolenId, false); // 탈취 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerReclaimedId, false); // 재탈환 안내 완료
        } // 처리 끝

        private static string[] GetIds( // 사건별 장면 ID 조회
            SaveData save, // 현재 저장
            StoryEventType eventType, // 발생 사건
            LocationId location) // 발생 장소
        { // 조회 시작
            return StoryQueueLogic.GetAvailable( // 대기 장면 조회
                    save, // 현재 저장
                    StoryCatalog.All, // 전체 장면
                    new StoryContext(eventType, location)) // 사건 자료
                .Select(scene => scene.Id) // ID 변환
                .ToArray(); // 배열 반환
        } // 조회 끝
    } // 클래스 끝
} // 공간 끝
