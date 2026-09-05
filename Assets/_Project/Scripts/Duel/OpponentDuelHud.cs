using UnityEngine;

namespace ProjectTheta.Duel
{
    public sealed class OpponentDuelHud : MonoBehaviour
    {
        private OpponentDuelController _duel;
        private GUIStyle _titleStyle;
        private GUIStyle _centerStyle;
        private GUIStyle _barStyle;

        public void Configure(
            OpponentDuelController duel)
        {
            _duel =
                duel;
        }

        private void OnGUI()
        {
            if (_duel == null ||
                !_duel.IsDueling)
            {
                return;
            }

            EnsureStyles();

            float width =
                Mathf.Min(
                    520f,
                    Screen.width -
                    40f);

            float height =
                230f;

            float x =
                (Screen.width -
                 width) *
                0.5f +
                _duel.VisualJoltOffsetX;

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
                    x + 20f,
                    y + 18f,
                    width - 40f,
                    34f),
                $"힘겨루기 - VS {_duel.ActiveOpponentName}",
                _titleStyle);

            GUI.Label(
                new Rect(
                    x + 32f,
                    y + 64f,
                    120f,
                    24f),
                "상대",
                _centerStyle);

            GUI.Label(
                new Rect(
                    x + width - 152f,
                    y + 64f,
                    120f,
                    24f),
                "PLAYER",
                _centerStyle);

            Rect track =
                new Rect(
                    x + 54f,
                    y + 94f,
                    width - 108f,
                    28f);

            GUI.Box(
                track,
                string.Empty);

            DrawThresholdLine(
                track,
                _duel.OpponentWinThresholdNormalized);

            DrawThresholdLine(
                track,
                _duel.PlayerWinThresholdNormalized);

            float fillWidth =
                track.width *
                _duel.ProgressNormalized;

            GUI.Box(
                new Rect(
                    track.x,
                    track.y,
                    fillWidth,
                    track.height),
                string.Empty,
                _barStyle);

            GUI.Label(
                new Rect(
                    x + 20f,
                    y + 124f,
                    width - 40f,
                    22f),
                "10% 이하: 패배                    90% 이상: 승리",
                _centerStyle);

            GUI.Label(
                new Rect(
                    x + 20f,
                    y + 146f,
                    width - 40f,
                    26f),
                $"다음 입력: {_duel.ExpectedInputLabel}  |  좌클릭 ↔ 우클릭 빠르게 교대",
                _centerStyle);

            GUI.Label(
                new Rect(
                    x + 20f,
                    y + 178f,
                    width - 40f,
                    24f),
                $"상대 밀려난 횟수: {_duel.ActiveOpponentLossCount} / {_duel.ActiveOpponentMaximumDefeats}",
                _centerStyle);

            GUI.Label(
                new Rect(
                    x + 20f,
                    y + 202f,
                    width - 40f,
                    22f),
                "승리: 상대 기절 / 3회 승리 시 상대 퇴장   |   패배: HP -10 + 플레이어 기절",
                _centerStyle);
        }

        private void DrawThresholdLine(
            Rect track,
            float normalized)
        {
            float x =
                track.x +
                track.width *
                Mathf.Clamp01(
                    normalized);

            Color previous =
                GUI.color;

            GUI.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.95f);

            GUI.DrawTexture(
                new Rect(
                    x - 1f,
                    track.y - 5f,
                    2f,
                    track.height + 10f),
                Texture2D.whiteTexture);

            GUI.color =
                previous;
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
                    fontSize =
                        22,
                    fontStyle =
                        FontStyle.Bold
                };

            _centerStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize =
                        14
                };

            _barStyle =
                new GUIStyle(
                    GUI.skin.box);
        }
    }
}
