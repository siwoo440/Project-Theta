---
# 프로젝트 θ 48일차 아트 1차 적용 설계

---
## 목표

일반 NPC·8개 지역·프롤로그에 실제 생성 이미지를 적용하고, 지도·로딩·장소 안내 화면이 같은 지역 이미지를 재사용하도록 연결한다. 정식 이미지가 없거나 불러오지 못해도 현재 스프라이트와 도형 기반 화면이 유지되어야 한다.

---
## 범위

48일차에서 새로 생성하고 저장소에 포함할 이미지는 18개다.

- 일반 NPC 대기 프레임 1개
- 일반 NPC 걷기 프레임 4개
- 지역 배경 8개
- 프롤로그 CG 5개

방해 적 30종, 지역 소품, 업적 아이콘, 주요 캐릭터 추가 표정, 보스 전용 UI는 이번 일차에 개별 이미지를 제작하지 않는다. 대신 이후 파일을 추가하면 코드 변경 없이 교체되도록 정식 주소와 기존 임시 이미지 폴백을 확정한다.

---
## 이미지 제작 규격

모든 이미지는 내장 이미지 생성 도구로 제작하며 워터마크·로고·글자를 포함하지 않는다.

### 일반 NPC

- 형식: 투명 배경 PNG
- 구성: 동일한 성인 여성 NPC 외형의 전신 측면형 게임 스프라이트
- 프레임: `Idle`, `Walk_0`, `Walk_1`, `Walk_2`, `Walk_3`
- 기준점: 발이 아래 중앙에 오도록 배치
- 스타일: 현대 도시 판타지 2D 게임 일러스트, 선명한 실루엣, 과도한 세부 묘사 제외
- 색상: 보라·금색 UI와 충돌하지 않는 중성 교복·일상복 계열

### 지역 배경

- 형식: 가로형 PNG
- 구성: 캐릭터와 게임 UI가 올라갈 수 있는 낮은 대비의 횡스크롤 배경
- 대상: `TrainingCenter`, `Beach`, `SubwayStation`, `FitnessCenter`, `NightMarket`, `ShoppingMall`, `OfficeTower`, `RooftopClub`
- 제한: 전경 인물·글자·로고·상호작용 오브젝트 제외
- 안전 영역: 화면 중앙과 하단 이동 구간의 시인성 유지

### 프롤로그 CG

- 형식: 가로형 PNG
- 구성: `IntroLogic.Pages`의 5개 장면과 일치하는 환경·실루엣 중심 삽화
- 대상: 인간 세상에 내려온 서큐버스, 정기, 최면과 동행, 방해 세력, 도시의 정점
- 제한: 주요 캐릭터 얼굴의 확정 묘사 제외, 글자·로고·UI 제외

---
## 저장 경로

```text
Assets/_Project/Resources/Characters/NPC/Civilian/Idle.png
Assets/_Project/Resources/Characters/NPC/Civilian/Walk_0.png
Assets/_Project/Resources/Characters/NPC/Civilian/Walk_1.png
Assets/_Project/Resources/Characters/NPC/Civilian/Walk_2.png
Assets/_Project/Resources/Characters/NPC/Civilian/Walk_3.png

Assets/_Project/Resources/Maps/<LocationId>/Background.png

Assets/_Project/Resources/Story/Intro/Page_01.png
Assets/_Project/Resources/Story/Intro/Page_02.png
Assets/_Project/Resources/Story/Intro/Page_03.png
Assets/_Project/Resources/Story/Intro/Page_04.png
Assets/_Project/Resources/Story/Intro/Page_05.png
```

Unity가 생성하는 각 `.meta` 파일도 함께 추적한다.

---
## 코드 구조

### 월드 아트 카탈로그

`WorldArtCatalog`가 일반 NPC·방해 적·지역 UI·프롤로그 CG의 정식 주소와 폴백 순서를 제공한다.

- 일반 NPC: `Characters/NPC/Civilian` 우선, `Characters/NPC_Female` 폴백
- 방해 적: `Characters/Disruptors/<DisruptorKind>` 우선, 기존 `DisruptorProfile.SpriteRoot` 폴백
- 지역 UI: `Maps/<LocationId>/Preview` 우선, 같은 지역 `Background` 폴백
- 프롤로그 CG: `Story/Intro/Page_01~05`

카탈로그는 경로만 책임지고 Unity 오브젝트 생성이나 리소스 캐시는 담당하지 않는다.

### 공용 이미지 로더

`RuntimeArtLoader`가 후보 경로 순서대로 `Sprite` 또는 `Texture2D`를 조회하고, 텍스처만 있는 경우 런타임 스프라이트를 만든다.

- 성공한 경로와 스프라이트 캐시
- 누락 경로 캐시
- 플레이 모드 재진입 시 캐시 초기화
- 호출 화면이 소유권을 갖지 않는 공유 스프라이트 제공

### 게임 화면 연결

- 일반 NPC 생성: 정식 `Civilian` 프레임 우선 사용
- 방해 적 생성: 종류별 정식 폴더 우선 사용
- 지역 맵: 기존 `MapArtLibrary`의 배경 폴더 규칙 유지
- 지도·장소 안내: 지역 `Preview` 또는 `Background`를 썸네일로 사용
- 로딩 화면: 선택 지역의 `Preview` 또는 `Background` 사용
- 프롤로그: 페이지 번호에 맞는 CG 표시, 누락 시 기존 보라색 빛 유지

---
## 데이터 흐름

1. 화면이나 스포너가 캐릭터 종류·지역·프롤로그 페이지를 카탈로그에 전달한다.
2. 카탈로그가 정식 주소부터 기존 폴백까지 후보 목록을 반환한다.
3. 로더가 첫 번째 유효 이미지를 캐시하고 화면에 전달한다.
4. 모든 후보가 없으면 호출부가 현재 도형·색상·기존 스프라이트를 유지한다.
5. 이후 정식 파일을 같은 주소에 추가하면 별도 코드 수정 없이 다음 실행부터 반영된다.

---
## 오류 처리와 호환성

- 이미지 누락은 오류가 아닌 정상 폴백 상황으로 처리
- 같은 누락 경로의 반복 `Resources.Load` 방지
- 기존 `Characters/NPC_Female`, `Geumtaeyang`, `PopularGuy` 폴더 유지
- 저장 데이터·게임 규칙·충돌체·정렬 순서 변경 금지
- 이미지 크기와 비율이 달라도 `preserveAspect` 또는 월드 배율로 안전하게 표시
- 생성 이미지가 Unity에서 불러와지지 않으면 해당 화면만 기존 표현 유지

---
## 테스트 전략

### 순수 EditMode 테스트

- 일반 NPC 정식 경로와 기존 폴백 순서
- 방해 적 종류별 정식 경로와 기존 폴백 순서
- 8개 지역 배경·미리보기 경로
- 프롤로그 5개 페이지 경로와 범위 보정
- 빈 경로·잘못된 페이지 입력의 안전한 처리

### 통합 검증

- 전체 솔루션 빌드 경고·오류 0건
- 생성 이미지 18개와 `.meta` 존재 확인
- 일반 NPC·지역 맵·프롤로그 소비 코드 컴파일 확인
- Unity 편집기 점유 시 배치 테스트 제한을 개발 기록에 명시
- `git diff --check` 통과와 독립 코드 검토 반영

---
## 완료 기준

- 18개 이미지가 정식 경로에 저장되고 Git 추적 대상에 포함
- 일반 NPC 대기·이동 프레임이 정식 이미지로 교체
- 8개 지역 배경이 게임 맵과 지역 UI에서 공유
- 프롤로그 5개 장면이 실제 CG를 표시
- 정식 방해 적·소품·추가 UI 이미지의 교체 주소가 문서화
- 이미지 누락 상태에서도 기존 화면과 플레이 흐름 유지
- Google 기획서 48일차 상태를 커밋 링크가 연결된 `완료`로 갱신
- `48일차 : NPC·지역·스토리 CG 아트 1차 적용` 형식의 구현 커밋을 `origin/main`에 푸시

---
## 제외 범위

- 522개 전체 이미지 일괄 제작
- 정식 BGM·효과음·보이스 교체
- 주요 캐릭터 얼굴과 추가 표정 재설계
- 지역 충돌체·게임 규칙·밸런스 변경
- Steam 배포용 스크린샷 제작
