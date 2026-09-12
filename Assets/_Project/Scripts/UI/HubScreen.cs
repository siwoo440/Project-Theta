using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Save;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 계약 서큐버스 허브다.
    ///
    /// 13일차에는 흐름 확인용 껍데기만 만든다.
    /// 업그레이드 구매와 진행도 표시는 15일차 작업이다.
    /// </summary>
    public sealed class HubScreen : MonoBehaviour
    {
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _leftStyle;
        private GUIStyle _buttonStyle;

        private bool _resultApplied;
        private StageResultSummary _lastResult =
            StageResultSummary.Empty;

        private bool _hasLastResult;

        private void Start()
        {
            // 스테이지에서 돌아왔다면 그 결과를 세이브에 반영한다.
            if (GameSession.Instance == null)
            {
                return;
            }

            _hasLastResult =
                GameSession.Instance.HasPendingResult;

            if (_hasLastResult)
            {
                _lastResult =
                    GameSession.Instance.PendingResult;
            }

            _resultApplied =
                GameSession.Instance.ConsumePendingResult();
        }

        private void OnGUI()
        {
            EnsureStyles();

            float width =
                Mathf.Min(
                    520f,
                    Screen.width *
                    0.74f);

            const float height = 360f;

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
                    y + 22f,
                    width,
                    34f),
                "계약 서큐버스 허브",
                _titleStyle);

            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            DrawStats(
                x + 40f,
                y + 76f,
                width - 80f,
                save);

            DrawLastResult(
                x + 40f,
                y + 196f,
                width - 80f);

            if (GUI.Button(
                    new Rect(
                        x + 40f,
                        y + height - 78f,
                        (width - 100f) * 0.5f,
                        42f),
                    "출 격",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.Stage);
            }

            if (GUI.Button(
                    new Rect(
                        x + 60f + ((width - 100f) * 0.5f),
                        y + height - 78f,
                        (width - 100f) * 0.5f,
                        42f),
                    "타이틀로",
                    _buttonStyle))
            {
                GameSession.Instance?.GoTo(
                    SceneDestination.MainMenu);
            }

            GUI.Label(
                new Rect(
                    x,
                    y + height - 28f,
                    width,
                    22f),
                "업그레이드는 15일차에 추가됩니다",
                _labelStyle);
        }

        private void DrawStats(
            float x,
            float y,
            float width,
            SaveData save)
        {
            if (save == null)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y,
                        width,
                        24f),
                    "저장 정보를 불러오지 못했습니다",
                    _leftStyle);

                return;
            }

            string[] rows =
            {
                $"플레이 횟수       {save.PlayCount}회",
                $"클리어 횟수       {save.ClearCount}회",
                $"최고 랭크         {save.BestRankLabel}",
                $"최고 점수         {save.BestScore:N0}",
                $"계약 정기         {save.ContractEssence}"
            };

            for (int i = 0;
                 i < rows.Length;
                 i++)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y + (i * 23f),
                        width,
                        22f),
                    rows[i],
                    _leftStyle);
            }
        }

        private void DrawLastResult(
            float x,
            float y,
            float width)
        {
            if (!_hasLastResult)
            {
                return;
            }

            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    22f),
                _lastResult.Cleared
                    ? $"직전 결과   클리어   랭크 {_lastResult.RankLabel}   {_lastResult.TotalScore:N0}점"
                    : $"직전 결과   실패   {_lastResult.TotalScore:N0}점",
                _leftStyle);

            GUI.Label(
                new Rect(
                    x,
                    y + 23f,
                    width,
                    22f),
                _resultApplied
                    ? "결과가 저장에 반영되었습니다"
                    : "반영할 결과가 없습니다",
                _leftStyle);
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
                    fontSize = 24,
                    fontStyle =
                        FontStyle.Bold
                };

            _labelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 13
                };

            _leftStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 15,
                    fontStyle =
                        FontStyle.Bold
                };

            _buttonStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 17,
                    fontStyle =
                        FontStyle.Bold
                };
        }
    }
}
