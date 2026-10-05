---
# 프로젝트 θ 48일차 아트 1차 적용 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 일반 NPC 5프레임·8개 지역 배경·프롤로그 CG 5개를 생성하고 NPC·맵·지도·장소 안내·로딩·프롤로그 화면에 안전한 폴백 구조로 적용한다.

**Architecture:** `WorldArtCatalog`가 정식 주소와 기존 주소의 우선순위를 순수 C#으로 제공하고, `RuntimeArtLoader`가 UI용 이미지 캐시와 누락 경로 캐시를 담당한다. 캐릭터 애니메이터는 후보 경로 배열을 받고, 지역 맵은 기존 `MapArtLibrary` 규칙을 유지하며 지도·장소 안내·로딩 화면이 같은 배경을 미리보기로 공유한다.

**Tech Stack:** Unity 6000.3 C#, Resources API, NUnit EditMode, 내장 이미지 생성 도구, Git

**Spec:** `docs/superpowers/specs/2026-10-05-day48-art-integration-design.md`

---
## Global Constraints

- 이미지 생성은 내장 이미지 생성 도구만 사용하고 한 자산당 한 번의 생성 또는 편집 호출을 사용한다.
- 프로젝트용 최종 이미지는 `Assets/_Project/Resources` 아래에 저장하고 기본 생성 폴더에만 남기지 않는다.
- 일반 NPC 이미지는 실제 투명 배경 PNG이며 동일한 성인 캐릭터 외형·의상·비율을 유지한다.
- 지역 배경과 프롤로그 CG에는 글자·로고·워터마크·전경 인물을 넣지 않는다.
- 신규·수정 C# 코드는 Allman 스타일과 줄별 한글 명사형 주석을 적용한다.
- 저장 데이터·게임 규칙·충돌체·정렬 순서·밸런스를 변경하지 않는다.
- 이미지 누락 시 현재 스프라이트·색상·도형 기반 화면을 유지한다.
- 모든 커밋 제목은 `48일차 : 내용` 형식이며 공동작성자를 넣지 않는다.

---
## Review Focus

- 정식 NPC 프레임 일부만 존재할 때 프레임별로 기존 `NPC_Female` 이미지가 선택되는지 확인
- 지원하지 않는 방해 적 종류나 빈 기존 루트가 들어와도 빈 경로 없이 안전한 후보를 반환하는지 확인
- 잘못된 프롤로그 페이지 번호가 `Page_01` 또는 `Page_05`로 보정되는지 확인
- 지역 미리보기 전용 파일이 없을 때 같은 지역 `Background`가 선택되는지 확인
- 공유 UI 스프라이트가 화면 파괴 과정에서 중복 제거되지 않고 플레이 모드 재진입 때만 정리되는지 확인

---
### Task 1: 이미지 18개 생성과 저장소 배치

**Files:**
- Create: `Assets/_Project/Resources/Characters/NPC/Civilian/Idle.png`
- Create: `Assets/_Project/Resources/Characters/NPC/Civilian/Walk_0.png`
- Create: `Assets/_Project/Resources/Characters/NPC/Civilian/Walk_1.png`
- Create: `Assets/_Project/Resources/Characters/NPC/Civilian/Walk_2.png`
- Create: `Assets/_Project/Resources/Characters/NPC/Civilian/Walk_3.png`
- Create: `Assets/_Project/Resources/Maps/<LocationId>/Background.png` 8개
- Create: `Assets/_Project/Resources/Story/Intro/Page_01.png`~`Page_05.png`
- Track: 위 이미지와 새 폴더의 Unity `.meta`

**Interfaces:**
- Consumes: 승인된 설계서의 스타일·구성·금지 요소
- Produces: Task 3~5가 `Resources.Load`로 읽는 18개 PNG

- [x] **Step 1: 일반 NPC 기준 이미지 생성**

내장 이미지 생성 도구로 `Idle`을 투명 배경 PNG로 생성한다. 프롬프트는 성인 여성 일반 시민, 현대 도시 판타지 2D 게임 스프라이트, 전신, 발 아래 중앙, 중성 일상복, 글자·그림자·소품·워터마크 제외를 고정한다.

- [x] **Step 2: 일반 NPC 걷기 프레임 4개 생성**

`Idle`을 외형 참고 이미지로 사용하고 왼발 접지·중간 교차·오른발 접지·중간 교차의 네 자세만 각각 변경한다. 외형·의상·색·카메라·크기·투명 배경은 고정한다.

- [x] **Step 3: 지역 배경 8개 생성**

각 `LocationId`별 한 장을 생성한다. 공통 프롬프트는 가로형 횡스크롤 배경, 낮은 대비, 중앙·하단 안전 영역, 보라·금색 현대 판타지 색 조화, 인물·글자·로고·워터마크 제외다.

- [x] **Step 4: 프롤로그 CG 5개 생성**

`IntroLogic.Pages`의 제목과 본문을 각각 장면 근거로 사용한다. 환경·실루엣 중심, 얼굴 확정 묘사 없음, 가로형 2D 서사 삽화, 글자·UI·로고·워터마크 제외를 고정한다.

- [x] **Step 5: 생성물 육안 검증과 프로젝트 복사**

각 결과를 `view_image`로 확인하고 주제·일관성·투명도·금지 요소를 검사한다. 불합격 이미지는 한 항목만 수정해 다시 생성한 뒤 승인된 결과만 설계서 경로로 복사한다.

- [x] **Step 6: 파일 수와 PNG 형식 확인**

Run: `Get-ChildItem Assets/_Project/Resources/Characters/NPC/Civilian,Assets/_Project/Resources/Maps,Assets/_Project/Resources/Story/Intro -Recurse -Filter *.png`

Expected: 신규 대상 파일 18개가 정확한 정식 경로에 존재

- [x] **Step 7: 이미지 묶음 커밋**

```bash
git add Assets/_Project/Resources/Characters/NPC Assets/_Project/Resources/Maps Assets/_Project/Resources/Story
git commit -m "48일차 : NPC·지역·프롤로그 아트 생성"
```

---
### Task 2: 월드 아트 주소와 공용 로더

**Files:**
- Create: `Assets/_Project/Scripts/Presentation/WorldArtCatalog.cs`
- Create: `Assets/_Project/Scripts/Presentation/WorldArtCatalog.cs.meta`
- Create: `Assets/_Project/Scripts/Presentation/RuntimeArtLoader.cs`
- Create: `Assets/_Project/Scripts/Presentation/RuntimeArtLoader.cs.meta`
- Create: `Assets/_Project/Tests/EditMode/Day48Tests.cs`
- Create: `Assets/_Project/Tests/EditMode/Day48Tests.cs.meta`

**Interfaces:**
- Consumes: `DisruptorKind`, `LocationId`, 기존 루트 문자열, 프롤로그 0 기반 페이지 번호
- Produces: `GetCivilianIdlePaths()`, `GetCivilianMovementPaths(int)`, `GetDisruptorIdlePaths(DisruptorKind,string)`, `GetDisruptorMovementPaths(DisruptorKind,string,int)`, `GetLocationPreviewPaths(LocationId)`, `GetIntroCgPaths(int)`, `RuntimeArtLoader.LoadFirst(string[],Vector2,float,out string)`

- [x] **Step 1: 카탈로그 계약 테스트 작성**

`Day48Tests`에 다음 단정문을 추가한다.

```csharp
CollectionAssert.AreEqual(new[] { "Characters/NPC/Civilian/Idle", "Characters/NPC_Female/Idle" }, WorldArtCatalog.GetCivilianIdlePaths());
CollectionAssert.AreEqual(new[] { "Characters/Disruptors/Lifeguard/Walk_2", "Characters/Disruptors/Lifeguard/Move_2", "Characters/Geumtaeyang/Walk_2", "Characters/Geumtaeyang/Move_2" }, WorldArtCatalog.GetDisruptorMovementPaths(DisruptorKind.Lifeguard, "Characters/Geumtaeyang", 2));
CollectionAssert.AreEqual(new[] { "Maps/Beach/Preview", "Maps/Beach/Background" }, WorldArtCatalog.GetLocationPreviewPaths(LocationId.Beach));
CollectionAssert.AreEqual(new[] { "Story/Intro/Page_01" }, WorldArtCatalog.GetIntroCgPaths(-1));
CollectionAssert.AreEqual(new[] { "Story/Intro/Page_05" }, WorldArtCatalog.GetIntroCgPaths(99));
```

빈 기존 루트에서 빈 문자열이 제거되는지, `RuntimeArtLoader.LoadFirst`를 같은 인자로 두 번 호출하면 같은 스프라이트 인스턴스를 반환하는지도 검증한다. 로더 검증은 `[Category("UnityIntegration")]`으로 분리한다.

- [x] **Step 2: RED 확인**

Run: `dotnet build ProjectTheta.Tests.EditMode.csproj --no-restore`

Expected: `WorldArtCatalog`과 `RuntimeArtLoader` 미정의 컴파일 실패

- [x] **Step 3: `WorldArtCatalog` 최소 구현**

모든 공개 함수는 새 배열을 반환하며 프레임은 0 이상으로, 프롤로그 페이지는 0~4로 보정한다. 빈 기존 루트는 정식 경로만 반환한다.

- [x] **Step 4: `RuntimeArtLoader` 구현**

`LoadFirst`는 후보별 `Resources.Load<Sprite>` 후 `Resources.Load<Texture2D>`를 시도한다. 캐시 키는 경로·pivot·pixelsPerUnit을 포함하고, 누락 경로는 별도 집합에 저장한다. `SubsystemRegistration`에서 생성 스프라이트와 캐시를 정리한다.

- [x] **Step 5: GREEN 확인**

Run: `dotnet build ProjectTheta.Tests.EditMode.csproj --no-restore`

Expected: 경고 0개, 오류 0개

- [x] **Step 6: 카탈로그 커밋**

```bash
git add Assets/_Project/Scripts/Presentation/WorldArtCatalog.cs Assets/_Project/Scripts/Presentation/WorldArtCatalog.cs.meta Assets/_Project/Scripts/Presentation/RuntimeArtLoader.cs Assets/_Project/Scripts/Presentation/RuntimeArtLoader.cs.meta Assets/_Project/Tests/EditMode/Day48Tests.cs Assets/_Project/Tests/EditMode/Day48Tests.cs.meta docs/superpowers/plans/2026-10-05-day48-art-integration.md
git commit -m "48일차 : 월드 아트 카탈로그 및 로더 구현"
```

---
### Task 3: 일반 NPC와 방해 적 교체 경로 연결

**Files:**
- Modify: `Assets/_Project/Scripts/Core/RuntimeCharacterSpriteAnimator.cs`
- Modify: `Assets/_Project/Scripts/Core/ProjectThetaPrototypeBootstrap.cs`
- Modify: `Assets/_Project/Scripts/Disruptors/DisruptorSpawner.cs`
- Test: `Assets/_Project/Tests/EditMode/Day48Tests.cs`

**Interfaces:**
- Consumes: Task 2의 NPC·방해 적 후보 경로 함수
- Produces: `RuntimeCharacterSpriteAnimator.Configure(string[],string[][],float,float)`와 실제 스포너 연결

- [x] **Step 1: 부분 프레임 폴백과 애니메이터 인터페이스 테스트 추가**

`GetCivilianMovementPaths(3)`의 정식 `Walk_3` 다음에 기존 `Move_3`가 존재하는지 검증한다. 리플렉션으로 `RuntimeCharacterSpriteAnimator.Configure(string[],string[][],float,float)` 오버로드가 존재하는지도 검증한다.

- [x] **Step 2: RED 확인**

Run: Day48 순수 테스트 실행

Expected: 새 애니메이터 후보 배열 오버로드 부재로 테스트 실패

- [x] **Step 3: 애니메이터 후보 배열 오버로드 구현**

`Configure(string[] idlePaths, string[][] movementPaths, float framesPerSecond = 8f, float pixelsPerUnit = 390f)`를 추가하고 각 프레임을 독립적으로 첫 유효 경로에서 불러온다. 기존 문자열·`CharacterArtId` 오버로드는 유지한다.

- [x] **Step 4: 일반 NPC·방해 적 스포너 연결**

일반 NPC는 `Civilian` 정식 경로와 `NPC_Female` 폴백을 사용한다. 방해 적은 `DisruptorKind` 정식 경로와 현재 `SpriteRoot` 폴백을 사용한다.

- [x] **Step 5: GREEN과 전체 빌드 확인**

Run: `dotnet build Project-Theta.slnx --no-restore`

Expected: 경고 0개, 오류 0개

- [x] **Step 6: NPC 연결 커밋**

```bash
git add Assets/_Project/Scripts/Core Assets/_Project/Scripts/Disruptors Assets/_Project/Tests/EditMode/Day48Tests.cs
git commit -m "48일차 : NPC 및 방해 적 아트 교체 경로 연결"
```

---
### Task 4: 지역 배경의 맵·지도·장소 안내·로딩 공유

**Files:**
- Modify: `Assets/_Project/Scripts/Map/MapArtLibrary.cs`
- Modify: `Assets/_Project/Scripts/UI/MapScreen.cs`
- Modify: `Assets/_Project/Scripts/UI/LocationGuidePanel.cs`
- Modify: `Assets/_Project/Scripts/UI/LoadingScreen.cs`
- Test: `Assets/_Project/Tests/EditMode/Day48Tests.cs`

**Interfaces:**
- Consumes: Task 1의 지역 배경과 Task 2의 `GetLocationPreviewPaths`
- Produces: `MapArtLibrary.TryGetFirst(string[])`, 지도 노드 썸네일, 장소 안내 배너, 로딩 배경

- [x] **Step 1: 8개 지역 주소 완전성과 조회 인터페이스 테스트 추가**

`LocationCatalog.All`의 모든 ID가 `Maps/<LocationId>/Background`과 `Preview → Background` 후보를 갖는지 검증한다. 리플렉션으로 `MapArtLibrary.TryGetFirst(string[])`가 존재하는지도 검증한다.

- [x] **Step 2: RED 확인**

Run: Day48 순수 테스트 실행

Expected: `MapArtLibrary.TryGetFirst` 부재로 테스트 실패

- [x] **Step 3: 지도와 장소 안내에 지역 미리보기 적용**

지도 노드와 장소 규칙 카드 상단에 첫 유효 미리보기 이미지를 배치한다. 이미지가 없으면 기존 색상·텍스트 구성을 유지한다.

- [x] **Step 4: 로딩 화면에 현재 도전 지역 적용**

`GameSession.Instance.Run`이 존재할 때 해당 지역 미리보기를 로딩 배경으로 사용하고, 없으면 현재 보라색 배경을 유지한다.

- [x] **Step 5: 맵 배경과 UI 공유 확인**

기존 `MapArtLibrary.TryGet(location, Background)`가 같은 `Maps/<LocationId>/Background`를 읽는지 테스트와 코드 검토로 확인한다.

- [x] **Step 6: 빌드와 회귀 테스트 확인**

Run: `dotnet build Project-Theta.slnx --no-restore`

Expected: 경고 0개, 오류 0개

- [x] **Step 7: 지역 UI 연결 커밋**

```bash
git add Assets/_Project/Scripts/Map Assets/_Project/Scripts/UI Assets/_Project/Tests/EditMode/Day48Tests.cs
git commit -m "48일차 : 지역 배경 및 UI 미리보기 적용"
```

---
### Task 5: 프롤로그 CG 연결

**Files:**
- Modify: `Assets/_Project/Scripts/UI/IntroSequence.cs`
- Modify: `Assets/_Project/Scripts/UI/IntroLogic.cs`
- Test: `Assets/_Project/Tests/EditMode/Day48Tests.cs`

**Interfaces:**
- Consumes: Task 1의 `Page_01~05`와 Task 2의 `GetIntroCgPaths`
- Produces: 페이지 전환마다 CG를 교체하고 누락 시 기존 글로우를 복원하는 프롤로그

- [x] **Step 1: 5개 페이지 소비 인터페이스 테스트 추가**

`IntroLogic.GetArtPaths(int)`가 0~4 모든 페이지를 중복 없이 `Page_01~05`에 대응시키고 범위 밖 번호를 양 끝으로 보정하는지 검증한다.

- [x] **Step 2: RED 확인**

Run: Day48 순수 테스트 실행

Expected: `IntroLogic.GetArtPaths` 미정의 컴파일 실패

- [x] **Step 3: 프롤로그 CG 표시 구현**

`IntroLogic.GetArtPaths(int)`를 추가하고 `ShowPage`에서 `RuntimeArtLoader.LoadFirst`를 호출한다. 성공하면 흰색·`preserveAspect`로 표시하고, 실패하면 생성 당시 보관한 글로우 스프라이트·색상으로 복원한다.

- [x] **Step 4: GREEN과 전체 빌드 확인**

Run: `dotnet build Project-Theta.slnx --no-restore`

Expected: 경고 0개, 오류 0개

- [x] **Step 5: 프롤로그 커밋**

```bash
git add Assets/_Project/Scripts/UI/IntroSequence.cs Assets/_Project/Tests/EditMode/Day48Tests.cs
git commit -m "48일차 : 프롤로그 CG 5장 적용"
```

---
### Task 6: 개발 기록·독립 검토·검증·기획서·푸시

**Files:**
- Create: `Devlogs/Day48/README.md`
- Modify: `docs/superpowers/plans/2026-10-05-day48-art-integration.md`
- External: Google 기획서 `개발 방향` 탭 48일차 상태 셀

**Interfaces:**
- Consumes: Task 1~5의 파일·테스트·커밋
- Produces: 검증 기록, 리뷰 반영, 완료 상태와 커밋 링크, `origin/main` 동기화

- [x] **Step 1: 개발 기록 작성**

18개 이미지 목록, 경로 규칙, 폴백, 적용 화면, 미제작 범위, 이미지 생성 프롬프트 요약을 기록한다.

- [x] **Step 2: 독립 코드 검토 요청**

검토자는 설계 범위·캐시 소유권·부분 프레임 폴백·UI 비율·이미지 파일 추적·줄별 한글 주석을 점검한다. 중요 지적은 수정 후 재검증한다.

- [x] **Step 3: 최종 검증**

Run: `dotnet build Project-Theta.slnx --no-restore`

Run: Day48 순수 회귀 테스트 실행

Run: `git diff --check`

Expected: 빌드 경고 0개·오류 0개, Day48 테스트 전부 통과, diff 오류 없음

- [x] **Step 4: Unity EditMode 통합 테스트 시도**

프로젝트가 다른 Unity 편집기에서 열려 있지 않을 때 Day48 테스트를 배치 실행한다. 점유 중이면 사용자 프로세스를 종료하지 않고 로그와 제한을 개발 기록에 남긴다.

- [x] **Step 5: 최종 기록 커밋**

```bash
git add Devlogs/Day48 docs/superpowers/plans/2026-10-05-day48-art-integration.md
git commit -m "48일차 : NPC·지역·스토리 CG 아트 1차 적용"
```

- [ ] **Step 6: Google 기획서 갱신**

최신 리비전과 `개발 방향` 탭의 48일차 행을 다시 읽고 상태 셀만 `예정 → 완료`로 변경한다. Step 5의 최종 기록 커밋 링크를 같은 셀에 적용하고 재조회한다.

- [ ] **Step 7: `origin/main` 푸시와 해시 확인**

Run: `git push origin main`

Run: `git rev-parse HEAD` and `git rev-parse origin/main`

Expected: 두 해시 일치, `git status --short --branch`가 `main...origin/main`
