using System;

namespace ProjectTheta.Boss
{
    public enum EndingId // 엔딩 식별값
    { // 열거 시작
        None = 0, // 엔딩 없음
        A = 1, // 독립 엔딩
        B = 2, // 동행 엔딩
        C = 3 // 패배 엔딩
    } // 열거 끝

    public enum EndingDecision // 엔딩 분기 결과
    { // 열거 시작
        None = 0, // 엔딩 없음
        EndingA = 1, // 엔딩 A 확정
        EndingB = 2, // 엔딩 B 확정
        EndingC = 3, // 엔딩 C 확정
        ChoiceAOrB = 4 // 엔딩 A/B 선택
    } // 열거 끝

    public static class EndingBranchLogic // 엔딩 분기 규칙
    { // 클래스 시작
        public const int ChoiceAffinity = 80; // 선택 해금 호감도

        public static EndingDecision Resolve( // 엔딩 분기 판정
            bool isFinalBattle, // 최종전 여부
            bool bossDefeated, // 보스 승리 여부
            bool abandoned, // 포기 여부
            int riellaAffinity) // 리엘라 호감도
        { // 판정 시작
            if (!isFinalBattle || abandoned) // 엔딩 제외 조건 확인
            { // 제외 시작
                return EndingDecision.None; // 엔딩 없음 반환
            } // 제외 끝

            if (!bossDefeated) // 최종전 패배 확인
            { // 패배 시작
                return EndingDecision.EndingC; // 엔딩 C 반환
            } // 패배 끝

            return riellaAffinity >= ChoiceAffinity // 선택 기준 확인
                ? EndingDecision.ChoiceAOrB // A/B 선택 반환
                : EndingDecision.EndingA; // 엔딩 A 반환
        } // 판정 끝

        public static EndingId GetDefaultEnding(EndingDecision decision) // 확정 엔딩 조회
        { // 조회 시작
            switch (decision) // 분기 종류 확인
            { // 분기 시작
                case EndingDecision.EndingA: // 엔딩 A 확정
                case EndingDecision.ChoiceAOrB: // 선택 UI 대체값
                    return EndingId.A; // 엔딩 A 반환

                case EndingDecision.EndingB: // 엔딩 B 확정
                    return EndingId.B; // 엔딩 B 반환

                case EndingDecision.EndingC: // 엔딩 C 확정
                    return EndingId.C; // 엔딩 C 반환

                default: // 엔딩 없음
                    return EndingId.None; // 없음 반환
            } // 분기 끝
        } // 조회 끝
    } // 클래스 끝

    /// <summary>
    /// 보스 함락 뒤 엔딩 연출의 시간표다 (29일차). 화면 없이 테스트할 수 있게 숫자만 다룬다.
    ///
    ///   0.0  함락 연출(빛기둥 · 흔들림)을 그대로 보여 준다
    ///   1.2  화면이 어두워진다
    ///   2.0  "ENDING" 제목, 이어서 문장이 한 줄씩 떠오른다
    ///   …    다 뜬 뒤 잠깐 머문다
    ///   끝   어둠이 걷히고 결과 화면이 나온다
    ///
    /// 문장이 뜨기 시작한 뒤에 클릭하면 바로 걷히는 단계로 건너뛴다.
    /// </summary>
    public static class EndingLogic
    {
        public const float WaitSeconds = 1.2f;
        public const float FadeInSeconds = 0.8f;
        public const float LineSeconds = 1.4f;
        public const float LineFadeSeconds = 0.5f;
        public const float HoldSeconds = 1.6f;
        public const float FadeOutSeconds = 0.6f;
        public const float BackdropAlpha = 0.88f;

        public const string Title = "ENDING A"; // 기존 호환 제목

        private static readonly string[] EndingALines = // 엔딩 A 문장
        { // 배열 시작
            "라이벌 서큐버스가 무릎을 꿇었다.", // 승리 결과
            "주인공은 계약에 기대지 않고 도시의 밤을 스스로 선택했다.", // 독립 선택
            "리엘라는 그 선택을 존중하며 각자의 길을 지켜보기로 했다.", // 관계 결론
            "서로 다른 길 끝에서도 두 사람의 약속은 남았다." // 약속 유지
        }; // 배열 끝

        private static readonly string[] EndingBLines = // 엔딩 B 문장
        { // 배열 시작
            "라이벌 서큐버스가 무릎을 꿇었다.", // 승리 결과
            "주인공과 리엘라는 계약서를 내려놓고 서로의 선택을 확인했다.", // 계약 해소
            "도시의 흐트러진 연결을 함께 바로잡는 긴 밤이 시작됐다.", // 공동 책임
            "이번에는 계약이 아니라 두 사람의 의지로 같은 길을 걸었다." // 동행 결말
        }; // 배열 끝

        private static readonly string[] EndingCLines = // 엔딩 C 문장
        { // 배열 시작
            "루프탑의 붉은 금빛 계약망을 끝내 끊지 못했다.", // 패배 결과
            "도시의 정기 흐름은 르미아에게 기울고 군중의 기억은 흐려졌다.", // 도시 변화
            "리엘라는 마지막 힘으로 주인공을 안전한 곳까지 돌려보냈다.", // 귀환 결과
            "끝나지 않은 계약을 되찾기 위한 다음 밤이 남았다." // 재도전 여지
        }; // 배열 끝

        public static readonly string[] Lines = EndingALines; // 기존 호환 문장

        public static float LinesStart =>
            WaitSeconds + FadeInSeconds;

        public static float FadeOutStart =>
            LinesStart + Lines.Length * LineSeconds + HoldSeconds;

        public static float TotalSeconds =>
            FadeOutStart + FadeOutSeconds;

        public static string GetTitle(EndingId endingId) // 엔딩 제목 조회
        { // 조회 시작
            switch (endingId) // 엔딩 확인
            { // 분기 시작
                case EndingId.B: // 엔딩 B
                    return "ENDING B"; // 제목 반환

                case EndingId.C: // 엔딩 C
                    return "ENDING C"; // 제목 반환

                case EndingId.A: // 엔딩 A
                default: // 호환 기본값
                    return "ENDING A"; // 제목 반환
            } // 분기 끝
        } // 조회 끝

        public static string[] GetLines(EndingId endingId) // 엔딩 문장 조회
        { // 조회 시작
            switch (endingId) // 엔딩 확인
            { // 분기 시작
                case EndingId.B: // 엔딩 B
                    return EndingBLines; // B 문장 반환

                case EndingId.C: // 엔딩 C
                    return EndingCLines; // C 문장 반환

                case EndingId.A: // 엔딩 A
                default: // 호환 기본값
                    return EndingALines; // A 문장 반환
            } // 분기 끝
        } // 조회 끝

        /// <summary>검은 막의 불투명도다.</summary>
        public static float GetBackdropAlpha(
            float elapsed)
        {
            if (elapsed <= WaitSeconds)
            {
                return 0f;
            }

            if (elapsed < LinesStart)
            {
                return BackdropAlpha * Clamp01((elapsed - WaitSeconds) / FadeInSeconds);
            }

            if (elapsed < FadeOutStart)
            {
                return BackdropAlpha;
            }

            return BackdropAlpha * (1f - Clamp01((elapsed - FadeOutStart) / FadeOutSeconds));
        }

        /// <summary>
        /// 글자의 불투명도다. index −1은 제목, 0부터는 문장이다.
        /// 제목은 문장 단계가 시작될 때 함께 뜬다.
        /// </summary>
        public static float GetTextAlpha(
            float elapsed,
            int index)
        {
            float appear =
                LinesStart +
                Math.Max(0, index) * LineSeconds;

            float fadeIn = Clamp01((elapsed - appear) / LineFadeSeconds);

            if (elapsed < FadeOutStart)
            {
                return fadeIn;
            }

            return fadeIn * (1f - Clamp01((elapsed - FadeOutStart) / FadeOutSeconds));
        }

        public static bool IsFinished(
            float elapsed)
        {
            return elapsed >= TotalSeconds;
        }

        /// <summary>건너뛸 수 있는 때인지다. 문장이 뜨기 시작한 뒤, 걷히기 전이다.</summary>
        public static bool CanSkip(
            float elapsed)
        {
            return elapsed >= LinesStart &&
                   elapsed < FadeOutStart;
        }

        /// <summary>건너뛰면 걷히는 단계 처음으로 간다. 이미 지났으면 그대로다.</summary>
        public static float Skip(
            float elapsed)
        {
            return CanSkip(elapsed)
                ? FadeOutStart
                : elapsed;
        }

        private static float Clamp01(
            float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
