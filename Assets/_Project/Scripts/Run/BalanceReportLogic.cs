using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProjectTheta.Run
{
    /// <summary>장소 하나의 기록 요약이다.</summary>
    public sealed class BalanceReportRow
    {
        public string Location;
        public int Count;
        public int Clears;
        public int Escapes;
        public float AverageSeconds;
        public float MinSeconds;
        public float MaxSeconds;
        public float AverageEssence;
        public float AverageTarget;
        public float AverageHypnosis;
        public float AverageLevel;
        public float AverageRampage;
        public float AverageStolen;
        public float AverageSecondsAfterGoal;

        public float ClearRate =>
            Count <= 0 ? 0f : Clears / (float)Count;

        public float EscapeRate =>
            Clears <= 0 ? 0f : Escapes / (float)Clears;
    }

    /// <summary>
    /// 기록 파일(RunLogs/*.json)을 모아 장소별 표로 만든다 (33일차). Unity 없이 도는 순수 계산이다.
    /// 치트를 쓴 기록과 장소 이름이 없는 옛 기록은 표에서 빼고 개수만 따로 센다.
    /// </summary>
    public static class BalanceReportLogic
    {
        public const string ClearedResult = "Cleared";

        public const string EscapedExit = "Escaped";

        public static List<BalanceReportRow> Build(
            IList<RunLogEntry> entries,
            out int cheated,
            out int unknown)
        {
            cheated = 0;
            unknown = 0;

            Dictionary<string, List<RunLogEntry>> groups =
                new Dictionary<string, List<RunLogEntry>>();

            List<string> order = new List<string>();

            if (entries != null)
            {
                foreach (RunLogEntry entry in entries)
                {
                    if (entry == null ||
                        entry.Stats == null)
                    {
                        continue;
                    }

                    if (entry.Stats.Cheated)
                    {
                        cheated++;
                        continue;
                    }

                    if (string.IsNullOrEmpty(entry.Location))
                    {
                        unknown++;
                        continue;
                    }

                    // 36일차: 심야 모드 기록은 "장소 ☾" 줄로 따로 묶는다.
                    string key = GetGroupKey(entry);

                    if (!groups.TryGetValue(key, out List<RunLogEntry> list))
                    {
                        list = new List<RunLogEntry>();
                        groups[key] = list;
                        order.Add(key);
                    }

                    list.Add(entry);
                }
            }

            List<BalanceReportRow> rows = new List<BalanceReportRow>();

            foreach (string location in order)
            {
                rows.Add(Summarize(location, groups[location]));
            }

            return rows;
        }

        private static BalanceReportRow Summarize(
            string location,
            List<RunLogEntry> list)
        {
            BalanceReportRow row =
                new BalanceReportRow
                {
                    Location = location,
                    Count = list.Count,
                    MinSeconds = float.MaxValue,
                    MaxSeconds = 0f
                };

            foreach (RunLogEntry entry in list)
            {
                RunStats s = entry.Stats;

                if (entry.Result == ClearedResult)
                {
                    row.Clears++;

                    if (entry.Exit == EscapedExit)
                    {
                        row.Escapes++;
                    }
                }

                row.AverageSeconds += s.TotalSeconds;
                row.MinSeconds = Math.Min(row.MinSeconds, s.TotalSeconds);
                row.MaxSeconds = Math.Max(row.MaxSeconds, s.TotalSeconds);
                row.AverageEssence += s.RecoveredEssence;
                row.AverageTarget += entry.TargetEssence;
                row.AverageHypnosis += s.HypnosisCount;
                row.AverageLevel += 1 + (s.LevelUpTimes == null ? 0 : s.LevelUpTimes.Count);
                row.AverageRampage += s.RampageWindups;
                row.AverageStolen += s.StolenCount;
                row.AverageSecondsAfterGoal += entry.SecondsAfterGoal;
            }

            float n = Math.Max(1, list.Count);

            row.AverageSeconds /= n;
            row.AverageEssence /= n;
            row.AverageTarget /= n;
            row.AverageHypnosis /= n;
            row.AverageLevel /= n;
            row.AverageRampage /= n;
            row.AverageStolen /= n;
            row.AverageSecondsAfterGoal /= n;

            if (row.MinSeconds == float.MaxValue)
            {
                row.MinSeconds = 0f;
            }

            return row;
        }

        /// <summary>보고서 글(마크다운 표)이다.</summary>
        public static string ToMarkdown(
            IList<BalanceReportRow> rows,
            int cheated,
            int unknown,
            string generatedAt)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("# 밸런스 보고서");
            builder.AppendLine();
            builder.AppendLine($"- 만든 시각: {generatedAt}");
            builder.AppendLine($"- 표에서 뺀 기록: 치트 {cheated}개 · 장소 없음 {unknown}개");
            builder.AppendLine();
            builder.AppendLine("| 장소 | 기록 | 클리어율 | 탈출률 | 평균 시간 | 최소~최대 | 목표 뒤 머문 시간 | 평균 정기 / 목표 | 최면 | 레벨 | 폭주 예고 | 빼앗김 |");
            builder.AppendLine("| --- | ---: | ---: | ---: | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: |");

            if (rows != null)
            {
                foreach (BalanceReportRow r in rows)
                {
                    builder.AppendLine(
                        $"| {r.Location} | {r.Count} | {Percent(r.ClearRate)} | {Percent(r.EscapeRate)} | " +
                        $"{Seconds(r.AverageSeconds)} | {Seconds(r.MinSeconds)}~{Seconds(r.MaxSeconds)} | {Seconds(r.AverageSecondsAfterGoal)} | " +
                        $"{Number(r.AverageEssence)} / {Number(r.AverageTarget)} | {Number(r.AverageHypnosis)} | {Decimal(r.AverageLevel)} | " +
                        $"{Decimal(r.AverageRampage)} | {Decimal(r.AverageStolen)} |");
                }
            }

            return builder.ToString();
        }

        /// <summary>디버그 패널에 보일 한 줄 요약이다.</summary>
        public static string ToSummary(
            IList<BalanceReportRow> rows)
        {
            if (rows == null ||
                rows.Count == 0)
            {
                return "기록이 없습니다";
            }

            int count = 0;
            float seconds = 0f;

            foreach (BalanceReportRow r in rows)
            {
                count += r.Count;
                seconds += r.AverageSeconds * r.Count;
            }

            return $"장소 {rows.Count}곳 · 기록 {count}개 · 평균 {Seconds(seconds / Math.Max(1, count))}";
        }

        public static string Seconds(
            float value)
        {
            int total = (int)Math.Round(Math.Max(0f, value));

            return $"{total / 60}:{total % 60:00}";
        }

        private static string Percent(
            float ratio)
        {
            return ((int)Math.Round(ratio * 100f)).ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static string Number(
            float value)
        {
            return ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
        }

        private static string Decimal(
            float value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        public static string GetGroupKey(
            RunLogEntry entry)
        {
            return entry.NightMode
                ? $"{entry.Location} ☾"
                : entry.Location;
        }
    }
}
