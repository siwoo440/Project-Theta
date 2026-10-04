using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Boss; // 엔딩 규칙 참조
using ProjectTheta.Run; // 심야 규칙 참조
using ProjectTheta.Save; // 저장 규칙 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.Story; // 이야기 규칙 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day45Tests // 45일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void AffinityGrant_IsIdempotentAndClampedToOneHundred() // 중복 지급과 상한 초과 방지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsTrue(RiellaAffinityLogic.Grant(save, "gift_alpha", 70)); // 첫 보상 지급
            Assert.IsFalse(RiellaAffinityLogic.Grant(save, "gift_alpha", 70)); // 같은 보상 차단
            Assert.IsTrue(RiellaAffinityLogic.Grant(save, "gift_beta", 70)); // 다른 보상 지급
            Assert.AreEqual(100, save.RiellaAffinity); // 호감도 상한 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void OldSave_RebuildsCompletedAffinityRewardsWithoutDuplicates() // 이전 저장 보상 복원 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.RiellaAffinity = 0; // 기존 호감도 없음
            save.RiellaAffinityRewards = null; // 기존 보상 기록 없음
            save.CompletedStories = new[] // 완료 장면 설정
            { // 목록 시작
                StoryCatalog.RiellaMorningAfterId, // H01 완료
                "clear_training" // 첫 지역 완료
            }; // 목록 끝

            RiellaAffinityLogic.Normalize(save); // 이전 저장 보상 복원
            RiellaAffinityLogic.Normalize(save); // 중복 보정 실행

            Assert.AreEqual(11, save.RiellaAffinity); // H01과 지역 보상 합계 확인
            Assert.AreEqual(2, save.RiellaAffinityRewards.Length); // 보상 출처 중복 방지 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void VersionTwoSave_RebuildsAffinityAfterStoryMigration() // 구버전 이야기 이관 보상 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.Version = 2; // 구버전 설정
            save.SeenStories = new[] // 기존 본 장면 설정
            { // 목록 시작
                StoryCatalog.RiellaMorningAfterId, // H01 장면
                "clear_training" // 첫 지역 장면
            }; // 목록 끝
            save.CompletedStories = new string[0]; // 신규 완료 목록 없음

            SaveDataLogic.Normalize(save); // 저장 전체 보정

            Assert.AreEqual(11, save.RiellaAffinity); // 이관 보상 합계 확인
            Assert.IsTrue(StoryProgressLogic.IsCompleted(save, StoryCatalog.RiellaMorningAfterId)); // 장면 완료 이관 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void StoryCompletion_GrantsConfiguredAffinityOnlyOnce() // 장면 완료 보상 중복 방지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // H01 최초 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // H01 중복 완료
            StoryProgressLogic.Complete(save, "clear_beach", false); // 지역 최초 완료
            StoryProgressLogic.Complete(save, "clear_beach", false); // 지역 중복 완료

            Assert.AreEqual(11, save.RiellaAffinity); // 최초 보상만 합산 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RelationshipEvents_UnlockFromRegionalAndMainMilestones() // 관계 이벤트 해금 순서 검증
        { // 테스트 시작
            SaveData save = CreateStoryFourSave(); // 스토리 4 완료 저장 생성

            StoryProgressLogic.Complete(save, "clear_training", false); // 첫 지역 완료

            Assert.AreEqual( // H02 해금 확인
                StoryCatalog.RiellaPromiseId, // 기대 장면 ID
                GetPendingHubStory(save)?.Id); // 허브 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaPromiseId, false); // H02 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeAId, false); // 인터루드 A 완료

            Assert.AreEqual( // H03 해금 확인
                StoryCatalog.RiellaCheckTogetherId, // 기대 장면 ID
                GetPendingHubStory(save)?.Id); // 허브 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaCheckTogetherId, false); // H03 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeBId, false); // 인터루드 B 완료

            Assert.AreEqual( // H04 해금 확인
                StoryCatalog.RiellaSharedBurdenId, // 기대 장면 ID
                GetPendingHubStory(save)?.Id); // 허브 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaSharedBurdenId, false); // H04 완료
            StoryProgressLogic.Complete(save, StoryCatalog.InterludeCId, false); // 인터루드 C 완료

            Assert.AreEqual( // H05 해금 확인
                StoryCatalog.RiellaSharedResponsibilityId, // 기대 장면 ID
                GetPendingHubStory(save)?.Id); // 허브 장면 조회

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaSharedResponsibilityId, false); // H05 완료
            StoryProgressLogic.Complete(save, StoryCatalog.StoryTruthId, false); // 스토리 13 완료

            Assert.AreEqual( // H06 해금 확인
                StoryCatalog.RiellaStayTogetherId, // 기대 장면 ID
                GetPendingHubStory(save)?.Id); // 허브 장면 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void EndingBranch_UsesFinalBattleResultAndAffinityBoundary() // 엔딩 승패와 호감도 경계 검증
        { // 테스트 시작
            Assert.AreEqual( // 최종전 패배 확인
                EndingDecision.EndingC, // 기대 분기
                EndingBranchLogic.Resolve(true, false, false, 100)); // 최종전 패배 판정
            Assert.AreEqual( // 낮은 호감도 승리 확인
                EndingDecision.EndingA, // 기대 분기
                EndingBranchLogic.Resolve(true, true, false, 79)); // 79 승리 판정
            Assert.AreEqual( // 기준 호감도 승리 확인
                EndingDecision.ChoiceAOrB, // 기대 분기
                EndingBranchLogic.Resolve(true, true, false, 80)); // 80 승리 판정
            Assert.AreEqual( // 포기 제외 확인
                EndingDecision.None, // 기대 분기
                EndingBranchLogic.Resolve(true, false, true, 100)); // 포기 판정
            Assert.AreEqual( // 일반 장소 제외 확인
                EndingDecision.None, // 기대 분기
                EndingBranchLogic.Resolve(false, false, false, 100)); // 일반 장소 판정
        } // 테스트 끝

        [Test] // 테스트 표시
        public void EndingProgress_RecordsDistinctRoutesAndCountsEveryEnding() // 엔딩 기록과 누적 횟수 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            EndingProgressLogic.Record(save, EndingId.A); // 엔딩 A 기록
            EndingProgressLogic.Record(save, EndingId.B); // 엔딩 B 기록
            EndingProgressLogic.Record(save, EndingId.A); // 엔딩 A 반복 기록

            Assert.AreEqual(3, save.Stats.Endings); // 전체 엔딩 횟수 확인
            CollectionAssert.AreEquivalent( // 고유 엔딩 기록 확인
                new[] { "ending_a", "ending_b" }, // 기대 엔딩 ID
                save.SeenEndings); // 실제 엔딩 ID
        } // 테스트 끝

        [Test] // 테스트 표시
        public void FinalDefeatEnding_UnlocksNightModeThroughStageResult() // 엔딩 C 심야 해금 연결 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.HasLocation = true; // 장소 결과 설정
            result.LocationId = (int)LocationId.RooftopClub; // 루프탑 설정
            result.EndingId = (int)EndingId.C; // 엔딩 C 설정

            save = SaveDataLogic.ApplyStageResult(save, result); // 결과 저장 반영

            Assert.AreEqual(1, save.Stats.Endings); // 엔딩 횟수 확인
            CollectionAssert.Contains(save.SeenEndings, "ending_c"); // 엔딩 C 기록 확인
            Assert.IsTrue(NightModeLogic.IsUnlocked(save)); // 심야 모드 해금 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void EndingPresentation_HasIndependentContentForAllRoutes() // 엔딩별 연출 자료 검증
        { // 테스트 시작
            Assert.AreEqual("ENDING A", EndingLogic.GetTitle(EndingId.A)); // 엔딩 A 제목 확인
            Assert.AreEqual("ENDING B", EndingLogic.GetTitle(EndingId.B)); // 엔딩 B 제목 확인
            Assert.AreEqual("ENDING C", EndingLogic.GetTitle(EndingId.C)); // 엔딩 C 제목 확인
            Assert.IsNotEmpty(EndingLogic.GetLines(EndingId.A)); // 엔딩 A 문장 확인
            Assert.IsNotEmpty(EndingLogic.GetLines(EndingId.B)); // 엔딩 B 문장 확인
            Assert.IsNotEmpty(EndingLogic.GetLines(EndingId.C)); // 엔딩 C 문장 확인
        } // 테스트 끝

        private static SaveData CreateStoryFourSave() // 스토리 4 완료 저장 생성
        { // 생성 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StoryProgressLogic.Complete(save, StoryCatalog.PrologueContractId, false); // 프롤로그 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstMissionBriefingId, false); // 첫 임무 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstHypnosisId, false); // 첫 최면 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FirstRecoveryId, false); // 첫 회수 완료
            StoryProgressLogic.Complete(save, StoryCatalog.LumiaEncounterId, false); // 르미아 조우 완료
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionBriefingId, false); // 경쟁 안내 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerStolenId, false); // 탈취 장면 완료
            StoryProgressLogic.Complete(save, StoryCatalog.FollowerReclaimedId, false); // 재탈환 완료
            StoryProgressLogic.Complete(save, StoryCatalog.CompetitionAftermathId, false); // 경쟁 정리 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaJealousyId, false); // 질투 장면 완료
            StoryProgressLogic.Complete(save, StoryCatalog.RiellaMorningAfterId, false); // H01 완료

            return save; // 저장 반환
        } // 생성 끝

        private static StoryScene GetPendingHubStory(SaveData save) // 허브 대기 장면 조회
        { // 조회 시작
            System.Collections.Generic.List<StoryScene> pending = StoryQueueLogic.GetAvailable( // 대기 장면 목록 조회
                save, // 저장 자료
                StoryCatalog.All, // 전체 장면 표
                new StoryContext(StoryEventType.HubReturn, LocationId.TrainingCenter)); // 허브 복귀 사건

            return pending.Count == 0 // 대기 장면 확인
                ? null // 장면 없음
                : pending[0]; // 첫 장면 반환
        } // 조회 끝
    } // 클래스 끝
} // 공간 끝
