using System; // 기본 자료형 참조
using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Core; // 결과 전달 규칙 참조
using ProjectTheta.Save; // 저장 규칙 참조
using ProjectTheta.Story; // 이야기 규칙 참조
using ProjectTheta.UI; // 로비 대사 규칙 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day46Tests // 46일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void Catalog_ContainsAllPlannedLobbyDialogues() // 기획 대사 전체 수록 검증
        { // 테스트 시작
            Assert.AreEqual(58, LobbyDialogueCatalog.All.Length); // 전체 대사 수 확인
            Assert.AreEqual("LOBBY_ENTER_01", LobbyDialogueCatalog.All[0].Id); // 첫 대사 ID 확인
            Assert.AreEqual("LOBBY_RARE_05", LobbyDialogueCatalog.All[57].Id); // 마지막 대사 ID 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void ReturnPriority_PrefersRampageThenRankDialogue() // 귀환 특수 대사 우선순위 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.Cleared = true; // 클리어 설정
            result.RankLabel = "S"; // S등급 설정
            result.RampageWindups = 1; // 폭주 경험 설정

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 귀환 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 대사 제외

            Assert.AreEqual("LOBBY_FOLLOWER_02", selected.Id); // 폭주 대사 우선 확인

            result.RampageWindups = 0; // 폭주 경험 제거
            selected = LobbyDialogueLogic.Select( // 일반 귀환 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 대사 제외

            StringAssert.StartsWith("LOBBY_CLEAR_S_", selected.Id); // S등급 대사 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RareDialogue_UsesFivePercentBoundary() // 희귀 대사 확률 경계 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            LobbyDialogueDefinition rare = LobbyDialogueLogic.Select( // 희귀 대사 선택
                LobbyDialogueTrigger.Enter, // 입장 조건
                save, // 저장 자료
                StageResultSummary.Empty, // 결과 없음
                new FixedRandom(0), // 고정 난수
                0.049f); // 희귀 확률 안쪽
            LobbyDialogueDefinition normal = LobbyDialogueLogic.Select( // 일반 대사 선택
                LobbyDialogueTrigger.Enter, // 입장 조건
                save, // 저장 자료
                StageResultSummary.Empty, // 결과 없음
                new FixedRandom(0), // 고정 난수
                0.05f); // 희귀 확률 경계

            StringAssert.StartsWith("LOBBY_RARE_", rare.Id); // 희귀 대사 확인
            StringAssert.StartsWith("LOBBY_ENTER_", normal.Id); // 일반 대사 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Selection_AvoidsImmediateRepeatAndRecordsSeenId() // 직전 대사 반복 방지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.LastLobbyDialogueId = "LOBBY_ENTER_01"; // 직전 대사 설정

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 입장 대사 선택
                LobbyDialogueTrigger.Enter, // 입장 조건
                save, // 저장 자료
                StageResultSummary.Empty, // 결과 없음
                new FixedRandom(0), // 첫 항목 난수
                1f); // 희귀 대사 제외
            bool changed = LobbyDialogueLogic.RecordSeen(save, selected); // 확인 기록 저장
            bool duplicate = LobbyDialogueLogic.RecordSeen(save, selected); // 중복 기록 시도

            Assert.AreNotEqual("LOBBY_ENTER_01", selected.Id); // 직전 대사 제외 확인
            Assert.IsTrue(changed); // 최초 기록 변경 확인
            Assert.IsFalse(duplicate); // 중복 기록 차단 확인
            CollectionAssert.Contains(save.SeenLobbyDialogues, selected.Id); // 확인 목록 저장 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void SaveCloneAndNormalize_PreserveValidLobbyDialogueState() // 로비 대사 저장 복제와 보정 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.LastLobbyDialogueId = "UNKNOWN"; // 잘못된 직전 ID 설정
            save.SeenLobbyDialogues = new[] // 확인 목록 설정
            { // 목록 시작
                "LOBBY_ENTER_01", // 정상 ID
                "LOBBY_ENTER_01", // 중복 ID
                "UNKNOWN" // 잘못된 ID
            }; // 목록 끝

            SaveDataLogic.Normalize(save); // 저장 보정
            SaveData copy = save.Clone(); // 저장 복제

            Assert.AreEqual(string.Empty, save.LastLobbyDialogueId); // 잘못된 직전 ID 제거 확인
            CollectionAssert.AreEqual(new[] { "LOBBY_ENTER_01" }, save.SeenLobbyDialogues); // 목록 보정 확인
            CollectionAssert.AreEqual(save.SeenLobbyDialogues, copy.SeenLobbyDialogues); // 복제 내용 확인
            Assert.AreNotSame(save.SeenLobbyDialogues, copy.SeenLobbyDialogues); // 배열 독립성 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void DiaryPaging_UsesTwentyEntriesPerPage() // 일기장 페이지 계산 검증
        { // 테스트 시작
            Assert.AreEqual(3, LobbyDialogueLogic.GetPageCount(58, 20)); // 전체 페이지 수 확인
            Assert.AreEqual(20, LobbyDialogueLogic.GetPageLength(58, 20, 0)); // 첫 페이지 수 확인
            Assert.AreEqual(18, LobbyDialogueLogic.GetPageLength(58, 20, 2)); // 마지막 페이지 수 확인
            Assert.AreEqual(0, LobbyDialogueLogic.GetPageLength(58, 20, 3)); // 범위 밖 페이지 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RelationshipDialogue_RequiresFourthRelationshipStory() // 관계 대사 해금 조건 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성

            Assert.IsFalse(LobbyDialogueLogic.IsRelationshipUnlocked(save)); // 초기 잠금 확인

            StoryProgressLogic.Complete(save, StoryCatalog.RiellaSharedBurdenId, false); // H04 완료

            Assert.IsTrue(LobbyDialogueLogic.IsRelationshipUnlocked(save)); // 관계 대사 해금 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void ReturnConditions_ConnectMasteryProgressNightAndFollowerHint() // 귀환 세부 조건 연결 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.HasLocation = true; // 장소 결과 설정
            result.LocationId = 0; // 첫 장소 설정
            result.Cleared = true; // 클리어 설정
            result.RankLabel = "A"; // A등급 설정
            save.LocationRecords = new[] // 장소 기록 설정
            { // 목록 시작
                new LocationStats { Location = 0, Clears = 6 } // 별 셋 기록
            }; // 목록 끝

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 숙련 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            Assert.AreEqual("LOBBY_MASTERY_02", selected.Id); // 별 셋 대사 확인

            save.LocationRecords[0].Clears = 2; // 반복 클리어 기록
            selected = LobbyDialogueLogic.Select( // 진행 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            Assert.AreEqual("LOBBY_PROGRESS_02", selected.Id); // 반복 장소 대사 확인

            result.Cleared = false; // 실패 설정
            result.NightMode = true; // 심야 도전 설정
            selected = LobbyDialogueLogic.Select( // 심야 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            StringAssert.StartsWith("LOBBY_NIGHT_", selected.Id); // 심야 대사 확인

            result.NightMode = false; // 심야 해제
            result.MaxFollowers = 1; // 동행 경험 설정
            selected = LobbyDialogueLogic.Select( // 동행 힌트 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            Assert.AreEqual("LOBBY_FOLLOWER_03", selected.Id); // 동행 힌트 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void LobbyResultInbox_DeliversConsumedResultOnlyOnce() // 귀환 결과 한 번 전달 검증
        { // 테스트 시작
            LobbyResultInbox inbox = new LobbyResultInbox(); // 결과 전달함 생성
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.TotalScore = 1234; // 확인 점수 설정

            inbox.Store(result); // 소비 결과 보관

            Assert.IsTrue(inbox.TryTake(out StageResultSummary first)); // 첫 전달 확인
            Assert.AreEqual(1234, first.TotalScore); // 전달 내용 확인
            Assert.IsFalse(inbox.TryTake(out StageResultSummary second)); // 중복 전달 차단 확인
            Assert.AreEqual(0, second.TotalScore); // 빈 결과 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RealisticFourthClear_AllowsRankDialogue() // 실제 반복 클리어 등급 대사 도달 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.LocationRecords = new[] // 장소 기록 설정
            { // 목록 시작
                new LocationStats { Location = 0, Clears = 4 } // 네 번째 클리어 기록
            }; // 목록 끝
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.HasLocation = true; // 장소 결과 설정
            result.LocationId = 0; // 첫 장소 설정
            result.Cleared = true; // 클리어 설정
            result.RankLabel = "S"; // S등급 설정

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 귀환 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            StringAssert.StartsWith("LOBBY_CLEAR_S_", selected.Id); // S등급 대사 도달 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RareDialogue_WaitsBehindSystemState() // 희귀 대사 시스템 우선순위 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.ContractEssence = 999999; // 성장 가능 정기 설정

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 입장 대사 선택
                LobbyDialogueTrigger.Enter, // 입장 조건
                save, // 저장 자료
                StageResultSummary.Empty, // 결과 없음
                new FixedRandom(0), // 고정 난수
                0.01f); // 희귀 확률 안쪽

            StringAssert.StartsWith("LOBBY_ESSENCE_", selected.Id); // 성장 상태 우선 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void CriticalSpeechCompletion_CannotBeOverwritten() // 출격 완료 알림 보호 검증
        { // 테스트 시작
            Assert.IsFalse(LobbyDialogueLogic.CanReplaceSpeech(true, true)); // 중요 알림 보유 중 교체 차단
            Assert.IsTrue(LobbyDialogueLogic.CanReplaceSpeech(true, false)); // 일반 대사 교체 허용
            Assert.IsTrue(LobbyDialogueLogic.CanReplaceSpeech(false, true)); // 재생 전 교체 허용
        } // 테스트 끝

        [Test] // 테스트 표시
        public void DiaryLayout_LeavesFooterButtonsClear() // 일기장 목록과 하단 버튼 분리 검증
        { // 테스트 시작
            float lastRowBottom = LobbyDialogueLogic.GetDiaryRowY(9) + LobbyDialogueLogic.DiaryRowHeight; // 마지막 행 아래 계산

            Assert.LessOrEqual(lastRowBottom, LobbyDialogueLogic.DiaryPageButtonY); // 하단 버튼 겹침 방지 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RepeatedExactMastery_FallsBackWithoutWrongStarLine() // 세부 조건 반복 대사 대체 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.LastLobbyDialogueId = "LOBBY_MASTERY_01"; // 직전 숙련 대사 설정
            save.LocationRecords = new[] // 장소 기록 설정
            { // 목록 시작
                new LocationStats { Location = 0, Clears = 3 } // 별 둘 기록
            }; // 목록 끝
            StageResultSummary result = StageResultSummary.Empty; // 빈 결과 생성
            result.HasLocation = true; // 장소 결과 설정
            result.LocationId = 0; // 첫 장소 설정
            result.Cleared = true; // 클리어 설정
            result.RankLabel = "S"; // S등급 설정

            LobbyDialogueDefinition selected = LobbyDialogueLogic.Select( // 귀환 대사 선택
                LobbyDialogueTrigger.Return, // 귀환 조건
                save, // 저장 자료
                result, // 직전 결과
                new FixedRandom(0), // 고정 난수
                1f); // 희귀 제외

            Assert.AreNotEqual("LOBBY_MASTERY_01", selected.Id); // 직전 대사 반복 방지 확인
            Assert.AreNotEqual("LOBBY_MASTERY_02", selected.Id); // 잘못된 별 셋 대사 방지 확인
            StringAssert.StartsWith("LOBBY_CLEAR_S_", selected.Id); // 등급 대사 대체 확인
        } // 테스트 끝

        private sealed class FixedRandom : Random // 고정 난수 도구
        { // 클래스 시작
            private readonly int _value; // 고정 값

            public FixedRandom( // 난수 도구 생성
                int value) // 고정 값
            { // 생성 시작
                _value = value; // 고정 값 저장
            } // 생성 끝

            public override int Next( // 범위 난수 반환
                int maxValue) // 최대 제외 값
            { // 반환 시작
                return maxValue <= 0 // 빈 범위 확인
                    ? 0 // 기본 값
                    : Math.Abs(_value) % maxValue; // 범위 값 반환
            } // 반환 끝
        } // 클래스 끝
    } // 클래스 끝
} // 공간 끝
