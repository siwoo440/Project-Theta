using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 도시 지배도다 (36일차). 엔딩 뒤에도 이어지는 최종 목표다.
    ///
    ///   장소마다  클리어 1 + 숙련 ★ 최대 3 + 심야 클리어 1 = 5점
    ///   8곳 = 40점 만점 → %
    ///   25 · 50 · 75 · 100%마다 칭호가 오른다. 100%면 마지막 후일담이 나온다.
    /// </summary>
    public static class DominionLogic
    {
        public const int LocationCount = 8;
        public const int PointsPerLocation = 5;

        public static readonly int[] TitleThresholds = { 0, 25, 50, 75, 100 };

        public static readonly string[] Titles =
        {
            "신참 서큐버스",
            "밤거리 단골",
            "뒷골목 실세",
            "도시의 그림자",
            "도시의 주인"
        };

        public static int MaxPoints =>
            LocationCount * PointsPerLocation;

        public static int GetPoints(
            LocationStats record)
        {
            if (record == null)
            {
                return 0;
            }

            return (record.Clears > 0 ? 1 : 0) +
                   MasteryLogic.GetStars(record.Clears) +
                   (record.NightClears > 0 ? 1 : 0);
        }

        public static int GetTotalPoints(
            SaveData save)
        {
            int total = 0;

            for (int i = 0; i < LocationCount; i++)
            {
                total += GetPoints(PlayStatsLogic.Get(save, i));
            }

            return total;
        }

        /// <summary>0~100이다. 40점을 모두 채워야 100이다(반올림으로 100이 되지 않게 내림).</summary>
        public static int GetPercent(
            SaveData save)
        {
            return (int)Math.Floor(GetTotalPoints(save) * 100.0 / MaxPoints);
        }

        public static float GetRatio(
            SaveData save)
        {
            return GetTotalPoints(save) / (float)MaxPoints;
        }

        public static int GetTitleIndex(
            int percent)
        {
            int index = 0;

            for (int i = 0; i < TitleThresholds.Length; i++)
            {
                if (percent >= TitleThresholds[i])
                {
                    index = i;
                }
            }

            return index;
        }

        public static string GetTitle(
            int percent)
        {
            return Titles[GetTitleIndex(percent)];
        }

        /// <summary>다음 칭호까지의 %다. 최고 칭호면 -1이다.</summary>
        public static int GetNextThreshold(
            int percent)
        {
            int index = GetTitleIndex(percent);

            return index + 1 < TitleThresholds.Length
                ? TitleThresholds[index + 1]
                : -1;
        }

        public static string GetSummary(
            SaveData save)
        {
            int percent = GetPercent(save);
            int next = GetNextThreshold(percent);

            return next < 0
                ? $"도시 지배도 {percent}%  「{GetTitle(percent)}」"
                : $"도시 지배도 {percent}%  「{GetTitle(percent)}」  ·  다음 칭호 {next}%";
        }

        /// <summary>장소 한 곳의 점수 내역이다. "클리어 ✓ · ★★☆ · ☾ ✗  3/5".</summary>
        public static string DescribeLocation(
            LocationStats record)
        {
            LocationStats r = record ?? new LocationStats();

            return
                $"클리어 {(r.Clears > 0 ? "✓" : "✗")} · {MasteryLogic.FormatStars(MasteryLogic.GetStars(r.Clears))} · ☾ {(r.NightClears > 0 ? "✓" : "✗")}   {GetPoints(r)}/{PointsPerLocation}";
        }
    }
}
