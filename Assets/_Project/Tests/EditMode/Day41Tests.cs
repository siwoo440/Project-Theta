using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Save; // 저장 자료 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조
using ProjectTheta.UI; // 지도 안내 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day41Tests // 41일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void Normalize_OldSaveGetsNeutralRegionalRisk() // 구버전 저장 보정 검증
        { // 테스트 시작
            SaveData oldSave = new SaveData // 구버전 저장 생성
            { // 자료 시작
                Version = 1, // 구버전 지정
                CurrentWorldTime = 99, // 손상 시간 지정
                RegionRiskMultipliers = null // 누락 배율 지정
            }; // 자료 끝

            SaveData normalized = SaveDataLogic.Normalize(oldSave); // 저장 보정 실행

            Assert.AreEqual(SaveDataLogic.CurrentVersion, normalized.Version); // 버전 갱신 확인
            Assert.AreEqual((int)WorldTimeOfDay.Morning, normalized.CurrentWorldTime); // 아침 기본값 확인
            Assert.AreEqual(8, normalized.RegionRiskMultipliers.Length); // 지역 수 확인
            CollectionAssert.AreEqual(new[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }, normalized.RegionRiskMultipliers); // 중립 배율 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Roll_ProducesDeterministicIndependentRegionalValues() // 지역 배율 생성 검증
        { // 테스트 시작
            float[] first = RegionRiskLogic.RollMultipliers(41041); // 첫 배율 생성
            float[] second = RegionRiskLogic.RollMultipliers(41041); // 같은 시드 생성

            Assert.AreEqual(8, first.Length); // 지역 수 확인
            CollectionAssert.AreEqual(first, second); // 결정성 확인
            Assert.Greater(new System.Collections.Generic.HashSet<float>(first).Count, 1); // 독립 값 확인

            foreach (float value in first) // 배율 순회
            { // 순회 시작
                Assert.That(value, Is.InRange(0.8f, 1.4f)); // 범위 확인
            } // 순회 끝
        } // 테스트 끝

        [Test] // 테스트 표시
        public void ClearedResult_AdvancesTimeAndRerollsOnce() // 클리어 시간 진행 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            float[] before = (float[])save.RegionRiskMultipliers.Clone(); // 이전 배율 복사
            StageResultSummary cleared = new StageResultSummary { Cleared = true }; // 클리어 결과 생성

            bool changed = RegionRiskLogic.ApplyStageTransition(save, cleared, 41041); // 전환 적용

            Assert.IsTrue(changed); // 전환 여부 확인
            Assert.AreEqual((int)WorldTimeOfDay.Day, save.CurrentWorldTime); // 낮 진행 확인
            CollectionAssert.AreNotEqual(before, save.RegionRiskMultipliers); // 재추첨 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void FailedResult_DoesNotAdvanceOrReroll() // 실패 유지 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            float[] before = (float[])save.RegionRiskMultipliers.Clone(); // 이전 배율 복사
            StageResultSummary failed = new StageResultSummary { Cleared = false }; // 실패 결과 생성

            bool changed = RegionRiskLogic.ApplyStageTransition(save, failed, 41041); // 전환 적용

            Assert.IsFalse(changed); // 전환 없음 확인
            Assert.AreEqual((int)WorldTimeOfDay.Morning, save.CurrentWorldTime); // 시간 유지 확인
            CollectionAssert.AreEqual(before, save.RegionRiskMultipliers); // 배율 유지 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Time_CyclesFromNightBackToMorning() // 시간 순환 검증
        { // 테스트 시작
            Assert.AreEqual(WorldTimeOfDay.Day, RegionRiskLogic.GetNextTime(WorldTimeOfDay.Morning)); // 아침 다음 확인
            Assert.AreEqual(WorldTimeOfDay.Evening, RegionRiskLogic.GetNextTime(WorldTimeOfDay.Day)); // 낮 다음 확인
            Assert.AreEqual(WorldTimeOfDay.Night, RegionRiskLogic.GetNextTime(WorldTimeOfDay.Evening)); // 저녁 다음 확인
            Assert.AreEqual(WorldTimeOfDay.Morning, RegionRiskLogic.GetNextTime(WorldTimeOfDay.Night)); // 밤 순환 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RiskMultiplier_AppliesToAllApprovedBalanceInputs() // 난이도 적용 검증
        { // 테스트 시작
            Assert.AreEqual(140, RegionRiskLogic.ApplyTargetEssence(100, 1.4f)); // 목표 정기 확인
            Assert.AreEqual(1f / 1.4f, RegionRiskLogic.GetHypnosisSpeedMultiplier(1.4f), 0.0001f); // 최면 저항 확인
            Assert.AreEqual(14f, RegionRiskLogic.ApplyAlertAmount(10f, 1.4f), 0.0001f); // 경계 상승 확인
            Assert.AreEqual(-10f, RegionRiskLogic.ApplyAlertAmount(-10f, 1.4f), 0.0001f); // 경계 감소 보존 확인
            Assert.AreEqual(10f / 1.4f, RegionRiskLogic.ApplyAbilityCooldown(10f, 1.4f), 0.0001f); // 재사용 시간 확인
            Assert.AreEqual(140, RegionRiskLogic.ApplyContractReward(100, 1.4f)); // 계약 보상 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Clone_PreservesRegionalRiskWithoutSharingArray() // 저장 복제 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.CurrentWorldTime = (int)WorldTimeOfDay.Evening; // 저녁 지정
            save.RegionRiskMultipliers[0] = 1.23f; // 학교 배율 지정

            SaveData copy = save.Clone(); // 저장 복제
            copy.RegionRiskMultipliers[0] = 0.8f; // 복제본 변경

            Assert.AreEqual((int)WorldTimeOfDay.Evening, copy.CurrentWorldTime); // 시간 복제 확인
            Assert.AreEqual(1.23f, save.RegionRiskMultipliers[0]); // 원본 분리 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void MapSummary_ShowsWorldTimeAndRegionalRisk() // 지도 문구 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.CurrentWorldTime = (int)WorldTimeOfDay.Evening; // 저녁 지정
            save.RegionRiskMultipliers[(int)LocationId.Beach] = 1.27f; // 해변 배율 지정

            string summary = RegionRiskLogic.BuildMapSummary(save, LocationId.Beach); // 지도 문구 생성

            StringAssert.Contains("현재 시간 저녁", summary); // 시간 표시 확인
            StringAssert.Contains("위험 ×1.27", summary); // 위험 표시 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RuntimeState_UsesSelectedRegionRisk() // 런타임 위험 상태 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.CurrentWorldTime = (int)WorldTimeOfDay.Night; // 밤 지정
            save.RegionRiskMultipliers[(int)LocationId.NightMarket] = 1.31f; // 야시장 배율 지정

            RegionRiskState.Configure(save, LocationId.NightMarket); // 런타임 상태 연결

            Assert.AreEqual(WorldTimeOfDay.Night, RegionRiskState.CurrentTime); // 현재 시간 확인
            Assert.AreEqual(1.31f, RegionRiskState.CurrentMultiplier); // 현재 배율 확인

            RegionRiskState.Configure(null, LocationId.TrainingCenter); // 상태 정리
        } // 테스트 끝

        [Test] // 테스트 표시
        public void GuideRows_ShowRiskAdjustedTarget() // 지도 목표 표시 검증
        { // 테스트 시작
            SaveData save = SaveDataLogic.CreateDefault(); // 기본 저장 생성
            save.RegionRiskMultipliers[(int)LocationId.TrainingCenter] = 1.2f; // 학교 배율 지정
            LocationDefinition school = LocationCatalog.Get(LocationId.TrainingCenter); // 학교 자료 조회

            string rows = LocationGuideLogic.BuildCoreRows(school, 0, false, save); // 핵심 수치 생성

            StringAssert.Contains("목표 정기  372", rows); // 조정 목표 확인
            StringAssert.Contains("위험 ×1.20", rows); // 위험 배율 확인
        } // 테스트 끝
    } // 클래스 끝
} // 공간 끝
