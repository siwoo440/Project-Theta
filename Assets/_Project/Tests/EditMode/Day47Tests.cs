using NUnit.Framework; // 테스트 도구 참조
using ProjectTheta.Core; // 캐릭터 애니메이터 참조
using ProjectTheta.Presentation; // 캐릭터 리소스 규칙 참조
using ProjectTheta.Story; // 이야기 대사 자료 참조
using UnityEngine; // Unity 오브젝트 참조

namespace ProjectTheta.Tests.EditMode // 편집 모드 테스트 공간
{ // 공간 시작
    public sealed class Day47Tests // 47일차 회귀 테스트
    { // 클래스 시작
        [Test] // 테스트 표시
        public void Catalog_ContainsThreeMainCharacters() // 주요 인물 등록 검증
        { // 테스트 시작
            Assert.AreEqual(3, CharacterArtCatalog.All.Length); // 인물 수 확인
            Assert.AreEqual(CharacterArtId.Protagonist, CharacterArtCatalog.All[0].Id); // 주인공 순서 확인
            Assert.AreEqual(CharacterArtId.Riella, CharacterArtCatalog.All[1].Id); // 리엘라 순서 확인
            Assert.AreEqual(CharacterArtId.Lumia, CharacterArtCatalog.All[2].Id); // 르미아 순서 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void SpeakerAliases_ResolveMainCharactersAndRejectUnknownSpeaker() // 화자 별칭 조회 검증
        { // 테스트 시작
            Assert.IsTrue(CharacterArtCatalog.TryGetCharacterBySpeaker("나", out CharacterArtId me)); // 나 화자 조회
            Assert.AreEqual(CharacterArtId.Protagonist, me); // 주인공 별칭 확인
            Assert.IsTrue(CharacterArtCatalog.TryGetCharacterBySpeaker("라이벌", out CharacterArtId rival)); // 라이벌 화자 조회
            Assert.AreEqual(CharacterArtId.Lumia, rival); // 르미아 별칭 확인
            Assert.IsFalse(CharacterArtCatalog.TryGetCharacterBySpeaker("교사", out _)); // 일반 화자 제외 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        public void PortraitPaths_PreferRequestedExpressionThenDefaultPortrait() // 초상 표정 폴백 검증
        { // 테스트 시작
            string[] paths = CharacterArtCatalog.GetPortraitPaths( // 초상 경로 조회
                CharacterArtId.Riella, // 리엘라 선택
                CharacterArtExpression.Worried); // 걱정 표정 선택

            CollectionAssert.AreEqual( // 경로 순서 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Portraits/Riella/Worried", // 걱정 초상
                    "Portraits/Riella/Default" // 기본 초상
                }, // 목록 끝
                paths); // 실제 경로
        } // 테스트 끝

        [Test] // 테스트 표시
        public void UnsupportedExpression_FallsBackToCharacterDefault() // 미지원 표정 기본 전환 검증
        { // 테스트 시작
            string[] paths = CharacterArtCatalog.GetPortraitPaths( // 초상 경로 조회
                CharacterArtId.Protagonist, // 주인공 선택
                CharacterArtExpression.Jealous); // 미지원 질투 표정 선택

            CollectionAssert.AreEqual( // 기본 경로 확인
                new[] { "Portraits/Protagonist/Default" }, // 예상 기본 경로
                paths); // 실제 경로
        } // 테스트 끝

        [Test] // 테스트 표시
        public void MovementPaths_PreferWalkAndPreserveLegacyMoveFrames() // 이동 프레임 호환 검증
        { // 테스트 시작
            string[] paths = CharacterArtCatalog.GetMovementPaths( // 이동 경로 조회
                CharacterArtId.Protagonist, // 주인공 선택
                2); // 세 번째 프레임 선택

            CollectionAssert.AreEqual( // 경로 순서 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/Protagonist/Walk_2", // 정식 걷기 경로
                    "Characters/Protagonist/Move_2", // 정식 호환 경로
                    "Characters/Player/Walk_2", // 임시 걷기 경로
                    "Characters/Player/Move_2" // 기존 이동 경로
                }, // 목록 끝
                paths); // 실제 경로
        } // 테스트 끝

        [Test] // 테스트 표시
        public void HubPaths_PreferRiellaPoseAndRetainLegacySuccubusFallback() // 허브 리엘라 폴백 검증
        { // 테스트 시작
            string[] paths = CharacterArtCatalog.GetHubPaths(true); // 앉은 자세 경로 조회

            CollectionAssert.AreEqual( // 경로 순서 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/Riella/Room_Sit", // 정식 앉은 자세
                    "Characters/Succubus/Room_Sit", // 기존 앉은 자세
                    "Characters/Riella/Idle", // 정식 기본 자세
                    "Characters/Succubus/Idle" // 기존 기본 자세
                }, // 목록 끝
                paths); // 실제 경로
        } // 테스트 끝

        [Test] // 테스트 표시
        public void ActionPaths_UseStableCharacterAndActionNames() // 주요 행동 경로 규칙 검증
        { // 테스트 시작
            string[] paths = CharacterArtCatalog.GetActionPaths( // 행동 경로 조회
                CharacterArtId.Lumia, // 르미아 선택
                CharacterArtAction.ReverseHypnosis); // 역최면 선택

            CollectionAssert.AreEqual( // 경로 순서 확인
                new[] // 예상 경로 목록
                { // 목록 시작
                    "Characters/Lumia/Actions/ReverseHypnosis", // 정식 행동 폴더 경로
                    "Characters/Lumia/ReverseHypnosis", // 단일 파일 호환 경로
                    "Characters/NPC_Female/ReverseHypnosis" // 기존 임시 경로
                }, // 목록 끝
                paths); // 실제 경로
        } // 테스트 끝

        [Test] // 테스트 표시
        public void StoryLine_PreservesRequestedPortraitExpression() // 대사 표정 저장 검증
        { // 테스트 시작
            StoryLine line = new StoryLine( // 대사 생성
                "르미아", // 화자 지정
                "재미있네.", // 대사 지정
                CharacterArtExpression.Interested); // 흥미 표정 지정

            Assert.AreEqual(CharacterArtExpression.Interested, line.Expression); // 표정 저장 확인
        } // 테스트 끝

        [Test] // 테스트 표시
        [Category("UnityIntegration")] // Unity 통합 분류
        public void Animator_LoadsExistingLegacyPlayerSprites() // 기존 주인공 리소스 연결 검증
        { // 테스트 시작
            GameObject host = new GameObject("Day47AnimatorTest"); // 테스트 오브젝트 생성

            try // 정리 보장 시작
            { // 시도 시작
                SpriteRenderer renderer = host.AddComponent<SpriteRenderer>(); // 스프라이트 렌더러 추가
                RuntimeCharacterSpriteAnimator animator = host.AddComponent<RuntimeCharacterSpriteAnimator>(); // 애니메이터 추가
                animator.Configure(CharacterArtId.Protagonist); // 주인공 리소스 설정

                Assert.AreEqual("Characters/Player/Idle", animator.LoadedIdlePath); // 기존 대기 경로 확인
                Assert.IsNotNull(renderer.sprite); // 실제 스프라이트 확인
            } // 시도 끝
            finally // 정리 시작
            { // 정리 블록 시작
                Object.DestroyImmediate(host); // 테스트 오브젝트 제거
            } // 정리 블록 끝
        } // 테스트 끝
    } // 클래스 끝
} // 공간 끝
