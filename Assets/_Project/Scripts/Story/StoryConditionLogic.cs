using System; // 문자열 비교 참조
using System.Collections.Generic; // 목록과 집합 참조
using ProjectTheta.Save; // 저장 자료 참조

namespace ProjectTheta.Story // 이야기 공간
{ // 공간 시작
    public static class StoryConditionLogic // 이야기 조건 규칙
    { // 클래스 시작
        public static bool AreMet(SaveData save, StoryCondition[] conditions) // 전체 조건 확인
        { // 확인 시작
            if (conditions == null || conditions.Length == 0) // 조건 없음 확인
            { // 허용 시작
                return true; // 조건 충족 반환
            } // 허용 끝

            foreach (StoryCondition condition in conditions) // 조건 순회
            { // 순회 시작
                if (!IsMet(save, condition)) // 개별 조건 확인
                { // 실패 시작
                    return false; // 조건 실패 반환
                } // 실패 끝
            } // 순회 끝

            return true; // 전체 충족 반환
        } // 확인 끝

        private static bool IsMet(SaveData save, StoryCondition condition) // 개별 조건 확인
        { // 확인 시작
            if (condition.Type == StoryConditionType.Always) // 항상 조건 확인
            { // 허용 시작
                return true; // 조건 충족 반환
            } // 허용 끝

            if (save == null) // 저장 확인
            { // 실패 시작
                return false; // 조건 실패 반환
            } // 실패 끝

            switch (condition.Type) // 조건 종류 분기
            { // 분기 시작
                case StoryConditionType.LocationCleared: // 장소 클리어 조건
                    return PlayStatsLogic.Get(save, (int)condition.Location).Clears > 0; // 장소 기록 확인

                case StoryConditionType.LocationsCleared: // 장소 수 조건
                    return AchievementLogic.GetValue(save, AchievementStat.LocationsCleared) >= condition.Threshold; // 클리어 수 확인

                case StoryConditionType.Endings: // 엔딩 조건
                    return save.Stats != null && save.Stats.Endings >= condition.Threshold; // 엔딩 수 확인

                case StoryConditionType.Dominion: // 지배도 조건
                    return DominionLogic.GetPercent(save) >= condition.Threshold; // 지배도 확인

                case StoryConditionType.StoryCompleted: // 선행 완료 조건
                    return StoryProgressLogic.IsCompleted(save, condition.StoryId); // 완료 여부 확인

                case StoryConditionType.ChoiceEquals: // 선택 일치 조건
                    return string.Equals( // 선택 비교
                        StoryProgressLogic.GetChoice(save, condition.StoryId), // 저장 선택
                        condition.ChoiceId, // 요구 선택
                        StringComparison.Ordinal); // 정확 비교

                case StoryConditionType.RegionalStoriesCompleted: // 일반 지역 이야기 수 조건
                    return StoryArcLogic.GetCompletedRegionalStories(save) >= condition.Threshold; // 완료 지역 수 확인

                default: // 알 수 없는 조건
                    return false; // 조건 실패 반환
            } // 분기 끝
        } // 확인 끝
    } // 클래스 끝

    public static class StoryQueueLogic // 이야기 대기열 규칙
    { // 클래스 시작
        public static List<StoryScene> GetReplayable( // 다시 보기 장면 조회
            SaveData save, // 저장 자료
            IList<StoryScene> scenes) // 장면 표
        { // 조회 시작
            List<StoryScene> result = new List<StoryScene>(); // 결과 목록

            if (scenes == null) // 장면 표 확인
            { // 빈 결과 시작
                return result; // 빈 목록 반환
            } // 빈 결과 끝

            foreach (StoryScene scene in scenes) // 장면 순회
            { // 순회 시작
                if (scene != null && // 장면 확인
                    scene.CanReplay && // 다시 보기 허용 확인
                    StoryProgressLogic.IsSeen(save, scene.Id)) // 본 장면 확인
                { // 추가 시작
                    result.Add(scene); // 결과 추가
                } // 추가 끝
            } // 순회 끝

            return result; // 다시 보기 목록 반환
        } // 조회 끝

        public static List<StoryScene> GetAvailable( // 재생 가능 장면 조회
            SaveData save, // 저장 자료
            IList<StoryScene> scenes, // 장면 표
            StoryContext context) // 발생 사건
        { // 조회 시작
            List<StoryScene> result = new List<StoryScene>(); // 결과 목록

            if (scenes == null) // 장면 표 확인
            { // 빈 결과 시작
                return result; // 빈 목록 반환
            } // 빈 결과 끝

            StoryScene active = null; // 진행 장면 자리

            foreach (StoryScene scene in scenes) // 장면 순회
            { // 순회 시작
                if (scene == null || string.IsNullOrEmpty(scene.Id)) // 장면 유효성 확인
                { // 제외 시작
                    continue; // 다음 장면 이동
                } // 제외 끝

                if (string.Equals(save == null ? string.Empty : save.ActiveStoryId, scene.Id, StringComparison.Ordinal)) // 진행 장면 확인
                { // 진행 저장 시작
                    active = scene; // 진행 장면 저장
                    continue; // 일반 판정 제외
                } // 진행 저장 끝

                if (!CanQueue(save, scene, context)) // 대기 가능 여부 확인
                { // 제외 시작
                    continue; // 다음 장면 이동
                } // 제외 끝

                result.Add(scene); // 결과 추가
            } // 순회 끝

            if (active != null && // 진행 장면 확인
                !StoryProgressLogic.IsCompleted(save, active.Id) && // 미완료 확인
                MatchesEvent(active, context)) // 사건 일치 확인
            { // 우선 배치 시작
                result.Insert(0, active); // 진행 장면 맨 앞 추가
            } // 우선 배치 끝

            return result; // 대기열 반환
        } // 조회 끝

        private static bool CanQueue(SaveData save, StoryScene scene, StoryContext context) // 대기 가능 확인
        { // 확인 시작
            if (!StoryArcLogic.CanPlayScene(save, scene.Id)) // 이야기 구간 해금 확인
            { // 잠금 확인 시작
                return false; // 잠긴 장면 제외
            } // 잠금 확인 끝

            if (!scene.AutoPlay || StoryProgressLogic.IsCompleted(save, scene.Id)) // 자동 재생과 완료 확인
            { // 거부 시작
                return false; // 대기 불가 반환
            } // 거부 끝

            if (!MatchesEvent(scene, context)) // 사건 일치 확인
            { // 거부 시작
                return false; // 대기 불가 반환
            } // 거부 끝

            if ((scene.EventType == StoryEventType.LocationEnter || scene.EventType == StoryEventType.LocationClear) && // 장소 사건 확인
                !scene.MatchAnyLocation && // 공용 장소 여부 확인
                scene.Location != context.Location) // 장소 일치 확인
            { // 거부 시작
                return false; // 대기 불가 반환
            } // 거부 끝

            return StoryConditionLogic.AreMet(save, scene.Conditions); // 조건 결과 반환
        } // 확인 끝

        private static bool MatchesEvent(StoryScene scene, StoryContext context) // 사건 일치 확인
        { // 확인 시작
            bool safeAreaContext = // 안전 구역 사건 여부
                context.EventType == StoryEventType.SafeArea || // 공용 안전 구역
                context.EventType == StoryEventType.HubReturn || // 허브 복귀
                context.EventType == StoryEventType.MapEnter; // 지도 진입

            return scene.EventType == StoryEventType.SafeArea // 안전 구역 장면 확인
                ? safeAreaContext // 안전 구역에서만 허용
                : scene.EventType == context.EventType; // 동일 사건만 허용
        } // 확인 끝
    } // 클래스 끝

    public static class StoryCatalogValidation // 이야기 표 검증
    { // 클래스 시작
        public static string[] GetInvalidSceneIds(IList<StoryScene> scenes) // 무효 장면 조회
        { // 조회 시작
            Dictionary<string, StoryScene> byId = new Dictionary<string, StoryScene>(StringComparer.Ordinal); // 장면 사전
            HashSet<string> invalid = new HashSet<string>(StringComparer.Ordinal); // 무효 ID 집합

            if (scenes == null) // 장면 표 확인
            { // 빈 결과 시작
                return new string[0]; // 빈 배열 반환
            } // 빈 결과 끝

            foreach (StoryScene scene in scenes) // 장면 순회
            { // 순회 시작
                if (scene == null || string.IsNullOrEmpty(scene.Id) || byId.ContainsKey(scene.Id)) // ID 유효성 확인
                { // 무효 시작
                    if (scene != null && !string.IsNullOrEmpty(scene.Id)) // ID 존재 확인
                    { // 기록 시작
                        invalid.Add(scene.Id); // 중복 ID 기록
                    } // 기록 끝

                    continue; // 다음 장면 이동
                } // 무효 끝

                byId.Add(scene.Id, scene); // 장면 등록
            } // 순회 끝

            foreach (StoryScene scene in byId.Values) // 등록 장면 순회
            { // 순회 시작
                foreach (StoryCondition condition in scene.Conditions) // 조건 순회
                { // 조건 시작
                    if ((condition.Type == StoryConditionType.StoryCompleted || condition.Type == StoryConditionType.ChoiceEquals) && // 선행 조건 확인
                        !byId.ContainsKey(condition.StoryId)) // 선행 장면 존재 확인
                    { // 무효 시작
                        invalid.Add(scene.Id); // 현재 장면 무효 기록
                    } // 무효 끝
                } // 조건 끝
            } // 순회 끝

            Dictionary<string, int> states = new Dictionary<string, int>(StringComparer.Ordinal); // 방문 상태 사전
            List<string> path = new List<string>(); // 탐색 경로

            foreach (string id in byId.Keys) // 장면 ID 순회
            { // 순회 시작
                Visit(id, byId, states, path, invalid); // 순환 탐색
            } // 순회 끝

            string[] result = new string[invalid.Count]; // 결과 배열 생성
            invalid.CopyTo(result); // 무효 ID 복사

            return result; // 결과 반환
        } // 조회 끝

        private static void Visit( // 선행 조건 순환 탐색
            string id, // 현재 장면 ID
            Dictionary<string, StoryScene> byId, // 장면 사전
            Dictionary<string, int> states, // 방문 상태
            List<string> path, // 탐색 경로
            HashSet<string> invalid) // 무효 ID 집합
        { // 탐색 시작
            if (states.TryGetValue(id, out int state)) // 방문 여부 확인
            { // 방문 처리 시작
                if (state == 1) // 현재 경로 재방문 확인
                { // 순환 기록 시작
                    int start = path.IndexOf(id); // 순환 시작 위치

                    for (int i = Math.Max(0, start); i < path.Count; i++) // 순환 경로 순회
                    { // 순회 시작
                        invalid.Add(path[i]); // 순환 장면 기록
                    } // 순회 끝
                } // 순환 기록 끝

                return; // 탐색 중단
            } // 방문 처리 끝

            states[id] = 1; // 탐색 중 표시
            path.Add(id); // 경로 추가

            foreach (StoryCondition condition in byId[id].Conditions) // 조건 순회
            { // 순회 시작
                if (condition.Type == StoryConditionType.StoryCompleted && byId.ContainsKey(condition.StoryId)) // 선행 완료 조건 확인
                { // 재귀 시작
                    Visit(condition.StoryId, byId, states, path, invalid); // 선행 장면 탐색
                } // 재귀 끝
            } // 순회 끝

            path.RemoveAt(path.Count - 1); // 경로 제거
            states[id] = 2; // 탐색 완료 표시
        } // 탐색 끝
    } // 클래스 끝
} // 공간 끝
