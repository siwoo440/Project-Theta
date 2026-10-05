using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Core; // 런타임 애니메이터 참조
using ProjectTheta.Disruptors; // 방해 적 종류 참조
using ProjectTheta.Presentation; // 월드 아트 도구 참조
using ProjectTheta.Stage.Locations; // 지역 식별자 참조
using UnityEngine; // Unity 그래픽 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day48Tests // 48일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void WorldArtCatalog_ReturnsCanonicalPathsBeforeFallbacks() // 정식 경로 우선순위 검증
        { // 테스트 시작
            CollectionAssert.AreEqual( // 시민 대기 경로 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/NPC/Civilian/Idle", // 정식 시민 대기 경로
                    "Characters/NPC_Female/Idle" // 기존 시민 대기 경로
                }, // 목록 끝
                WorldArtCatalog.GetCivilianIdlePaths()); // 시민 대기 경로 조회
            CollectionAssert.AreEqual( // 방해 적 이동 경로 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/Disruptors/Lifeguard/Walk_2", // 정식 걷기 경로
                    "Characters/Disruptors/Lifeguard/Move_2", // 정식 이동 호환 경로
                    "Characters/Geumtaeyang/Walk_2", // 기존 걷기 경로
                    "Characters/Geumtaeyang/Move_2" // 기존 이동 경로
                }, // 목록 끝
                WorldArtCatalog.GetDisruptorMovementPaths( // 방해 적 이동 경로 조회
                    DisruptorKind.Lifeguard, // 인명 구조원 선택
                    "Characters/Geumtaeyang", // 기존 루트 지정
                    2)); // 세 번째 프레임 지정
            CollectionAssert.AreEqual( // 지역 미리보기 경로 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Maps/Beach/Preview", // 정식 미리보기 경로
                    "Maps/Beach/Background" // 배경 폴백 경로
                }, // 목록 끝
                WorldArtCatalog.GetLocationPreviewPaths(LocationId.Beach)); // 해변 경로 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void WorldArtCatalog_ClampsFramesAndIntroPages() // 입력 범위 보정 검증
        { // 테스트 시작
            CollectionAssert.AreEqual( // 음수 프레임 보정 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/NPC/Civilian/Walk_0", // 정식 보정 경로
                    "Characters/NPC/Civilian/Move_0", // 정식 호환 경로
                    "Characters/NPC_Female/Walk_0", // 기존 걷기 경로
                    "Characters/NPC_Female/Move_0" // 기존 이동 경로
                }, // 목록 끝
                WorldArtCatalog.GetCivilianMovementPaths(-5)); // 음수 프레임 조회
            CollectionAssert.AreEqual( // 첫 장 보정 확인
                new[] { "Story/Intro/Page_01" }, // 첫 장 예상 경로
                WorldArtCatalog.GetIntroCgPaths(-1)); // 음수 페이지 조회
            CollectionAssert.AreEqual( // 마지막 장 보정 확인
                new[] { "Story/Intro/Page_05" }, // 마지막 장 예상 경로
                WorldArtCatalog.GetIntroCgPaths(99)); // 초과 페이지 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void WorldArtCatalog_RemovesEmptyLegacyRoots() // 빈 기존 경로 제거 검증
        { // 테스트 시작
            CollectionAssert.AreEqual( // 대기 경로 수 확인
                new[] { "Characters/Disruptors/Dj/Idle" }, // 정식 대기 경로만 기대
                WorldArtCatalog.GetDisruptorIdlePaths(DisruptorKind.Dj, string.Empty)); // 빈 기존 루트 조회
            CollectionAssert.AreEqual( // 이동 경로 수 확인
                new[] // 예상 정식 경로 목록
                { // 목록 시작
                    "Characters/Disruptors/Dj/Walk_0", // 정식 걷기 경로
                    "Characters/Disruptors/Dj/Move_0" // 정식 이동 호환 경로
                }, // 목록 끝
                WorldArtCatalog.GetDisruptorMovementPaths( // 빈 기존 이동 경로 조회
                    DisruptorKind.Dj, // DJ 선택
                    null, // 기존 루트 없음
                    -1)); // 음수 프레임 지정
        } // 테스트 끝

        [Test] // 테스트 표시
        public void CivilianMovementPaths_PreserveEachCanonicalAndLegacyFrame() // 시민 프레임 폴백 검증
        { // 테스트 시작
            CollectionAssert.AreEqual( // 이동 경로 순서 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/NPC/Civilian/Walk_3", // 정식 걷기 경로
                    "Characters/NPC/Civilian/Move_3", // 정식 이동 호환 경로
                    "Characters/NPC_Female/Walk_3", // 기존 걷기 경로
                    "Characters/NPC_Female/Move_3" // 기존 이동 경로
                }, // 목록 끝
                WorldArtCatalog.GetCivilianMovementPaths(3)); // 네 번째 프레임 경로 조회
        } // 테스트 끝

        [Test] // 테스트 표시
        public void RuntimeAnimator_ExposesCandidateArrayConfigureOverload() // 후보 배열 설정 인터페이스 검증
        { // 테스트 시작
            Assert.IsNotNull( // 오버로드 존재 확인
                typeof(RuntimeCharacterSpriteAnimator).GetMethod( // 공개 메서드 조회
                    "Configure", // 설정 메서드 이름
                    new[] // 매개변수 형식 목록
                    { // 목록 시작
                        typeof(string[]), // 대기 후보 배열 형식
                        typeof(string[][]), // 이동 후보 배열 형식
                        typeof(float), // 초당 프레임 형식
                        typeof(float) // 픽셀 단위 형식
                    })); // 조회 결과 전달
        } // 테스트 끝

        [Test] // 테스트 표시
        [Category("UnityIntegration")] // Unity 통합 분류
        public void RuntimeArtLoader_ReusesCachedSpriteForSameArguments() // 로더 캐시 재사용 검증
        { // 테스트 시작
            string[] paths = // 테스트 경로 목록
            { // 목록 시작
                "Characters/NPC/Civilian/Idle" // 생성 시민 대기 경로
            }; // 목록 끝
            Sprite first = RuntimeArtLoader.LoadFirst( // 첫 스프라이트 로드
                paths, // 후보 경로 전달
                new Vector2(0.5f, 0.5f), // 중앙 피벗 지정
                100f, // 픽셀 단위 지정
                out string firstPath); // 첫 실제 경로 수신
            Sprite second = RuntimeArtLoader.LoadFirst( // 두 번째 스프라이트 로드
                paths, // 같은 후보 경로 전달
                new Vector2(0.5f, 0.5f), // 같은 피벗 지정
                100f, // 같은 픽셀 단위 지정
                out string secondPath); // 두 번째 실제 경로 수신

            Assert.IsNotNull(first); // 첫 로드 성공 확인
            Assert.AreSame(first, second); // 같은 인스턴스 확인
            Assert.AreEqual(firstPath, secondPath); // 같은 실제 경로 확인
        } // 테스트 끝
    } // 클래스 끝
} // 공간 끝
