---
# 48일차 — NPC·지역·프롤로그 아트 1차 적용

일반 시민 NPC 5종, 지역 배경 8종, 프롤로그 CG 5종을 생성하고 실제 게임 화면의 정식 리소스 경로에 연결했다.

---
## 생성 이미지 18종

- 일반 시민 NPC 5종: `Idle`, `Walk_0`, `Walk_1`, `Walk_2`, `Walk_3`
- 지역 배경 8종: `TrainingCenter`, `Beach`, `SubwayStation`, `FitnessCenter`, `NightMarket`, `ShoppingMall`, `OfficeTower`, `RooftopClub`
- 프롤로그 CG 5종: `Page_01`, `Page_02`, `Page_03`, `Page_04`, `Page_05`

---
## 리소스 주소 규칙

- 일반 시민: `Resources/Characters/NPC/Civilian/Idle.png`, `Walk_0~3.png`
- 방해 적 정식 주소: `Resources/Characters/Disruptors/<DisruptorKind>/Idle.png`, `Walk_0~3.png`
- 지역 배경: `Resources/Maps/<LocationId>/Background.png`
- 지역 전용 미리보기 확장 주소: `Resources/Maps/<LocationId>/Preview.png`
- 프롤로그 CG: `Resources/Story/Intro/Page_01~05.png`

`Resources.Load` 주소에는 파일 확장자를 넣지 않는다. 지역 미리보기는 `Preview → Background` 순서로 조회해 별도 미리보기 파일이 없어도 이번 배경 8종을 공유한다.

---
## 폴백과 로더

- 일반 시민: `Characters/NPC/Civilian` 우선, `Characters/NPC_Female` 폴백
- 방해 적: `Characters/Disruptors/<DisruptorKind>` 우선, 기존 `SpriteRoot` 폴백
- 이동 프레임: 각 프레임마다 `Walk_n → Move_n → 기존 Walk_n → 기존 Move_n` 순서 조회
- 지역 UI: `Maps/<LocationId>/Preview → Maps/<LocationId>/Background` 순서 조회
- 프롤로그: CG 누락 시 기존 보라색 글로우 복원
- 공용 로더: 스프라이트 우선 조회, 텍스처 기반 런타임 생성 폴백, 경로·피벗·픽셀 단위별 캐시
- 캐릭터 로더: 전체 텍스처와 발 기준 피벗 사용, 768px 기준 월드 높이 정규화
- 지역 UI: 원본 종횡비 유지

---
## 적용 화면

- 스테이지 일반 NPC 대기·걷기 애니메이션
- 사람형 방해 적의 종류별 정식 교체 경로
- 스테이지 맵 배경
- 도시 지도 노드 썸네일과 선택 정보 패널
- 스테이지 장소 규칙 카드 상단 배너
- 현재 도전 지역 로딩 배경
- 새 게임 프롤로그 5개 페이지 CG

이미지가 없거나 로드에 실패하면 기존 텍스트·색상·실루엣·레거시 리소스를 유지한다.

---
## 이미지 생성 프롬프트 요약

- 시민 NPC: 현대 도시 판타지 2D 전신 측면 스프라이트, 중성 일상복, 투명 배경, 동일 인물·의상·비율 유지, 네 단계 걷기 자세만 변경
- 지역 배경: 가로형 횡스크롤 배경, 중앙·하단 플레이 안전 영역, 보라·금색 HUD 조화, 지역별 시간대와 구조 강조, 인물·글자·로고 제외
- 프롤로그 CG: 보라·금색 현대 도시 판타지 서사 삽화, 환경과 실루엣 중심, 얼굴 확정 묘사·글자·UI·로고 제외

---
## 미제작 범위

- 개별 방해 적, 주요 캐릭터, 추가 일반 NPC 변형의 실제 신규 이미지
- 아이콘·버튼·초상·보상·상태 표시를 포함한 전체 UI 리소스 묶음
- 기획서 필요 리소스 탭의 전체 522개 목표 중 이번 핵심 18종을 제외한 항목

미제작 리소스는 정식 주소와 기존 폴백 구조만 유지한다.

---
## 검증

- 전체 솔루션 빌드 성공: 경고 0개, 오류 0개
- Day48 순수 회귀 테스트 10개 통과
- 이미지 18개와 Unity 가져오기 메타 추적 확인
- 독립 검토 중요 항목 3건 수정: 캐릭터 피벗·PPU, 프레임 높이, 지역 이미지 비율
- Day48 범위 Unity 메타 후행 공백 정리와 `git diff --check` 통과
- Unity EditMode 배치 테스트 시도: `Temp/UnityLockfile`과 실행 중 Unity 프로세스 7개 확인
- Unity 배치 실행은 테스트 결과 XML 생성 전에 반환 코드 1로 종료
- 사용자 Unity 프로세스는 종료하지 않았으며 실제 `Resources` 로드는 편집기 점유 해제 뒤 재확인 필요
- Google 기획서 `개발 방향` 탭 48일차 상태를 `완료`로 갱신하고 최종 기록 커밋 연결
- `origin/main` 푸시 후 로컬·원격 해시 일치 확인
