using System;

namespace ProjectTheta.Save
{
    /// <summary>커서 크기 선택지다.</summary>
    public enum CursorSize
    {
        Small = 0,
        Normal = 1,
        Large = 2
    }

    /// <summary>
    /// 게임 설정 규칙이다 (31일차). 음량 · 화면 · 커서 크기의 기본값 · 범위 · 계산을 정한다.
    /// Unity 없이 도는 순수 계산이고, 실제 적용은 <see cref="Core.SettingsApplier"/>가 한다.
    /// </summary>
    public static class SettingsLogic
    {
        public const float DefaultVolume = 0.8f;

        public const int DefaultResolutionIndex = 2;

        /// <summary>해상도 선택지다. 기본 세 가지만 둔다.</summary>
        public static readonly int[,] Resolutions =
        {
            { 1280, 720 },
            { 1600, 900 },
            { 1920, 1080 }
        };

        public static int ResolutionCount =>
            Resolutions.GetLength(0);

        /// <summary>커서 크기 배율이다. 보통이 1배다.</summary>
        public static readonly float[] CursorScales = { 0.75f, 1f, 1.35f };

        public static readonly string[] CursorLabels = { "작게", "보통", "크게" };

        public static float ClampVolume(
            float value)
        {
            if (float.IsNaN(value))
            {
                return DefaultVolume;
            }

            return Math.Max(0f, Math.Min(1f, value));
        }

        /// <summary>실제로 쓰는 효과음 크기다. 전체 × 효과음.</summary>
        public static float GetEffectiveSfx(
            SaveData save)
        {
            return save == null
                ? DefaultVolume * DefaultVolume
                : ClampVolume(save.MasterVolume) * ClampVolume(save.SfxVolume);
        }

        /// <summary>실제로 쓰는 음악 크기다. 전체 × 음악.</summary>
        public static float GetEffectiveMusic(
            SaveData save)
        {
            return save == null
                ? DefaultVolume * DefaultVolume
                : ClampVolume(save.MasterVolume) * ClampVolume(save.MusicVolume);
        }

        public static int ClampResolutionIndex(
            int index)
        {
            return index < 0 || index >= ResolutionCount
                ? DefaultResolutionIndex
                : index;
        }

        public static int GetWidth(
            int index)
        {
            return Resolutions[ClampResolutionIndex(index), 0];
        }

        public static int GetHeight(
            int index)
        {
            return Resolutions[ClampResolutionIndex(index), 1];
        }

        public static string GetResolutionLabel(
            int index)
        {
            return $"{GetWidth(index)}×{GetHeight(index)}";
        }

        public static CursorSize ClampCursor(
            int value)
        {
            return value < (int)CursorSize.Small || value > (int)CursorSize.Large
                ? CursorSize.Normal
                : (CursorSize)value;
        }

        public static float GetCursorScale(
            int value)
        {
            return CursorScales[(int)ClampCursor(value)];
        }

        /// <summary>"80%"다.</summary>
        public static string FormatVolume(
            float value)
        {
            return $"{(int)Math.Round(ClampVolume(value) * 100f)}%";
        }

        /// <summary>새 세이브의 설정 기본값을 넣는다.</summary>
        public static void ApplyDefaults(
            SaveData save)
        {
            if (save == null)
            {
                return;
            }

            save.SettingsInitialized = true;
            save.MasterVolume = DefaultVolume;
            save.SfxVolume = DefaultVolume;
            save.MusicVolume = DefaultVolume;
            save.Fullscreen = true;
            save.ResolutionIndex = DefaultResolutionIndex;
            save.CursorSize = (int)CursorSize.Normal;
        }

        /// <summary>
        /// 설정을 안전한 상태로 맞춘다.
        /// 예전 세이브에는 설정 칸이 없어 음량이 0(무음)으로 읽히므로, 설정을 저장한 적이 없으면 기본값을 넣는다.
        /// </summary>
        public static void Normalize(
            SaveData save)
        {
            if (save == null)
            {
                return;
            }

            if (!save.SettingsInitialized)
            {
                ApplyDefaults(save);

                return;
            }

            save.MasterVolume = ClampVolume(save.MasterVolume);
            save.SfxVolume = ClampVolume(save.SfxVolume);
            save.MusicVolume = ClampVolume(save.MusicVolume);
            save.ResolutionIndex = ClampResolutionIndex(save.ResolutionIndex);
            save.CursorSize = (int)ClampCursor(save.CursorSize);
        }
    }
}
