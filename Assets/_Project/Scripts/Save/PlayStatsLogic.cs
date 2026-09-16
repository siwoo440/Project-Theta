using System;
using System.Collections.Generic;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 장소 결과를 누적 통계에 더하고, 통계 창에 보일 글자를 만든다 (29일차).
    /// Unity 없이 도는 순수 계산이다.
    /// </summary>
    public static class PlayStatsLogic
    {
        /// <summary>세이브의 통계 칸을 안전한 상태로 맞춘다. 예전 세이브는 빈 기록이 된다.</summary>
        public static void Normalize(
            SaveData data)
        {
            if (data == null)
            {
                return;
            }

            if (data.Stats == null)
            {
                data.Stats = new PlayStats();
            }

            if (data.LocationRecords == null)
            {
                data.LocationRecords = new LocationStats[0];
            }

            List<LocationStats> cleaned = new List<LocationStats>();

            for (int i = 0; i < data.LocationRecords.Length; i++)
            {
                LocationStats record = data.LocationRecords[i];

                if (record == null ||
                    Find(cleaned, record.Location) != null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(record.BestRank))
                {
                    record.BestRank = "-";
                }

                cleaned.Add(record);
            }

            data.LocationRecords = cleaned.ToArray();
        }

        /// <summary>장소 하나의 기록이다. 없으면 빈 기록을 돌려준다(세이브에는 넣지 않는다).</summary>
        public static LocationStats Get(
            SaveData data,
            int location)
        {
            LocationStats found =
                data == null || data.LocationRecords == null
                    ? null
                    : Find(data.LocationRecords, location);

            return found ?? new LocationStats { Location = location };
        }

        /// <summary>
        /// 결과 하나를 통계에 더한다. 장소 정보가 없거나 치트를 쓴 도전은 더하지 않는다.
        /// </summary>
        public static void Apply(
            SaveData target,
            StageResultSummary result)
        {
            if (target == null ||
                !result.HasLocation ||
                result.Cheated)
            {
                return;
            }

            Normalize(target);

            PlayStats stats = target.Stats;

            stats.TotalSeconds += Math.Max(0f, result.PlaySeconds);
            stats.Attempts++;

            if (result.Cleared)
            {
                stats.Clears++;
            }

            if (result.BossDefeated)
            {
                stats.Endings++;
            }

            stats.Hypnosis += Math.Max(0, result.HypnosisCount);
            stats.MaxFollowers = Math.Max(stats.MaxFollowers, result.MaxFollowers);
            stats.RecoveredFollowers += Math.Max(0, result.RecoveredFollowers);
            stats.Stolen += Math.Max(0, result.StolenCount);
            stats.Reclaimed += Math.Max(0, result.ReclaimCount);

            stats.RecoveredEssence += Math.Max(0, result.RecoveredEssence);
            stats.ContractEssence += Math.Max(0, result.ContractEssence);
            stats.BestEssence = Math.Max(stats.BestEssence, result.RecoveredEssence);

            stats.RampageWindups += Math.Max(0, result.RampageWindups);
            stats.RampageSurvived += Math.Max(0, result.RampageSurvived);
            stats.Captures += Math.Max(0, result.CaptureCount);
            stats.DuelWins += Math.Max(0, result.DuelWins);

            stats.LevelUps += Math.Max(0, result.LevelUps);
            stats.CardsPicked += Math.Max(0, result.CardsPicked);

            LocationStats record = Find(target.LocationRecords, result.LocationId);

            if (record == null)
            {
                record = new LocationStats { Location = result.LocationId };

                LocationStats[] grown = new LocationStats[target.LocationRecords.Length + 1];
                Array.Copy(target.LocationRecords, grown, target.LocationRecords.Length);
                grown[grown.Length - 1] = record;
                target.LocationRecords = grown;
            }

            record.Attempts++;
            record.BestEssence = Math.Max(record.BestEssence, result.RecoveredEssence);

            if (!result.Cleared)
            {
                return;
            }

            record.Clears++;

            if (result.PlaySeconds > 0f &&
                (record.BestClearSeconds <= 0f ||
                 result.PlaySeconds < record.BestClearSeconds))
            {
                record.BestClearSeconds = result.PlaySeconds;
            }

            if (SaveDataLogic.IsBetterRank(result.RankLabel, record.BestRank))
            {
                record.BestRank = result.RankLabel;
            }
        }

        // 표시 ----------------------------------------------------------

        public static float GetClearRate(
            int attempts,
            int clears)
        {
            return attempts <= 0
                ? 0f
                : Math.Min(1f, clears / (float)attempts);
        }

        /// <summary>"1시간 02분 05초" · "3분 07초" · "12초"다.</summary>
        public static string FormatDuration(
            float seconds)
        {
            int total = (int)Math.Floor(Math.Max(0f, seconds));
            int hours = total / 3600;
            int minutes = total % 3600 / 60;
            int rest = total % 60;

            if (hours > 0)
            {
                return $"{hours}시간 {minutes:00}분 {rest:00}초";
            }

            if (minutes > 0)
            {
                return $"{minutes}분 {rest:00}초";
            }

            return $"{rest}초";
        }

        /// <summary>"2:35"다. 기록이 없으면 "-"다.</summary>
        public static string FormatClock(
            float seconds)
        {
            if (seconds <= 0f)
            {
                return "-";
            }

            int total = (int)Math.Floor(seconds);

            return $"{total / 60}:{total % 60:00}";
        }

        public static string FormatPercent(
            float ratio)
        {
            return $"{(int)Math.Round(Math.Max(0f, Math.Min(1f, ratio)) * 100f)}%";
        }

        /// <summary>통계 창 요약 칸이다. (분류 제목, [항목, 값] 목록) 순서로 돌려준다.</summary>
        public static List<StatsSection> BuildSections(
            PlayStats stats)
        {
            PlayStats s = stats ?? new PlayStats();

            return new List<StatsSection>
            {
                new StatsSection(
                    "전체",
                    new[]
                    {
                        new StatsRow("총 플레이 시간", FormatDuration(s.TotalSeconds)),
                        new StatsRow("장소 도전", $"{s.Attempts}회"),
                        new StatsRow("클리어", $"{s.Clears}회"),
                        new StatsRow("클리어율", FormatPercent(GetClearRate(s.Attempts, s.Clears))),
                        new StatsRow("엔딩 (보스 함락)", $"{s.Endings}회")
                    }),
                new StatsSection(
                    "최면 · 동행",
                    new[]
                    {
                        new StatsRow("최면 성공", $"{s.Hypnosis}명"),
                        new StatsRow("최대 동행자", $"{s.MaxFollowers}명"),
                        new StatsRow("회수한 동행자", $"{s.RecoveredFollowers}명"),
                        new StatsRow("빼앗긴 동행자", $"{s.Stolen}명"),
                        new StatsRow("되찾은 동행자", $"{s.Reclaimed}명")
                    }),
                new StatsSection(
                    "정기",
                    new[]
                    {
                        new StatsRow("회수한 정기", $"{s.RecoveredEssence}"),
                        new StatsRow("계약 정기", $"{s.ContractEssence}"),
                        new StatsRow("한 번에 가장 많이", $"{s.BestEssence}")
                    }),
                new StatsSection(
                    "위기",
                    new[]
                    {
                        new StatsRow("폭주 예고", $"{s.RampageWindups}회"),
                        new StatsRow("폭주 버팀", $"{s.RampageSurvived}회"),
                        new StatsRow("붙잡힘", $"{s.Captures}회"),
                        new StatsRow("힘겨루기 승리", $"{s.DuelWins}회")
                    }),
                new StatsSection(
                    "성장",
                    new[]
                    {
                        new StatsRow("레벨업", $"{s.LevelUps}회"),
                        new StatsRow("고른 강화 카드", $"{s.CardsPicked}장")
                    })
            };
        }

        /// <summary>장소별 표 한 줄이다. 도전 · 클리어 · 최고 정기 · 최단 시간 · 최고 등급.</summary>
        public static string[] BuildLocationRow(
            LocationStats record)
        {
            LocationStats r = record ?? new LocationStats();

            return new[]
            {
                $"{r.Attempts}",
                $"{r.Clears}",
                $"{r.BestEssence}",
                FormatClock(r.BestClearSeconds),
                string.IsNullOrEmpty(r.BestRank) ? "-" : r.BestRank
            };
        }

        public static readonly string[] LocationColumns =
        {
            "도전", "클리어", "최고 정기", "최단 클리어", "최고 등급"
        };

        private static LocationStats Find(
            IList<LocationStats> list,
            int location)
        {
            if (list == null)
            {
                return null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null &&
                    list[i].Location == location)
                {
                    return list[i];
                }
            }

            return null;
        }
    }

    public struct StatsRow
    {
        public string Label;
        public string Value;

        public StatsRow(
            string label,
            string value)
        {
            Label = label;
            Value = value;
        }
    }

    public sealed class StatsSection
    {
        public readonly string Title;
        public readonly StatsRow[] Rows;

        public StatsSection(
            string title,
            StatsRow[] rows)
        {
            Title = title;
            Rows = rows;
        }
    }
}
