---
# 47일차 — 주요 캐릭터 정식 리소스 1차 적용

주인공·리엘라·르미아의 스프라이트와 대화 초상을 코드 변경 없이 교체할 수 있는 공통 카탈로그를 구축했다.

---
## 적용 내용

- 주인공·리엘라·르미아의 캐릭터 ID와 리소스 주소 통합
- 정식 `Walk_0~3` 우선, `Move_0~3` 및 기존 임시 폴더 순서의 폴백 적용
- 화자와 표정 상태에 따른 대화용 반신 초상 자동 선택
- 내레이션과 일반 NPC 대사에서 주요 캐릭터 초상 숨김
- 리엘라 허브·로딩 이미지와 르미아 보스·분신 이미지 공통 카탈로그 연결
- 정식 파일 부재 시 기존 이미지 또는 실루엣 유지
- 최종 결전과 관계 장면에 대표 표정 상태 연결

---
## 리소스 주소 규칙

- 주인공 게임 스프라이트: `Resources/Characters/Protagonist/Idle.png`, `Walk_0~3.png`
- 리엘라 게임 스프라이트: `Resources/Characters/Riella/Idle.png`, `Walk_0~3.png`
- 르미아 게임 스프라이트: `Resources/Characters/Lumia/Idle.png`, `Walk_0~3.png`
- 대화 초상: `Resources/Portraits/<Character>/<Expression>.png`
- 주요 행동: `Resources/Characters/<Character>/Actions/<Action>.png`
- 리엘라 허브 자세: `Resources/Characters/Riella/Room_Stand.png`, `Room_Sit.png`

`<Character>`는 `Protagonist`, `Riella`, `Lumia` 중 하나를 사용한다. 파일 확장자는 `Resources.Load` 주소에서 제외한다.

---
## 표정 이름

- 주인공: `Default`, `Focused`, `Embarrassed`, `Angry`, `Tired`
- 리엘라: `Default`, `Work`, `Playful`, `Worried`, `Jealous`, `Angry`, `Weakened`, `Sincere`
- 르미아: `Default`, `Interested`, `Provoking`, `Angry`, `Sincere`, `Weakened`, `Defeated`, `Ending`

지원하지 않는 표정이나 표정 파일이 없으면 같은 캐릭터의 `Default` 초상으로 전환한다.

---
## 주요 행동 이름

- `Hypnosis`: 주인공 최면 시전
- `ContractMagic`: 리엘라 계약 마법
- `ReverseHypnosis`: 르미아 역최면 시전
- `Victory`: 승리 행동
- `Defeat`: 패배 행동

---
## 기존 리소스 폴백

- 주인공: `Characters/Player`
- 리엘라: `Characters/Succubus`
- 르미아: `Characters/NPC_Female`

정식 리소스가 추가되면 카탈로그의 정식 주소가 먼저 선택되므로 기존 임시 파일을 삭제하지 않아도 교체된다.

---
## 검증

- 전체 솔루션 빌드 성공: 경고 0개, 오류 0개
- Day47 순수 회귀 테스트 8개 통과
- 캐릭터 등록, 화자 별칭, 표정 폴백, 이동 호환, 허브 자세, 행동 주소, 대사 표정 저장 검증
- 기존 주인공 `Resources` 스프라이트 연결 통합 테스트 추가 및 컴파일 검증

---
## 확인 필요

- 정식 이미지 파일은 아직 저장소에 없으므로 실제 외형과 크롭은 리소스 추가 뒤 Unity 화면 확인 필요
- Unity 편집기가 프로젝트를 점유해 배치 모드 통합 테스트 결과 파일 미생성
