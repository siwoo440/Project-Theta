using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
        private static Sprite _squareSprite;

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

            CreateCamera(player.transform);
            CreateNpcs(player);
            CreateRecoveryPointPlaceholder();
            CreateCursorController();

            CreateHud(
                player.GetComponent<HypnosisCaster>());
        }

        private PlayerSideViewController CreatePlayer()
        {
            GameObject player =
                new GameObject("Player");

            player.transform.position =
                new Vector3(-13.5f, -0.45f, 0f);

            SpriteRenderer renderer =
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
                new Vector2(0.52f, 0.34f);

            playerCollider.offset =
                new Vector2(0f, 0.17f);

            player.AddComponent<DepthSortByY>();

            PlayerSideViewController controller =
                player.AddComponent<
                    PlayerSideViewController>();

            player.AddComponent<HypnosisCaster>();

            return controller;
        }

        private void CreateCamera(Transform target)
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                GameObject cameraObject =
                    new GameObject("Main Camera");

                camera =
                    cameraObject.AddComponent<Camera>();

                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 4.65f;

            camera.backgroundColor =
                new Color(0.16f, 0.20f, 0.21f);

            camera.transform.position =
                new Vector3(-10.5f, 1.2f, -10f);

            CameraFollow2D follow =
                camera.GetComponent<CameraFollow2D>() ??
                camera.gameObject.AddComponent<
                    CameraFollow2D>();

            follow.Configure(
                target,
                new Vector2(-10.5f, -1.25f),
                new Vector2(10.5f, 1.65f));
        }

        private void CreateNpcs(
            PlayerSideViewController player)
        {
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
                    new Vector2(0.48f, 0.32f);

                collider.offset =
                    new Vector2(0f, 0.16f);

                NpcAgent agent =
                    npc.AddComponent<NpcAgent>();

                npc.AddComponent<HypnosisTarget>();
                npc.AddComponent<NpcHypnosisStatusView>();
                npc.AddComponent<DepthSortByY>();

                agent.Configure(
                    player.transform,
                    animator);
            }
        }

        private void CreateRecoveryPointPlaceholder()
        {
            GameObject recoveryPoint =
                CreatePlaceholder(
                    "RecoveryPointPlaceholder",
                    new Vector3(
                        14.6f,
                        -4.45f,
                        0f),
                    new Color(
                        0.68f,
                        0.28f,
                        0.96f));

            recoveryPoint.transform.localScale =
                new Vector3(1.3f, 1.0f, 1f);

            recoveryPoint.AddComponent<DepthSortByY>();
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

        private void CreateHud(
            HypnosisCaster caster)
        {
            GameObject hud =
                new GameObject("PrototypeHud");

            PrototypeHud prototypeHud =
                hud.AddComponent<PrototypeHud>();

            prototypeHud.Configure(caster);
        }

        private static GameObject CreatePlaceholder(
            string objectName,
            Vector3 position,
            Color color)
        {
            GameObject actor =
                new GameObject(objectName);

            actor.transform.position = position;

            SpriteRenderer renderer =
                actor.AddComponent<SpriteRenderer>();

            renderer.sprite = GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = 0;

            return actor;
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            Texture2D texture =
                new Texture2D(1, 1)
                {
                    name =
                        "ProjectTheta_RuntimeSquare",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

            texture.SetPixel(
                0,
                0,
                Color.white);

            texture.Apply();

            _squareSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);

            return _squareSprite;
        }
    }
}
