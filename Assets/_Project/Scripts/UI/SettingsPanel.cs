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
    ///   전체 음량 / 효과음 / 음악        슬라이더 (바꾸면 바로 들림)
    ///   화면       전체화면 · 창 모드     해상도 1280×720 · 1600×900 · 1920×1080
    ///   커서 크기  작게 · 보통 · 크게
    ///   화면 흔들림  켜짐 · 꺼짐
    ///
    /// 값은 바꾸는 즉시 적용하고, 창을 닫을 때 저장한다.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        private const float LabelX = 44f;
        private const float ControlX = 300f;
        private const float RowGap = 64f;

        private UiOverlayParts _parts;
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
        private bool _filling;
        private float _lastPreview;

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

        private void Build(
            Transform canvas,
            int sortingOrder)
        {
            _parts =
                UiOverlay.Create(
                    canvas,
                    "SettingsOverlay",
                    sortingOrder,
                    new Vector2(1000f, 680f),
                    "설정",
                    Close);

            RectTransform w = _parts.Window;
            float top = 110f;

            _master = VolumeRow(w, "전체 음량", top, out _masterValue);
            top += RowGap;
            _sfx = VolumeRow(w, "효과음", top, out _sfxValue);
            top += RowGap;
            _music = VolumeRow(w, "음악", top, out _musicValue);

            UiOverlay.Label(w, "(배경음악은 준비 중)", ControlX, top + 30f, 400f, UiTheme.FontTiny, UiTheme.TextDisabled);
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

        public void Open()
        {
            if (_parts.Root == null)
            {
                return;
            }

            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);
            Fill();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            _parts.Root.SetActive(false);
            UiEscapeStack.Remove(this);

            GameSession.Instance?.WriteSave();

            Closed?.Invoke();
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);
        }

        private void Update()
        {
            if (!IsOpen)
            {
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
    }
}
