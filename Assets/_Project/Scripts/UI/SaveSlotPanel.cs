using System;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>저장 칸 창을 여는 목적이다.</summary>
    public enum SaveSlotMode
    {
        /// <summary>이어하기: 저장된 칸만 고를 수 있다.</summary>
        Load,

        /// <summary>처음부터: 빈 칸은 바로, 기록이 있는 칸은 한 번 더 눌러 덮어쓴다.</summary>
        NewGame,

        /// <summary>허브 저장(37일차): 지금 칸 · 빈 칸은 바로, 다른 기록이 있는 칸은 한 번 더 눌러 덮어쓴다.</summary>
        Save
    }

    /// <summary>
    /// 메인 메뉴의 저장 칸 창이다 (34일차).
    ///
    ///   ┌ 1번 칸 ─────────────────────────────── [불러오기] ┐
    ///   │ 저장 2026-09-17 12:30 · 플레이 42분                │
    ///   │ 계약 정기 1,240 · 클리어 장소 3/8 · 업적 12        │
    ///   └────────────────────────────────────────────────────┘
    /// 칸을 고르면 <see cref="SlotChosen"/>으로 알리고, 메인 메뉴가 불러오기 · 새 게임 · 허브 이동을 한다.
    /// Esc · 닫기로 닫는다.
    /// </summary>
    public sealed class SaveSlotPanel : MonoBehaviour
    {
        private const float WindowWidth = 1100f;
        private const float WindowHeight = 640f;
        private const float CardTop = 120f;
        private const float CardHeight = 132f;
        private const float CardGap = 16f;

        private sealed class SlotView
        {
            public Image Background;
            public Text Title;
            public Text Detail;
            public UiButton Action;
        }

        private UiOverlayParts _parts;
        private Text _heading;
        private Text _hint;
        private readonly SlotView[] _views = new SlotView[SaveSlotLogic.SlotCount];

        private SaveSlotSummary[] _summaries = new SaveSlotSummary[SaveSlotLogic.SlotCount];
        private SaveSlotMode _mode;
        private int _activeSlot = -1;
        private int _confirmSlot = -1;
        private float _confirmRemaining;

        /// <summary>칸을 골랐다. (모드, 칸 번호)</summary>
        public event Action<SaveSlotMode, int> SlotChosen;

        public bool IsOpen =>
            _parts.Root != null &&
            _parts.Root.activeSelf;

        public static SaveSlotPanel Create(
            Transform canvas,
            int sortingOrder)
        {
            GameObject host = new GameObject("SaveSlotPanel");
            host.transform.SetParent(canvas, false);

            SaveSlotPanel panel = host.AddComponent<SaveSlotPanel>();
            panel.Build(canvas, sortingOrder);

            return panel;
        }

        private void Build(
            Transform canvas,
            int sortingOrder)
        {
            _parts =
                UiOverlay.Create(
                    canvas,
                    "SaveSlotOverlay",
                    sortingOrder,
                    new Vector2(WindowWidth, WindowHeight),
                    "저장 칸",
                    Close);

            RectTransform w = _parts.Window;

            // 제목은 모드마다 바꾼다. UiOverlay가 만든 제목 글자를 찾아 쓴다.
            Transform title = w.Find("Title");
            _heading = title == null ? null : title.GetComponent<Text>();

            _hint = UiOverlay.Label(w, string.Empty, 40f, 84f, WindowWidth - 80f, UiTheme.FontSmall, UiTheme.TextMuted);

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i] = BuildSlot(w, i, CardTop + i * (CardHeight + CardGap));
            }
        }

        private SlotView BuildSlot(
            RectTransform w,
            int slot,
            float top)
        {
            Image background =
                UiFactory.CreateImage(
                    w,
                    $"Slot_{slot}",
                    UiTheme.RowFill);

            UiFactory.Place(background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -top), new Vector2(WindowWidth - 80f, CardHeight));

            Text title =
                UiFactory.CreateText(
                    background.transform,
                    "Title",
                    SaveSlotLogic.GetSlotLabel(slot),
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -12f), new Vector2(300f, 36f));

            Text detail =
                UiFactory.CreateText(
                    background.transform,
                    "Detail",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.UpperLeft);

            UiFactory.Place(detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -52f), new Vector2(720f, 72f));

            UiButton action =
                UiFactory.CreateButton(
                    background.transform,
                    "Action",
                    "-",
                    UiTheme.FontBody,
                    true);

            UiFactory.Place(action.Background.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(260f, 56f));

            action.Button.onClick.AddListener(() => Choose(slot));

            return new SlotView
            {
                Background = background,
                Title = title,
                Detail = detail,
                Action = action
            };
        }

        // 열기 · 닫기 ---------------------------------------------------

        public void Open(
            SaveSlotMode mode,
            SaveSlotSummary[] summaries,
            int activeSlot = -1)
        {
            if (_parts.Root == null)
            {
                return;
            }

            _mode = mode;
            _activeSlot = activeSlot;
            _summaries = summaries ?? new SaveSlotSummary[SaveSlotLogic.SlotCount];
            _confirmSlot = -1;
            _confirmRemaining = 0f;

            if (_heading != null)
            {
                _heading.text = GetHeading(mode);
            }

            _hint.text = GetHint(mode);

            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);

            Fill();
        }

        private static string GetHeading(
            SaveSlotMode mode)
        {
            switch (mode)
            {
                case SaveSlotMode.Load:
                    return "이어하기 · 불러올 칸";

                case SaveSlotMode.Save:
                    return "저장 · 저장할 칸";

                default:
                    return "처음부터 · 새로 시작할 칸";
            }
        }

        private static string GetHint(
            SaveSlotMode mode)
        {
            switch (mode)
            {
                case SaveSlotMode.Load:
                    return "저장된 칸을 골라 이어서 플레이합니다. 장소를 마칠 때와 허브의 [저장] 버튼으로 저장됩니다.";

                case SaveSlotMode.Save:
                    return "다른 칸에 저장하면 그 칸으로 이어서 플레이합니다(이후 자동 저장도 그 칸). 기록이 있는 칸은 한 번 더 눌러야 덮어씁니다.";

                default:
                    return "기록이 있는 칸을 고르면 진행 · 업적 · 통계가 지워집니다. 설정 · 키는 그대로 남습니다.";
            }
        }

        public void Close()
        {
            if (_parts.Root != null)
            {
                _parts.Root.SetActive(false);
            }

            _confirmSlot = -1;
            UiEscapeStack.Remove(this);
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);
        }

        private void Fill()
        {
            int total =
                ProjectTheta.Stage.Locations.LocationCatalog.All.Count;

            int latest =
                SaveSlotLogic.FindLatest(
                    _summaries);

            for (int i = 0; i < _views.Length; i++)
            {
                SlotView view = _views[i];
                SaveSlotSummary summary = i < _summaries.Length ? _summaries[i] : new SaveSlotSummary { Slot = i };
                bool confirming = _confirmSlot == i;

                bool isActive = _mode == SaveSlotMode.Save && i == _activeSlot;

                view.Title.text =
                    isActive
                        ? $"{SaveSlotLogic.GetSlotLabel(i)}   <color=#9FD3A8>지금 칸</color>"
                        : i == latest
                            ? $"{SaveSlotLogic.GetSlotLabel(i)}   <color=#E8C15A>최근</color>"
                            : SaveSlotLogic.GetSlotLabel(i);

                view.Detail.text = SaveSlotLogic.Describe(summary, total);
                view.Detail.color = summary.Exists ? UiTheme.TextPrimary : UiTheme.TextDisabled;

                if (_mode == SaveSlotMode.Load)
                {
                    view.Action.SetText(summary.Exists ? "불러오기" : "비어 있음");
                    view.Action.SetInteractable(summary.Exists);
                }
                else if (_mode == SaveSlotMode.Save)
                {
                    view.Action.SetText(SaveSlotLogic.GetSaveButton(summary, isActive, confirming));
                    view.Action.SetInteractable(true);
                }
                else
                {
                    view.Action.SetText(SaveSlotLogic.GetNewGameButton(summary, confirming));
                    view.Action.SetInteractable(true);
                }

                view.Background.color =
                    confirming
                        ? new Color(0.34f, 0.10f, 0.12f, 0.95f)
                        : UiTheme.RowFill;
            }
        }

        private void Choose(
            int slot)
        {
            if (!SaveSlotLogic.IsValid(slot) ||
                slot >= _summaries.Length)
            {
                return;
            }

            SaveSlotSummary summary = _summaries[slot];

            if (_mode == SaveSlotMode.Load &&
                !summary.Exists)
            {
                return;
            }

            bool needsConfirm =
                (_mode == SaveSlotMode.NewGame &&
                 !SaveSlotLogic.CanStartNewGame(summary, _confirmSlot == slot)) ||
                (_mode == SaveSlotMode.Save &&
                 !SaveSlotLogic.CanSave(summary, slot == _activeSlot, _confirmSlot == slot));

            if (needsConfirm)
            {
                // 실수로 지우지 않게 한 번 더 눌러야 덮어쓴다.
                _confirmSlot = slot;
                _confirmRemaining = SaveSlotLogic.OverwriteConfirmSeconds;
                GameAudio.Play(GameSfx.UiTick);
                Fill();

                return;
            }

            if (_mode != SaveSlotMode.Save)
            {
                GameAudio.Play(GameSfx.UiStamp);
            }

            Close();

            SlotChosen?.Invoke(_mode, slot);
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (_confirmSlot >= 0)
            {
                _confirmRemaining -= Time.unscaledDeltaTime;

                if (_confirmRemaining <= 0f)
                {
                    _confirmSlot = -1;
                    Fill();
                }
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                Close();
            }
#endif
        }
    }
}
