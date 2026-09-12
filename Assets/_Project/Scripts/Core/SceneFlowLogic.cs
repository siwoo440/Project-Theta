using System;

namespace ProjectTheta.Core
{
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Hub = "Hub";
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
        /// 흐름:
        ///   Boot → MainMenu → Hub → Stage → Hub → ...
        ///   Hub → MainMenu (타이틀 복귀)
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
                           SceneDestination.Stage ||
                       destination ==
                           SceneDestination.MainMenu;
            }

            if (IsStageScene(
                    currentScene))
            {
                return destination ==
                       SceneDestination.Hub;
            }

            return false;
        }

        /// <summary>현재 씬에서 세이브를 파일에 써야 하는 전환인지 판정한다.</summary>
        public static bool ShouldSaveOnTransition(
            string currentScene,
            SceneDestination destination)
        {
            // 스테이지를 떠날 때 결과가 확정되므로 이때 반드시 저장한다.
            return IsStageScene(
                       currentScene) &&
                   destination ==
                   SceneDestination.Hub;
        }
    }
}
