using System.Collections.Generic;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>장소 안내의 적 한 줄이다 (35일차).</summary>
    public sealed class LocationGuideEnemy
    {
        public readonly string Name;
        public readonly string Effect;
        public readonly string Counter;

        public LocationGuideEnemy(
            string name,
            string effect,
            string counter)
        {
            Name = name;
            Effect = effect;
            Counter = counter;
        }
    }

    /// <summary>장소 하나의 안내다 (35일차). 지도 상세 창 · 스테이지 규칙 카드가 쓴다.</summary>
    public sealed class LocationGuide
    {
        public readonly LocationId Id;

        /// <summary>1(쉬움) ~ 5(어려움)다. 추천 장소를 고를 때도 쓴다.</summary>
        public readonly int Difficulty;

        public readonly string[] Rules;
        public readonly LocationGuideEnemy[] Enemies;
        public readonly string[] Tips;

        public LocationGuide(
            LocationId id,
            int difficulty,
            string[] rules,
            LocationGuideEnemy[] enemies,
            string[] tips)
        {
            Id = id;
            Difficulty = difficulty;
            Rules = rules ?? new string[0];
            Enemies = enemies ?? new LocationGuideEnemy[0];
            Tips = tips ?? new string[0];
        }
    }

    /// <summary>
    /// 장소 8곳의 규칙 · 적 · 팁 · 난이도 표다 (35일차).
    ///
    /// 문구는 23~26일차 장소 규칙 코드와 방해 세력 배치표를 보고 실제 동작대로 적었다.
    /// 규칙 코드를 바꾸면 이 표도 함께 고친다.
    /// </summary>
    public static class LocationGuideCatalog
    {
        public const int MinDifficulty = 1;
        public const int MaxDifficulty = 5;

        private static LocationGuideEnemy E(string name, string effect, string counter)
        {
            return new LocationGuideEnemy(name, effect, counter);
        }

        private static readonly Dictionary<LocationId, LocationGuide> Guides =
            new Dictionary<LocationId, LocationGuide>
            {
                [LocationId.TrainingCenter] = new LocationGuide(
                    LocationId.TrainingCenter,
                    1,
                    new[]
                    {
                        "쉬는 시간 종: 40초마다 8초간 사람들이 빨리 움직인다",
                        "자습실(2F~4F): 안에서 대시하면 경계도 +10",
                        "감시자는 최면 유지 · 대시만 문제 삼는다. 걷기는 괜찮다"
                    },
                    new[]
                    {
                        E("교육 조교", "모든 층을 순찰하며 앞쪽 부채꼴을 본다", "등 뒤에서 최면 · 파동으로 멍하게"),
                        E("인사팀 평가관 (3F·4F)", "근태 체크: 동행자 1명에 10초 표식, 그대로 회수하면 정기 −20%", "표식이 끝난 뒤 회수 · 시야 밖으로 빼기")
                    },
                    new[]
                    {
                        "자습실은 걸어서 지나가세요",
                        "평가관이 반대쪽을 볼 때 최면하세요"
                    }),

                [LocationId.Beach] = new LocationGuide(
                    LocationId.Beach,
                    2,
                    new[]
                    {
                        "동시 운반: 3명 이상 한 번에 회수하면 정기 ×1.2, 1~2명이면 ×0.5",
                        "밀물: 30초마다 예고 뒤 8초간 물가 줄이 잠기고 위로 밀려난다",
                        "파라솔 그늘 안은 라이프가드에게 보이지 않는다(드론은 보임)"
                    },
                    new[]
                    {
                        E("라이프가드", "순찰하며 넓은 시야로 본다", "파라솔 그늘 안에서 최면"),
                        E("헌팅남", "무리 맨 뒤 동행자에게 붙어 데려가려 한다", "그 동행자 곁에 붙어 있기 · 파동"),
                        E("라이프가드 반장 (망루)", "호루라기: 주변 중립 NPC 8초간 최면 −50%, 경계도 +25", "반장의 사각에서 · 경보 반경 밖 NPC 노리기"),
                        E("드론 촬영자", "드론 원 안에 3초 있으면 6초간 추적 촬영(경계도 상승)", "원 밖으로 · 파라솔로 끊기 · 촬영자에게 파동이면 드론 착륙")
                    },
                    new[]
                    {
                        "3명 이상 모아서 한 번에 회수하세요",
                        "밀물 예고가 뜨면 위쪽 줄로 올라가세요"
                    }),

                [LocationId.SubwayStation] = new LocationGuide(
                    LocationId.SubwayStation,
                    2,
                    new[]
                    {
                        "열차: 45초마다 도착해 층마다 휴대폰 행인이 내린다",
                        "개찰구: 동행 5명 이상이면 한 명씩만 통과(문이 열린 동안은 통과)",
                        "비상 경계에도 증원은 없고 회수 지점만 10초 잠긴다"
                    },
                    new[]
                    {
                        E("역무원", "동행 5명 이상이면 반대편 동행자를 세워 둔다", "4명 이하로 나눠 통과 · 파동"),
                        E("전단지 알바", "바로 앞 동행자를 2초 붙잡는다", "반대편 줄로 지나가기"),
                        E("안내 방송실 (1F)", "승강장 변경: 5초간 인파가 한쪽으로 흐르고 느려진다", "파동으로 버티기"),
                        E("휴대폰 행인", "직진하며 부딪친 동행자를 옆 줄로 밀어낸다", "줄 바꾸기 · 대시로 밀치기")
                    },
                    new[]
                    {
                        "개찰구는 열차 문이 열릴 때 통과하세요",
                        "동행자를 최소 1명은 끝까지 지키세요"
                    }),

                [LocationId.FitnessCenter] = new LocationGuide(
                    LocationId.FitnessCenter,
                    3,
                    new[]
                    {
                        "운동 구역: 최면 ×1.4, 동행자 충동 ×1.5",
                        "요가실(1F): 최면 ×0.8, 충동이 오르지 않는다",
                        "★ 대회 앞둔 선수: 최면이 느리지만 회수하면 정기 +60"
                    },
                    new[]
                    {
                        E("퍼스널 트레이너", "담당 회원 최면을 보면 달려와 게이지를 절반으로", "멀리 있거나 반대쪽을 볼 때 · 파동"),
                        E("헬스 고인물 (1F)", "\"으랏차!\" 자세 뒤 밀어붙여 휘청이게 한다", "자세 중 대시로 받아치면 퇴장"),
                        E("관장 (2F)", "단체 PT 10초: 충동 ×1.6", "요가실로 피하기"),
                        E("수영장 코치 (2F)", "수영장 최면 중 \"전원 입수\": 게이지 0, 5초간 최면 불가", "확성기를 들기 전에 끝내기 · 파동")
                    },
                    new[]
                    {
                        "동행자는 요가실에 두고 운동 구역에서 최면하세요",
                        "단체 PT 경보가 뜨면 요가실로 피하세요"
                    }),

                [LocationId.NightMarket] = new LocationGuide(
                    LocationId.NightMarket,
                    3,
                    new[]
                    {
                        "어둠: 등불 밖에서는 최면 사거리 ×0.7",
                        "좁은 골목이라 동행자가 한 줄로 따라온다",
                        "비상 경계에도 증원은 없고 회수 지점만 10초 잠긴다"
                    },
                    new[]
                    {
                        E("호객꾼", "노점 앞을 지나는 동행자를 세워 둔다. 6초 안에 못 찾으면 잃는다", "붙잡힌 동행자 옆에 1초 붙어 있기"),
                        E("소매치기", "등 뒤에서 회수 전 정기의 30%를 훔친다", "\"등 뒤!\" 예고에 대시 · 20초 안에 잡으면 +50"),
                        E("촬영팀", "조명 안에서 최면하면 경계도 +30, 8초간 따라온다", "조명 밖에서 활동 · 파동이면 조명 꺼짐"),
                        E("취객", "동행자와 부딪치면 충동 +15", "대시로 밀쳐내기")
                    },
                    new[]
                    {
                        "등불 아래에서 최면하세요",
                        "\"✋ 등 뒤!\"가 뜨면 바로 대시하세요"
                    }),

                [LocationId.ShoppingMall] = new LocationGuide(
                    LocationId.ShoppingMall,
                    4,
                    new[]
                    {
                        "잠입: 한 번도 들키지 않고 회수하면 정기 ×1.3",
                        "폐점 방송: 제한 시간 70%부터 시간이 ×1.3 빨리 흐른다",
                        "기둥 · 진열대 안은 CCTV와 보안요원에게 보이지 않는다",
                        "[보안실] 직원을 최면하면 그 층 CCTV가 30초 꺼진다"
                    },
                    new[]
                    {
                        E("보안요원", "한 명이 발견하면 같은 층 요원이 모두 달려온다", "파동을 맞히면 그 층 무전이 5초 끊김"),
                        E("CCTV", "좌우를 번갈아 보며 무전을 보낸다", "기둥 뒤에 숨기 · 보안실 직원 최면"),
                        E("판촉 직원 (1F)", "앞을 지나는 동행자를 3초 붙잡는다", "반대편 줄로 지나가기"),
                        E("보안팀장 (2F)", "경계도 60 이상이면 플레이어 층 셔터 2곳을 12초 닫는다", "경계도 60 아래 유지 · 경광등이 켜지면 셔터 밖으로")
                    },
                    new[]
                    {
                        "층마다 보안실 직원을 먼저 최면하세요",
                        "폐점 방송 전에 목표를 채우세요"
                    }),

                [LocationId.OfficeTower] = new LocationGuide(
                    LocationId.OfficeTower,
                    4,
                    new[]
                    {
                        "출입증 게이트: [출입증] 직원을 데려와야 위층이 열린다(비상계단은 경계도 +20)",
                        "정전: 두 번, 8초간 불이 꺼진다. 최면 사거리 ×0.7",
                        "엘리베이터 정원 4명: 나머지는 아래층에서 20초 기다린다",
                        "★ 대표 비서(최상층): 회수하면 정기 +80"
                    },
                    new[]
                    {
                        E("꼰대 부장", "반경 4m 안의 최면 게이지를 모두 0으로", "25초마다 탕비실에서 쉴 때 최면"),
                        E("야근 경비원 (2F·3F)", "좁고 긴 시야, 정전 중에도 본다", "시야 밖에서 최면"),
                        E("사내 인기남 (1F·2F)", "동행자를 빼앗는다. 탕비실 근처에서 ×2", "탕비실 근처 피하기 · 무리를 촘촘히"),
                        E("비서실장 (최상층)", "직원 동행자 최대 2명을 회의실로 12초 데려간다", "방문객 위주로 데리고 다니기")
                    },
                    new[]
                    {
                        "정전 때는 경비원만 피하면 됩니다",
                        "층을 옮길 때는 동행자를 4명 이하로 맞추세요"
                    }),

                [LocationId.RooftopClub] = new LocationGuide(
                    LocationId.RooftopClub,
                    5,
                    new[]
                    {
                        "세력이 라이벌의 1.2배 이상이어야 보호막(3장)을 공격할 수 있다",
                        "드롭: 박자마다 동행자 충동 상승, 직후 1.5초는 최면 ×1.5",
                        "VIP 입구: 동행 6명을 넘으면 위층에 못 들어간다",
                        "정신력이 0이 되면 동행자 2명을 잃고, 30초 안에 두 번이면 패배"
                    },
                    new[]
                    {
                        E("바운서 (1F)", "동행 6명 초과 시 입장을 막는다", "[VIP] 게스트와 함께 · 파동"),
                        E("클럽 MD", "동행자를 빼앗는다. 드롭 직후 ×3", "드롭 직후엔 무리 곁을 지키기"),
                        E("DJ (2F)", "30초마다 템포를 올려 드롭이 잦아진다", "파동을 맞히면 템포 한 단계 내려감"),
                        E("라이벌 서큐버스 (보스)", "역최면 시선 · 매혹 파동 · 샴페인 타워 · 분신 · VIP 구역", "줄 바꾸기 · [바텐더] 최면 · 그림자 있는 쪽이 진짜")
                    },
                    new[]
                    {
                        "1F에서 [VIP] 게스트를 먼저 확보하세요",
                        "드롭 직후 1.5초에 몰아서 최면하세요"
                    })
            };

        private static readonly LocationGuide Empty =
            new LocationGuide(
                LocationId.TrainingCenter,
                MinDifficulty,
                new[] { "특별한 규칙이 없습니다" },
                new LocationGuideEnemy[0],
                new[] { "동행자를 모아 회수 지점으로 데려가세요" });

        public static bool Has(
            LocationId id)
        {
            return Guides.ContainsKey(id);
        }

        public static LocationGuide Get(
            LocationId id)
        {
            return Guides.TryGetValue(id, out LocationGuide guide)
                ? guide
                : Empty;
        }

        /// <summary>
        /// 경쟁자(금태양 · 인기남)가 실제로 나오는지다.
        /// 루프탑 클럽은 표에 켜져 있어도 부트스트랩이 만들지 않는다(라이벌 서큐버스가 대신한다).
        /// </summary>
        public static bool RivalsAppear(
            LocationDefinition location)
        {
            return location != null &&
                   location.HasRivals &&
                   location.Id != LocationId.RooftopClub;
        }
    }
}
