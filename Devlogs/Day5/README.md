# Project θ 개발 일지 — 5일차

## 개발 목표

4일차에 완성한 최면 동행 시스템 위에 동행 NPC의 충동과 폭주 상태를 추가했다.

5일차 핵심 흐름은 다음과 같다.

```text
최면 완료
→ Following
→ 충동 상승
→ Warning
→ Danger
→ Preparing
→ Rampaging
→ Recovering
→ Following 복귀
```

## 충동 시스템

동행 중인 NPC마다 개별 충동 수치를 가지도록 ImpulseMeter를 추가했다.

초기 수치:

- 최대 충동: 100
- 기본 증가량: 초당 3
- Warning 시작: 65
- Danger 시작: 85
- NPC별 증가 속도 편차: 약 ±15%

NPC마다 충동 증가 속도를 조금 다르게 두어 여러 동행 NPC가 동시에 같은 타이밍에 폭주하는 현상을 줄였다.

## Impulse 상태

다음 상태를 추가했다.

- Calm
- Warning
- Danger
- Preparing
- Rampaging
- Recovering

일반 동행 상태에서는 충동이 지속적으로 증가한다.

충동이 100에 도달하면 폭주 준비 단계로 전환된다.

## 폭주 준비

폭주가 즉시 시작되지 않고 약 1초 동안 Preparing 상태를 거친다.

Preparing 상태에서는:

- 기존 Follower 이동 일시 중단
- NPC 정지
- 플레이어 방향 바라보기
- 하트 아이콘 숨김
- 느낌표 아이콘 표시

이 과정을 통해 실제 돌진 전에 플레이어가 위험 상태를 확인할 수 있도록 했다.

## 폭주

Preparing 상태가 끝나면 Rampaging 상태로 전환된다.

초기 수치:

- 돌진 속도: 8.4
- 폭주 지속시간: 1.8초
- 플레이어 접촉 판정 거리: 0.55

폭주 중에는 FollowerController 대신 ImpulseMeter가 Rigidbody2D 이동을 제어한다.

플레이어와 접촉하거나 폭주 시간이 끝나면 Recovering 상태로 전환된다.

## 동행 이동 제어권 분리

FollowerController에 외부 이동 제어 상태를 추가했다.

평상시:

```text
FollowerController
→ NPC 추적 이동
```

폭주 준비 및 폭주 중:

```text
FollowerController 일시 정지
→ ImpulseMeter가 Rigidbody2D 제어
```

폭주가 끝나면 다시 FollowerController에 이동 제어권을 반환한다.

기존의 자연스러운 동행 대열, 개인별 위치 편차, 흔들림, Soft Separation은 그대로 유지한다.

## 폭주 동시 실행 제한

RampageCoordinator를 추가했다.

현재 프로토타입에서는 한 번에 한 NPC만 Preparing / Rampaging 상태에 진입할 수 있다.

다른 NPC가 충동 100에 도달한 경우 현재 폭주가 끝날 때까지 Danger 상태에서 대기한다.

이를 통해 동행 NPC 수가 많아져도 여러 NPC가 동시에 폭주하여 테스트가 불가능해지는 상황을 방지했다.

## 회복

폭주가 끝나면 Recovering 상태로 전환된다.

초기 수치:

- 회복 시간: 1.1초
- 회복 후 충동: 20

Recovering 상태가 끝나면 다시 일반 Following 상태로 돌아가며 충동 상승을 재개한다.

## 상태 UI

NPC 하단 상태 게이지를 충동 시스템과 연결했다.

최면 전:

```text
보라색 최면 게이지
```

최면 완료 후:

```text
충동 게이지
+
하트 아이콘
```

폭주 준비 및 폭주 중:

```text
충동 게이지
+
느낌표 아이콘
```

폭주가 종료되면 느낌표가 사라지고 다시 하트 아이콘으로 돌아온다.

## 느낌표 아이콘

폭주 경고용 느낌표 이미지를 추가했다.

경로:

```text
Assets/_Project/Resources/UI/Hypnosis/Exclamation.png
```

느낌표 원본 이미지와 하트 원본 이미지 크기가 다르기 때문에 런타임에서 하트의 표시 크기를 기준으로 느낌표 크기를 자동 보정한다.

따라서 게임 화면에서는 하트와 느낌표가 같은 크기로 표시된다.

## 마우스 커서 크기 조정

기본 동전 커서와 최면 중 커서 모두 기존 대비 0.5배 크기로 조정했다.

```text
동전 커서 = 0.5배
최면 커서 = 0.5배
```

기존 Cursor.SetCursor 경고 방지를 위해 다음 방식은 그대로 유지한다.

```text
Resources Texture
→ RenderTexture 복사
→ RGBA32 Texture2D
→ MipMap 없음
→ Readable 유지
→ Cursor.SetCursor
```

## HUD

5일차 HUD에 충동 및 폭주 상태 확인 기능을 추가했다.

표시 항목:

- 현재 동행 인원
- 동행 NPC 중 최저 유지도
- 현재 최면 대상
- 현재 가장 높은 충동 수치
- 해당 NPC 이름
- 현재 폭주 중인 NPC

이를 통해 여러 NPC를 동시에 동행시킨 상태에서도 충동 시스템의 진행 상황을 확인할 수 있다.

## 테스트

ImpulseLogic EditMode 테스트를 추가했다.

검증 항목:

- 충동 증가 계산
- 최대값 100 Clamp
- Calm 판정
- Warning 판정
- Danger 판정

기존 이동, NPC AI, 최면, 동행, 유지도, Soft Separation 테스트는 유지한다.

## 5일차 완료 기준

다음 흐름을 기준으로 5일차 기능을 확인한다.

```text
NPC 최면
→ 하트 표시
→ 동행 시작
→ 충동 상승
→ Warning / Danger
→ 충동 100
→ 하트가 느낌표로 변경
→ 폭주 준비
→ 플레이어에게 돌진
→ 접촉 또는 시간 종료
→ Recovering
→ 느낌표 제거
→ 하트 복귀
→ 일반 동행 재개
```

## 다음 개발 방향

6일차에서는 현재 프로토타입의 개별 시스템을 스테이지 진행 구조와 연결한다.

예정 항목:

```text
정기 회수
→ 스테이지 시간 제한
→ 목표 정기량
→ 성공 / 실패
→ HUD 통합
```

5일차에서 구현한 폭주 결과를 이후 시간 및 자원 페널티와 연결할 예정이다.
