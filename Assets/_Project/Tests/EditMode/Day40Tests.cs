using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Disruptors; // 방해 세력 참조
using ProjectTheta.Stage.Locations; // 장소 자료 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day40Tests // 40일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void HighSchool_PreservesSerializedLocationId() // 저장 장소 번호 보존 검증
        { // 테스트 시작
            Assert.AreEqual(0, (int)LocationId.TrainingCenter); // 기존 번호 확인
            Assert.AreEqual(LocationId.TrainingCenter, LocationCatalog.StartLocation); // 시작 장소 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void HighSchool_UsesCurrentPresentation() // 최신 학교 표시 검증
        { // 테스트 시작
            LocationDefinition location = LocationCatalog.Get(LocationId.TrainingCenter); // 시작 장소 조회

            Assert.AreEqual("고등학교", location.DisplayName); // 장소 이름 확인
            Assert.AreEqual("본관 1F", location.GetFloorLabel(0)); // 층 이름 확인
            StringAssert.Contains("교사", location.DisruptorPreview); // 교사 표시 확인
            StringAssert.Contains("생활지도부장", location.DisruptorPreview); // 지도부장 표시 확인
            StringAssert.DoesNotContain("인기남", location.DisruptorPreview); // 인기남 제외 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Teacher_UsesApprovedDetectionAndChaseValues() // 교사 수치 검증
        { // 테스트 시작
            DisruptorProfile teacher = DisruptorCatalog.Get(DisruptorKind.TrainingAssistant); // 교사 자료 조회

            Assert.AreEqual("교사", teacher.DisplayName); // 표시 이름 확인
            Assert.AreEqual(32.5f, teacher.SightHalfAngle); // 반각 확인
            Assert.AreEqual(7f, teacher.SightRange); // 시야 거리 확인
            Assert.AreEqual(2.2f, teacher.MoveSpeed); // 순찰 속도 확인
            Assert.AreEqual(4.4f, teacher.RushSpeed); // 추적 속도 확인
            Assert.AreEqual(6f, teacher.ChaseSeconds); // 추적 시간 확인
            Assert.AreEqual(0.35f, teacher.DetectionSeconds); // 감지 확정 확인
            Assert.AreEqual(0.8f, teacher.CaptureRange); // 포획 거리 확인
            Assert.AreEqual(1f, teacher.BlackoutSeconds); // 암전 시간 확인
            Assert.AreEqual(4f, teacher.TimePenaltySeconds); // 시간 벌점 확인
            Assert.AreEqual(2f, teacher.ReacquireGraceSeconds); // 재감지 유예 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void Teacher_IgnoresWalkingAndDetectsRiskyActions() // 교사 행동 감지 검증
        { // 테스트 시작
            Assert.IsFalse(SchoolEnemyLogic.IsTeacherSuspicious(false, false, false)); // 걷기 허용 확인
            Assert.IsTrue(SchoolEnemyLogic.IsTeacherSuspicious(true, false, false)); // 달리기 감지 확인
            Assert.IsTrue(SchoolEnemyLogic.IsTeacherSuspicious(false, true, false)); // 대시 감지 확인
            Assert.IsTrue(SchoolEnemyLogic.IsTeacherSuspicious(false, false, true)); // 최면 감지 확인
            Assert.AreEqual(6f, SchoolEnemyLogic.ApplyTimePenalty(10f, 4f)); // 시간 벌점 확인
            Assert.AreEqual(0f, SchoolEnemyLogic.ApplyTimePenalty(3f, 4f)); // 시간 하한 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void DisciplineHead_UsesApprovedInterventionValues() // 생활지도부장 수치 검증
        { // 테스트 시작
            DisruptorProfile head = DisruptorCatalog.Get(DisruptorKind.HrEvaluator); // 지도부장 자료 조회

            Assert.AreEqual("생활지도부장", head.DisplayName); // 표시 이름 확인
            Assert.AreEqual(45f, head.SightHalfAngle); // 반각 확인
            Assert.AreEqual(8f, head.SightRange); // 시야 거리 확인
            Assert.AreEqual(2.4f, head.MoveSpeed); // 순찰 속도 확인
            Assert.AreEqual(4.2f, head.RushSpeed); // 돌진 속도 확인
            Assert.AreEqual(0.5f, head.DetectionSeconds); // 감지 확정 확인
            Assert.AreEqual(2.5f, head.PushDistance); // 밀치기 거리 확인
            Assert.AreEqual(0.6f, head.StunSeconds); // 경직 시간 확인
            Assert.AreEqual(3f, head.HypnosisBlockSeconds); // 최면 차단 확인
            Assert.AreEqual(10f, head.CooldownSeconds); // 재사용 시간 확인
            Assert.AreEqual(0.65f, head.SelfHypnosisMultiplier); // 본인 저항 확인
            Assert.AreEqual(20, head.EssenceReward); // 정기 보상 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void HighSchool_UsesOnlyGeumtaeyangAsRival() // 학교 경쟁자 구성 검증
        { // 테스트 시작
            LocationDefinition school = LocationCatalog.Get(LocationId.TrainingCenter); // 학교 자료 조회
            LocationDefinition beach = LocationCatalog.Get(LocationId.Beach); // 비교 장소 조회

            Assert.IsTrue(LocationGuideCatalog.RivalsAppear(school)); // 금태양 등장 확인
            Assert.IsFalse(LocationGuideCatalog.PopularGuyAppears(school)); // 학교 인기남 제외 확인
            Assert.IsTrue(LocationGuideCatalog.PopularGuyAppears(beach)); // 다른 장소 유지 확인
        } // 테스트 끝
    } // 클래스 끝
} // 공간 끝
