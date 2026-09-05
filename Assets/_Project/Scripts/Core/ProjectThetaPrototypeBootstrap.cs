using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Capture;
using ProjectTheta.Duel;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Rival;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreatePrototype()
        {
            string sceneName =
                SceneManager.GetActiveScene().name;

            if (sceneName != "Test" &&
                sceneName != "TestStage")
            {
                return;
            }

            if (FindFirstObjectByType<
                    ProjectThetaPrototypeBootstrap>() != null)
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
            SchoolHallwayPrototypeBuilder.Build();

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

            CreateCamera(
                player.transform);

            CreateNpcs(
                player);

            CreateGeumtaeyang(
                stage,
                followers,
                player);

            CreatePopularGuy(
                stage,
                followers,
                player);

            CreateRecoveryPoint(
                stage,
                followers);

            CreateCursorController();

            CreateCaptureHud(
                capture);

            CreateDuelHud(
                duel);

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

            player.transform.position =
                new Vector3(
                    -13.5f,
                    -0.45f,
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
            player.AddComponent<HypnosisCaster>();

            PlayerHealth health =
                player.AddComponent<PlayerHealth>();

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

            return controller;
        }

        private void CreateCamera(
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

            follow.Configure(
                target,
                new Vector2(
                    -10.5f,
                    -1.25f),
                new Vector2(
                    10.5f,
                    1.65f));
        }

        private void CreateNpcs(
            PlayerSideViewController player)
        {
            Collider2D playerCollider =
                player.GetComponent<Collider2D>();

            Vector2[] positions =
            {
                new Vector2(-10.5f, 0.15f),
                new Vector2(-8.0f, -1.25f),
                new Vector2(-5.2f, -3.75f),
                new Vector2(-2.0f, -0.45f),
                new Vector2(1.5f, -2.15f),
                new Vector2(4.8f, -4.05f),
                new Vector2(7.5f, 0.10f),
                new Vector2(9.8f, -1.55f),
                new Vector2(12.2f, -3.25f),
                new Vector2(15.0f, -0.70f)
            };

            for (int i = 0;
                 i < positions.Length;
                 i++)
            {
                GameObject npc =
                    new GameObject(
                        $"FemaleNPC_{i + 1:00}");

                npc.transform.position =
                    positions[i];

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
                RivalFollowerManager>();

            RivalController controller =
                rival.AddComponent<
                    RivalController>();

            controller.Configure(
                stage,
                playerFollowers,
                animator);

            OpponentDuelTarget duelTarget =
                rival.AddComponent<
                    OpponentDuelTarget>();

            duelTarget.Configure(
                OpponentDuelKind.Geumtaeyang);
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
                PopularGuyFollowerManager>();

            PopularGuyController controller =
                popularGuy.AddComponent<
                    PopularGuyController>();

            controller.Configure(
                stage,
                playerFollowers,
                animator);

            OpponentDuelTarget duelTarget =
                popularGuy.AddComponent<
                    OpponentDuelTarget>();

            duelTarget.Configure(
                OpponentDuelKind.PopularGuy);
        }

        private void CreateRecoveryPoint(
            StageSessionController stage,
            FollowerManager followers)
        {
            GameObject recovery =
                new GameObject(
                    "RecoveryPoint");

            recovery.AddComponent<
                SpriteRenderer>();

            RecoveryPoint point =
                recovery.AddComponent<
                    RecoveryPoint>();

            point.Configure(
                stage,
                followers,
                new Vector2(
                    16.2f,
                    -2.15f),
                new Vector2(
                    1.8f,
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
            GameObject hud =
                new GameObject(
                    "PrototypeHud");

            PrototypeHud prototypeHud =
                hud.AddComponent<PrototypeHud>();

            prototypeHud.Configure(
                caster,
                stage,
                health,
                capture);
        }
    }
}
