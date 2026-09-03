using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    public sealed class ProjectThetaPrototypeBootstrap : MonoBehaviour
    {
        private static Sprite _squareSprite; // 공용 스프라이트

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreatePrototype()
        {
            if (SceneManager.GetActiveScene().name != "Test") // 테스트 씬 확인
            {
                return; // 자동 생성 중단
            }

            if (FindFirstObjectByType<ProjectThetaPrototypeBootstrap>() != null) // 기존 부트스트랩 확인
            {
                return; // 중복 생성 중단
            }

            GameObject bootstrapObject = new GameObject("ProjectThetaPrototypeBootstrap"); // 부트스트랩 생성
            bootstrapObject.AddComponent<ProjectThetaPrototypeBootstrap>(); // 컴포넌트 추가
        }

        private void Start()
        {
            CreateBackground(); // 배경 생성
            PlayerSideViewController player = CreatePlayer(); // 플레이어 생성
            CreateCamera(player.transform); // 카메라 생성
            CreateStageGoal(); // 목표 생성
            CreateCollector(); // 회수 지점 생성
            CreateNpcs(); // NPC 생성
            CreateHud(player.GetComponent<HypnosisCaster>()); // HUD 생성
        }

        private PlayerSideViewController CreatePlayer()
        {
            GameObject player = CreateActor("Player", new Vector3(-6f, 0f, 0f), new Color(0.2f, 0.75f, 1f)); // 플레이어 생성
            Rigidbody2D body = player.AddComponent<Rigidbody2D>(); // 물리 추가
            body.gravityScale = 0f; // 중력 제거
            body.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전만 고정
            player.AddComponent<BoxCollider2D>(); // 충돌 추가
            PlayerSideViewController controller = player.AddComponent<PlayerSideViewController>(); // 이동 추가
            LineRenderer line = player.AddComponent<LineRenderer>(); // 시선 라인 추가
            line.positionCount = 2; // 라인 점 설정
            line.startWidth = 0.06f; // 시작 폭 설정
            line.endWidth = 0.02f; // 끝 폭 설정
            line.material = new Material(Shader.Find("Sprites/Default")); // 라인 재질 설정
            line.startColor = new Color(1f, 0.2f, 0.85f); // 라인 시작색
            line.endColor = new Color(1f, 0.5f, 0.9f); // 라인 끝색
            line.enabled = false; // 초기 숨김
            player.AddComponent<HypnosisCaster>(); // 최면 시전자 추가
            return controller; // 플레이어 반환
        }

        private void CreateCamera(Transform target)
        {
            Camera camera = Camera.main; // 메인 카메라 탐색
            if (camera == null) // 카메라 확인
            {
                GameObject cameraObject = new GameObject("Main Camera"); // 카메라 객체 생성
                camera = cameraObject.AddComponent<Camera>(); // 카메라 추가
                cameraObject.tag = "MainCamera"; // 메인 태그 설정
            }

            camera.orthographic = true; // 직교 카메라 설정
            camera.orthographicSize = 4.2f; // 화면 크기 설정
            camera.transform.position = new Vector3(target.position.x, 1.2f, -10f); // 초기 위치 설정
            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>() ?? camera.gameObject.AddComponent<CameraFollow2D>(); // 추적 추가
            follow.Configure(target); // 추적 대상 설정
        }

        private void CreateNpcs()
        {
            Vector2[] positions = { new Vector2(-2.5f, -1.2f), new Vector2(0f, 0.9f), new Vector2(2.5f, -0.4f), new Vector2(5f, 1.3f), new Vector2(7.5f, -1.1f), new Vector2(10f, 0.5f) }; // NPC 위치
            for (int i = 0; i < positions.Length; i++) // NPC 순회
            {
                GameObject npc = CreateActor($"NPC_{i + 1:00}", new Vector3(positions[i].x, positions[i].y, 0f), new Color(1f, 0.55f, 0.75f)); // NPC 생성
                npc.AddComponent<BoxCollider2D>(); // 충돌 추가
                npc.AddComponent<NpcAgent>(); // NPC AI 추가
                npc.AddComponent<ProjectTheta.Companion.FollowerController>(); // 동행 추가
                npc.AddComponent<HypnosisTarget>(); // 최면 대상 추가
            }
        }

        private void CreateCollector()
        {
            GameObject collector = CreateActor("EssenceCollector", new Vector3(13f, 0f, 0f), new Color(0.7f, 0.25f, 1f)); // 회수 지점 생성
            collector.transform.localScale = new Vector3(1.4f, 5.4f, 1f); // 지점 크기 설정
            BoxCollider2D trigger = collector.AddComponent<BoxCollider2D>(); // 트리거 추가
            trigger.isTrigger = true; // 트리거 설정
            Rigidbody2D collectorBody = collector.AddComponent<Rigidbody2D>(); // 트리거 물리 추가
            collectorBody.bodyType = RigidbodyType2D.Kinematic; // 정적 회수 지점 설정
            collectorBody.gravityScale = 0f; // 중력 제거
            collector.AddComponent<EssenceCollector>(); // 회수 기능 추가
        }

        private void CreateStageGoal()
        {
            GameObject goal = new GameObject("StageGoalManager"); // 목표 객체 생성
            goal.AddComponent<StageGoalManager>(); // 목표 기능 추가
        }

        private void CreateHud(HypnosisCaster caster)
        {
            GameObject hud = new GameObject("PrototypeHud"); // HUD 객체 생성
            PrototypeHud prototypeHud = hud.AddComponent<PrototypeHud>(); // HUD 기능 추가
            prototypeHud.Configure(caster); // 시전자 연결
        }

        private void CreateBackground()
        {
            GameObject background = CreateActor("PrototypeBackground", new Vector3(3.5f, 0.3f, 1f), new Color(0.92f, 0.93f, 0.95f)); // 배경 생성
            background.transform.localScale = new Vector3(32f, 8f, 1f); // 배경 크기 설정
            SpriteRenderer renderer = background.GetComponent<SpriteRenderer>(); // 렌더러 참조
            renderer.sortingOrder = -100; // 배경 순서 설정
        }

        private static GameObject CreateActor(string objectName, Vector3 position, Color color)
        {
            GameObject actor = new GameObject(objectName); // 객체 생성
            actor.transform.position = position; // 위치 설정
            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>(); // 렌더러 추가
            renderer.sprite = GetSquareSprite(); // 스프라이트 설정
            renderer.color = color; // 색상 설정
            renderer.sortingOrder = 0; // 정렬 설정
            if (objectName != "PrototypeBackground" && objectName != "EssenceCollector") // 캐릭터 계열 확인
            {
                actor.AddComponent<DepthSortByY>(); // 깊이 정렬 추가
            }
            actor.transform.localScale = new Vector3(0.65f, 1.5f, 1f); // 기본 크기 설정
            return actor; // 객체 반환
        }

        private static Sprite GetSquareSprite()
        {
            if (_squareSprite != null) // 캐시 확인
            {
                return _squareSprite; // 캐시 반환
            }

            Texture2D texture = new Texture2D(1, 1); // 텍스처 생성
            texture.SetPixel(0, 0, Color.white); // 픽셀 설정
            texture.Apply(); // 텍스처 적용
            _squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f); // 스프라이트 생성
            return _squareSprite; // 스프라이트 반환
        }
    }
}
