using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Balance;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>
    /// ② 수치 탭이다. 게임을 멈추지 않고 밸런스 수치를 슬라이더로 바꾼다 (20일차).
    ///
    /// 바뀐 값은 <see cref="BalanceTuningSession"/>이 복사본에 쓰므로 자산 원본은 그대로다.
    /// 재생을 멈추면 전부 자산 값으로 돌아간다. 마음에 드는 값은 "자산에 저장"으로 남긴다(에디터 전용).
    /// </summary>
    public sealed class DebugTuningTab : IDebugTab
    {
        private sealed class Row
        {
            public TuningParameter Parameter;
            public Slider Slider;
            public Text Label;
            public Text Value;
        }

        private readonly List<Row> _rows =
            new List<Row>();

        private Text _summaryText;
        private Text _messageText;
        private float _messageRemaining;

        public string Title => "수치";

        public void Build(
            RectTransform root)
        {
            const float labelWidth = 150f;
            const float sliderX = 156f;
            const float sliderWidth = 206f;
            const float valueX = 370f;
            float row = DebugUi.RowHeight;

            float y = 0f;

            IReadOnlyList<TuningParameter> all =
                BalanceTuningCatalog.All;

            TuningGroup? currentGroup = null;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                TuningParameter parameter = all[i];

                if (currentGroup != parameter.Group)
                {
                    if (currentGroup != null)
                    {
                        y += DebugUi.SectionGap - 4f;
                    }

                    currentGroup = parameter.Group;

                    TuningGroup group = parameter.Group;

                    DebugUi.Button(
                        root,
                        "되돌리기",
                        DebugUi.ContentWidth - 76f,
                        y + 2f,
                        76f,
                        row - 4f,
                        () => BalanceTuningSession.ResetGroup(group));

                    y = DebugUi.Section(
                        root,
                        BalanceTuningCatalog.GetGroupName(group),
                        y);
                }

                Row view =
                    new Row
                    {
                        Parameter = parameter
                    };

                view.Label = DebugUi.Label(root, parameter.Label, 8f, y, labelWidth, UiTheme.TextMuted);

                view.Slider =
                    UiFactory.CreateSlider(
                        root,
                        "Slider",
                        UiTheme.AccentSoft);

                DebugUi.PlaceTopLeft(
                    view.Slider.GetComponent<RectTransform>(),
                    sliderX,
                    y + 5f,
                    sliderWidth,
                    row - 10f);

                view.Slider.minValue = parameter.Minimum;
                view.Slider.maxValue = parameter.Maximum;
                view.Slider.wholeNumbers = parameter.IsInteger;

                view.Slider.SetValueWithoutNotify(
                    BalanceTuningSession.Get(parameter));

                view.Slider.onValueChanged.AddListener(
                    value => BalanceTuningSession.Set(parameter, value));

                view.Value = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary, UiTheme.FontSmall, TextAnchor.MiddleRight, true);

                _rows.Add(view);

                y += row;
            }

            y += DebugUi.SectionGap;

            Image line =
                UiFactory.CreateImage(
                    root,
                    "FooterLine",
                    UiTheme.PanelEdge);

            DebugUi.PlaceTopLeft(
                line.rectTransform,
                0f,
                y,
                DebugUi.ContentWidth,
                1f);

            y += 8f;

            _summaryText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextMuted);

            y += row + 4f;

            DebugUi.Button(
                root,
                "전부 되돌리기",
                0f,
                y,
                200f,
                36f,
                () =>
                {
                    BalanceTuningSession.ResetAll();
                    ShowMessage("자산 값으로 되돌렸습니다");
                });

#if UNITY_EDITOR
            DebugUi.Button(
                root,
                "자산에 저장",
                DebugUi.ContentWidth - 200f,
                y,
                200f,
                36f,
                SaveToAsset,
                true);
#endif

            y += 42f;

            _messageText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.Positive);
        }

        public void Refresh()
        {
            for (int i = 0;
                 i < _rows.Count;
                 i++)
            {
                Row view = _rows[i];

                float value =
                    BalanceTuningSession.Get(
                        view.Parameter);

                // 되돌리기로 값이 바뀌면 슬라이더도 따라가야 한다. 알림 없이 옮겨 되돌리기가 다시 편집으로 잡히지 않게 한다.
                if (!Mathf.Approximately(
                        view.Slider.value,
                        value))
                {
                    view.Slider.SetValueWithoutNotify(
                        value);
                }

                bool modified =
                    BalanceTuningSession.IsModified(
                        view.Parameter);

                view.Value.text =
                    modified
                        ? "• " + view.Parameter.Format(value)
                        : view.Parameter.Format(value);

                view.Value.color =
                    modified
                        ? UiTheme.Gold
                        : UiTheme.TextPrimary;

                view.Label.color =
                    modified
                        ? UiTheme.TextPrimary
                        : UiTheme.TextMuted;
            }

            int count =
                BalanceTuningSession.CountModified();

            _summaryText.text =
                count == 0
                    ? "자산 값 그대로입니다"
                    : $"바뀐 값 {count}개   (재생을 멈추면 자산 값으로 돌아갑니다)";

            _summaryText.color =
                count == 0
                    ? UiTheme.TextMuted
                    : UiTheme.Gold;

            if (_messageRemaining > 0f)
            {
                _messageRemaining -= 0.1f;

                if (_messageRemaining <= 0f)
                {
                    _messageText.text = string.Empty;
                }
            }
        }

        private void ShowMessage(
            string message)
        {
            _messageText.text = message;
            _messageRemaining = 3f;
        }

#if UNITY_EDITOR
        private void SaveToAsset()
        {
            StageBalanceDatabase database =
                Resources.Load<StageBalanceDatabase>(
                    BalanceBootstrap.StageBalancePath);

            if (database == null)
            {
                ShowMessage("StageBalanceDatabase 자산을 찾지 못했습니다");

                return;
            }

            int count =
                BalanceTuningSession.CountModified();

            StageBalanceValues saved =
                database.SaveStageValues(
                    BalanceOverrides.StageOrDefault);

            BalanceTuningSession.CommitAsBaseline(
                saved);

            ShowMessage(
                $"{count}개 값을 StageBalanceDatabase.asset에 저장했습니다");
        }
#endif
    }
}
