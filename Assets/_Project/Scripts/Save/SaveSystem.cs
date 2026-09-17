using System;
using System.IO;
using UnityEngine;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 세이브 파일 입출력이다.
    ///
    /// 읽기 실패는 절대 게임을 막지 않는다. 파일이 없거나 손상됐으면
    /// 조용히 기본값으로 시작한다. 세이브 오류로 게임이 켜지지 않는 것이 최악이다.
    ///
    /// 34일차: 저장 칸 3개 + 공용 설정 파일로 나눴다 (<see cref="SaveSlotLogic"/>).
    /// 쓸 때는 임시 파일에 먼저 쓰고 바꿔 끼워, 쓰는 도중 꺼져도 원래 파일이 깨지지 않게 한다.
    /// </summary>
    public static class SaveSystem
    {
        public static string Folder =>
            Application.persistentDataPath;

        public static string LegacyPath =>
            Path.Combine(
                Folder,
                SaveSlotLogic.LegacyFileName);

        public static string SettingsPath =>
            Path.Combine(
                Folder,
                SaveSlotLogic.SettingsFileName);

        public static string GetSlotPath(
            int slot)
        {
            return Path.Combine(
                Folder,
                SaveSlotLogic.GetFileName(
                    slot));
        }

        public static bool SlotExists(
            int slot)
        {
            return SaveSlotLogic.IsValid(slot) &&
                   FileExists(
                       GetSlotPath(slot));
        }

        // 칸 ------------------------------------------------------------

        /// <summary>칸을 읽는다. 없거나 깨졌으면 null이다.</summary>
        public static SaveData TryLoadSlot(
            int slot)
        {
            if (!SaveSlotLogic.IsValid(slot))
            {
                return null;
            }

            return ReadSave(
                GetSlotPath(slot));
        }

        /// <summary>칸을 읽는다. 없거나 깨졌으면 기본값이다.</summary>
        public static SaveData LoadSlot(
            int slot)
        {
            return TryLoadSlot(slot) ??
                   SaveDataLogic.CreateDefault();
        }

        public static bool SaveSlot(
            int slot,
            SaveData data)
        {
            if (!SaveSlotLogic.IsValid(slot))
            {
                return false;
            }

            SaveData normalized =
                SaveDataLogic.Normalize(
                    data);

            return WriteText(
                GetSlotPath(slot),
                JsonUtility.ToJson(
                    normalized,
                    true));
        }

        public static SaveSlotSummary[] GetSummaries()
        {
            SaveSlotSummary[] summaries =
                new SaveSlotSummary[SaveSlotLogic.SlotCount];

            for (int i = 0; i < summaries.Length; i++)
            {
                summaries[i] =
                    SaveSlotLogic.Summarize(
                        i,
                        TryLoadSlot(i));
            }

            return summaries;
        }

        // 설정 ----------------------------------------------------------

        /// <summary>공용 설정을 읽는다. 없거나 깨졌으면 null이다.</summary>
        public static SettingsData LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return null;
                }

                string json =
                    File.ReadAllText(
                        SettingsPath);

                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : JsonUtility.FromJson<SettingsData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveSystem] 설정을 읽지 못해 기본값으로 시작합니다: {exception.Message}");

                return null;
            }
        }

        public static bool SaveSettings(
            SettingsData settings)
        {
            if (settings == null)
            {
                return false;
            }

            return WriteText(
                SettingsPath,
                JsonUtility.ToJson(
                    settings,
                    true));
        }

        // 옛 세이브 옮기기 -----------------------------------------------

        /// <summary>
        /// 33일차까지의 세이브(projecttheta_save.json)를 1번 칸과 설정 파일로 옮긴다.
        /// 원본 파일은 지우지 않는다.
        /// </summary>
        public static bool MigrateLegacyIfNeeded()
        {
            bool anySlot = false;

            for (int i = 0; i < SaveSlotLogic.SlotCount; i++)
            {
                anySlot |= SlotExists(i);
            }

            if (!SaveSlotLogic.ShouldMigrate(
                    FileExists(LegacyPath),
                    FileExists(SettingsPath),
                    anySlot))
            {
                return false;
            }

            SaveData legacy =
                ReadSave(
                    LegacyPath);

            if (legacy == null)
            {
                return false;
            }

            try
            {
                SaveSlotLogic.Stamp(
                    legacy,
                    File.GetLastWriteTime(
                        LegacyPath));
            }
            catch (Exception)
            {
                SaveSlotLogic.Stamp(
                    legacy,
                    DateTime.Now);
            }

            bool moved =
                SaveSlot(0, legacy) &&
                SaveSettings(
                    SaveSlotLogic.ExtractSettings(
                        legacy));

            if (moved)
            {
                Debug.Log(
                    "[SaveSystem] 예전 세이브를 1번 칸으로 옮겼습니다. 원본 파일은 그대로 남겨 둡니다.");
            }

            return moved;
        }

        // 공통 ----------------------------------------------------------

        private static SaveData ReadSave(
            string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                string json =
                    File.ReadAllText(
                        path);

                if (string.IsNullOrWhiteSpace(
                        json))
                {
                    return null;
                }

                SaveData loaded =
                    JsonUtility.FromJson<SaveData>(
                        json);

                return loaded == null
                    ? null
                    : SaveDataLogic.Normalize(
                        loaded);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveSystem] 세이브를 읽지 못했습니다({Path.GetFileName(path)}): {exception.Message}");

                return null;
            }
        }

        private static bool WriteText(
            string path,
            string text)
        {
            try
            {
                string directory =
                    Path.GetDirectoryName(
                        path);

                if (!string.IsNullOrEmpty(
                        directory) &&
                    !Directory.Exists(
                        directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }

                string temp =
                    path + ".tmp";

                File.WriteAllText(
                    temp,
                    text);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(
                    temp,
                    path);

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveSystem] 저장에 실패했습니다({Path.GetFileName(path)}): {exception.Message}");

                return false;
            }
        }

        private static bool FileExists(
            string path)
        {
            try
            {
                return File.Exists(
                    path);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
