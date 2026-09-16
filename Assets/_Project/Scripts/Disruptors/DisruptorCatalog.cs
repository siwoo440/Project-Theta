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

    /// <summary>방해 세력 종류다. 22일차에는 기업 연수원 2종만 있고, 23일차부터 장소마다 늘어난다.</summary>
    public enum DisruptorKind
    {
        TrainingAssistant = 0,
        HrEvaluator = 1
    }

    /// <summary>특수 능력 종류다 (부록 C.4).</summary>
    public enum SpecialAbilityKind
    {
        None = 0,
        AttendanceCheck = 1
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
    }

    /// <summary>한 장소에 어떤 방해 세력을 몇 층에 둘지다.</summary>
    public struct DisruptorPlacement
    {
        public DisruptorKind Kind;
        public int Floor;
        public float X;

        public DisruptorPlacement(
            DisruptorKind kind,
            int floor,
            float x)
        {
            Kind = kind;
            Floor = floor;
            X = x;
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
            }
        };

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
        /// 장소별 배치다. 연수원: 교육 조교 층마다 1명, 인사팀 평가관 3F · 4F에 1명씩 (부록 C.5).
        /// 1F는 튜토리얼 층이라 조교만 둔다.
        /// </summary>
        public static List<DisruptorPlacement> GetPlacements(
            LocationId location,
            int floorCount)
        {
            List<DisruptorPlacement> result =
                new List<DisruptorPlacement>();

            if (location != LocationId.TrainingCenter)
            {
                return result;
            }

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

            return result;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Override = null;
        }
    }
}
