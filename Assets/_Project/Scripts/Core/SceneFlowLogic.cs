using System;

namespace ProjectTheta.Core
{
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Hub = "Hub";

        /// <summary>21일차: 한 판 안에서 다음 장소를 고르는 도시 지도 씬이다.</summary>
        public const string Map = "Map";
        public const string Stage = "TestStage";

        /// <summary>2일차까지 쓰던 예전 테스트 씬 이름이다.</summary>
        public const string LegacyStage = "Test";
    }

    /// <summary>화면 흐름상 이동할 수 있는 목적지다.</summary>
    public enum SceneDestination
    {
        None,
        MainMenu,
        Hub,
        Map,
        Stage
    }

    /// <summary>
    /// 씬 전환 규칙이다.
    ///
    /// 실제 로딩은 <see cref="SceneFlowController"/>가 하고, 여기서는
    /// "어느 씬에서 어디로 갈 수 있는가"만 판정해 테스트 가능하게 분리했다.
    /// </summary>
    public static class SceneFlowLogic
    {
        /// <summary>부트 씬은 항상 타이틀로 자동 진입한다.</summary>
        public const SceneDestination BootDestination =
            SceneDestination.MainMenu;

        public static bool IsStageScene(
            string sceneName)
        {
            return string.Equals(
                       sceneName,
                       SceneNames.Stage,
                       StringComparison.Ordinal) ||
                   string.Equals(
                       sceneName,
                       SceneNames.LegacyStage,
                       StringComparison.Ordinal);
        }

        public static string GetSceneName(
            SceneDestination destination)
        {
            switch (destination)
            {
                case SceneDestination.MainMenu:
                    return SceneNames.MainMenu;

                case SceneDestination.Hub:
                    return SceneNames.Hub;

                case SceneDestination.Map:
                    return SceneNames.Map;

                case SceneDestination.Stage:
                    return SceneNames.Stage;

                case SceneDestination.None:
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 현재 씬에서 해당 목적지로 이동할 수 있는지 판정한다.
        ///
        /// 흐름 (21일차부터 지도를 거친다):
        ///   Boot → MainMenu → Hub → Map → Stage → Map → Stage … → Hub
        ///   Hub → MainMenu (타이틀 복귀)
        ///   Map → Hub (판 포기)
        ///   Stage → Hub (실패 또는 마지막 구역 클리어)
        /// </summary>
        public static bool CanTransition(
            string currentScene,
            SceneDestination destination)
        {
            if (destination ==
                SceneDestination.None)
            {
                return false;
            }

            if (string.Equals(
                    currentScene,
                    SceneNames.Boot,
                    StringComparison.Ordinal))
            {
                return destination ==
                       SceneDestination.MainMenu;
            }

            if (string.Equals(
                    currentScene,
                    SceneNames.MainMenu,
                    StringComparison.Ordinal))
            {
                return destination ==
                       SceneDestination.Hub;
            }

            if (string.Equals(
                    currentScene,
                    SceneNames.Hub,
                    StringComparison.Ordinal))
            {
                return destination ==
                           SceneDestination.Map ||
                       destination ==
                           SceneDestination.MainMenu;
            }

            if (string.Equals(
                    currentScene,
                    SceneNames.Map,
                    StringComparison.Ordinal))
            {
                return destination ==
                           SceneDestination.Stage ||
                       destination ==
                           SceneDestination.Hub;
            }

            if (IsStageScene(
                    currentScene))
            {
                return destination ==
                           SceneDestination.Hub ||
                       destination ==
                           SceneDestination.Map;
            }

            return false;
        }

        /// <summary>현재 씬에서 세이브를 파일에 써야 하는 전환인지 판정한다.</summary>
        public static bool ShouldSaveOnTransition(
            string currentScene,
            SceneDestination destination)
        {
            // 스테이지를 떠날 때 결과가 확정되므로 이때 반드시 저장한다.
            // 21일차: 구역을 클리어하고 지도로 갈 때도 그 구역의 정기를 확정해 저장한다.
            // 다음 구역에서 실패해도 이미 클리어한 구역의 보상은 남는다(기획서 24장).
            return IsStageScene(
                       currentScene) &&
                   (destination ==
                        SceneDestination.Hub ||
                    destination ==
                        SceneDestination.Map);
        }
    }
}
