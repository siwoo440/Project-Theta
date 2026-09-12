using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Duel
{
    /// <summary>
    /// 경쟁자와의 힘겨루기 UI다.
    ///
    /// 15일차에 IMGUI를 걷어내고 Canvas(uGUI)로 다시 만들었다.
    /// 게이지는 가운데에서 양쪽으로 밀리는 줄다리기 형태이고,
    /// 승·패 임계선을 흰 선으로 표시해 남은 거리를 눈으로 알 수 있게 했다.
    /// </summary>
    public sealed class OpponentDuelHud : MonoBehaviour
    {
        private OpponentDuelController _duel;

        private Canvas _canvas;
        private RectTransform _panel;
        private Text _titleText;
        private Text _expectedText;
        private Text _lossText;
        private UiBar _bar;
        private RectTransform _loseLine;
        private RectTransform _winLine;

        public void Configure(
            OpponentDuelController duel)
        {
            _duel =
                duel;
        }

        private void Start()
        {
            Build();

            SetVisible(
                false);
        }

        private void Update()
        {
            bool dueling =
                _duel != null &&
                _duel.IsDueling;

            SetVisible(
                dueling);

            if (!dueling)
            {
                return;
            }

            Refresh();
        }

        private void SetVisible(
            bool value)
        {
            if (_canvas == null ||
                _canvas.gameObject.activeSelf == value)
            {
                return;
            }

            _canvas.gameObject.SetActive(
                value);
        }

        private void Refresh()
        {
            _panel.anchoredPosition =
                new Vector2(
                    _duel.VisualJoltOffsetX,
                    _panel.anchoredPosition.y);

            _titleText.text =
                $"힘겨루기   VS {_duel.ActiveOpponentName}";

            float progress =
                _duel.ProgressNormalized;

            _bar.SetValue(
                progress);

            // 패배선에 가까우면 붉게, 승리선에 가까우면 금색으로 물든다.
            _bar.SetColor(
                progress <= _duel.OpponentWinThresholdNormalized + 0.12f
                    ? UiTheme.Danger
                    : progress >= _duel.PlayerWinThresholdNormalized - 0.12f
                        ? UiTheme.Gold
                        : UiTheme.Accent);

            _expectedText.text =
                $"다음 입력   {_duel.ExpectedInputLabel}";

            _lossText.text =
                $"상대 밀려난 횟수  {_duel.ActiveOpponentLossCount} / {_duel.ActiveOpponentMaximumDefeats}";

            PlaceLine(
                _loseLine,
                _duel.OpponentWinThresholdNormalized);

            PlaceLine(
                _winLine,
                _duel.PlayerWinThresholdNormalized);
        }

        private static void PlaceLine(
            RectTransform line,
            float normalized)
        {
            if (line == null)
            {
                return;
            }

            float clamped =
                Mathf.Clamp01(
                    normalized);

            line.anchorMin =
                new Vector2(clamped, 0f);

            line.anchorMax =
                new Vector2(clamped, 1f);
        }

        // 화면 조립 ------------------------------------------------------

        private void Build()
        {
            _canvas =
                UiFactory.CreateCanvas(
                    "DuelHudCanvas",
                    110,
                    transform);

            Image dim =
                UiFactory.CreateImage(
                    _canvas.transform,
                    "Dim",
                    new Color(0f, 0f, 0f, 0.34f));

            UiFactory.Stretch(
                dim.rectTransform);

            _panel =
                UiFactory.CreatePanel(
                    _canvas.transform,
                    "DuelPanel",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge,
                    3f);

            UiFactory.Place(
                _panel,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(760f, 330f));

            _titleText =
                UiFactory.CreateText(
                    _panel,
                    "Title",
                    string.Empty,
                    UiTheme.FontHeading + 2,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _titleText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -32f),
                new Vector2(720f, 36f));

            Text opponentSide =
                UiFactory.CreateText(
                    _panel,
                    "OpponentSide",
                    "상대",
                    UiTheme.FontBody,
                    UiTheme.Danger,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(
                opponentSide.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(46f, -76f),
                new Vector2(200f, 24f));

            Text playerSide =
                UiFactory.CreateText(
                    _panel,
                    "PlayerSide",
                    "PLAYER",
                    UiTheme.FontBody,
                    UiTheme.Gold,
                    TextAnchor.MiddleRight,
                    FontStyle.Bold);

            UiFactory.Place(
                playerSide.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-46f, -76f),
                new Vector2(200f, 24f));

            _bar =
                UiFactory.CreateBar(
                    _panel,
                    "DuelBar",
                    UiTheme.Accent,
                    UiTheme.TrackFill);

            UiFactory.Place(
                _bar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -112f),
                new Vector2(660f, 34f));

            _loseLine =
                CreateThresholdLine(
                    "LoseLine");

            _winLine =
                CreateThresholdLine(
                    "WinLine");

            Text thresholds =
                UiFactory.CreateText(
                    _panel,
                    "Thresholds",
                    "10% 이하 패배                    90% 이상 승리",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                thresholds.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -152f),
                new Vector2(720f, 22f));

            _expectedText =
                UiFactory.CreateText(
                    _panel,
                    "Expected",
                    string.Empty,
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                _expectedText.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -188f),
                new Vector2(720f, 28f));

            Text hint =
                UiFactory.CreateText(
                    _panel,
                    "Hint",
                    "좌클릭 ↔ 우클릭 빠르게 교대",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                hint.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -218f),
                new Vector2(720f, 22f));

            _lossText =
                UiFactory.CreateText(
                    _panel,
                    "LossText",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                _lossText.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 48f),
                new Vector2(720f, 24f));

            Text outcome =
                UiFactory.CreateText(
                    _panel,
                    "Outcome",
                    "승리 시 상대 기절 · 3회 승리 시 퇴장      패배 시 HP -10 + 기절",
                    UiTheme.FontTiny,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(
                outcome.rectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 22f),
                new Vector2(720f, 20f));
        }

        /// <summary>게이지 위에 겹쳐 그리는 승패 임계선이다.</summary>
        private RectTransform CreateThresholdLine(
            string name)
        {
            Image line =
                UiFactory.CreateImage(
                    _bar.Root,
                    name,
                    new Color(1f, 1f, 1f, 0.92f));

            RectTransform rect =
                line.rectTransform;

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 1f);

            rect.offsetMin = new Vector2(-1.5f, -6f);
            rect.offsetMax = new Vector2(1.5f, 6f);

            return rect;
        }
    }
}
