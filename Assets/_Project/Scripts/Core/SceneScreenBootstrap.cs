using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectTheta.Core;
using ProjectTheta.UI;

namespace ProjectTheta.Core
{
    /// <summary>
    /// Boot · MainMenu · Hub 씬의 내용을 런타임에 구성한다.
    /// 스테이지 씬은 기존 <see cref="ProjectThetaPrototypeBootstrap"/>이 담당한다.
    ///
    /// 씬 파일을 비워 두고 코드로 만드는 것은 이 프로젝트의 기존 방식과 같다.
    /// </summary>
    public sealed class SceneScreenBootstrap : MonoBehaviour
    {
        /// <summary>씬 로드마다 <see cref="SceneBootstrapRouter"/>가 호출한다.</summary>
        public static void CreateForScene()
        {
            string sceneName =
                SceneManager.GetActiveScene().name;

            if (sceneName != SceneNames.Boot &&
                sceneName != SceneNames.MainMenu &&
                sceneName != SceneNames.Hub)
            {
                return;
            }

            if (FindFirstObjectByType<
                    SceneScreenBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrapObject =
                new GameObject(
                    "SceneScreenBootstrap");

            bootstrapObject.AddComponent<
                SceneScreenBootstrap>();
        }

        private void Start()
        {
            EnsureCamera();

            string sceneName =
                SceneManager.GetActiveScene().name;

            if (sceneName == SceneNames.Boot)
            {
                // 부트 씬은 세이브를 읽은 뒤 곧바로 타이틀로 넘어간다.
                GameSession.Instance?.GoTo(
                    SceneFlowLogic.BootDestination);

                return;
            }

            if (sceneName == SceneNames.MainMenu)
            {
                if (FindFirstObjectByType<
                        MainMenuScreen>() == null)
                {
                    gameObject.AddComponent<
                        MainMenuScreen>();
                }

                return;
            }

            if (sceneName == SceneNames.Hub)
            {
                if (FindFirstObjectByType<
                        HubScreen>() == null)
                {
                    gameObject.AddComponent<
                        HubScreen>();
                }
            }
        }

        /// <summary>씬에 카메라가 없으면 배경만 칠할 카메라를 만든다.</summary>
        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject =
                new GameObject(
                    "UICamera")
                {
                    tag = "MainCamera"
                };

            Camera camera =
                cameraObject.AddComponent<Camera>();

            camera.orthographic =
                true;

            camera.clearFlags =
                CameraClearFlags.SolidColor;

            camera.backgroundColor =
                new Color(
                    0.06f,
                    0.05f,
                    0.09f,
                    1f);
        }
    }
}
