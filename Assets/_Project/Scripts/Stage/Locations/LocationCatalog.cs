using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>도시 지도의 장소 8곳이다 (기획서 부록 B).</summary>
    public enum LocationId
    {
        TrainingCenter = 0,
        Beach = 1,
        SubwayStation = 2,
        FitnessCenter = 3,
        NightMarket = 4,
        ShoppingMall = 5,
        OfficeTower = 6,
        RooftopClub = 7
    }

    /// <summary>장소의 시간대다. 한 판은 낮 → 저녁 → 밤 순서로 흐른다.</summary>
    public enum LocationTimeOfDay
    {
        Day = 0,
        Evening = 1,
        Night = 2
    }

    /// <summary>장소의 대표 목표다. 21일차에는 전부 정기 할당량으로 돌고, 나머지는 23일차부터 채운다.</summary>
    public enum LocationObjective
    {
        EssenceQuota = 0,
        GroupCarry = 1,
        Survival = 2,
        SpecialTarget = 3,
        Boss = 4,
        Stealth = 5
    }

    /// <summary>
    /// 장소 하나의 설정표다 (21일차).
    ///
    /// 장소를 만드는 코드는 하나이고, 이 표만 바꾸면 다른 장소가 된다.
    /// 층 · NPC 수 · 시간 · 목표 · 색 · 지도 위치를 담는다.
    /// </summary>
    [Serializable]
    public sealed class LocationDefinition
    {
        public LocationId Id;
        public string DisplayName;
        public LocationTimeOfDay TimeOfDay;
        public LocationObjective Objective;

        /// <summary>지도 정보 카드에 쓰는 한 줄 소개다.</summary>
        public string Summary;

        /// <summary>지도에 미리 보여주는 방해 세력 이름이다 (부록 C). 실제 등장은 22일차부터 순차 구현.</summary>
        public string DisruptorPreview;

        public int FloorCount = 1;

        /// <summary>층별 NPC 수에 곱한다. 1이면 연수원과 같은 밀도다.</summary>
        public float NpcDensity = 1f;

        public float TimeLimitSeconds = 180f;
        public int TargetEssence = 140;

        /// <summary>금태양 · 인기남을 배치하는지다.</summary>
        public bool HasRivals = true;

        /// <summary>튜토리얼 안내를 띄우는 장소인지다. 시작 구역만 켠다.</summary>
        public bool ShowsTutorial;

        /// <summary>벽 · 바닥 색에 곱하는 장소 색이다. 임시 장소를 서로 구분하는 용도다.</summary>
        public Color Tint = Color.white;

        /// <summary>지도 위 위치다. (0,0)이 왼쪽 아래, (1,1)이 오른쪽 위다.</summary>
        public Vector2 MapPosition;

        /// <summary>층 표지판 앞에 붙는 이름이다. 예: "교육동" → "교육동 1F".</summary>
        public string FloorPrefix = string.Empty;

        public string GetFloorLabel(
            int floorIndex)
        {
            string floor =
                FloorPlanLogic.GetLabel(
                    floorIndex);

            return string.IsNullOrEmpty(
                FloorPrefix)
                ? floor
                : $"{FloorPrefix} {floor}";
        }
    }

    /// <summary>
    /// 장소 8곳의 기본 표다 (21일차).
    ///
    /// 연수원을 뺀 7곳은 아직 색과 이름만 다른 임시 장소다.
    /// 환경 규칙과 방해 세력은 23일차부터 장소별로 채운다.
    /// 나중에 자산으로 덮어쓸 수 있게 다른 표들과 같은 Override 구조를 둔다.
    /// </summary>
    public static class LocationCatalog
    {
        public const LocationId StartLocation = LocationId.TrainingCenter;
        public const LocationId FinalLocation = LocationId.RooftopClub;

        public static LocationDefinition[] Override { get; set; }

        private static readonly LocationDefinition[] Defaults =
        {
            new LocationDefinition
            {
                Id = LocationId.TrainingCenter,
                DisplayName = "기업 연수원",
                TimeOfDay = LocationTimeOfDay.Day,
                Objective = LocationObjective.EssenceQuota,
                Summary = "신입 사원 합숙 교육 시설. 쉬는 시간마다 복도가 붐빈다",
                DisruptorPreview = "교육 조교 · 인사팀 평가관 · 금태양 · 인기남",
                FloorCount = 4,
                NpcDensity = 1f,
                TimeLimitSeconds = 180f,
                TargetEssence = 140,
                HasRivals = true,
                ShowsTutorial = true,
                Tint = Color.white,
                MapPosition = new Vector2(0.10f, 0.36f),
                FloorPrefix = "교육동"
            },
            new LocationDefinition
            {
                Id = LocationId.Beach,
                DisplayName = "해변가",
                TimeOfDay = LocationTimeOfDay.Day,
                Objective = LocationObjective.GroupCarry,
                Summary = "탁 트인 백사장. 숨을 곳은 파라솔 그늘뿐이다",
                DisruptorPreview = "라이프가드 · 헌팅남 · 라이프가드 반장 · 드론 촬영자",
                FloorCount = 1,
                NpcDensity = 1.2f,
                TimeLimitSeconds = 180f,
                TargetEssence = 150,
                HasRivals = true,
                Tint = new Color(1.00f, 0.94f, 0.76f),
                MapPosition = new Vector2(0.46f, 0.00f),
                FloorPrefix = "백사장"
            },
            new LocationDefinition
            {
                Id = LocationId.SubwayStation,
                DisplayName = "지하철 환승역",
                TimeOfDay = LocationTimeOfDay.Evening,
                Objective = LocationObjective.Survival,
                Summary = "퇴근길 환승역. 열차가 설 때마다 인파가 몰려든다",
                DisruptorPreview = "역무원 · 휴대폰만 보는 행인 · 전단지 알바 · 안내 방송실",
                FloorCount = 2,
                NpcDensity = 1f,
                TimeLimitSeconds = 170f,
                TargetEssence = 150,
                HasRivals = true,
                Tint = new Color(0.82f, 0.86f, 0.92f),
                MapPosition = new Vector2(0.46f, 0.36f),
                FloorPrefix = "승강장"
            },
            new LocationDefinition
            {
                Id = LocationId.FitnessCenter,
                DisplayName = "헬스장 · 스포츠센터",
                TimeOfDay = LocationTimeOfDay.Evening,
                Objective = LocationObjective.SpecialTarget,
                Summary = "운동 중인 사람은 최면이 빨리 차지만 충동도 빨리 오른다",
                DisruptorPreview = "퍼스널 트레이너 · 헬스 고인물 · 관장 · 수영장 코치",
                FloorCount = 2,
                NpcDensity = 0.9f,
                TimeLimitSeconds = 180f,
                TargetEssence = 160,
                HasRivals = true,
                Tint = new Color(0.80f, 0.94f, 0.88f),
                MapPosition = new Vector2(0.84f, 0.36f),
                FloorPrefix = "센터"
            },
            new LocationDefinition
            {
                Id = LocationId.NightMarket,
                DisplayName = "야시장",
                TimeOfDay = LocationTimeOfDay.Night,
                Objective = LocationObjective.EssenceQuota,
                Summary = "등불 아래 노점 골목. 좁고 붐비며 소매치기가 섞여 있다",
                DisruptorPreview = "호객꾼 · 취객 · 촬영팀 · 소매치기",
                FloorCount = 1,
                NpcDensity = 1.3f,
                TimeLimitSeconds = 180f,
                TargetEssence = 170,
                HasRivals = true,
                Tint = new Color(1.00f, 0.80f, 0.62f),
                MapPosition = new Vector2(0.84f, 0.70f),
                FloorPrefix = "골목"
            },
            new LocationDefinition
            {
                Id = LocationId.ShoppingMall,
                DisplayName = "쇼핑몰",
                TimeOfDay = LocationTimeOfDay.Evening,
                Objective = LocationObjective.Stealth,
                Summary = "에스컬레이터로 이어진 층. CCTV와 보안요원이 서로 연락한다",
                DisruptorPreview = "보안요원 · CCTV · 판촉 직원 · 보안팀장",
                FloorCount = 3,
                NpcDensity = 1f,
                TimeLimitSeconds = 190f,
                TargetEssence = 170,
                HasRivals = true,
                Tint = new Color(0.96f, 0.88f, 0.96f),
                MapPosition = new Vector2(0.46f, 0.70f),
                FloorPrefix = "몰"
            },
            new LocationDefinition
            {
                Id = LocationId.OfficeTower,
                DisplayName = "오피스 타워",
                TimeOfDay = LocationTimeOfDay.Night,
                Objective = LocationObjective.SpecialTarget,
                Summary = "야근 중인 고층 빌딩. 출입증 게이트를 순서대로 열어야 한다",
                DisruptorPreview = "꼰대 부장 · 야근 경비원 · 출입증 게이트 · 비서실장",
                FloorCount = 3,
                NpcDensity = 0.9f,
                TimeLimitSeconds = 200f,
                TargetEssence = 180,
                HasRivals = true,
                Tint = new Color(0.78f, 0.82f, 0.96f),
                MapPosition = new Vector2(0.10f, 0.70f),
                FloorPrefix = "타워"
            },
            new LocationDefinition
            {
                Id = LocationId.RooftopClub,
                DisplayName = "루프탑 클럽",
                TimeOfDay = LocationTimeOfDay.Night,
                Objective = LocationObjective.Boss,
                Summary = "라이벌 서큐버스의 영역. 음악 박자 위에서 세력전을 벌인다",
                DisruptorPreview = "바운서 · 클럽 MD · DJ · 라이벌 서큐버스",
                FloorCount = 2,
                NpcDensity = 1.1f,
                TimeLimitSeconds = 210f,
                TargetEssence = 200,
                HasRivals = true,
                Tint = new Color(0.92f, 0.74f, 0.96f),
                MapPosition = new Vector2(0.46f, 1.00f),
                FloorPrefix = "루프탑"
            }
        };

        public static IReadOnlyList<LocationDefinition> All =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Defaults;

        /// <summary>장소를 찾는다. 없으면 시작 장소를 돌려줘 게임이 멈추지 않게 한다.</summary>
        public static LocationDefinition Get(
            LocationId id)
        {
            IReadOnlyList<LocationDefinition> all = All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (all[i] != null &&
                    all[i].Id == id)
                {
                    return all[i];
                }
            }

            return Defaults[0];
        }

        public static string GetTimeLabel(
            LocationTimeOfDay time)
        {
            switch (time)
            {
                case LocationTimeOfDay.Day:
                    return "낮";

                case LocationTimeOfDay.Evening:
                    return "저녁";

                default:
                    return "밤";
            }
        }

        public static string GetObjectiveLabel(
            LocationObjective objective)
        {
            switch (objective)
            {
                case LocationObjective.GroupCarry:
                    return "동시 운반";

                case LocationObjective.Stealth:
                    return "잠입";

                case LocationObjective.Survival:
                    return "생존";

                case LocationObjective.SpecialTarget:
                    return "특수 대상 함락";

                case LocationObjective.Boss:
                    return "보스 함락";

                default:
                    return "정기 할당량";
            }
        }

        /// <summary>시간대를 나타내는 색이다. 지도 표식과 스테이지 색조에 쓴다.</summary>
        public static Color GetTimeColor(
            LocationTimeOfDay time)
        {
            switch (time)
            {
                case LocationTimeOfDay.Day:
                    return new Color(1.00f, 0.82f, 0.36f);

                case LocationTimeOfDay.Evening:
                    return new Color(1.00f, 0.52f, 0.40f);

                default:
                    return new Color(0.46f, 0.50f, 1.00f);
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Override = null;
        }
    }

    /// <summary>
    /// 지금 스테이지 씬이 어느 장소인지다. 스테이지 부트스트랩이 정하고, HUD · 결과 화면이 읽는다.
    /// </summary>
    public static class LocationContext
    {
        private static LocationDefinition _current;

        public static LocationDefinition Current =>
            _current ?? LocationCatalog.Get(LocationCatalog.StartLocation);

        public static void Set(
            LocationDefinition location)
        {
            _current = location;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
