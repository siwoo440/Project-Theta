# 한 판 통합 확인 목록

큰 기능을 넣은 뒤마다 이 목록을 **위에서부터 끊지 않고** 한 번 돈다.
어긋난 줄이 나오면 번호를 적어두고 멈춘다. 그 줄의 "어긋나면 의심할 곳"부터 본다.

- 준비: `Boot.unity`를 열고 재생
- 소요: 한 바퀴 약 10분 (두 번째 판 포함)
- 기준: 18일차 (`FloorPlanLogic.DefaultFloorCount = 4`, 금태양 2F · 인기남 3F)

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
| 5 | `출 격` | **게임이 멈추고 "시작 계약" 카드 3장**, 테두리 색이 서로 다름 | `RunProgression`, `RunUpgradeDrawLogic` |
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
| 34 | `허브로` | 허브로 돌아감 | `GameSession.GoTo` |

## 6. 허브 → 재출격 (이전 판이 남지 않는가)

**이 구간이 18일차 확인의 핵심이다.** 도메인 리로드가 꺼진 설정에서 이전 판의 값이 남는지를 본다.

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 35 | 허브 확인 | 직전 결과 카드에 방금 판 결과, 계약 정기 증가 | `HubScreen`, `ContractEssenceLogic` |
| 36 | 성장 하나 구매 | 잔액 감소, 핍 증가 | `SaveDataLogic.TryPurchaseUpgrade` |
| 37 | 다시 `출 격` | **"시작 계약" 카드가 다시 나옴**, `Lv 1`부터 시작 | `RunProgression` |
| 38 | 카드 선택 화면의 별 | 지난 판에 고른 카드도 **`NEW`로 표시** (스택이 남지 않음) | `RunUpgradeMultipliers` 초기화 |
| 39 | 1층 시작 | 층 표시가 **1F**, 경쟁자 없음 | `FloorVisibility`, `_metOpponents` |
| 40 | 튜토리얼 | 지난 판에 끝까지 마쳤다면 **안내가 안 뜸** | `SaveData.TutorialCompleted` |
| 41 | 게임이 멈춰 있지 않음 | 카드를 고른 뒤 정상 진행 | `GameplayPause` 초기화 |

## 7. 에디터 재생 종료 → 다시 재생

| # | 할 것 | 보여야 하는 것 | 어긋나면 의심할 곳 |
| ---: | --- | --- | --- |
| 42 | **카드 화면이 떠 있는 상태로** 재생 종료 | - | - |
| 43 | 다시 재생 → 출격 | 게임이 **멈춘 채로 시작하지 않음** | `GameplayPause.ResetOnPlayModeEnter` |
| 44 | 효과음 | 버튼 틱·도장·구매 소리가 남 | `GameAudio.ResetOnPlayModeEnter` |
| 45 | 한글 | 여전히 한글로 보임 | `UiFontProvider.ResetOnPlayModeEnter` |
| 46 | Test Runner → Run All | 전부 통과 | - |

---

## 기록

| 날짜 | 일차 | 통과 | 어긋난 번호 | 메모 |
| --- | --- | --- | --- | --- |
| | 18 | | | |
