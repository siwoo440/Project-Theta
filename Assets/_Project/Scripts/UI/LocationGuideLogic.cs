using System;
using System.Collections.Generic;
using System.Text;
using ProjectTheta.Balance;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.UI
{
    /// <summary>지도 오른쪽 패널의 상세 창 종류다 (35일차).</summary>
    public enum LocationDetailKind
    {
        None = 0,
        Enemies = 1,
        Rules = 2,
        Rewards = 3,
        Record = 4
    }

    /// <summary>
    /// 장소 안내 문구 · 추천 장소를 만든다 (35일차).
    ///
    /// 지도 패널, 지도 상세 창(적 정보 · 장소 규칙 · 보상 · 기록), 스테이지 첫 입장 규칙 카드가 같은 문구를 쓴다.
    /// Unity 없이 도는 순수 계산이다.
    /// </summary>
    public static class LocationGuideLogic
    {
        public const string Accent = "#E8C15A";
        public const string Warning = "#FF8A7A";

        public static string GetDetailTitle(
            LocationDetailKind kind)
        {
            switch (kind)
            {
                case LocationDetailKind.Enemies:
                    return "적 정보";

                case LocationDetailKind.Rules:
                    return "장소 규칙";

                case LocationDetailKind.Rewards:
                    return "보상";

                case LocationDetailKind.Record:
                    return "기록";

                default:
                    return string.Empty;
            }
        }

        /// <summary>같은 버튼을 다시 누르면 닫는다.</summary>
        public static LocationDetailKind Toggle(
            LocationDetailKind current,
            LocationDetailKind pressed)
        {
            return current == pressed
                ? LocationDetailKind.None
                : pressed;
        }

        /// <summary>"◆◆◇◇◇"다. 숙련도(★)와 헷갈리지 않게 다른 모양을 쓴다.</summary>
        public static string FormatDifficulty(
            int difficulty)
        {
            int clamped =
                Math.Max(
                    LocationGuideCatalog.MinDifficulty,
                    Math.Min(
                        LocationGuideCatalog.MaxDifficulty,
                        difficulty));

            return new string('◆', clamped) +
                   new string('◇', LocationGuideCatalog.MaxDifficulty - clamped);
        }

        /// <summary>패널 위 큰 번호다. 1번 → "01".</summary>
        public static string FormatNumber(
            int index)
        {
            return $"{Math.Max(0, index) + 1:00}";
        }

        /// <summary>목표 방식 설명이다.</summary>
        public static string GetObjectiveGuide(
            LocationObjective objective)
        {
            switch (objective)
            {
                case LocationObjective.Survival:
                    return "열차가 도착할 때마다 몰려드는 인파를 버티세요. 정해진 열차 수를 버티고 동행자가 남아 있으면 클리어입니다.";

                case LocationObjective.Boss:
                    return "라이벌 서큐버스의 보호막을 모두 깨고 함락하면 클리어입니다. 정기를 채워도 끝나지 않습니다.";

                default:
                    return "목표 정기를 채우면 추격이 시작됩니다. 1F 회수 지점에서 탈출하거나 시간이 끝나면 클리어입니다.";
            }
        }

        // 추천 · 첫 입장 --------------------------------------------------

        /// <summary>
        /// 추천 장소다. 아직 클리어하지 않은 장소 중 난이도가 가장 낮은 곳,
        /// 같으면 목표 정기가 낮은 곳, 그래도 같으면 앞 순서다. 모두 깼으면 없다.
        /// </summary>
        public static LocationId? GetRecommended(
            IReadOnlyList<LocationId> candidates,
            SaveData save)
        {
            LocationId? best = null;
            int bestDifficulty = int.MaxValue;
            int bestTarget = int.MaxValue;

            if (candidates == null)
            {
                return null;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                LocationId id = candidates[i];

                if (PlayStatsLogic.Get(save, (int)id).Clears > 0)
                {
                    continue;
                }

                int difficulty = LocationGuideCatalog.Get(id).Difficulty;
                int target = LocationCatalog.Get(id).TargetEssence;

                if (difficulty < bestDifficulty ||
                    (difficulty == bestDifficulty && target < bestTarget))
                {
                    best = id;
                    bestDifficulty = difficulty;
                    bestTarget = target;
                }
            }

            return best;
        }

        /// <summary>이 칸에서 아직 한 번도 도전하지 않은 장소인지다. 첫 입장 규칙 카드를 띄울지 정한다.</summary>
        public static bool IsFirstVisit(
            SaveData save,
            LocationId id)
        {
            return PlayStatsLogic.Get(save, (int)id).Attempts <= 0;
        }

        // 패널 · 상세 창 문구 ---------------------------------------------

        /// <summary>패널의 핵심 수치 세 줄이다.</summary>
        public static string BuildCoreRows(
            LocationDefinition location,
            int stars)
        {
            int target =
                MasteryLogic.GetTargetEssence(
                    location.TargetEssence,
                    stars);

            string mastery =
                stars > 0
                    ? $"{MasteryLogic.FormatStars(stars)}   (보상 +{Percent(MasteryLogic.RewardBonusPerStar * stars)}%)"
                    : MasteryLogic.FormatStars(0);

            return
                $"제한 시간  {PlayStatsLogic.FormatClock(location.TimeLimitSeconds)}     목표 정기  {target}\n" +
                $"층  {location.FloorCount}개     경쟁자  {(LocationGuideCatalog.RivalsAppear(location) ? "금태양 · 인기남" : "없음")}\n" +
                $"숙련  {mastery}";
        }

        public static string BuildDetail(
            LocationDetailKind kind,
            LocationDefinition location,
            SaveData save)
        {
            LocationGuide guide = LocationGuideCatalog.Get(location.Id);
            LocationStats record = PlayStatsLogic.Get(save, (int)location.Id);

            switch (kind)
            {
                case LocationDetailKind.Enemies:
                    return BuildEnemyText(location, guide);

                case LocationDetailKind.Rules:
                    return BuildRuleText(location, guide);

                case LocationDetailKind.Rewards:
                    return BuildRewardText(location, record, save);

                case LocationDetailKind.Record:
                    return BuildRecordText(record);

                default:
                    return string.Empty;
            }
        }

        public static string BuildEnemyText(
            LocationDefinition location,
            LocationGuide guide)
        {
            StringBuilder text = new StringBuilder();

            foreach (LocationGuideEnemy enemy in guide.Enemies)
            {
                text.Append($"<color={Accent}><b>■ {enemy.Name}</b></color>\n");
                text.Append($"   {enemy.Effect}\n");
                text.Append($"   <color=#9FD3A8>대처</color>  {enemy.Counter}\n\n");
            }

            // 루프탑 클럽은 표에 켜져 있어도 금태양 · 인기남이 나오지 않는다.
            if (LocationGuideCatalog.RivalsAppear(location))
            {
                text.Append($"<color={Accent}><b>■ 금태양 · 인기남 (경쟁자)</b></color>\n");
                text.Append("   내 동행자를 노리고 힘겨루기를 걸어온다\n");
                text.Append("   <color=#9FD3A8>대처</color>  무리를 촘촘히 · 힘겨루기에서 이기기\n");
            }

            return text.ToString().TrimEnd();
        }

        public static string BuildRuleText(
            LocationDefinition location,
            LocationGuide guide)
        {
            StringBuilder text = new StringBuilder();

            text.Append($"<color={Accent}><b>목표 · {LocationCatalog.GetObjectiveLabel(location.Objective)}</b></color>\n");
            text.Append($"{GetObjectiveGuide(location.Objective)}\n\n");

            text.Append($"<color={Accent}><b>이 장소의 규칙</b></color>\n");

            foreach (string rule in guide.Rules)
            {
                text.Append($"· {rule}\n");
            }

            text.Append($"\n<color={Accent}><b>공략 팁</b></color>\n");

            foreach (string tip in guide.Tips)
            {
                text.Append($"· {tip}\n");
            }

            return text.ToString().TrimEnd();
        }

        public static string BuildRewardText(
            LocationDefinition location,
            LocationStats record,
            SaveData save)
        {
            StageBalanceValues values = BalanceOverrides.StageOrDefault;
            int stars = MasteryLogic.GetStars(record.Clears);
            int next = MasteryLogic.GetClearsToNextStar(record.Clears);

            StringBuilder text = new StringBuilder();

            text.Append($"<color={Accent}><b>계약 정기 계산</b></color>\n");
            text.Append($"· 클리어  회수 정기 × {Percent(values.ContractEssenceRatio)}% + 등급 보너스 + 초과 정기 × {Percent(values.ContractOverflowRatio)}%\n");
            text.Append($"· 등급 보너스  S +{values.ContractRankBonusS} · A +{values.ContractRankBonusA} · B +{values.ContractRankBonusB}\n");
            text.Append($"· <color={Warning}>실패  목표까지만 × {Percent(values.ContractEssenceRatio)}% × {ContractEssenceLogic.FailurePercent}%</color>\n");
            text.Append($"· <color={Warning}>포기  0</color>\n\n");

            text.Append($"<color={Accent}><b>숙련 {MasteryLogic.FormatStars(stars)}</b></color>\n");
            text.Append($"· 지금  목표 +{Percent(MasteryLogic.TargetBonusPerStar * stars)}% · 보상 +{Percent(MasteryLogic.RewardBonusPerStar * stars)}%\n");
            text.Append(
                next > 0
                    ? $"· 다음 ★까지 클리어 {next}회 (★마다 목표 +{Percent(MasteryLogic.TargetBonusPerStar)}% · 보상 +{Percent(MasteryLogic.RewardBonusPerStar)}%)\n"
                    : "· 최고 숙련입니다\n");

            List<AchievementDefinition> achievements = GetLocationAchievements(location.Id);

            if (achievements.Count > 0)
            {
                text.Append($"\n<color={Accent}><b>이 장소의 업적</b></color>\n");

                foreach (AchievementDefinition achievement in achievements)
                {
                    bool unlocked = AchievementLogic.IsUnlocked(save, achievement.Id);

                    text.Append(
                        unlocked
                            ? $"· {achievement.Name}  <color={Accent}>달성</color>\n"
                            : $"· {achievement.Name}  —  {achievement.Description}  (+{achievement.Reward})\n");
                }
            }

            return text.ToString().TrimEnd();
        }

        public static string BuildRecordText(
            LocationStats record)
        {
            int next = MasteryLogic.GetClearsToNextStar(record.Clears);

            if (record.Attempts <= 0)
            {
                return "아직 도전하지 않은 장소입니다.\n\n처음 들어가면 장소 규칙 카드가 한 번 나옵니다.";
            }

            return
                $"도전          {record.Attempts}회\n" +
                $"클리어        {record.Clears}회   ({PlayStatsLogic.FormatPercent(PlayStatsLogic.GetClearRate(record.Attempts, record.Clears))})\n" +
                $"최고 등급     {record.BestRank}\n" +
                $"최고 정기     {record.BestEssence}\n" +
                $"최단 클리어   {PlayStatsLogic.FormatClock(record.BestClearSeconds)}\n\n" +
                (next > 0
                    ? $"다음 ★까지 클리어 {next}회"
                    : "숙련 최고 단계입니다");
        }

        public static List<AchievementDefinition> GetLocationAchievements(
            LocationId id)
        {
            List<AchievementDefinition> result = new List<AchievementDefinition>();

            foreach (AchievementDefinition achievement in AchievementLogic.All)
            {
                if (achievement.Location == (int)id)
                {
                    result.Add(achievement);
                }
            }

            return result;
        }

        private static int Percent(
            float ratio)
        {
            return (int)Math.Round(ratio * 100f);
        }
    }
}
