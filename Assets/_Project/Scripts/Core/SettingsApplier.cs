using System;
using UnityEngine;
using ProjectTheta.Presentation;
using ProjectTheta.Save;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 세이브의 설정을 실제 게임에 적용한다 (31일차).
    ///   음량      <see cref="GameAudio"/> 배율
    ///   화면      전체화면 · 해상도
    ///   커서      <see cref="CursorScale"/> (바뀌면 <see cref="Changed"/>로 커서가 다시 만든다)
    ///   흔들림    <see cref="CameraShake.Enabled"/>
    /// 게임을 켤 때 한 번, 설정 창에서 값을 바꿀 때마다 부른다.
    /// </summary>
    public static class SettingsApplier
    {
        private static int _appliedWidth = -1;
        private static int _appliedHeight = -1;
        private static bool _appliedFullscreen;

        /// <summary>지금 커서 크기 배율이다. 보통이 1이다.</summary>
        public static float CursorScale { get; private set; } = 1f;

        /// <summary>설정이 적용될 때마다 알린다.</summary>
        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _appliedWidth = -1;
            _appliedHeight = -1;
            CursorScale = 1f;
            Changed = null;
        }

        public static void Apply(
            SaveData save)
        {
            if (save == null)
            {
                return;
            }

            SettingsLogic.Normalize(save);

            GameAudio.SfxVolume = SettingsLogic.GetEffectiveSfx(save);
            GameAudio.MusicVolume = SettingsLogic.GetEffectiveMusic(save);

            CameraShake.Enabled = !save.ScreenShakeDisabled;

            CursorScale = SettingsLogic.GetCursorScale(save.CursorSize);

            // 32일차: 키 설정.
            GameInput.Load(save.KeyBindings);

            ApplyScreen(save);

            Changed?.Invoke();
        }

        private static void ApplyScreen(
            SaveData save)
        {
            int width = SettingsLogic.GetWidth(save.ResolutionIndex);
            int height = SettingsLogic.GetHeight(save.ResolutionIndex);

            // 에디터 Game 창은 해상도를 바꿀 수 없다. 같은 값이면 다시 부르지 않는다.
            if (Application.isEditor ||
                (width == _appliedWidth &&
                 height == _appliedHeight &&
                 save.Fullscreen == _appliedFullscreen))
            {
                return;
            }

            _appliedWidth = width;
            _appliedHeight = height;
            _appliedFullscreen = save.Fullscreen;

            Screen.SetResolution(
                width,
                height,
                save.Fullscreen
                    ? FullScreenMode.FullScreenWindow
                    : FullScreenMode.Windowed);
        }
    }
}
