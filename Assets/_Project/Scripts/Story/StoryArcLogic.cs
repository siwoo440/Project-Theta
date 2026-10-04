using ProjectTheta.Save; // 저장 자료 참조

namespace ProjectTheta.Story // 이야기 공간
{ // 공간 시작
    public static class StoryArcLogic // 이야기 구간 해금 규칙
    { // 클래스 시작
        public static bool AreRegionalEpisodesUnlocked(SaveData save) // 지역 이야기 해금 확인
        { // 확인 시작
            return StoryProgressLogic.IsCompleted( // 완료 여부 반환
                save, // 현재 저장
                StoryCatalog.RiellaMorningAfterId); // 스토리 4 마지막 장면
        } // 확인 끝

        public static bool CanPlayScene( // 장면 재생 가능 확인
            SaveData save, // 현재 저장
            string sceneId) // 장면 ID
        { // 확인 시작
            return !IsRegionalPlaceholder(sceneId) || // 초반 장면과 후일담 허용
                   AreRegionalEpisodesUnlocked(save); // 지역 이야기 해금 확인
        } // 확인 끝

        private static bool IsRegionalPlaceholder(string sceneId) // 기존 지역 장면 확인
        { // 확인 시작
            switch (sceneId) // 장면 ID 분기
            { // 분기 시작
                case "enter_training": // 고등학교 입장
                case "clear_training": // 고등학교 클리어
                case "enter_beach": // 해변가 입장
                case "clear_beach": // 해변가 클리어
                case "enter_subway": // 지하철 입장
                case "clear_subway": // 지하철 클리어
                case "enter_fitness": // 스포츠센터 입장
                case "clear_fitness": // 스포츠센터 클리어
                case "enter_market": // 야시장 입장
                case "clear_market": // 야시장 클리어
                case "enter_mall": // 쇼핑몰 입장
                case "clear_mall": // 쇼핑몰 클리어
                case "enter_office": // 오피스 입장
                case "clear_office": // 오피스 클리어
                case "enter_club": // 루프탑 입장
                case "clear_club": // 루프탑 클리어
                case "rival_taunt_3": // 첫 경쟁자 도발
                case "rival_taunt_6": // 둘째 경쟁자 도발
                    return true; // 지역 이야기 판정

                default: // 초반 또는 후일담
                    return false; // 일반 장면 판정
            } // 분기 끝
        } // 확인 끝
    } // 클래스 끝

    public static class StageStoryEventLogic // 스테이지 사건 변환 규칙
    { // 클래스 시작
        public static StoryEventType GetHypnosisEvent(bool wasReclaim) // 최면 사건 변환
        { // 변환 시작
            return wasReclaim // 재탈환 여부 확인
                ? StoryEventType.FollowerReclaimed // 재탈환 사건 반환
                : StoryEventType.HypnosisSucceeded; // 첫 확보 사건 반환
        } // 변환 끝

        public static StoryEventType GetDuelEvent() // 힘겨루기 사건 변환
        { // 변환 시작
            return StoryEventType.None; // 소유권 변경 없음 반환
        } // 변환 끝
    } // 클래스 끝
} // 공간 끝
