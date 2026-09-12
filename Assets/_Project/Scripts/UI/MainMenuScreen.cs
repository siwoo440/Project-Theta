using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Save;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 타이틀 화면이다. 16일차에 정식 UI로 교체하므로 지금은 IMGUI로 둔다.
    /// </summary>
    public sealed class MainMenuScreen : MonoBehaviour
    {
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;

        private void OnGUI()
        {
            EnsureStyles();

            float width =
                Mathf.Min(
                    460f,
                    Screen.width *
                    0.7f);

            const float height = 300f;

            float x =
                (Screen.width -
                 width) *
                0.5f;

            float y =
                (Screen.height -
                 height) *
                0.5f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height),
                string.Empty);

            GUI.Label(
                new Rect(
                    x,
                    y + 32f,
                    width,
                    48f),
                "프로젝트 θ",
                _titleStyle);

            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            GUI.Label(
                new Rect(
                    x,
                    y + 96f,
                    width,
                    24f),
                save == null
                    ? "저장 정보 없음"
                    : $"플레이 {save.PlayCount}회   클리어 {save.ClearCount}회   최고 랭크 {save.BestRankLabel}",
                _labelStyle);

            if (GUI.Button(
                    new Rect(
                        x + (width * 0.25f),
                        y + 150f,
                        width * 0.5f,
                        44f),
                    "시 작",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.Hub);
            }

            GUI.Label(
                new Rect(
                    x,
                    y + 214f,
                    width,
                    22f),
                save == null
                    ? string.Empty
                    : $"최고 점수 {save.BestScore:N0}",
                _labelStyle);

            GUI.Label(
                new Rect(
                    x,
                    y + height - 34f,
                    width,
                    22f),
                "13일차 화면 흐름 골격",
                _labelStyle);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 34,
                    fontStyle =
                        FontStyle.Bold
                };

            _labelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 15
                };

            _buttonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 18,
                    fontStyle =
                        FontStyle.Bold
                };
        }
    }
}
