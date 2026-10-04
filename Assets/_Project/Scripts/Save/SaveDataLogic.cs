using System;

namespace ProjectTheta.Save
{
    using System.Collections.Generic; // 목록과 집합 참조
    using ProjectTheta.Story; // 이야기 ID 참조

    /// <summary>
    /// 세이브 데이터에 대한 판정과 갱신 규칙이다.
    ///
    /// 파일 입출력은 <see cref="SaveSystem"/>이 담당하고,
    /// 여기서는 Unity에 의존하지 않는 순수 계산만 다뤄 테스트로 고정한다.
    /// </summary>
    public static class SaveDataLogic
    {
        public const int CurrentVersion = 3;

        /// <summary>성장 계열 수다. 최면 / 관리 / 안정 / 기동.</summary>
        public const int UpgradeTrackCount = 4;

        public static SaveData CreateDefault()
        {
            SaveData data =
                new SaveData
                {
                    Version = CurrentVersion,
                    ClearCount = 0,
                    PlayCount = 0,
                    BestScore = 0,
                    BestRankLabel = "-",
                    ContractEssence = 0,
                    CurrentWorldTime =
                        (int)Stage.Locations.WorldTimeOfDay.Morning,
                    RegionRiskMultipliers =
                        Stage.Locations.RegionRiskLogic.CreateNeutralMultipliers(),
                    UpgradeLevels =
                        new int[UpgradeTrackCount]
                };

            // 31일차: 설정 기본값(음량 80% · 전체화면 · 1920×1080 · 커서 보통).
            SettingsLogic.ApplyDefaults(
                data);

            return data;
        }

        /// <summary>
        /// 읽어들인 데이터를 안전한 상태로 보정한다.
        /// 손상되었거나 필드가 비어 있어도 게임이 시작되지 않는 일이 없어야 한다.
        /// </summary>
        public static SaveData Normalize(
            SaveData data)
        {
            if (data == null)
            {
                return CreateDefault();
            }

            if (data.UpgradeLevels == null ||
                data.UpgradeLevels.Length !=
                UpgradeTrackCount)
            {
                int[] levels =
                    new int[UpgradeTrackCount];

                if (data.UpgradeLevels != null)
                {
                    int copyCount =
                        Math.Min(
                            data.UpgradeLevels.Length,
                            UpgradeTrackCount);

                    Array.Copy(
                        data.UpgradeLevels,
                        levels,
                        copyCount);
                }

                data.UpgradeLevels =
                    levels;
            }

            for (int i = 0;
                 i < data.UpgradeLevels.Length;
                 i++)
            {
                data.UpgradeLevels[i] =
                    UpgradeLogic.ClampLevel(
                        data.UpgradeLevels[i]);
            }

            data.ClearCount =
                Math.Max(
                    0,
                    data.ClearCount);

            data.PlayCount =
                Math.Max(
                    data.ClearCount,
                    Math.Max(
                        0,
                        data.PlayCount));

            data.BestScore =
                Math.Max(
                    0,
                    data.BestScore);

            data.ContractEssence =
                Math.Max(
                    0,
                    data.ContractEssence);

            if (data.RegionRiskMultipliers == null ||
                data.RegionRiskMultipliers.Length !=
                Stage.Locations.RegionRiskLogic.RegionCount)
            {
                data.RegionRiskMultipliers =
                    Stage.Locations.RegionRiskLogic.CreateNeutralMultipliers();
            }
            else
            {
                for (int i = 0;
                     i < data.RegionRiskMultipliers.Length;
                     i++)
                {
                    data.RegionRiskMultipliers[i] =
                        data.RegionRiskMultipliers[i] <= 0f
                            ? Stage.Locations.RegionRiskLogic.NeutralMultiplier
                            : Stage.Locations.RegionRiskLogic.ClampMultiplier(
                                data.RegionRiskMultipliers[i]);
                }
            }

            data.CurrentWorldTime =
                Stage.Locations.RegionRiskLogic.NormalizeTimeValue(
                    data.CurrentWorldTime);

            if (string.IsNullOrEmpty(
                    data.BestRankLabel))
            {
                data.BestRankLabel =
                    "-";
            }

            // 29일차: 통계 칸이 없던 예전 세이브도 빈 기록으로 읽는다.
            PlayStatsLogic.Normalize(
                data);

            // 30일차: 알 수 없는 업적 ID를 지운다.
            Story.StoryLogic.Normalize(
                data);

            // 45일차: 이야기 이관 뒤 관계 보상과 엔딩 기록을 복원한다.
            RiellaAffinityLogic.Normalize( // 호감도 자료 보정
                data); // 저장 자료 전달

            EndingProgressLogic.Normalize( // 엔딩 기록 보정
                data); // 저장 자료 전달

            AchievementLogic.Normalize(
                data);

            // 31일차: 설정 칸이 없던 예전 세이브는 기본값으로 채운다.
            SettingsLogic.Normalize(
                data);

            data.Version =
                Migrate(
                    data.Version);

            return data;
        }

        /// <summary>
        /// 구버전 저장을 현재 버전으로 올린다.
        /// 아직 구버전이 없으므로 버전 번호만 맞추지만, 경로를 미리 만들어 둔다.
        /// </summary>
        public static int Migrate(
            int version)
        {
            if (version <= 0)
            {
                return CurrentVersion;
            }

            return version >
                   CurrentVersion
                ? CurrentVersion
                : CurrentVersion;
        }

        /// <summary>랭크 라벨의 우열을 비교한다. 높을수록 좋은 랭크다.</summary>
        public static int GetRankOrder(
            string rankLabel)
        {
            switch (rankLabel)
            {
                case "S":
                    return 4;

                case "A":
                    return 3;

                case "B":
                    return 2;

                case "C":
                    return 1;

                default:
                    return 0;
            }
        }

        public static bool IsBetterRank(
            string candidate,
            string current)
        {
            return GetRankOrder(
                       candidate) >
                   GetRankOrder(
                       current);
        }

        public static int GetUpgradeLevel(
            SaveData data,
            UpgradeTrack track)
        {
            SaveData target =
                Normalize(
                    data);

            int index =
                (int)track;

            return index >= 0 &&
                   index < target.UpgradeLevels.Length
                ? target.UpgradeLevels[index]
                : 0;
        }

        /// <summary>
        /// 성장 한 단계를 구매한다. 정기가 모자라거나 만렙이면 아무것도 하지 않는다.
        /// </summary>
        public static bool TryPurchaseUpgrade(
            SaveData data,
            UpgradeTrack track)
        {
            SaveData target =
                Normalize(
                    data);

            int index =
                (int)track;

            if (index < 0 ||
                index >= target.UpgradeLevels.Length)
            {
                return false;
            }

            int level =
                target.UpgradeLevels[index];

            if (!UpgradeLogic.CanPurchase(
                    level,
                    target.ContractEssence))
            {
                return false;
            }

            target.ContractEssence =
                UpgradeLogic.GetRemainingAfterPurchase(
                    level,
                    target.ContractEssence);

            target.UpgradeLevels[index] =
                UpgradeLogic.ClampLevel(
                    level + 1);

            return true;
        }

        /// <summary>
        /// 장소 한 번의 결과를 세이브에 반영한다.
        /// 최고 기록은 더 좋을 때만 갱신하고, 낮은 기록은 무시한다.
        /// </summary>
        public static SaveData ApplyStageResult(
            SaveData data,
            StageResultSummary result)
        {
            SaveData target =
                Normalize(
                    data);

            target.PlayCount++;

            if (result.Cleared)
            {
                target.ClearCount++;
            }

            if (result.TotalScore >
                target.BestScore)
            {
                target.BestScore =
                    result.TotalScore;
            }

            // 랭크는 클리어한 판에서만 의미가 있다.
            if (result.Cleared &&
                IsBetterRank(
                    result.RankLabel,
                    target.BestRankLabel))
            {
                target.BestRankLabel =
                    result.RankLabel;
            }

            target.ContractEssence +=
                Math.Max(
                    0,
                    result.ContractEssence);

            // 29일차: 누적 통계 · 장소별 기록.
            PlayStatsLogic.Apply(
                target,
                result);

            // 30일차: 새로 달성한 업적을 남기고 보상을 준다.
            AchievementLogic.UnlockNew(
                target);

            return target;
        }
    }

    public static class RiellaAffinityLogic // 리엘라 호감도 규칙
    { // 클래스 시작
        public const int Minimum = 0; // 호감도 최솟값
        public const int Maximum = 100; // 호감도 최댓값
        public const int StoryEventReward = 10; // 관계 이벤트 보상
        public const int UniqueStageReward = 1; // 고유 지역 보상

        private static readonly HashSet<string> RelationshipStoryIds = new HashSet<string>(StringComparer.Ordinal) // 관계 이벤트 ID 집합
        { // 집합 시작
            StoryCatalog.RiellaMorningAfterId, // H01 장면
            StoryCatalog.RiellaPromiseId, // H02 장면
            StoryCatalog.RiellaCheckTogetherId, // H03 장면
            StoryCatalog.RiellaSharedBurdenId, // H04 장면
            StoryCatalog.RiellaSharedResponsibilityId, // H05 장면
            StoryCatalog.RiellaStayTogetherId // H06 장면
        }; // 집합 끝

        private static readonly HashSet<string> CurrentUniqueStageStoryIds = new HashSet<string>(StringComparer.Ordinal) // 현재 고유 지역 장면 집합
        { // 집합 시작
            "clear_training", // 학교 첫 완료
            "clear_beach", // 해변 첫 완료
            "clear_subway", // 지하철 첫 완료
            "clear_fitness", // 헬스장 첫 완료
            "clear_market", // 야시장 첫 완료
            "clear_mall", // 쇼핑몰 첫 완료
            "clear_office" // 오피스 첫 완료
        }; // 집합 끝

        public static void Normalize(SaveData save) // 호감도 저장 보정
        { // 보정 시작
            if (save == null) // 저장 확인
            { // 중단 시작
                return; // 보정 중단
            } // 중단 끝

            save.RiellaAffinity = Clamp(save.RiellaAffinity); // 호감도 범위 보정
            save.RiellaAffinityRewards = NormalizeIds(save.RiellaAffinityRewards); // 보상 출처 보정
            string[] completed = save.CompletedStories ?? new string[0]; // 완료 장면 목록

            foreach (string storyId in completed) // 완료 장면 순회
            { // 순회 시작
                ApplyStoryCompletion(save, storyId); // 누락 보상 복원
            } // 순회 끝
        } // 보정 끝

        public static bool ApplyStoryCompletion(SaveData save, string storyId) // 이야기 완료 보상 적용
        { // 적용 시작
            if (RelationshipStoryIds.Contains(storyId)) // 관계 이벤트 확인
            { // 관계 보상 시작
                return Grant(save, storyId, StoryEventReward); // 관계 보상 지급
            } // 관계 보상 끝

            if (CurrentUniqueStageStoryIds.Contains(storyId)) // 고유 지역 확인
            { // 지역 보상 시작
                return Grant(save, storyId, UniqueStageReward); // 지역 보상 지급
            } // 지역 보상 끝

            return false; // 보상 없음 반환
        } // 적용 끝

        public static bool Grant(SaveData save, string rewardId, int amount) // 고유 보상 지급
        { // 지급 시작
            if (save == null || string.IsNullOrEmpty(rewardId) || amount <= 0) // 입력 확인
            { // 거부 시작
                return false; // 지급 실패
            } // 거부 끝

            string[] rewards = save.RiellaAffinityRewards ?? new string[0]; // 기존 보상 목록

            if (Array.IndexOf(rewards, rewardId) >= 0) // 중복 보상 확인
            { // 거부 시작
                return false; // 중복 지급 차단
            } // 거부 끝

            string[] grown = new string[rewards.Length + 1]; // 확장 보상 목록
            Array.Copy(rewards, grown, rewards.Length); // 기존 보상 복사
            grown[grown.Length - 1] = rewardId; // 신규 보상 출처 추가
            save.RiellaAffinityRewards = grown; // 확장 목록 저장
            save.RiellaAffinity = Clamp(save.RiellaAffinity + amount); // 호감도 증가

            return true; // 지급 성공
        } // 지급 끝

        public static int Clamp(int value) // 호감도 범위 제한
        { // 제한 시작
            return Math.Max(Minimum, Math.Min(Maximum, value)); // 제한값 반환
        } // 제한 끝

        private static string[] NormalizeIds(string[] values) // 보상 ID 배열 보정
        { // 보정 시작
            List<string> result = new List<string>(); // 결과 목록
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal); // 중복 확인 집합

            if (values != null) // 기존 배열 확인
            { // 순회 준비
                foreach (string value in values) // 값 순회
                { // 순회 시작
                    if (!string.IsNullOrEmpty(value) && seen.Add(value)) // 유효성과 중복 확인
                    { // 추가 시작
                        result.Add(value); // 결과 추가
                    } // 추가 끝
                } // 순회 끝
            } // 순회 준비 끝

            return result.ToArray(); // 보정 배열 반환
        } // 보정 끝
    } // 클래스 끝
}
