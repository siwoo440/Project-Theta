using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Capture;
using ProjectTheta.Duel;
using ProjectTheta.Companion;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Items;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Rival;
using ProjectTheta.Run;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI;
using ProjectTheta.UI.DebugTools;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
        /// <summary>지금 구역 번호다. 0이 1구역이다. NPC 등급 구성에 더한다.</summary>
        private int _zoneStep;

        /// <summary>장소별 NPC 밀도다.</summary>
        private float _npcDensity = 1f;

        /// <summary>
        /// 씬 로드마다 <see cref="SceneBootstrapRouter"/>가 호출한다.
        /// 이미 구성되어 있으면 아무것도 하지 않는다.
        /// </summary>
        public static void CreateForScene()
        {
            string sceneName =
                SceneManager.GetActiveScene().name;

            if (!SceneFlowLogic.IsStageScene(
                    sceneName))
            {
                return;
            }

            // 허브에서 재출격하면 씬이 새로 로드되므로 이전 오브젝트는 이미 사라져 있다.
            // 그래도 중복 생성만은 확실히 막는다.
            if (FindFirstObjectByType<
                    ProjectThetaPrototypeBootstrap>() != null ||
                FindFirstObjectByType<
                    PlayerSideViewController>() != null)
            {
                return;
            }

            GameObject bootstrapObject =
                new GameObject(
                    "ProjectThetaPrototypeBootstrap");

            bootstrapObject.AddComponent<
                ProjectThetaPrototypeBootstrap>();
        }

        private void Start()
        {
            // 치트와 게임 속도는 판마다 꺼진 채로 시작한다 (20일차).
            DebugCheats.ResetForRun();

            GameplayPause.SetDebugSpeed(
                1f);

            // 21일차: 지도에서 고른 장소를 짓는다. 스테이지 씬을 바로 재생하면 연수원으로 시작한다.
            RunSession session =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.EnsureRunForStage();

            LocationDefinition location =
                LocationCatalog.Get(
                    session != null
                        ? session.Location
                        : LocationCatalog.StartLocation);

            LocationContext.Set(
                location);

            // 어려운 장소일수록 같은 층이라도 고급 NPC가 더 섞인다 (29일차: 구역 번호 대신 장소 단계).
            _zoneStep =
                RunRouteLogic.GetTier(
                    location.Id);

            _npcDensity =
                location.NpcDensity;

            int floorCount =
                Mathf.Max(
                    1,
                    location.FloorCount);

            // 27일차: 장소마다 배경 · 바닥 · 소품이 다른 맵을 짓는다.
            SchoolHallwayPrototypeBuilder.Build(
                floorCount,
                location.Id);

            // 28일차: 규칙 소품(노점 · 게이트 · 바 …) 색을 이 장소 맵 톤에 맞춘다.
            LocationProps.Theme =
                Map.MapThemeCatalog.Get(
                    location.Id);

            PlayerSideViewController player =
                CreatePlayer();

            StageSessionController stage =
                player.GetComponent<
                    StageSessionController>();

            // 30일차: 숙련도(★)마다 목표 정기가 조금씩 오른다.
            stage.ApplyObjective(
                location.TimeLimitSeconds,
                Save.MasteryLogic.GetTargetEssence(
                    location.TargetEssence,
                    session == null
                        ? 0
                        : session.Stars));

            FollowerManager followers =
                player.GetComponent<
                    FollowerManager>();

            PlayerHealth health =
                player.GetComponent<
                    PlayerHealth>();

            PlayerCaptureController capture =
                player.GetComponent<
                    PlayerCaptureController>();

            OpponentDuelController duel =
                player.GetComponent<
                    OpponentDuelController>();

            CameraFollow2D cameraFollow =
                CreateCamera(
                    player.transform);

            // 카메라 배경색을 시간대로 덮어쓰므로 카메라를 만든 뒤에 둔다.
            TimeOfDayOverlay.Create(
                location.TimeOfDay);

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                // 24일차: 헬스장은 층마다 "대회 앞둔 선수", 25일차: 오피스 최상층에 "대표 비서"를 둔다.
                List<GameObject> npcs =
                    CreateNpcs(
                        player,
                        floor,
                        GetSpecialTargetX(
                            location.Id,
                            floor,
                            floorCount));

                // 25일차: 쇼핑몰 보안실 직원, 오피스 직원 · 방문객 · 출입증 신분을 붙인다.
                AssignNpcRoles(
                    location.Id,
                    floor,
                    floorCount,
                    npcs);

                CreateRecoveryPoint(
                    stage,
                    followers,
                    floor);
            }

            FloorTransitionController floorTransition =
                CreateFloorTransition(
                    player,
                    followers,
                    cameraFollow,
                    floorCount);

            // 26일차: 루프탑 클럽은 라이벌 서큐버스와 싸우므로 기존 경쟁자를 내보내지 않는다.
            if (location.HasRivals &&
                location.Id != LocationId.RooftopClub)
            {
                CreateGeumtaeyang(
                    stage,
                    followers,
                    player,
                    floorCount);

                CreatePopularGuy(
                    stage,
                    followers,
                    player,
                    floorCount);
            }

            // 22일차: 장소의 방해 세력과 구역 경계도, 연수원 환경 규칙.
            DisruptorSpawner.Create(
                location,
                floorCount,
                stage,
                player.transform,
                followers);

            // 25일차: 오피스 여부도 정적 값이라 구역마다 다시 정한다.
            OfficeLayout.Active =
                location.Id == LocationId.OfficeTower;

            // 23일차: 어둠은 야시장에서만 켠다. 정적 값이라 구역이 바뀔 때마다 다시 정한다.
            LanternLight.SetDarkness(
                location.Id == LocationId.NightMarket);

            switch (location.Id)
            {
                case LocationId.TrainingCenter:
                    CreateTrainingCenterRules(
                        stage,
                        player,
                        floorCount);
                    break;

                case LocationId.Beach:
                    CreateBeachRules(
                        stage,
                        player,
                        followers);
                    break;

                case LocationId.NightMarket:
                    CreateNightMarketRules(
                        followers);
                    break;

                case LocationId.SubwayStation:
                    CreateSubwayRules(
                        stage,
                        floorCount);
                    break;

                case LocationId.FitnessCenter:
                    CreateFitnessCenterRules(
                        floorCount);
                    break;

                case LocationId.ShoppingMall:
                    CreateMallRules(
                        stage,
                        player,
                        followers,
                        floorCount);
                    break;

                case LocationId.OfficeTower:
                    CreateOfficeRules(
                        stage,
                        player,
                        followers,
                        floorCount);
                    break;

                case LocationId.RooftopClub:
                    CreateClubRules(
                        stage,
                        player,
                        followers,
                        floorCount);
                    break;
            }

            CreateCursorController();

            CreateCaptureHud(
                capture);

            CreateDuelHud(
                duel);

            StageScoreTracker scoreTracker =
                player.GetComponent<
                    StageScoreTracker>();

            scoreTracker.Configure(
                stage,
                followers);

            RunProgression runProgression =
                CreateRunProgression(
                    player,
                    stage,
                    scoreTracker,
                    floorTransition,
                    session);

            CreateVfxDirector(
                player,
                runProgression,
                floorTransition);

            CreateStageResultPanel(
                stage,
                scoreTracker);

            CreateStageTelemetry(
                stage,
                followers,
                health);

            CreateStageEndController(
                stage,
                player,
                player.GetComponent<HypnosisCaster>(),
                capture);

            CreateHud(
                player.GetComponent<HypnosisCaster>(),
                stage,
                health);

            RunStatsRecorder recorder =
                CreateRunStatsRecorder(
                    player,
                    stage,
                    floorTransition,
                    runProgression);

            // 33일차: 목표를 채우면 1F 회수 지점이 탈출구가 된다(정기 목표 장소).
            if (StageExitLogic.UsesExit(location.Objective))
            {
                new GameObject("ExitGate")
                    .AddComponent<ExitGate>()
                    .Configure(
                        stage,
                        floorTransition,
                        player.transform,
                        FloorSpace.ToWorld(
                            StageExitLogic.ExitFloor,
                            new Vector2(
                                FloorLayout.RecoveryX,
                                FloorLayout.RecoveryY)),
                        new Vector2(
                            FloorLayout.RecoveryWidth,
                            5.0f));

                new GameObject("ExitBanner")
                    .AddComponent<UI.ExitBanner>()
                    .Configure(
                        stage,
                        scoreTracker,
                        floorTransition);
            }

            // 35일차: 장소 규칙 카드. 이 칸에서 처음 도전하는 장소면 시작할 때 한 번 뜬다.
            //         일시정지 메뉴가 찾아 쓰므로 먼저 만든다.
            new GameObject("LocationGuide")
                .AddComponent<LocationGuidePanel>()
                .Configure(
                    stage,
                    LocationGuideLogic.IsFirstVisit(
                        GameSession.Instance == null
                            ? null
                            : GameSession.Instance.Save,
                        location.Id));

            // 31일차: Esc 일시정지 메뉴(계속 · 조작법 · 설정 · 포기).
            new GameObject("PauseMenu")
                .AddComponent<UI.PauseMenu>()
                .Configure(stage);

            CreateDebugPanel(
                new DebugPanelContext
                {
                    Caster = player.GetComponent<HypnosisCaster>(),
                    Stage = stage,
                    Health = health,
                    Focus = player.GetComponent<PlayerFocus>(),
                    Followers = followers,
                    Run = runProgression,
                    Floors = floorTransition,
                    Rampage = player.GetComponent<RampageCoordinator>(),
                    Capture = capture,
                    Recorder = recorder
                });
        }

        /// <summary>
        /// 기업 연수원 환경 규칙을 붙인다 (22일차).
        ///   쉬는 시간 종  40초마다 8초 동안 복도 NPC가 빨라진다
        ///   자습실        층마다 한 칸, 안에서 대시하면 경계도가 오른다
        /// 1F는 튜토리얼 층이라 자습실을 두지 않는다.
        /// </summary>
        private void CreateTrainingCenterRules(
            StageSessionController stage,
            PlayerSideViewController player,
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "TrainingCenterRules");

            rules.AddComponent<BreakTimeBell>().Configure(
                stage);

            for (int floor = 1;
                 floor < floorCount;
                 floor++)
            {
                GameObject room =
                    new GameObject(
                        $"QuietRoom_{floor + 1}F");

                room.transform.SetParent(
                    rules.transform,
                    false);

                room.AddComponent<QuietRoomZone>().Configure(
                    player,
                    floor,
                    floor % 2 == 0
                        ? -7f
                        : 7f);
            }
        }

        /// <summary>
        /// 해변가 환경 규칙을 붙인다 (23일차).
        ///   밀물 · 썰물  30초마다 바닷가 쪽 줄이 8초 막힌다
        ///   파라솔 그늘  안에 있으면 라이프가드에게 보이지 않는다
        ///   망루 · 샤워장 표지 (샤워장 = 회수 지점)
        /// </summary>
        private void CreateBeachRules(
            StageSessionController stage,
            PlayerSideViewController player,
            FollowerManager followers)
        {
            GameObject rules =
                new GameObject(
                    "BeachRules");

            rules.AddComponent<TideCycle>().Configure(
                stage,
                player,
                followers);

            Color[] canopies =
            {
                new Color(1.00f, 0.45f, 0.45f, 0.80f),
                new Color(0.40f, 0.75f, 1.00f, 0.80f),
                new Color(1.00f, 0.85f, 0.35f, 0.80f),
                new Color(0.55f, 0.90f, 0.60f, 0.80f)
            };

            float[] parasols =
                DisruptorCatalog.BeachParasolX;

            for (int i = 0;
                 i < parasols.Length;
                 i++)
            {
                GameObject parasol =
                    new GameObject(
                        $"Parasol_{i + 1}");

                parasol.transform.SetParent(
                    rules.transform,
                    false);

                // 그늘은 밀물에 잠기지 않는 높이에 둔다. 번갈아 앞뒤로 엇갈린다.
                parasol.AddComponent<ParasolShade>().Configure(
                    0,
                    parasols[i],
                    i % 2 == 0 ? -2.4f : -0.9f,
                    canopies[i % canopies.Length]);
            }

            LocationProps.LifeguardTower(
                rules.transform,
                0,
                DisruptorCatalog.BeachTowerX,
                -1.2f);

            LocationProps.Sign(
                rules.transform,
                0,
                new Vector2(FloorLayout.RecoveryX, FloorSpace.WalkMaxY + 1.2f),
                "샤워장",
                new Color(0.55f, 0.85f, 1.00f));
        }

        /// <summary>
        /// 야시장 환경 규칙을 붙인다 (23일차).
        ///   어둠과 등불  노점 등불 아래에서만 최면 사거리가 온전하다
        ///   좁은 골목    동행자가 한 줄로 따라온다
        /// </summary>
        private void CreateNightMarketRules(
            FollowerManager followers)
        {
            GameObject rules =
                new GameObject(
                    "NightMarketRules");

            followers.SingleFile = true;

            string[] titles =
            {
                "꼬치", "떡볶이", "어묵", "호떡"
            };

            Color[] awnings =
            {
                new Color(0.95f, 0.35f, 0.30f),
                new Color(1.00f, 0.60f, 0.25f),
                new Color(0.95f, 0.80f, 0.30f),
                new Color(0.85f, 0.40f, 0.70f)
            };

            float[] stalls =
                DisruptorCatalog.NightMarketStallX;

            for (int i = 0;
                 i < stalls.Length;
                 i++)
            {
                LocationProps.Stall(
                    rules.transform,
                    0,
                    stalls[i],
                    awnings[i % awnings.Length],
                    titles[i % titles.Length]);

                LanternLight.Create(
                    rules.transform,
                    $"Lantern_{i + 1}",
                    FloorSpace.ToWorld(
                        0,
                        new Vector2(stalls[i], -1.8f)),
                    2.2f,
                    new Color(1.00f, 0.70f, 0.35f, 0.26f),
                    true);
            }
        }

        /// <summary>
        /// 지하철 환승역 환경 규칙을 붙인다 (24일차).
        ///   열차 도착  45초마다 행인이 쏟아지고, 도착한 열차 수가 생존 목표가 된다
        ///   개찰구     층마다 가운데. 역무원이 동행자 5명 이상을 한 명씩 통과시킨다
        /// </summary>
        private void CreateSubwayRules(
            StageSessionController stage,
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "SubwayRules");

            rules.AddComponent<TrainArrival>().Configure(
                stage,
                floorCount);

            float gateX =
                DisruptorCatalog.SubwayGateX;

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                // 개찰구 기둥을 세로로 늘어놓는다. 기둥 사이로 지나간다는 느낌만 준다.
                for (float y = FloorSpace.WalkMinY + 0.4f;
                     y < FloorSpace.WalkMaxY;
                     y += 1.2f)
                {
                    LocationProps.Structure(
                        rules.transform,
                        "GatePost",
                        FloorSpace.ToWorld(floor, new Vector2(gateX, y)),
                        new Vector2(0.35f, 0.3f),
                        new Color(0.55f, 0.60f, 0.65f),
                        -44);
                }

                LocationProps.Sign(
                    rules.transform,
                    floor,
                    new Vector2(gateX, FloorSpace.WalkMaxY + 1.2f),
                    "개찰구 · 5명 이상은 한 명씩",
                    new Color(0.75f, 0.85f, 0.95f),
                    20);
            }
        }

        /// <summary>
        /// 헬스장 환경 규칙을 붙인다 (24일차).
        ///   심박 구역  운동 구역은 최면 · 충동이 빠르고, 요가실은 충동이 오르지 않는다
        ///   수영장     수영장 코치의 "전원 입수" 범위
        /// </summary>
        private void CreateFitnessCenterRules(
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "FitnessCenterRules");

            GymZoneSpec[] zones =
                GymLayout.Zones;

            for (int i = 0;
                 i < zones.Length;
                 i++)
            {
                if (zones[i].Floor >= floorCount)
                {
                    continue;
                }

                GameObject zone =
                    new GameObject(
                        $"GymZone_{zones[i].Kind}_{zones[i].Floor + 1}F");

                zone.transform.SetParent(
                    rules.transform,
                    false);

                zone.AddComponent<GymZone>().Configure(
                    zones[i]);
            }
        }

        private static float? GetSpecialTargetX(
            LocationId location,
            int floor,
            int floorCount)
        {
            switch (location)
            {
                case LocationId.FitnessCenter:
                    return GymLayout.GetAthleteX(floor);

                case LocationId.OfficeTower:
                    return floor == floorCount - 1
                        ? OfficeLayout.ExecutiveX
                        : (float?)null;

                default:
                    return null;
            }
        }

        /// <summary>장소 NPC에게 신분을 붙인다 (25일차).</summary>
        private static void AssignNpcRoles(
            LocationId location,
            int floor,
            int floorCount,
            List<GameObject> npcs)
        {
            if (npcs == null ||
                npcs.Count == 0)
            {
                return;
            }

            if (location == LocationId.ShoppingMall)
            {
                GameObject staff =
                    FindClosest(
                        npcs,
                        MallLayout.SecurityRoomX,
                        null);

                staff.AddComponent<NpcRoleMark>().Configure(NpcRole.SecurityRoom);
                staff.AddComponent<SecurityRoomLink>();

                return;
            }

            if (location == LocationId.RooftopClub)
            {
                // 1F에 VIP 게스트 한 명. 데리고 있으면 바운서가 막지 않는다.
                if (floor == 0)
                {
                    FindClosest(npcs, ClubLayout.VipGuestX, null)
                        .AddComponent<NpcRoleMark>()
                        .Configure(NpcRole.VipGuest);
                }

                // 27일차: 2F 바텐더. 최면하면 샴페인 타워가 무너진다.
                if (floor == floorCount - 1)
                {
                    FindClosest(npcs, ClubLayout.BarX, null)
                        .AddComponent<NpcRoleMark>()
                        .Configure(NpcRole.Bartender);
                }

                return;
            }

            if (location != LocationId.OfficeTower)
            {
                return;
            }

            bool top = floor == floorCount - 1;

            GameObject passHolder =
                top
                    ? null
                    : FindClosest(
                        npcs,
                        OfficeLayout.PassHolderX,
                        null);

            for (int i = 0;
                 i < npcs.Count;
                 i++)
            {
                GameObject npc = npcs[i];

                AthleteMark executive = npc.GetComponent<AthleteMark>();

                if (executive != null)
                {
                    executive.Configure(
                        "★ 대표 비서",
                        MallOfficeValues.ExecutiveSecretaryBonus);
                }

                // 세 명 중 한 명은 외부 방문객이다. 긴급 회의에 끌려가지 않는다.
                NpcRole role =
                    npc == passHolder
                        ? NpcRole.PassHolder
                        : executive == null && i % 3 == 2
                            ? NpcRole.Visitor
                            : NpcRole.Employee;

                npc.AddComponent<NpcRoleMark>().Configure(role);
            }
        }

        private static GameObject FindClosest(
            List<GameObject> npcs,
            float x,
            GameObject exclude)
        {
            GameObject best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0;
                 i < npcs.Count;
                 i++)
            {
                if (npcs[i] == null ||
                    npcs[i] == exclude ||
                    npcs[i].GetComponent<AthleteMark>() != null)
                {
                    continue;
                }

                float distance = Mathf.Abs(npcs[i].transform.position.x - x);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = npcs[i];
                }
            }

            return best ?? npcs[0];
        }

        /// <summary>
        /// 쇼핑몰 환경 규칙을 붙인다 (25일차).
        ///   셔터     층마다 두 곳. 보안팀장이 봉쇄한다
        ///   기둥     층마다 두 곳. CCTV · 보안요원 시야를 가린다
        ///   폐점     제한 시간 70%에 방송, 이후 시간이 빨리 흐른다
        /// </summary>
        private void CreateMallRules(
            StageSessionController stage,
            PlayerSideViewController player,
            FollowerManager followers,
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "MallRules");

            rules.AddComponent<MallClosing>().Configure(
                stage);

            Rigidbody2D playerBody =
                player.GetComponent<Rigidbody2D>();

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                for (int i = 0;
                     i < MallLayout.ShutterX.Length;
                     i++)
                {
                    GameObject shutter =
                        new GameObject(
                            $"Shutter_{floor + 1}F_{i + 1}");

                    shutter.transform.SetParent(
                        rules.transform,
                        false);

                    shutter.AddComponent<ShutterGate>().Configure(
                        floor,
                        MallLayout.ShutterX[i],
                        playerBody,
                        followers);
                }

                for (int i = 0;
                     i < MallLayout.PillarX.Length;
                     i++)
                {
                    GameObject pillar =
                        new GameObject(
                            $"Pillar_{floor + 1}F_{i + 1}");

                    pillar.transform.SetParent(
                        rules.transform,
                        false);

                    pillar.AddComponent<ParasolShade>().ConfigurePillar(
                        floor,
                        MallLayout.PillarX[i],
                        i % 2 == 0 ? -2.2f : -0.8f);
                }

                LocationProps.Sign(
                    rules.transform,
                    floor,
                    new Vector2(MallLayout.SecurityRoomX, FloorSpace.WalkMaxY + 1.2f),
                    "보안실",
                    new Color(0.65f, 0.80f, 1.00f));
            }
        }

        /// <summary>
        /// 오피스 타워 환경 규칙을 붙인다 (25일차).
        ///   출입증 게이트  맨 위층을 뺀 층마다 위층 계단을 잠근다
        ///   정전          제한 시간 35% · 70%에 8초
        ///   탕비실 · 회의실 표지
        /// </summary>
        private void CreateOfficeRules(
            StageSessionController stage,
            PlayerSideViewController player,
            FollowerManager followers,
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "OfficeRules");

            rules.AddComponent<Blackout>().Configure(
                stage,
                floorCount);

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                if (floor < floorCount - 1)
                {
                    GameObject gate =
                        new GameObject(
                            $"PassGate_{floor + 1}F");

                    gate.transform.SetParent(
                        rules.transform,
                        false);

                    gate.AddComponent<PassGate>().Configure(
                        floor,
                        player.transform,
                        followers);
                }

                float pantryX = OfficeLayout.GetPantryX(floor);
                float meetingX = OfficeLayout.GetMeetingRoomX(floor);

                LocationProps.Structure(
                    rules.transform,
                    "Pantry",
                    FloorSpace.ToWorld(floor, new Vector2(pantryX, FloorSpace.WalkMaxY + 0.35f)),
                    new Vector2(2.4f, 0.8f),
                    new Color(0.55f, 0.45f, 0.35f),
                    -45);

                LocationProps.Sign(
                    rules.transform,
                    floor,
                    new Vector2(pantryX, FloorSpace.WalkMaxY + 1.2f),
                    "탕비실",
                    new Color(0.85f, 0.70f, 0.50f));

                LocationProps.Box(
                    rules.transform,
                    "MeetingRoom",
                    FloorSpace.ToWorld(floor, new Vector2(meetingX, (FloorSpace.WalkMinY + FloorSpace.WalkMaxY) * 0.5f)),
                    new Vector2(Disruptors.MeetingLogic.RoomHalfWidth * 2f, FloorSpace.WalkMaxY - FloorSpace.WalkMinY),
                    new Color(0.45f, 0.55f, 0.85f, 0.12f),
                    -57);

                LocationProps.Sign(
                    rules.transform,
                    floor,
                    new Vector2(meetingX, FloorSpace.WalkMaxY + 1.2f),
                    "회의실",
                    new Color(0.70f, 0.80f, 1.00f));
            }
        }

        /// <summary>
        /// 루프탑 클럽 규칙과 보스전을 붙인다 (26일차).
        ///   음악 박자   드롭 · 조명 점멸 · 템포
        ///   VIP 입구    1F 위층 계단을 바운서가 지킨다
        ///   보스전      2F 라이벌 서큐버스, 플레이어 정신력, 보스 HUD
        /// </summary>
        private void CreateClubRules(
            StageSessionController stage,
            PlayerSideViewController player,
            FollowerManager followers,
            int floorCount)
        {
            GameObject rules =
                new GameObject(
                    "ClubRules");

            rules.AddComponent<ClubBeat>().Configure(
                stage,
                followers,
                floorCount);

            rules.AddComponent<VipEntrance>().Configure(
                followers);

            // 29일차: 보스를 함락하면 결과 화면 전에 엔딩 장면을 보여 준다.
            rules.AddComponent<Boss.EndingSequence>();

            int top = Mathf.Max(0, floorCount - 1);

            LocationProps.Structure(
                rules.transform,
                "EntranceBar",
                FloorSpace.ToWorld(0, new Vector2(ClubLayout.EntranceBarX, FloorSpace.WalkMaxY + 0.35f)),
                new Vector2(3f, 0.8f),
                new Color(0.30f, 0.15f, 0.35f),
                -45);

            LocationProps.Sign(
                rules.transform,
                0,
                new Vector2(ClubLayout.EntranceBarX, FloorSpace.WalkMaxY + 1.2f),
                "BAR",
                new Color(1.00f, 0.60f, 0.90f));

            LocationProps.Structure(
                rules.transform,
                "DjBooth",
                FloorSpace.ToWorld(top, new Vector2(ClubLayout.DjBoothX, FloorSpace.WalkMaxY + 0.4f)),
                new Vector2(2.4f, 1f),
                new Color(0.15f, 0.20f, 0.30f),
                -45);

            LocationProps.Sign(
                rules.transform,
                top,
                new Vector2(ClubLayout.DjBoothX, FloorSpace.WalkMaxY + 1.3f),
                "DJ 부스",
                new Color(0.55f, 0.95f, 1.00f));

            LocationProps.Structure(
                rules.transform,
                "TopBar",
                FloorSpace.ToWorld(top, new Vector2(ClubLayout.BarX, FloorSpace.WalkMaxY + 0.35f)),
                new Vector2(3f, 0.8f),
                new Color(0.30f, 0.15f, 0.35f),
                -45);

            LocationProps.Sign(
                rules.transform,
                top,
                new Vector2((ClubLayout.DanceFloorMinX + ClubLayout.DanceFloorMaxX) * 0.5f, FloorSpace.WalkMaxY + 1.3f),
                "댄스플로어 · 라이벌 서큐버스의 영역",
                new Color(0.85f, 0.60f, 1.00f));

            Boss.PlayerMind mind =
                player.gameObject.AddComponent<Boss.PlayerMind>();

            Boss.BossBattle.Create(
                stage,
                player.transform,
                followers,
                top);

            BossHudView.Create(
                mind);
        }

        /// <summary>한 판 기록을 붙인다 (20일차). 층 이동·레벨 알림을 구독하므로 둘 다 만들어진 뒤에 부른다.</summary>
        private RunStatsRecorder CreateRunStatsRecorder(
            PlayerSideViewController player,
            StageSessionController stage,
            FloorTransitionController floorTransition,
            RunProgression runProgression)
        {
            GameObject recorderObject =
                new GameObject(
                    "RunStatsRecorder");

            RunStatsRecorder recorder =
                recorderObject.AddComponent<
                    RunStatsRecorder>();

            recorder.Configure(
                stage,
                floorTransition,
                player.GetComponent<PlayerFocus>(),
                runProgression);

            return recorder;
        }

        /// <summary>
        /// F1 디버그 패널을 붙인다 (20일차).
        /// 에디터와 개발 빌드에서만 만든다. 정식 빌드에는 치트가 들어가지 않는다.
        /// </summary>
        private void CreateDebugPanel(
            DebugPanelContext context)
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }

            GameObject panelObject =
                new GameObject(
                    "DebugPanel");

            DebugPanel panel =
                panelObject.AddComponent<
                    DebugPanel>();

            panel.Configure(
                context);
        }

        private PlayerSideViewController CreatePlayer()
        {
            GameObject player =
                new GameObject("Player");

            // 1층 계단실 근처에서 시작한다.
            player.transform.position =
                new Vector3(
                    FloorLayout.PlayerStartX,
                    FloorLayout.PlayerStartY,
                    0f);

            player.AddComponent<SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                player.AddComponent<
                    RuntimeCharacterSpriteAnimator>();

            animator.Configure(
                "Characters/Player",
                9f,
                390f);

            Rigidbody2D body =
                player.AddComponent<Rigidbody2D>();

            body.gravityScale = 0f;
            body.constraints =
                RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;
            body.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D playerCollider =
                player.AddComponent<BoxCollider2D>();

            playerCollider.size =
                new Vector2(
                    0.52f,
                    0.34f);

            playerCollider.offset =
                new Vector2(
                    0f,
                    0.17f);

            player.AddComponent<DepthSortByY>();

            PlayerSideViewController controller =
                player.AddComponent<
                    PlayerSideViewController>();

            player.AddComponent<FollowerManager>();
            player.AddComponent<RampageCoordinator>();
            player.AddComponent<PlayerFocus>();
            player.AddComponent<HypnosisCaster>();

            PlayerHealth health =
                player.AddComponent<PlayerHealth>();

            player.AddComponent<
                PlayerUpgrades>();

            player.AddComponent<
                StageScoreTracker>();

            StageSessionController stage =
                player.AddComponent<
                    StageSessionController>();

            stage.Configure(
                health,
                player.GetComponent<FollowerManager>());

            player.AddComponent<
                PlayerCaptureController>();

            player.AddComponent<
                OpponentDuelController>();

            player.AddComponent<
                HypnosisWaveCaster>();

            player.AddComponent<
                PlayerConsumables>();

            return controller;
        }

        private CameraFollow2D CreateCamera(
            Transform target)
        {
            Camera camera =
                Camera.main;

            if (camera == null)
            {
                GameObject cameraObject =
                    new GameObject(
                        "Main Camera");

                camera =
                    cameraObject.AddComponent<Camera>();

                cameraObject.tag =
                    "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 4.65f;
            camera.backgroundColor =
                new Color(
                    0.16f,
                    0.20f,
                    0.21f);

            camera.transform.position =
                new Vector3(
                    -10.5f,
                    1.2f,
                    -10f);

            CameraFollow2D follow =
                camera.GetComponent<CameraFollow2D>() ??
                camera.gameObject.AddComponent<
                    CameraFollow2D>();

            // 경계는 층 기준(1층 좌표)으로 준다.
            // 실제 카메라 위치는 플레이어가 선 층만큼 위로 올라간다.
            follow.Configure(
                target,
                new Vector2(
                    -10.5f,
                    -1.25f),
                new Vector2(
                    10.5f,
                    1.65f));

            follow.SnapToTarget();

            return follow;
        }

        /// <summary>
        /// 한 층의 NPC를 배치한다.
        /// 층마다 인원과 등급 구성이 다르고, 위층일수록 고급 NPC가 섞인다.
        /// </summary>
        private List<GameObject> CreateNpcs(
            PlayerSideViewController player,
            int floorIndex,
            float? athleteX = null)
        {
            List<GameObject> created =
                new List<GameObject>();

            Collider2D playerCollider =
                player.GetComponent<Collider2D>();

            int count =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        FloorPlanLogic.GetNpcCount(
                            floorIndex) *
                        _npcDensity));

            NpcGrade[] grades =
                FloorPlanLogic.BuildGrades(
                    floorIndex + _zoneStep,
                    count);

            int athleteIndex =
                athleteX == null
                    ? -1
                    : FindClosestSpawnIndex(
                        athleteX.Value,
                        count);

            for (int i = 0;
                 i < count;
                 i++)
            {
                GameObject npc =
                    new GameObject(
                        $"FemaleNPC_{floorIndex + 1}F_{i + 1:00}");

                npc.transform.position =
                    FloorSpace.ToWorld(
                        floorIndex,
                        GetNpcSpawnPosition(
                            i,
                            count));

                npc.AddComponent<SpriteRenderer>();

                RuntimeCharacterSpriteAnimator animator =
                    npc.AddComponent<
                        RuntimeCharacterSpriteAnimator>();

                animator.Configure(
                    "Characters/NPC_Female",
                    7f,
                    390f);

                Rigidbody2D body =
                    npc.AddComponent<Rigidbody2D>();

                body.gravityScale = 0f;
                body.constraints =
                    RigidbodyConstraints2D.FreezeRotation;
                body.collisionDetectionMode =
                    CollisionDetectionMode2D.Continuous;
                body.interpolation =
                    RigidbodyInterpolation2D.Interpolate;
                body.mass = 0.65f;

                BoxCollider2D collider =
                    npc.AddComponent<BoxCollider2D>();

                collider.size =
                    new Vector2(
                        0.48f,
                        0.32f);

                collider.offset =
                    new Vector2(
                        0f,
                        0.16f);

                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        collider,
                        playerCollider,
                        true);
                }

                npc.AddComponent<NpcSoftSeparation>();

                NpcAgent agent =
                    npc.AddComponent<NpcAgent>();

                NpcProfile profile =
                    npc.AddComponent<NpcProfile>();

                // 특수 대상 선수는 희귀 등급으로 고정한다.
                profile.Configure(
                    i == athleteIndex
                        ? NpcGrade.Rare
                        : grades[i % grades.Length]);

                if (i == athleteIndex)
                {
                    npc.AddComponent<AthleteMark>();
                }

                npc.AddComponent<HypnosisTarget>();
                npc.AddComponent<FollowerController>();
                npc.AddComponent<ImpulseMeter>();
                npc.AddComponent<NpcHypnosisStatusView>();
                npc.AddComponent<DepthSortByY>();

                agent.Configure(
                    player.transform,
                    animator);

                created.Add(npc);
            }

            return created;
        }

        private static int FindClosestSpawnIndex(
            float x,
            int count)
        {
            int best = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0;
                 i < count;
                 i++)
            {
                float distance =
                    Mathf.Abs(
                        GetNpcSpawnPosition(i, count).x - x);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// 복도를 가로로 고르게 나눠 NPC를 세운다.
        /// 난수를 쓰지 않아 같은 층은 항상 같은 배치가 되고, 밸런스를 읽기 쉽다.
        /// </summary>
        private static Vector2 GetNpcSpawnPosition(
            int index,
            int count)
        {
            // 왼쪽 아래층 계단, 오른쪽 위층 계단과 회수 지점 앞은 비워 둔다.
            // 계단으로 올라오자마자 NPC에 둘러싸이지 않게 하기 위해서다.
            const float minX = -10.0f;
            const float maxX = 11.5f;

            float t =
                count <= 1
                    ? 0.5f
                    : index / (float)(count - 1);

            float x =
                Mathf.Lerp(
                    minX,
                    maxX,
                    t);

            // 앞뒤로 지그재그를 줘 한 줄로 서지 않게 한다.
            float[] depths =
            {
                0.15f,
                -1.25f,
                -3.75f,
                -0.45f,
                -2.15f,
                -4.05f
            };

            return new Vector2(
                x,
                depths[index % depths.Length]);
        }

        /// <summary>
        /// 한 판 안의 레벨·강화를 붙인다.
        /// 점수 집계기와 층 이동이 먼저 만들어져 있어야 이벤트를 구독할 수 있다.
        /// </summary>
        /// <summary>
        /// 연출 담당을 붙인다. 레벨·층 이동 알림을 구독하므로 그 둘이 먼저 만들어져 있어야 한다.
        /// </summary>
        private void CreateVfxDirector(
            PlayerSideViewController player,
            RunProgression runProgression,
            FloorTransitionController floorTransition)
        {
            GameObject directorObject =
                new GameObject(
                    "StageVfxDirector");

            StageVfxDirector director =
                directorObject.AddComponent<
                    StageVfxDirector>();

            director.Configure(
                player.transform,
                runProgression,
                floorTransition,
                player.GetComponent<RampageCoordinator>());
        }

        private RunProgression CreateRunProgression(
            PlayerSideViewController player,
            StageSessionController stage,
            StageScoreTracker scoreTracker,
            FloorTransitionController floorTransition,
            RunSession session)
        {
            GameObject panelObject =
                new GameObject(
                    "RunUpgradeChoicePanel");

            RunUpgradeChoicePanel panel =
                panelObject.AddComponent<
                    RunUpgradeChoicePanel>();

            RunProgression progression =
                player.gameObject.AddComponent<
                    RunProgression>();

            progression.Configure(
                stage,
                scoreTracker,
                floorTransition,
                panel,
                session);

            return progression;
        }

        /// <summary>층 이동 처리를 붙인다. 계단은 이미 층마다 지어져 있다.</summary>
        private FloorTransitionController CreateFloorTransition(
            PlayerSideViewController player,
            FollowerManager followers,
            CameraFollow2D cameraFollow,
            int floorCount)
        {
            GameObject transition =
                new GameObject(
                    "FloorTransition");

            FloorTransitionController controller =
                transition.AddComponent<
                    FloorTransitionController>();

            controller.Configure(
                player.transform,
                followers,
                cameraFollow,
                floorCount);

            return controller;
        }

        private void CreateGeumtaeyang(
            StageSessionController stage,
            FollowerManager playerFollowers,
            PlayerSideViewController player,
            int floorCount)
        {
            GameObject rival =
                new GameObject(
                    "금태양_01");

            // 1층은 연습 층이다. 금태양은 2층에서 처음 만난다.
            Vector2 rivalStart =
                FloorSpace.ToWorld(
                    FloorPlanLogic.ClampFloor(
                        OpponentFloorPlan.GeumtaeyangStartFloor,
                        floorCount),
                    new Vector2(
                        -4.0f,
                        -2.7f));

            rival.transform.position =
                new Vector3(
                    rivalStart.x,
                    rivalStart.y,
                    0f);

            rival.AddComponent<
                SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                rival.AddComponent<
                    RuntimeCharacterSpriteAnimator>();

            animator.Configure(
                "Characters/Geumtaeyang",
                8f,
                390f);

            animator.SetBaseTint(
                Color.white);

            Rigidbody2D body =
                rival.AddComponent<
                    Rigidbody2D>();

            body.gravityScale =
                0f;

            body.constraints =
                RigidbodyConstraints2D.FreezeRotation;

            body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            body.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D collider =
                rival.AddComponent<
                    BoxCollider2D>();

            collider.size =
                new Vector2(
                    0.52f,
                    0.34f);

            collider.offset =
                new Vector2(
                    0f,
                    0.17f);

            collider.isTrigger =
                true;

            rival.AddComponent<
                DepthSortByY>();

            rival.AddComponent<
                OpponentFollowerManager>();

            GeumtaeyangController controller =
                rival.AddComponent<
                    GeumtaeyangController>();

            controller.Configure(
                stage,
                playerFollowers,
                animator);

            rival.AddComponent<
                OpponentDuelTarget>();
        }

        private void CreatePopularGuy(
            StageSessionController stage,
            FollowerManager playerFollowers,
            PlayerSideViewController player,
            int floorCount)
        {
            GameObject popularGuy =
                new GameObject(
                    "인기남_01");

            // 인기남은 3층에서 처음 만난다. 위층일수록 경쟁자가 한 명씩 늘어난다.
            Vector2 popularGuyStart =
                FloorSpace.ToWorld(
                    FloorPlanLogic.ClampFloor(
                        OpponentFloorPlan.PopularGuyStartFloor,
                        floorCount),
                    new Vector2(
                        8.2f,
                        -2.8f));

            popularGuy.transform.position =
                new Vector3(
                    popularGuyStart.x,
                    popularGuyStart.y,
                    0f);

            popularGuy.AddComponent<
                SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                popularGuy.AddComponent<
                    RuntimeCharacterSpriteAnimator>();

            animator.Configure(
                "Characters/PopularGuy",
                8f,
                390f);

            animator.SetBaseTint(
                Color.white);

            Rigidbody2D body =
                popularGuy.AddComponent<
                    Rigidbody2D>();

            body.gravityScale =
                0f;

            body.constraints =
                RigidbodyConstraints2D.FreezeRotation;

            body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            body.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D collider =
                popularGuy.AddComponent<
                    BoxCollider2D>();

            collider.size =
                new Vector2(
                    0.52f,
                    0.34f);

            collider.offset =
                new Vector2(
                    0f,
                    0.17f);

            collider.isTrigger =
                true;

            popularGuy.AddComponent<
                DepthSortByY>();

            popularGuy.AddComponent<
                OpponentFollowerManager>();

            PopularGuyController controller =
                popularGuy.AddComponent<
                    PopularGuyController>();

            controller.Configure(
                stage,
                playerFollowers,
                animator);

            popularGuy.AddComponent<
                OpponentDuelTarget>();
        }

        /// <summary>
        /// 층마다 회수 지점을 둔다.
        /// 계단은 복도 왼쪽 끝, 회수 지점은 오른쪽 끝이라
        /// "데리고 가로지르는" 이동이 매 층마다 생긴다.
        /// </summary>
        private void CreateRecoveryPoint(
            StageSessionController stage,
            FollowerManager followers,
            int floorIndex)
        {
            GameObject recovery =
                new GameObject(
                    $"RecoveryPoint_{floorIndex + 1}F");

            recovery.AddComponent<
                SpriteRenderer>();

            RecoveryPoint point =
                recovery.AddComponent<
                    RecoveryPoint>();

            point.Configure(
                stage,
                followers,
                FloorSpace.ToWorld(
                    floorIndex,
                    new Vector2(
                        FloorLayout.RecoveryX,
                        FloorLayout.RecoveryY)),
                new Vector2(
                    FloorLayout.RecoveryWidth,
                    5.0f));
        }

        private void CreateCursorController()
        {
            if (FindFirstObjectByType<
                    HypnosisCursorController>() != null)
            {
                return;
            }

            GameObject cursor =
                new GameObject(
                    "HypnosisCursorController");

            cursor.AddComponent<
                HypnosisCursorController>();
        }

        private void CreateCaptureHud(
            PlayerCaptureController capture)
        {
            GameObject captureHud =
                new GameObject(
                    "CaptureHud");

            CaptureHudView view =
                captureHud.AddComponent<
                    CaptureHudView>();

            view.Configure(
                capture);
        }

        private void CreateDuelHud(
            OpponentDuelController duel)
        {
            GameObject duelHud =
                new GameObject(
                    "OpponentDuelHud");

            OpponentDuelHud view =
                duelHud.AddComponent<
                    OpponentDuelHud>();

            view.Configure(
                duel);
        }

        private void CreateStageResultPanel(
            StageSessionController stage,
            StageScoreTracker tracker)
        {
            GameObject panelObject =
                new GameObject(
                    "StageResultPanel");

            StageResultPanel panel =
                panelObject.AddComponent<
                    StageResultPanel>();

            panel.Configure(
                stage,
                tracker);
        }

        private void CreateStageTelemetry(
            StageSessionController stage,
            FollowerManager followers,
            PlayerHealth health)
        {
            GameObject telemetryObject =
                new GameObject(
                    "StageTelemetry");

            StageTelemetry telemetry =
                telemetryObject.AddComponent<
                    StageTelemetry>();

            telemetry.Configure(
                stage,
                followers,
                health);
        }

        private void CreateStageEndController(
            StageSessionController stage,
            PlayerSideViewController movement,
            HypnosisCaster hypnosis,
            PlayerCaptureController capture)
        {
            GameObject endControllerObject =
                new GameObject(
                    "StageEndController");

            StageEndController endController =
                endControllerObject.AddComponent<
                    StageEndController>();

            endController.Configure(
                stage,
                movement,
                hypnosis,
                capture);
        }

        private void CreateHud(
            HypnosisCaster caster,
            StageSessionController stage,
            PlayerHealth health)
        {
            // 개발용 수치는 20일차부터 F1 디버그 패널(CreateDebugPanel)이 맡는다.
            GameObject hud =
                new GameObject(
                    "StageHud");

            StageHudView view =
                hud.AddComponent<StageHudView>();

            view.Configure(
                caster,
                stage,
                health);
        }
    }
}
