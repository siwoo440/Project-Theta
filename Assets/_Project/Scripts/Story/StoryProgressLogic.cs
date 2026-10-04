using System; // 배열 기능 참조
using System.Collections.Generic; // 목록 기능 참조
using ProjectTheta.Save; // 저장 자료 참조

namespace ProjectTheta.Story // 이야기 공간
{ // 공간 시작
    public static class StoryProgressLogic // 이야기 진행 규칙
    { // 클래스 시작
        public static void Normalize(SaveData save) // 이야기 저장 보정
        { // 보정 시작
            if (save == null) // 저장 확인
            { // 중단 시작
                return; // 보정 중단
            } // 중단 끝

            save.SeenStories = NormalizeIds(save.SeenStories); // 본 장면 보정
            save.CompletedStories = NormalizeIds(save.CompletedStories); // 완료 장면 보정
            save.StoryChoices = NormalizeChoices(save.StoryChoices); // 선택 결과 보정
            save.ActiveStoryId = save.ActiveStoryId ?? string.Empty; // 진행 장면 보정

            if (save.Version < 3) // 구버전 확인
            { // 이관 시작
                foreach (string id in save.SeenStories) // 기존 본 장면 순회
                { // 순회 시작
                    AddId(ref save.CompletedStories, id); // 완료 장면 이관
                } // 순회 끝
            } // 이관 끝

            if (IsCompleted(save, save.ActiveStoryId)) // 완료 장면 진행 여부
            { // 정리 시작
                save.ActiveStoryId = string.Empty; // 진행 장면 해제
            } // 정리 끝
        } // 보정 끝

        public static bool IsSeen(SaveData save, string storyId) // 본 장면 확인
        { // 확인 시작
            return Contains(save == null ? null : save.SeenStories, storyId); // 포함 여부 반환
        } // 확인 끝

        public static bool IsCompleted(SaveData save, string storyId) // 완료 장면 확인
        { // 확인 시작
            return Contains(save == null ? null : save.CompletedStories, storyId); // 포함 여부 반환
        } // 확인 끝

        public static bool Begin(SaveData save, string storyId, bool replay) // 장면 시작
        { // 시작 처리
            if (save == null || string.IsNullOrEmpty(storyId)) // 입력 확인
            { // 거부 시작
                return false; // 시작 실패
            } // 거부 끝

            if (replay) // 다시 보기 확인
            { // 다시 보기 시작
                return true; // 저장 변경 없음
            } // 다시 보기 끝

            if (IsCompleted(save, storyId)) // 완료 여부 확인
            { // 거부 시작
                return false; // 중복 시작 거부
            } // 거부 끝

            AddId(ref save.SeenStories, storyId); // 본 장면 추가
            save.ActiveStoryId = storyId; // 진행 장면 저장

            return true; // 시작 성공
        } // 시작 처리 끝

        public static bool Complete(SaveData save, string storyId, bool replay) // 장면 완료
        { // 완료 처리
            if (save == null || string.IsNullOrEmpty(storyId)) // 입력 확인
            { // 거부 시작
                return false; // 완료 실패
            } // 거부 끝

            if (replay) // 다시 보기 확인
            { // 다시 보기 시작
                return true; // 저장 변경 없음
            } // 다시 보기 끝

            AddId(ref save.SeenStories, storyId); // 본 장면 추가
            AddId(ref save.CompletedStories, storyId); // 완료 장면 추가

            if (string.Equals(save.ActiveStoryId, storyId, StringComparison.Ordinal)) // 현재 장면 확인
            { // 진행 해제 시작
                save.ActiveStoryId = string.Empty; // 진행 장면 해제
            } // 진행 해제 끝

            return true; // 완료 성공
        } // 완료 처리 끝

        public static bool SetChoice( // 선택 결과 저장
            SaveData save, // 저장 자료
            string storyId, // 장면 ID
            string choiceId, // 선택 ID
            bool replay = false) // 다시 보기 여부
        { // 저장 시작
            if (save == null || // 저장 확인
                string.IsNullOrEmpty(storyId) || // 장면 ID 확인
                string.IsNullOrEmpty(choiceId)) // 선택 ID 확인
            { // 거부 시작
                return false; // 저장 실패
            } // 거부 끝

            if (replay) // 다시 보기 확인
            { // 다시 보기 시작
                return true; // 저장 변경 없음
            } // 다시 보기 끝

            StoryChoiceRecord[] choices = save.StoryChoices ?? new StoryChoiceRecord[0]; // 기존 선택 목록

            for (int i = 0; i < choices.Length; i++) // 선택 목록 순회
            { // 순회 시작
                if (!string.Equals(choices[i].StoryId, storyId, StringComparison.Ordinal)) // 장면 일치 확인
                { // 다음 선택 시작
                    continue; // 다음 선택 이동
                } // 다음 선택 끝

                choices[i] = new StoryChoiceRecord(storyId, choiceId); // 선택 결과 교체
                save.StoryChoices = choices; // 목록 저장

                return true; // 교체 성공
            } // 순회 끝

            StoryChoiceRecord[] grown = new StoryChoiceRecord[choices.Length + 1]; // 확장 목록 생성
            Array.Copy(choices, grown, choices.Length); // 기존 선택 복사
            grown[grown.Length - 1] = new StoryChoiceRecord(storyId, choiceId); // 신규 선택 추가
            save.StoryChoices = grown; // 확장 목록 저장

            return true; // 추가 성공
        } // 저장 끝

        public static string GetChoice(SaveData save, string storyId) // 선택 결과 조회
        { // 조회 시작
            if (save == null || save.StoryChoices == null || string.IsNullOrEmpty(storyId)) // 입력 확인
            { // 기본값 시작
                return string.Empty; // 빈 선택 반환
            } // 기본값 끝

            for (int i = save.StoryChoices.Length - 1; i >= 0; i--) // 최신 선택부터 순회
            { // 순회 시작
                StoryChoiceRecord choice = save.StoryChoices[i]; // 선택 조회

                if (string.Equals(choice.StoryId, storyId, StringComparison.Ordinal)) // 장면 일치 확인
                { // 반환 시작
                    return choice.ChoiceId ?? string.Empty; // 선택 ID 반환
                } // 반환 끝
            } // 순회 끝

            return string.Empty; // 선택 없음 반환
        } // 조회 끝

        private static bool Contains(string[] values, string value) // ID 포함 확인
        { // 확인 시작
            return values != null && // 배열 확인
                   !string.IsNullOrEmpty(value) && // 값 확인
                   Array.IndexOf(values, value) >= 0; // 포함 여부 반환
        } // 확인 끝

        private static void AddId(ref string[] values, string value) // ID 추가
        { // 추가 시작
            if (string.IsNullOrEmpty(value) || Contains(values, value)) // 값과 중복 확인
            { // 중단 시작
                return; // 추가 중단
            } // 중단 끝

            string[] current = values ?? new string[0]; // 현재 배열
            string[] grown = new string[current.Length + 1]; // 확장 배열 생성
            Array.Copy(current, grown, current.Length); // 기존 값 복사
            grown[grown.Length - 1] = value; // 신규 값 추가
            values = grown; // 확장 배열 저장
        } // 추가 끝

        private static string[] NormalizeIds(string[] values) // ID 배열 보정
        { // 보정 시작
            List<string> result = new List<string>(); // 결과 목록
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal); // 중복 확인 집합

            if (values != null) // 배열 확인
            { // 순회 준비
                foreach (string value in values) // 값 순회
                { // 순회 시작
                    if (!string.IsNullOrEmpty(value) && seen.Add(value)) // 유효 값 확인
                    { // 추가 시작
                        result.Add(value); // 결과 추가
                    } // 추가 끝
                } // 순회 끝
            } // 순회 준비 끝

            return result.ToArray(); // 보정 배열 반환
        } // 보정 끝

        private static StoryChoiceRecord[] NormalizeChoices(StoryChoiceRecord[] values) // 선택 배열 보정
        { // 보정 시작
            List<StoryChoiceRecord> result = new List<StoryChoiceRecord>(); // 결과 목록

            if (values == null) // 배열 확인
            { // 기본값 시작
                return result.ToArray(); // 빈 배열 반환
            } // 기본값 끝

            foreach (StoryChoiceRecord value in values) // 선택 순회
            { // 순회 시작
                if (string.IsNullOrEmpty(value.StoryId) || string.IsNullOrEmpty(value.ChoiceId)) // 유효성 확인
                { // 제외 시작
                    continue; // 다음 선택 이동
                } // 제외 끝

                int existing = result.FindIndex(item => string.Equals(item.StoryId, value.StoryId, StringComparison.Ordinal)); // 기존 장면 조회

                if (existing >= 0) // 기존 선택 확인
                { // 교체 시작
                    result[existing] = value; // 최신 선택 교체
                } // 교체 끝
                else // 신규 선택 처리
                { // 추가 시작
                    result.Add(value); // 선택 추가
                } // 추가 끝
            } // 순회 끝

            return result.ToArray(); // 보정 배열 반환
        } // 보정 끝
    } // 클래스 끝

    public static class StoryPlaybackLogic // 이야기 재생 규칙
    { // 클래스 시작
        public static int GetSkipStopIndex( // 건너뛰기 정지 위치 조회
            IList<StoryScene> scenes, // 장면 목록
            int currentIndex) // 현재 장면 번호
        { // 조회 시작
            if (scenes == null) // 장면 목록 확인
            { // 없음 시작
                return -1; // 선택 장면 없음
            } // 없음 끝

            for (int i = Math.Max(0, currentIndex); i < scenes.Count; i++) // 남은 장면 순회
            { // 순회 시작
                StoryScene scene = scenes[i]; // 장면 조회

                if (scene != null && scene.Choices.Length > 0) // 선택지 존재 확인
                { // 반환 시작
                    return i; // 선택 장면 번호 반환
                } // 반환 끝
            } // 순회 끝

            return -1; // 선택 장면 없음
        } // 조회 끝
    } // 클래스 끝
} // 공간 끝
