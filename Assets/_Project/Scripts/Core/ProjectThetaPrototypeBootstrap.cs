using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Capture;
using ProjectTheta.Duel;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Items;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Rival;
using ProjectTheta.Run;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
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
            int floorCount =
                FloorPlanLogic.DefaultFloorCount;

            SchoolHallwayPrototypeBuilder.Build(
                floorCount);

            PlayerSideViewController player =
                CreatePlayer();

            StageSessionController stage =
                player.GetComponent<
                    StageSessionController>();

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

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                CreateNpcs(
                    player,
                    floor);

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

            CreateGeumtaeyang(
                stage,
                followers,
                player);

            CreatePopularGuy(
                stage,
                followers,
                player);

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

            CreateRunProgression(
                player,
                stage,
                scoreTracker,
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
                health,
                capture);
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
        private void CreateNpcs(
            PlayerSideViewController player,
            int floorIndex)
        {
            Collider2D playerCollider =
                player.GetComponent<Collider2D>();

            int count =
                FloorPlanLogic.GetNpcCount(
                    floorIndex);

            NpcGrade[] grades =
                FloorPlanLogic.BuildGrades(
                    floorIndex,
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

                profile.Configure(
                    grades[i % grades.Length]);

                npc.AddComponent<HypnosisTarget>();
                npc.AddComponent<FollowerController>();
                npc.AddComponent<ImpulseMeter>();
                npc.AddComponent<NpcHypnosisStatusView>();
                npc.AddComponent<DepthSortByY>();

                agent.Configure(
                    player.transform,
                    animator);
            }
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
        private void CreateRunProgression(
            PlayerSideViewController player,
            StageSessionController stage,
            StageScoreTracker scoreTracker,
            FloorTransitionController floorTransition)
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
                panel);
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
            PlayerSideViewController player)
        {
            GameObject rival =
                new GameObject(
                    "금태양_01");

            rival.transform.position =
                new Vector3(
                    -4.0f,
                    -2.7f,
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
            PlayerSideViewController player)
        {
            GameObject popularGuy =
                new GameObject(
                    "인기남_01");

            popularGuy.transform.position =
                new Vector3(
                    8.2f,
                    -2.8f,
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
            PlayerHealth health,
            PlayerCaptureController capture)
        {
            // 플레이어용 정식 HUD와 개발용 디버그 HUD를 나눠 붙인다.
            GameObject hud =
                new GameObject(
                    "StageHud");

            StageHudView view =
                hud.AddComponent<StageHudView>();

            view.Configure(
                caster,
                stage,
                health);

            PrototypeHud prototypeHud =
                hud.AddComponent<PrototypeHud>();

            prototypeHud.Configure(
                caster,
                stage,
                capture);
        }
    }
}
