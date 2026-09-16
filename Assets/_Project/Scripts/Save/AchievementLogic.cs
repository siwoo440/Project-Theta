using System;
using System.Collections.Generic;

namespace ProjectTheta.Save
{
    /// <summary>업적이 보는 숫자다.</summary>
    public enum AchievementStat
    {
        Attempts,
        Clears,
        Endings,
        Hypnosis,
        MaxFollowers,
        RecoveredEssence,
        BestEssence,
        Reclaimed,
        RampageSurvived,
        DuelWins,
        LevelUps,
        CardsPicked,
        PlayMinutes,
        SRanks,
        LocationsCleared,
        MasteredLocations,

        // --- 32일차 ---
        RecoveredFollowers,
        ContractEssence,
        Captures,
        CleanClears,

        /// <summary>최단 클리어가 <see cref="AchievementLogic.FastClearSeconds"/>초 이하인 장소 수다.</summary>
        FastClearLocations,

        /// <summary>지정한 장소(<see cref="AchievementDefinition.Location"/>)의 최고 등급이 S면 1이다.</summary>
        LocationSRank,

        /// <summary>최고 등급이 S인 장소 수다.</summary>
        SRankLocations
    }

    /// <summary>업적 하나의 정의다.</summary>
    public sealed class AchievementDefinition
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly AchievementStat Stat;
        public readonly int Target;
        public readonly int Reward;

        /// <summary>장소를 지정하는 업적의 장소 번호다. 없으면 −1이다 (32일차).</summary>
        public readonly int Location;

        public AchievementDefinition(
            string id,
            string name,
            string description,
            AchievementStat stat,
            int target,
            int reward,
            int location = -1)
        {
            Id = id;
            Name = name;
            Description = description;
            Stat = stat;
            Target = target;
            Reward = reward;
            Location = location;
        }
    }

    /// <summary>
    /// 업적이다 (30일차). 29일차 누적 통계를 목표로 삼는다.
    /// 달성하면 계약 정기 보상을 받고 세이브에 ID가 남는다. Unity 없이 도는 순수 계산이다.
    /// </summary>
    public static class AchievementLogic
    {
        /// <summary>"번개 출근" 기준 시간(초)이다.</summary>
        public const float FastClearSeconds = 90f;

        private const int TrainingCenter = 0;
        private const int RooftopClub = 7;

        /// <summary>
        /// 업적 목록이다. 업적 창은 이 순서대로 세 줄에 나눠 싣는다.
        /// 30일차 23개 + 32일차 23개. ID는 세이브에 남으므로 바꾸지 않는다.
        /// </summary>
        public static readonly AchievementDefinition[] All =
        {
            // 도전 · 클리어
            new AchievementDefinition("first_step", "첫 출근", "장소에 처음 도전하기", AchievementStat.Attempts, 1, 20),
            new AchievementDefinition("attempts_50", "개근상", "장소 도전 50회", AchievementStat.Attempts, 50, 100),
            new AchievementDefinition("first_clear", "첫 계약", "장소를 처음 클리어하기", AchievementStat.Clears, 1, 30),
            new AchievementDefinition("clear_10", "단골 손님", "장소 클리어 10회", AchievementStat.Clears, 10, 80),
            new AchievementDefinition("clear_30", "도시의 얼굴", "장소 클리어 30회", AchievementStat.Clears, 30, 200),
            new AchievementDefinition("clear_100", "도시의 전설", "장소 클리어 100회", AchievementStat.Clears, 100, 500),
            new AchievementDefinition("clean_10", "무결점", "동행자를 빼앗기지 않고 클리어 10회", AchievementStat.CleanClears, 10, 200),
            new AchievementDefinition("fast_clear", "번개 출근", "아무 장소나 90초 안에 클리어", AchievementStat.FastClearLocations, 1, 120),

            // 장소
            new AchievementDefinition("tour_4", "도시 산책", "서로 다른 장소 4곳 클리어", AchievementStat.LocationsCleared, 4, 60),
            new AchievementDefinition("tour_8", "도시 정복", "장소 8곳 모두 클리어", AchievementStat.LocationsCleared, 8, 150),
            new AchievementDefinition("mastery_1", "단골 장소", "장소 하나를 ★3으로", AchievementStat.MasteredLocations, 1, 80),
            new AchievementDefinition("mastery_4", "단골 거리", "장소 4곳을 ★3으로", AchievementStat.MasteredLocations, 4, 200),
            new AchievementDefinition("mastery_8", "도시의 주인", "장소 8곳 모두 ★3으로", AchievementStat.MasteredLocations, 8, 400),
            new AchievementDefinition("s_training", "모범 연수생", "기업 연수원 S등급 클리어", AchievementStat.LocationSRank, 1, 100, TrainingCenter),
            new AchievementDefinition("s_club", "루프탑의 주인", "루프탑 클럽 S등급 클리어", AchievementStat.LocationSRank, 1, 250, RooftopClub),
            new AchievementDefinition("s_all", "올 S", "장소 8곳 모두 최고 등급 S", AchievementStat.SRankLocations, 8, 500),

            // 엔딩 · 등급
            new AchievementDefinition("ending_1", "루프탑의 밤", "라이벌 서큐버스 함락 (엔딩)", AchievementStat.Endings, 1, 150),
            new AchievementDefinition("ending_3", "밤의 여왕", "엔딩 3회", AchievementStat.Endings, 3, 250),
            new AchievementDefinition("ending_10", "영원한 밤", "엔딩 10회", AchievementStat.Endings, 10, 500),
            new AchievementDefinition("s_rank_5", "완벽주의", "S등급 5번", AchievementStat.SRanks, 5, 120),
            new AchievementDefinition("s_rank_20", "완벽의 경지", "S등급 20번", AchievementStat.SRanks, 20, 400),

            // 최면 · 동행
            new AchievementDefinition("hypnosis_50", "눈빛 연습", "최면 성공 50명", AchievementStat.Hypnosis, 50, 40),
            new AchievementDefinition("hypnosis_300", "매혹의 시선", "최면 성공 300명", AchievementStat.Hypnosis, 300, 120),
            new AchievementDefinition("hypnosis_1000", "도시의 꿈", "최면 성공 1000명", AchievementStat.Hypnosis, 1000, 300),
            new AchievementDefinition("hypnosis_3000", "최면의 정점", "최면 성공 3000명", AchievementStat.Hypnosis, 3000, 600),
            new AchievementDefinition("followers_8", "행렬", "동행자 8명을 한 번에 데리고 다니기", AchievementStat.MaxFollowers, 8, 60),
            new AchievementDefinition("followers_12", "대행렬", "동행자 12명을 한 번에 데리고 다니기", AchievementStat.MaxFollowers, 12, 150),
            new AchievementDefinition("recover_500", "인도자", "동행자 500명 회수", AchievementStat.RecoveredFollowers, 500, 150),
            new AchievementDefinition("reclaim_20", "되찾은 마음", "빼앗긴 동행자 20명 되찾기", AchievementStat.Reclaimed, 20, 60),
            new AchievementDefinition("reclaim_100", "되찾은 사랑", "빼앗긴 동행자 100명 되찾기", AchievementStat.Reclaimed, 100, 200),

            // 정기
            new AchievementDefinition("essence_2000", "정기 수집가", "정기 누적 2000 회수", AchievementStat.RecoveredEssence, 2000, 60),
            new AchievementDefinition("essence_10000", "정기의 바다", "정기 누적 10000 회수", AchievementStat.RecoveredEssence, 10000, 200),
            new AchievementDefinition("essence_30000", "정기의 제왕", "정기 누적 30000 회수", AchievementStat.RecoveredEssence, 30000, 500),
            new AchievementDefinition("big_haul", "한탕", "한 장소에서 정기 300 회수", AchievementStat.BestEssence, 300, 80),
            new AchievementDefinition("big_haul_500", "대박", "한 장소에서 정기 500 회수", AchievementStat.BestEssence, 500, 200),
            new AchievementDefinition("contract_5000", "계약 부자", "계약 정기 누적 5000", AchievementStat.ContractEssence, 5000, 200),

            // 위기
            new AchievementDefinition("rampage_10", "폭주 조련사", "동행자 폭주 10번 버티기", AchievementStat.RampageSurvived, 10, 60),
            new AchievementDefinition("rampage_50", "폭주 마스터", "동행자 폭주 50번 버티기", AchievementStat.RampageSurvived, 50, 200),
            new AchievementDefinition("duel_10", "힘겨루기 달인", "힘겨루기 10번 승리", AchievementStat.DuelWins, 10, 60),
            new AchievementDefinition("duel_30", "힘겨루기 챔피언", "힘겨루기 30번 승리", AchievementStat.DuelWins, 30, 200),
            new AchievementDefinition("captured_20", "불굴", "붙잡히기 20회", AchievementStat.Captures, 20, 80),

            // 성장 · 시간
            new AchievementDefinition("levelups_100", "성장통", "레벨업 100회", AchievementStat.LevelUps, 100, 150),
            new AchievementDefinition("cards_50", "카드 수집가", "강화 카드 50장 고르기", AchievementStat.CardsPicked, 50, 50),
            new AchievementDefinition("cards_200", "카드 도감", "강화 카드 200장 고르기", AchievementStat.CardsPicked, 200, 200),
            new AchievementDefinition("hours_1", "야근 수당", "총 60분 플레이", AchievementStat.PlayMinutes, 60, 60),
            new AchievementDefinition("hours_5", "밤샘 근무", "총 300분 플레이", AchievementStat.PlayMinutes, 300, 300)
        };

        public static AchievementDefinition Get(
            string id)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Id == id)
                {
                    return All[i];
                }
            }

            return null;
        }

        public static bool IsUnlocked(
            SaveData save,
            string id)
        {
            if (save == null ||
                save.UnlockedAchievements == null)
            {
                return false;
            }

            return Array.IndexOf(save.UnlockedAchievements, id) >= 0;
        }

        public static int CountUnlocked(
            SaveData save)
        {
            int count = 0;

            for (int i = 0; i < All.Length; i++)
            {
                if (IsUnlocked(save, All[i].Id))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>업적이 보는 지금 값이다. 장소를 지정하는 업적은 정의를 넘기는 쪽을 쓴다.</summary>
        public static int GetValue(
            SaveData save,
            AchievementDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            if (definition.Stat == AchievementStat.LocationSRank)
            {
                return save != null &&
                       PlayStatsLogic.Get(save, definition.Location).BestRank == "S"
                    ? 1
                    : 0;
            }

            return GetValue(save, definition.Stat);
        }

        /// <summary>업적이 보는 지금 값이다.</summary>
        public static int GetValue(
            SaveData save,
            AchievementStat stat)
        {
            if (save == null)
            {
                return 0;
            }

            PlayStats s = save.Stats ?? new PlayStats();

            switch (stat)
            {
                case AchievementStat.Attempts: return s.Attempts;
                case AchievementStat.Clears: return s.Clears;
                case AchievementStat.Endings: return s.Endings;
                case AchievementStat.Hypnosis: return s.Hypnosis;
                case AchievementStat.MaxFollowers: return s.MaxFollowers;
                case AchievementStat.RecoveredEssence: return s.RecoveredEssence;
                case AchievementStat.BestEssence: return s.BestEssence;
                case AchievementStat.Reclaimed: return s.Reclaimed;
                case AchievementStat.RampageSurvived: return s.RampageSurvived;
                case AchievementStat.DuelWins: return s.DuelWins;
                case AchievementStat.LevelUps: return s.LevelUps;
                case AchievementStat.CardsPicked: return s.CardsPicked;
                case AchievementStat.PlayMinutes: return (int)(s.TotalSeconds / 60f);
                case AchievementStat.SRanks: return s.SRanks;
                case AchievementStat.LocationsCleared: return CountLocations(save, 1);
                case AchievementStat.MasteredLocations: return CountLocations(save, MasteryLogic.StarClears[MasteryLogic.StarClears.Length - 1]);
                case AchievementStat.RecoveredFollowers: return s.RecoveredFollowers;
                case AchievementStat.ContractEssence: return s.ContractEssence;
                case AchievementStat.Captures: return s.Captures;
                case AchievementStat.CleanClears: return s.CleanClears;
                case AchievementStat.FastClearLocations: return CountRecords(save, r => r.BestClearSeconds > 0f && r.BestClearSeconds <= FastClearSeconds);
                case AchievementStat.SRankLocations: return CountRecords(save, r => r.BestRank == "S");
                default: return 0;
            }
        }

        /// <summary>진행도 0~1이다.</summary>
        public static float GetProgress(
            SaveData save,
            AchievementDefinition definition)
        {
            if (definition == null ||
                definition.Target <= 0)
            {
                return 1f;
            }

            if (IsUnlocked(save, definition.Id))
            {
                return 1f;
            }

            return Math.Min(1f, GetValue(save, definition) / (float)definition.Target);
        }

        /// <summary>
        /// 새로 달성한 업적을 세이브에 남기고 보상을 준다. 새로 달성한 목록을 돌려준다.
        /// 이미 달성한 업적은 다시 보상하지 않는다.
        /// </summary>
        public static List<AchievementDefinition> UnlockNew(
            SaveData save)
        {
            List<AchievementDefinition> unlocked = new List<AchievementDefinition>();

            if (save == null)
            {
                return unlocked;
            }

            List<string> ids =
                new List<string>(save.UnlockedAchievements ?? new string[0]);

            for (int i = 0; i < All.Length; i++)
            {
                AchievementDefinition definition = All[i];

                if (ids.Contains(definition.Id) ||
                    GetValue(save, definition) < definition.Target)
                {
                    continue;
                }

                ids.Add(definition.Id);
                save.ContractEssence += Math.Max(0, definition.Reward);
                unlocked.Add(definition);
            }

            save.UnlockedAchievements = ids.ToArray();

            return unlocked;
        }

        /// <summary>두 ID 목록의 차이다. 알림에 쓴다.</summary>
        public static List<AchievementDefinition> Diff(
            string[] before,
            string[] after)
        {
            List<AchievementDefinition> result = new List<AchievementDefinition>();

            if (after == null)
            {
                return result;
            }

            for (int i = 0; i < after.Length; i++)
            {
                if (before != null &&
                    Array.IndexOf(before, after[i]) >= 0)
                {
                    continue;
                }

                AchievementDefinition definition = Get(after[i]);

                if (definition != null)
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        /// <summary>알 수 없는 ID · 중복을 지운다. 예전 세이브는 빈 목록이 된다.</summary>
        public static void Normalize(
            SaveData save)
        {
            if (save == null)
            {
                return;
            }

            List<string> cleaned = new List<string>();

            if (save.UnlockedAchievements != null)
            {
                for (int i = 0; i < save.UnlockedAchievements.Length; i++)
                {
                    string id = save.UnlockedAchievements[i];

                    if (Get(id) != null &&
                        !cleaned.Contains(id))
                    {
                        cleaned.Add(id);
                    }
                }
            }

            save.UnlockedAchievements = cleaned.ToArray();
        }

        private static int CountRecords(
            SaveData save,
            Func<LocationStats, bool> match)
        {
            if (save.LocationRecords == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < save.LocationRecords.Length; i++)
            {
                if (save.LocationRecords[i] != null &&
                    match(save.LocationRecords[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountLocations(
            SaveData save,
            int minimumClears)
        {
            if (save.LocationRecords == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < save.LocationRecords.Length; i++)
            {
                if (save.LocationRecords[i] != null &&
                    save.LocationRecords[i].Clears >= minimumClears)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
