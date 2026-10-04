using ProjectTheta.Stage.Locations; // 장소 자료 참조

namespace ProjectTheta.Story // 이야기 공간
{ // 공간 시작
    public enum StoryTrigger // 기존 이야기 조건
    { // 열거 시작
        LocationEnter = 0, // 장소 입장
        LocationClear = 1, // 장소 클리어
        LocationsCleared = 2, // 여러 장소 클리어
        Endings = 3, // 엔딩 횟수
        Dominion = 4, // 도시 지배도
        Custom = 5 // 신규 조건 조합
    } // 열거 끝

    public enum StoryEventType // 이야기 발생 사건
    { // 열거 시작
        None = 0, // 사건 없음
        LocationEnter = 1, // 장소 진입
        LocationClear = 2, // 장소 클리어
        HubReturn = 3, // 허브 복귀
        MapEnter = 4, // 지도 진입
        SafeArea = 5, // 지도 또는 허브
        BossVictory = 6, // 보스 승리
        BossDefeat = 7, // 보스 패배
        FirstStageEnter = 8, // 첫 계약 임무 진입
        HypnosisSucceeded = 9, // 최면 성공
        RecoveryConfirmed = 10, // 회수 확정
        FollowerStolen = 11, // 동행자 탈취
        FollowerReclaimed = 12 // 동행자 재탈환
    } // 열거 끝

    public enum StoryConditionType // 이야기 해금 조건 종류
    { // 열거 시작
        Always = 0, // 항상 허용
        LocationCleared = 1, // 특정 장소 클리어
        LocationsCleared = 2, // 서로 다른 장소 수
        Endings = 3, // 엔딩 횟수
        Dominion = 4, // 도시 지배도
        StoryCompleted = 5, // 선행 이야기 완료
        ChoiceEquals = 6, // 선행 선택 일치
        RegionalStoriesCompleted = 7 // 일반 지역 이야기 완료 수
    } // 열거 끝

    public readonly struct StoryContext // 이야기 사건 자료
    { // 자료 시작
        public readonly StoryEventType EventType; // 발생 사건
        public readonly LocationId Location; // 관련 장소

        public StoryContext( // 사건 생성
            StoryEventType eventType, // 발생 사건
            LocationId location) // 관련 장소
        { // 생성 시작
            EventType = eventType; // 사건 저장
            Location = location; // 장소 저장
        } // 생성 끝
    } // 자료 끝

    public readonly struct StoryCondition // 이야기 해금 조건
    { // 자료 시작
        public readonly StoryConditionType Type; // 조건 종류
        public readonly LocationId Location; // 조건 장소
        public readonly int Threshold; // 조건 기준값
        public readonly string StoryId; // 선행 장면 ID
        public readonly string ChoiceId; // 선행 선택 ID

        private StoryCondition( // 조건 생성
            StoryConditionType type, // 조건 종류
            LocationId location, // 조건 장소
            int threshold, // 조건 기준값
            string storyId, // 선행 장면 ID
            string choiceId) // 선행 선택 ID
        { // 생성 시작
            Type = type; // 종류 저장
            Location = location; // 장소 저장
            Threshold = threshold; // 기준값 저장
            StoryId = storyId ?? string.Empty; // 장면 ID 저장
            ChoiceId = choiceId ?? string.Empty; // 선택 ID 저장
        } // 생성 끝

        public static StoryCondition Always() // 항상 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.Always, LocationId.TrainingCenter, 0, string.Empty, string.Empty); // 항상 조건 반환
        } // 생성 끝

        public static StoryCondition LocationCleared(LocationId location) // 장소 클리어 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.LocationCleared, location, 1, string.Empty, string.Empty); // 장소 조건 반환
        } // 생성 끝

        public static StoryCondition LocationsCleared(int threshold) // 장소 수 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.LocationsCleared, LocationId.TrainingCenter, threshold, string.Empty, string.Empty); // 장소 수 조건 반환
        } // 생성 끝

        public static StoryCondition Endings(int threshold) // 엔딩 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.Endings, LocationId.TrainingCenter, threshold, string.Empty, string.Empty); // 엔딩 조건 반환
        } // 생성 끝

        public static StoryCondition Dominion(int threshold) // 지배도 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.Dominion, LocationId.TrainingCenter, threshold, string.Empty, string.Empty); // 지배도 조건 반환
        } // 생성 끝

        public static StoryCondition StoryCompleted(string storyId) // 선행 완료 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.StoryCompleted, LocationId.TrainingCenter, 0, storyId, string.Empty); // 선행 완료 조건 반환
        } // 생성 끝

        public static StoryCondition ChoiceEquals(string storyId, string choiceId) // 선택 일치 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.ChoiceEquals, LocationId.TrainingCenter, 0, storyId, choiceId); // 선택 조건 반환
        } // 생성 끝

        public static StoryCondition RegionalStoriesCompleted(int threshold) // 지역 이야기 수 조건 생성
        { // 생성 시작
            return new StoryCondition(StoryConditionType.RegionalStoriesCompleted, LocationId.TrainingCenter, threshold, string.Empty, string.Empty); // 지역 이야기 수 조건 반환
        } // 생성 끝
    } // 자료 끝

    public readonly struct StoryLine // 이야기 대사 한 줄
    { // 자료 시작
        public readonly string Speaker; // 말하는 사람
        public readonly string Text; // 대사 내용

        public StoryLine( // 대사 생성
            string speaker, // 말하는 사람
            string text) // 대사 내용
        { // 생성 시작
            Speaker = speaker ?? string.Empty; // 화자 저장
            Text = text ?? string.Empty; // 내용 저장
        } // 생성 끝
    } // 자료 끝

    public sealed class StoryChoice // 이야기 선택지
    { // 클래스 시작
        public readonly string Id; // 선택 ID
        public readonly string Text; // 표시 문구

        public StoryChoice( // 선택지 생성
            string id, // 선택 ID
            string text) // 표시 문구
        { // 생성 시작
            Id = id ?? string.Empty; // ID 저장
            Text = text ?? string.Empty; // 문구 저장
        } // 생성 끝
    } // 클래스 끝

    public sealed class StoryScene // 이야기 장면
    { // 클래스 시작
        public readonly string Id; // 장면 ID
        public readonly string Title; // 장면 제목
        public readonly StoryTrigger Trigger; // 기존 조건
        public readonly StoryEventType EventType; // 발생 사건
        public readonly LocationId Location; // 관련 장소
        public readonly int Threshold; // 기존 기준값
        public readonly bool AutoPlay; // 자동 재생 여부
        public readonly bool CanReplay; // 다시 보기 여부
        public readonly bool MatchAnyLocation; // 모든 장소 일치 여부
        public readonly StoryCondition[] Conditions; // 해금 조건
        public readonly StoryChoice[] Choices; // 선택지 목록
        public readonly StoryLine[] Lines; // 대사 목록

        public StoryScene( // 기존 장면 생성
            string id, // 장면 ID
            string title, // 장면 제목
            StoryTrigger trigger, // 기존 조건
            LocationId location, // 관련 장소
            int threshold, // 기존 기준값
            params StoryLine[] lines) // 대사 목록
            : this( // 신규 생성자로 연결
                id, // 장면 ID
                title, // 장면 제목
                GetLegacyEvent(trigger), // 발생 사건 변환
                location, // 관련 장소
                true, // 자동 재생 허용
                true, // 다시 보기 허용
                GetLegacyConditions(trigger, location, threshold), // 조건 변환
                new StoryChoice[0], // 선택지 없음
                lines) // 대사 목록
        { // 생성 시작
            Trigger = trigger; // 기존 조건 보존
            Threshold = threshold; // 기존 기준값 보존
        } // 생성 끝

        public StoryScene( // 신규 장면 생성
            string id, // 장면 ID
            string title, // 장면 제목
            StoryEventType eventType, // 발생 사건
            LocationId location, // 관련 장소
            bool autoPlay, // 자동 재생 여부
            bool canReplay, // 다시 보기 여부
            StoryCondition[] conditions, // 해금 조건
            StoryChoice[] choices, // 선택지 목록
            StoryLine[] lines) // 대사 목록
            : this( // 확장 생성자로 연결
                id, // 장면 ID
                title, // 장면 제목
                eventType, // 발생 사건
                location, // 관련 장소
                autoPlay, // 자동 재생 여부
                canReplay, // 다시 보기 여부
                false, // 지정 장소만 일치
                conditions, // 해금 조건
                choices, // 선택지 목록
                lines) // 대사 목록
        { // 생성 시작
        } // 생성 끝

        public StoryScene( // 공용 장소 장면 생성
            string id, // 장면 ID
            string title, // 장면 제목
            StoryEventType eventType, // 발생 사건
            LocationId location, // 관련 장소
            bool autoPlay, // 자동 재생 여부
            bool canReplay, // 다시 보기 여부
            bool matchAnyLocation, // 모든 장소 일치 여부
            StoryCondition[] conditions, // 해금 조건
            StoryChoice[] choices, // 선택지 목록
            StoryLine[] lines) // 대사 목록
        { // 생성 시작
            Id = id ?? string.Empty; // ID 저장
            Title = title ?? string.Empty; // 제목 저장
            Trigger = StoryTrigger.Custom; // 신규 조건 표시
            EventType = eventType; // 사건 저장
            Location = location; // 장소 저장
            Threshold = 0; // 기존 기준값 초기화
            AutoPlay = autoPlay; // 자동 재생 저장
            CanReplay = canReplay; // 다시 보기 저장
            MatchAnyLocation = matchAnyLocation; // 모든 장소 일치 저장
            Conditions = conditions ?? new StoryCondition[0]; // 조건 저장
            Choices = choices ?? new StoryChoice[0]; // 선택지 저장
            Lines = lines ?? new StoryLine[0]; // 대사 저장
        } // 생성 끝

        public bool HasLocation => // 장소 연관 여부
            !MatchAnyLocation && // 공용 장소 제외
            (EventType == StoryEventType.LocationEnter || // 장소 진입 확인
             EventType == StoryEventType.LocationClear || // 장소 클리어 확인
             Trigger == StoryTrigger.LocationEnter || // 기존 입장 확인
             Trigger == StoryTrigger.LocationClear); // 기존 클리어 확인

        private static StoryEventType GetLegacyEvent(StoryTrigger trigger) // 기존 사건 변환
        { // 변환 시작
            return trigger == StoryTrigger.LocationEnter // 장소 입장 확인
                ? StoryEventType.LocationEnter // 장소 입장 사건
                : StoryEventType.SafeArea; // 안전 구역 사건
        } // 변환 끝

        private static StoryCondition[] GetLegacyConditions( // 기존 조건 변환
            StoryTrigger trigger, // 기존 조건
            LocationId location, // 관련 장소
            int threshold) // 기존 기준값
        { // 변환 시작
            switch (trigger) // 기존 조건 분기
            { // 분기 시작
                case StoryTrigger.LocationClear: // 장소 클리어
                    return new[] { StoryCondition.LocationCleared(location) }; // 장소 조건 반환

                case StoryTrigger.LocationsCleared: // 여러 장소 클리어
                    return new[] { StoryCondition.LocationsCleared(threshold) }; // 장소 수 조건 반환

                case StoryTrigger.Endings: // 엔딩 횟수
                    return new[] { StoryCondition.Endings(threshold) }; // 엔딩 조건 반환

                case StoryTrigger.Dominion: // 도시 지배도
                    return new[] { StoryCondition.Dominion(threshold) }; // 지배도 조건 반환

                default: // 입장과 신규 조건
                    return new StoryCondition[0]; // 조건 없음 반환
            } // 분기 끝
        } // 변환 끝
    } // 클래스 끝
} // 공간 끝
