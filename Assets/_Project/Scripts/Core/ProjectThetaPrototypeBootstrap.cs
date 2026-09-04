using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Player;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
        private static Sprite _squareSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreatePrototype()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName != "Test" && sceneName != "TestStage")
            {
                return;
            }

            if (FindFirstObjectByType<ProjectThetaPrototypeBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrapObject =
                new GameObject("ProjectThetaPrototypeBootstrap");

            bootstrapObject.AddComponent<ProjectThetaPrototypeBootstrap>();
        }

        private void Start()
        {
            SchoolHallwayPrototypeBuilder.Build();

            PlayerSideViewController player = CreatePlayer();

            CreateCamera(player.transform);
            CreateNpcPlaceholders();
            CreateRecoveryPointPlaceholder();
            CreateHud();
        }

        private PlayerSideViewController CreatePlayer()
        {
            GameObject player = CreateActor(
                "Player",
                new Vector3(-13.5f, -0.45f, 0f),
                new Color(0.20f, 0.72f, 0.96f));

            player.transform.localScale =
                new Vector3(0.72f, 1.35f, 1f);

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D playerCollider =
                player.AddComponent<BoxCollider2D>();

            playerCollider.size = new Vector2(0.82f, 0.78f);
            playerCollider.offset = new Vector2(0f, -0.08f);

            player.AddComponent<DepthSortByY>();

            return player.AddComponent<PlayerSideViewController>();
        }

        private void CreateCamera(Transform target)
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
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
                camera.gameObject.AddComponent<CameraFollow2D>();

            follow.Configure(
                target,
                new Vector2(-10.5f, -1.25f),
                new Vector2(10.5f, 1.65f));
        }

        private void CreateNpcPlaceholders()
        {
            Vector2[] positions =
            {
                new Vector2(-8.0f, 0.25f),
                new Vector2(-5.8f, -1.15f),
                new Vector2(-1.2f, -0.10f),
                new Vector2(2.0f, -1.40f),
                new Vector2(8.2f, 0.15f),
                new Vector2(11.6f, -0.75f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject npc = CreateActor(
                    $"NPC_Placeholder_{i + 1:00}",
                    positions[i],
                    new Color(0.96f, 0.52f, 0.69f));

                npc.transform.localScale =
                    new Vector3(0.68f, 1.28f, 1f);

                BoxCollider2D collider =
                    npc.AddComponent<BoxCollider2D>();

                collider.size = new Vector2(0.82f, 0.75f);
                collider.offset = new Vector2(0f, -0.08f);

                npc.AddComponent<DepthSortByY>();
            }
        }

        private void CreateRecoveryPointPlaceholder()
        {
            GameObject recoveryPoint = CreateActor(
                "RecoveryPointPlaceholder",
                new Vector3(14.6f, -0.55f, 0f),
                new Color(0.68f, 0.28f, 0.96f));

            recoveryPoint.transform.localScale =
                new Vector3(1.3f, 1.8f, 1f);

            recoveryPoint.AddComponent<DepthSortByY>();
        }

        private void CreateHud()
        {
            GameObject hud = new GameObject("PrototypeHud");
            hud.AddComponent<PrototypeHud>();
        }

        private static GameObject CreateActor(
            string objectName,
            Vector3 position,
            Color color)
        {
            GameObject actor = new GameObject(objectName);
            actor.transform.position = position;

            SpriteRenderer renderer =
                actor.AddComponent<SpriteRenderer>();

            renderer.sprite = GetSquareSprite();
            renderer.color = color;
            renderer.sortingOrder = 0;

            actor.transform.localScale =
                new Vector3(0.65f, 1.5f, 1f);

            return actor;
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null)
            {
                return _squareSprite;
            }

            Texture2D texture = new Texture2D(1, 1)
            {
                name = "ProjectTheta_RuntimeSquare",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            texture.SetPixel(0, 0, Color.white);
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
