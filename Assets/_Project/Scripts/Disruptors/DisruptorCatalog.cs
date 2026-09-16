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
        Pickpocket = 8
    }

    /// <summary>특수 능력 종류다 (부록 C.4).</summary>
    public enum SpecialAbilityKind
    {
        None = 0,
        AttendanceCheck = 1,
        WhistleAlarm = 2,
        LiveBroadcast = 3,
        Pickpocket = 4
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
            }
        };

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

        public static IReadOnlyList<DisruptorProfile> All =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Defaults;

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
        ///   해변가  라이프가드 2명, 헌팅남 1조(2명), 라이프가드 반장 1명(망루)
        ///   야시장  호객꾼 노점마다 1명, 취객 2명, 촬영팀 1명, 소매치기 1명
        /// 드론 촬영자는 24일차로 넘겼다.
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

                default:
                    return null;
            }
        }

        private static void AddBeach(
            List<DisruptorPlacement> result)
        {
            result.Add(new DisruptorPlacement(DisruptorKind.Lifeguard, 0, -8f));
            result.Add(new DisruptorPlacement(DisruptorKind.Lifeguard, 0, 8f));
            result.Add(new DisruptorPlacement(DisruptorKind.BeachHunter, 0, 2.5f, 0));
            result.Add(new DisruptorPlacement(DisruptorKind.BeachHunter, 0, 3.5f, 1));
            result.Add(new DisruptorPlacement(DisruptorKind.LifeguardChief, 0, BeachTowerX));
        }

        private static void AddNightMarket(
            List<DisruptorPlacement> result)
        {
            for (int i = 0;
                 i < NightMarketStallX.Length;
                 i++)
            {
                result.Add(new DisruptorPlacement(DisruptorKind.StallTout, 0, NightMarketStallX[i], i));
            }

            result.Add(new DisruptorPlacement(DisruptorKind.Drunkard, 0, -6f));
            result.Add(new DisruptorPlacement(DisruptorKind.Drunkard, 0, 6f));
            result.Add(new DisruptorPlacement(DisruptorKind.FilmCrew, 0, 0f));
            result.Add(new DisruptorPlacement(DisruptorKind.Pickpocket, 0, -11f));
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
        }
    }
}
