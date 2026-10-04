# Day 47 Character Art Routing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 주인공·리엘라·르미아의 게임 스프라이트와 대화 초상을 동일한 주소·상태 규칙으로 교체할 수 있게 한다.

**Architecture:** 순수 C# 카탈로그가 캐릭터별 리소스 경로와 표정 폴백 순서를 제공한다. 기존 런타임 애니메이터·대화창·허브·로딩 화면은 카탈로그만 참조하고, 실제 파일이 없으면 기존 경로 또는 실루엣을 유지한다.

**Tech Stack:** Unity 2022 C#, Resources API, NUnit EditMode 테스트

**Spec:** Google 기획서 `개발 방향` 탭 47일차와 `필요 리소스` 탭 UI-01·UI-03·UI-05

## Global Constraints

- 커밋 제목은 `47일차 : 내용` 형식
- 신규·수정 C# 코드는 Allman 스타일과 줄별 한글 명사형 주석 적용
- 정식 이미지가 없는 상태에서도 기존 화면 동작 유지
- 공동작성자 미사용

## Review Focus

- 알 수 없는 화자·표정 입력 시 안전한 기본 폴백
- 기존 `Move_0~3`와 신규 `Walk_0~3`의 호환 순서
- 내레이션에서 초상 숨김
- 리엘라 허브·로딩 이미지의 기존 실루엣 유지
- 르미아 보스 경로가 카탈로그와 일치

---

### Task 1: 캐릭터 리소스 카탈로그

**Files:**
- Create: `Assets/_Project/Scripts/Presentation/CharacterArtCatalog.cs`
- Test: `Assets/_Project/Tests/EditMode/Day47Tests.cs`

**Interfaces:**
- Produces: 캐릭터 ID, 런타임 루트, 초상 경로 후보, 이동 프레임 경로 후보 조회 함수

- [x] 카탈로그 계약 테스트 작성
- [x] 테스트 실패 확인
- [x] 최소 카탈로그 구현
- [x] 테스트 통과 확인

### Task 2: 런타임·대화·허브 연결

**Files:**
- Modify: `Assets/_Project/Scripts/Core/RuntimeCharacterSpriteAnimator.cs`
- Modify: `Assets/_Project/Scripts/UI/DialogueOverlay.cs`
- Modify: `Assets/_Project/Scripts/UI/HubSuccubus.cs`
- Modify: `Assets/_Project/Scripts/UI/LoadingScreen.cs`
- Modify: `Assets/_Project/Scripts/Core/ProjectThetaPrototypeBootstrap.cs`
- Modify: `Assets/_Project/Scripts/Boss/RivalSuccubus.cs`
- Test: `Assets/_Project/Tests/EditMode/Day47Tests.cs`

**Interfaces:**
- Consumes: Task 1 카탈로그 조회 함수
- Produces: 기존 임시 리소스와 신규 정식 리소스의 무중단 교체 흐름

- [x] 연결 경로 회귀 테스트 작성
- [x] 테스트 실패 확인
- [x] 런타임·UI 연결 구현
- [x] 테스트 통과 확인

### Task 3: 문서화·검증·배포

**Files:**
- Create: `Devlogs/Day47/README.md`
- Track: `Assets/_Project/Tests/EditMode/Day46Tests.cs.meta`

**Interfaces:**
- Consumes: Task 1~2의 확정 경로와 상태 규칙
- Produces: 리소스 교체 안내, 검증 결과, 기획서 완료 기록

- [x] 리소스 폴더·파일명·폴백 규칙 문서화
- [x] 전체 빌드와 EditMode 테스트 실행
- [x] 독립 코드 리뷰 반영
- [x] Google 기획서 47일차 완료 갱신과 읽기 검증
- [ ] `main` 커밋과 `origin/main` 푸시
