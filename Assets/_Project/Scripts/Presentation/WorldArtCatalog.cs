using System; // 수학 보정 도구 참조
using ProjectTheta.Disruptors; // 방해 적 종류 참조
using ProjectTheta.Stage.Locations; // 지역 식별자 참조

namespace ProjectTheta.Presentation // 화면 표현 공간
{ // 공간 시작
    public static class WorldArtCatalog // 월드 아트 경로 규칙
    { // 클래스 시작
        private const string CivilianRoot = "Characters/NPC/Civilian"; // 시민 정식 루트
        private const string CivilianLegacyRoot = "Characters/NPC_Female"; // 시민 기존 루트
        private const string DisruptorRoot = "Characters/Disruptors"; // 방해 적 정식 루트
        private const string MapRoot = "Maps"; // 지역 이미지 루트
        private const string IntroRoot = "Story/Intro"; // 프롤로그 이미지 루트

        public static string[] GetCivilianIdlePaths() // 시민 대기 경로 조회
        { // 조회 시작
            return new[] // 후보 경로 반환
            { // 목록 시작
                $"{CivilianRoot}/Idle", // 정식 대기 경로
                $"{CivilianLegacyRoot}/Idle" // 기존 대기 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetCivilianMovementPaths( // 시민 이동 경로 조회
            int frame) // 프레임 번호
        { // 조회 시작
            int safeFrame = Math.Max(0, frame); // 안전 프레임 보정

            return new[] // 후보 경로 반환
            { // 목록 시작
                $"{CivilianRoot}/Walk_{safeFrame}", // 정식 걷기 경로
                $"{CivilianRoot}/Move_{safeFrame}", // 정식 이동 호환 경로
                $"{CivilianLegacyRoot}/Walk_{safeFrame}", // 기존 걷기 경로
                $"{CivilianLegacyRoot}/Move_{safeFrame}" // 기존 이동 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetDisruptorIdlePaths( // 방해 적 대기 경로 조회
            DisruptorKind kind, // 방해 적 종류
            string legacyRoot) // 기존 리소스 루트
        { // 조회 시작
            string canonicalRoot = GetDisruptorRoot(kind); // 정식 루트 생성
            string safeLegacyRoot = NormalizeLegacyRoot(legacyRoot); // 기존 루트 정리

            if (string.IsNullOrEmpty(safeLegacyRoot)) // 기존 루트 없음 확인
            { // 정식 경로 반환 시작
                return new[] { $"{canonicalRoot}/Idle" }; // 정식 대기 경로 반환
            } // 정식 경로 반환 끝

            return new[] // 후보 경로 반환
            { // 목록 시작
                $"{canonicalRoot}/Idle", // 정식 대기 경로
                $"{safeLegacyRoot}/Idle" // 기존 대기 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetDisruptorMovementPaths( // 방해 적 이동 경로 조회
            DisruptorKind kind, // 방해 적 종류
            string legacyRoot, // 기존 리소스 루트
            int frame) // 프레임 번호
        { // 조회 시작
            string canonicalRoot = GetDisruptorRoot(kind); // 정식 루트 생성
            string safeLegacyRoot = NormalizeLegacyRoot(legacyRoot); // 기존 루트 정리
            int safeFrame = Math.Max(0, frame); // 안전 프레임 보정

            if (string.IsNullOrEmpty(safeLegacyRoot)) // 기존 루트 없음 확인
            { // 정식 경로 반환 시작
                return new[] // 정식 후보 반환
                { // 목록 시작
                    $"{canonicalRoot}/Walk_{safeFrame}", // 정식 걷기 경로
                    $"{canonicalRoot}/Move_{safeFrame}" // 정식 이동 호환 경로
                }; // 목록 끝
            } // 정식 경로 반환 끝

            return new[] // 후보 경로 반환
            { // 목록 시작
                $"{canonicalRoot}/Walk_{safeFrame}", // 정식 걷기 경로
                $"{canonicalRoot}/Move_{safeFrame}", // 정식 이동 호환 경로
                $"{safeLegacyRoot}/Walk_{safeFrame}", // 기존 걷기 경로
                $"{safeLegacyRoot}/Move_{safeFrame}" // 기존 이동 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetLocationPreviewPaths( // 지역 미리보기 경로 조회
            LocationId location) // 지역 식별자
        { // 조회 시작
            string root = $"{MapRoot}/{location}"; // 지역 루트 생성

            return new[] // 후보 경로 반환
            { // 목록 시작
                $"{root}/Preview", // 전용 미리보기 경로
                $"{root}/Background" // 지역 배경 폴백 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetIntroCgPaths( // 프롤로그 CG 경로 조회
            int pageIndex) // 0 기반 페이지 번호
        { // 조회 시작
            int safePageIndex = Math.Max(0, Math.Min(4, pageIndex)); // 페이지 범위 보정
            int fileNumber = safePageIndex + 1; // 1 기반 파일 번호 변환

            return new[] { $"{IntroRoot}/Page_{fileNumber:00}" }; // CG 경로 반환
        } // 조회 끝

        private static string GetDisruptorRoot( // 방해 적 정식 루트 생성
            DisruptorKind kind) // 방해 적 종류
        { // 생성 시작
            return $"{DisruptorRoot}/{kind}"; // 종류별 정식 루트 반환
        } // 생성 끝

        private static string NormalizeLegacyRoot( // 기존 루트 정리
            string legacyRoot) // 기존 루트 문자열
        { // 정리 시작
            if (string.IsNullOrWhiteSpace(legacyRoot)) // 빈 문자열 확인
            { // 빈 문자열 처리 시작
                return string.Empty; // 빈 루트 반환
            } // 빈 문자열 처리 끝

            return legacyRoot.Trim().TrimEnd('/'); // 공백과 끝 구분자 제거
        } // 정리 끝
    } // 클래스 끝
} // 공간 끝
