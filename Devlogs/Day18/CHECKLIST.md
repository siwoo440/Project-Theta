# 한 판 통합 확인 목록

큰 기능을 넣은 뒤마다 이 목록을 **위에서부터 끊지 않고** 한 번 돈다.
어긋난 줄이 나오면 번호를 적어두고 멈춘다. 그 줄의 "어긋나면 의심할 곳"부터 본다.

- 준비: `Boot.unity`를 열고 재생
- 소요: 한 바퀴 약 12분 (두 번째 판 포함)
- 기준: 37일차 (`FloorPlanLogic.DefaultFloorCount = 4`, 금태양 2F · 인기남 3F)

> **처음 도는 경우**: 세이브를 지우고 시작하면 튜토리얼 항목까지 확인할 수 있다.
> 세이브 위치는 `Application.persistentDataPath/projecttheta_save.json`이다.

---

## 1. 타이틀 → 허브

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 1 | 재생 | 타이틀 "프로젝트 θ"가 **한글로** 보임 (네모 아님) | `UiFontProvider` |
| 2 | `이어하기` 또는 `처음부터` (34일차) | 허브로 넘어감 | `SceneBootstrapRouter`, `GameSession` |
| 3 | 허브 확인 (36일차: 학생의 방) | 위 띠에 계약 정기. `강 화` 창에 성장 4행, `난이도` 창에 3버튼, `통 계` 창에 "밸런스 자산 적용됨" | `BalanceBootstrap` |
| 4 | 난이도 `보통` 선택 | 보통 버튼이 보라로 채워짐 | `HubScreen.RefreshDifficulty` |

## 2. 출격 → 1층 (연습 층)

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 5 | `출 격` → 지도에서 `출  발` | **게임이 멈추고 "시작 계약" 카드 3장**, 테두리 색이 서로 다름 | `RunProgression`, `RunUpgradeDrawLogic` |
| 6 | `1` 키 | 카드 선택 → 게임 재개, 좌상단 `Lv 1` | `RunUpgradeChoicePanel` |
| 7 | 1층 둘러보기 | **금태양·인기남 없음** | `OpponentFloorPlan` |
| 8 | 튜토리얼 안내 | 화면 위쪽 "좌클릭을 유지해 NPC를 최면하세요" (32일차: 최면 키 설정을 따름, 기본 좌클릭만) | `StageHudView.RefreshTutorial` |
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
| 34 | `지도로` (29일차부터 `허브로` 버튼과 나란히. 흐름은 6-3 구간) | 도시 지도로 돌아감 | `GameSession.GoTo` |

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
| 58 | 허브 위 `설 정` → `화면 흔들림 꺼짐` → 출격 | 흔들림만 사라지고 **파문·빛기둥은 그대로** | `CameraShake.Enabled`, `SaveData.ScreenShakeDisabled` |
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

## 6-3. 도시 지도 흐름 (21일차 추가, 29일차에 판 없이 다시 씀)

**가장 중요한 줄은 88번과 91번이다.** 장소마다 레벨·카드가 새로 시작하는지, 허브에 다녀와도 아무 장소나 고를 수 있는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 85 | 허브 → `출 격` | **도시 지도** 화면. 장소 8곳 모두 누를 수 있고 배지 "[1] 새 장소" · 클럽은 "[8] 보스". 위쪽에 구역 경로 줄 · 판 포기 버튼이 **없음**. 오른쪽 위 `통  계` · `허브로` (35일차: 장소 칸에 난이도 ◆, 오른쪽 패널은 6-17 참고) | `HubScreen` 출격, `MapScreen`, `RunRouteLogic.GetLocations` |
| 86 | 기업 연수원 선택 → `출  발` (또는 Enter) | 연수원 4층 시작, 우상단 "기업 연수원 · 낮"(구역 번호 없음), 층 표시 "교육동 1F", 시작 계약 카드 | `GameSession.BeginLocation`, `LocationContext` |
| 87 | F1 → 치트 → `장소 즉시 클리어` → 결과 | 제목 "기업 연수원 — CLEAR", 아래 버튼 **`지도로` · `허브로`** 두 개 | `StageResultPanel` |
| 88 | 지도로 → 아무 장소 선택 → 출발 | **Lv 1 · 카드 0장으로 시작**하고 시작 계약 카드가 **다시 뜸**. 저녁/밤이면 화면이 주황/남색. 튜토리얼 안내 없음 | `RunSession`, `RunProgression.Configure`, `TimeOfDayOverlay` |
| 89 | 지도 화면에서 숫자 키 `1`~`8` | 해당 장소가 선택되고 오른쪽 정보 · "내 기록"이 바뀜. **클리어한 장소도 다시 고를 수 있음** | `MapScreen.ReadKeyboard` |
| 90 | 같은 장소를 두 번 연속 도전 | 지도가 마지막 장소를 미리 골라 둠. 두 번째도 Lv 1부터. 배지가 "클리어 N회 · 최고 등급" | `MapScreen.Start`, `PlayStatsLogic.Get` |
| 91 | 결과 → `허브로` → 다시 출격 | 지도에서 **아무 장소나** 고를 수 있음(1구역부터 다시 시작하는 개념 없음) | `GameSession.ClearRun` |
| 92 | 일부러 시간 초과 → 결과 | 버튼이 여전히 `지도로` · `허브로`. 지도로 가서 바로 다른 장소에 도전 가능 | `SceneFlowLogic.ShouldSaveOnTransition` |
| 93 | 지도에서 `허브로` | 바로 허브로 감(확인 없음) | `MapScreen.GoToHub` |
| 94 | 에디터에서 `TestStage` 씬을 바로 재생 | 오류 없이 연수원으로 시작 | `GameSession.EnsureRunForStage` |
| 95 | 기록 폴더의 새 json | `Location`과 `Zone`(장소 단계 0~4) 항목이 들어 있음 (도전마다 파일 1개) | `RunStatsRecorder` |

## 6-4. 방해 세력 · 구역 경계도 (22일차 추가)

**가장 중요한 줄은 97번과 104번이다.** 걷기만 해서는 들키지 않는지, 예고 없이 능력이 터지지 않는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 96 | 연수원 1F 시작 → 20초 대기 | 시작 직후에는 적이 없고, 20초쯤 흰 파문과 함께 층마다 **교육 조교**(푸른빛)가 나타나 좌우 순찰, 바닥에 옅은 시야 부채꼴. 중상단 "경계도 평온" 막대 | `DisruptorSpawner`, `DisruptorStatusView` |
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
| 110 | 지도로 모든 장소를 한 번씩 이동 | 8곳 모두 방해 세력 · 경계도 막대가 **있음**, 연수원에서만 쉬는 시간 | `DisruptorCatalog.GetPlacements` |
| 111 | F1 상태 탭 | "방해 세력" 묶음에 경계도 수치 · 증원 횟수 · 가까운 적 상태 | `DebugStatusTab.RefreshDisruptors` |

## 6-5. 해변가 · 야시장 (23일차 추가)

**가장 중요한 줄은 114번과 124번이다.** 그늘에 숨으면 정말 안 들키는지, 소매치기를 쫓아가 되찾을 수 있는지다.
지도에서 해변가 · 야시장을 바로 고른다 (모든 장소 열기).

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 112 | 해변가 시작 | 모래빛 바닥, 파라솔 4개(색 천 + 바닥 그늘), 가운데 **망루**와 금색 ◆ **라이프가드 반장**, 오른쪽 끝 "샤워장", 20초마다 적이 한 명씩 등장(라이프가드 → 헌팅남 → 반장 → 드론 촬영자), 중상단 아래 "다음 밀물까지 N초 · 동시 운반 …" | `CreateBeachRules`, `DisruptorCatalog.AddBeach` |
| 113 | 라이프가드 시야 안에서 최면 | 연수원 조교처럼 `?` → `!` → 경계도 상승 (시야가 더 김) | `WatcherRole` |
| 114 | **파라솔 그늘 안에서** 라이프가드를 마주 보고 최면 | `?`가 뜨지 않음, 시야 부채꼴도 진해지지 않음 | `ParasolShade.IsShaded`, `WatcherRole` 그늘 판정 |
| 115 | 반장 시야(넓은 부채꼴)에서 들킴 | "호루라기 경보! ■■…" **1초 예고** → 붉은 큰 파문, 주변 NPC 머리 위 "경계!", 경계도 +25 | `WhistleAlarmAbility` |
| 116 | "경계!" NPC에게 최면 | 게이지가 평소 절반 속도, 8초 뒤 "경계!"가 사라지면 원래 속도 | `WhistleAlarm`, `HypnosisTarget.BuildPerSecond` |
| 117 | 동행자를 데리고 헌팅남 근처로 | 헌팅남이 **맨 뒤 동행자** 옆에 붙고 머리 위 "♥ N%"가 오름. 가득 차면 "헌팅당했다!" + 그 동행자 이탈 | `ContesterRole`, `ClaimLogic` |
| 118 | 헌팅남이 붙은 동행자 곁으로 플레이어가 감 | "♥ N%"가 빠르게 줄어듦 | `ClaimLogic.GuardDistance` |
| 119 | 20초쯤 기다림 | "파도가 온다!" + 물빛 깜빡임 3초 → "밀물!" 물이 화면 아래쪽 줄을 덮음, 그 줄로 못 내려감, 8초 뒤 빠짐 | `TideCycle`, `TideLogic` |
| 120 | 동행자 1~2명씩 나눠 회수 / 3명 이상 한 번에 회수 | 나눠 오면 정기가 절반, 한 번에 오면 1.2배 | `LocationObjectiveLogic.GetBatchMultiplier` |
| 121 | 야시장 시작 | 화면이 어둡고 **노점 4개**(천막 · 이름) 앞에 주황 등불 빛, 등불 사이는 어두움. 적은 20초마다 한 명씩(호객꾼 → 소매치기 → 촬영팀 → 취객) | `CreateNightMarketRules`, `LanternLight` |
| 122 | 등불 빛 안 / 어두운 틈에서 NPC를 최면 | 어두운 곳에서는 **더 가까이 가야** 최면이 걸림 | `HypnosisCaster.FindBestTarget` 사거리 배율 |
| 123 | 동행자를 데리고 걷기 | 동행자가 **한 줄**로 따라옴 | `FollowerManager.SingleFile` |
| 124 | 동행자를 데리고 이름표 없는 회색 손님(소매치기)에게 다가감 | "✋ 등 뒤!" 1초 → "소매치기! 정기 -N 잡아라!" + 흔들림, 이름표 "◆ 소매치기" 드러남, 동행자 머리 위 "털림 -30%", HUD "소매치기 도주 중 20초" | `PickpocketAbility`, `PickpocketMark` |
| 125 | 도주하는 소매치기에게 닿음 (대시 추천) | "되찾았다! +50", "털림" 표식 전부 사라짐, 소매치기 퇴장 | `PickpocketAbility.Resolve` |
| 126 | (새 구역) 124 뒤 20초 동안 쫓지 않음 | "소매치기가 인파 속으로 사라졌다", 표식은 남아 회수 정기 70% | `PickpocketLogic.EscapeSeconds` |
| 127 | 호객꾼이 선 노점(두 번째) 앞 3m를 동행자와 지나감 | 호객꾼 "시식하고 가세요~", 동행자 하나가 멈추고 "호객 중 6"… 돌아가 1초 곁에 있으면 "되찾았다!", 6초 방치하면 "노점에 붙잡혔다!" + 이탈 | `StallToutRole`, `StallHoldLogic` |
| 128 | 비틀거리는 **취객**이 동행자와 부딪침 / 취객에게 대시 | "휘청! 충동 +15" / "밀침!" + 취객 1m 밀려나 잠깐 멈춤 | `BlockerRole`, `ImpulseMeter.AddImpulse` |
| 129 | **촬영팀**(◆, 앞에 흰 조명) 조명 안에서 최면 | "라이브 방송! ■■…" 1초 → 조명이 붉어지고 머리 위 "● LIVE", 경계도 +30, 8초 동안 따라옴. 조명 안에서는 사거리가 온전함 | `LiveBroadcastAbility` |
| 130 | 촬영팀에게 파동 | `zZ` + 조명이 꺼짐, 방송 중이었다면 끊김 | `LiveBroadcastAbility.LateUpdate` |
| 131 | 야시장에서 경계도 +30 치트 세 번 | 비상 · 회수 지점 잠금은 걸리지만 **증원은 오지 않음** | `DisruptorCatalog.GetReinforcementKind` |
| 132 | F1 치트 `밀물 즉시` (해변가) · `소매치기 발동` (야시장, 동행자 데리고) | 바로 119 · 124번 흐름이 시작됨 | `DebugCheatTab.StartTide` · `TriggerPickpocket` |

## 6-6. 지하철 환승역 · 헬스장 · 드론 (24일차 추가)

**가장 중요한 줄은 135번과 143번이다.** 지하철이 정기가 아니라 열차 생존으로 끝나는지, 고인물을 대시로 받아칠 수 있는지다.
지도에서 지하철역 · 헬스장을 바로 고른다 (모든 장소 열기).

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 133 | 해변가 80초 뒤 (또는 치트 `적 최대 인원 등장`) | 왼쪽에 ◆ **드론 촬영자**, 공중에 드론(검은 막대)이 원을 그리며 날고 바닥에 옅은 흰 원 | `DroneTrackingAbility` |
| 134 | 드론 원 안에 3초 서 있기 (파라솔 그늘 안이어도) | 원이 노랗게 짙어짐 → "추적 촬영! ■■…" 1.5초 깜빡임 → 원이 붉게 플레이어를 따라옴, 머리 위 "● REC", 원 안에 있는 동안 경계도 상승. 추적 중 파라솔 아래로 가면 "드론이 놓쳤다" / 촬영자에게 파동 → "드론 착륙!" 이후 드론 멈춤 | `DroneLogic`, `ParasolShade` |
| 135 | 지하철 시작 | 층마다 노란 승강장 선 · "1호선 승강장", 가운데 **개찰구 기둥**과 역무원, 중상단 아래 "열차 0/3 · 다음 열차 N초 · 생존: …" | `CreateSubwayRules`, `TrainArrival` |
| 136 | 약 41초 대기 | "열차가 들어옵니다" → 45초에 "열차 도착 (1/3) · 인파 주의" + 약한 흔들림, 층마다 빈자리만큼 회색 **휴대폰 행인**이 양쪽에서 걸어 나와 끝에 닿으면 사라짐 | `TrainArrival.ReleaseCommuters`, `CommuterRole` |
| 137 | 행인이 동행자와 부딪침 / 행인에게 대시 | 동행자 머리 위 "툭" + 옆 줄로 밀림 / 행인이 옆으로 밀려나 잠깐 멈춤 | `CommuterRole` |
| 138 | 동행자 **5명 이상**으로 개찰구를 건넘 | 역무원 머리 위 "한 명씩! (N)", 뒤따르던 동행자가 개찰구 앞에서 멈췄다가 **1초에 한 명씩** 건너옴. 4명 이하면 그냥 통과 | `GateRole`, `GateLogic` |
| 139 | 열차 문이 열린 5초 동안 5명 이상으로 개찰구 통과 | 검사 없이 모두 통과, HUD "열차 도착! 인파에 섞이면 개찰구 통과" | `TrainArrival.IsCrowdRush` |
| 140 | 층 끝의 **전단지 알바** 바로 앞으로 동행자와 지나감 | "헬스장 3개월 반값이에요~", 동행자 하나가 2초 멈춤("전단지 받는 중…") → "다시 출발", **동행자를 잃지 않음** | `StallToutRole` 전단지 모드 |
| 141 | 20초쯤 뒤 1F 전광판 | "♪ 안내 방송 → → →" 3초 → "승강장 변경! 인파 …" 5초 동안 동행자가 화살표 쪽으로 끌려가고 느려짐. 그동안 파동 → "파동으로 버티는 중" 끌려가지 않음 | `PlatformChangeAbility`, `CrowdFlow` |
| 142 | 열차 3대(약 135초)까지 동행자 1명 이상 유지 | 3대째 "열차 3대 통과! 버텼다" 뒤 **CLEAR** (정기가 목표에 못 미쳐도). 동행자가 0명이면 계속 진행, 시간이 끝나면 실패 | `ObjectiveStateLogic`, `StageSessionController.EvaluateState` |
| 143 | 헬스장 1F에서 동행자를 데리고 가운데 **헬스 고인물**에게 다가감 | 고인물이 따라와 머리 위 "으랏차!" 0.8초 → 곁에 있으면 "밀려났다!" + 1.4m 밀림 + 1.5초 못 움직임. 자세 중 **대시**하면 "받아치기! 고인물 퇴장" + 30초 `zZ` | `BrawlerRole`, `PlayerSideViewController.ApplyStagger` |
| 144 | 헬스장 구역 | 1F 왼쪽 주황 "운동 구역 · 최면↑ 충동↑"(러닝머신), 오른쪽 보라 "요가실 · 충동 없음"(매트), 2F 왼쪽 파란 "수영장", 오른쪽 운동 구역 | `GymZone`, `GymLayout` |
| 145 | 운동 구역 / 요가실 NPC 최면, 동행자를 데리고 각 구역에 서 있기 | 운동 구역은 빨리 걸림 · 충동 빨리 참 / 요가실은 느리게 걸림 · 충동이 오르지 않음 | `HeartRateLogic`, `ImpulseMeter`, `HypnosisTarget` |
| 146 | 층마다 금색 "★ 대회 앞둔 선수" NPC 최면 → 회수 | 최면이 절반 속도, 회수하면 "★ 선수 함락 +60" 반짝임 · 정기 게이지로 날아감 | `AthleteMark`, `StageSessionController.TryRecoverFollower` |
| 147 | **퍼스널 트레이너** 옆 NPC를 트레이너가 보는 앞에서 최면 | 트레이너 "회원님!" 머리 위 "회원님!" → 달려와 "정신 차려요! 게이지 절반". 트레이너가 반대쪽을 볼 때는 달려오지 않음 | `RescuerRole`, `RescueLogic` |
| 148 | 2F에서 동행자를 데리고 있기 (시작 15초 이후) | ◆ **관장** "단체 PT 호출! ■■…" 1.5초 → "단체 PT 시작!", 운동 구역 바닥이 진한 주황, HUD "단체 PT 10초 · …". 요가실에 있는 동행자는 충동이 오르지 않음 | `GroupPtAbility`, `GymZone.BeginPt` |
| 149 | 2F 수영장 NPC를 최면하기 시작 | ◆ **수영장 코치** "전원 입수! ■■…" 1.2초 → 파란 파문, 수영장 NPC 게이지 0 + 머리 위 "잠수 중" 5초 동안 최면이 안 걸림 | `AllInAbility`, `HypnosisBlock` |
| 150 | F1 치트 `열차 즉시` · `방송 즉시` (지하철) | 바로 136 · 141번 흐름 | `DebugCheatTab.ArriveTrain` · `TriggerAnnouncement` |
| 151 | 지하철 → 다음 구역 / 헬스장 → 다음 구역 | 인파 흐름 · 단체 PT가 다음 구역에 남지 않음 | `DisruptorSpawner.Create` 상태 초기화 |
| 152 | 아무 장소에서 F1 상태 탭 "방해 세력" 줄을 보며 대기 | "층당 0/4명"에서 시작해 20초마다 1씩 올라 80초에 "4/4", 어느 층도 적이 4명을 넘지 않음(열차 행인 · 증원 포함). 플레이어 바로 옆(3m)에는 새 적이 생기지 않음 | `PopulationLogic`, `DisruptorSpawner.SpawnDue` |
| 153 | F1 치트 `적 최대 인원 등장` / 수치 탭 "적 증가 간격(초)"를 0으로 | 층마다 남은 적이 바로 등장 / 0으로 바꾸면 그 자리에서 바로 최대 인원까지 등장 | `DisruptorSpawner.DebugFillAll`, `DisruptorRampSeconds` |

## 6-7. 쇼핑몰 · 오피스 타워 (25일차 추가)

**가장 중요한 줄은 157번과 164번이다.** 보안팀장이 복도를 막았을 때 셔터를 뚫고 지나갈 수 없는지, 출입증 없이는 위층 계단이 잠기는지다.
빨리 보려면 F1 치트 `적 최대 인원 등장`을 먼저 누른다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 154 | 쇼핑몰 시작 | 층마다 천장 **CCTV**(작은 상자 + 바닥 부채꼴, 3초마다 방향 전환), 셔터 틀 · 붉은 경광등 2곳, 반투명 **기둥** 2개, 왼쪽 "보안실" 표지 · "[보안실]" 직원. HUD "폐점까지 N초 · 잠입: …" | `CreateMallRules`, `DisruptorCatalog.AddMall` |
| 155 | CCTV 부채꼴 안에서 최면 | `?` → `!` → "무전: 요원 N명 출동", 같은 층 **보안요원** 머리 위 "무전 출동" + 발견 지점으로 달려옴. HUD "발각됨 · 잠입 보너스 없음" | `RadioLink`, `WatcherRole.Spotted` |
| 156 | 보안요원 한 명에게 파동 후 다른 감시자에게 들킴 | "무전 끊김", 5초 동안은 다른 요원이 달려오지 않음 | `RadioLogic.CutSeconds` |
| 157 | 경계도 60 이상(치트 `경계도 +30` 두 번)에서 2F ◆ **보안팀장** 등장 뒤 | "셔터 봉쇄! ■■…" 1.5초 경광등 깜빡임 → 플레이어 층 셔터 2곳이 12초 닫힘. **걷기 · 대시로 통과 불가**, 동행자도 막힘. HUD "셔터 봉쇄 N초" | `ShutterLockAbility`, `ShutterGate` |
| 158 | 기둥 그림자 안에서 CCTV · 요원을 마주 보고 최면 | `?`가 뜨지 않음 | `ParasolShade.ConfigurePillar` |
| 159 | "[보안실]" 직원을 최면 | "보안실 장악 · CCTV N대 30초 정지", 그 층 CCTV에 `zZ`, 부채꼴 사라짐 | `SecurityRoomLink` |
| 160 | 1F **판촉 직원** 앞을 동행자와 지나감 | "시음해 보고 가세요~", 동행자 3초 멈춤 뒤 "다시 출발", 잃지 않음 | `StallToutRole` 판촉 모드 |
| 161 | 한 번도 들키지 않고 회수 / 들킨 뒤 회수 | 들키지 않았으면 정기 ×1.3 | `StealthLogic`, `ZoneAlert.WasSpotted` |
| 162 | 제한 시간 70% (또는 치트 `폐점 즉시`) | "폐점 안내 방송 · 시간이 빨리 흐릅니다" + 흔들림, 남은 시간이 1.3배 빨리 줄어듦 | `MallClosing`, `StageSessionController.TimeFlowMultiplier` |
| 163 | 오피스 타워 시작 | 층마다 "탕비실"(갈색 탁자) · 파란 "회의실" 구역, 위층 계단 앞 유리문 + 붉은 불 "출입증 게이트 · [출입증] 직원 필요", NPC 머리 위 "[출입증]" 1명 · "방문객" 여러 명, 3F "★ 대표 비서" | `CreateOfficeRules`, `AssignNpcRoles` |
| 164 | 출입증 직원 없이 위층 계단에서 F | "출입증이 필요합니다 · 한 번 더 누르면 비상계단 (경계도 +20)", 계단 안내 "출입증 게이트 잠김". 2.5초 안에 다시 F → 올라가고 경계도 +20 | `FloorTransitionController.TryUse`, `PassGate.IsLocked` |
| 165 | "[출입증]" 직원을 최면해 데리고 다님 | "출입증 확인 · 게이트 열림", 불이 초록, 계단이 그냥 열림. HUD "출입증 게이트 1/2 열림" | `PassGate.Update` |
| 166 | **꼰대 부장** 옆(4m)에서 NPC 최면 | "이거 오늘까지 해! (최면 N명 초기화)" + 주황 파문, 범위 안 게이지 0. 가끔 "커피 좀 마시고 올게" → 탕비실에서 "커피 휴식" 동안은 외치지 않음 | `ManagerRole`, `ManagerLogic` |
| 167 | 탕비실 근처에서 **사내 인기남**이 동행자에게 붙음 | 머리 위 "♥ N%"가 다른 곳보다 두 배 빨리 참, 가득 차면 "사내 인기남에게 넘어갔다!" | `ContesterRole`, `MallOfficeValues.GetPantryMultiplier` |
| 168 | 제한 시간 35% · 70% (또는 치트 `정전 즉시`) | "전등이 깜빡인다…" 3초 깜빡임 → "정전!" 8초 화면 어두움. 조교 · 부장 등은 못 보고 **야근 경비원**만 발각, 최면 사거리 짧아짐 | `Blackout`, `BlackoutLogic.CanSee` |
| 169 | 직원 동행자를 데리고 있기 (시작 20초 이후) | ◆ **비서실장** "긴급 회의 소집! ■■…" → 직원 최대 2명 머리 위 "회의 중 12…" 회의실로 걸어감, "방문객"은 남음. 12초 뒤 다시 따라옴. 그 사이 회의실에 들어가면 "회의 중 난입! 경계도 +40" | `EmergencyMeetingAbility`, `MeetingSummons` |
| 170 | 3F "★ 대표 비서" 최면 → 회수 | 최면 절반 속도, "★ 특수 대상 함락 +80" | `AthleteMark.Configure` |
| 171 | F1 치트 `게이트 모두 열기` | 두 게이트가 초록으로 바뀌고 계단이 열림 | `PassGate.DebugOpenAll` |
| 172 | 오피스 → 다른 장소 | 사내 인기남 탕비실 배율 · 계단 잠금이 남지 않음 | `OfficeLayout.Active`, `PassGate` 목록 |

## 6-8. 루프탑 클럽 · 보스전 (26일차 추가)

**가장 중요한 줄은 178번과 181번이다.** 세력이 우세해야만 보스에게 최면이 걸리는지, 보스를 함락해야만 클리어되는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 173 | 루프탑 클럽 시작 | 금태양 · 인기남이 **나오지 않음**. 1F "BAR", 계단 앞 "VIP 라운지 · 동행 6명까지", "[VIP]" 손님 1명. 2F "DJ 부스", "댄스플로어 · 라이벌 서큐버스의 영역", 보라빛 ◆ **라이벌 서큐버스**. 위에 보스 패널, 왼쪽 아래 정신력, 오른쪽 위 "템포 1 · 드롭까지 N초" | `CreateClubRules`, `BossHudView` |
| 174 | 8초 대기 | "♪ DROP" + 흔들림, 바닥 빛 번쩍, 동행자 충동 +8, 드롭 직후 1.5초 동안 최면이 빨라짐. 3초마다 바닥이 희게 번쩍임(점멸) | `ClubBeat`, `ClubBeatLogic` |
| 175 | 동행자 7명 이상으로 1F 위층 계단에서 F | "바운서: 동행 6명을 넘으면 입장 불가 …", 계단 안내 "바운서가 막고 있습니다". 6명 이하 / "[VIP]"를 데리고 / 바운서에게 파동 → 올라감 | `VipEntrance`, `BouncerLogic` |
| 176 | 클럽 MD 옆에 동행자를 둔 채 드롭 | 드롭 직후 "♥ N%"가 세 배로 빨리 참 | `ContesterRole` × `ClubBeat.ContestMultiplier` |
| 177 | 2F에서 대기 | 라이벌이 1초마다 가까운 손님을 보라 파문과 함께 자기 편으로 만듦(게이지 색 보라). 보스 패널 "라이벌 세력"이 오름. 30초 뒤 ◆ **DJ** "템포 업! ■■…" 2초 → "템포 2단계", 드롭 간격 짧아짐. DJ에게 파동 → "템포 다운" | `RivalSuccubus.Claim`, `TempoUpAbility` |
| 178 | 내 세력이 라이벌의 1.2배 **미만**일 때 라이벌 곁(4.5m)에서 최면 | 평소처럼 주변 NPC가 걸림 (보스는 안 걸림), 패널 "세력 쟁탈 · 손님을 먼저 확보하세요" | `BossBattle.ShouldCaptureFocus` |
| 179 | 라이벌 편 손님을 되찾거나 1F에서 동행자를 늘려 세력 1.2배 이상 → 라이벌 곁에서 최면 유지 | 패널 "보호막 공격 가능 · 최면 중", 노란 막대가 6초에 차면 보라 파편 + "보호막 파괴! 남은 2장", ◆◆◇ | `BossBattleLogic.AdvanceShield` |
| 180 | 보호막 3장을 모두 깸 | "보호막 전부 파괴! 본체를 최면하세요", 패널 "본체 최면", 막대가 분홍 지배 게이지로 바뀜. 놓으면 천천히 빠짐. 라이벌 기술이 더 자주 나옴 | `BossPhase.Dominate` |
| 181 | 지배 게이지 100 | "라이벌 서큐버스 함락!", 빛기둥 · 멈칫 · 흔들림 → **CLEAR**. 함락 전에는 정기가 목표를 넘어도 끝나지 않음 | `BossBattle.Defeat`, `ObjectiveStateLogic` 보스 분기 |
| 182 | 라이벌 앞 같은 줄에 서 있기 | 라이벌 머리 위 "역최면 시선! ■■…" + 붉은 가는 선 1.5초 → 굵은 빔 1.2초, 맞으면 정신력 감소. 줄을 바꾸면 안 맞음, **바닥이 희게 번쩍이는 순간은 안 맞음**(빔이 옅어짐) | `ReverseGazeAbility`, `BossSkillValues.GazeHits` |
| 183 | 동행자를 데리고 라이벌 10m 안 | 무리 자리에 보라 원 1.2초 → "매혹 파동 (N명)", 원 안 동행자 머리 위 "매혹 40%". 세 번 쌓이면 "라이벌에게 매혹됐다!" + 이탈. 파동을 쓰면 매혹 표시가 사라짐 | `CharmWaveAbility`, `RivalCharm` |
| 184 | 정신력 0 (수치 탭 "정신력 피해 배율" 3으로 올리면 빠름) | "정신력 붕괴! 동행자를 잃었다", 5초 못 움직임, 동행자 2명이 라이벌 편으로, 정신력 50으로 회복. 30초 안에 한 번 더 → 실패(체력 0) | `PlayerMind.Collapse`, `MindLogic` |
| 185 | F1 치트 `드롭 즉시 · 템포 +1` / `보스 진행` | 바로 드롭 · 템포 상승 / 누를 때마다 보호막 한 장, 다 깨지면 지배 +50 | `DebugCheatTab.DropAndTempo` · `AdvanceBoss` |
| 186 | 클럽 → 다른 장소 | 보스 HUD · 드롭 · 정신력이 남지 않음, 최면이 평소대로 NPC에 걸림 | `BossBattle.OnDestroy`(FocusOverride 해제) |

### 6-8b. 보스 기술 추가 · 엘리베이터 정원 (27일차 추가)

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 187 | 보스전 시작 18초 뒤 | "샴페인 타워! ■■…" 1.5초 → 2F 바에 금색 타워, "샴페인 타워 · 손님이 바로 모임", 그 층 손님이 바 쪽으로 끌려가고 라이벌이 두 배로 빨리 가져감. 10초 뒤 사라짐 | `ChampagneTowerAbility` |
| 188 | 타워가 선 동안 2F "[바텐더]"를 최면 | "바텐더를 빼앗아 샴페인 타워가 무너졌다!" + 금색 파편, 즉시 사라짐 | `ChampagneTowerAbility.IsBartenderTaken` |
| 189 | 보스전 시작 25초 뒤 같은 층에 있기 | 1초 화면 어두워짐 → 라이벌 양옆에 똑같은 이름의 **분신 2명**, 분신도 시선 빔을 쏨(피해 절반). 본체만 발밑 그림자. HUD "분신 2명 · 그림자 있는 쪽이 진짜" | `CloneDancerAbility`, `CloneDancer` |
| 190 | 분신에게 파동 | "분신 소멸!", 15초가 지나거나 보스가 함락돼도 사라짐 | `CloneDancer.Update` |
| 191 | 보스전 시작 35초 뒤 같은 층에 있기 | 플레이어 양옆에 보라 로프 1.5초 깜빡임 → 15초 동안 보라 구역, "VIP 구역 N초 · 안에서 최면 절반", 구역 안 NPC 최면이 느림. DJ가 멍한 순간이면 "VIP 구역 (DJ 멍함 · 절반)" 7.5초 | `VipZoneAbility`, `HypnosisTarget.BuildPerSecond` |
| 192 | 보스 결과 화면 | 제목에 "· 라이벌 서큐버스 함락" / 실패 시 "· 라이벌 보호막 N장 남음" | `StageResultPanel.FillTexts` |
| 193 | 오피스에서 동행자 5명 이상으로 계단 이동 | 4명만 함께 옮겨지고 "엘리베이터 정원 4명 · N명은 아래층에서 대기 (20초)". 남은 동행자 머리 위 "엘리베이터 대기 20…", 제자리에서 멈춤 | `FloorTransitionController.MoveFollowers`, `ElevatorWait` |
| 194 | 20초 안에 그 층으로 돌아감 / 돌아가지 않음 | 대기 표시가 사라지고 다시 따라옴(다음 이동 때 4명 한도 안에서 함께 탐) / "기다리다 떠났다" + 이탈 | `ElevatorWait.Update` |
| 195 | 오피스 외 장소에서 5명 이상으로 계단 이동 | 인원 제한 없이 모두 함께 이동 | `OfficeLayout.Active` |

## 6-9. 장소별 맵 디자인 (27일차 추가)

**가장 중요한 줄은 204번이다.** 맵이 바뀌어도 이동 범위 · 계단 · 벤치 · 자판기 자리 판정이 그대로인지다.
지도에서 장소를 하나씩 골라 1F(층이 여럿이면 2F도)를 본다. 그림 파일이 없으면 도형으로 그려진다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 196 | 기업 연수원 | 흰 벽 · 파란 경계선, "강의실 A/B" 문, 화이트보드 · "교육 일정", 화분 · 정수기, 회색 카펫 격자 · 파란 안내선, 대기 의자 · 커피 머신. **사물함 · 게시판 · 교실 문이 없음** | `TrainingCenterDecor` |
| 197 | 해변가 | 벽 대신 하늘 · 해 · 구름 · 수평선 바다 · 요트, 야자수 3그루, "SNACK" 오두막, 모래 점무늬 · 아래쪽 젖은 모래 · 파도 거품, 비치타월, 선베드 · 아이스박스, 나무 데크 계단 | `BeachDecor`, `MapBaseLayers` 하늘 |
| 198 | 지하철 환승역 | 흰 타일 벽 · 노선 색 띠(1F 파랑 · 2F 초록), "시티 환승역 · N호선", 스크린도어 8칸(개찰구 앞은 비어 있음), 노선도 · 광고판, 노란 점자 블록, 역 벤치 · 승차권 발매기, 에스컬레이터 난간 | `SubwayDecor` |
| 199 | 헬스장 | 콘크리트 벽 · 전신 거울 3개 · "NO PAIN · NO GAIN", 검정 고무 매트, 정수기 · 수건 걸이, 벤치 프레스 · 단백질 음료 기계. 2F 왼쪽은 파란 수영장 타일 벽 · 레인 줄 | `FitnessDecor` |
| 200 | 야시장 | 밤하늘 · 달 · 건물 실루엣과 창 불빛, "포차" · "24시" 네온, 늘어진 전구줄, 노점 위 김, 보도블록 · 물웅덩이, 박스 · 드럼통 불 · "OPEN" 입간판, 플라스틱 테이블 · 음식 수레 | `NightMarketDecor` |
| 201 | 쇼핑몰 | 크림색 벽, 브랜드 간판 쇼윈도 3개(마네킹 2개씩), 층 안내판 "NF", 광택 대리석 바닥 · 빛 띠, 몰 벤치 · 안내 키오스크, 에스컬레이터 난간. 셔터 자리 · 보안실 표지를 가리지 않음 | `MallDecor` |
| 202 | 오피스 타워 | 창밖 야경 띠 · "THETA CORP.", 파티션 · 모니터 책상(일부만 켜짐), 복사기, 형광등 절반 꺼짐, 네이비 카펫, 라운지 소파 · 정수기, **엘리베이터 문 계단**. 탕비실 · 회의실 · 게이트를 가리지 않음 | `OfficeDecor`, 계단 모양 |
| 203 | 루프탑 클럽 | 도시 야경 · 별 · 서치라이트, 유리 난간, "ROOFTOP BAR"(1F) · "VIP LOUNGE"(2F) 네온, 스피커 · 라운지 소파 · 화분, 어두운 LED 격자 바닥(드롭 때 규칙 빛이 위에 번쩍), VIP 소파 · 칵테일 테이블 | `ClubDecor` |
| 204 | 각 장소에서 벽 끝 · 계단 · 벤치 · 자판기 자리로 걸어가기 | 이동 범위 · 계단 [F] · 벤치(5.2) · 자판기(−10.2) 자리 막힘이 **연수원과 똑같음**. 층 이동 · 회수 지점 정상 | `MapBaseLayers` 충돌체, `SchoolHallwayPrototypeBuilder` |
| 205 | 캐릭터가 소품 앞뒤를 지나감 | 벤치 · 자판기 대체 소품과 앞뒤가 자연스러움. 간판 글자가 캐릭터 · 머리 위 글자보다 뒤 | `MapPainter.Prop`, `Label` |
| 206 | (선택) `Resources/Maps/Beach/Background.png`를 넣고 재생 | 해변 하늘 · 바다 도형 대신 그 그림이 벽 자리에 깔림 | `MapArtLibrary` |

## 6-10. 로딩창 · 소품 색 · 맵 최적화 (28일차 추가)

**가장 중요한 줄은 207번이다.** 모든 화면 전환이 로딩창을 거치고, 전환 뒤 게임이 멈춘 채로 남지 않는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 207 | 메인 메뉴 → 허브 → 지도 → 스테이지 → 결과 → 지도/허브를 차례로 넘김 | 넘길 때마다 어두운 로딩창이 뜨고, 새 화면이 다 지어진 뒤 0.25초 동안 흐려지며 사라짐. 넘긴 뒤 조작 · 시간 흐름 정상 | `LoadingScreen`, `GameSession.GoTo` |
| 208 | 로딩창 가운데 | 주인공이 가운데 **로딩 바 위를** 바의 왼쪽 끝에서 오른쪽 끝까지 달림(바가 차오르는 끝과 같은 가로 위치). 바가 100%일 때 바 오른쪽 끝에 도착해 멈춰 서고, 잠깐(0.35초) 보인 뒤 넘어감. 로딩이 빨라도 1.4초쯤은 달리는 모습이 보임 | `LoadingScreenLogic.GetRunnerX` |
| 209 | 로딩창 서큐버스 | 주인공 **왼쪽 위**에서 살짝 늦게 따라오며 위아래로 둥실거림. 그림이 없으면 보라색 날개 · 뿔 · 꼬리 실루엣 | `LoadingScreen.BuildCompanion` |
| 210 | 로딩창 아래 · 오른쪽 아래 | 주인공 아래 로딩 바가 줄어들지 않고 100%까지 차고 옆에 %. 오른쪽 아래 "TIP" 패널 문장이 3초마다 바뀌고 같은 문장이 연달아 나오지 않음 | `LoadingScreenLogic`, `LoadingTips` |
| 211 | 로딩창이 뜬 동안 버튼 · 키를 연타 | 두 번 넘어가거나 오류가 나지 않음 | `LoadingScreen.IsLoading` |
| 212 | 야시장 · 해변가 · 오피스 · 클럽 · 헬스장 · 쇼핑몰의 규칙 소품 | 노점 · 망루 · 탕비실 · 바 · DJ 부스 · 러닝머신 · 셔터 틀 · 게이트 기둥 색이 맵 톤과 어울리고 바닥에 묻히지 않음. 큰 소품 위에 장소 강조색 테두리 한 줄 | `LocationProps.Structure`, `MapPropTint` |
| 213 | 각 장소 바닥 무늬 | 27일차와 같은 줄눈 · 모래 알갱이 · 벽돌 · LED 칸 · 지하철 점자 블록이 보임(한 장으로 구운 그림) | `FloorPatternLayout`, `FloorPatternBaker` |
| 214 | 8곳을 돌며 Console 확인 | `[Map] … 예산 … 초과` 경고가 없음. Profiler에서 스테이지 진입 프레임이 27일차보다 가벼움 | `MapBudget`, `SchoolHallwayPrototypeBuilder` |

## 6-11. 통계 · 보스 엔딩 (29일차 추가)

**가장 중요한 줄은 217번이다.** 장소를 끝내고 지도로 돌아오면 통계 숫자가 바로 늘어나는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 215 | 지도에서 `통  계` 버튼, 또는 장소를 고르지 않은 상태에서 Tab | 지도 위에 어두운 막과 "플레이 통계" 창. 위에 카드 5장(전체 / 최면 · 동행 / 정기 / 위기 / 성장), 아래 장소 8곳 표(도전 · 클리어 · 최고 정기 · 최단 클리어 · 최고 등급). 처음이면 0 · "-" | `StatsPanel`, `PlayStatsLogic.BuildSections` |
| 216 | 통계 창에서 Esc · Tab · `닫기` | 창이 닫히고 곧바로 다시 열리지 않음. 창이 열린 동안 숫자 키 · Enter로 지도가 반응하지 않음 | `StatsPanel.Close`, `MapScreen.Update` |
| 217 | 연수원을 한 번 클리어 → `지도로` → 통계 | 장소 도전 1 · 클리어 1 · 클리어율 100% · 최면 성공 수 · 최대 동행자 · 회수한 정기가 늘어남. 연수원 줄에 최단 클리어 시간 · 등급 | `StageResultPanel.SubmitResultToSession`, `PlayStatsLogic.Apply` |
| 218 | 한 번 실패 → 통계 | 도전만 늘고 클리어는 그대로, 클리어율이 내려감. 최단 클리어 시간은 바뀌지 않음 | `PlayStatsLogic.Apply` |
| 219 | F1 치트를 쓴 뒤 클리어 → 통계 | 통계 숫자가 **늘지 않음**(계약 정기 · 허브 기록은 예전처럼 반영) | `StageResultSummary.Cheated` |
| 220 | 게임 종료 → 다시 실행 → 통계 | 숫자가 그대로 남아 있음. 예전 세이브로 시작해도 오류 없이 0부터 | `SaveData.Stats`, `PlayStatsLogic.Normalize` |
| 221 | 루프탑 클럽 보스 함락 | 빛기둥 · 흔들림 뒤 1.2초 후 화면이 어두워지고 금색 "ENDING", 문장 4줄이 한 줄씩 뜸 → 잠시 뒤 걷히고 결과 화면(제목 "라이벌 서큐버스 함락") | `EndingSequence`, `EndingLogic` |
| 222 | 엔딩 문장이 뜨는 중 클릭 · Space | 바로 걷히는 단계로 넘어감. 엔딩 전 1.2초 · 걷히는 중에는 클릭이 무시됨 | `EndingLogic.Skip` |
| 223 | 엔딩 뒤 `지도로` → 통계 | 엔딩(보스 함락) 1회. 지도에서 계속 다른 장소를 고를 수 있음 | `PlayStats.Endings` |

## 6-12. 업적 · 장소 숙련도 · 엔딩 해금 (30일차 추가)

**가장 중요한 줄은 226번과 230번이다.** 숙련도가 목표 · 보상에 실제로 반영되는지, 엔딩 뒤 시작 계약이 4장으로 늘어나는지다.
세이브를 지우고 시작하면 첫 업적 알림부터 볼 수 있다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 224 | 새 세이브로 연수원 클리어 → `지도로` | 위쪽 가운데 금테 알림 "업적 달성! 첫 출근" → 이어서 "첫 계약"이 차례로 뜨고 사라짐. 허브 계약 정기에 보상(+20, +30)이 더해짐 | `AchievementToast`, `GameSession.TakeNewAchievements`, `AchievementLogic.UnlockNew` |
| 225 | 지도 장소 배지 | "[1] ☆☆☆ 새 장소" / 클리어한 곳 "[1] ★☆☆ 클리어 1 · B". 오른쪽 정보창 첫 줄 "숙련 ★☆☆ (다음 ★까지 클리어 2회)" | `MapScreen.RefreshNodes`, `MasteryLogic` |
| 226 | ★1 장소를 고름 → 정보창 · 출발 | 목표 정기가 기본값의 105%로 표시되고 "(★1 · 보상 +10%)". 스테이지 HUD 목표 정기도 같은 값. 클리어 시 계약 정기가 10% 더 많음 | `MasteryLogic.GetTargetEssence`, `StageResultPanel.SubmitResultToSession` |
| 227 | 같은 장소 3번 · 6번 클리어 | ★★☆ → ★★★, 목표 +10% → +15%. ★★★이 되면 "단골 장소" 업적 | `MasteryLogic.StarClears` |
| 228 | 지도 `통계` 창 | 장소별 표 오른쪽 끝에 "숙련" 열(★) | `PlayStatsLogic.BuildLocationRow` |
| 229 | 허브 → `통 계` 창 · `업적 보기` (36일차: 카드가 창으로 바뀜) | 창에 플레이 시간 · 도전 · 클리어(율) · 업적 N/49 · 엔딩 상태. 버튼을 누르면 업적 49개가 한 줄씩 늘어선 창(32일차, 마우스 휠 · 오른쪽 막대로 스크롤), 달성은 금색 "달성", 나머지는 진행 막대와 "값 / 목표". Esc · 닫기로 닫힘 | `HubScreen.RefreshStats`, `AchievementPanel` |
| 230 | 루프탑 클럽 보스 함락(엔딩) 뒤 아무 장소 출발 | 시작 계약 카드가 **4장**(모두 다른 계열), 키 1~4로 고를 수 있음. 허브 카드에 "해금: 시작 계약 카드 4장". 레벨업 카드는 여전히 3장 | `MasteryLogic.GetStartChoiceCount`, `RunProgression.OpenNextChoice`, `RunUpgradeChoicePanel` |
| 231 | 지도를 거쳐 허브로 | 허브 "직전 도전" 카드에 장소 이름 · 결과 · 계약 정기가 보임(스테이지에서 바로 허브로 오지 않아도) | `GameSession.LastResult` |
| 232 | 게임 종료 → 다시 실행 | 업적 · ★이 그대로이고 알림이 다시 뜨지 않음 | `SaveData.UnlockedAchievements` |

## 6-13. 일시정지 · 설정 · 조작법 · 종료 (31일차 추가)

**가장 중요한 줄은 235번과 238번이다.** Esc 메뉴를 닫은 뒤 게임이 멈춘 채로 남지 않는지, 포기하면 계약 정기가 0인지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 233 | 메인 메뉴 | `시 작` 아래에 `설 정` · `업 적` · `종 료` 세 버튼, 그 아래 기록 패널 (32일차: 조작법 → 업적) | `MainMenuScreen.BuildMenuRow` |
| 234 | 메인 메뉴 `종 료` | "한 번 더"로 바뀌고 3초 안에 다시 누르면 재생이 멈춤(실행 파일이면 꺼짐). 3초가 지나면 "종 료"로 돌아감 | `MainMenuScreen.Quit` |
| 235 | 스테이지에서 Esc → `계속하기` 또는 Esc | 게임이 멈추고 "일시정지" 창(장소 · 목표 표시). 닫으면 **바로 다시 움직임**(타이머 · NPC · 이동) | `PauseMenu`, `GameplayPause` |
| 236 | 카드 선택 중 · 결과 화면 · 보스 엔딩 · 로딩 중 Esc | 일시정지 창이 뜨지 않음 | `PauseMenuLogic.CanOpen` |
| 237 | 일시정지 → `조작법` → Esc → Esc | 조작법 창(이동 · 대시 · F · 최면 · 파동 · 회수 · 탈출 · 카드 · 지도 · Esc)과 아래에 이번 장소 설명 · 방해 세력. 첫 Esc는 조작법만, 두 번째 Esc는 일시정지를 닫음 | `ControlsPanel`, `UiEscapeStack` |
| 238 | 일시정지 → `포기하기` 두 번 | "한 번 더 누르면 포기합니다" → 결과 화면 제목 "… — GAVE UP", 계약 정기 **+0**, `지도로` · `허브로`. 통계에 도전 1회 · 클리어 0 | `StageSessionController.Abandon`, `PauseMenuLogic.GetContractEssence` |
| 239 | 설정 창(메인 메뉴 · 허브 위 `설 정` · 일시정지) | 전체 음량 · 효과음 · 음악 슬라이더와 %, 화면(전체화면 · 창 모드), 해상도 3개, 커서 크기 3개, 화면 흔들림 켜짐 · 꺼짐. 고른 값은 보라색 | `SettingsPanel` |
| 240 | 효과음 · 전체 음량을 끌기 | 끄는 동안 "딱" 소리가 점점 작아짐. 0%면 게임 효과음이 들리지 않음 | `GameAudio.SfxVolume`, `SettingsApplier` |
| 241 | 스테이지에서 커서 크기 작게 · 크게 | 동전 커서 · 최면 커서가 바로 작아지거나 커짐 | `HypnosisCursorController.HandleSettingsChanged` |
| 242 | (36일차: 허브 설정 카드가 없어짐) 설정 창 화면 흔들림 끔 → 창을 닫고 다시 열기 | 여전히 "꺼짐" | `SettingsPanel`, `SaveData.ScreenShakeDisabled` |
| 243 | 설정 바꿈 → 게임 종료 → 다시 실행 | 음량 · 커서 · 흔들림이 그대로. 예전 세이브로 시작해도 소리가 남(80%) | `SaveData.SettingsInitialized`, `SettingsLogic.Normalize` |
| 244 | (실행 파일) 해상도 · 창 모드 바꾸기 | 창 크기가 1280×720 · 1600×900 · 1920×1080으로 바뀌고 UI 배치가 유지됨. 에디터에서는 바뀌지 않음(정상) | `SettingsApplier.ApplyScreen` |
| 245 | 지도 통계 창 · 허브 업적 창에서 Esc | 창만 닫히고 다른 것은 열리지 않음 | `StatsPanel`, `AchievementPanel` |

## 6-14. 키 설정 (32일차 추가)

**가장 중요한 줄은 248번과 250번이다.** 바꾼 키로 실제 조작이 되는지, 겹치는 키는 경고만 뜨고 저장되지 않는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 246 | 설정 창 열기 | 위에 `일반` · `키 설정` 탭. 처음엔 `일반`(31일차 내용 그대로) | `SettingsPanel.ShowPage` |
| 247 | `키 설정` 탭 | 기본(위 · 아래 · 왼쪽 · 오른쪽 이동, 대시, 층 이동) / 최면(최면, 파동) / 위기(탈출 A · B) / 아이템(1 · 2) 줄, 기본 · 보조 칸. 기본값: W/↑ · S/↓ · A/← · D/→ · Shift/Space · F · **마우스 왼쪽(보조 없음)** · 마우스 오른쪽 · 마우스 왼쪽 · 마우스 오른쪽 · 1 · 2 | `InputBindingLogic.CreateDefault` |
| 248 | 층 이동 기본 칸 → G → 스테이지에서 G | 칸이 "키를 누르세요…"(보라) → "G", 초록 "'층 이동 · 상호작용' → G". 계단 앞 안내가 "[G] 2F로 올라가기", G로 층 이동, F는 반응 없음 | `GameInput`, `FloorTransitionController`, `FloorStairway` |
| 249 | 최면 보조 칸 → E → 스테이지 | 좌클릭과 E 둘 다로 최면이 됨. 최면 커서도 E에 반응 | `HypnosisCaster`, `HypnosisCursorController` |
| 250 | 대시 기본 칸 → W | 빨간 경고 "W 키는 이미 '위로 이동'에서 쓰고 있습니다. 저장하지 않았습니다." 대시 칸은 Shift 그대로 | `InputBindingLogic.TrySet` (Conflict) |
| 251 | 아무 칸 → Esc / 아무 칸 → F1 | Esc: "취소했습니다."(창은 닫히지 않음). F1: "바꿀 수 없는 키" 경고 | `SettingsPanel.UpdateListen`, `ReservedCodes` |
| 252 | 탈출 A 칸 → 마우스 오른쪽 / → Q | 오른쪽: 탈출 B와 겹쳐 경고. Q: 저장되고, 붙잡혔을 때 안내가 "Q → 우클릭 → …", Q · 우클릭 번갈아 탈출 | `GetContext`, `CaptureHudView`, `PlayerCaptureController` |
| 253 | 파동 기본 칸 → 마우스 가운데 | HUD "파동(휠클릭 유지)", 휠 버튼을 누르고 있으면 파동 | `HypnosisWaveCaster`, `StageHudView` |
| 254 | 아이템 1 → Z | 화면 아래 아이템 칸 글자가 "Z", Z로 사용 (스테이지를 다시 들어가면 반영) | `PlayerConsumables`, `StageHudView` |
| 255 | 보조 칸 옆 `×` | 보조 키가 비워지고 `×`가 사라짐. 기본 칸에는 `×`가 없음 | `InputBindingLogic.Clear` |
| 256 | `기본값으로 되돌리기` 두 번 | "한 번 더 누르면 되돌립니다" → 모든 칸 기본값, 초록 안내 | `SettingsPanel.ResetKeys` |
| 257 | 키 바꿈 → 게임 종료 → 다시 실행 | 바꾼 키 그대로. 조작법 창(허브 · 일시정지)에도 바뀐 키가 보임, 아이템 1 · 2 줄 있음 | `SaveData.KeyBindings`, `ControlsCatalog.Build` |
| 260 | 업적 창의 새 업적(개근상 · 무결점 · 번개 출근 · 모범 연수생 · 루프탑의 주인 · 올 S · 대박 · 계약 부자 · 불굴 · 성장통 등) | 한 줄에 이름 · 설명 · +보상 · 진행 막대 · 값/목표가 겹치지 않음. 휠로 끝까지 내려가면 "밤샘 근무"가 마지막, 다시 열면 맨 위부터. 90초 안에 클리어하면 "번개 출근", 연수원 S면 "모범 연수생", 빼앗김 없이 클리어하면 무결점 진행도가 오름 | `AchievementLogic` (32일차 23개 추가), `AchievementPanel` |
| 259 | 메인 메뉴 `업 적` | 허브와 같은 업적 창(달성 N/46, 한 줄 목록 · 스크롤, 진행 막대). Esc · 닫기로 닫힘. 메인 메뉴에 조작법 버튼이 없음 | `MainMenuScreen`, `AchievementPanel` |
| 258 | 일시정지 → 설정 → 키 설정에서 칸을 누르고 마우스 클릭 | 게임이 멈춘 채라 최면 · 탈출 입력이 게임에 들어가지 않음 | `GameplayPause.IsPaused` |

## 6-15. 목표 달성 뒤 탈출 · 목표 정기 조정 · 밸런스 보고서 (33일차 추가)

**가장 중요한 줄은 262번과 264번이다.** 목표를 채워도 바로 끝나지 않는지, 1F 회수 지점에서 탈출하면 클리어되는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 261 | 지도에서 장소 정보 | 목표 정기가 연수원 310 · 해변가 330 · 지하철 330 · 헬스장 350 · 야시장 370 · 쇼핑몰 370 · 오피스 400 · 클럽 440 (★이 있으면 그만큼 더) | `LocationCatalog` |
| 262 | 연수원에서 목표 정기까지 모으기 | 결과 화면이 뜨지 **않고** 게임이 계속됨. 위쪽에 금색 띠 "목표 달성! … 남아서 더 모으면 보상 ↑ (초과 +N · 남은 N초)", 1F 회수 지점에 금빛 기둥 · "EXIT [F]" | `StageExitLogic.Resolve`, `ExitBanner`, `ExitGate` |
| 263 | 탈출 가능 상태에서 2F 이상에 있기 | 띠 문구가 "1F로 내려가 회수 지점에서 탈출". 2F 회수 지점에서는 F를 눌러도 탈출하지 않음(동행자 회수는 계속 됨) | `StageExitLogic.CanExit` |
| 264 | 1F 회수 지점 안에서 F | 결과 화면, 제목 "… — CLEAR · 탈출 성공 · 초과 정기 +N". 마지막으로 데려온 동행자도 정기에 들어감 | `StageSessionController.RequestExit` |
| 265 | 목표를 채운 뒤 탈출하지 않고 시간 끝까지 | 남은 15초부터 띠가 붉게 깜빡임. 시간이 끝나면 **클리어**, 제목 "· 시간 종료 (목표 달성)" | `StageExitLogic`, `ExitBanner` |
| 266 | 목표를 채운 뒤 적에게 쓰러짐 | 실패(FAILED - HP) | `StageExitLogic.Resolve` |
| 267 | 지하철 · 루프탑 클럽 | 탈출 띠 · EXIT 기둥이 없음. 열차 생존 · 보스 함락으로 예전처럼 바로 끝남 | `StageExitLogic.UsesExit` |
| 268 | 키 설정에서 층 이동 · 상호작용을 G로 바꾼 뒤 탈출 | 기둥 글자 "EXIT [G]", 띠 "[G] 탈출", G로 탈출됨 | `GameInput.ShortLabel` |
| 269 | F1 치트 `장소 즉시 클리어` | 목표 채움 + 바로 탈출(결과 화면 "탈출 성공") | `DebugCheatTab.ClearZone` |
| 270 | 탈출 가능 상태에서 Esc → 설정 → 조작법 | 조작법 창에 "탈출 (목표 달성 뒤) · 1F 회수 지점에서 F" 줄 | `ControlsCatalog` |
| 271 | 장소 몇 곳을 끝낸 뒤 F1 → 기록 탭 → `보고서 만들기` | 금색 한 줄 "장소 N곳 · 기록 N개 · 평균 m:ss → report.md". 기록 폴더의 `report.md`에 장소별 기록 수 · 클리어율 · 탈출률 · 평균/최소~최대 시간 · 목표 뒤 머문 시간 · 평균 정기/목표 · 최면 · 레벨 · 폭주 · 빼앗김 표. 치트 기록은 빠짐 | `BalanceReportLogic`, `DebugRecordTab.MakeReport` |
| 272 | 새 기록 json | `Exit`(Escaped / TimeUp / None), `TargetEssence`, `SecondsAfterGoal` 항목이 있음 | `RunStatsRecorder` |

## 6-16. 실패 보상 · 추격 · 위기 화면 효과 · 저장/불러오기 (34일차 추가)

**가장 중요한 줄은 273번, 276번, 278번이다.** 예전 세이브가 1번 칸으로 옮겨졌는지, 이어하기 · 처음부터가 맞게 동작하는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 273 | 33일차 세이브가 있는 채로 처음 재생 | 콘솔 "[SaveSystem] 예전 세이브를 1번 칸으로 옮겼습니다". 저장 폴더에 `projecttheta_slot1.json` · `projecttheta_settings.json`이 생기고 `projecttheta_save.json`은 그대로 남음 | `SaveSystem.MigrateLegacyIfNeeded` |
| 274 | 메인 메뉴 | `이어하기`(보라) · `처음부터` 두 버튼, 아래 줄 설정 · 업적 · 종료. 기록 칸 "최근 1번 칸 · 플레이 N회 …" | `MainMenuScreen.BuildStartButtons` |
| 275 | `이어하기` | "이어하기 · 불러올 칸" 창. 1번 칸에 저장 시각 · 플레이 시간 · 계약 정기 · 클리어 장소 · 업적 수, "최근" 표시. 빈 칸 버튼은 "비어 있음"(누를 수 없음) | `SaveSlotPanel`, `SaveSlotLogic.Describe` |
| 276 | 1번 칸 `불러오기` | 로딩 → 허브. 계약 정기 · 강화 · 업적이 예전과 같음 | `GameSession.LoadSlot` |
| 277 | 허브 아래 줄 `저  장` → (37일차) 저장 칸 창에서 지금 칸 `여기에 저장` | 아래 띠에 금색 "1번 칸에 저장했습니다 · 날짜 시각", 4초 뒤 사라짐 | `HubScreen.HandleSaveSlotChosen` |
| 278 | 타이틀로 → `처음부터` → 2번 칸(빈 칸) `새로 시작` | 허브. 계약 정기 0 · 강화 0 · 튜토리얼 안내가 다시 나옴. 설정 · 키는 그대로 | `GameSession.NewGame`, `SaveSlotLogic.CreateNewGame` |
| 279 | 2번 칸으로 장소 하나 끝낸 뒤 타이틀로 → `이어하기` | 2번 칸이 "최근", 1번 칸 기록은 그대로 | `SaveSlotLogic.FindLatest` |
| 280 | `처음부터` → 기록이 있는 1번 칸 `덮어쓰기` | 칸이 붉어지고 버튼 "한 번 더 누르면 덮어씀". 3초 안에 다시 누르면 새 게임, 기다리면 원래대로 | `SaveSlotLogic.CanStartNewGame` |
| 281 | 저장 칸 창에서 Esc · 닫기 | 창만 닫힘 | `UiEscapeStack` |
| 282 | 저장된 칸이 하나도 없는 상태(저장 폴더 비우기) | `이어하기`가 흐리고 눌리지 않음. 기록 칸 "저장된 칸이 없습니다 · 처음부터 시작하세요" | `MainMenuScreen.Refresh` |
| 283 | 설정에서 음량 · 키를 바꾸고 다른 칸 불러오기 | 바꾼 설정 · 키가 그대로 | `SaveSystem.SaveSettings`(`projecttheta_settings.json`) |
| 284 | 허브 씬을 바로 재생 | 콘솔 "칸을 고르지 않고 들어와 N번 칸을 씁니다". 저장하면 그 칸에 저장됨 | `GameSession.HandleSceneLoaded` |
| 285 | 정기 목표 장소에서 시간 초과 실패 | 결과 계약 정기 줄 "+N  실패 · 보상 50% (−M)" 빨간 글자. 예전의 절반 | `ContractEssenceLogic.Compute` |
| 286 | 목표를 넘긴 뒤(탈출 가능) 쓰러짐 | 초과분은 빠지고 목표까지만 절반으로 계산 | `ContractEssenceLogic.Compute` |
| 287 | 도중 포기 | 여전히 +0, 실패 문구 없음 | `PauseMenuLogic.GetContractEssence` |
| 288 | 정기 목표를 채움 | 플레이어 위 붉은 "추격 시작! 적이 늘어납니다", 흔들림 · 효과음. 곧 적이 늘어 층당 최대 6명(10초마다 1명) | `DisruptorSpawner.UpdateChase`, `PopulationLogic` |
| 289 | F1 튜닝 → 위험 묶음 | "추격 층당 적 최대"(4~8) · "추격 적 증가 간격(초)" 항목, 바꾸면 바로 반영 | `BalanceTuning` |
| 290 | 체력 30% 이하 / 헌팅남이 동행자 게이지 절반 넘김 / 회수 지점 잠김 / 남은 15초 | 화면 가장자리가 붉게 맥동. 급할수록 진함 | `DangerLogic`, `StageVfxDirector.Update` |
| 291 | 설정 → 일반 → 위기 화면 효과 `꺼짐` | 위 상황 · 폭주 모두 붉은 테두리가 나오지 않음. 다시 켜면 나옴 | `SaveData.DangerEffectDisabled` |
| 292 | 로딩 화면 TIP 몇 번 보기 | 추격 · 실패 보상 · 위기 효과 · 저장 버튼 TIP이 섞여 나옴 | `LoadingTips` |

## 6-17. 지도 장소 패널 · 장소 규칙 · 추천 · 첫 소개 화면 (35일차 추가)

**가장 중요한 줄은 293번, 296번, 303번이다.** 장소를 고르면 패널이 들어오는지, 버튼마다 상세 창이 뜨는지, 처음 들어간 장소에서 규칙 카드가 뜨고 게임이 멈추는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 293 | 새 게임(모든 장소 미도전)으로 지도 열기 | 기업 연수원이 골라진 채로 오른쪽 패널이 **바로** 떠 있음. 연수원 칸 위에 금색 "◆ 추천"이 깜빡임 | `MapScreen.Start`, `LocationGuideLogic.GetRecommended` |
| 294 | 장소 칸 모양 | 둘째 줄 "난이도 ◆◇◇◇◇ · 낮"(클럽은 ◆◆◆◆◆). 배지의 숙련 ★과 모양이 다름 | `MapScreen.BuildNode` |
| 295 | 패널 내용 | 위: 시간대 · 목표(왼쪽) · "난이도 ◆…"(오른쪽). 큰 금색 번호 "01" + 이름, 소개, 핵심 수치 3줄(제한 시간 · 목표 정기 / 층 · 경쟁자 / 숙련), 추천이면 "◆ 추천 장소 …" 깜빡임. 버튼 4개, 보유 계약 정기, `닫 기` · `출 발 ▶` | `MapScreen.BuildInfoPanel`, `LocationGuideLogic.BuildCoreRows` |
| 296 | `적 정보` · `장소 규칙` · `보 상` · `기 록`을 차례로 누르기 | 패널 왼쪽에 금색 테두리 상세 창. 누른 버튼만 보라색. 같은 버튼을 다시 누르거나 `×`를 누르면 닫힘 | `MapScreen.ToggleDetail`, `LocationGuideLogic.Toggle` |
| 297 | 해변가 → `적 정보` | 라이프가드 · 헌팅남 · 라이프가드 반장 · 드론 촬영자 + 금태양 · 인기남, 각 "대처" 줄 | `LocationGuideCatalog`, `BuildEnemyText` |
| 298 | 루프탑 클럽 → `적 정보` · 핵심 수치 | 바운서 · 클럽 MD · DJ · 라이벌 서큐버스만. **금태양 · 인기남 없음**, 핵심 수치 "경쟁자 없음" | `LocationGuideCatalog.RivalsAppear` |
| 299 | `장소 규칙` (지하철 · 클럽 · 나머지 하나씩) | 목표 문구가 각각 열차 생존 / 보스 함락 / 정기 + 탈출. 이 장소의 규칙 · 공략 팁 | `BuildRuleText`, `GetObjectiveGuide` |
| 300 | `보 상` | 계약 정기 계산(클리어 · 등급 보너스 · 실패 50% · 포기 0), 숙련 단계와 다음 ★까지, 연수원 · 클럽은 "이 장소의 업적" | `BuildRewardText` |
| 301 | `기 록` (미도전 / 도전한 장소) | 미도전: "아직 도전하지 않은 장소입니다". 도전함: 도전 · 클리어(비율) · 최고 등급 · 최고 정기 · 최단 클리어 · 다음 ★ | `BuildRecordText` |
| 302 | Tab / Esc / 빈 곳 | 장소를 고른 상태에서 Tab은 장소 규칙 열고 닫기, Esc는 상세 창 → 패널 순서로 닫힘. 패널이 닫히면 오른쪽으로 빠지고 지도가 가운데로 옴. 고른 장소가 없으면 Tab이 통계 | `MapScreen.ReadKeyboard`, `AnimatePanel` |
| 303 | 처음 도전하는 장소로 출발 | 로딩 · 시작 카드 선택이 끝난 뒤 "장소 규칙 · 장소 이름" 카드(왼쪽 규칙 · 오른쪽 적). 게임 멈춤. `확인` · Enter · Esc로 닫으면 게임이 흐름 | `LocationGuidePanel`, `LocationGuideLogic.IsFirstVisit` |
| 304 | 같은 장소 두 번째 도전 | 규칙 카드가 자동으로 뜨지 않음 | `PlayStats` 장소 기록 `Attempts` |
| 305 | 장소 안에서 Esc | 일시정지 메뉴가 계속하기 · **장소 규칙** · 조작법 · 설정 · 포기하기 순서. `장소 규칙`을 누르면 카드가 메뉴 위에 뜨고 Esc로 카드만 닫힘 | `PauseMenu.OpenGuide` |
| 306 | 다른 저장 칸으로 새 게임 → 이미 다른 칸에서 가 본 장소 | 새 칸 기준 첫 도전이라 카드가 다시 뜸 | 칸별 `LocationRecords` |
| 307 | 메인 메뉴 → `처음부터` → 칸 고르기 | 허브로 가기 전에 검은 소개 화면 5장. 제목이 아래에서 떠오르고 "1 / 5". 클릭 · Space · Enter로 넘김, 마지막 장 버튼 "시작하기 ▶" → 허브 | `IntroSequence`, `MainMenuScreen.HandleSlotChosen` |
| 308 | 소개 도중 `건너뛰기` · Esc | 곧바로 허브로 감(새 게임은 이미 저장됨) | `IntroSequence.Finish` |
| 309 | `이어하기` | 소개 없이 바로 허브 | `MainMenuScreen.HandleSlotChosen` |
| 310 | 메인 메뉴 아래 줄 | 설정 · 업적 · **소개** · 종료 네 버튼. `소 개`는 소개만 보고 메인 메뉴로 돌아옴 | `MainMenuScreen.BuildMenuRow` |
| 311 | 로딩 TIP | 장소 규칙 · 추천 · 일시정지 규칙 카드 TIP이 섞여 나옴 | `LoadingTips` |

## 6-18. 학생의 방 허브 · 이야기 · 심야 모드 · 도시 지배도 (36일차 추가)

**가장 중요한 줄은 312번, 314번, 320번, 326번이다.** 허브 배경이 방으로 보이는지, 강화 창을 켜고 끌 수 있는지, 입장 이야기 → 규칙 카드 순서가 맞는지, 심야 모드가 적용되는지다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 312 | 허브 들어가기 | 화면 대부분이 **밤의 학생 방**: 창문(밤하늘 · 달 · 도시 실루엣 · 깜빡이는 불빛) · 커튼 · 포스터 · 벽시계(실제 시각) · 책장 · 책상(보라 노트 · 스탠드 불빛) · 의자 · 협탁(일기장) · 침대 · 러그. UI는 위 · 아래 **반투명 띠**뿐 | `HubRoomBackdrop`, `HubScreen.Build` |
| 313 | 위 띠 | 왼쪽 "학생의 방 · N번 칸" · 계약 정기, 가운데 "도시 지배도 N% 「칭호」 · 다음 칭호 N%" + 보라 막대, 오른쪽 `업 적` `조작법` `설 정` | `HubScreen.BuildTopBar`, `DominionLogic.GetSummary` |
| 314 | 아래 `강 화` | 방이 살짝만 어두워지고 가운데 반투명 창 "강화 · 계약 노트"(보유 계약 정기 · 4계열 · 구매). 버튼이 보라색. 다시 누르기 · `×` · 어두운 곳 클릭 · Esc로 닫힘 | `HubWindow`, `HubScreen.SetPanel`, `HubRoomLogic.Toggle` |
| 315 | `통 계` · `난이도` · `일기장` 차례로 | 한 번에 하나만 열림. 통계 창: 요약 3줄 · 장소 8곳 지배도 내역("클리어 ✓ · ★★☆ · ☾ ✗ 3/5") · `업적 보기` · `자세한 통계`(지도와 같은 통계 창). 난이도 창: 3버튼. 일기장: 이야기 20칸, 본 것만 제목 · 나머지 "？？？" | `HubScreen.BuildStatsWindow` · `BuildDifficultyWindow` · `BuildDiaryWindow` |
| 316 | 방 물건에 마우스 올리기 | 노트 · 벽시계 · 책장 · 창문 · 일기장에 보라 빛과 이름표("계약 노트 · 강화" 등)가 서서히 뜸 | `HubHotspot` |
| 317 | 물건 누르기 | 노트 → 강화 창, 벽시계 → 난이도, 책장 → 통계, 일기장 → 일기장, **창문 → 지도로 출격** | `HubScreen.HandleRoomObject`, `HubRoomLogic.GetPanel` |
| 318 | 직전 결과가 있을 때 / 없을 때 | 있으면 왼쪽 아래 작은 반투명 카드(장소 · 클리어/실패 · 점수 · 계약 정기, 심야면 ☾). 없으면 카드 없음 | `HubScreen.RefreshResult` |
| 319 | 아래 `출 격 ▶` (`저 장` · `타이틀로`는 355~358번) | 지도로 감. 저장 문구는 아래 띠 가운데 | `HubScreen.Sortie` |
| 320 | 새 게임으로 연수원 처음 출발 | 로딩 · 시작 카드 뒤 화면 아래 **대사 창**(「첫 출근」 · 이름표 · 한 글자씩). 게임 멈춤. 클릭 · Enter로 넘김(Space는 안 넘어감). 끝나면 **이어서 규칙 카드** | `StageStoryIntro`, `DialogueOverlay`, `LocationGuidePanel` |
| 321 | 대사 중 Esc · `건너뛰기` | 대사가 닫히고 규칙 카드로 넘어감. 같은 장소 두 번째 도전에는 대사 없음 | `DialogueOverlay.SkipAll`, `StoryLogic.GetEnterScene` |
| 322 | 연수원 클리어 → 지도로 | 지도 위에 대사 「첫 계약」. 대사 중 Enter · 숫자 키로 지도가 반응하지 않음 | `StoryPlayback.PlayPending`, `MapScreen.Update` |
| 323 | 서로 다른 장소 3곳 · 6곳 클리어 뒤 지도/허브 | 라이벌 도발 「지켜보는 눈」 · 「초대장」 | `StoryTrigger.LocationsCleared` |
| 324 | 루프탑 클럽 처음 입장 · 엔딩 뒤 | 입장 대사 「라이벌」(라이벌 이름표 분홍). 엔딩 뒤 지도/허브에 「후일담 · 새벽」 | `StoryCatalog` |
| 325 | 지도 패널 `이야기` · 허브 일기장 제목 누르기 | 지도: 상세 창에 그 장소에서 본 대사 전문("본 이야기 N / 2"). 허브: 누른 장면이 대사 창으로 다시 재생 | `LocationDetailKind.Story`, `HubScreen.ReplayStory` |
| 326 | 엔딩을 본 칸에서 지도 → 패널 `☾ 심야 꺼짐` 누르기 | "☾ 심야 켜짐", 패널 테두리 남색, 추천 줄 자리에 "☾ 심야 목표 ×1.3 · 시간 ×0.9 · 추격 7명 · 보상 ×1.5", 핵심 수치의 목표 · 제한 시간이 바뀐 값 | `MapScreen.ToggleNight`, `NightModeLogic` |
| 327 | 엔딩 전 칸 | 버튼 "☾ 심야 (잠김)", 누를 수 없음 | `NightModeLogic.IsUnlocked` |
| 328 | 심야로 출발 | 스테이지 목표 정기 ×1.3 · 제한 시간 ×0.9, 규칙 카드 윗줄에 심야 요약, 목표 달성 뒤 층당 적 최대 7명, 경계도가 더 빨리 오름 | `ProjectThetaPrototypeBootstrap`, `NightModeState`, `DisruptorSpawner.ChaseMax`, `ZoneAlert.Add` |
| 329 | 심야 클리어 결과 | 제목에 "· ☾ 심야", 계약 정기 ×1.5. 지도 장소 칸 배지 끝에 "☾", 통계 표 클리어 칸 "N (☾1)", 업적 "심야 영업" | `StageResultPanel`, `PlayStatsLogic`, `AchievementLogic` |
| 330 | 심야 실패 | "실패 · 보상 50%"도 심야 배율까지 반영, 장소 ☾는 생기지 않음 | `StageResultPanel`, `PlayStatsLogic.Apply` |
| 331 | 치트 없이 심야 몇 판 → F1 기록 → `보고서 만들기` | 표에 "장소 ☾" 줄이 따로 생김 | `BalanceReportLogic.GetGroupKey` |
| 332 | 클리어 · ★ · 심야가 늘어날 때 | 허브 지배도 %가 오르고 창밖 불빛이 그만큼 **보라색**으로 물듦. 25 · 50 · 75 · 100%에서 칭호가 바뀜 | `DominionLogic`, `HubRoomBackdrop.SetDominion` |
| 333 | 지배도 100% | 지도/허브에 「후일담 · 도시의 주인」 | `StoryTrigger.Dominion` |
| 334 | 메인 메뉴 저장 칸 창 | 칸마다 셋째 줄 "도시 지배도 N% 「칭호」" | `SaveSlotLogic.Describe` |
| 335 | 35일차 이전 세이브(클리어한 장소가 있는 칸)로 이어하기 | 이미 깬 장소들의 클리어 장면이 지도/허브 도착 때 이어서 나옴(Esc로 한꺼번에 건너뛰기 가능) | `StoryLogic.GetPending` |
| 336 | 허브에 여러 번 들어가기(지도 ↔ 허브) | 서큐버스(그림이 없으면 보라 실루엣)가 창가 · 책상 옆 · 침대 위 · 책장 앞 · 러그 위 · 침대 앞 중 한 곳에 있고, **바로 전과 다른 자리**. 숨 쉬듯 살짝 오르내림. 앉은 자리는 작게 | `HubSuccubus`, `HubSuccubusLogic.PickSpot` |
| 337 | 서큐버스 누르기 | 머리 오른쪽 위(침대 쪽처럼 오른쪽 끝이면 왼쪽 위) **말풍선**에 대사가 한 글자씩. 꼬리가 머리 쪽을 가리킴. 몇 초 뒤 사라지고, 다시 누르면 다른 대사. 창이 열려 있거나 이야기 중이면 반응 없음 | `HubSuccubus.Talk`, `HubSuccubusLogic.ShouldFlip` |
| 338 | 진행에 따른 대사 | 새 게임: "연수원부터 가 보자" 등, 정기가 모이면 "노트에서 강화해 볼까?", 엔딩 뒤: 심야 모드 권유 | `HubSuccubusLogic.GetLines` |
| 339 | `Resources/Characters/Succubus/`에 `Room_Stand.png` · `Room_Sit.png`(또는 `Idle.png`) 넣기 | 실루엣 대신 그림이 보이고, 발끝이 자리 위치에 맞음 | `HubSuccubus.BuildArt` |

## 6-19. 배경음악 · 효과음 (37일차 추가)

**가장 중요한 줄은 340번, 343번, 345번이다.** 화면마다 음악이 바뀌는지, 추격 · 위기 때 긴장 겹이 붙는지, 설정 음악 음량이 먹는지다. 음원은 모두 `Tools/generate_temp_audio.py`로 만든 **임시 음원**이다.

| # | 할 것 | 보여야 · 들려야 하는 것 | 어긋나면 의심할 곳 |
| --- | --- | --- | --- |
| 340 | 메인 메뉴 → 허브 → 지도 → 장소 | Title → Room → City → 장소 곡으로 **부드럽게 이어짐**(1.2초 크로스페이드). 같은 곡 화면끼리는 끊기지 않음 | `MusicPlayer.UpdateTrack`, `MusicLogic.GetTrack` |
| 341 | 장소 8곳 | 장소마다 다른 곡(연수원 · 해변가 · 지하철 · 헬스장 · 야시장 · 쇼핑몰 · 오피스), 루프탑 클럽은 빠른 Boss 곡 | `MusicLogic.GetLocationTrack` |
| 342 | 보스 함락 | 엔딩 장면 동안 Ending 곡, 결과 뒤 지도에서 City 곡 | `EndingSequence.IsPlaying` |
| 343 | 정기 목표 달성(추격) · 남은 15초 · 체력 30% 이하 | 추격이면 16분 하이햇 · 킥 **긴장 겹**이 60%로, 위기면 100%로 커지고 곡이 살짝 빨라짐. 박자가 곡과 맞음. 위기가 끝나면 서서히 빠짐 | `MusicLogic.GetIntensity`, `MusicPlayer.KeepTensionInStep` |
| 344 | 심야 모드로 장소 | 곡이 살짝 낮고 느림 | `MusicLogic.GetPitch` |
| 345 | 설정 → 음악 음량 0% / 100% | 음악이 꺼짐 / 들림(효과음은 그대로). 음악 줄 아래 문구 "(임시 음원 · 정식 음악으로 교체 예정)" | `GameAudio.MusicVolume`, `SettingsPanel` |
| 346 | 대사 창이 뜰 때 | 음악이 60%로 줄고, 글자가 나올 때 작은 "톡" 소리(말하는 사람마다 높이가 다름, 두 글자마다). 대사가 끝나면 음악이 돌아옴 | `DialogueOverlay.RefreshLine`, `MusicLogic.ShouldBlip` |
| 347 | 허브 서큐버스 누르기 | "뽁" 말풍선 소리 + 글자 소리 | `HubSuccubus.Talk` |
| 348 | 허브 창 · 지도 상세 창 열고 닫기 | 열 때 올라가는 소리, 닫을 때 내려가는 소리 | `GameSfx.WindowOpen` · `WindowClose` |
| 349 | 허브 `저 장` · 지도 `☾ 심야` 켜기 | 저장 두 음 · 낮은 종소리 | `GameSfx.Save` · `NightToggle` |
| 350 | 정기 목표 달성 | 탈출 띠가 뜰 때 밝은 3화음, 추격 시작 알림과 함께 두 음 경보 | `ExitBanner`, `StageVfxDirector.HandleChaseStarted` |
| 351 | 탈출 · 시간 종료 클리어 / 실패 / 포기 | 결과 화면에 팡파르 / 내려가는 음 / 소리 없음 | `StageResultPanel.TakeSnapshot` |
| 352 | 체력이 30% 아래로 · 회수 지점 잠김 | 붉은 테두리가 켜질 때 낮은 경고음(3초에 한 번까지) | `StageVfxDirector.Update` |
| 353 | 업적 달성 알림 | 반짝이는 상승음 | `AchievementToast` |
| 354 | `python Tools/generate_temp_audio.py` 다시 실행 | 파일이 다시 만들어지고 `.meta` GUID는 그대로(Unity 참조 유지) | `generate_temp_assets.write_meta` |
| 355 | 허브 `저 장` | 방 위에 "저장 · 저장할 칸" 창(열리는 소리). 지금 칸에 초록 "지금 칸", 버튼 "여기에 저장". 빈 칸 "이 칸에 저장", 기록 있는 다른 칸 "덮어쓰기" | `SaveSlotPanel`(Save 모드), `SaveSlotLogic.GetSaveButton` |
| 356 | 다른 칸 `덮어쓰기` 한 번 → 3초 안에 한 번 더 | 첫 번째: 칸이 붉어지고 "한 번 더 누르면 덮어씀". 두 번째: 저장음, "N번 칸에 저장했습니다 · 이제 N번 칸으로 플레이". 위 띠 칸 이름이 바뀌고, 이후 장소를 마치면 그 칸에 자동 저장 | `GameSession.SaveToSlot`, `SaveSlotLogic.GetSavedMessage` |
| 357 | 빈 칸에 저장 → 타이틀 → `이어하기` | 저장 칸 목록에 원래 칸과 새 칸이 모두 있고, 새 칸이 "최근" | `SaveSlotLogic.FindLatest` |
| 358 | 허브 `타이틀로` 한 번 / 3초 기다리기 / 두 번 | 한 번: 버튼이 붉게 "한 번 더 누르면 나감", 경고음, 아래 띠에 붉은 "타이틀로 나갈까요? 마지막 저장 … · 한 번 더 누르면 나갑니다". 3초가 지나면 원래대로. 3초 안에 두 번째: 메인 메뉴로 감 | `HubScreen.BackToTitle`, `HubRoomLogic.GetLeaveWarning` |

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
