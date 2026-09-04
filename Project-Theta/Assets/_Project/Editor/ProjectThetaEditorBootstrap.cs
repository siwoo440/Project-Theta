using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using ProjectTheta.Core;

namespace ProjectTheta.Editor
{
    [InitializeOnLoad]
    public static class ProjectThetaEditorBootstrap
    {
        private const string Root = "Assets/_Project"; // 프로젝트 루트
        private const string Scenes = Root + "/Scenes"; // 씬 경로
        private const string Settings = Root + "/Settings"; // 설정 경로

        static ProjectThetaEditorBootstrap()
        {
            EditorApplication.delayCall += EnsureProjectWhenSafe; // 지연 초기화 등록
        }

        private static void EnsureProjectWhenSafe()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) // 플레이 모드 진입/실행 확인
            {
                return; // 플레이 중에는 씬/에셋 편집 금지
            }

            EnsureProject(); // 에디트 모드에서만 프로젝트 보정
        }

        [MenuItem("Project Theta/Setup Prototype")]
        public static void EnsureProject()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) // 플레이 모드 진입/실행 확인
            {
                Debug.LogWarning("[Project Theta] Setup Prototype은 Edit Mode에서만 실행할 수 있습니다."); // 안내
                return; // 플레이 중 씬 저장 API 호출 방지
            }

            EnsureFolders(); // 폴더 확인
            EnsureUrp2D(); // 렌더러 확인
            EnsureSortingLayers(); // 정렬 레이어 확인
            EnsureScenes(); // 씬 확인
            AssetDatabase.SaveAssets(); // 에셋 저장
            AssetDatabase.Refresh(); // 에셋 갱신
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Scenes); // 씬 폴더 생성
            Directory.CreateDirectory(Settings); // 설정 폴더 생성
        }

        private static void EnsureUrp2D()
        {
            string rendererPath = Settings + "/ProjectThetaRenderer2D.asset"; // 렌더러 경로
            string pipelinePath = Settings + "/ProjectThetaURP.asset"; // 파이프라인 경로
            Renderer2DData rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>(rendererPath); // 렌더러 로드
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath); // 파이프라인 로드
            if (rendererData == null) // 렌더러 확인
            {
                rendererData = ScriptableObject.CreateInstance<Renderer2DData>(); // 렌더러 생성
                AssetDatabase.CreateAsset(rendererData, rendererPath); // 렌더러 저장
            }

            if (pipeline == null) // 파이프라인 확인
            {
                pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>(); // 파이프라인 생성
                AssetDatabase.CreateAsset(pipeline, pipelinePath); // 파이프라인 저장
            }

            SerializedObject serializedPipeline = new SerializedObject(pipeline); // 파이프라인 직렬화
            SerializedProperty rendererList = serializedPipeline.FindProperty("m_RendererDataList"); // 렌더러 목록 탐색
            if (rendererList != null) // 목록 확인
            {
                rendererList.arraySize = 1; // 목록 크기 설정
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData; // 2D 렌더러 연결
            }

            SerializedProperty defaultRenderer = serializedPipeline.FindProperty("m_DefaultRendererIndex"); // 기본 렌더러 탐색
            if (defaultRenderer != null) // 기본 렌더러 확인
            {
                defaultRenderer.intValue = 0; // 기본 인덱스 설정
            }

            serializedPipeline.ApplyModifiedPropertiesWithoutUndo(); // 설정 적용
            GraphicsSettings.defaultRenderPipeline = pipeline; // 그래픽 파이프라인 설정
            QualitySettings.renderPipeline = pipeline; // 품질 파이프라인 설정
            EditorUtility.SetDirty(pipeline); // 변경 표시
        }

        private static void EnsureSortingLayers()
        {
            string[] names = { "Background", "EnvironmentBack", "Character", "EnvironmentFront", "VFX" }; // 정렬 레이어 목록
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]); // 태그 설정 로드
            SerializedProperty layers = tagManager.FindProperty("m_SortingLayers"); // 정렬 레이어 탐색
            foreach (string name in names) // 레이어 순회
            {
                if (HasSortingLayer(layers, name)) // 기존 레이어 확인
                {
                    continue; // 중복 건너뜀
                }

                layers.InsertArrayElementAtIndex(layers.arraySize); // 레이어 슬롯 추가
                SerializedProperty layer = layers.GetArrayElementAtIndex(layers.arraySize - 1); // 새 레이어 참조
                layer.FindPropertyRelative("name").stringValue = name; // 레이어 이름 설정
                layer.FindPropertyRelative("uniqueID").intValue = GenerateSortingLayerId(layers.arraySize); // 레이어 ID 설정
                layer.FindPropertyRelative("locked").boolValue = false; // 잠금 해제
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo(); // 레이어 적용
        }

        private static bool HasSortingLayer(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++) // 레이어 순회
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i); // 레이어 참조
                if (layer.FindPropertyRelative("name").stringValue == name) // 이름 비교
                {
                    return true; // 존재 반환
                }
            }

            return false; // 미존재 반환
        }

        private static int GenerateSortingLayerId(int seed)
        {
            unchecked // 오버플로 허용
            {
                return ("ProjectTheta_" + seed).GetHashCode(); // 정렬 ID 생성
            }
        }

        private static void EnsureScenes()
        {
            string bootPath = Scenes + "/Boot.unity"; // 부트 씬 경로
            string testPath = Scenes + "/Test.unity"; // 테스트 씬 경로
            if (!File.Exists(testPath)) // 테스트 씬 확인
            {
                Scene testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 테스트 씬 생성
                EditorSceneManager.SaveScene(testScene, testPath); // 테스트 씬 저장
            }

            EnsureTestCamera(testPath); // 테스트 씬 카메라 보장

            if (!File.Exists(bootPath)) // 부트 씬 확인
            {
                Scene bootScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // 부트 씬 생성
                GameObject loader = new GameObject("BootLoader"); // 로더 생성
                loader.AddComponent<BootLoader>(); // 로더 기능 추가
                EditorSceneManager.SaveScene(bootScene, bootPath); // 부트 씬 저장
            }

            EditorBuildSettings.scenes = new[] // 빌드 씬 목록
            {
                new EditorBuildSettingsScene(bootPath, true), // 부트 씬 등록
                new EditorBuildSettingsScene(testPath, true) // 테스트 씬 등록
            };
        }

        private static void EnsureTestCamera(string testPath)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) // 플레이 모드 진입/실행 확인
            {
                return; // 플레이 중에는 씬을 열거나 저장하지 않음
            }

            Scene loadedScene = SceneManager.GetSceneByPath(testPath); // 이미 열린 테스트 씬 탐색
            bool wasLoaded = loadedScene.IsValid() && loadedScene.isLoaded; // 기존 로드 상태 확인
            Scene testScene = wasLoaded
                ? loadedScene
                : EditorSceneManager.OpenScene(testPath, OpenSceneMode.Additive); // 다른 씬을 유지한 채 테스트 씬 열기

            Camera camera = FindCameraInScene(testScene); // 테스트 씬 내부 카메라 탐색
            if (camera == null) // 카메라가 없다면 생성
            {
                GameObject cameraObject = new GameObject("Main Camera"); // 카메라 오브젝트 생성
                SceneManager.MoveGameObjectToScene(cameraObject, testScene); // 테스트 씬 소속으로 이동
                camera = cameraObject.AddComponent<Camera>(); // 카메라 컴포넌트 추가
            }

            camera.gameObject.name = "Main Camera"; // 이름 통일
            camera.gameObject.tag = "MainCamera"; // Camera.main 탐색용 태그 설정
            camera.orthographic = true; // 2D 직교 카메라 설정
            camera.orthographicSize = 4.2f; // 프로토타입 화면 범위 설정
            camera.transform.position = new Vector3(0f, 1.2f, -10f); // 기본 카메라 위치

            EditorSceneManager.MarkSceneDirty(testScene); // 씬 변경 표시
            EditorSceneManager.SaveScene(testScene, testPath); // 카메라를 씬 파일에 저장

            if (!wasLoaded) // 원래 열려 있지 않았던 씬이면
            {
                EditorSceneManager.CloseScene(testScene, true); // 저장 후 다시 닫아 사용자의 현재 씬 유지
            }
        }

        private static Camera FindCameraInScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) // 루트 오브젝트 순회
            {
                Camera camera = root.GetComponentInChildren<Camera>(true); // 비활성 자식까지 탐색
                if (camera != null)
                {
                    return camera; // 첫 카메라 반환
                }
            }

            return null; // 카메라 없음
        }
    }
}
