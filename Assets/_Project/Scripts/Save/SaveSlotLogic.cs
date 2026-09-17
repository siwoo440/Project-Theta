using System;
using System.Collections.Generic;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 모든 저장 칸이 함께 쓰는 설정이다 (34일차).
    /// 처음부터 새로 시작해도 음량 · 화면 · 키 설정은 그대로 남아야 해서 칸 파일과 따로 저장한다.
    /// </summary>
    [Serializable]
    public sealed class SettingsData
    {
        public bool SettingsInitialized;
        public float MasterVolume = SettingsLogic.DefaultVolume;
        public float SfxVolume = SettingsLogic.DefaultVolume;
        public float MusicVolume = SettingsLogic.DefaultVolume;
        public bool Fullscreen = true;
        public int ResolutionIndex = SettingsLogic.DefaultResolutionIndex;
        public int CursorSize = (int)Save.CursorSize.Normal;
        public bool ScreenShakeDisabled;
        public bool DangerEffectDisabled;
        public string[] KeyBindings = new string[0];
    }

    /// <summary>저장 칸 하나의 요약이다. 메인 메뉴의 칸 목록이 보여 준다.</summary>
    public struct SaveSlotSummary
    {
        public int Slot;
        public bool Exists;
        public string SavedAt;
        public long SavedTicks;
        public int ContractEssence;
        public int ClearedLocations;
        public int Clears;
        public int Endings;
        public float PlaySeconds;
        public int Achievements;

        /// <summary>36일차: 도시 지배도(%)다.</summary>
        public int DominionPercent;
    }

    /// <summary>
    /// 저장 칸 규칙이다 (34일차).
    ///
    ///   칸 3개      projecttheta_slot1.json ~ slot3.json  (진행 · 업적 · 통계)
    ///   설정 파일   projecttheta_settings.json            (음량 · 화면 · 커서 · 흔들림 · 위기 효과 · 키)
    ///   옛 세이브   projecttheta_save.json                → 처음 한 번 1번 칸으로 옮긴다(원본은 남긴다)
    ///
    /// 이어하기는 저장된 칸을 불러오고, 처음부터는 고른 칸을 새 게임으로 덮어쓴다.
    /// 파일을 직접 다루지 않는 순수 로직이라 테스트할 수 있다. 파일 입출력은 <see cref="SaveSystem"/>이 한다.
    /// </summary>
    public static class SaveSlotLogic
    {
        public const int SlotCount = 3;
        public const string SettingsFileName = "projecttheta_settings.json";
        public const string LegacyFileName = "projecttheta_save.json";
        public const string TimeFormat = "yyyy-MM-dd HH:mm";

        /// <summary>기록이 있는 칸을 "처음부터"로 고를 때, 다시 눌러야 하는 시간(초)이다.</summary>
        public const float OverwriteConfirmSeconds = 3f;

        public static bool IsValid(
            int slot)
        {
            return slot >= 0 &&
                   slot < SlotCount;
        }

        public static string GetFileName(
            int slot)
        {
            return $"projecttheta_slot{slot + 1}.json";
        }

        public static string GetSlotLabel(
            int slot)
        {
            return $"{slot + 1}번 칸";
        }

        // 설정 ----------------------------------------------------------

        public static SettingsData ExtractSettings(
            SaveData save)
        {
            if (save == null)
            {
                return null;
            }

            return new SettingsData
            {
                SettingsInitialized = save.SettingsInitialized,
                MasterVolume = save.MasterVolume,
                SfxVolume = save.SfxVolume,
                MusicVolume = save.MusicVolume,
                Fullscreen = save.Fullscreen,
                ResolutionIndex = save.ResolutionIndex,
                CursorSize = save.CursorSize,
                ScreenShakeDisabled = save.ScreenShakeDisabled,
                DangerEffectDisabled = save.DangerEffectDisabled,
                KeyBindings = save.KeyBindings == null
                    ? new string[0]
                    : (string[])save.KeyBindings.Clone()
            };
        }

        /// <summary>설정 파일의 값을 칸 데이터에 덮어쓴다. 설정이 없으면 칸 데이터를 그대로 둔다.</summary>
        public static void ApplySettings(
            SettingsData settings,
            SaveData save)
        {
            if (settings == null ||
                save == null)
            {
                return;
            }

            save.SettingsInitialized = settings.SettingsInitialized;
            save.MasterVolume = settings.MasterVolume;
            save.SfxVolume = settings.SfxVolume;
            save.MusicVolume = settings.MusicVolume;
            save.Fullscreen = settings.Fullscreen;
            save.ResolutionIndex = settings.ResolutionIndex;
            save.CursorSize = settings.CursorSize;
            save.ScreenShakeDisabled = settings.ScreenShakeDisabled;
            save.DangerEffectDisabled = settings.DangerEffectDisabled;
            save.KeyBindings =
                settings.KeyBindings == null
                    ? new string[0]
                    : (string[])settings.KeyBindings.Clone();

            SettingsLogic.Normalize(save);
        }

        /// <summary>새 게임 데이터다. 진행 · 업적 · 튜토리얼은 처음 상태, 설정만 이어받는다.</summary>
        public static SaveData CreateNewGame(
            SettingsData settings)
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            ApplySettings(
                settings,
                data);

            return data;
        }

        // 저장 시각 · 요약 ---------------------------------------------

        public static void Stamp(
            SaveData save,
            DateTime now)
        {
            if (save == null)
            {
                return;
            }

            save.SavedAt = now.ToString(TimeFormat);
            save.SavedTicks = now.Ticks;
        }

        public static SaveSlotSummary Summarize(
            int slot,
            SaveData save)
        {
            SaveSlotSummary summary =
                new SaveSlotSummary
                {
                    Slot = slot,
                    Exists = save != null,
                    SavedAt = string.Empty
                };

            if (save == null)
            {
                return summary;
            }

            summary.SavedAt = save.SavedAt ?? string.Empty;
            summary.SavedTicks = save.SavedTicks;
            summary.ContractEssence = save.ContractEssence;
            summary.Clears = save.ClearCount;
            summary.Endings = save.Stats == null ? 0 : save.Stats.Endings;
            summary.PlaySeconds = save.Stats == null ? 0f : save.Stats.TotalSeconds;
            summary.Achievements = save.UnlockedAchievements == null ? 0 : save.UnlockedAchievements.Length;
            summary.DominionPercent = DominionLogic.GetPercent(save);

            if (save.LocationRecords != null)
            {
                foreach (LocationStats record in save.LocationRecords)
                {
                    if (record != null &&
                        record.Clears > 0)
                    {
                        summary.ClearedLocations++;
                    }
                }
            }

            return summary;
        }

        /// <summary>"12분" · "1시간 05분"이다.</summary>
        public static string FormatPlayTime(
            float seconds)
        {
            int minutes =
                float.IsNaN(seconds) ||
                seconds <= 0f
                    ? 0
                    : (int)(seconds / 60f);

            return minutes >= 60
                ? $"{minutes / 60}시간 {minutes % 60:00}분"
                : $"{minutes}분";
        }

        /// <summary>칸 목록 한 칸의 두 줄 설명이다.</summary>
        public static string Describe(
            SaveSlotSummary summary,
            int totalLocations)
        {
            if (!summary.Exists)
            {
                return "비어 있음";
            }

            string savedAt =
                string.IsNullOrEmpty(summary.SavedAt)
                    ? "저장 시각 없음"
                    : $"저장 {summary.SavedAt}";

            string ending =
                summary.Endings > 0
                    ? "  ·  엔딩 달성"
                    : string.Empty;

            return
                $"{savedAt}  ·  플레이 {FormatPlayTime(summary.PlaySeconds)}\n" +
                $"계약 정기 {summary.ContractEssence:N0}  ·  클리어 장소 {summary.ClearedLocations}/{Math.Max(0, totalLocations)}  ·  업적 {summary.Achievements}{ending}\n" +
                $"도시 지배도 {summary.DominionPercent}%  「{DominionLogic.GetTitle(summary.DominionPercent)}」";
        }

        /// <summary>가장 최근에 저장한 칸이다. 없으면 -1. 시각이 같으면 앞 칸이다.</summary>
        public static int FindLatest(
            IReadOnlyList<SaveSlotSummary> summaries)
        {
            int latest = -1;
            long latestTicks = long.MinValue;

            if (summaries == null)
            {
                return latest;
            }

            for (int i = 0; i < summaries.Count; i++)
            {
                if (!summaries[i].Exists ||
                    summaries[i].SavedTicks <= latestTicks)
                {
                    continue;
                }

                latest = summaries[i].Slot;
                latestTicks = summaries[i].SavedTicks;
            }

            return latest;
        }

        public static bool AnyExists(
            IReadOnlyList<SaveSlotSummary> summaries)
        {
            return FindLatest(summaries) >= 0;
        }

        /// <summary>
        /// 옛 세이브를 1번 칸으로 옮길지다.
        /// 설정 파일이나 칸 파일이 하나라도 있으면 이미 옮긴 것이므로 다시 하지 않는다.
        /// </summary>
        public static bool ShouldMigrate(
            bool legacyExists,
            bool settingsExists,
            bool anySlotExists)
        {
            return legacyExists &&
                   !settingsExists &&
                   !anySlotExists;
        }

        /// <summary>
        /// 장소 화면에서 바로 실행했을 때처럼 칸을 고르지 않고 들어온 경우 쓸 칸이다.
        /// 가장 최근 칸, 없으면 1번 칸이다.
        /// </summary>
        public static int GetFallbackSlot(
            IReadOnlyList<SaveSlotSummary> summaries)
        {
            int latest = FindLatest(summaries);

            return latest >= 0 ? latest : 0;
        }

        // 처음부터 ------------------------------------------------------

        // 허브 저장 (37일차) -------------------------------------------

        /// <summary>
        /// 허브 [저장] 창의 버튼 글자다.
        /// 지금 칸 · 빈 칸은 바로 저장, 다른 기록이 있는 칸은 한 번 더 눌러야 덮어쓴다.
        /// </summary>
        public static string GetSaveButton(
            SaveSlotSummary summary,
            bool isActive,
            bool confirming)
        {
            if (isActive)
            {
                return "여기에 저장";
            }

            if (!summary.Exists)
            {
                return "이 칸에 저장";
            }

            return confirming
                ? "한 번 더 누르면 덮어씀"
                : "덮어쓰기";
        }

        public static bool CanSave(
            SaveSlotSummary summary,
            bool isActive,
            bool confirming)
        {
            return isActive ||
                   !summary.Exists ||
                   confirming;
        }

        /// <summary>저장 결과 문구다. 다른 칸에 저장했으면 그 칸으로 이어서 플레이한다고 알린다.</summary>
        public static string GetSavedMessage(
            int slot,
            int previousSlot,
            string savedAt)
        {
            return slot == previousSlot
                ? $"{GetSlotLabel(slot)}에 저장했습니다  ·  {savedAt}"
                : $"{GetSlotLabel(slot)}에 저장했습니다 · 이제 {GetSlotLabel(slot)}으로 플레이  ·  {savedAt}";
        }

        /// <summary>처음부터를 고른 칸의 버튼 글자다.</summary>
        public static string GetNewGameButton(
            SaveSlotSummary summary,
            bool confirming)
        {
            if (!summary.Exists)
            {
                return "새로 시작";
            }

            return confirming
                ? "한 번 더 누르면 덮어씀"
                : "덮어쓰기";
        }

        /// <summary>처음부터: 기록이 있는 칸은 확인 중일 때만 시작한다.</summary>
        public static bool CanStartNewGame(
            SaveSlotSummary summary,
            bool confirming)
        {
            return !summary.Exists ||
                   confirming;
        }
    }
}
