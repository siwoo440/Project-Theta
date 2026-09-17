using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 장소 한 번의 결과를 스테이지에서 허브 · 지도로 넘기는 묶음이다.
    /// 29일차부터 누적 통계(<see cref="PlayStats"/>)에 쓸 숫자도 함께 담는다.
    /// </summary>
    public struct StageResultSummary
    {
        public bool Cleared;
        public int RecoveredEssence;
        public int TotalScore;
        public string RankLabel;

        /// <summary>36일차: 심야 모드로 도전했는지다.</summary>
        public bool NightMode;

        /// <summary>이번 판에서 환산된 계약 정기다.</summary>
        public int ContractEssence;
        public int TargetEssence;

        // --- 29일차: 통계 ---
        /// <summary>장소 정보가 있는 결과인지다. 없으면 통계에 더하지 않는다.</summary>
        public bool HasLocation;
        public int LocationId;
        public float PlaySeconds;
        public bool BossDefeated;
        public bool Cheated;

        public int HypnosisCount;
        public int MaxFollowers;
        public int RecoveredFollowers;
        public int StolenCount;
        public int ReclaimCount;
        public int RampageWindups;
        public int RampageSurvived;
        public int CaptureCount;
        public int DuelWins;
        public int LevelUps;
        public int CardsPicked;

        public static StageResultSummary Empty =>
            new StageResultSummary
            {
                Cleared = false,
                RecoveredEssence = 0,
                TotalScore = 0,
                RankLabel = "-",
                ContractEssence = 0,
                TargetEssence = 0
            };
    }

    /// <summary>
    /// 저장 파일에 들어가는 내용이다.
    ///
    /// JsonUtility로 직렬화하므로 public 필드만 사용한다.
    /// 13일차에는 클리어 횟수·최고 기록만 실제로 채우고,
    /// 계약 정기와 성장 레벨은 필드만 준비해 14일차에 사용한다.
    /// </summary>
    [Serializable]
    public sealed class SaveData
    {
        /// <summary>저장 형식 버전이다. 이후 마이그레이션 경로의 기준이 된다.</summary>
        public int Version = SaveDataLogic.CurrentVersion;

        public int ClearCount;
        public int PlayCount;
        public int BestScore;
        public string BestRankLabel = "-";

        // --- 14일차에 사용 ---
        public int ContractEssence;
        public int[] UpgradeLevels = new int[4];

        // --- 18일차 ---
        /// <summary>
        /// 튜토리얼을 끝까지 마쳤는지다. 마친 뒤로는 안내 문구를 띄우지 않는다.
        /// 예전 세이브에는 이 필드가 없지만 JsonUtility가 false로 채우므로 호환된다.
        /// </summary>
        public bool TutorialCompleted;

        // --- 19일차 ---
        /// <summary>
        /// 화면 흔들림을 끈 상태인지다.
        /// "켜짐"이 아니라 "꺼짐"으로 저장하는 이유: 예전 세이브에는 이 항목이 없어서
        /// JsonUtility가 false로 채운다. "켜짐"으로 두면 예전 세이브는 흔들림이 꺼진 채 시작된다.
        /// </summary>
        public bool ScreenShakeDisabled;

        // --- 34일차 ---
        /// <summary>
        /// 위기 화면 효과(가장자리 붉은 맥동)를 끈 상태인지다. 흔들림과 같은 이유로 "꺼짐"으로 저장한다.
        /// </summary>
        public bool DangerEffectDisabled;

        /// <summary>마지막으로 저장한 때("yyyy-MM-dd HH:mm")다. 저장 칸 목록에 보여 준다.</summary>
        public string SavedAt = string.Empty;

        /// <summary>마지막으로 저장한 때(Ticks)다. 가장 최근 칸을 고를 때 쓴다.</summary>
        public long SavedTicks;

        // --- 36일차 ---
        /// <summary>본 이야기 장면 ID다 (<see cref="Story.StoryLogic"/>).</summary>
        public string[] SeenStories = new string[0];

        // --- 29일차 ---
        /// <summary>지금까지의 누적 통계다. 지도의 [통계] 창이 보여 준다.</summary>
        public PlayStats Stats = new PlayStats();

        /// <summary>장소별 누적 기록이다. 한 번이라도 도전한 장소만 들어 있다.</summary>
        public LocationStats[] LocationRecords = new LocationStats[0];

        // --- 30일차 ---
        /// <summary>달성한 업적 ID다 (<see cref="AchievementLogic"/>).</summary>
        public string[] UnlockedAchievements = new string[0];

        // --- 31일차: 설정 ---
        /// <summary>
        /// 설정을 한 번이라도 저장했는지다. 예전 세이브는 false라서 <see cref="SettingsLogic.Normalize"/>가 기본값을 넣는다.
        /// (음량 칸이 없으면 0으로 읽혀 무음이 되기 때문이다.)
        /// </summary>
        public bool SettingsInitialized;

        public float MasterVolume = SettingsLogic.DefaultVolume;
        public float SfxVolume = SettingsLogic.DefaultVolume;
        public float MusicVolume = SettingsLogic.DefaultVolume;
        public bool Fullscreen = true;
        public int ResolutionIndex = SettingsLogic.DefaultResolutionIndex;

        /// <summary><see cref="Save.CursorSize"/> 번호다.</summary>
        public int CursorSize = (int)Save.CursorSize.Normal;

        // --- 32일차: 키 설정 ---
        /// <summary>"MoveUp=W,UpArrow" 줄들이다. 비어 있으면 기본 키를 쓴다 (<see cref="Core.InputBindingLogic"/>).</summary>
        public string[] KeyBindings = new string[0];

        public SaveData Clone()
        {
            SaveData copy =
                new SaveData
                {
                    Version = Version,
                    ClearCount = ClearCount,
                    PlayCount = PlayCount,
                    BestScore = BestScore,
                    BestRankLabel = BestRankLabel,
                    ContractEssence = ContractEssence,
                    TutorialCompleted = TutorialCompleted,
                    ScreenShakeDisabled = ScreenShakeDisabled,
                    DangerEffectDisabled = DangerEffectDisabled,
                    SavedAt = SavedAt,
                    SavedTicks = SavedTicks,
                    SeenStories = SeenStories == null
                        ? new string[0]
                        : (string[])SeenStories.Clone(),
                    UpgradeLevels = new int[UpgradeLevels?.Length ?? 4],
                    Stats = Stats == null ? new PlayStats() : Stats.Clone(),
                    LocationRecords = new LocationStats[LocationRecords?.Length ?? 0],
                    SettingsInitialized = SettingsInitialized,
                    MasterVolume = MasterVolume,
                    SfxVolume = SfxVolume,
                    MusicVolume = MusicVolume,
                    Fullscreen = Fullscreen,
                    ResolutionIndex = ResolutionIndex,
                    CursorSize = CursorSize,
                    KeyBindings = KeyBindings == null
                        ? new string[0]
                        : (string[])KeyBindings.Clone(),
                    UnlockedAchievements = UnlockedAchievements == null
                        ? new string[0]
                        : (string[])UnlockedAchievements.Clone()
                };

            for (int i = 0; i < copy.LocationRecords.Length; i++)
            {
                copy.LocationRecords[i] = LocationRecords[i]?.Clone();
            }

            if (UpgradeLevels != null)
            {
                Array.Copy(
                    UpgradeLevels,
                    copy.UpgradeLevels,
                    UpgradeLevels.Length);
            }

            return copy;
        }
    }
}
