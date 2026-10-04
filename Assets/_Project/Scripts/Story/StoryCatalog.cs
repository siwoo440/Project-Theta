using System.Collections.Generic;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Story
{
    /// <summary>
    /// 이야기 장면 표다 (36일차). 대사는 초안이며 자유롭게 고쳐도 된다(ID만 유지).
    ///
    ///   장소마다 입장 1 · 첫 클리어 1 (루프탑 클럽 입장은 라이벌 대면)
    ///   라이벌 도발 2 (클리어 장소 3 · 6곳)
    ///   후일담 2 (엔딩 1회 · 도시 지배도 100%)
    /// 순서는 다시 보기 목록 순서이기도 하다.
    /// </summary>
    public static class StoryCatalog
    {
        public const string Me = "나";
        public const string Rival = "라이벌";
        public const string Protagonist = "주인공"; // 주인공 화자
        public const string Riella = "리엘라"; // 계약 서큐버스 화자
        public const string Lumia = "르미아"; // 라이벌 서큐버스 화자
        public const string PrologueContractId = "prologue_contract"; // 계약 프롤로그 ID
        public const string FirstMissionBriefingId = "story_01_first_mission"; // 첫 임무 안내 ID
        public const string FirstHypnosisId = "story_01_first_hypnosis"; // 첫 최면 ID
        public const string FirstRecoveryId = "story_01_first_recovery"; // 첫 회수 ID
        public const string LumiaEncounterId = "story_02_lumia_encounter"; // 르미아 조우 ID
        public const string CompetitionBriefingId = "story_03_competition_briefing"; // 경쟁 임무 안내 ID
        public const string FollowerStolenId = "story_03_follower_stolen"; // 동행자 탈취 ID
        public const string FollowerReclaimedId = "story_03_follower_reclaimed"; // 동행자 재탈환 ID
        public const string CompetitionAftermathId = "story_03_competition_aftermath"; // 경쟁 임무 정리 ID
        public const string RiellaJealousyId = "story_04_riella_jealousy"; // 리엘라 질투 ID
        public const string RiellaMorningAfterId = "story_04_morning_after"; // 다음 날 아침 ID
        public const string RiellaPromiseId = "riella_h02_promise"; // 리엘라 H02 ID
        public const string RiellaCheckTogetherId = "riella_h03_check_together"; // 리엘라 H03 ID
        public const string RiellaSharedBurdenId = "riella_h04_shared_burden"; // 리엘라 H04 ID
        public const string RiellaSharedResponsibilityId = "riella_h05_shared_responsibility"; // 리엘라 H05 ID
        public const string RiellaStayTogetherId = "riella_h06_stay_together"; // 리엘라 H06 ID
        public const string InterludeAId = "main_interlude_a_lumia_returns"; // 르미아 재등장 ID
        public const string LumiaContestId = "main_interlude_a_lumia_contest"; // 르미아 쟁탈 ID
        public const string LumiaReclaimId = "main_interlude_a_lumia_reclaim"; // 르미아 재탈환 ID
        public const string InterludeBId = "main_interlude_b_hidden_past"; // 숨겨진 과거 ID
        public const string InterludeCId = "main_interlude_c_city_anomaly"; // 도시 이상 ID
        public const string StoryTruthId = "story_13_two_truths"; // 두 개의 진실 ID
        public const string BattleEveId = "story_13_4_battle_eve"; // 결전 전야 ID
        public const string BossOpeningId = "story_14_rooftop_night"; // 루프탑 결전 개막 ID
        public const string BossVictoryId = "story_14_lumia_last_dialogue"; // 르미아 마지막 대화 ID

        private static StoryLine L(string speaker, string text)
        {
            return new StoryLine(speaker, text);
        }

        private static StoryLine N(string text)
        {
            return new StoryLine(string.Empty, text);
        }

        private static StoryScene Day43Scene( // 43일차 장면 생성
            string id, // 장면 ID
            string title, // 장면 제목
            StoryEventType eventType, // 발생 사건
            string prerequisite, // 선행 장면 ID
            StoryChoice[] choices, // 선택지 목록
            params StoryLine[] lines) // 대사 목록
        { // 생성 시작
            StoryCondition[] conditions = string.IsNullOrEmpty(prerequisite) // 선행 장면 확인
                ? new StoryCondition[0] // 선행 조건 없음
                : new[] { StoryCondition.StoryCompleted(prerequisite) }; // 선행 완료 조건

            return new StoryScene( // 장면 반환
                id, // 장면 ID
                title, // 장면 제목
                eventType, // 발생 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                true, // 자동 재생 허용
                true, // 일기장 재생 허용
                eventType == StoryEventType.LocationClear, // 장소 클리어 공용 처리
                conditions, // 해금 조건
                choices ?? new StoryChoice[0], // 선택지 보정
                lines); // 대사 목록
        } // 생성 끝

        private static StoryScene Day44Scene( // 44일차 장면 생성
            string id, // 장면 ID
            string title, // 장면 제목
            StoryEventType eventType, // 발생 사건
            LocationId location, // 관련 장소
            bool matchAnyLocation, // 모든 장소 일치 여부
            StoryCondition[] conditions, // 해금 조건
            params StoryLine[] lines) // 대사 목록
        { // 생성 시작
            return new StoryScene( // 장면 반환
                id, // 장면 ID
                title, // 장면 제목
                eventType, // 발생 사건
                location, // 관련 장소
                true, // 자동 재생 허용
                true, // 일기장 재생 허용
                matchAnyLocation, // 모든 장소 일치 여부
                conditions, // 해금 조건
                new StoryChoice[0], // 선택지 없음
                lines); // 대사 목록
        } // 생성 끝

        public static readonly StoryScene[] All =
        {
            // 프롤로그 · 스토리 1~4 --------------------------------------
            Day43Scene( // 계약 프롤로그 생성
                PrologueContractId, // 장면 ID
                "프롤로그 · 계약", // 장면 제목
                StoryEventType.SafeArea, // 첫 안전 구역 사건
                string.Empty, // 선행 장면 없음
                new[] { new StoryChoice("accept_contract", "계약한다") }, // 자발적 계약 선택
                N("깊은 밤, 닫혀 있던 방 안에 낯선 여자가 나타난다."), // 도입 서술
                L(Protagonist, "누구야. 어떻게 들어왔지?"), // 주인공 경계
                L(Riella, "리엘라. 네 정기를 조금 가져가려 했는데, 생각이 바뀌었어."), // 리엘라 소개
                L(Riella, "네 안에는 평범한 인간과 다른 정기가 잠들어 있어. 한 번 빼앗고 끝내기엔 아까울 만큼."), // 특별한 정기 설명
                L(Protagonist, "그래서 계약을 하자는 거야? 내가 얻는 건 뭔데?"), // 계약 조건 질문
                L(Riella, "사람의 의식을 흔들어 네 곁에 두는 힘. 최면이라고 부르면 이해하기 쉽겠네."), // 능력 보상 설명
                N("나는 위험을 알면서도 직접 확인한 힘에 대한 호기심을 버리지 못한다."), // 선택 동기
                L(Protagonist, "필요한 조건을 숨기지 않는다면, 내 선택으로 결정할게."), // 선택 조건 확인
                L(Riella, "그럼 선택해. 나와 계약할지, 여기서 끝낼지.")), // 계약 선택 요청

            Day43Scene( // 첫 임무 안내 생성
                FirstMissionBriefingId, // 장면 ID
                "스토리 1 · 첫 번째 계약 임무", // 장면 제목
                StoryEventType.FirstStageEnter, // 첫 장소 진입 사건
                PrologueContractId, // 계약 프롤로그 선행
                null, // 선택지 없음
                N("선택한 장소의 외곽에서 리엘라가 주변에 옅은 마법을 펼친다."), // 장소 도착
                L(Protagonist, "평범하게 지내는 사람들 앞에서 정말 이 힘을 써도 되는 건가."), // 첫 행동 망설임
                L(Riella, "사람을 조종하는 마법은 아니야. 이상한 일을 중요하게 받아들이지 못하도록 판단과 관심을 낮추는 보조 마법이지."), // 인지저하 설명
                L(Riella, "넓은 장소를 덮는 동안만 유지돼. 화면의 제한 시간이 끝나기 전에 목표를 마치고 빠져나와."), // 제한 시간 연결
                L(Riella, "감시자의 시선을 오래 끌면 경계도가 올라가. 마법이 모든 의심을 지워 주지는 않아."), // 경계도 연결
                L(Riella, "먼저 혼자 있는 성인 한 명을 골라. 시선을 맞추고 최면을 끝까지 유지해.")), // 첫 최면 지시

            Day43Scene( // 첫 최면 장면 생성
                FirstHypnosisId, // 장면 ID
                "스토리 1 · 첫 최면", // 장면 제목
                StoryEventType.HypnosisSucceeded, // 최면 성공 사건
                FirstMissionBriefingId, // 임무 안내 선행
                null, // 선택지 없음
                N("최면이 완성되자 대상의 시선이 주인공에게 고정된다."), // 성공 서술
                L(Protagonist, "정말 통했어."), // 성공 반응
                L(Riella, "성공은 시작일 뿐이야. 이제 동행 상태로 데리고 움직여."), // 동행 연결
                L(Riella, "최면이 풀리거나 다른 경쟁자에게 빼앗기기 전에 회수 지점까지 안전하게 데려가야 해.")), // 회수 목표 안내

            Day43Scene( // 첫 회수 장면 생성
                FirstRecoveryId, // 장면 ID
                "스토리 1 · 첫 회수", // 장면 제목
                StoryEventType.RecoveryConfirmed, // 회수 확정 사건
                FirstHypnosisId, // 첫 최면 선행
                null, // 선택지 없음
                N("회수 지점을 통과하자 동행자에게 이어져 있던 마력이 정기로 바뀐다."), // 회수 확정 서술
                L(Riella, "이제야 네 성과로 확정됐어. 최면, 동행, 회수까지가 한 흐름이야."), // 핵심 순환 정리
                L(Protagonist, "얻는 것보다 끝까지 지키는 일이 더 길군."), // 주인공 학습
                L(Riella, "맞아. 남은 시간과 경계도를 보면서 목표를 채우고 빠져나가.")), // 임무 마무리 안내

            Day43Scene( // 르미아 조우 장면 생성
                LumiaEncounterId, // 장면 ID
                "스토리 2 · 경쟁의 시작", // 장면 제목
                StoryEventType.LocationClear, // 첫 임무 클리어 사건
                FirstRecoveryId, // 첫 회수 선행
                null, // 선택지 없음
                N("첫 임무를 마치고 돌아가려는 골목에서 붉은 금빛 마력이 길을 막는다."), // 조우 서술
                L(Lumia, "네가 인간하고 계약을 맺는 날도 오는구나, 리엘라."), // 르미아 등장
                L(Riella, "르미아. 이 계약과 정기는 네 몫이 아니야."), // 리엘라 경계
                L(Lumia, "처음 데리고 다니는 인간 계약자라니, 흥미롭네. 다음에는 어느 쪽이 먼저 가져가는지 보자."), // 경쟁 선언
                L(Protagonist, "첫 임무부터 다른 서큐버스의 경쟁에 끼어든 셈인가."), // 상황 인식
                N("첫 계약 임무는 성공했지만, 도시의 정기와 사람을 둘러싼 경쟁이 시작된다.")), // 스토리 2 종료

            Day43Scene( // 경쟁 임무 안내 생성
                CompetitionBriefingId, // 장면 ID
                "스토리 3 · 이상한 경쟁자들", // 장면 제목
                StoryEventType.SafeArea, // 다음 안전 구역 사건
                LumiaEncounterId, // 르미아 조우 선행
                null, // 선택지 없음
                L(Protagonist, "르미아와는 언제부터 알던 사이야?"), // 과거 질문
                L(Riella, "오래전부터 경쟁했어. 지금은 다음 임무가 먼저야."), // 답변 회피
                L(Riella, "이번에는 사람을 확보한 뒤에도 뒤를 확인해. 이 도시에는 네 동행자를 노리는 성인 경쟁자도 있어."), // 쟁탈 예고
                N("주인공은 계약을 계속하면서 리엘라가 숨기는 과거도 직접 확인하기로 한다.")), // 임무 목적

            Day43Scene( // 동행자 탈취 장면 생성
                FollowerStolenId, // 장면 ID
                "스토리 3 · 빼앗으려는 사람", // 장면 제목
                StoryEventType.FollowerStolen, // 동행자 탈취 사건
                CompetitionBriefingId, // 경쟁 임무 안내 선행
                null, // 선택지 없음
                N("뒤따르던 성인 경쟁자가 동행자에게 접근하자 최면의 연결이 크게 흔들린다."), // 탈취 상황
                L(Riella, "회수하기 전까지 소유 상태는 확정된 게 아니야."), // 소유권 설명
                L(Riella, "사이에 들어가 압박을 끊어. 빼앗겼다면 다시 최면해서 되찾을 수 있어."), // 대응 방법
                L(Protagonist, "새 대상을 찾기 전에 이미 확보한 사람부터 지켜야겠군.")), // 학습 반응

            Day43Scene( // 동행자 재탈환 장면 생성
                FollowerReclaimedId, // 장면 ID
                "스토리 3 · 되찾는 방법", // 장면 제목
                StoryEventType.FollowerReclaimed, // 재탈환 사건
                FollowerStolenId, // 동행자 탈취 선행
                null, // 선택지 없음
                N("흔들리던 연결이 다시 주인공 쪽으로 돌아오며 동행 상태가 안정된다."), // 재탈환 성공
                L(Riella, "그래. 회수되지 않았다면 아직 끝난 게 아니야."), // 재탈환 확인
                L(Protagonist, "최면의 성공보다 유지와 회수가 더 긴 싸움이군."), // 경쟁 규칙 이해
                L(Riella, "이제 앞만 보지 말고 동행자의 소유 상태도 계속 확인해.")), // 관리 안내

            Day43Scene( // 경쟁 임무 정리 생성
                CompetitionAftermathId, // 장면 ID
                "스토리 3 · 익숙한 흔적", // 장면 제목
                StoryEventType.LocationClear, // 경쟁 임무 클리어 사건
                FollowerReclaimedId, // 재탈환 선행
                null, // 선택지 없음
                N("임무가 끝난 자리에서 르미아와 닮은 옅은 마력의 흔적이 발견된다."), // 흔적 발견
                L(Protagonist, "저 사람들은 조종당한 게 아니지만, 누군가 경쟁심을 건드린 흔적은 남아 있어."), // 주인공 추론
                L(Riella, "확정할 수 없는 흔적이야. 르미아의 부하라고 단정하지 마."), // 리엘라 선 긋기
                L(Protagonist, "그 마법을 바로 알아본 걸 보면 단순한 최근 경쟁자는 아니겠지.")), // 과거 의문

            Day43Scene( // 리엘라 질투 장면 생성
                RiellaJealousyId, // 장면 ID
                "스토리 4 · 질투하는 서큐버스", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                CompetitionAftermathId, // 경쟁 임무 정리 선행
                null, // 선택지 없음
                N("방으로 돌아온 뒤에도 리엘라는 르미아가 주인공에게 보인 관심을 계속 신경 쓴다."), // 관계 장면 도입
                L(Protagonist, "르미아가 나한테 관심을 보인 게 그렇게 거슬려?"), // 질투 지적
                L(Riella, "질투 아니거든. 내 계약자를 다른 서큐버스에게 빼앗기고 싶지 않을 뿐이야."), // 감정 노출
                L(Protagonist, "그 말이 질투와 얼마나 다른지는 모르겠네."), // 관계 반응
                N("두 사람은 아직 관계를 정의하지 않지만 서로를 계약 이상의 개인으로 의식하기 시작한다.")), // 관계 변화

            Day43Scene( // 다음 날 아침 장면 생성
                RiellaMorningAfterId, // 장면 ID
                "스토리 4 · 다음 날 아침", // 장면 제목
                StoryEventType.SafeArea, // 다음 지도 또는 허브 사건
                RiellaJealousyId, // 질투 장면 선행
                null, // 선택지 없음
                N("다음 날 아침, 리엘라는 전날의 일을 말하지 않은 채 지나치게 평소처럼 행동한다."), // 후일담 도입
                L(Protagonist, "어제 일은 그냥 넘어갈 생각이야?"), // 관계 확인
                L(Riella, "임무가 많아. 도시에서 네가 직접 확인할 것도 늘었고."), // 회피와 다음 목표
                L(Protagonist, "그럼 원하는 지역부터 돌아보지. 계약의 배경도 내가 직접 찾겠어."), // 자유 탐색 선언
                N("스토리 4가 끝나고 도시의 지역 에피소드가 자유 탐색 구조로 열린다.")), // 지역 개방

            Day44Scene( // 리엘라 H02 생성
                RiellaPromiseId, // 장면 ID
                "리엘라 H02 · 첫 번째 약속", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaMorningAfterId), // H01 완료
                    StoryCondition.RegionalStoriesCompleted(1) // 첫 지역 완료
                }, // 목록 끝
                N("첫 지역 임무를 마친 뒤 리엘라가 귀환 기록을 먼저 펼쳐 든다."), // 장면 도입
                L(Riella, "앞으로도 돌아오면 결과부터 함께 확인하자. 혼자 판단하고 숨기지 말고."), // 공동 확인 제안
                L(Protagonist, "계약 보고가 아니라 서로에게 하는 약속으로 받아들일게."), // 약속 수락
                L(Riella, "좋아. 그럼 다음 임무에서도 반드시 같이 돌아오는 거야.")), // 관계 약속

            Day44Scene( // 리엘라 H03 생성
                RiellaCheckTogetherId, // 장면 ID
                "리엘라 H03 · 함께 확인할 것", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaPromiseId), // H02 완료
                    StoryCondition.StoryCompleted(InterludeAId) // 인터루드 A 완료
                }, // 목록 끝
                N("르미아가 남긴 의문을 정리하던 리엘라가 숨겨 둔 기록 한 장을 꺼낸다."), // 장면 도입
                L(Riella, "확실하지 않아서 말하지 않았어. 하지만 이제는 네가 판단할 자료도 함께 볼게."), // 정보 공유
                L(Protagonist, "정답보다 같은 자료를 보고 결정하는 게 중요해."), // 신뢰 확인
                L(Riella, "그 말을 지킬게. 이번에는 정말 함께 확인하자.")), // 공동 조사 약속

            Day44Scene( // 리엘라 H04 생성
                RiellaSharedBurdenId, // 장면 ID
                "리엘라 H04 · 나눈 짐", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaCheckTogetherId), // H03 완료
                    StoryCondition.StoryCompleted(InterludeBId) // 인터루드 B 완료
                }, // 목록 끝
                N("과거 계약망의 기록을 읽은 뒤에도 리엘라는 한동안 페이지를 넘기지 못한다."), // 장면 도입
                L(Protagonist, "그때의 선택까지 혼자 책임질 필요는 없어. 지금의 결정은 같이 하자."), // 부담 분담
                L(Riella, "계약자에게 이런 말을 듣게 될 줄은 몰랐네."), // 감정 반응
                L(Riella, "그래도 이번에는 기대 볼게. 네가 먼저 놓지만 않는다면.")), // 신뢰 심화

            Day44Scene( // 리엘라 H05 생성
                RiellaSharedResponsibilityId, // 장면 ID
                "리엘라 H05 · 함께 질 책임", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaSharedBurdenId), // H04 완료
                    StoryCondition.StoryCompleted(InterludeCId) // 인터루드 C 완료
                }, // 목록 끝
                N("도시 전체의 이상을 확인한 두 사람은 결전 이후의 책임까지 목록에 적는다."), // 장면 도입
                L(Riella, "르미아를 막는 것만으로 끝나지 않아. 흐트러진 계약도 우리가 정리해야 해."), // 이후 책임
                L(Protagonist, "계약의 힘을 쓴 만큼 결과도 같이 감당하지."), // 책임 수락
                L(Riella, "이제 네 선택을 믿어. 나도 내 몫을 피하지 않을게.")), // 상호 신뢰

            Day44Scene( // 리엘라 H06 생성
                RiellaStayTogetherId, // 장면 ID
                "리엘라 H06 · 결전 뒤에도", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaSharedResponsibilityId), // H05 완료
                    StoryCondition.StoryCompleted(StoryTruthId) // 스토리 13 완료
                }, // 목록 끝
                N("결전 준비를 끝낸 밤, 리엘라가 계약서가 아닌 빈 종이를 내민다."), // 장면 도입
                L(Riella, "루프탑이 끝난 뒤에도 네 곁에 남는 건 계약 조건이 아니었으면 해."), // 관계 고백
                L(Protagonist, "그 답은 결전이 끝나도 바뀌지 않아. 함께 돌아오자."), // 동행 선택
                L(Riella, "응. 이번에는 계약이 아니라 내 선택으로 함께 갈게.")), // 최종 관계 확인

            // 메인 인터루드 · 결전 --------------------------------------
            Day44Scene( // 인터루드 A 생성
                InterludeAId, // 장면 ID
                "메인 인터루드 A · 르미아의 재등장", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(RiellaMorningAfterId), // 스토리 4 완료
                    StoryCondition.RegionalStoriesCompleted(2) // 일반 지역 두 곳 완료
                }, // 목록 끝
                N("두 지역의 임무를 마친 귀환 동선에 붉은 금빛 마력이 길을 막는다."), // 르미아 재등장
                L(Lumia, "리엘라가 왜 그렇게 많은 정기를 필요로 하는지 알고 있어?"), // 계약 의문 제시
                L(Protagonist, "답을 알고 있다면 네가 말해."), // 주인공 반문
                L(Lumia, "남이 준 답보다 직접 빼앗아 확인한 답이 오래 남는 법이야."), // 경쟁 예고
                L(Riella, "저 말에 휘둘리지 마. 아직 확인되지 않은 이야기야.")), // 리엘라 경계

            Day44Scene( // 르미아 쟁탈 장면 생성
                LumiaContestId, // 장면 ID
                "르미아 개입 · 붉은 금빛 손길", // 장면 제목
                StoryEventType.FollowerStolen, // 동행자 탈취 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                true, // 모든 장소 일치
                new[] { StoryCondition.StoryCompleted(InterludeAId) }, // 인터루드 A 선행
                N("경쟁자의 압박 사이로 붉은 금빛 마력이 끼어들어 동행자의 연결을 흔든다."), // 개입 서술
                L(Lumia, "회수하기 전까지는 누구의 성과도 아니잖아."), // 르미아 도발
                L(Riella, "흔적을 놓치지 마. 되찾으면 저 마력의 방향도 읽을 수 있어.")), // 재탈환 지시

            Day44Scene( // 르미아 재탈환 장면 생성
                LumiaReclaimId, // 장면 ID
                "르미아 개입 · 되찾은 연결", // 장면 제목
                StoryEventType.FollowerReclaimed, // 재탈환 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                true, // 모든 장소 일치
                new[] { StoryCondition.StoryCompleted(LumiaContestId) }, // 쟁탈 장면 선행
                N("되찾은 연결에서 붉은 금빛 마력의 잔향이 도시 위쪽으로 이어진다."), // 흔적 확인
                L(Protagonist, "르미아는 사람을 빼앗으려던 게 아니라 반응을 시험한 거야."), // 개입 목적 추론
                L(Riella, "그리고 우리가 어디까지 알아냈는지도 확인했겠지.")), // 리엘라 판단

            Day44Scene( // 인터루드 B 생성
                InterludeBId, // 장면 ID
                "메인 인터루드 B · 숨겨진 과거", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(InterludeAId), // 인터루드 A 완료
                    StoryCondition.RegionalStoriesCompleted(4) // 일반 지역 네 곳 완료
                }, // 목록 끝
                N("네 지역에서 모은 흔적을 펼쳐 놓자 리엘라가 더는 시선을 피하지 않는다."), // 추궁 도입
                L(Protagonist, "이 마력들이 같은 계약 체계에서 나온 거라면 네가 모를 리 없어."), // 증거 제시
                L(Riella, "나와 르미아는 과거 같은 계약 체계 안에서 움직였어."), // 과거 인정
                L(Riella, "하지만 사고의 원인과 그때의 선택은 아직 단정할 수 없어."), // 미확정 진실
                N("숨겨진 과거의 일부가 드러나고 루프탑으로 이어지는 경로가 확인된다.")), // 루프탑 단서

            Day44Scene( // 인터루드 C 생성
                InterludeCId, // 장면 ID
                "메인 인터루드 C · 도시 전체의 이상", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(InterludeBId), // 인터루드 B 완료
                    StoryCondition.RegionalStoriesCompleted(6) // 일반 지역 여섯 곳 완료
                }, // 목록 끝
                N("서로 떨어진 지역의 붉은 금빛 마력이 동시에 반응해 한 방향으로 흐른다."), // 도시 이상
                L(Protagonist, "개별 경쟁이 아니야. 도시 전체의 정기 흐름을 한곳으로 모으고 있어."), // 세력 확장 확인
                L(Riella, "남은 지역까지 확인한 뒤 모든 증거를 시간 순서로 맞춰 보자.")), // 최종 조사 지시

            Day44Scene( // 두 개의 진실 생성
                StoryTruthId, // 장면 ID
                "스토리 13 · 두 개의 진실", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] // 해금 조건 목록
                { // 목록 시작
                    StoryCondition.StoryCompleted(InterludeCId), // 인터루드 C 완료
                    StoryCondition.RegionalStoriesCompleted(7) // 일반 지역 일곱 곳 완료
                }, // 목록 끝
                N("일곱 지역의 기록과 마력 잔향을 사건 발생 순서로 다시 배열한다."), // 증거 정리
                L(Riella, "과거 계약망은 사고 뒤 무너졌고 기록과 권한은 도시 곳곳으로 흩어졌어."), // 계약망 진실
                L(Protagonist, "르미아는 흩어진 연결을 다시 묶고 있고, 내 정기는 그 망과 비정상적으로 공명한다."), // 현재 진실
                L(Riella, "이번에는 숨기지 않을게. 끝까지 함께 확인하자.")), // 공동 결심

            Day44Scene( // 결전 전야 생성
                BattleEveId, // 장면 ID
                "스토리 13.4 · 결전 전야", // 장면 제목
                StoryEventType.HubReturn, // 허브 복귀 사건
                LocationId.TrainingCenter, // 공용 기준 장소
                false, // 지정 사건만 일치
                new[] { StoryCondition.StoryCompleted(StoryTruthId) }, // 두 개의 진실 선행
                N("도시의 모든 붉은 금빛 흐름이 루프탑 클럽으로 모인다."), // 결전 위치 확정
                L(Protagonist, "르미아가 연결을 완성하기 전에 루프탑에서 끝낸다."), // 출격 결정
                L(Riella, "혼자 떠안지 마. 이번 계약은 우리 둘의 선택이니까."), // 관계 확인
                N("루프탑 클럽의 최종 결전이 지도에서 열린다.")), // 루프탑 해금

            Day44Scene( // 루프탑 결전 개막 생성
                BossOpeningId, // 장면 ID
                "스토리 14 · 루프탑의 밤", // 장면 제목
                StoryEventType.LocationEnter, // 루프탑 진입 사건
                LocationId.RooftopClub, // 루프탑 전용
                false, // 지정 장소 일치
                new[] { StoryCondition.StoryCompleted(BattleEveId) }, // 결전 전야 선행
                N("루프탑의 군중과 정기가 붉은 금빛 계약망을 따라 르미아에게 모인다."), // 보스전 개막
                L(Lumia, "도시의 밤을 두고 마지막으로 경쟁해 볼까?"), // 르미아 도전
                L(Riella, "군중을 되찾고 보호막을 무너뜨린 뒤 본체의 연결을 끊어."), // 전투 단계 안내
                L(Protagonist, "이번에는 사람도, 계약도 네 방식대로 묶게 두지 않아.")), // 주인공 선언

            Day44Scene( // 르미아 마지막 대화 생성
                BossVictoryId, // 장면 ID
                "스토리 14.2.5 · 르미아의 마지막 대화", // 장면 제목
                StoryEventType.BossVictory, // 보스 승리 사건
                LocationId.RooftopClub, // 루프탑 전용
                false, // 지정 장소 일치
                new[] { StoryCondition.StoryCompleted(BossOpeningId) }, // 보스 개막 선행
                N("붉은 금빛 계약망이 끊어지고 루프탑의 정기 흐름이 제자리로 돌아간다."), // 승리 결과
                L(Lumia, "졌네. 적어도 네 선택이 리엘라의 명령만은 아니라는 건 알겠어."), // 패배 인정
                L(Protagonist, "도시의 계약 흐름에서 물러나. 다음 선택은 그 뒤에 들어 줄게."), // 퇴각 요구
                L(Lumia, "약속할게. 오늘 밤은 너희가 이겼어.")), // 르미아 퇴각

            // 고등학교 ---------------------------------------------------
            new StoryScene("enter_training", "야간 수업", StoryTrigger.LocationEnter, LocationId.TrainingCenter, 0,
                N("성인 교육생의 야간 수업이 끝나 간다. 복도에는 피곤한 얼굴들이 가득하다."),
                L(Me, "지친 사람일수록 마음이 쉽게 열리지. 여기서부터 시작하자."),
                L("교사", "거기! 수업 중에 복도에서 뭐 하는 거죠?")),

            new StoryScene("clear_training", "첫 계약", StoryTrigger.LocationClear, LocationId.TrainingCenter, 0,
                L(Me, "첫 정기가 몸에 스며든다… 이 정도면 버틸 수 있어."),
                N("창밖으로 도시의 불빛이 보인다. 저 너머에 더 많은 사람들이 있다.")),

            // 해변가 -----------------------------------------------------
            new StoryScene("enter_beach", "뜨거운 백사장", StoryTrigger.LocationEnter, LocationId.Beach, 0,
                N("한낮의 백사장. 숨을 곳은 파라솔 그늘뿐이다."),
                L("라이프가드", "물때 바뀝니다! 물가에서 떨어지세요!"),
                L(Me, "한 명씩은 비효율적이야. 여럿을 한 번에 데려가자.")),

            new StoryScene("clear_beach", "파도 소리", StoryTrigger.LocationClear, LocationId.Beach, 0,
                L("헌팅남", "저기요, 아까 그 사람들 다 어디 갔어요?"),
                L(Me, "글쎄? 파도에 휩쓸려 간 거 아닐까.")),

            // 지하철 환승역 ------------------------------------------------
            new StoryScene("enter_subway", "퇴근길 인파", StoryTrigger.LocationEnter, LocationId.SubwayStation, 0,
                N("열차가 설 때마다 사람들이 쏟아져 나온다."),
                L("안내 방송", "승강장이 변경되었습니다. 승객 여러분께서는…"),
                L(Me, "이 물결 속에서 내 사람들을 놓치면 안 돼.")),

            new StoryScene("clear_subway", "막차", StoryTrigger.LocationClear, LocationId.SubwayStation, 0,
                L(Me, "인파는 파도 같아. 버티는 법만 알면 무섭지 않아."),
                N("막차가 떠나고, 역에 정적이 내려앉았다.")),

            // 헬스장 -----------------------------------------------------
            new StoryScene("enter_fitness", "뜨거운 심장", StoryTrigger.LocationEnter, LocationId.FitnessCenter, 0,
                N("쇠 부딪히는 소리와 거친 숨소리."),
                L("관장", "자, 전원 단체 PT 들어갑니다! 하나, 둘!"),
                L(Me, "심장이 빨리 뛰는 사람은 최면도 빨리 걸려. 대신 다루기 어렵겠지.")),

            new StoryScene("clear_fitness", "대회 전날", StoryTrigger.LocationClear, LocationId.FitnessCenter, 0,
                L("퍼스널 트레이너", "회원님들이 오늘따라 왜 이렇게 멍하시지…?"),
                L(Me, "운동보다 좋은 휴식을 줬을 뿐이야.")),

            // 야시장 -----------------------------------------------------
            new StoryScene("enter_market", "등불 골목", StoryTrigger.LocationEnter, LocationId.NightMarket, 0,
                N("좁은 골목에 등불이 흔들린다. 어둠 속에는 낯선 손길도 섞여 있다."),
                L("호객꾼", "언니, 이거 한번 드셔 보고 가세요!"),
                L(Me, "등불 밖은 눈이 흐려져. 빛 아래에서 움직이자.")),

            new StoryScene("clear_market", "빈 주머니", StoryTrigger.LocationClear, LocationId.NightMarket, 0,
                L("소매치기", "쳇, 오늘은 내가 털린 기분이네."),
                L(Me, "훔치는 건 네 일이 아니라 내 일이야.")),

            // 쇼핑몰 -----------------------------------------------------
            new StoryScene("enter_mall", "폐점 30분 전", StoryTrigger.LocationEnter, LocationId.ShoppingMall, 0,
                N("반짝이는 진열대, 천장마다 달린 카메라."),
                L("보안요원", "3층 이상 없음. 계속 순찰한다."),
                L(Me, "들키지 않고 해내면 더 많이 얻을 수 있어. 기둥 뒤로.")),

            new StoryScene("clear_mall", "꺼진 화면", StoryTrigger.LocationClear, LocationId.ShoppingMall, 0,
                L("보안팀장", "CCTV에 아무것도 안 찍혔다고? 그럼 손님들은 어디로…"),
                L(Me, "카메라는 마음까지는 못 보니까.")),

            // 오피스 타워 -------------------------------------------------
            new StoryScene("enter_office", "야근의 탑", StoryTrigger.LocationEnter, LocationId.OfficeTower, 0,
                N("밤 열한 시. 꺼지지 않는 층들."),
                L("꼰대 부장", "다들 집에 갈 생각 하지 마. 보고서 끝날 때까지!"),
                L(Me, "위층으로 가려면 출입증이 필요해. 직원 한 명을 데려가야겠어.")),

            new StoryScene("clear_office", "정시 퇴근", StoryTrigger.LocationClear, LocationId.OfficeTower, 0,
                L("비서실장", "대표님 비서가… 퇴근을 했다고요? 이 시간에?"),
                L(Me, "가끔은 쉬어야지. 누군가 대신 일해 주면 되니까.")),

            // 루프탑 클럽 -------------------------------------------------
            new StoryScene("enter_club", "라이벌", StoryTrigger.LocationEnter, LocationId.RooftopClub, 0,
                N("도시 꼭대기. 음악이 심장처럼 울린다."),
                L(Rival, "드디어 왔네, 꼬마 서큐버스. 이 도시의 밤은 내 거야."),
                L(Me, "그 밤, 오늘 내가 가져갈게."),
                L(Rival, "그럼 춤으로 증명해 봐. 내 손님들을 빼앗을 수 있다면.")),

            new StoryScene("clear_club", "음악이 멈춘 밤", StoryTrigger.LocationClear, LocationId.RooftopClub, 0,
                L(Rival, "…나보다 빛나는 밤은 처음이야."),
                L(Me, "이제부터 이 도시는 내가 지킬게.")),

            // 라이벌 도발 --------------------------------------------------
            new StoryScene("rival_taunt_3", "지켜보는 눈", StoryTrigger.LocationsCleared, LocationId.TrainingCenter, 3,
                N("휴대폰에 낯선 메시지가 도착했다."),
                L(Rival, "제법이네. 학교, 바닷가… 귀여운 동네 산책은 즐거웠어?"),
                L(Me, "…누구지? 이 기척, 같은 서큐버스야.")),

            new StoryScene("rival_taunt_6", "초대장", StoryTrigger.LocationsCleared, LocationId.TrainingCenter, 6,
                N("창틀에 보라색 초대장이 꽂혀 있다. '루프탑 클럽, 오늘 밤.'"),
                L(Rival, "도시의 반을 가져갔다고 우쭐하지 마. 꼭대기는 아직 내 거니까."),
                L(Me, "기다려. 곧 올라갈게.")),

            // 후일담 ------------------------------------------------------
            new StoryScene("epilogue_ending", "후일담 · 새벽", StoryTrigger.Endings, LocationId.TrainingCenter, 1,
                N("루프탑의 음악이 멈추고, 도시에 새벽이 왔다."),
                L(Rival, "착각하지 마. 난 사라지지 않아. 밤마다 다시 올라올 거야."),
                L(Me, "그럼 밤마다 다시 이기면 되지."),
                N("더 깊은 밤, 심야의 도시가 열렸다.")),

            new StoryScene("epilogue_dominion", "후일담 · 도시의 주인", StoryTrigger.Dominion, LocationId.TrainingCenter, 100,
                N("어느 골목을 걸어도, 사람들은 그녀의 이름을 속삭인다."),
                L(Rival, "…인정할게. 이 도시의 밤은 네 거야."),
                L(Me, "그래도 가끔은 같이 춤춰 줄게."),
                N("학생의 작은 방 창문 너머로, 도시의 불빛이 온통 보랏빛이었다."))
        };

        private static Dictionary<string, StoryScene> _byId;

        public static StoryScene Get(
            string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, StoryScene>();

                foreach (StoryScene scene in All)
                {
                    _byId[scene.Id] = scene;
                }
            }

            return id != null &&
                   _byId.TryGetValue(id, out StoryScene found)
                ? found
                : null;
        }
    }
}
