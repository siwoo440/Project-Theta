# Project θ 개발 일지 — 9일차

## 개발 목표

8일차에 구현한 기본 NPC 소유권·쟁탈·재탈환 시스템을 확장하여, 서로 다른 목적과 행동 템포를 가진 경쟁자 2종을 구성했다.

기존 라이벌 명칭은 게임 내에서 **금태양**으로 변경했고, 신규 경쟁자 **인기남**을 추가했다.

9일차의 핵심은 다음과 같다.

```text
Player
↕
금태양
↕
인기남

Neutral NPC까지 포함한 다중 소유권 경쟁
```

## 소유권 구조 확장

NPC 소유권을 다음 4종으로 확장했다.

```text
Neutral
Player
Geumtaeyang
PopularGuy
```

게임 내 표시 명칭은 `Geumtaeyang`을 **금태양**, `PopularGuy`를 **인기남**으로 사용한다.

플레이어는 금태양 또는 인기남 소유 NPC 모두 기존 `E / LMB` 최면 입력으로 다시 탈환할 수 있다.

## 금태양 AI 고도화

금태양은 기존처럼 플레이어가 확보한 동행 NPC를 주요 대상으로 삼는다.

단순히 가장 가까운 NPC만 고르는 대신 다음 요소를 이용해 타겟을 평가한다.

- 금태양과의 거리
- 플레이어와 NPC 사이의 거리

플레이어에게서 떨어져 있으면서 금태양이 접근하기 쉬운 NPC가 더 높은 우선순위를 가진다.

기본 추적 수치는 다음과 같다.

| 항목 | 값 |
| --- | ---: |
| 이동 속도 | 4.2 |
| 탐색 거리 | 10 |
| 추적 포기 거리 | 13 |
| 최대 추적 시간 | 5초 |
| 쟁탈 거리 | 1.2 |
| 지배 감소 | 18 / sec |
| 시작 Idle | 0.6~1.5초 |
| 타겟 상실 후 Idle | 0.4~0.9초 |
| 확보 후 Idle | 0.5~1.2초 |

대상이 너무 멀어지거나 최대 추적 시간을 초과하면 해당 NPC를 포기하고 다시 Idle과 Search 과정을 거친다.

## 인기남 추가

새 경쟁자 `인기남_01`을 추가했다.

인기남은 금태양과 달리 **중립 NPC 선점**을 가장 먼저 시도한다.

행동 우선순위는 다음과 같다.

```text
Neutral NPC 존재
→ 가장 가까운 Neutral NPC 접근
→ 거리 1.2 이내
→ 즉시 인기남 소유
→ 인기남 추종 시작

Neutral NPC 없음
→ Player 또는 금태양 소유 NPC 탐색
→ 상대 지배 수치 감소
→ 0
→ 인기남 소유 전환
```

중립 NPC는 별도의 최면 축적 과정을 거치지 않고 접근 성공 즉시 인기남 소유가 된다.

## 인기남 행동 템포

인기남은 금태양보다 느리고 여유 있는 타입으로 조정했다.

최종 수치는 다음과 같다.

| 항목 | 값 |
| --- | ---: |
| 이동 속도 | 2.1 |
| 대상 재탐색 | 0.90초 |
| 추적 포기 거리 | 13 |
| 최대 추적 시간 | 7.5초 |
| 행동 거리 | 1.2 |
| 쟁탈 지배 감소 | 12 / sec |
| 시작 Idle | 1.8~4.5초 |
| 타겟 상실 후 Idle | 1.2~2.7초 |
| 확보 후 Idle | 1.5~3.6초 |

이동 속도는 기존 4.2에서 절반인 2.1로 낮췄다.

또한 이전 인기남 기본값보다 행동 대기 시간을 2배 늘려 금태양과 확실한 템포 차이를 만들었다.

쟁탈 속도 역시 금태양의 18/sec보다 느린 12/sec를 사용한다.

## 인기남 Follower 시스템

인기남에게 확보된 NPC는 중립 상태로 돌아가지 않고 인기남을 직접 따라간다.

이를 위해 다음 구조를 추가했다.

```text
PopularGuyFollowerManager
PopularGuyFollowerController
```

인기남도 여러 명의 NPC를 동시에 보유할 수 있다.

플레이어가 인기남 소유 NPC를 탈환하면 인기남 Follower 목록에서 제거된 뒤 Player Follower로 다시 등록된다.

## 금태양과 인기남 간 쟁탈

인기남은 중립 NPC가 모두 소진된 후 자신 소유가 아닌 최면 NPC를 대상으로 삼는다.

따라서 다음 전환이 가능하다.

```text
Player → 금태양
Player → 인기남
금태양 → 인기남
인기남 → Player
금태양 → Player
```

금태양은 현재 플레이어 소유 NPC를 중심으로 행동하며, 인기남은 Neutral 우선 확보 후 다른 세력 NPC를 노리는 차별화된 구조를 가진다.

## 상태 충돌 방지

플레이어 소유 NPC가 다음 상태일 때는 금태양 또는 인기남의 쟁탈 대상에서 제외한다.

- Preparing
- Rampaging
- Capturing
- Recovering

소유권이 실제로 변경될 때는 기존 Follower/Impulse 제어를 정리한 뒤 새 소유자의 Follower 시스템에 등록한다.

이를 통해 하나의 NPC가 Player와 상대 양쪽에 동시에 등록되는 상황을 방지한다.

## 타겟 위험 표시

NPC가 금태양 또는 인기남의 현재 타겟으로 지정되면 상태 UI에서 위험 표시를 확인할 수 있도록 연결했다.

소유권 게이지 색상도 각 세력에 맞춰 구분한다.

```text
Player      → 분홍
금태양      → 빨강
인기남      → 청록
```

기존 플레이어 NPC의 충동 경고 UI도 유지한다.

## Debug HUD 확장

Prototype HUD를 `Day 09 Debug` 기준으로 확장했다.

금태양과 인기남 각각 다음 정보를 확인할 수 있다.

- 현재 AI State
- 보유 NPC 수
- 현재 타겟
- 남은 지배 수치
- 인기남의 경우 현재 모드: 중립 선점 / 쟁탈

## 경쟁자 전용 캐릭터 이미지 적용

금태양과 인기남에게 전용 캐릭터 이미지를 추가했다.

리소스 경로는 다음과 같다.

```text
Assets/_Project/Resources/Characters/Geumtaeyang/
Assets/_Project/Resources/Characters/PopularGuy/
```

각 폴더는 현재 런타임 캐릭터 애니메이터 구조에 맞춰 다음 파일을 사용한다.

```text
Idle.png
Move_0.png
Move_1.png
Move_2.png
Move_3.png
```

현재 전용 보행 프레임이 별도로 제작된 상태는 아니므로 동일한 캐릭터 이미지를 각 Move 슬롯에도 사용한다.

금태양과 인기남의 임시 Player Sprite Tint는 제거하고 원본 이미지 색상을 그대로 사용한다.

## 캐릭터 크기 및 투명 배경 정리

금태양과 인기남 이미지는 플레이어와 비슷한 게임 내 크기로 보이도록 캔버스와 실제 캐릭터 표시 크기를 다시 조정했다.

두 캐릭터 모두 투명 배경 PNG를 사용한다.

## Sprite Import 경고 수정

이미지 교체 과정에서 Unity Console에 다음 경고가 반복 발생했다.

```text
Open Sprite Editor Window to fix 'Move_2_0' not generated
because the rect lies (partially) outside of texture.
```

원인은 이전 이미지의 Sprite Editor 분할 Rect가 `.png.meta`에 남아 있었기 때문이다.

기존 Meta는 다음과 같은 상태였다.

```text
Sprite Mode: Multiple
Idle_0 / Move_0_0 / Move_1_0 / Move_2_0 / Move_3_0
기존 이미지 크기 기준 Sprite Rect
```

이미지 크기가 변경된 뒤에도 이 Rect가 유지되어 새 텍스처 범위를 벗어났다.

최종적으로 금태양·인기남의 총 10개 PNG Meta를 다음 방식으로 정리했다.

```text
Sprite Mode: Multiple
→ Sprite Mode: Single

기존 Sprite Sheet Rect
→ 제거

기존 파일 GUID
→ 유지
```

따라서 런타임 `RuntimeCharacterSpriteAnimator`가 `Resources.Load<Texture2D>`와 `Sprite.Create`를 사용하는 현재 구조에 불필요한 Sprite Editor Slice 정보를 제거했다.

## 테스트 코드

9일차 확장에 맞춰 다음 테스트 로직을 추가·갱신했다.

`OwnershipContestLogicTests`

- Player가 금태양과 인기남 NPC를 탈환할 수 있는지 확인
- 금태양이 Player NPC만 쟁탈하는지 확인
- 인기남이 Player·금태양 NPC를 쟁탈할 수 있는지 확인
- Neutral 및 자기 소유 NPC를 쟁탈하지 않는지 확인

`PopularGuyLogicTests`

- 인기남 행동 텀 배율 확인
- 인기남 쟁탈 속도 차이 확인
- 소유권별 인기남 쟁탈 가능 여부 확인

`OpponentTargetingLogicTests`

- 금태양 타겟 평가 점수 확인
- 추적 거리 초과 시 포기 확인
- 최대 추적 시간 초과 시 포기 확인

기존 `RivalIdleLogicTests`, `RivalTargetDecisionLogicTests`도 유지한다.

## 9일차 완료 기준

다음 흐름을 기준으로 9일차 시스템을 확인한다.

```text
TestStage 시작
↓
금태양 / 인기남 각각 Idle
↓
플레이어가 NPC 일부 최면

인기남
→ Neutral NPC 우선 접근
→ 즉시 선점
→ 인기남 추종

금태양
→ 플레이어 동행 NPC 평가
→ 접근
→ Player 지배 수치 감소
→ 0
→ 금태양 소유

Neutral NPC 소진
↓
인기남이 다른 세력 NPC 쟁탈
↓
금태양 NPC 또는 Player NPC를 인기남이 확보

플레이어
→ 금태양/인기남 소유 NPC에 E 또는 LMB
→ 상대 지배 수치 0
→ Player 소유 복귀
```

9일차를 기준으로 단일 라이벌과 플레이어 간의 1:1 쟁탈에서 벗어나, **Player·금태양·인기남이 서로 NPC 확보를 경쟁하는 기본 다세력 구조**가 완성되었다.
