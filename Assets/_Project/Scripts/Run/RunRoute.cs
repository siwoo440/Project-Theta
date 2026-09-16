using System.Collections.Generic;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 지도에서 장소를 고르는 규칙이다. Unity 없이 도는 순수 계산이다.
    ///
    /// 29일차: "판" 개념을 없앴다. 구역 번호 · 짜인 경로 · 판 끝이 없고,
    /// 장소 8곳 중 어디든 몇 번이든 골라 들어간다.
    /// </summary>
    public static class RunRouteLogic
    {
        /// <summary>고를 수 있는 장소 전부다. 장소 번호순이다.</summary>
        public static List<LocationId> GetLocations()
        {
            List<LocationId> result =
                new List<LocationId>();

            IReadOnlyList<LocationDefinition> all =
                LocationCatalog.All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (all[i] != null &&
                    !result.Contains(all[i].Id))
                {
                    result.Add(all[i].Id);
                }
            }

            result.Sort();

            return result;
        }

        /// <summary>
        /// 장소의 난이도 단계다 (0~4). 예전 "구역 번호" 자리에 쓴다.
        /// 단계가 높을수록 같은 층이라도 고급 NPC가 더 섞인다.
        ///   연수원 0 · 낮 1 · 저녁 2 · 밤 3 · 루프탑 클럽 4
        /// </summary>
        public static int GetTier(
            LocationId location)
        {
            if (location == LocationCatalog.StartLocation)
            {
                return 0;
            }

            if (location == LocationCatalog.FinalLocation)
            {
                return 4;
            }

            switch (LocationCatalog.Get(location).TimeOfDay)
            {
                case LocationTimeOfDay.Day:
                    return 1;

                case LocationTimeOfDay.Evening:
                    return 2;

                default:
                    return 3;
            }
        }
    }

    /// <summary>
    /// 장소 한 번 도전이다 (21일차 "판"을 29일차에 바꿈).
    ///
    /// 지도에서 장소를 고를 때마다 새로 만들어, 레벨 · 강화 카드는 장소마다 1레벨 · 0장에서 시작한다.
    /// 영구 성장은 허브 강화(계약 정기)로만 한다.
    /// <see cref="Core.GameSession"/>이 들고 있어 스테이지 씬을 불러와도 사라지지 않는다. 세이브하지는 않는다.
    /// </summary>
    public sealed class RunSession
    {
        public RunSession(
            int seed,
            LocationId location)
        {
            Seed = seed;
            Location = location;
        }

        public int Seed { get; }

        /// <summary>이번에 도전하는 장소다.</summary>
        public LocationId Location { get; }

        /// <summary>레벨 · 경험치다. 이 장소 안에서만 이어진다.</summary>
        public RunLevelState Level { get; } =
            new RunLevelState();

        /// <summary>고른 강화 카드다. 이 장소 안에서만 이어진다.</summary>
        public RunUpgradeState Upgrades { get; } =
            new RunUpgradeState();

        /// <summary>시작 계약 카드를 이미 골랐는지다. 장소에 들어갈 때 한 번 나온다.</summary>
        public bool StartContractTaken { get; set; }

        /// <summary>고르지 못하고 남은 레벨업 카드 수다. 같은 장소를 다시 지을 때 이어서 고른다.</summary>
        public int CarriedChoices { get; set; }

        /// <summary>이 장소의 숙련도(★ 0~3)다. 출발할 때 세이브에서 읽어 둔다 (30일차).</summary>
        public int Stars { get; set; }

        /// <summary>시작 계약에서 제시하는 카드 수다. 엔딩을 보면 4장이다 (30일차).</summary>
        public int StartChoiceCount { get; set; } = 3;

        /// <summary>이 도전의 결과를 이미 기록했는지다. 결과를 두 번 세지 않게 한다.</summary>
        public bool IsRecorded { get; private set; }

        /// <summary>결과를 기록했다고 표시한다. 처음 한 번만 true를 돌려준다.</summary>
        public bool MarkRecorded()
        {
            if (IsRecorded)
            {
                return false;
            }

            IsRecorded = true;

            return true;
        }
    }
}
