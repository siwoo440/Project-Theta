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
    /// <summary>
    /// 설정 창이다 (31일차). 메인 메뉴 · 허브 · 일시정지 메뉴가 같이 쓴다.
    ///
    ///   [일반]     음량 3종 · 화면 · 해상도 · 커서 크기 · 화면 흔들림 · 위기 화면 효과(34일차)
    ///   [키 설정]  조작마다 기본 · 보조 키 (32일차)
    ///
    /// 값은 바꾸는 즉시 적용하고, 창을 닫을 때 저장한다.
    /// 키 칸을 누르면 다음에 누른 키 · 마우스 버튼이 들어간다. Esc는 취소다.
    /// 다른 조작이 이미 쓰는 키면 경고만 띄우고 저장하지 않는다.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        private const float WindowWidth = 1000f;
        private const float WindowHeight = 860f;
        private const float LabelX = 44f;
        private const float ControlX = 300f;
        private const float RowGap = 64f;

        private const float KeyLabelX = 70f;
        private const float KeySlotX = 420f;
        private const float KeySlotWidth = 210f;
        private const float KeySlotGap = 20f;
        private const float KeyRowHeight = 38f;
        private const float ResetConfirmSeconds = 3f;

        private sealed class KeyRow
        {
            public GameAction Action;
            public UiButton[] Slots;
            public UiButton ClearButton;
        }

        /// <summary>키 입력을 기다리는 동안 Esc 순서 맨 위에 올려, Esc가 창을 닫지 않고 취소가 되게 한다.</summary>
        private sealed class ListenToken
        {
        }

        private UiOverlayParts _parts;
        private RectTransform _generalPage;
        private RectTransform _keysPage;
        private UiButton _generalTab;
        private UiButton _keysTab;

        private Slider _master;
        private Slider _sfx;
        private Slider _music;
        private Text _masterValue;
        private Text _sfxValue;
        private Text _musicValue;
        private UiButton _fullscreen;
        private UiButton _windowed;
        private readonly UiButton[] _resolutions = new UiButton[SettingsLogic.Resolutions.GetLength(0)];
        private readonly UiButton[] _cursors = new UiButton[3];
        private UiButton _shakeOn;
        private UiButton _shakeOff;
        private UiButton _dangerOn;
        private UiButton _dangerOff;
        private bool _filling;
        private float _lastPreview;

        private readonly KeyRow[] _keyRows = new KeyRow[InputBindingLogic.ActionCount];
        private Text _keyMessage;
        private UiButton _resetKeys;
        private float _resetConfirm;
        private readonly ListenToken _listenToken = new ListenToken();
        private bool _listening;
        private GameAction _listenAction;
        private int _listenSlot;
        private int _listenStartFrame;

        /// <summary>창이 닫힐 때 알린다. 허브가 화면 흔들림 버튼을 다시 그리는 데 쓴다.</summary>
        public event Action Closed;

        public bool IsOpen =>
            _parts.Root != null &&
            _parts.Root.activeSelf;

        private static SaveData CurrentSave =>
            GameSession.Instance == null
                ? null
                : GameSession.Instance.Save;

        public static SettingsPanel Create(
            Transform canvas,
            int sortingOrder)
        {
            GameObject host = new GameObject("SettingsPanel");
            host.transform.SetParent(canvas, false);

            SettingsPanel panel = host.AddComponent<SettingsPanel>();
            panel.Build(canvas, sortingOrder);

            return panel;
        }

        // 조립 ----------------------------------------------------------

        private void Build(
            Transform canvas,
            int sortingOrder)
        {
            _parts =
                UiOverlay.Create(
                    canvas,
                    "SettingsOverlay",
                    sortingOrder,
                    new Vector2(WindowWidth, WindowHeight),
                    "설정",
                    Close);

            RectTransform w = _parts.Window;

            _generalTab = UiOverlay.Button(w, "GeneralTab", "일반", 170f, 26f, 140f, 44f, () => ShowPage(false));
            _keysTab = UiOverlay.Button(w, "KeysTab", "키 설정", 320f, 26f, 140f, 44f, () => ShowPage(true));

            _generalPage = CreatePage(w, "GeneralPage");
            _keysPage = CreatePage(w, "KeysPage");

            BuildGeneral(_generalPage);
            BuildKeys(_keysPage);

            ShowPage(false);
        }

        private static RectTransform CreatePage(
            RectTransform window,
            string name)
        {
            RectTransform page = UiFactory.CreateRect(window, name);
            UiFactory.Stretch(page);

            return page;
        }

        private void BuildGeneral(
            RectTransform w)
        {
            float top = 110f;

            _master = VolumeRow(w, "전체 음량", top, out _masterValue);
            top += RowGap;
            _sfx = VolumeRow(w, "효과음", top, out _sfxValue);
            top += RowGap;
            _music = VolumeRow(w, "음악", top, out _musicValue);

            // 37일차: 배경음악이 생겨 "준비 중" 문구를 뺐다. 임시 음원이다.
            UiOverlay.Label(w, "(임시 음원 · 정식 음악으로 교체 예정)", ControlX, top + 30f, 400f, UiTheme.FontTiny, UiTheme.TextDisabled);
            top += RowGap + 12f;

            UiOverlay.Label(w, "화면", LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);
            _fullscreen = UiOverlay.Button(w, "Fullscreen", "전체화면", ControlX, top, 180f, 40f, () => SetFullscreen(true));
            _windowed = UiOverlay.Button(w, "Windowed", "창 모드", ControlX + 190f, top, 180f, 40f, () => SetFullscreen(false));
            top += RowGap;

            UiOverlay.Label(w, "해상도", LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);

            for (int i = 0; i < _resolutions.Length; i++)
            {
                int index = i;
                _resolutions[i] =
                    UiOverlay.Button(w, $"Resolution_{i}", SettingsLogic.GetResolutionLabel(i), ControlX + i * 190f, top, 180f, 40f, () => SetResolution(index));
            }

            top += RowGap;

            UiOverlay.Label(w, "커서 크기", LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);

            for (int i = 0; i < _cursors.Length; i++)
            {
                int index = i;
                _cursors[i] =
                    UiOverlay.Button(w, $"Cursor_{i}", SettingsLogic.CursorLabels[i], ControlX + i * 190f, top, 180f, 40f, () => SetCursor(index));
            }

            top += RowGap;

            UiOverlay.Label(w, "화면 흔들림", LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);
            _shakeOn = UiOverlay.Button(w, "ShakeOn", "켜짐", ControlX, top, 180f, 40f, () => SetShake(true));
            _shakeOff = UiOverlay.Button(w, "ShakeOff", "꺼짐", ControlX + 190f, top, 180f, 40f, () => SetShake(false));
            top += RowGap;

            // 34일차: 체력 · 빼앗김 직전 · 회수 잠김 · 남은 시간 때 화면 가장자리가 붉게 물든다.
            UiOverlay.Label(w, "위기 화면 효과", LabelX, top, 240f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);
            _dangerOn = UiOverlay.Button(w, "DangerOn", "켜짐", ControlX, top, 180f, 40f, () => SetDanger(true));
            _dangerOff = UiOverlay.Button(w, "DangerOff", "꺼짐", ControlX + 190f, top, 180f, 40f, () => SetDanger(false));
            top += RowGap;

            // 38일차: 실행 파일에서 저장 파일 위치를 찾기 쉽게 한다.
            UiOverlay.Label(w, "저장 폴더", LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);
            UiOverlay.Button(w, "OpenSaveFolder", "폴더 열기", ControlX, top, 180f, 40f, OpenSaveFolder);
            UiOverlay.Label(w, SettingsLogic.GetFolderLabel(SaveSystem.Folder), ControlX + 190f, top + 2f, 480f, UiTheme.FontTiny, UiTheme.TextDisabled);

            Text note =
                UiFactory.CreateText(
                    w,
                    "Note",
                    "해상도 · 화면 모드는 실행 파일에서만 바뀝니다. 설정은 창을 닫을 때 저장됩니다.",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(note.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(LabelX, 22f), new Vector2(900f, 28f));
        }

        private static void OpenSaveFolder()
        {
            GameAudio.Play(GameSfx.UiTick);
            Application.OpenURL(SettingsLogic.GetFolderUrl(SaveSystem.Folder));
        }

        private Slider VolumeRow(
            RectTransform w,
            string label,
            float top,
            out Text value)
        {
            UiOverlay.Label(w, label, LabelX, top, 220f, UiTheme.FontSubheading, UiTheme.TextPrimary, true);

            Slider slider =
                UiFactory.CreateSlider(
                    w,
                    label,
                    UiTheme.Accent);

            UiFactory.Place((RectTransform)slider.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(ControlX, -top - 8f), new Vector2(460f, 22f));

            slider.minValue = 0f;
            slider.maxValue = 1f;

            value = UiOverlay.Label(w, "-", ControlX + 480f, top, 120f, UiTheme.FontBody, UiTheme.TextPrimary, true);

            slider.onValueChanged.AddListener(_ => OnVolumeChanged());

            return slider;
        }

        private void BuildKeys(
            RectTransform w)
        {
            float top = 96f;

            UiOverlay.Label(w, "기본 키", KeySlotX, top, KeySlotWidth, UiTheme.FontSmall, UiTheme.TextMuted, true, TextAnchor.MiddleCenter);
            UiOverlay.Label(w, "보조 키", KeySlotX + KeySlotWidth + KeySlotGap, top, KeySlotWidth, UiTheme.FontSmall, UiTheme.TextMuted, true, TextAnchor.MiddleCenter);
            top += 34f;

            string lastGroup = null;

            for (int i = 0; i < InputBindingLogic.ActionCount; i++)
            {
                GameAction action = (GameAction)i;
                string group = InputBindingLogic.GetGroup(action);

                if (group != lastGroup)
                {
                    lastGroup = group;
                    UiOverlay.Label(w, group, LabelX, top, 200f, UiTheme.FontSmall, UiTheme.Gold, true);
                    top += 30f;
                }

                _keyRows[i] = BuildKeyRow(w, action, top);
                top += KeyRowHeight;
            }

            _keyMessage =
                UiFactory.CreateText(
                    w,
                    "KeyMessage",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            _keyMessage.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiFactory.Place(_keyMessage.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(LabelX, 70f), new Vector2(620f, 56f));

            _resetKeys =
                UiFactory.CreateButton(
                    w,
                    "ResetKeys",
                    "기본값으로 되돌리기",
                    UiTheme.FontBody);

            UiFactory.Place(_resetKeys.Background.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 76f), new Vector2(260f, 46f));
            _resetKeys.Button.onClick.AddListener(ResetKeys);

            Text note =
                UiFactory.CreateText(
                    w,
                    "KeyNote",
                    "칸을 누른 뒤 키나 마우스 버튼을 누르세요. Esc는 취소. 화면 조작(카드 숫자 · 지도 · Esc)은 고정입니다.",
                    UiTheme.FontSmall,
                    UiTheme.TextDisabled,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(note.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(LabelX, 22f), new Vector2(920f, 28f));
        }

        private KeyRow BuildKeyRow(
            RectTransform w,
            GameAction action,
            float top)
        {
            UiOverlay.Label(w, InputBindingLogic.GetLabel(action), KeyLabelX, top, 340f, UiTheme.FontBody, UiTheme.TextPrimary);

            KeyRow row =
                new KeyRow
                {
                    Action = action,
                    Slots = new UiButton[KeyBindingTable.SlotCount]
                };

            for (int s = 0; s < KeyBindingTable.SlotCount; s++)
            {
                int slot = s;

                row.Slots[s] =
                    UiOverlay.Button(
                        w,
                        $"Key_{action}_{s}",
                        "-",
                        KeySlotX + s * (KeySlotWidth + KeySlotGap),
                        top,
                        KeySlotWidth,
                        KeyRowHeight - 6f,
                        () => BeginListen(action, slot));
            }

            // 보조 칸만 비울 수 있다.
            row.ClearButton =
                UiOverlay.Button(
                    w,
                    $"Clear_{action}",
                    "×",
                    KeySlotX + 2f * (KeySlotWidth + KeySlotGap) - 12f,
                    top,
                    36f,
                    KeyRowHeight - 6f,
                    () => ClearSecondary(action));

            return row;
        }

        // 열기 · 닫기 ---------------------------------------------------

        public void Open()
        {
            if (_parts.Root == null)
            {
                return;
            }

            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);

            ShowPage(false);
            Fill();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            CancelListen(false);

            _parts.Root.SetActive(false);
            UiEscapeStack.Remove(this);

            GameSession.Instance?.WriteSave();

            Closed?.Invoke();
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(_listenToken);
            UiEscapeStack.Remove(this);
        }

        private void ShowPage(
            bool keys)
        {
            CancelListen(false);

            _generalPage.gameObject.SetActive(!keys);
            _keysPage.gameObject.SetActive(keys);

            MarkTab(_generalTab, !keys);
            MarkTab(_keysTab, keys);

            if (keys)
            {
                _keyMessage.text = string.Empty;
                RefreshKeys();
            }
        }

        private static void MarkTab(
            UiButton tab,
            bool selected)
        {
            Mark(tab, selected);
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (_resetConfirm > 0f)
            {
                _resetConfirm -= Time.unscaledDeltaTime;

                if (_resetConfirm <= 0f)
                {
                    _resetKeys.SetText("기본값으로 되돌리기");
                }
            }

            if (_listening)
            {
                UpdateListen();

                return;
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

        // 일반 ----------------------------------------------------------

        private void Fill()
        {
            SaveData save = CurrentSave;

            if (save == null)
            {
                return;
            }

            SettingsLogic.Normalize(save);

            _filling = true;
            _master.value = save.MasterVolume;
            _sfx.value = save.SfxVolume;
            _music.value = save.MusicVolume;
            _filling = false;

            RefreshLabels(save);
            RefreshKeys();
        }

        private void RefreshLabels(
            SaveData save)
        {
            _masterValue.text = SettingsLogic.FormatVolume(save.MasterVolume);
            _sfxValue.text = SettingsLogic.FormatVolume(save.SfxVolume);
            _musicValue.text = SettingsLogic.FormatVolume(save.MusicVolume);

            Mark(_fullscreen, save.Fullscreen);
            Mark(_windowed, !save.Fullscreen);

            for (int i = 0; i < _resolutions.Length; i++)
            {
                Mark(_resolutions[i], save.ResolutionIndex == i);
            }

            for (int i = 0; i < _cursors.Length; i++)
            {
                Mark(_cursors[i], save.CursorSize == i);
            }

            Mark(_shakeOn, !save.ScreenShakeDisabled);
            Mark(_shakeOff, save.ScreenShakeDisabled);
            Mark(_dangerOn, !save.DangerEffectDisabled);
            Mark(_dangerOff, save.DangerEffectDisabled);
        }

        /// <summary>고른 버튼은 보라, 나머지는 기본색이다.</summary>
        private static void Mark(
            UiButton button,
            bool selected)
        {
            if (button.Background == null)
            {
                return;
            }

            button.Background.color =
                selected
                    ? UiTheme.PrimaryButtonNormal
                    : UiTheme.ButtonNormal;

            if (button.Label != null)
            {
                button.Label.color =
                    selected
                        ? UiTheme.TextPrimary
                        : UiTheme.TextMuted;
            }
        }

        private void OnVolumeChanged()
        {
            SaveData save = CurrentSave;

            if (_filling ||
                save == null)
            {
                return;
            }

            save.MasterVolume = SettingsLogic.ClampVolume(_master.value);
            save.SfxVolume = SettingsLogic.ClampVolume(_sfx.value);
            save.MusicVolume = SettingsLogic.ClampVolume(_music.value);

            ApplyAndRefresh(save);

            // 끌 때마다 울리면 시끄럽다. 0.12초에 한 번만 들려 준다.
            if (Time.unscaledTime - _lastPreview > 0.12f)
            {
                _lastPreview = Time.unscaledTime;
                GameAudio.Play(GameSfx.UiTick);
            }
        }

        private void SetFullscreen(
            bool value)
        {
            Change(save => save.Fullscreen = value);
        }

        private void SetResolution(
            int index)
        {
            Change(save => save.ResolutionIndex = SettingsLogic.ClampResolutionIndex(index));
        }

        private void SetCursor(
            int index)
        {
            Change(save => save.CursorSize = (int)SettingsLogic.ClampCursor(index));
        }

        private void SetShake(
            bool enabled)
        {
            Change(save => save.ScreenShakeDisabled = !enabled);
        }

        private void SetDanger(
            bool enabled)
        {
            Change(save => save.DangerEffectDisabled = !enabled);
        }

        private void Change(
            Action<SaveData> apply)
        {
            SaveData save = CurrentSave;

            if (save == null)
            {
                return;
            }

            apply(save);
            ApplyAndRefresh(save);

            GameAudio.Play(GameSfx.UiTick);
        }

        private void ApplyAndRefresh(
            SaveData save)
        {
            save.SettingsInitialized = true;
            SettingsApplier.Apply(save);
            RefreshLabels(save);
        }

        // 키 설정 -------------------------------------------------------

        private void RefreshKeys()
        {
            KeyBindingTable table = GameInput.Table;

            foreach (KeyRow row in _keyRows)
            {
                if (row == null)
                {
                    continue;
                }

                for (int s = 0; s < row.Slots.Length; s++)
                {
                    bool waiting =
                        _listening &&
                        _listenAction == row.Action &&
                        _listenSlot == s;

                    row.Slots[s].SetText(
                        waiting
                            ? "키를 누르세요…"
                            : InputBindingLogic.GetDisplayName(table.Get(row.Action, s)));

                    Mark(row.Slots[s], waiting);
                }

                row.ClearButton.Button.gameObject.SetActive(
                    !string.IsNullOrEmpty(table.Get(row.Action, 1)));
            }
        }

        private void BeginListen(
            GameAction action,
            int slot)
        {
            _listening = true;
            _listenAction = action;
            _listenSlot = slot;

            // 칸을 누른 그 입력이 바로 들어가지 않게 이 프레임은 건너뛴다.
            _listenStartFrame = Time.frameCount;

            UiEscapeStack.Push(_listenToken);

            ShowMessage($"'{InputBindingLogic.GetLabel(action)}'에 쓸 키를 누르세요. (Esc: 취소)", UiTheme.TextMuted);
            GameAudio.Play(GameSfx.UiTick);
            RefreshKeys();
        }

        private void UpdateListen()
        {
            if (Time.frameCount <= _listenStartFrame)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame)
            {
                UiEscapeStack.TryConsume(_listenToken, Time.frameCount);
                CancelListen(true);

                return;
            }
#endif

            if (!GameInput.TryReadAnyPress(out string code))
            {
                return;
            }

            GameAction action = _listenAction;
            int slot = _listenSlot;

            CancelListen(false);
            ApplyBinding(action, slot, code);
        }

        private void CancelListen(
            bool showMessage)
        {
            if (!_listening)
            {
                return;
            }

            _listening = false;
            UiEscapeStack.Remove(_listenToken);

            if (showMessage)
            {
                ShowMessage("취소했습니다.", UiTheme.TextMuted);
            }

            RefreshKeys();
        }

        private void ApplyBinding(
            GameAction action,
            int slot,
            string code)
        {
            KeyBindingTable table = GameInput.Table.Clone();

            BindingResult result =
                InputBindingLogic.TrySet(
                    table,
                    action,
                    slot,
                    code,
                    out BindingSlot conflict);

            if (result == BindingResult.Ok)
            {
                Commit(table);
                ShowMessage($"'{InputBindingLogic.GetLabel(action)}' → {InputBindingLogic.GetDisplayName(code)}", UiTheme.Positive);
                GameAudio.Play(GameSfx.UiStamp);
            }
            else if (result == BindingResult.Unchanged)
            {
                ShowMessage("같은 키입니다.", UiTheme.TextMuted);
            }
            else
            {
                // 겹치거나 막힌 키는 경고만 하고 저장하지 않는다.
                ShowMessage(InputBindingLogic.DescribeResult(result, code, conflict), UiTheme.Danger);
            }

            RefreshKeys();
        }

        private void ClearSecondary(
            GameAction action)
        {
            CancelListen(false);

            KeyBindingTable table = GameInput.Table.Clone();

            if (InputBindingLogic.Clear(table, action, 1) == BindingResult.Ok)
            {
                Commit(table);
                ShowMessage($"'{InputBindingLogic.GetLabel(action)}' 보조 키를 비웠습니다.", UiTheme.TextMuted);
                GameAudio.Play(GameSfx.UiTick);
            }

            RefreshKeys();
        }

        /// <summary>실수로 누르지 않게 두 번 눌러야 되돌린다.</summary>
        private void ResetKeys()
        {
            CancelListen(false);

            if (_resetConfirm <= 0f)
            {
                _resetConfirm = ResetConfirmSeconds;
                _resetKeys.SetText("한 번 더 누르면 되돌립니다");
                GameAudio.Play(GameSfx.UiTick);

                return;
            }

            _resetConfirm = 0f;
            _resetKeys.SetText("기본값으로 되돌리기");

            Commit(InputBindingLogic.CreateDefault());
            ShowMessage("모든 키를 기본값으로 되돌렸습니다.", UiTheme.Positive);
            GameAudio.Play(GameSfx.UiStamp);
            RefreshKeys();
        }

        private static void Commit(
            KeyBindingTable table)
        {
            SaveData save = CurrentSave;

            GameInput.SetTable(table);

            if (save == null)
            {
                return;
            }

            save.SettingsInitialized = true;
            save.KeyBindings = InputBindingLogic.Serialize(table);
        }

        private void ShowMessage(
            string text,
            Color color)
        {
            _keyMessage.text = text;
            _keyMessage.color = color;
        }
    }
}
