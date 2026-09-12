using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 씬이 로드될 때마다 해당 씬에 맞는 부트스트랩을 실행한다.
    ///
    /// <see cref="RuntimeInitializeOnLoadMethod"/>는 런타임이 시작될 때 한 번만 호출되므로,
    /// 그것만으로는 허브에서 스테이지로 재진입했을 때 씬이 구성되지 않는다.
    /// 따라서 최초 씬은 직접 구성하고, 이후 씬은 sceneLoaded 이벤트로 처리한다.
    /// </summary>
    public static class SceneBootstrapRouter
    {
        private static bool _subscribed;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!_subscribed)
            {
                _subscribed =
                    true;

                SceneManager.sceneLoaded -=
                    HandleSceneLoaded;

                SceneManager.sceneLoaded +=
                    HandleSceneLoaded;
            }

            // 최초 씬은 sceneLoaded 구독 전에 이미 로드되었으므로 직접 구성한다.
            Build(
                SceneManager.GetActiveScene().name);
        }

        private static void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (mode !=
                LoadSceneMode.Single)
            {
                return;
            }

            Build(
                scene.name);
        }

        /// <summary>
        /// 각 부트스트랩은 이미 구성되어 있으면 스스로 빠져나가므로
        /// 중복 호출되어도 안전하다.
        /// </summary>
        private static void Build(
            string sceneName)
        {
            if (SceneFlowLogic.IsStageScene(
                    sceneName))
            {
                ProjectThetaPrototypeBootstrap.CreateForScene();

                return;
            }

            if (sceneName == SceneNames.Boot ||
                sceneName == SceneNames.MainMenu ||
                sceneName == SceneNames.Hub)
            {
                SceneScreenBootstrap.CreateForScene();
            }
        }
    }
}
