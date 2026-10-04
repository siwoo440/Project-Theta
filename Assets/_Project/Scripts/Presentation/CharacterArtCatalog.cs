using System; // 문자열 비교 도구 참조

namespace ProjectTheta.Presentation // 화면 표현 공간
{ // 공간 시작
    public enum CharacterArtId // 주요 캐릭터 식별자
    { // 열거 시작
        Protagonist = 0, // 주인공
        Riella = 1, // 리엘라
        Lumia = 2 // 르미아
    } // 열거 끝

    public enum CharacterArtExpression // 대화 초상 표정
    { // 열거 시작
        Default = 0, // 기본 표정
        Focused = 1, // 집중 표정
        Embarrassed = 2, // 당황 표정
        Angry = 3, // 분노 표정
        Tired = 4, // 피로 표정
        Work = 5, // 업무 표정
        Playful = 6, // 장난 표정
        Worried = 7, // 걱정 표정
        Jealous = 8, // 질투 표정
        Weakened = 9, // 약화 표정
        Sincere = 10, // 진심 표정
        Interested = 11, // 흥미 표정
        Provoking = 12, // 도발 표정
        Defeated = 13, // 패배 표정
        Ending = 14 // 엔딩 표정
    } // 열거 끝

    public enum CharacterArtAction // 주요 행동 식별자
    { // 열거 시작
        Hypnosis = 0, // 최면 시전
        ContractMagic = 1, // 계약 마법
        ReverseHypnosis = 2, // 역최면 시전
        Victory = 3, // 승리 행동
        Defeat = 4 // 패배 행동
    } // 열거 끝

    public sealed class CharacterArtDefinition // 캐릭터 리소스 정의
    { // 클래스 시작
        public readonly CharacterArtId Id; // 캐릭터 ID
        public readonly string ResourceName; // 리소스 폴더명
        public readonly string LegacyRoot; // 기존 임시 경로
        public readonly CharacterArtExpression[] Expressions; // 지원 표정 목록

        public CharacterArtDefinition( // 리소스 정의 생성
            CharacterArtId id, // 캐릭터 ID
            string resourceName, // 리소스 폴더명
            string legacyRoot, // 기존 임시 경로
            CharacterArtExpression[] expressions) // 지원 표정 목록
        { // 생성 시작
            Id = id; // ID 저장
            ResourceName = resourceName; // 폴더명 저장
            LegacyRoot = legacyRoot; // 기존 경로 저장
            Expressions = expressions ?? Array.Empty<CharacterArtExpression>(); // 표정 목록 저장
        } // 생성 끝
    } // 클래스 끝

    public static class CharacterArtCatalog // 주요 캐릭터 리소스 규칙
    { // 클래스 시작
        private const string CharacterRoot = "Characters"; // 게임 스프라이트 루트
        private const string PortraitRoot = "Portraits"; // 대화 초상 루트

        public static readonly CharacterArtDefinition[] All = // 전체 주요 캐릭터
        { // 목록 시작
            new CharacterArtDefinition( // 주인공 정의
                CharacterArtId.Protagonist, // 주인공 ID
                "Protagonist", // 정식 폴더명
                "Characters/Player", // 기존 임시 폴더
                new[] // 지원 표정 목록
                { // 목록 시작
                    CharacterArtExpression.Default, // 기본 표정
                    CharacterArtExpression.Focused, // 집중 표정
                    CharacterArtExpression.Embarrassed, // 당황 표정
                    CharacterArtExpression.Angry, // 분노 표정
                    CharacterArtExpression.Tired // 피로 표정
                }), // 주인공 정의 끝
            new CharacterArtDefinition( // 리엘라 정의
                CharacterArtId.Riella, // 리엘라 ID
                "Riella", // 정식 폴더명
                "Characters/Succubus", // 기존 임시 폴더
                new[] // 지원 표정 목록
                { // 목록 시작
                    CharacterArtExpression.Default, // 기본 표정
                    CharacterArtExpression.Work, // 업무 표정
                    CharacterArtExpression.Playful, // 장난 표정
                    CharacterArtExpression.Worried, // 걱정 표정
                    CharacterArtExpression.Jealous, // 질투 표정
                    CharacterArtExpression.Angry, // 분노 표정
                    CharacterArtExpression.Weakened, // 약화 표정
                    CharacterArtExpression.Sincere // 진심 표정
                }), // 리엘라 정의 끝
            new CharacterArtDefinition( // 르미아 정의
                CharacterArtId.Lumia, // 르미아 ID
                "Lumia", // 정식 폴더명
                "Characters/NPC_Female", // 기존 임시 폴더
                new[] // 지원 표정 목록
                { // 목록 시작
                    CharacterArtExpression.Default, // 기본 표정
                    CharacterArtExpression.Interested, // 흥미 표정
                    CharacterArtExpression.Provoking, // 도발 표정
                    CharacterArtExpression.Angry, // 분노 표정
                    CharacterArtExpression.Sincere, // 진심 표정
                    CharacterArtExpression.Weakened, // 약화 표정
                    CharacterArtExpression.Defeated, // 패배 표정
                    CharacterArtExpression.Ending // 엔딩 표정
                }) // 르미아 정의 끝
        }; // 전체 목록 끝

        public static CharacterArtDefinition Get( // 캐릭터 정의 조회
            CharacterArtId id) // 캐릭터 ID
        { // 조회 시작
            for (int i = 0; i < All.Length; i++) // 전체 정의 순회
            { // 순회 시작
                if (All[i].Id == id) // ID 일치 확인
                { // 일치 시작
                    return All[i]; // 정의 반환
                } // 일치 끝
            } // 순회 끝

            return All[0]; // 안전 기본 정의 반환
        } // 조회 끝

        public static bool TryGetCharacterBySpeaker( // 화자 캐릭터 조회
            string speaker, // 화자 이름
            out CharacterArtId id) // 캐릭터 ID
        { // 조회 시작
            if (string.Equals(speaker, "주인공", StringComparison.Ordinal) || // 주인공 이름 확인
                string.Equals(speaker, "나", StringComparison.Ordinal)) // 주인공 별칭 확인
            { // 주인공 일치 시작
                id = CharacterArtId.Protagonist; // 주인공 ID 설정

                return true; // 조회 성공 반환
            } // 주인공 일치 끝

            if (string.Equals(speaker, "리엘라", StringComparison.Ordinal)) // 리엘라 이름 확인
            { // 리엘라 일치 시작
                id = CharacterArtId.Riella; // 리엘라 ID 설정

                return true; // 조회 성공 반환
            } // 리엘라 일치 끝

            if (string.Equals(speaker, "르미아", StringComparison.Ordinal) || // 르미아 이름 확인
                string.Equals(speaker, "라이벌", StringComparison.Ordinal)) // 르미아 별칭 확인
            { // 르미아 일치 시작
                id = CharacterArtId.Lumia; // 르미아 ID 설정

                return true; // 조회 성공 반환
            } // 르미아 일치 끝

            id = CharacterArtId.Protagonist; // 안전 기본 ID 설정

            return false; // 조회 실패 반환
        } // 조회 끝

        public static string[] GetIdlePaths( // 대기 스프라이트 경로 조회
            CharacterArtId id) // 캐릭터 ID
        { // 조회 시작
            CharacterArtDefinition definition = Get(id); // 캐릭터 정의 조회
            string root = GetCanonicalRoot(definition); // 정식 루트 생성

            return new[] // 경로 목록 반환
            { // 목록 시작
                $"{root}/Idle", // 정식 대기 경로
                $"{definition.LegacyRoot}/Idle" // 기존 대기 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetMovementPaths( // 이동 스프라이트 경로 조회
            CharacterArtId id, // 캐릭터 ID
            int frame) // 프레임 번호
        { // 조회 시작
            CharacterArtDefinition definition = Get(id); // 캐릭터 정의 조회
            string root = GetCanonicalRoot(definition); // 정식 루트 생성
            int safeFrame = Math.Max(0, frame); // 안전 프레임 보정

            return new[] // 경로 목록 반환
            { // 목록 시작
                $"{root}/Walk_{safeFrame}", // 정식 걷기 경로
                $"{root}/Move_{safeFrame}", // 정식 호환 경로
                $"{definition.LegacyRoot}/Walk_{safeFrame}", // 기존 걷기 경로
                $"{definition.LegacyRoot}/Move_{safeFrame}" // 기존 이동 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetPortraitPaths( // 대화 초상 경로 조회
            CharacterArtId id, // 캐릭터 ID
            CharacterArtExpression expression) // 요청 표정
        { // 조회 시작
            CharacterArtDefinition definition = Get(id); // 캐릭터 정의 조회
            string root = $"{PortraitRoot}/{definition.ResourceName}"; // 초상 루트 생성

            if (expression == CharacterArtExpression.Default || // 기본 표정 확인
                !SupportsExpression(definition, expression)) // 지원 여부 확인
            { // 기본 반환 시작
                return new[] { $"{root}/Default" }; // 기본 초상 반환
            } // 기본 반환 끝

            return new[] // 표정 경로 반환
            { // 목록 시작
                $"{root}/{expression}", // 요청 표정 경로
                $"{root}/Default" // 기본 초상 폴백
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetHubPaths( // 허브 리엘라 경로 조회
            bool seated) // 앉은 자세 여부
        { // 조회 시작
            CharacterArtDefinition definition = Get(CharacterArtId.Riella); // 리엘라 정의 조회
            string root = GetCanonicalRoot(definition); // 정식 루트 생성
            string pose = seated ? "Room_Sit" : "Room_Stand"; // 자세 파일명 선택

            return new[] // 경로 목록 반환
            { // 목록 시작
                $"{root}/{pose}", // 정식 자세 경로
                $"{definition.LegacyRoot}/{pose}", // 기존 자세 경로
                $"{root}/Idle", // 정식 기본 경로
                $"{definition.LegacyRoot}/Idle" // 기존 기본 경로
            }; // 목록 끝
        } // 조회 끝

        public static string[] GetActionPaths( // 주요 행동 경로 조회
            CharacterArtId id, // 캐릭터 ID
            CharacterArtAction action) // 행동 ID
        { // 조회 시작
            CharacterArtDefinition definition = Get(id); // 캐릭터 정의 조회
            string root = GetCanonicalRoot(definition); // 정식 루트 생성

            return new[] // 경로 목록 반환
            { // 목록 시작
                $"{root}/Actions/{action}", // 정식 행동 폴더 경로
                $"{root}/{action}", // 단일 파일 호환 경로
                $"{definition.LegacyRoot}/{action}" // 기존 임시 경로
            }; // 목록 끝
        } // 조회 끝

        private static string GetCanonicalRoot( // 정식 게임 루트 생성
            CharacterArtDefinition definition) // 캐릭터 정의
        { // 생성 시작
            return $"{CharacterRoot}/{definition.ResourceName}"; // 정식 루트 반환
        } // 생성 끝

        private static bool SupportsExpression( // 표정 지원 확인
            CharacterArtDefinition definition, // 캐릭터 정의
            CharacterArtExpression expression) // 요청 표정
        { // 확인 시작
            for (int i = 0; i < definition.Expressions.Length; i++) // 지원 목록 순회
            { // 순회 시작
                if (definition.Expressions[i] == expression) // 표정 일치 확인
                { // 일치 시작
                    return true; // 지원 반환
                } // 일치 끝
            } // 순회 끝

            return false; // 미지원 반환
        } // 확인 끝
    } // 클래스 끝
} // 공간 끝
