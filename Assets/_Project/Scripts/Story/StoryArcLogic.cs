using ProjectTheta.Save; // 저장 자료 참조

namespace ProjectTheta.Story // 이야기 공간
{ // 공간 시작
    public static class StoryArcLogic // 이야기 구간 해금 규칙
    { // 클래스 시작
        private static readonly string[] RegionalClearIds = // 일반 지역 완료 장면 ID
        { // 목록 시작
            "clear_training", // 고등학교 완료
            "clear_beach", // 해변가 완료
            "clear_subway", // 지하철 완료
            "clear_fitness", // 스포츠센터 완료
            "clear_market", // 야시장 완료
            "clear_mall", // 쇼핑몰 완료
            "clear_office" // 오피스 완료
        }; // 목록 끝

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

        public static int GetCompletedRegionalStories(SaveData save) // 완료 일반 지역 수 조회
        { // 조회 시작
            int completed = 0; // 완료 수 초기화

            foreach (string sceneId in RegionalClearIds) // 지역 완료 장면 순회
            { // 순회 시작
                if (StoryProgressLogic.IsCompleted(save, sceneId)) // 지역 완료 확인
                { // 완료 반영 시작
                    completed++; // 완료 수 증가
                } // 완료 반영 끝
            } // 순회 끝

            return completed; // 완료 수 반환
        } // 조회 끝

        public static bool IsLocationUnlocked( // 장소 해금 확인
            SaveData save, // 현재 저장
            Stage.Locations.LocationId location) // 확인 장소
        { // 확인 시작
            return location != Stage.Locations.LocationId.RooftopClub || // 일반 지역 허용
                   StoryProgressLogic.IsCompleted(save, StoryCatalog.BattleEveId); // 결전 전야 완료 확인
        } // 확인 끝

        public static string GetLocationLockMessage( // 장소 잠금 문구 조회
            SaveData save, // 현재 저장
            Stage.Locations.LocationId location) // 확인 장소
        { // 조회 시작
            return IsLocationUnlocked(save, location) // 해금 여부 확인
                ? string.Empty // 잠금 문구 없음
                : "일반 지역 7곳과 결전 전야 완료 필요"; // 루프탑 잠금 문구
        } // 조회 끝

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
