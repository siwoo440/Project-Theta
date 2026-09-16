using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>방해 세력의 역할 7종이다 (부록 C.2.3). 장소별 캐릭터는 역할에 외형 · 수치만 다르게 입힌다.</summary>
    public enum DisruptorRole
    {
        Watcher = 0,
        Rescuer = 1,
        Contester = 2,
        Blocker = 3,
        Duelist = 4,
        Thief = 5,
        Gate = 6
    }

    /// <summary>방해 세력 종류다. 22일차 기업 연수원 2종, 23일차 해변가 · 야시장 7종.</summary>
    public enum DisruptorKind
    {
        TrainingAssistant = 0,
        HrEvaluator = 1,

        // 해변가 (23일차)
        Lifeguard = 2,
        BeachHunter = 3,
        LifeguardChief = 4,

        // 야시장 (23일차)
        StallTout = 5,
        Drunkard = 6,
        FilmCrew = 7,
        Pickpocket = 8,

        // 지하철 환승역 (24일차)
        StationAttendant = 9,
        PhoneCommuter = 10,
        FlyerStaff = 11,
        PlatformAnnouncer = 12,

        // 헬스장 (24일차)
        PersonalTrainer = 13,
        GymVeteran = 14,
        GymDirector = 15,
        SwimCoach = 16,

        // 해변가 (24일차, 23일차에서 넘어옴)
        DronePhotographer = 17,

        // 쇼핑몰 (25일차)
        SecurityGuard = 18,
        SecurityCamera = 19,
        PromoStaff = 20,
        SecurityChief = 21,

        // 오피스 타워 (25일차)
        OfficeManager = 22,
        NightGuard = 23,
        OfficeRomeo = 24,
        ChiefSecretary = 25
    }

    /// <summary>특수 능력 종류다 (부록 C.4).</summary>
    public enum SpecialAbilityKind
    {
        None = 0,
        AttendanceCheck = 1,
        WhistleAlarm = 2,
        LiveBroadcast = 3,
        Pickpocket = 4,
        PlatformChange = 5,
        GroupPt = 6,
        AllIn = 7,
        DroneTracking = 8,
        ShutterLock = 9,
        EmergencyMeeting = 10
    }

    /// <summary>
    /// 방해 세력 한 종류의 설정표다 (22일차).
    /// 장소 표처럼 이 표만 바꾸면 다른 캐릭터가 된다.
    /// </summary>
    [Serializable]
    public sealed class DisruptorProfile
    {
        public DisruptorKind Kind;
        public DisruptorRole Role;
        public string DisplayName;

        /// <summary>금색 이름표를 달고 특수 능력을 쓰는 개체인지다.</summary>
        public bool IsSpecial;

        public SpecialAbilityKind Ability;

        /// <summary>임시 외형: 기존 캐릭터 스프라이트 폴더와 색이다. 전용 그림은 29일차 이후 교체.</summary>
        public string SpriteRoot = "Characters/Geumtaeyang";
        public Color Tint = Color.white;

        /// <summary>시야 부채꼴 전체 각도의 절반이다. 30이면 60° 부채꼴이다.</summary>
        public float SightHalfAngle = 30f;
        public float SightRange = 6f;

        public float MoveSpeed = 2.4f;

        /// <summary>순찰하지 않고 한 자리에 서서 좌우를 번갈아 본다.</summary>
        public bool Stationary;

        /// <summary>제자리 개체가 바라보는 방향을 바꾸는 간격이다.</summary>
        public float LookAroundSeconds = 5f;

        /// <summary>발견한 뒤 쫓아가는 시간이다.</summary>
        public float ChaseSeconds = 3f;

        public float TelegraphSeconds = 1.2f;
        public float CooldownSeconds = 12f;

        /// <summary>파라솔 그늘 안도 본다 (23일차). 드론처럼 위에서 보는 개체용이다.</summary>
        public bool IgnoresShade;

        /// <summary>능력을 쓰기 전까지 이름표를 숨긴다 (23일차, 소매치기).</summary>
        public bool HiddenUntilFired;

        /// <summary>사람이 아닌 고정 사물이다 (24일차, 안내 방송실). 캐릭터 그림 대신 상자를 그린다.</summary>
        public bool IsObject;

        /// <summary>정전 중에도 본다 (25일차, 손전등을 든 야근 경비원).</summary>
        public bool SeesInBlackout;
    }

    /// <summary>한 장소에 어떤 방해 세력을 몇 층에 둘지다.</summary>
    public struct DisruptorPlacement
    {
        public DisruptorKind Kind;
        public int Floor;
        public float X;

        /// <summary>같은 종류 안에서의 순번이다. 헌팅남 2인조가 서로 다른 동행자를 노리는 데 쓴다.</summary>
        public int Variant;

        public DisruptorPlacement(
            DisruptorKind kind,
            int floor,
            float x,
            int variant = 0)
        {
            Kind = kind;
            Floor = floor;
            X = x;
            Variant = variant;
        }
    }

    /// <summary>
    /// 방해 세력 기본 표와 장소별 배치다 (22일차).
    /// 다른 밸런스 표처럼 Override로 자산에서 덮어쓸 수 있다.
    /// </summary>
    public static class DisruptorCatalog
    {
        public static DisruptorProfile[] Override { get; set; }

        private static readonly DisruptorProfile[] Defaults =
        {
            new DisruptorProfile
            {
                Kind = DisruptorKind.TrainingAssistant,
                Role = DisruptorRole.Watcher,
                DisplayName = "교육 조교",
                IsSpecial = false,
                Ability = SpecialAbilityKind.None,
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.70f, 0.78f, 0.95f),
                SightHalfAngle = 30f,
                SightRange = 6f,
                MoveSpeed = 2.4f,
                Stationary = false,
                ChaseSeconds = 3f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.HrEvaluator,
                Role = DisruptorRole.Watcher,
                DisplayName = "인사팀 평가관",
                IsSpecial = true,
                Ability = SpecialAbilityKind.AttendanceCheck,
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.72f, 0.72f, 0.80f),
                SightHalfAngle = 45f,
                SightRange = 8f,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 5f,
                ChaseSeconds = 0f,
                TelegraphSeconds = 1.2f,
                CooldownSeconds = 12f
            },

            // 해변가 ------------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.Lifeguard,
                Role = DisruptorRole.Watcher,
                DisplayName = "라이프가드",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(1.00f, 0.62f, 0.55f),
                SightHalfAngle = 35f,
                SightRange = 9f,
                MoveSpeed = 2.6f,
                ChaseSeconds = 3f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.BeachHunter,
                Role = DisruptorRole.Contester,
                DisplayName = "헌팅남",
                SpriteRoot = "Characters/PopularGuy",
                Tint = new Color(0.95f, 0.78f, 0.58f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 2.0f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.LifeguardChief,
                Role = DisruptorRole.Watcher,
                DisplayName = "라이프가드 반장",
                IsSpecial = true,
                Ability = SpecialAbilityKind.WhistleAlarm,
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(1.00f, 0.45f, 0.40f),
                SightHalfAngle = 60f,
                SightRange = 12f,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 6f,
                ChaseSeconds = 0f,
                TelegraphSeconds = 1.0f,
                CooldownSeconds = 15f
            },

            // 야시장 ------------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.StallTout,
                Role = DisruptorRole.Contester,
                DisplayName = "호객꾼",
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(1.00f, 0.80f, 0.55f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 4f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.Drunkard,
                Role = DisruptorRole.Blocker,
                DisplayName = "취객",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.85f, 0.70f, 0.75f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 1.0f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.FilmCrew,
                Role = DisruptorRole.Watcher,
                DisplayName = "촬영팀",
                IsSpecial = true,
                Ability = SpecialAbilityKind.LiveBroadcast,
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(1.00f, 0.92f, 0.70f),
                // 부채꼴 대신 들고 다니는 조명으로 본다. 감시 부채꼴은 쓰지 않는다.
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 1.4f,
                ChaseSeconds = 0f,
                TelegraphSeconds = 1.0f,
                CooldownSeconds = 18f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.Pickpocket,
                Role = DisruptorRole.Thief,
                DisplayName = "소매치기",
                IsSpecial = true,
                Ability = SpecialAbilityKind.Pickpocket,
                HiddenUntilFired = true,
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.62f, 0.62f, 0.70f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 2.2f,
                TelegraphSeconds = 1.0f,
                CooldownSeconds = 8f
            },

            // 지하철 환승역 ------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.StationAttendant,
                Role = DisruptorRole.Gate,
                DisplayName = "역무원",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.55f, 0.70f, 0.60f),
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 4f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.PhoneCommuter,
                Role = DisruptorRole.Blocker,
                DisplayName = "휴대폰 행인",
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.62f, 0.66f, 0.74f),
                // 공통 순찰 대신 행인 역할이 직접 직진시킨다.
                MoveSpeed = 1.2f,
                Stationary = true,
                LookAroundSeconds = 999f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.FlyerStaff,
                Role = DisruptorRole.Contester,
                DisplayName = "전단지 알바",
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.90f, 0.95f, 0.60f),
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 3f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.PlatformAnnouncer,
                Role = DisruptorRole.Gate,
                DisplayName = "안내 방송실",
                IsSpecial = true,
                IsObject = true,
                Ability = SpecialAbilityKind.PlatformChange,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 999f,
                TelegraphSeconds = 3f,
                CooldownSeconds = 42f
            },

            // 헬스장 ------------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.PersonalTrainer,
                Role = DisruptorRole.Rescuer,
                DisplayName = "퍼스널 트레이너",
                SpriteRoot = "Characters/PopularGuy",
                Tint = new Color(0.60f, 0.85f, 0.95f),
                // 이동 목표로 달려갈 때 1.5배라 실제 달리기는 3.2m/s다.
                MoveSpeed = 2.13f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.GymVeteran,
                Role = DisruptorRole.Duelist,
                DisplayName = "헬스 고인물",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.85f, 0.60f, 0.45f),
                MoveSpeed = 1.34f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.GymDirector,
                Role = DisruptorRole.Duelist,
                DisplayName = "관장",
                IsSpecial = true,
                Ability = SpecialAbilityKind.GroupPt,
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(1.00f, 0.72f, 0.40f),
                MoveSpeed = 1.2f,
                TelegraphSeconds = 1.5f,
                CooldownSeconds = 25f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.SwimCoach,
                Role = DisruptorRole.Rescuer,
                DisplayName = "수영장 코치",
                IsSpecial = true,
                Ability = SpecialAbilityKind.AllIn,
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.55f, 0.75f, 1.00f),
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 5f,
                TelegraphSeconds = 1.2f,
                CooldownSeconds = 20f
            },

            // 해변가 드론 --------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.DronePhotographer,
                Role = DisruptorRole.Watcher,
                DisplayName = "드론 촬영자",
                IsSpecial = true,
                IgnoresShade = true,
                Ability = SpecialAbilityKind.DroneTracking,
                // 부채꼴 대신 드론의 원형 시야로 본다.
                SightHalfAngle = 0f,
                SightRange = 0f,
                SpriteRoot = "Characters/PopularGuy",
                Tint = new Color(0.80f, 0.95f, 0.80f),
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 7f,
                TelegraphSeconds = 1.5f,
                CooldownSeconds = 10f
            }
        };

        /// <summary>지하철 개찰구 자리다. 층마다 가운데 하나다.</summary>
        public const float SubwayGateX = 0f;

        /// <summary>해변가 드론 촬영자 자리와 드론 궤도 중심이다.</summary>
        public const float BeachDroneX = -6.5f;

        /// <summary>야시장 노점 자리다. 호객꾼과 노점 그림이 같은 자리를 쓴다.</summary>
        public static readonly float[] NightMarketStallX =
        {
            -9f, -3f, 3f, 9f
        };

        /// <summary>해변가 파라솔 자리다. 그늘 그림과 판정이 같은 자리를 쓴다.</summary>
        public static readonly float[] BeachParasolX =
        {
            -10f, -4f, 5.5f, 11f
        };

        /// <summary>해변가 망루 자리다. 라이프가드 반장이 선다.</summary>
        public const float BeachTowerX = 0.5f;

        /// <summary>쇼핑몰 · 오피스 타워 기본 표다 (25일차). 기본 표 뒤에 이어 붙는다.</summary>
        private static readonly DisruptorProfile[] MallOfficeDefaults =
        {
            // 쇼핑몰 ------------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.SecurityGuard,
                Role = DisruptorRole.Watcher,
                DisplayName = "보안요원",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.45f, 0.48f, 0.58f),
                SightHalfAngle = 30f,
                SightRange = 7f,
                // 추적 · 무전 출동은 1.5배라 3.4m/s에 가깝다.
                MoveSpeed = 2.5f,
                ChaseSeconds = 3f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.SecurityCamera,
                Role = DisruptorRole.Watcher,
                DisplayName = "CCTV",
                IsObject = true,
                SightHalfAngle = 25f,
                SightRange = 6f,
                MoveSpeed = 0f,
                Stationary = true,
                // 6초 주기로 좌우를 번갈아 본다.
                LookAroundSeconds = 3f,
                ChaseSeconds = 0f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.PromoStaff,
                Role = DisruptorRole.Contester,
                DisplayName = "판촉 직원",
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(1.00f, 0.75f, 0.85f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 4f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.SecurityChief,
                Role = DisruptorRole.Gate,
                DisplayName = "보안팀장",
                IsSpecial = true,
                Ability = SpecialAbilityKind.ShutterLock,
                SpriteRoot = "Characters/PopularGuy",
                Tint = new Color(0.40f, 0.42f, 0.55f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 1.6f,
                TelegraphSeconds = 1.5f,
                CooldownSeconds = 30f
            },

            // 오피스 타워 --------------------------------------------------
            new DisruptorProfile
            {
                Kind = DisruptorKind.OfficeManager,
                Role = DisruptorRole.Rescuer,
                DisplayName = "꼰대 부장",
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.75f, 0.68f, 0.60f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 1.4f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.NightGuard,
                Role = DisruptorRole.Watcher,
                DisplayName = "야근 경비원",
                SeesInBlackout = true,
                SpriteRoot = "Characters/Geumtaeyang",
                Tint = new Color(0.40f, 0.45f, 0.65f),
                SightHalfAngle = 20f,
                SightRange = 8f,
                MoveSpeed = 2.2f,
                ChaseSeconds = 3f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.OfficeRomeo,
                Role = DisruptorRole.Contester,
                DisplayName = "사내 인기남",
                SpriteRoot = "Characters/PopularGuy",
                Tint = new Color(0.80f, 0.85f, 1.00f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 2.0f
            },
            new DisruptorProfile
            {
                Kind = DisruptorKind.ChiefSecretary,
                Role = DisruptorRole.Gate,
                DisplayName = "비서실장",
                IsSpecial = true,
                Ability = SpecialAbilityKind.EmergencyMeeting,
                SpriteRoot = "Characters/NPC_Female",
                Tint = new Color(0.70f, 0.70f, 0.85f),
                SightHalfAngle = 0f,
                SightRange = 0f,
                MoveSpeed = 0f,
                Stationary = true,
                LookAroundSeconds = 6f,
                TelegraphSeconds = 1.5f,
                CooldownSeconds = 35f
            }
        };

        private static DisruptorProfile[] _builtIn;

        /// <summary>기본 표 전체다. 날마다 붙인 표를 한 번만 이어 붙인다.</summary>
        private static DisruptorProfile[] BuiltIn
        {
            get
            {
                if (_builtIn == null)
                {
                    _builtIn = new DisruptorProfile[Defaults.Length + MallOfficeDefaults.Length];
                    Defaults.CopyTo(_builtIn, 0);
                    MallOfficeDefaults.CopyTo(_builtIn, Defaults.Length);
                }

                return _builtIn;
            }
        }

        public static IReadOnlyList<DisruptorProfile> All =>
            Override != null &&
            Override.Length > 0
                ? Override
                : BuiltIn;

        public static DisruptorProfile Get(
            DisruptorKind kind)
        {
            IReadOnlyList<DisruptorProfile> all = All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (all[i] != null &&
                    all[i].Kind == kind)
                {
                    return all[i];
                }
            }

            return Defaults[0];
        }

        /// <summary>
        /// 장소별 배치다 (부록 C.5).
        ///   연수원  교육 조교 층마다 1명, 인사팀 평가관 3F · 4F에 1명씩. 1F는 튜토리얼 층이라 조교만 둔다.
        ///   해변가  라이프가드 · 헌팅남 · 라이프가드 반장(망루) · 드론 촬영자
        ///   야시장  호객꾼 · 소매치기 · 촬영팀 · 취객
        ///
        /// 24일차: 한 층에 최대 4명이다 (<see cref="PopulationLogic.MaxPerFloor"/>, 사물 제외).
        /// 판이 시작되면 시간에 따라 배치표 <b>앞쪽부터</b> 한 명씩 등장하므로, 먼저 보여 줄 적을 앞에 둔다.
        /// </summary>
        public static List<DisruptorPlacement> GetPlacements(
            LocationId location,
            int floorCount)
        {
            List<DisruptorPlacement> result =
                new List<DisruptorPlacement>();

            switch (location)
            {
                case LocationId.TrainingCenter:
                    AddTrainingCenter(result, floorCount);
                    break;

                case LocationId.Beach:
                    AddBeach(result);
                    break;

                case LocationId.NightMarket:
                    AddNightMarket(result);
                    break;

                case LocationId.SubwayStation:
                    AddSubway(result, floorCount);
                    break;

                case LocationId.FitnessCenter:
                    AddFitnessCenter(result, floorCount);
                    break;

                case LocationId.ShoppingMall:
                    AddMall(result, floorCount);
                    break;

                case LocationId.OfficeTower:
                    AddOffice(result, floorCount);
                    break;
            }

            return result;
        }

        /// <summary>
        /// 비상 때 증원으로 보낼 종류다. 증원을 보내지 않는 장소는 null이다.
        /// 야시장에는 감시 순찰자가 없어 증원 대신 회수 지점 잠금만 걸린다.
        /// </summary>
        public static DisruptorKind? GetReinforcementKind(
            LocationId location)
        {
            switch (location)
            {
                case LocationId.TrainingCenter:
                    return DisruptorKind.TrainingAssistant;

                case LocationId.Beach:
                    return DisruptorKind.Lifeguard;

                case LocationId.FitnessCenter:
                    return DisruptorKind.PersonalTrainer;

                case LocationId.ShoppingMall:
                    return DisruptorKind.SecurityGuard;

                case LocationId.OfficeTower:
                    return DisruptorKind.NightGuard;

                default:
                    return null;
            }
        }

        private static void AddBeach(
            List<DisruptorPlacement> result)
        {
            // 층당 4명 제한으로 라이프가드 · 헌팅남은 한 명씩만 둔다.
            result.Add(new DisruptorPlacement(DisruptorKind.Lifeguard, 0, 8f));
            result.Add(new DisruptorPlacement(DisruptorKind.BeachHunter, 0, 2.5f, 0));
            result.Add(new DisruptorPlacement(DisruptorKind.LifeguardChief, 0, BeachTowerX));
            result.Add(new DisruptorPlacement(DisruptorKind.DronePhotographer, 0, BeachDroneX));
        }

        /// <summary>
        /// 지하철: 층마다 개찰구 역무원, 층마다 전단지 알바, 1F 안내 방송실.
        /// 휴대폰 행인은 배치하지 않고 열차가 도착할 때마다 내린다.
        /// </summary>
        private static void AddSubway(
            List<DisruptorPlacement> result,
            int floorCount)
        {
            int floors = Math.Max(1, floorCount);

            for (int floor = 0;
                 floor < floors;
                 floor++)
            {
                result.Add(new DisruptorPlacement(DisruptorKind.StationAttendant, floor, SubwayGateX + 1.4f));
                result.Add(new DisruptorPlacement(DisruptorKind.FlyerStaff, floor, floor % 2 == 0 ? -7f : 7f));
            }

            result.Add(new DisruptorPlacement(DisruptorKind.PlatformAnnouncer, 0, 6f));
        }

        /// <summary>
        /// 헬스장: 트레이너 3명(운동 구역 · 요가실), 고인물 1명, 관장 1명(2F), 수영장 코치 1명(2F 수영장).
        /// 구역 자리는 <see cref="Stage.Locations.GymLayout"/>와 맞춘다.
        /// </summary>
        private static void AddFitnessCenter(
            List<DisruptorPlacement> result,
            int floorCount)
        {
            int top = Math.Max(0, floorCount - 1);

            result.Add(new DisruptorPlacement(DisruptorKind.PersonalTrainer, 0, -5f));
            result.Add(new DisruptorPlacement(DisruptorKind.GymVeteran, 0, 0.5f));
            result.Add(new DisruptorPlacement(DisruptorKind.PersonalTrainer, 0, 5f));
            result.Add(new DisruptorPlacement(DisruptorKind.PersonalTrainer, top, 7f));
            result.Add(new DisruptorPlacement(DisruptorKind.SwimCoach, top, -4f));
            result.Add(new DisruptorPlacement(DisruptorKind.GymDirector, top, 2.5f));
        }

        /// <summary>
        /// 쇼핑몰: 층마다 보안요원 2명(1F는 판촉 직원 포함), 2F 보안팀장, 층마다 CCTV(사물, 인원에 세지 않음).
        /// </summary>
        private static void AddMall(
            List<DisruptorPlacement> result,
            int floorCount)
        {
            int floors = Math.Max(1, floorCount);

            for (int floor = 0;
                 floor < floors;
                 floor++)
            {
                float[] cameras = Stage.Locations.MallLayout.GetCameraX(floor);

                for (int i = 0;
                     i < cameras.Length;
                     i++)
                {
                    result.Add(new DisruptorPlacement(DisruptorKind.SecurityCamera, floor, cameras[i]));
                }

                switch (floor)
                {
                    case 0:
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, -4f));
                        result.Add(new DisruptorPlacement(DisruptorKind.PromoStaff, floor, 2f));
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, 10f));
                        break;

                    case 1:
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, -6f));
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityChief, floor, 1f));
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, 7f));
                        break;

                    default:
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, -2f));
                        result.Add(new DisruptorPlacement(DisruptorKind.SecurityGuard, floor, 8f));
                        break;
                }
            }
        }

        /// <summary>
        /// 오피스 타워: 층마다 꼰대 부장, 사내 인기남(1F · 2F), 야근 경비원(2F · 3F), 최상층 비서실장.
        /// 출입증 게이트는 방해 세력이 아니라 장소 규칙이다.
        /// </summary>
        private static void AddOffice(
            List<DisruptorPlacement> result,
            int floorCount)
        {
            int floors = Math.Max(1, floorCount);
            int top = floors - 1;

            for (int floor = 0;
                 floor < floors;
                 floor++)
            {
                result.Add(new DisruptorPlacement(DisruptorKind.OfficeManager, floor, floor % 2 == 0 ? -2f : 3f));

                if (floor >= 1)
                {
                    result.Add(new DisruptorPlacement(DisruptorKind.NightGuard, floor, floor % 2 == 0 ? 4f : -5f));
                }

                if (floor < top ||
                    floors == 1)
                {
                    result.Add(new DisruptorPlacement(DisruptorKind.OfficeRomeo, floor, floor % 2 == 0 ? 4f : -9f));
                }
            }

            result.Add(new DisruptorPlacement(DisruptorKind.ChiefSecretary, top, 10.5f));
        }

        private static void AddNightMarket(
            List<DisruptorPlacement> result)
        {
            // 층당 4명 제한으로 호객꾼은 두 번째 노점 한 곳, 취객은 한 명만 둔다.
            result.Add(new DisruptorPlacement(DisruptorKind.StallTout, 0, NightMarketStallX[1]));
            result.Add(new DisruptorPlacement(DisruptorKind.Pickpocket, 0, -11f));
            result.Add(new DisruptorPlacement(DisruptorKind.FilmCrew, 0, 0f));
            result.Add(new DisruptorPlacement(DisruptorKind.Drunkard, 0, 6f));
        }

        private static void AddTrainingCenter(
            List<DisruptorPlacement> result,
            int floorCount)
        {
            int floors =
                Math.Max(1, floorCount);

            for (int floor = 0;
                 floor < floors;
                 floor++)
            {
                // 층마다 순찰 시작 위치를 엇갈려 같은 자리에 겹쳐 보이지 않게 한다.
                result.Add(
                    new DisruptorPlacement(
                        DisruptorKind.TrainingAssistant,
                        floor,
                        floor % 2 == 0 ? -2f : 4f));

                if (floor >= 2)
                {
                    result.Add(
                        new DisruptorPlacement(
                            DisruptorKind.HrEvaluator,
                            floor,
                            floor % 2 == 0 ? 6.5f : -5.5f));
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Override = null;
            _builtIn = null;
        }
    }
}
