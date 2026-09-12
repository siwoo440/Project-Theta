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
    /// </summary>
    public static class SaveSystem
    {
        public const string FileName = "projecttheta_save.json";

        public static string FilePath =>
            Path.Combine(
                Application.persistentDataPath,
                FileName);

        public static bool Exists()
        {
            try
            {
                return File.Exists(
                    FilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(
                        FilePath))
                {
                    return SaveDataLogic.CreateDefault();
                }

                string json =
                    File.ReadAllText(
                        FilePath);

                if (string.IsNullOrWhiteSpace(
                        json))
                {
                    return SaveDataLogic.CreateDefault();
                }

                SaveData loaded =
                    JsonUtility.FromJson<SaveData>(
                        json);

                return SaveDataLogic.Normalize(
                    loaded);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveSystem] 세이브를 읽지 못해 기본값으로 시작합니다: {exception.Message}");

                return SaveDataLogic.CreateDefault();
            }
        }

        public static bool Save(
            SaveData data)
        {
            try
            {
                SaveData normalized =
                    SaveDataLogic.Normalize(
                        data);

                string json =
                    JsonUtility.ToJson(
                        normalized,
                        true);

                string directory =
                    Path.GetDirectoryName(
                        FilePath);

                if (!string.IsNullOrEmpty(
                        directory) &&
                    !Directory.Exists(
                        directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }

                File.WriteAllText(
                    FilePath,
                    json);

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveSystem] 세이브 저장에 실패했습니다: {exception.Message}");

                return false;
            }
        }

        public static bool Delete()
        {
            try
            {
                if (File.Exists(
                        FilePath))
                {
                    File.Delete(
                        FilePath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
