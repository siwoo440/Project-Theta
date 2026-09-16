using System;
using System.Collections.Generic;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 한 판의 경로 규칙이다 (21일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   1구역  기업 연수원 (고정)
    ///   2구역  낮 · 저녁 장소 중 2곳 제시 → 플레이어가 선택
    ///   3구역  저녁 · 밤 장소 중 2곳 제시 → 선택
    ///   4구역  저녁 · 밤 장소 중 2곳 제시 → 선택
    ///   5구역  루프탑 클럽 (고정)
    ///
    /// 두 가지를 지킨다.
    ///   · 같은 장소는 한 판에 한 번만 간다.
    ///   · 시간은 거꾸로 흐르지 않는다. 밤 장소에 간 뒤에는 저녁 장소를 제시하지 않는다.
    /// 제시 후보는 시드와 구역 번호로 정해져서, 지도를 다시 열어도 같은 후보가 나온다.
    ///
    /// 24일차: <see cref="OpenAllLocations"/>가 켜져 있으면 위 규칙 대신
    /// 모든 구역에서 아직 안 간 장소 전부를 고를 수 있다. 장소를 하나씩 만들며 바로 들어가 보기 위해서다.
    /// 짜인 경로로 되돌리려면 이 값만 false로 바꾼다.
    /// </summary>
    public static class RunRouteLogic
    {
        public const int ZoneCount = 5;

        /// <summary>한 번에 제시하는 후보 수다 (짜인 경로일 때).</summary>
        public const int ChoicesPerStep = 2;

        /// <summary>
        /// 모든 장소 열기 (24일차). 켜면 구역마다 아직 안 간 장소 8곳 중 어디든 고른다.
        /// const로 두면 꺼진 쪽 코드가 "닿지 않는 코드" 경고를 내므로 읽기 전용 정적 값으로 둔다.
        /// </summary>
        public static readonly bool OpenAllLocations = true;

        private static readonly LocationId[] MiddleLocations =
        {
            LocationId.Beach,
            LocationId.SubwayStation,
            LocationId.FitnessCenter,
            LocationId.NightMarket,
            LocationId.ShoppingMall,
            LocationId.OfficeTower
        };

        public static bool IsFinalStep(
            int step)
        {
            return step >= ZoneCount - 1;
        }

        /// <summary>그 구역에서 고를 수 있는 장소다. 모든 장소 열기가 켜져 있으면 안 간 장소 전부다.</summary>
        public static List<LocationId> GetCandidates(
            int step,
            IReadOnlyList<LocationId> visited,
            int seed)
        {
            return OpenAllLocations
                ? GetOpenCandidates(visited)
                : GetCuratedCandidates(step, visited, seed);
        }

        /// <summary>아직 안 간 장소 전부다. 장소 번호순이다.</summary>
        public static List<LocationId> GetOpenCandidates(
            IReadOnlyList<LocationId> visited)
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
                    !Contains(visited, all[i].Id) &&
                    !result.Contains(all[i].Id))
                {
                    result.Add(all[i].Id);
                }
            }

            result.Sort();

            return result;
        }

        /// <summary>
        /// 짜인 경로의 후보다 (21일차). 첫 구역과 마지막 구역은 한 곳뿐이다.
        /// </summary>
        public static List<LocationId> GetCuratedCandidates(
            int step,
            IReadOnlyList<LocationId> visited,
            int seed)
        {
            List<LocationId> result =
                new List<LocationId>();

            if (step <= 0)
            {
                result.Add(LocationCatalog.StartLocation);

                return result;
            }

            if (step >= ZoneCount - 1)
            {
                result.Add(LocationCatalog.FinalLocation);

                return result;
            }

            LocationTimeOfDay latest =
                GetLatestTime(
                    visited);

            List<LocationId> pool =
                new List<LocationId>();

            for (int i = 0;
                 i < MiddleLocations.Length;
                 i++)
            {
                LocationId id = MiddleLocations[i];

                if (Contains(visited, id))
                {
                    continue;
                }

                LocationTimeOfDay time =
                    LocationCatalog.Get(id).TimeOfDay;

                if (time < latest ||
                    !IsTimeAllowedAtStep(step, time))
                {
                    continue;
                }

                pool.Add(id);
            }

            // 규칙대로면 비지 않지만, 장소 표를 자산으로 바꿨을 때를 대비해 안 간 곳 전체로 넓힌다.
            if (pool.Count == 0)
            {
                for (int i = 0;
                     i < MiddleLocations.Length;
                     i++)
                {
                    if (!Contains(visited, MiddleLocations[i]))
                    {
                        pool.Add(MiddleLocations[i]);
                    }
                }
            }

            Shuffle(
                pool,
                new Random(
                    unchecked(seed * 31 + step * 7919)));

            int take =
                Math.Min(
                    ChoicesPerStep,
                    pool.Count);

            for (int i = 0;
                 i < take;
                 i++)
            {
                result.Add(pool[i]);
            }

            // 지도 위 순서가 매번 흔들리지 않게 장소 번호순으로 정렬한다.
            result.Sort();

            return result;
        }

        /// <summary>2구역은 낮 · 저녁, 3 · 4구역은 저녁 · 밤을 제시한다.</summary>
        public static bool IsTimeAllowedAtStep(
            int step,
            LocationTimeOfDay time)
        {
            if (step == 1)
            {
                return time != LocationTimeOfDay.Night;
            }

            return time != LocationTimeOfDay.Day;
        }

        public static LocationTimeOfDay GetLatestTime(
            IReadOnlyList<LocationId> visited)
        {
            LocationTimeOfDay latest =
                LocationTimeOfDay.Day;

            if (visited == null)
            {
                return latest;
            }

            for (int i = 0;
                 i < visited.Count;
                 i++)
            {
                LocationTimeOfDay time =
                    LocationCatalog.Get(visited[i]).TimeOfDay;

                if (time > latest)
                {
                    latest = time;
                }
            }

            return latest;
        }

        private static bool Contains(
            IReadOnlyList<LocationId> list,
            LocationId id)
        {
            if (list == null)
            {
                return false;
            }

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                if (list[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Shuffle(
            List<LocationId> list,
            Random random)
        {
            for (int i = list.Count - 1;
                 i > 0;
                 i--)
            {
                int j = random.Next(i + 1);

                LocationId temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }

    /// <summary>한 구역을 끝낸 기록이다. 지도와 결과 화면에 쓴다.</summary>
    public struct ZoneRecord
    {
        public LocationId Location;
        public bool Cleared;
        public int RecoveredEssence;
        public int ContractEssence;
        public string RankLabel;
    }

    /// <summary>
    /// 지금 진행 중인 한 판이다 (21일차).
    ///
    /// 구역마다 스테이지 씬을 다시 불러오므로, 구역을 넘어 이어져야 하는 값은 전부 여기 둔다.
    /// <see cref="Core.GameSession"/>이 들고 있어 씬이 바뀌어도 사라지지 않는다. 세이브하지는 않는다.
    /// </summary>
    public sealed class RunSession
    {
        private readonly List<LocationId> _visited =
            new List<LocationId>();

        private readonly List<ZoneRecord> _records =
            new List<ZoneRecord>();

        public RunSession(
            int seed)
        {
            Seed = seed;
        }

        public int Seed { get; }

        /// <summary>레벨 · 경험치다. 구역을 넘어 이어진다.</summary>
        public RunLevelState Level { get; } =
            new RunLevelState();

        /// <summary>고른 강화 카드다. 구역을 넘어 이어진다.</summary>
        public RunUpgradeState Upgrades { get; } =
            new RunUpgradeState();

        /// <summary>시작 계약 카드를 이미 골랐는지다. 첫 구역에서만 나온다.</summary>
        public bool StartContractTaken { get; set; }

        /// <summary>구역이 끝날 때 고르지 못하고 남은 레벨업 카드 수다. 다음 구역 시작 때 이어서 고른다.</summary>
        public int CarriedChoices { get; set; }

        /// <summary>지도에서 고른, 지금 플레이할 장소다. 고르기 전이면 null이다.</summary>
        public LocationId? SelectedLocation { get; private set; }

        public IReadOnlyList<LocationId> Visited =>
            _visited;

        public IReadOnlyList<ZoneRecord> Records =>
            _records;

        /// <summary>다음에 고를 구역 번호다. 0이 1구역이다.</summary>
        public int NextStep =>
            _records.Count;

        /// <summary>판이 끝났는지다. 실패했거나 마지막 구역을 끝냈으면 true다.</summary>
        public bool IsFinished { get; private set; }

        public int TotalRecoveredEssence
        {
            get
            {
                int total = 0;

                for (int i = 0;
                     i < _records.Count;
                     i++)
                {
                    total += _records[i].RecoveredEssence;
                }

                return total;
            }
        }

        public int TotalContractEssence
        {
            get
            {
                int total = 0;

                for (int i = 0;
                     i < _records.Count;
                     i++)
                {
                    total += _records[i].ContractEssence;
                }

                return total;
            }
        }

        public List<LocationId> GetCandidates()
        {
            return IsFinished
                ? new List<LocationId>()
                : RunRouteLogic.GetCandidates(
                    NextStep,
                    _visited,
                    Seed);
        }

        /// <summary>지도에서 장소를 고른다. 이번 구역 후보가 아니면 거절한다.</summary>
        public bool Select(
            LocationId location)
        {
            if (IsFinished ||
                !GetCandidates().Contains(location))
            {
                return false;
            }

            SelectedLocation = location;

            return true;
        }

        /// <summary>
        /// 구역 결과를 기록한다. 실패했거나 마지막 구역이면 판이 끝난다.
        /// 같은 구역을 두 번 기록하지 않는다.
        /// </summary>
        public void RecordZone(
            ZoneRecord record)
        {
            if (IsFinished ||
                SelectedLocation == null ||
                SelectedLocation.Value != record.Location)
            {
                return;
            }

            _records.Add(record);
            _visited.Add(record.Location);

            SelectedLocation = null;

            if (!record.Cleared ||
                _records.Count >= RunRouteLogic.ZoneCount)
            {
                IsFinished = true;
            }
        }

        /// <summary>지도에서 판을 포기한다.</summary>
        public void Abandon()
        {
            SelectedLocation = null;
            IsFinished = true;
        }

        /// <summary>지금 구역이 마지막 구역인지다. 결과 화면 버튼 문구에 쓴다.</summary>
        public bool IsPlayingFinalZone =>
            SelectedLocation != null &&
            RunRouteLogic.IsFinalStep(
                NextStep);
    }
}
