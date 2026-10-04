using System;
using System.Collections.Generic;
using ProjectTheta.Save;
using ProjectTheta.Story;

namespace ProjectTheta.UI
{
    public enum LobbyDialogueTrigger // 로비 대사 발생 조건
    { // 열거 시작
        Enter = 0, // 허브 입장
        Idle = 1, // 허브 방치
        Depart = 2, // 출격 직전
        Return = 3, // 스테이지 귀환
        Upgrade = 4 // 영구 성장 직후
    } // 열거 끝

    public enum LobbyDialogueCategory // 로비 대사 분류
    { // 열거 시작
        Enter = 0, // 허브 입장
        Idle = 1, // 일반 방치
        Depart = 2, // 출격 전
        ClearS = 3, // S등급 귀환
        ClearA = 4, // A등급 귀환
        ClearB = 5, // B등급 귀환
        ClearC = 6, // C등급 귀환
        Fail = 7, // 실패 귀환
        Essence = 8, // 성장 가능
        Upgrade = 9, // 성장 구매
        FollowerMany = 10, // 다수 동행
        Rampage = 11, // 폭주 경험
        FollowerHint = 12, // 동행 힌트
        Progress = 13, // 도시 진행
        Mastery = 14, // 장소 숙련
        City = 15, // 도시 지배
        Night = 16, // 심야 모드
        Rival = 17, // 라이벌 등장
        RivalStolen = 18, // 라이벌 탈취
        Relation = 19, // 관계 심화
        Rare = 20 // 희귀 대사
    } // 열거 끝

    public sealed class LobbyDialogueDefinition // 로비 대사 자료
    { // 클래스 시작
        public readonly string Id; // 대사 ID
        public readonly LobbyDialogueCategory Category; // 대사 분류
        public readonly string Text; // 대사 본문

        public LobbyDialogueDefinition( // 대사 자료 생성
            string id, // 대사 ID
            LobbyDialogueCategory category, // 대사 분류
            string text) // 대사 본문
        { // 생성 시작
            Id = id; // ID 저장
            Category = category; // 분류 저장
            Text = text; // 본문 저장
        } // 생성 끝
    } // 클래스 끝

    public static class LobbyDialogueCatalog // 로비 대사 목록
    { // 클래스 시작
        public static readonly LobbyDialogueDefinition[] All = // 전체 대사
        { // 목록 시작
            Line("LOBBY_ENTER_01", LobbyDialogueCategory.Enter, "왔네. 오늘은 어디까지 해볼 생각이야?"), // 입장 1
            Line("LOBBY_ENTER_02", LobbyDialogueCategory.Enter, "생각보다 빨리 돌아왔네. 벌써 내가 보고 싶었던 건 아니지?"), // 입장 2
            Line("LOBBY_ENTER_03", LobbyDialogueCategory.Enter, "어서 와. 도시 쪽은 여전히 시끄럽더라."), // 입장 3
            Line("LOBBY_ENTER_04", LobbyDialogueCategory.Enter, "준비됐으면 지도부터 봐. 가만히 있는다고 계약이 진행되진 않으니까."), // 입장 4
            Line("LOBBY_ENTER_05", LobbyDialogueCategory.Enter, "오늘도 계약대로 움직여 줘. 성과는 내가 확실히 계산해 줄 테니까."), // 입장 5
            Line("LOBBY_IDLE_01", LobbyDialogueCategory.Idle, "계속 그렇게 보고 있을 거야?"), // 방치 1
            Line("LOBBY_IDLE_02", LobbyDialogueCategory.Idle, "할 일이 없으면 지도라도 확인해 봐."), // 방치 2
            Line("LOBBY_IDLE_03", LobbyDialogueCategory.Idle, "인간은 참 신기해. 바쁠 땐 쉬고 싶어 하고, 쉴 수 있을 땐 아무것도 안 하네."), // 방치 3
            Line("LOBBY_IDLE_04", LobbyDialogueCategory.Idle, "설마 내가 먼저 말을 걸어주길 기다리고 있었어?"), // 방치 4
            Line("LOBBY_IDLE_05", LobbyDialogueCategory.Idle, "계약자는 밖에서 일하고, 서큐버스는 여기서 기다린다. 역할 분담이 꽤 완벽하지?"), // 방치 5
            Line("LOBBY_DEPART_01", LobbyDialogueCategory.Depart, "욕심내는 건 좋은데, 회수하기 전까진 확정된 게 아니야."), // 출격 1
            Line("LOBBY_DEPART_02", LobbyDialogueCategory.Depart, "사람을 모으는 것보다 끝까지 데리고 오는 게 더 어려울 거야."), // 출격 2
            Line("LOBBY_DEPART_03", LobbyDialogueCategory.Depart, "너무 많이 데리고 다니면 관리하기 힘들어진다는 것 정도는 기억하고 있지?"), // 출격 3
            Line("LOBBY_DEPART_04", LobbyDialogueCategory.Depart, "경계가 올라가기 시작하면 미련 부리지 마. 물러나는 것도 실력이야."), // 출격 4
            Line("LOBBY_DEPART_05", LobbyDialogueCategory.Depart, "다녀와. 오늘은 얼마나 가져올지 기대하고 있을게."), // 출격 5
            Line("LOBBY_CLEAR_S_01", LobbyDialogueCategory.ClearS, "완벽하네. 그 정도면 내 계약자라고 자랑해도 되겠어."), // S 귀환 1
            Line("LOBBY_CLEAR_S_02", LobbyDialogueCategory.ClearS, "깔끔했어. 다음에도 그렇게 해준다면 내가 할 일이 줄겠는데?"), // S 귀환 2
            Line("LOBBY_CLEAR_A_01", LobbyDialogueCategory.ClearA, "잘했어. 조금 아쉬운 부분은 있지만 충분히 만족스러워."), // A 귀환
            Line("LOBBY_CLEAR_B_01", LobbyDialogueCategory.ClearB, "무난하네. 하지만 네 실력이 이 정도에서 끝은 아니잖아?"), // B 귀환
            Line("LOBBY_CLEAR_C_01", LobbyDialogueCategory.ClearC, "일단 돌아오긴 했네. 다음엔 조금 덜 엉망으로 해보자."), // C 귀환
            Line("LOBBY_FAIL_01", LobbyDialogueCategory.Fail, "실패했다고 계약이 끝나는 건 아니야. 다시 가면 돼."), // 실패 1
            Line("LOBBY_FAIL_02", LobbyDialogueCategory.Fail, "욕심을 너무 부렸지? 표정만 봐도 알겠는데."), // 실패 2
            Line("LOBBY_FAIL_03", LobbyDialogueCategory.Fail, "정기를 잃은 건 아깝지만, 같은 실수만 반복하지 않으면 돼."), // 실패 3
            Line("LOBBY_FAIL_04", LobbyDialogueCategory.Fail, "이번엔 네가 졌어. 다음에도 질지는 네가 결정하는 거고."), // 실패 4
            Line("LOBBY_FAIL_05", LobbyDialogueCategory.Fail, "그래도 살아서 돌아왔잖아. 다시 준비해."), // 실패 5
            Line("LOBBY_ESSENCE_01", LobbyDialogueCategory.Essence, "계약 정기가 꽤 모였네. 쌓아두기만 하면 아무 의미 없어."), // 정기 1
            Line("LOBBY_ESSENCE_02", LobbyDialogueCategory.Essence, "힘이 필요하다면 계약 정기를 써. 공짜로 강해질 방법은 없으니까."), // 정기 2
            Line("LOBBY_UPGRADE_01", LobbyDialogueCategory.Upgrade, "좋아. 전보다 조금 더 쓸 만해졌네."), // 성장 1
            Line("LOBBY_UPGRADE_02", LobbyDialogueCategory.Upgrade, "강해지는 건 좋은데, 힘만 믿고 무리하진 마."), // 성장 2
            Line("LOBBY_UPGRADE_03", LobbyDialogueCategory.Upgrade, "최면만 강해진다고 모든 문제가 해결되진 않아. 네 뒤에 있는 사람들도 관리해야 하니까."), // 성장 3
            Line("LOBBY_FOLLOWER_01", LobbyDialogueCategory.FollowerMany, "사람이 많아질수록 강해진 것 같지? 사실 그때부터 관리가 시작되는 거야."), // 다수 동행 1
            Line("LOBBY_FOLLOWER_02", LobbyDialogueCategory.Rampage, "폭주는 갑자기 일어나는 게 아니야. 경고를 계속 무시한 결과지."), // 폭주
            Line("LOBBY_FOLLOWER_03", LobbyDialogueCategory.FollowerHint, "한 명 더 데려갈지, 지금 돌아올지. 그런 판단이 결국 차이를 만들 거야."), // 동행 힌트
            Line("LOBBY_FOLLOWER_04", LobbyDialogueCategory.FollowerMany, "욕심내는 건 싫어하지 않아. 감당할 수만 있다면."), // 다수 동행 2
            Line("LOBBY_PROGRESS_01", LobbyDialogueCategory.Progress, "도시가 슬슬 네 존재를 기억하기 시작했어."), // 도시 진행 1
            Line("LOBBY_PROGRESS_02", LobbyDialogueCategory.Progress, "처음엔 낯선 장소였는데, 이제 꽤 익숙해 보이네."), // 도시 진행 2
            Line("LOBBY_MASTERY_01", LobbyDialogueCategory.Mastery, "그 장소는 이제 네가 어디로 움직일지 먼저 알고 있겠네."), // 숙련 1
            Line("LOBBY_MASTERY_02", LobbyDialogueCategory.Mastery, "별 셋이라… 이제 거기선 초보 행세도 못 하겠어."), // 숙련 2
            Line("LOBBY_CITY_01", LobbyDialogueCategory.City, "도시 곳곳에 네 흔적이 남고 있어. 생각보다 빠른데?"), // 도시 지배
            Line("LOBBY_NIGHT_01", LobbyDialogueCategory.Night, "밤이 깊어지면 도시도 조금 더 솔직해지지."), // 심야 1
            Line("LOBBY_NIGHT_02", LobbyDialogueCategory.Night, "심야의 도시는 낮과 같은 장소라고 생각하지 않는 편이 좋아."), // 심야 2
            Line("LOBBY_NIGHT_03", LobbyDialogueCategory.Night, "어둠은 널 숨겨주기도 하지만, 다른 것들도 숨겨주거든."), // 심야 3
            Line("LOBBY_NIGHT_04", LobbyDialogueCategory.Night, "밤에는 나쁜 선택이 조금 더 매력적으로 보이지. 조심해."), // 심야 4
            Line("LOBBY_RIVAL_01", LobbyDialogueCategory.Rival, "그 애가 움직이기 시작했네."), // 라이벌 1
            Line("LOBBY_RIVAL_02", LobbyDialogueCategory.Rival, "내 경쟁자라고 해서 가볍게 보진 마. 오랫동안 살아남은 데엔 이유가 있으니까."), // 라이벌 2
            Line("LOBBY_RIVAL_03", LobbyDialogueCategory.RivalStolen, "네 사람을 빼앗겼어? 그럼 다시 가져와. 간단하잖아."), // 라이벌 탈취
            Line("LOBBY_RIVAL_04", LobbyDialogueCategory.Rival, "그 애는 네가 가진 걸 탐낼 거야. 원래 남의 것을 빼앗는 걸 좋아하거든."), // 라이벌 4
            Line("LOBBY_RIVAL_05", LobbyDialogueCategory.Rival, "흥미롭네. 이제 이 계약이 너와 나만의 문제는 아니게 됐어."), // 라이벌 5
            Line("LOBBY_RELATION_01", LobbyDialogueCategory.Relation, "처음보다 표정이 많이 편해졌네. 이제 내가 익숙해진 거야?"), // 관계 1
            Line("LOBBY_RELATION_02", LobbyDialogueCategory.Relation, "처음에는 계약 때문에 함께 있었지만… 지금은 꼭 그것뿐인지는 모르겠네."), // 관계 2
            Line("LOBBY_RELATION_03", LobbyDialogueCategory.Relation, "나를 믿어도 되냐고? 그건 네가 직접 판단해야지."), // 관계 3
            Line("LOBBY_RELATION_04", LobbyDialogueCategory.Relation, "인간하고 이렇게 오래 지내게 될 줄은 나도 몰랐어."), // 관계 4
            Line("LOBBY_RELATION_05", LobbyDialogueCategory.Relation, "계약이 끝난 뒤에도 네가 이 방으로 돌아올지, 조금 궁금해졌어."), // 관계 5
            Line("LOBBY_RARE_01", LobbyDialogueCategory.Rare, "왜 그렇게 쳐다봐? 서큐버스가 네 방에 있는 게 이제 와서 이상해?"), // 희귀 1
            Line("LOBBY_RARE_02", LobbyDialogueCategory.Rare, "계약서에 내가 집안일까지 한다는 내용은 없었어."), // 희귀 2
            Line("LOBBY_RARE_03", LobbyDialogueCategory.Rare, "오늘은 쉬어도 된다고 말해줄 줄 알았어? 유감이네."), // 희귀 3
            Line("LOBBY_RARE_04", LobbyDialogueCategory.Rare, "가끔은 네가 계약자인지 내가 계약자인지 헷갈릴 때가 있어."), // 희귀 4
            Line("LOBBY_RARE_05", LobbyDialogueCategory.Rare, "……뭐야. 아무 말도 안 했어. 빨리 준비나 해.") // 희귀 5
        }; // 목록 끝

        public static LobbyDialogueDefinition Find( // ID 대사 조회
            string id) // 대사 ID
        { // 조회 시작
            for (int i = 0; i < All.Length; i++) // 전체 대사 순회
            { // 순회 시작
                if (string.Equals(All[i].Id, id, StringComparison.Ordinal)) // ID 일치 확인
                { // 일치 시작
                    return All[i]; // 대사 반환
                } // 일치 끝
            } // 순회 끝

            return null; // 대사 없음 반환
        } // 조회 끝

        private static LobbyDialogueDefinition Line( // 대사 자료 생성
            string id, // 대사 ID
            LobbyDialogueCategory category, // 대사 분류
            string text) // 대사 본문
        { // 생성 시작
            return new LobbyDialogueDefinition(id, category, text); // 대사 자료 반환
        } // 생성 끝
    } // 클래스 끝

    public static class LobbyDialogueLogic // 로비 대사 선택 규칙
    { // 클래스 시작
        public const float RareChance = 0.05f; // 희귀 대사 확률
        public const float IdleSeconds = 25f; // 방치 대기 시간
        public const int ManyFollowerCount = 4; // 다수 동행 기준
        public const float DiaryRowStartY = 132f; // 일기장 첫 행 위치
        public const float DiaryRowHeight = 40f; // 일기장 행 높이
        public const float DiaryRowGap = 4f; // 일기장 행 간격
        public const float DiaryPageButtonY = 602f; // 페이지 버튼 위치

        public static bool CanReplaceSpeech( // 말풍선 교체 가능 여부
            bool isSpeaking, // 현재 재생 여부
            bool hasCompletion) // 중요 완료 알림 여부
        { // 확인 시작
            return !isSpeaking || !hasCompletion; // 중요 재생 보호 반환
        } // 확인 끝

        public static float GetDiaryRowY( // 일기장 행 위치 계산
            int row) // 행 번호
        { // 계산 시작
            return DiaryRowStartY + Math.Max(0, row) * (DiaryRowHeight + DiaryRowGap); // 행 위치 반환
        } // 계산 끝

        public static LobbyDialogueDefinition Select( // 조건별 대사 선택
            LobbyDialogueTrigger trigger, // 발생 조건
            SaveData save, // 저장 자료
            StageResultSummary result, // 직전 결과
            Random random, // 난수 도구
            float rareRoll) // 희귀 확률 값
        { // 선택 시작
            Random picker = random ?? new Random(); // 안전 난수 도구
            LobbyDialogueCategory category = ResolveCategory(trigger, save, result, rareRoll); // 우선 분류 결정
            LobbyDialogueDefinition exact = ResolveExactDialogue(category, save, result); // 세부 조건 대사 조회
            string lastId = save == null ? string.Empty : save.LastLobbyDialogueId; // 직전 대사 ID
            List<LobbyDialogueDefinition> candidates = exact == null // 세부 대사 확인
                ? GetByCategory(category) // 분류 후보 목록
                : new List<LobbyDialogueDefinition> { exact }; // 세부 후보 한 개
            LobbyDialogueDefinition selected = Pick(candidates, picker, lastId); // 우선 후보 선택

            if (selected != null) // 우선 후보 확인
            { // 우선 후보 시작
                return selected; // 선택 대사 반환
            } // 우선 후보 끝

            LobbyDialogueCategory fallback = trigger == LobbyDialogueTrigger.Return && category != ResolveResultCategory(result) // 귀환 특수 대사 확인
                ? ResolveResultCategory(result) // 등급 대사 대체
                : trigger == LobbyDialogueTrigger.Idle // 방치 대사 확인
                    ? LobbyDialogueCategory.Idle // 일반 방치 대체
                    : LobbyDialogueCategory.Enter; // 일반 입장 대체
            selected = Pick(GetByCategory(fallback), picker, lastId); // 하위 우선순위 선택

            return selected ?? candidates[0]; // 대체 대사 또는 불가피한 후보 반환
        } // 선택 끝

        public static LobbyDialogueCategory ResolveCategory( // 우선 분류 결정
            LobbyDialogueTrigger trigger, // 발생 조건
            SaveData save, // 저장 자료
            StageResultSummary result, // 직전 결과
            float rareRoll) // 희귀 확률 값
        { // 결정 시작
            if (trigger == LobbyDialogueTrigger.Upgrade) // 성장 직후 확인
            { // 성장 시작
                return LobbyDialogueCategory.Upgrade; // 성장 대사 반환
            } // 성장 끝

            if (trigger == LobbyDialogueTrigger.Depart) // 출격 직전 확인
            { // 출격 시작
                return LobbyDialogueCategory.Depart; // 출격 대사 반환
            } // 출격 끝

            if (trigger == LobbyDialogueTrigger.Return) // 귀환 확인
            { // 귀환 시작
                if (result.RampageWindups > 0) // 폭주 경험 확인
                { // 폭주 시작
                    return LobbyDialogueCategory.Rampage; // 폭주 대사 반환
                } // 폭주 끝

                if (result.StolenCount > 0) // 라이벌 탈취 확인
                { // 탈취 시작
                    return LobbyDialogueCategory.RivalStolen; // 탈취 대사 반환
                } // 탈취 끝

                if (DidIncreaseMastery(save, result)) // 숙련 상승 확인
                { // 숙련 시작
                    return LobbyDialogueCategory.Mastery; // 숙련 대사 반환
                } // 숙련 끝

                if (DidReachDominionTitle(save, result)) // 도시 지배 단계 상승 확인
                { // 지배 상승 시작
                    return LobbyDialogueCategory.City; // 도시 지배 대사 반환
                } // 지배 상승 끝

                if (DidRepeatLocationClear(save, result)) // 동일 장소 반복 확인
                { // 반복 시작
                    return LobbyDialogueCategory.Progress; // 진행 대사 반환
                } // 반복 끝

                if (result.MaxFollowers >= ManyFollowerCount) // 다수 동행 확인
                { // 다수 시작
                    return LobbyDialogueCategory.FollowerMany; // 동행 대사 반환
                } // 다수 끝

                if (result.NightMode) // 심야 도전 확인
                { // 심야 시작
                    return LobbyDialogueCategory.Night; // 심야 대사 반환
                } // 심야 끝

                if (result.MaxFollowers > 0) // 동행 경험 확인
                { // 동행 시작
                    return LobbyDialogueCategory.FollowerHint; // 동행 힌트 반환
                } // 동행 끝

                return ResolveResultCategory(result); // 결과 등급 대사 반환
            } // 귀환 끝

            if (trigger == LobbyDialogueTrigger.Idle && result.NightMode) // 심야 방치 확인
            { // 심야 방치 시작
                return LobbyDialogueCategory.Night; // 심야 대사 반환
            } // 심야 방치 끝

            if (IsRelationshipUnlocked(save)) // 관계 해금 확인
            { // 관계 시작
                return LobbyDialogueCategory.Relation; // 관계 대사 반환
            } // 관계 끝

            if (IsRivalUnlocked(save)) // 라이벌 해금 확인
            { // 라이벌 시작
                return LobbyDialogueCategory.Rival; // 라이벌 대사 반환
            } // 라이벌 끝

            if (HubSuccubusLogic.CanBuyAnyUpgrade(save)) // 성장 가능 확인
            { // 정기 시작
                return LobbyDialogueCategory.Essence; // 정기 대사 반환
            } // 정기 끝

            if (rareRoll >= 0f && rareRoll < RareChance) // 일반 상태 희귀 확률 확인
            { // 희귀 시작
                return LobbyDialogueCategory.Rare; // 희귀 대사 반환
            } // 희귀 끝

            return trigger == LobbyDialogueTrigger.Idle // 방치 조건 확인
                ? LobbyDialogueCategory.Idle // 방치 대사 반환
                : LobbyDialogueCategory.Enter; // 입장 대사 반환
        } // 결정 끝

        public static bool RecordSeen( // 대사 확인 기록
            SaveData save, // 저장 자료
            LobbyDialogueDefinition dialogue) // 선택 대사
        { // 기록 시작
            if (save == null || dialogue == null) // 입력 확인
            { // 입력 없음 시작
                return false; // 변경 없음 반환
            } // 입력 없음 끝

            save.LastLobbyDialogueId = dialogue.Id; // 직전 대사 저장
            string[] current = save.SeenLobbyDialogues ?? new string[0]; // 기존 목록 조회

            for (int i = 0; i < current.Length; i++) // 기존 목록 순회
            { // 순회 시작
                if (string.Equals(current[i], dialogue.Id, StringComparison.Ordinal)) // 중복 확인
                { // 중복 시작
                    return false; // 변경 없음 반환
                } // 중복 끝
            } // 순회 끝

            string[] grown = new string[current.Length + 1]; // 확장 배열 생성
            Array.Copy(current, grown, current.Length); // 기존 기록 복사
            grown[current.Length] = dialogue.Id; // 새 ID 추가
            save.SeenLobbyDialogues = grown; // 확장 목록 저장

            return true; // 변경 반환
        } // 기록 끝

        public static void Normalize( // 저장 자료 보정
            SaveData save) // 저장 자료
        { // 보정 시작
            if (save == null) // 저장 없음 확인
            { // 저장 없음 시작
                return; // 보정 종료
            } // 저장 없음 끝

            if (LobbyDialogueCatalog.Find(save.LastLobbyDialogueId) == null) // 직전 ID 검증
            { // 잘못된 ID 시작
                save.LastLobbyDialogueId = string.Empty; // 직전 ID 초기화
            } // 잘못된 ID 끝

            List<string> valid = new List<string>(); // 정상 ID 목록
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal); // 중복 검사 집합
            string[] source = save.SeenLobbyDialogues ?? new string[0]; // 원본 목록

            for (int i = 0; i < source.Length; i++) // 원본 순회
            { // 순회 시작
                string id = source[i]; // 현재 ID

                if (LobbyDialogueCatalog.Find(id) != null && seen.Add(id)) // 정상 고유 ID 확인
                { // 정상 시작
                    valid.Add(id); // 정상 ID 추가
                } // 정상 끝
            } // 순회 끝

            save.SeenLobbyDialogues = valid.ToArray(); // 보정 목록 저장
        } // 보정 끝

        public static bool IsSeen( // 대사 확인 여부
            SaveData save, // 저장 자료
            string id) // 대사 ID
        { // 확인 시작
            string[] seen = save == null ? null : save.SeenLobbyDialogues; // 확인 목록 조회

            if (seen == null) // 목록 없음 확인
            { // 목록 없음 시작
                return false; // 미확인 반환
            } // 목록 없음 끝

            for (int i = 0; i < seen.Length; i++) // 목록 순회
            { // 순회 시작
                if (string.Equals(seen[i], id, StringComparison.Ordinal)) // ID 일치 확인
                { // 일치 시작
                    return true; // 확인 반환
                } // 일치 끝
            } // 순회 끝

            return false; // 미확인 반환
        } // 확인 끝

        public static bool IsRelationshipUnlocked( // 관계 대사 해금 확인
            SaveData save) // 저장 자료
        { // 확인 시작
            return StoryProgressLogic.IsCompleted(save, StoryCatalog.RiellaSharedBurdenId); // H04 완료 반환
        } // 확인 끝

        public static bool IsRivalUnlocked( // 라이벌 대사 해금 확인
            SaveData save) // 저장 자료
        { // 확인 시작
            return StoryProgressLogic.IsCompleted(save, StoryCatalog.LumiaEncounterId); // 르미아 조우 완료 반환
        } // 확인 끝

        public static int GetPageCount( // 페이지 수 계산
            int itemCount, // 전체 항목 수
            int pageSize) // 페이지 크기
        { // 계산 시작
            return itemCount <= 0 || pageSize <= 0 // 유효 범위 확인
                ? 0 // 빈 페이지 반환
                : (itemCount + pageSize - 1) / pageSize; // 올림 나눗셈 반환
        } // 계산 끝

        public static int GetPageLength( // 페이지 항목 수 계산
            int itemCount, // 전체 항목 수
            int pageSize, // 페이지 크기
            int page) // 페이지 번호
        { // 계산 시작
            if (itemCount <= 0 || pageSize <= 0 || page < 0) // 유효 범위 확인
            { // 범위 밖 시작
                return 0; // 빈 페이지 반환
            } // 범위 밖 끝

            int start = page * pageSize; // 시작 위치 계산

            return start >= itemCount // 시작 범위 확인
                ? 0 // 빈 페이지 반환
                : Math.Min(pageSize, itemCount - start); // 남은 항목 수 반환
        } // 계산 끝

        private static LobbyDialogueCategory ResolveResultCategory( // 귀환 등급 분류
            StageResultSummary result) // 직전 결과
        { // 분류 시작
            if (!result.Cleared) // 실패 확인
            { // 실패 시작
                return LobbyDialogueCategory.Fail; // 실패 대사 반환
            } // 실패 끝

            switch ((result.RankLabel ?? string.Empty).Trim().ToUpperInvariant()) // 등급 분기
            { // 분기 시작
                case "S": // S등급
                    return LobbyDialogueCategory.ClearS; // S 대사 반환

                case "A": // A등급
                    return LobbyDialogueCategory.ClearA; // A 대사 반환

                case "B": // B등급
                    return LobbyDialogueCategory.ClearB; // B 대사 반환

                default: // 나머지 등급
                    return LobbyDialogueCategory.ClearC; // C 대사 반환
            } // 분기 끝
        } // 분류 끝

        private static bool DidIncreaseMastery( // 숙련 상승 확인
            SaveData save, // 저장 자료
            StageResultSummary result) // 직전 결과
        { // 확인 시작
            if (save == null || !result.Cleared || !result.HasLocation) // 유효 결과 확인
            { // 결과 없음 시작
                return false; // 상승 없음 반환
            } // 결과 없음 끝

            int clears = PlayStatsLogic.Get(save, result.LocationId).Clears; // 현재 클리어 수

            for (int i = 0; i < MasteryLogic.StarClears.Length; i++) // 별 기준 순회
            { // 순회 시작
                if (clears == MasteryLogic.StarClears[i]) // 기준 도달 확인
                { // 도달 시작
                    return true; // 상승 반환
                } // 도달 끝
            } // 순회 끝

            return false; // 상승 없음 반환
        } // 확인 끝

        private static bool DidRepeatLocationClear( // 동일 장소 반복 확인
            SaveData save, // 저장 자료
            StageResultSummary result) // 직전 결과
        { // 확인 시작
            return save != null && // 저장 존재 확인
                   result.Cleared && // 클리어 확인
                   result.HasLocation && // 장소 결과 확인
                   PlayStatsLogic.Get(save, result.LocationId).Clears == 2; // 두 번째 클리어 반환
        } // 확인 끝

        private static bool DidReachDominionTitle( // 도시 지배 단계 상승 확인
            SaveData save, // 저장 자료
            StageResultSummary result) // 직전 결과
        { // 확인 시작
            if (save == null || !result.Cleared || !result.HasLocation) // 유효 결과 확인
            { // 결과 없음 시작
                return false; // 상승 없음 반환
            } // 결과 없음 끝

            SaveData previous = save.Clone(); // 직전 상태 복원용 복제
            LocationStats record = PlayStatsLogic.Get(previous, result.LocationId); // 직전 장소 기록 조회
            record.Clears = Math.Max(0, record.Clears - 1); // 이번 클리어 제거

            if (result.NightMode) // 심야 클리어 확인
            { // 심야 시작
                record.NightClears = Math.Max(0, record.NightClears - 1); // 이번 심야 클리어 제거
            } // 심야 끝

            int before = DominionLogic.GetTitleIndex(DominionLogic.GetPercent(previous)); // 직전 지배 단계 계산
            int after = DominionLogic.GetTitleIndex(DominionLogic.GetPercent(save)); // 현재 지배 단계 계산

            return after > before; // 단계 상승 반환
        } // 확인 끝

        private static LobbyDialogueDefinition ResolveExactDialogue( // 세부 조건 대사 조회
            LobbyDialogueCategory category, // 선택 분류
            SaveData save, // 저장 자료
            StageResultSummary result) // 직전 결과
        { // 조회 시작
            if (category == LobbyDialogueCategory.Mastery) // 숙련 대사 확인
            { // 숙련 시작
                int stars = save == null || !result.HasLocation // 장소 확인
                    ? 0 // 별 없음
                    : MasteryLogic.GetStars(save, result.LocationId); // 현재 별 수

                return LobbyDialogueCatalog.Find( // 숙련 대사 반환
                    stars >= MasteryLogic.MaxStars // 최대 숙련 확인
                        ? "LOBBY_MASTERY_02" // 별 셋 대사 ID
                        : "LOBBY_MASTERY_01"); // 일반 숙련 대사 ID
            } // 숙련 끝

            if (category == LobbyDialogueCategory.Progress) // 진행 대사 확인
            { // 진행 시작
                int clears = save == null || !result.HasLocation // 장소 확인
                    ? 0 // 클리어 없음
                    : PlayStatsLogic.Get(save, result.LocationId).Clears; // 현재 클리어 수

                return LobbyDialogueCatalog.Find( // 진행 대사 반환
                    clears > 1 // 반복 클리어 확인
                        ? "LOBBY_PROGRESS_02" // 반복 대사 ID
                        : "LOBBY_PROGRESS_01"); // 도시 진행 대사 ID
            } // 진행 끝

            return null; // 세부 대사 없음 반환
        } // 조회 끝

        private static List<LobbyDialogueDefinition> GetByCategory( // 분류 후보 조회
            LobbyDialogueCategory category) // 대사 분류
        { // 조회 시작
            List<LobbyDialogueDefinition> matches = new List<LobbyDialogueDefinition>(); // 후보 목록 생성

            for (int i = 0; i < LobbyDialogueCatalog.All.Length; i++) // 전체 대사 순회
            { // 순회 시작
                if (LobbyDialogueCatalog.All[i].Category == category) // 분류 일치 확인
                { // 일치 시작
                    matches.Add(LobbyDialogueCatalog.All[i]); // 후보 추가
                } // 일치 끝
            } // 순회 끝

            return matches; // 후보 반환
        } // 조회 끝

        private static LobbyDialogueDefinition Pick( // 후보 대사 선택
            IList<LobbyDialogueDefinition> candidates, // 후보 목록
            Random random, // 난수 도구
            string lastId) // 직전 대사 ID
        { // 선택 시작
            if (candidates == null || candidates.Count == 0) // 후보 없음 확인
            { // 후보 없음 시작
                return null; // 대사 없음 반환
            } // 후보 없음 끝

            int start = random.Next(candidates.Count); // 시작 위치 선택

            for (int offset = 0; offset < candidates.Count; offset++) // 후보 순회
            { // 순회 시작
                LobbyDialogueDefinition candidate = candidates[(start + offset) % candidates.Count]; // 순환 후보 조회

                if (!string.Equals(candidate.Id, lastId, StringComparison.Ordinal)) // 직전 ID 제외
                { // 다른 대사 시작
                    return candidate; // 대사 반환
                } // 다른 대사 끝
            } // 순회 끝

            return null; // 반복 없는 후보 없음 반환
        } // 선택 끝
    } // 클래스 끝

    /// <summary>방 안 서큐버스의 자세다. 자세마다 그림 파일이 다르다.</summary>
    public enum HubSuccubusPose
    {
        Stand = 0,
        Sit = 1
    }

    /// <summary>서큐버스가 서 있을 수 있는 자리 하나다. 좌표는 발끝(1920×1080 가운데 기준)이다.</summary>
    public struct HubSuccubusSpot
    {
        public readonly string Name;
        public readonly float X;
        public readonly float Y;
        public readonly HubSuccubusPose Pose;

        public HubSuccubusSpot(
            string name,
            float x,
            float y,
            HubSuccubusPose pose)
        {
            Name = name;
            X = x;
            Y = y;
            Pose = pose;
        }
    }

    /// <summary>
    /// 허브 방 안의 서큐버스 규칙이다 (36일차).
    ///
    ///   허브에 들어올 때마다 자리 6곳 중 하나에 무작위로 있다(바로 전 자리는 피한다).
    ///   누르면 머리 오른쪽 위 말풍선에 대사가 나온다. 다시 누르면 다른 대사다.
    ///   대사는 진행 상황(첫 출격 전 · 강화 가능 · 라이벌 · 엔딩 · 지배도)에 맞춰 섞인다.
    ///
    /// 그림 파일 (Resources/Characters/Succubus/):
    ///   Room_Stand.png · Room_Sit.png → 없으면 Idle.png → 없으면 임시 실루엣
    ///   발끝이 그림 아래 가운데에 오게 그리면 된다.
    /// </summary>
    public static class HubSuccubusLogic
    {
        public const string ArtRoot = "Characters/Succubus";

        /// <summary>화면에서의 키(px)다.</summary>
        public const float StandHeight = 300f;
        public const float SitHeight = 220f;

        public const float BubbleWidth = 340f;
        public const float BubbleMinHeight = 64f;
        public const float BubblePadding = 16f;

        /// <summary>말풍선이 이 x를 넘으면 머리 왼쪽 위로 뒤집는다(화면 오른쪽 끝 여백 포함).</summary>
        public const float ScreenHalfWidth = 960f;
        public const float ScreenMargin = 24f;

        /// <summary>
        /// 자리 6곳이다. 방 물건(노트 · 일기장 · 창문 · 책장 · 시계)의 누르는 자리와 겹치지 않게 골랐다.
        /// </summary>
        public static readonly HubSuccubusSpot[] Spots =
        {
            new HubSuccubusSpot("창가", 90f, -330f, HubSuccubusPose.Stand),
            new HubSuccubusSpot("책상 옆", -120f, -250f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("침대 위", 560f, -222f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("책장 앞", -520f, -300f, HubSuccubusPose.Stand),
            new HubSuccubusSpot("러그 위", -60f, -405f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("침대 앞", 430f, -345f, HubSuccubusPose.Stand)
        };

        public static readonly string[] CommonLines =
        {
            "오늘 밤은 어디로 가 볼까?",
            "창밖 불빛 예쁘지? 언젠가 전부 내 거야.",
            "노트에 계약을 적어 두면 더 강해질 수 있어.",
            "시계는 거짓말을 안 해. 밤은 생각보다 짧아.",
            "일기장은 몰래 보지 마… 아니, 봐도 돼.",
            "너무 오래 쉬면 정기가 식어 버려.",
            "학생 방이라 좁지만, 나름 아늑하지?",
            "사람들 마음은 생각보다 쉽게 흔들려. 조심해서 다뤄야 해."
        };

        public static string GetArtPath(
            HubSuccubusPose pose)
        {
            return pose == HubSuccubusPose.Sit
                ? $"{ArtRoot}/Room_Sit"
                : $"{ArtRoot}/Room_Stand";
        }

        public static string FallbackArtPath =>
            $"{ArtRoot}/Idle";

        public static float GetHeight(
            HubSuccubusPose pose)
        {
            return pose == HubSuccubusPose.Sit
                ? SitHeight
                : StandHeight;
        }

        /// <summary>자리 번호를 고른다. 자리가 둘 이상이면 바로 전 자리는 피한다.</summary>
        public static int PickSpot(
            Random random,
            int lastSpot)
        {
            int count = Spots.Length;

            if (count <= 1)
            {
                return 0;
            }

            if (lastSpot < 0 ||
                lastSpot >= count)
            {
                return random.Next(count);
            }

            // 전 자리를 뺀 나머지 중에서 고른다.
            int pick = random.Next(count - 1);

            return pick >= lastSpot
                ? pick + 1
                : pick;
        }

        /// <summary>
        /// 지금 진행 상황에 맞는 대사 목록이다.
        /// 상황 대사는 두 번 넣어 공통 대사보다 자주 나오게 한다.
        /// </summary>
        public static List<string> GetLines(
            SaveData save)
        {
            List<string> lines = new List<string>(CommonLines);

            void Add(string line)
            {
                lines.Add(line);
                lines.Add(line);
            }

            if (save == null)
            {
                return lines;
            }

            PlayStats stats = save.Stats ?? new PlayStats();
            int cleared = AchievementLogic.GetValue(save, AchievementStat.LocationsCleared);
            int dominion = DominionLogic.GetPercent(save);
            bool ended = stats.Endings > 0;

            if (stats.Attempts <= 0)
            {
                Add("처음이니까 고등학교부터 가 보자. 야간 수업을 마친 성인 교육생이 많대.");
                Add("창문을 누르면 바로 도시로 나갈 수 있어.");
            }

            if (CanBuyAnyUpgrade(save))
            {
                Add("계약 정기가 모였어. 노트에서 강화해 볼까?");
            }

            if (StoryLogic.IsSeen(save, "rival_taunt_3") &&
                !ended)
            {
                Add("그 라이벌… 루프탑에서 날 기다리고 있겠지.");
            }

            if (cleared >= 6 &&
                !ended)
            {
                Add("초대장이 왔어. 루프탑 클럽, 이제 갈 때가 됐나?");
            }

            if (ended)
            {
                Add("라이벌을 이겼는데도, 밤이 또 기다려져.");

                if (AchievementLogic.GetValue(save, AchievementStat.NightLocations) <= 0)
                {
                    Add("심야 모드, 아직 안 가 봤지? 지도에서 ☾를 눌러 봐.");
                }
                else
                {
                    Add("심야의 도시는 역시 짜릿해.");
                }
            }

            if (dominion >= 100)
            {
                Add("이 도시의 밤은 이제 전부 내 거야.");
            }
            else if (dominion >= 50)
            {
                Add("도시의 절반이 내 이름을 속삭여.");
            }

            return lines;
        }

        public static bool CanBuyAnyUpgrade(
            SaveData save)
        {
            if (save == null)
            {
                return false;
            }

            foreach (UpgradeTrack track in Enum.GetValues(typeof(UpgradeTrack)))
            {
                int level = SaveDataLogic.GetUpgradeLevel(save, track);

                if (!UpgradeLogic.IsMaxLevel(level) &&
                    UpgradeLogic.CanPurchase(level, save.ContractEssence))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>대사를 고른다. 둘 이상이면 바로 전 대사와 다른 것이다.</summary>
        public static string PickLine(
            IList<string> lines,
            Random random,
            string last)
        {
            if (lines == null ||
                lines.Count == 0)
            {
                return string.Empty;
            }

            for (int attempt = 0; attempt < 8; attempt++)
            {
                string line = lines[random.Next(lines.Count)];

                if (line != last)
                {
                    return line;
                }
            }

            foreach (string line in lines)
            {
                if (line != last)
                {
                    return line;
                }
            }

            return lines[0];
        }

        /// <summary>말풍선이 떠 있는 시간(초)이다. 글이 길수록 오래.</summary>
        public static float GetBubbleSeconds(
            string text)
        {
            int length = text == null ? 0 : text.Length;

            return Math.Max(2.5f, Math.Min(6f, 1.5f + length * 0.08f));
        }

        /// <summary>
        /// 말풍선을 머리 왼쪽 위로 뒤집어야 하는지다.
        /// 오른쪽 위에 두면 화면 밖으로 나갈 때만 뒤집는다.
        /// </summary>
        public static bool ShouldFlip(
            float characterX,
            float headOffsetX,
            float bubbleWidth)
        {
            return characterX + headOffsetX + bubbleWidth > ScreenHalfWidth - ScreenMargin;
        }
    }
}
