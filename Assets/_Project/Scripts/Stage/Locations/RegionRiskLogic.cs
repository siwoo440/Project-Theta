using System; // 기본 계산 참조
using ProjectTheta.Save; // 저장 자료 참조
using UnityEngine; // 유니티 초기화 참조

namespace ProjectTheta.Stage.Locations // 장소 규칙 공간
{ // 공간 시작
    public enum WorldTimeOfDay // 세계 시간대
    { // 열거 시작
        Morning = 0, // 아침
        Day = 1, // 낮
        Evening = 2, // 저녁
        Night = 3 // 밤
    } // 열거 끝

    public static class RegionRiskLogic // 지역 위험 계산
    { // 클래스 시작
        public const int RegionCount = 8; // 지역 수
        public const float MinimumMultiplier = 0.8f; // 최소 위험 배율
        public const float MaximumMultiplier = 1.4f; // 최대 위험 배율
        public const float NeutralMultiplier = 1f; // 중립 위험 배율

        public static float[] CreateNeutralMultipliers() // 중립 배율 생성
        { // 생성 시작
            return new[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }; // 중립 배율 반환
        } // 생성 끝

        public static float[] RollMultipliers(int seed) // 지역 배율 생성
        { // 생성 시작
            System.Random random = new System.Random(seed); // 시드 난수 생성
            float[] result = new float[RegionCount]; // 결과 배열 생성

            for (int i = 0; i < result.Length; i++) // 지역 순회
            { // 순회 시작
                double value = MinimumMultiplier + // 최소값 더하기
                               random.NextDouble() * // 난수 비율
                               (MaximumMultiplier - MinimumMultiplier); // 범위 적용
                result[i] = (float)Math.Round( // 소수 둘째 자리 반올림
                    value, // 원본 값
                    2, // 자릿수
                    MidpointRounding.AwayFromZero); // 반올림 방식
            } // 순회 끝

            return result; // 배율 반환
        } // 생성 끝

        public static bool ApplyStageTransition(SaveData save, StageResultSummary result, int seed) // 결과 전환 적용
        { // 전환 시작
            if (save == null || !result.Cleared) // 저장과 클리어 확인
            { // 중단 시작
                return false; // 전환 없음
            } // 중단 끝

            WorldTimeOfDay current = GetTime(save); // 현재 시간 조회
            save.CurrentWorldTime = (int)GetNextTime(current); // 다음 시간 저장
            save.RegionRiskMultipliers = RollMultipliers(seed); // 지역 배율 재추첨

            return true; // 전환 완료
        } // 전환 끝

        public static WorldTimeOfDay GetNextTime(WorldTimeOfDay current) // 다음 시간 계산
        { // 계산 시작
            switch (current) // 현재 시간 분기
            { // 분기 시작
                case WorldTimeOfDay.Morning: // 아침 처리
                    return WorldTimeOfDay.Day; // 낮 반환

                case WorldTimeOfDay.Day: // 낮 처리
                    return WorldTimeOfDay.Evening; // 저녁 반환

                case WorldTimeOfDay.Evening: // 저녁 처리
                    return WorldTimeOfDay.Night; // 밤 반환

                default: // 밤과 손상값 처리
                    return WorldTimeOfDay.Morning; // 아침 순환
            } // 분기 끝
        } // 계산 끝

        public static int NormalizeTimeValue(int value) // 시간 번호 보정
        { // 보정 시작
            return value >= (int)WorldTimeOfDay.Morning && // 최소 범위 확인
                   value <= (int)WorldTimeOfDay.Night // 최대 범위 확인
                ? value // 유효 값 반환
                : (int)WorldTimeOfDay.Morning; // 아침 기본값 반환
        } // 보정 끝

        public static WorldTimeOfDay GetTime(SaveData save) // 저장 시간 조회
        { // 조회 시작
            int value = save == null // 저장 확인
                ? (int)WorldTimeOfDay.Morning // 기본 아침
                : NormalizeTimeValue(save.CurrentWorldTime); // 저장값 보정

            return (WorldTimeOfDay)value; // 시간대 반환
        } // 조회 끝

        public static string GetTimeLabel(WorldTimeOfDay time) // 시간 이름 조회
        { // 조회 시작
            switch (time) // 시간 분기
            { // 분기 시작
                case WorldTimeOfDay.Day: // 낮 처리
                    return "낮"; // 낮 이름

                case WorldTimeOfDay.Evening: // 저녁 처리
                    return "저녁"; // 저녁 이름

                case WorldTimeOfDay.Night: // 밤 처리
                    return "밤"; // 밤 이름

                default: // 아침 처리
                    return "아침"; // 아침 이름
            } // 분기 끝
        } // 조회 끝

        public static LocationTimeOfDay ToLocationTime(WorldTimeOfDay time) // 배경 시간 변환
        { // 변환 시작
            switch (time) // 세계 시간 분기
            { // 분기 시작
                case WorldTimeOfDay.Evening: // 저녁 처리
                    return LocationTimeOfDay.Evening; // 저녁 배경 반환

                case WorldTimeOfDay.Night: // 밤 처리
                    return LocationTimeOfDay.Night; // 밤 배경 반환

                default: // 아침과 낮 처리
                    return LocationTimeOfDay.Day; // 낮 배경 반환
            } // 분기 끝
        } // 변환 끝

        public static float ClampMultiplier(float value) // 위험 배율 보정
        { // 보정 시작
            if (float.IsNaN(value) || float.IsInfinity(value)) // 비정상 값 확인
            { // 기본값 시작
                return NeutralMultiplier; // 중립값 반환
            } // 기본값 끝

            return Math.Max( // 최소값 적용
                MinimumMultiplier, // 최소 배율
                Math.Min( // 최대값 적용
                    MaximumMultiplier, // 최대 배율
                    value)); // 원본 배율
        } // 보정 끝

        public static float GetMultiplier(SaveData save, LocationId location) // 지역 배율 조회
        { // 조회 시작
            int index = (int)location; // 장소 번호 변환

            if (save == null || // 저장 확인
                save.RegionRiskMultipliers == null || // 배열 확인
                index < 0 || // 최소 번호 확인
                index >= save.RegionRiskMultipliers.Length) // 최대 번호 확인
            { // 기본값 시작
                return NeutralMultiplier; // 중립값 반환
            } // 기본값 끝

            float value = save.RegionRiskMultipliers[index]; // 저장 배율 조회

            return value <= 0f // 구버전 기본값 확인
                ? NeutralMultiplier // 중립값 반환
                : ClampMultiplier(value); // 배율 보정 반환
        } // 조회 끝

        public static int ApplyTargetEssence(int value, float multiplier) // 목표 정기 적용
        { // 계산 시작
            return RoundPositive( // 양수 반올림
                Math.Max(0, value) * // 안전 목표
                (double)ClampMultiplier(multiplier)); // 위험 배율
        } // 계산 끝

        public static float GetHypnosisSpeedMultiplier(float multiplier) // 최면 속도 적용
        { // 계산 시작
            return 1f / ClampMultiplier(multiplier); // 저항 역배율 반환
        } // 계산 끝

        public static float ApplyAlertAmount(float value, float multiplier) // 경계 상승 적용
        { // 계산 시작
            return value > 0f // 상승 여부 확인
                ? value * ClampMultiplier(multiplier) // 위험 배율 적용
                : value; // 감소량 보존
        } // 계산 끝

        public static float ApplyAbilityCooldown(float value, float multiplier) // 재사용 시간 적용
        { // 계산 시작
            return Math.Max(0f, value) / ClampMultiplier(multiplier); // 위험 역배율 적용
        } // 계산 끝

        public static int ApplyContractReward(int value, float multiplier) // 계약 보상 적용
        { // 계산 시작
            return RoundPositive( // 양수 반올림
                Math.Max(0, value) * // 안전 보상
                (double)ClampMultiplier(multiplier)); // 위험 배율
        } // 계산 끝

        public static string BuildMapSummary(SaveData save, LocationId location) // 지도 문구 생성
        { // 생성 시작
            WorldTimeOfDay time = GetTime(save); // 현재 시간 조회
            float risk = GetMultiplier(save, location); // 지역 위험 조회

            return $"현재 시간 {GetTimeLabel(time)}  ·  위험 ×{risk:0.00}"; // 지도 문구 반환
        } // 생성 끝

        private static int RoundPositive(double value) // 양수 반올림
        { // 계산 시작
            return Math.Max( // 최소값 적용
                0, // 최소 결과
                (int)Math.Round( // 정수 반올림
                    value, // 원본 값
                    MidpointRounding.AwayFromZero)); // 반올림 방식
        } // 계산 끝
    } // 클래스 끝

    public static class RegionRiskState // 현재 지역 위험 상태
    { // 클래스 시작
        public static WorldTimeOfDay CurrentTime { get; private set; } = // 현재 세계 시간
            WorldTimeOfDay.Morning; // 기본 아침

        public static float CurrentMultiplier { get; private set; } = // 현재 위험 배율
            RegionRiskLogic.NeutralMultiplier; // 기본 중립 배율

        public static void Configure( // 런타임 상태 연결
            SaveData save, // 저장 자료
            LocationId location) // 현재 장소
        { // 연결 시작
            CurrentTime = RegionRiskLogic.GetTime(save); // 시간 연결
            CurrentMultiplier = RegionRiskLogic.GetMultiplier(save, location); // 배율 연결
        } // 연결 끝

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 플레이 진입 초기화
        private static void ResetOnPlayModeEnter() // 상태 초기화
        { // 초기화 시작
            CurrentTime = WorldTimeOfDay.Morning; // 아침 복원
            CurrentMultiplier = RegionRiskLogic.NeutralMultiplier; // 중립 복원
        } // 초기화 끝
    } // 클래스 끝
} // 공간 끝
