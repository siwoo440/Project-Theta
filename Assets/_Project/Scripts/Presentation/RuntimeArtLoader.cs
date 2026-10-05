using System.Collections.Generic; // 캐시 자료 구조 참조
using System.Globalization; // 숫자 키 형식 참조
using UnityEngine; // Unity 리소스 참조

namespace ProjectTheta.Presentation // 화면 표현 공간
{ // 공간 시작
    public static class RuntimeArtLoader // 런타임 아트 로더
    { // 클래스 시작
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>(); // 스프라이트 캐시
        private static readonly HashSet<string> MissingPaths = new HashSet<string>(); // 누락 경로 집합
        private static readonly List<Sprite> CreatedSprites = new List<Sprite>(); // 생성 스프라이트 목록

        public static Sprite LoadFirst( // 첫 유효 스프라이트 로드
            string[] resourcePaths, // 후보 리소스 경로
            Vector2 pivot, // 스프라이트 피벗
            float pixelsPerUnit, // 단위당 픽셀 수
            out string loadedPath) // 실제 로드 경로
        { // 로드 시작
            loadedPath = string.Empty; // 실제 경로 초기화

            if (resourcePaths == null) // 후보 목록 없음 확인
            { // 빈 후보 처리 시작
                return null; // 로드 실패 반환
            } // 빈 후보 처리 끝

            float safePixelsPerUnit = Mathf.Max(1f, pixelsPerUnit); // 픽셀 단위 보정

            for (int i = 0; i < resourcePaths.Length; i++) // 후보 경로 순회
            { // 순회 시작
                string path = resourcePaths[i]; // 현재 경로 조회

                if (string.IsNullOrWhiteSpace(path) || MissingPaths.Contains(path)) // 무효 또는 누락 경로 확인
                { // 건너뛰기 시작
                    continue; // 다음 후보 이동
                } // 건너뛰기 끝

                string cacheKey = BuildCacheKey(path, pivot, safePixelsPerUnit); // 캐시 키 생성

                if (SpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null) // 캐시 적중 확인
                { // 캐시 반환 시작
                    loadedPath = path; // 실제 경로 설정

                    return cachedSprite; // 캐시 스프라이트 반환
                } // 캐시 반환 끝

                Sprite sprite = Resources.Load<Sprite>(path); // 스프라이트 리소스 조회

                if (sprite == null) // 직접 스프라이트 없음 확인
                { // 텍스처 폴백 시작
                    Texture2D texture = Resources.Load<Texture2D>(path); // 텍스처 리소스 조회

                    if (texture != null) // 텍스처 존재 확인
                    { // 스프라이트 생성 시작
                        sprite = Sprite.Create( // 스프라이트 생성
                            texture, // 원본 텍스처 지정
                            new Rect(0f, 0f, texture.width, texture.height), // 전체 영역 지정
                            pivot, // 피벗 지정
                            safePixelsPerUnit); // 픽셀 단위 지정
                        CreatedSprites.Add(sprite); // 생성 목록 등록
                    } // 스프라이트 생성 끝
                } // 텍스처 폴백 끝

                if (sprite == null) // 최종 로드 실패 확인
                { // 누락 기록 시작
                    MissingPaths.Add(path); // 누락 경로 등록

                    continue; // 다음 후보 이동
                } // 누락 기록 끝

                SpriteCache[cacheKey] = sprite; // 캐시 등록
                loadedPath = path; // 실제 경로 설정

                return sprite; // 로드 성공 반환
            } // 순회 끝

            return null; // 전체 로드 실패 반환
        } // 로드 끝

        public static Sprite LoadFirstNormalized( // 정규화 스프라이트 로드
            string[] resourcePaths, // 후보 리소스 경로
            Vector2 pivot, // 스프라이트 피벗
            float pixelsPerUnit, // 기준 단위당 픽셀 수
            float referenceHeightPixels, // 기준 텍스처 높이
            out string loadedPath) // 실제 로드 경로
        { // 로드 시작
            loadedPath = string.Empty; // 실제 경로 초기화

            if (resourcePaths == null) // 후보 목록 없음 확인
            { // 빈 후보 처리 시작
                return null; // 로드 실패 반환
            } // 빈 후보 처리 끝

            float safePixelsPerUnit = Mathf.Max(1f, pixelsPerUnit); // 픽셀 단위 보정
            float safeReferenceHeight = Mathf.Max(1f, referenceHeightPixels); // 기준 높이 보정

            for (int i = 0; i < resourcePaths.Length; i++) // 후보 경로 순회
            { // 순회 시작
                string path = resourcePaths[i]; // 현재 경로 조회

                if (string.IsNullOrWhiteSpace(path) || MissingPaths.Contains(path)) // 무효 또는 누락 경로 확인
                { // 건너뛰기 시작
                    continue; // 다음 후보 이동
                } // 건너뛰기 끝

                Sprite importedSprite = Resources.Load<Sprite>(path); // 임포트 스프라이트 우선 조회
                Texture2D texture = importedSprite == null // 임포트 결과 확인
                    ? Resources.Load<Texture2D>(path) // 텍스처 폴백 조회
                    : importedSprite.texture; // 전체 원본 텍스처 조회

                if (texture == null) // 원본 텍스처 없음 확인
                { // 누락 처리 시작
                    MissingPaths.Add(path); // 누락 경로 등록

                    continue; // 다음 후보 이동
                } // 누락 처리 끝

                float normalizedPixelsPerUnit = CalculateNormalizedPixelsPerUnit( // 정규화 픽셀 단위 계산
                    texture.height, // 실제 텍스처 높이
                    safeReferenceHeight, // 기준 텍스처 높이
                    safePixelsPerUnit); // 기준 픽셀 단위
                string cacheKey = string.Concat( // 전체 텍스처 캐시 키 생성
                    BuildCacheKey(path, pivot, normalizedPixelsPerUnit), // 공통 캐시 키 조각
                    "|full"); // 전체 텍스처 구분자

                if (SpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null) // 캐시 적중 확인
                { // 캐시 반환 시작
                    loadedPath = path; // 실제 경로 설정

                    return cachedSprite; // 캐시 스프라이트 반환
                } // 캐시 반환 끝

                Sprite sprite = Sprite.Create( // 전체 텍스처 스프라이트 생성
                    texture, // 원본 텍스처 지정
                    new Rect(0f, 0f, texture.width, texture.height), // 전체 영역 지정
                    pivot, // 피벗 지정
                    normalizedPixelsPerUnit); // 정규화 픽셀 단위 지정
                sprite.name = path.Replace('/', '_') + "_Normalized"; // 정규화 이름 지정
                CreatedSprites.Add(sprite); // 생성 목록 등록
                SpriteCache[cacheKey] = sprite; // 캐시 등록
                loadedPath = path; // 실제 경로 설정

                return sprite; // 로드 성공 반환
            } // 순회 끝

            return null; // 전체 로드 실패 반환
        } // 로드 끝

        public static float CalculateNormalizedPixelsPerUnit( // 정규화 픽셀 단위 계산
            float sourceHeightPixels, // 실제 텍스처 높이
            float referenceHeightPixels, // 기준 텍스처 높이
            float pixelsPerUnit) // 기준 단위당 픽셀 수
        { // 계산 시작
            float safeSourceHeight = Mathf.Max(1f, sourceHeightPixels); // 실제 높이 보정
            float safeReferenceHeight = Mathf.Max(1f, referenceHeightPixels); // 기준 높이 보정
            float safePixelsPerUnit = Mathf.Max(1f, pixelsPerUnit); // 픽셀 단위 보정

            return safePixelsPerUnit * safeSourceHeight / safeReferenceHeight; // 동일 월드 높이 반환
        } // 계산 끝

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 런타임 초기화 표시
        private static void ResetCaches() // 런타임 캐시 초기화
        { // 초기화 시작
            for (int i = 0; i < CreatedSprites.Count; i++) // 생성 스프라이트 순회
            { // 순회 시작
                if (CreatedSprites[i] != null) // 생성 스프라이트 존재 확인
                { // 제거 시작
                    Object.Destroy(CreatedSprites[i]); // 생성 스프라이트 제거
                } // 제거 끝
            } // 순회 끝

            CreatedSprites.Clear(); // 생성 목록 초기화
            SpriteCache.Clear(); // 스프라이트 캐시 초기화
            MissingPaths.Clear(); // 누락 경로 초기화
        } // 초기화 끝

        private static string BuildCacheKey( // 캐시 키 생성
            string path, // 리소스 경로
            Vector2 pivot, // 스프라이트 피벗
            float pixelsPerUnit) // 단위당 픽셀 수
        { // 생성 시작
            return string.Concat( // 불변 형식 키 반환
                path, // 경로 조각
                "|", // 구분자
                pivot.x.ToString("R", CultureInfo.InvariantCulture), // 피벗 X 조각
                "|", // 구분자
                pivot.y.ToString("R", CultureInfo.InvariantCulture), // 피벗 Y 조각
                "|", // 구분자
                pixelsPerUnit.ToString("R", CultureInfo.InvariantCulture)); // 픽셀 단위 조각
        } // 생성 끝
    } // 클래스 끝
} // 공간 끝
