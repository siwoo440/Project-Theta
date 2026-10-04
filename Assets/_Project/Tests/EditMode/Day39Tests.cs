using System.Collections.Generic; // 목록 자료형
using NUnit.Framework; // 테스트 도구
using ProjectTheta.Save; // 저장 기능
namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 네임스페이스 시작
    public sealed class Day39SaveRegressionTests // 39일차 저장 회귀 테스트
    { // 테스트 클래스 시작
        [Test] // 단위 테스트 표시
        public void Latest_Slot_Ignores_Invalid_Summaries() // 잘못된 칸 번호 제외 검증
        { // 테스트 시작
            List<SaveSlotSummary> summaries = new List<SaveSlotSummary> // 저장 칸 요약 목록
            { // 목록 시작
                new SaveSlotSummary { Slot = -1, Exists = true, SavedTicks = 900 }, // 음수 칸 오염 자료
                new SaveSlotSummary { Slot = 1, Exists = true, SavedTicks = 100 }, // 정상 최근 칸 자료
                new SaveSlotSummary { Slot = 99, Exists = true, SavedTicks = 1000 } // 범위 초과 오염 자료
            }; // 목록 끝
            Assert.AreEqual(1, SaveSlotLogic.FindLatest(summaries)); // 정상 칸 선택 확인
            Assert.AreEqual(1, SaveSlotLogic.GetFallbackSlot(summaries)); // 대체 칸 선택 확인
        } // 테스트 끝
    } // 테스트 클래스 끝
} // 네임스페이스 끝
