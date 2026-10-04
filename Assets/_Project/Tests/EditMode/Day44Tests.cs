using System; // 동작 전달 기능 참조
using System.Linq; // 목록 조회 기능 참조
using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Save; // 저장 자료 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.Story; // 이야기 자료 참조
using ProjectTheta.UI; // 이야기 재생 기능 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day44Tests // 44일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void InterludeA_RequiresTwoCompletedRegionalStories() // 인터루드 A 조기 해금 방지 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성
            CompleteRegions(save, "clear_training"); // 한 지역 완료

            Assert.AreEqual(0, GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter).Length); // 한 지역 해금 차단

            CompleteRegions(save, "clear_beach"); // 두 번째 지역 완료

            CollectionAssert.AreEqual( // 인터루드 A 해금 확인
                new[] { StoryCatalog.InterludeAId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void LumiaContest_UsesStealAndReclaimEventsWithoutBlockingInterludeB() // 선택형 르미아 쟁탈 사건 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성
            CompleteRegions(save, "clear_training", "clear_beach"); // 두 지역 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeAId, false); // 인터루드 A 완료

            CollectionAssert.AreEqual( // 르미아 쟁탈 확인
                new[] { StoryCatalog.LumiaContestId }, // 기대 장면
                GetIds(save, StoryEventType.FollowerStolen, LocationId.NightMarket)); // 탈취 사건 조회

            CompleteRegions(save, "clear_subway", "clear_fitness"); // 네 지역 완료

            CollectionAssert.AreEqual( // 쟁탈 미완료와 무관한 인터루드 B 확인
                new[] { StoryCatalog.InterludeBId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회

            StoryProgressLogic.Complete(save, StoryCatalog.LumiaContestId, false); // 쟁탈 장면 완료

            CollectionAssert.AreEqual( // 르미아 재탈환 확인
                new[] { StoryCatalog.LumiaReclaimId }, // 기대 장면
                GetIds(save, StoryEventType.FollowerReclaimed, LocationId.NightMarket)); // 재탈환 사건 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void InterludesBAndC_RequireFourAndSixRegionalStories() // 인터루드 누적 지역 조건 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성
            CompleteRegions(save, "clear_training", "clear_beach", "clear_subway", "clear_fitness"); // 네 지역 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeAId, false); // 인터루드 A 완료

            CollectionAssert.AreEqual( // 인터루드 B 확인
                new[] { StoryCatalog.InterludeBId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회

            StoryProgressLogic.Complete(save, StoryCatalog.InterludeBId, false); // 인터루드 B 완료
            CompleteRegions(save, "clear_market"); // 다섯 지역 완료

            Assert.AreEqual(0, GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter).Length); // 여섯 지역 전 차단

            CompleteRegions(save, "clear_mall"); // 여섯 지역 완료

            CollectionAssert.AreEqual( // 인터루드 C 확인
                new[] { StoryCatalog.InterludeCId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void TruthAndBattleEve_RequireAllSevenGeneralRegions() // 진상과 결전 전야 조건 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성
            CompleteRegions( // 일곱 일반 지역 완료
                save, // 현재 저장
                "clear_training", // 고등학교 완료
                "clear_beach", // 해변가 완료
                "clear_subway", // 지하철 완료
                "clear_fitness", // 스포츠센터 완료
                "clear_market", // 야시장 완료
                "clear_mall", // 쇼핑몰 완료
                "clear_office"); // 오피스 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeAId, false); // 인터루드 A 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeBId, false); // 인터루드 B 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeCId, false); // 인터루드 C 완료

            CollectionAssert.AreEqual( // 진상 장면 확인
                new[] { StoryCatalog.StoryTruthId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회

            StoryProgressLogic.Complete(save, StoryCatalog.StoryTruthId, false); // 진상 장면 완료

            CollectionAssert.AreEqual( // 결전 전야 확인
                new[] { StoryCatalog.BattleEveId }, // 기대 장면
                GetIds(save, StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void EventPlayback_RequeriesUntilNoNewSceneRemains() // 연속 해금 장면 재조회 검증
        { // 테스트 시작
            int batchCount = 0; // 재생 묶음 수
            int finishCount = 0; // 완료 알림 수
            Func<Action, bool> playNext = continuation => // 다음 묶음 재생 대역
            { // 대역 시작
                batchCount++; // 재생 요청 증가

                if (batchCount > 2) // 남은 묶음 확인
                { // 빈 묶음 시작
                    return false; // 재생 없음 반환
                } // 빈 묶음 끝

                continuation(); // 묶음 완료 알림

                return true; // 재생 시작 반환
            }; // 대역 끝

            bool started = StoryPlayback.PlaySequence( // 연속 재생 실행
                playNext, // 다음 묶음 재생
                () => finishCount++); // 최종 완료 기록

            Assert.IsTrue(started); // 최초 재생 시작 확인
            Assert.AreEqual(3, batchCount); // 두 묶음 뒤 빈 목록 재조회 확인
            Assert.AreEqual(1, finishCount); // 최종 완료 한 번 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Rooftop_RemainsLockedUntilBattleEveCompletes() // 루프탑 조기 진입 방지 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성

            Assert.IsTrue(StoryArcLogic.IsLocationUnlocked(save, LocationId.Beach)); // 일반 지역 허용
            Assert.IsFalse(StoryArcLogic.IsLocationUnlocked(save, LocationId.RooftopClub)); // 루프탑 잠금 확인

            StoryProgressLogic.Complete(save, StoryCatalog.BattleEveId, false); // 결전 전야 완료

            Assert.IsTrue(StoryArcLogic.IsLocationUnlocked(save, LocationId.RooftopClub)); // 루프탑 해금 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RooftopBoss_QueuesOpeningAndVictoryDialogueInOrder() // 보스 장면 순서 검증
        { // 테스트 시작
            SaveData save = CreateRegionalSave(); // 지역 이야기 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.BattleEveId, false); // 결전 전야 완료

            Assert.AreEqual( // 보스 개막 확인
                StoryCatalog.BossOpeningId, // 기대 장면
                StoryLogic.GetEnterScene(save, LocationId.RooftopClub).Id); // 실제 진입 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.BossOpeningId, false); // 보스 개막 완료

            CollectionAssert.AreEqual( // 승리 대화 확인
                new[] { StoryCatalog.BossVictoryId }, // 기대 장면
                GetIds(save, StoryEventType.BossVictory, LocationId.RooftopClub)); // 보스 승리 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Day44Catalog_HasValidDependenciesAndExpectedScenes() // 44일차 장면 표 검증
        { // 테스트 시작
            string[] expected = new[] // 기대 장면 ID
            { // 목록 시작
                StoryCatalog.InterludeAId, // 인터루드 A
                StoryCatalog.LumiaContestId, // 르미아 쟁탈
                StoryCatalog.LumiaReclaimId, // 르미아 재탈환
                StoryCatalog.InterludeBId, // 인터루드 B
                StoryCatalog.InterludeCId, // 인터루드 C
                StoryCatalog.StoryTruthId, // 두 개의 진실
                StoryCatalog.BattleEveId, // 결전 전야
                StoryCatalog.BossOpeningId, // 보스 개막
                StoryCatalog.BossVictoryId // 보스 승리
            }; // 목록 끝
            string[] actual = StoryCatalog.All // 전체 장면 조회
                .Where(scene => expected.Contains(scene.Id)) // 44일차 장면 선별
                .Select(scene => scene.Id) // 장면 ID 변환
                .ToArray(); // 배열 생성

            CollectionAssert.AreEqual(expected, actual); // 장면 순서 확인
            CollectionAssert.IsEmpty(StoryCatalogValidation.GetInvalidSceneIds(StoryCatalog.All)); // 전체 의존성 확인
        } // 테스트 끝

        private static SaveData CreateRegionalSave() // 지역 이야기 저장 생성
        { // 생성 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 계약 프롤로그 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstMissionBriefingId, false); // 첫 임무 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstHypnosisId, false); // 첫 최면 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstRecoveryId, false); // 첫 회수 완료
            StoryProgressLogic.Complete(save, StoryCatalog.LumiaEncounterId, false); // 르미아 조우 완료
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionBriefingId, false); // 경쟁 임무 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerStolenId, false); // 동행자 탈취 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerReclaimedId, false); // 동행자 재탈환 완료
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionAftermathId, false); // 경쟁 임무 정리 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaJealousyId, false); // 리엘라 질투 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // 스토리 4 완료

            return save; // 저장 반환
        } // 생성 끝

        private static void CompleteRegions( // 지역 완료 처리
            SaveData save, // 현재 저장
            params string[] sceneIds) // 지역 완료 장면 ID
        { // 처리 시작
            foreach (string sceneId in sceneIds) // 장면 ID 순회
            { // 순회 시작
                StoryProgressLogic.Complete(save, sceneId, false); // 지역 완료 기록
            } // 순회 끝
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
