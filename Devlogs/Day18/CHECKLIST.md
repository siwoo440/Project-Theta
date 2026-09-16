# 한 판 통합 확인 목록

큰 기능을 넣은 뒤마다 이 목록을 **위에서부터 끊지 않고** 한 번 돈다.
어긋난 줄이 나오면 번호를 적어두고 멈춘다. 그 줄의 "어긋나면 의심할 곳"부터 본다.

- 준비: `Boot.unity`를 열고 재생
- 소요: 한 바퀴 약 12분 (두 번째 판 포함)
- 기준: 23일차 (`FloorPlanLogic.DefaultFloorCount = 4`, 금태양 2F · 인기남 3F)

> **처음 도는 경우**: 세이브를 지우고 시작하면 튜토리얼 항목까지 확인할 수 있다.
> 세이브 위치는 `Application.persistentDataPath/projecttheta_save.json`이다.

---

## 1. 타이틀 → 허브

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 1 | 재생 | 타이틀 "프로젝트 θ"가 **한글로** 보임 (네모 아님) | `UiFontProvider` |
| 2 | `시 작` | 허브로 넘어감 | `SceneBootstrapRouter`, `GameSession` |
| 3 | 허브 확인 | 계약 정기, 성장 4행, 난이도 3버튼, "밸런스 자산 적용됨" | `BalanceBootstrap` |
| 4 | 난이도 `보통` 선택 | 보통 버튼이 보라로 채워짐 | `HubScreen.RefreshDifficulty` |

## 2. 출격 → 1층 (연습 층)

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 5 | `출 격` → 지도에서 `출  발` | **게임이 멈추고 "시작 계약" 카드 3장**, 테두리 색이 서로 다름 | `RunProgression`, `RunUpgradeDrawLogic` |
| 6 | `1` 키 | 카드 선택 → 게임 재개, 좌상단 `Lv 1` | `RunUpgradeChoicePanel` |
| 7 | 1층 둘러보기 | **금태양·인기남 없음** | `OpponentFloorPlan` |
| 8 | 튜토리얼 안내 | 화면 위쪽 "E 또는 좌클릭을 유지해 NPC를 최면하세요" | `StageHudView.RefreshTutorial` |
| 9 | NPC 최면 | 좌상단 `+10`, 경험치 게이지 상승, 안내가 다음 단계로 | `RunExperienceLogic` |
| 10 | 2명 이상 동행 | 안내 "회수 지점으로 데려가야…" | `TutorialFlowLogic` |
| 11 | 오른쪽 끝 회수 지점으로 데려감 | 정기 증가, 경험치 `+30` 이상 | `StageSessionController.FlushPendingBatch` |
| 12 | 레벨업할 때까지 반복 | 게이지가 **흰색으로 번쩍** + "레벨 2 달성" 카드 | `StageHudView.RefreshLevel` |
| 13 | 최면(좌클릭)을 누른 채로 레벨업 | 카드가 **저절로 선택되지 않음** | `RunUpgradeChoicePanel.InputGuardSeconds` |
| 14 | 카드 화면에서 `1` · `2` 키 | 카드만 선택되고 **아이템은 쓰이지 않음** | `GameplayPause`, `PlayerConsumables` |

## 3. 2층 (금태양)

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 15 | 1층 오른쪽 계단 앞 | "[F] 2F로 올라가기" | `FloorStairway`, `FloorLayout` |
| 16 | `W` / `S` 키 | **층이 바뀌지 않음** | `FloorTransitionController.ReadInteractPressed` |
| 17 | `F` 키 | 2층 **왼쪽 계단 앞**에 도착, 동행자가 함께 옴, 벽이 약간 푸름 | `FloorTransitionController.MoveFollowers` |
| 18 | 도착 직후 | 경험치 `+25`, **카드는 안 나옴** | `RunProgression.HandleFloorChanged` |
| 19 | 2층 둘러보기 | **금태양 있음**, 인기남 없음 | `OpponentFloorPlan` |
| 20 | 금태양이 동행자를 뺏어감 | 되찾으면 경험치 `+25`, 안내가 폭주 단계로 | `StageScoreTracker.ReportHypnosisSuccess` |
| 21 | 금태양과 힘겨루기 승리 | 경험치 `+30` | `RunProgression.HandleDuelWon` |
| 22 | 2층 왼쪽 계단에서 `F` | 1층 **오른쪽 계단 앞**에 도착 | `FloorStairway.GetArrivalPosition` |
| 23 | 1층에서 3초 이상 대기 | **금태양이 계단으로 따라 내려옴** (이미 만났으므로) | `FloorTransitionController._metOpponents` |

## 4. 3층 (인기남) · 폭주

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 24 | 3층까지 올라감 | **인기남 있음** | `OpponentFloorPlan` |
| 25 | 인기남 행동 관찰 | **벽에 붙어 서 있지 않음** (다른 층 NPC를 노리지 않음) | `OpponentControllerBase.IsOnSameFloor` |
| 26 | 동행자 충동을 쌓아 폭주시킴 | 폭주 NPC가 달려옴 | `ImpulseMeter` |
| 27 | 붙잡히지 않고 도망침 | 폭주가 끝나면 경험치 `+20`, **튜토리얼 안내가 사라짐** | `RampageCoordinator.NotifySurvived` |
| 28 | 일부러 붙잡혀 봄 (다음 폭주에서) | 탈출 화면이 뜨고, 끝나도 **경험치 없음** | `ImpulseMeter.UpdateRampaging` |

## 5. 4층 → 결과

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 29 | 4층까지 올라감 | 우상단 층 표시 4칸이 모두 채워짐 | `StageHudView.RedrawFloorMarks` |
| 30 | 4층에서 3초 이상 대기 | 만난 경쟁자 둘 다 따라 올라옴 | `ChaseOpponentsToCurrentFloor` |
| 31 | 목표 정기 달성 | 클리어 → 결과 화면 | `StageEndController` |
| 32 | 결과 화면 | 도달 층 `4F (4개 층)`, 런 레벨, 강화 목록 | `StageResultPanel` |
| 33 | 클릭 | 연출 스킵, 랭크 도장 | `StageResultRevealLogic` |
| 34 | `지도로 (다음 구역)` (21일차부터. 판 전체 흐름은 6-3 구간) | 도시 지도로 돌아감 | `GameSession.GoTo` |

## 6. 허브 → 재출격 (이전 판이 남지 않는가)

**이 구간이 18일차 확인의 핵심이다.** 도메인 리로드가 꺼진 설정에서 이전 판의 값이 남는지를 본다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 35 | 허브 확인 | 직전 결과 카드에 방금 판 결과, 계약 정기 증가 | `HubScreen`, `ContractEssenceLogic` |
| 36 | 성장 하나 구매 | 잔액 감소, 핍 증가 | `SaveDataLogic.TryPurchaseUpgrade` |
| 37 | 다시 `출 격` → `출  발` | **"시작 계약" 카드가 다시 나옴**, `Lv 1`부터 시작 | `RunProgression` |
| 38 | 카드 선택 화면의 별 | 지난 판에 고른 카드도 **`NEW`로 표시** (스택이 남지 않음) | `RunUpgradeMultipliers` 초기화 |
| 39 | 1층 시작 | 층 표시가 **1F**, 경쟁자 없음 | `FloorVisibility`, `_metOpponents` |
| 40 | 튜토리얼 | 지난 판에 끝까지 마쳤다면 **안내가 안 뜸** | `SaveData.TutorialCompleted` |
| 41 | 게임이 멈춰 있지 않음 | 카드를 고른 뒤 정상 진행 | `GameplayPause` 초기화 |

## 6-1. 연출 (19일차 추가)

**가장 중요한 줄은 50번이다.** 레벨업 멈칫과 카드 화면의 완전 정지가 겹치는 곳이다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 47 | NPC 최면 성공 | 보라 파문이 퍼지고 **하트가 톡** 떠오름, 종소리 | `StageVfxDirector.HandleHypnosis` |
| 48 | 빼앗긴 NPC 되찾기 | 파문이 **두 겹**, 금색 파편 | `HandleHypnosis(wasReclaim)` |
| 49 | 3명 모아서 회수 | 금색 **빛기둥** (1명일 때보다 굵음), `+정기` 숫자가 **HUD 정기 게이지로 날아감**, 약한 흔들림 | `HandleRecovery`, `GameVfx.EssenceTarget` |
| 50 | **레벨업** | 금색 고리 + "LEVEL UP" + **아주 짧은 멈칫** → 카드 화면. **카드 화면이 떠 있는 동안 게임이 움직이지 않음** | `TimeScaleLogic.Resolve`, `GameplayPause.HitStop` |
| 51 | 카드 선택 | 게임이 정상 속도로 재개 (느린 채로 남지 않음) | `GameplayPause.TickHitStop` |
| 52 | 동행자 폭주 준비 | NPC 자리에 **붉은 파문 + "!"**, 화면 가장자리가 **붉게 맥동** | `HandleRampageWindup`, `GetDangerIntensity` |
| 53 | 폭주 피함 | "회피!" 글자 + 하늘색 파문 + 흔들림, 가장자리 맥동이 사라짐 | `HandleRampageSurvived` |
| 54 | 폭주에 붙잡힘 | **강한 흔들림** 한 번 | `HandleCapture` |
| 55 | 힘겨루기 승리 | 흰 파편 + 멈칫 + 흔들림, 묵직한 소리 | `HandleDuelWon` |
| 56 | 계단으로 층 이동 | 화면이 **잠깐 어두워졌다 밝아짐**, 발소리 | `HandleFloorChanged`, `GameVfx.Fade` |
| 57 | 다른 층에서 폭주·최면이 일어남 | **지금 층 화면에는 연출이 안 뜸** | `GameVfx.IsVisible` |
| 58 | 허브 → 설정 → `화면 흔들림 꺼짐` → 출격 | 흔들림만 사라지고 **파문·빛기둥은 그대로** | `CameraShake.Enabled`, `SaveData.ScreenShakeDisabled` |
| 59 | 게임 재실행 | 흔들림 꺼짐 설정이 유지됨 | `HubScreen.ToggleScreenShake` |
| 60 | 연출이 많이 겹치게 (체인 최면 + 대량 회수) | 멈추거나 끊기지 않음, 오래된 연출부터 사라짐 | `VfxRunner.MaximumWorld` |
| 62 | 집중력이 남은 상태로 최면 | 좌상단 "최면 가속 ×1.6", 게이지가 눈에 띄게 빨리 참 | `PlayerFocus.HypnosisSpeedMultiplier` |
| 63 | 최면을 계속 걸어 **집중력을 0으로** 만듦 | "기본 속도"로 바뀌고 **최면은 끊기지 않고 계속 차오름** | `HypnosisCaster`, `FocusLogic.GetHypnosisSpeedMultiplier` |
| 64 | 집중력 0에서 대시·최면 파동 | 쓸 수 없음 (집중력 부족), 최면은 여전히 가능 | `PlayerFocus.TrySpend` |
| 65 | 최면을 멈추고 잠시 기다림 | 0.8초 뒤부터 집중력 회복, 다시 가속 표시 | `PlayerFocus.Update` |

## 6-2. 디버그 패널 · UI 꾸미기 (20일차 추가)

**가장 중요한 줄은 67번과 71번이다.** 패널 위 클릭이 게임으로 새는지, 자산 원본이 몰래 바뀌는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 66 | 출격 → **F1** | 오른쪽에 디버그 패널, 상태 탭의 층·레벨·체력·집중력이 실제 값과 같음. 다시 F1이면 닫힘 | `DebugPanel.Update`, `DebugStatusTab` |
| 67 | 패널 위에서 **좌클릭 유지 · 우클릭 유지** | **최면·파동이 나가지 않음**. 패널 밖에서는 정상 | `PointerGuard`, `HypnosisCaster.ReadHypnosisHeld` |
| 68 | 수치 탭 → `일반 최면 배율`을 3으로 | 값이 금색 + `•`, "바뀐 값 1개", 최면이 **즉시** 빨라짐 | `BalanceTuningSession.Set` |
| 69 | 최면 묶음 `되돌리기` → `전부 되돌리기` | 슬라이더가 1.30으로 돌아가고 "자산 값 그대로입니다" | `ResetGroup`, `ResetAll` |
| 70 | 값을 바꾼 채로 **재생 종료 → 다시 재생** | 수치 탭이 전부 자산 값 (바꾼 값이 남지 않음) | `BalanceBootstrap.Reset` |
| 71 | 값을 바꾼 채로 재생 종료 → `StageBalanceDatabase.asset` 인스펙터 확인 | **자산 값은 그대로** (저장 버튼을 누르지 않았으므로) | `BalanceTuningSession.BeginEdit` 복사본 |
| 72 | 값을 바꾸고 `자산에 저장` → 재생 종료 → 인스펙터 | 자산에 바꾼 값이 들어가 있음. Git 변경에 `StageBalanceDatabase.asset`이 뜸 | `StageBalanceDatabase.SaveStageValues` |
| 73 | 치트 탭 → `3F` | 동행과 함께 3층 계단 앞 도착, 첫 방문 경험치 | `FloorTransitionController.DebugTravelTo` |
| 74 | `무적 ON` → 폭주에 붙잡힘 | 체력이 줄지 않음 | `PlayerHealth.TakeDamage`, `DebugCheats` |
| 75 | `레벨업` | **정확히 한 레벨** 오르고 카드 화면 | `DebugCheatTab.LevelUp` |
| 76 | `이 층 NPC 전부 최면` | 하트 연출과 함께 동행 합류, 넘친 인원은 최면이 풀림 | `HypnosisCaster.DebugClaim` |
| 77 | `가장 가까운 동행 폭주` | 곧바로 붉은 파문 + "!" | `ImpulseMeter.DebugFillImpulse` |
| 78 | 게임 속도 `×0.25` → 결과 화면 → 허브 → 재출격 | 허브와 다음 판은 **정상 속도** | `DebugPanel.OnDestroy`, 부트스트랩 `SetDebugSpeed(1)` |
| 79 | 치트를 쓴 뒤 다음 판 | 무적·무한이 **꺼져** 있음, 기록 탭 "치트 없음" | `DebugCheats.ResetForRun` |
| 80 | 판을 끝낸 뒤 기록 탭 → `기록 폴더 열기` | `RunLogs/날짜_시간.json`에 층별 시간 · 횟수 · `Cheated` | `RunStatsRecorder.Finish` |
| 81 | 집중력이 남은 상태 | HUD 집중력 게이지 **끝에서 하늘색 빛이 맥동**, 0이 되면 빛이 꺼짐 | `StageHudView` `_focusGlow` |
| 82 | 레벨업 카드에 마우스 올리기 | 카드가 살짝 커지고 **계열 색 빛**이 뒤로 번짐 | `UiHoverEffect.Glow` |
| 83 | S·A 랭크로 클리어 | 도장 뒤에 **랭크 색 빛 번짐** | `StageResultPanel._rankGlow` |
| 84 | 허브 · 타이틀 | 배경에 보라 빛 알갱이가 **천천히 떠오름**, 버튼에 마우스를 올리면 살짝 커짐 | `UiFloatingMotes`, `UiHoverEffect` |

## 6-3. 도시 지도 · 구역 흐름 (21일차 추가)

**가장 중요한 줄은 88번과 91번이다.** 구역을 넘어도 레벨·카드가 이어지는지, 끝난 판이 다음 출격에 남지 않는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 85 | 허브 → `출 격` | **도시 지도** 화면. 기업 연수원만 빛나고 "선택됨", 경로 줄 1칸 "기업 연수원" · 5칸 "루프탑 클럽" | `HubScreen` 출격, `MapScreen`, `RunRouteLogic` |
| 86 | `출  발` (또는 Enter) | 연수원 4층 시작, 우상단 "구역 1/5 · 기업 연수원 · 낮", 층 표시 "교육동 1F", 시작 계약 카드 | `ProjectThetaPrototypeBootstrap`, `LocationContext` |
| 87 | F1 → 치트 → `구역 즉시 클리어` → 결과 | 제목 "구역 1/5 · 기업 연수원 — CLEAR", 버튼 **"지도로 (다음 구역)"** | `StageResultPanel.RecordZoneInRun` |
| 88 | 지도로 → 후보 2곳 중 하나 선택 → 출발 | 경로 줄 1칸에 "기업 연수원 A". 새 장소는 **색이 다르고** 저녁/밤이면 화면이 주황/남색. **레벨·카드가 그대로이고 시작 계약 카드가 다시 뜨지 않음**. 튜토리얼 안내 없음 | `RunSession`, `RunProgression.Configure`, `TimeOfDayOverlay` |
| 89 | 지도 화면에서 숫자 키 `1` · `2` | 해당 후보가 선택되고 오른쪽 정보가 바뀜. 잠긴 장소는 눌리지 않음 | `MapScreen.ReadKeyboard` |
| 90 | 3·4구역을 거쳐 루프탑 클럽 클리어 | 결과 제목에 "한 판 종료 · 5구역 · 계약 정기 +N", 버튼 **"허브로"** | `RunSession.IsFinished` |
| 91 | 허브로 → 다시 출격 | 지도가 **1구역부터** 새로 시작, 경로 줄이 비어 있음, Lv 1 | `GameSession.EndRun`, `BeginRun` |
| 92 | 2구역에서 일부러 시간 초과 | 결과 버튼이 "허브로". 허브의 계약 정기에 **1구역 몫은 남아 있음** | `SceneFlowLogic.ShouldSaveOnTransition` |
| 93 | 지도에서 `판 포기하고 허브로` 한 번 | "한 번 더 누르면 포기합니다"로 바뀌고, 3초 안에 다시 누르면 허브 | `MapScreen.Abandon` |
| 94 | 에디터에서 `TestStage` 씬을 바로 재생 | 오류 없이 연수원(구역 1/5)으로 시작 | `GameSession.EnsureRunForStage` |
| 95 | 기록 폴더의 새 json | `Location`과 `Zone` 항목이 들어 있음 (구역마다 파일 1개) | `RunStatsRecorder` |

## 6-4. 방해 세력 · 구역 경계도 (22일차 추가)

**가장 중요한 줄은 97번과 104번이다.** 걷기만 해서는 들키지 않는지, 예고 없이 능력이 터지지 않는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 96 | 연수원 1F 시작 | 층마다 **교육 조교**(푸른빛)가 좌우 순찰, 바닥에 옅은 시야 부채꼴. 중상단 "경계도 평온" 막대 | `DisruptorSpawner`, `DisruptorStatusView` |
| 97 | 조교 시야 안에서 **걷기만** | `?`가 뜨지 않음, 부채꼴만 노랗게 진해짐 | `WatcherRole` 수상한 행동 판정 |
| 98 | 조교 시야 안에서 최면 유지 | `?`(노랑→주황) 약 0.8초 뒤 `!`(빨강) + 붉은 파문, 조교가 쫓아옴, 경계도 막대 상승 | `DetectionLogic.Advance`, `ZoneAlert.Add` |
| 99 | 조교 등 뒤에서 최면 | `?`가 뜨지 않음 | `DetectionLogic.IsInSight` |
| 100 | 경계도 30 이상 | "경계도 주의" 떠오름, 막대 노랑, 조교가 빨라짐 | `ZoneAlertLogic` 단계 효과 |
| 101 | 파동(우클릭)을 조교에게 맞힘 | 조교 머리 위 `zZ` 3초, 시야 부채꼴 사라짐, 경계도 -15 | `HypnosisWaveCaster.StunNearbyDisruptors` |
| 102 | 발각 없이 가만히 있음 | 경계도가 천천히 내려감 (초당 4) | `ZoneAlert.LateUpdate` |
| 103 | F1 → 치트 `경계도 +30` 세 번 | "비상! 증원 · 회수 지점 잠김", 화면 흔들림, **조교 1명 추가**가 복도 끝에서 옴, 회수 지점 붉게 10초 잠김, 막대가 경계(60)로 내려옴 | `ZoneAlert.TriggerEmergency`, `RecoveryPoint.LockAll` |
| 104 | 3F **인사팀 평가관**(금색 ◆ 이름표) 앞에 동행자를 데려감 | 머리 위 "근태 체크! ■■□□…" **1.2초 예고** → 동행자 머리 위 "근태 체크 10" | `AttendanceCheckAbility`, `SpecialAbilityLogic` |
| 105 | 표식 붙은 동행자를 가까이 둠 | 유지도가 천천히 줄어듦 (디버그 상태 탭 최저 유지도) | `FollowerController` 표식 처리 |
| 106 | 표식이 붙은 채 회수 | 그 동행자 정기 20% 적게 들어옴 | `StageSessionController.TryRecoverFollower` |
| 107 | 평가관 예고 중 파동 맞힘 | 예고가 멈추고 멍함이 끝나면 이어서 진행 (예고를 건너뛰지 않음) | `SpecialAbilityLogic.Tick` |
| 108 | 연수원에서 40초 대기 | "쉬는 시간!" + 8초 동안 복도 NPC가 빨라짐 | `BreakTimeBell` |
| 109 | 2F 이상 "자습실 · 뛰지 마세요" 구역에서 대시 | "쉿!" + 경계도 +10 (감시자가 안 봐도) | `QuietRoomZone` |
| 110 | 지도로 아직 채우지 않은 장소(지하철역 · 헬스장 · 쇼핑몰 등) 이동 | 방해 세력 · 경계도 막대 · 쉬는 시간 **없음** | `DisruptorCatalog.GetPlacements` |
| 111 | F1 상태 탭 | "방해 세력" 묶음에 경계도 수치 · 증원 횟수 · 가까운 적 상태 | `DebugStatusTab.RefreshDisruptors` |

## 6-5. 해변가 · 야시장 (23일차 추가)

**가장 중요한 줄은 114번과 124번이다.** 그늘에 숨으면 정말 안 들키는지, 소매치기를 쫓아가 되찾을 수 있는지다.
2구역 지도에서 해변가, 3구역 이후 야시장을 고른다. 경로에 없으면 새 판을 시작해 다시 고른다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 112 | 해변가 시작 | 모래빛 바닥, 파라솔 4개(색 천 + 바닥 그늘), 가운데 **망루**와 금색 ◆ **라이프가드 반장**, 오른쪽 끝 "샤워장", 라이프가드 2명 순찰, 중상단 아래 "다음 밀물까지 N초 · 동시 운반 …" | `CreateBeachRules`, `DisruptorCatalog.AddBeach` |
| 113 | 라이프가드 시야 안에서 최면 | 연수원 조교처럼 `?` → `!` → 경계도 상승 (시야가 더 김) | `WatcherRole` |
| 114 | **파라솔 그늘 안에서** 라이프가드를 마주 보고 최면 | `?`가 뜨지 않음, 시야 부채꼴도 진해지지 않음 | `ParasolShade.IsShaded`, `WatcherRole` 그늘 판정 |
| 115 | 반장 시야(넓은 부채꼴)에서 들킴 | "호루라기 경보! ■■…" **1초 예고** → 붉은 큰 파문, 주변 NPC 머리 위 "경계!", 경계도 +25 | `WhistleAlarmAbility` |
| 116 | "경계!" NPC에게 최면 | 게이지가 평소 절반 속도, 8초 뒤 "경계!"가 사라지면 원래 속도 | `WhistleAlarm`, `HypnosisTarget.BuildPerSecond` |
| 117 | 동행자 4명 이상을 데리고 헌팅남 근처로 | 헌팅남 2명이 **맨 뒤 두 명** 옆에 붙고 머리 위 "♥ N%"가 오름. 가득 차면 "헌팅당했다!" + 그 동행자 이탈 | `ContesterRole`, `ClaimLogic` |
| 118 | 헌팅남이 붙은 동행자 곁으로 플레이어가 감 | "♥ N%"가 빠르게 줄어듦 | `ClaimLogic.GuardDistance` |
| 119 | 20초쯤 기다림 | "파도가 온다!" + 물빛 깜빡임 3초 → "밀물!" 물이 화면 아래쪽 줄을 덮음, 그 줄로 못 내려감, 8초 뒤 빠짐 | `TideCycle`, `TideLogic` |
| 120 | 동행자 1~2명씩 나눠 회수 / 3명 이상 한 번에 회수 | 나눠 오면 정기가 절반, 한 번에 오면 1.2배 | `LocationObjectiveLogic.GetBatchMultiplier` |
| 121 | 야시장 시작 | 화면이 어둡고 **노점 4개**(천막 · 이름) 앞에 주황 등불 빛, 등불 사이는 어두움 | `CreateNightMarketRules`, `LanternLight` |
| 122 | 등불 빛 안 / 어두운 틈에서 NPC를 최면 | 어두운 곳에서는 **더 가까이 가야** 최면이 걸림 | `HypnosisCaster.FindBestTarget` 사거리 배율 |
| 123 | 동행자를 데리고 걷기 | 동행자가 **한 줄**로 따라옴 | `FollowerManager.SingleFile` |
| 124 | 동행자를 데리고 이름표 없는 회색 손님(소매치기)에게 다가감 | "✋ 등 뒤!" 1초 → "소매치기! 정기 -N 잡아라!" + 흔들림, 이름표 "◆ 소매치기" 드러남, 동행자 머리 위 "털림 -30%", HUD "소매치기 도주 중 20초" | `PickpocketAbility`, `PickpocketMark` |
| 125 | 도주하는 소매치기에게 닿음 (대시 추천) | "되찾았다! +50", "털림" 표식 전부 사라짐, 소매치기 퇴장 | `PickpocketAbility.Resolve` |
| 126 | (새 구역) 124 뒤 20초 동안 쫓지 않음 | "소매치기가 인파 속으로 사라졌다", 표식은 남아 회수 정기 70% | `PickpocketLogic.EscapeSeconds` |
| 127 | 노점 앞 3m를 동행자와 지나감 | 호객꾼 "시식하고 가세요~", 동행자 하나가 멈추고 "호객 중 6"… 돌아가 1초 곁에 있으면 "되찾았다!", 6초 방치하면 "노점에 붙잡혔다!" + 이탈 | `StallToutRole`, `StallHoldLogic` |
| 128 | 비틀거리는 **취객**이 동행자와 부딪침 / 취객에게 대시 | "휘청! 충동 +15" / "밀침!" + 취객 1m 밀려나 잠깐 멈춤 | `BlockerRole`, `ImpulseMeter.AddImpulse` |
| 129 | **촬영팀**(◆, 앞에 흰 조명) 조명 안에서 최면 | "라이브 방송! ■■…" 1초 → 조명이 붉어지고 머리 위 "● LIVE", 경계도 +30, 8초 동안 따라옴. 조명 안에서는 사거리가 온전함 | `LiveBroadcastAbility` |
| 130 | 촬영팀에게 파동 | `zZ` + 조명이 꺼짐, 방송 중이었다면 끊김 | `LiveBroadcastAbility.LateUpdate` |
| 131 | 야시장에서 경계도 +30 치트 세 번 | 비상 · 회수 지점 잠금은 걸리지만 **증원은 오지 않음** | `DisruptorCatalog.GetReinforcementKind` |
| 132 | F1 치트 `밀물 즉시` (해변가) · `소매치기 발동` (야시장, 동행자 데리고) | 바로 119 · 124번 흐름이 시작됨 | `DebugCheatTab.StartTide` · `TriggerPickpocket` |

## 7. 에디터 재생 종료 → 다시 재생

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 42 | **카드 화면이 떠 있는 상태로** 재생 종료 | - | - |
| 43 | 다시 재생 → 출격 | 게임이 **멈춘 채로 시작하지 않음** | `GameplayPause.ResetOnPlayModeEnter` |
| 44 | 효과음 | 버튼 틱·도장·구매 소리가 남 | `GameAudio.ResetOnPlayModeEnter` |
| 45 | 한글 | 여전히 한글로 보임 | `UiFontProvider.ResetOnPlayModeEnter` |
| 46 | Test Runner → Run All | 전부 통과 | - |
| 61 | **레벨업 멈칫 도중** 재생 종료 → 다시 재생 | 게임이 **느린 채로 시작하지 않음** | `GameplayPause.ForceClear`, `VfxRunner.OnDestroy` |

---

## 기록

| 날짜 | 일차 | 통과 | 어긋난 번호 | 메모 |
| --- | --- | --- | --- | --- |
| | 18 | | | |
