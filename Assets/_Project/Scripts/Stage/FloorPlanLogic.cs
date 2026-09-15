using UnityEngine;
using ProjectTheta.NPC;

namespace ProjectTheta.Stage
{
    /// <summary>계단이 향하는 방향이다.</summary>
    public enum FloorStairDirection
    {
        Up = 0,
        Down = 1
    }

    /// <summary>
    /// 한 층의 구성이다.
    ///
    /// 층마다 씬을 따로 두지 않는다. 같은 복도를 색조와 NPC 구성만 바꿔 다시 짓는다.
    /// 씬을 로드하면 세션 상태가 끊기고, 13일차에 맞춘 씬 흐름과 이중으로 얽힌다.
    /// </summary>
    public struct FloorDefinition
    {
        public int Index;
        public string Label;
        public int NpcCount;

        /// <summary>층이 올라갈수록 벽 색조를 조금씩 차갑게 바꿔 층을 구분한다.</summary>
        public Color WallTint;

        public bool HasUpStair;
        public bool HasDownStair;
    }

    /// <summary>
    /// 한 판을 이루는 층 구성 규칙이다.
    ///
    /// 기획서 14.5절의 구역 개념을 층으로 옮겼다.
    /// 구역과 달리 층은 오르내릴 수 있으므로 되돌아갈 수 있고,
    /// 그래서 보상은 "처음 도달"에만 준다.
    /// </summary>
    public static class FloorPlanLogic
    {
        public const int DefaultFloorCount = 4;

        /// <summary>1층의 NPC 수다. 위층으로 갈수록 조금씩 늘어난다.</summary>
        public const int BaseNpcCount = 10;

        public const int NpcCountPerFloor = 2;

        public const int MaximumNpcCount = 18;

        public static int ClampFloor(
            int index,
            int floorCount)
        {
            int safeCount =
                Mathf.Max(
                    1,
                    floorCount);

            return Mathf.Clamp(
                index,
                0,
                safeCount - 1);
        }

        /// <summary>화면에 쓰는 층 이름이다. 0번 층이 1F다.</summary>
        public static string GetLabel(
            int index)
        {
            return $"{index + 1}F";
        }

        public static bool HasUpStair(
            int index,
            int floorCount)
        {
            return index <
                   Mathf.Max(
                       1,
                       floorCount) - 1;
        }

        public static bool HasDownStair(
            int index)
        {
            return index > 0;
        }

        public static int GetNpcCount(
            int index)
        {
            return Mathf.Clamp(
                BaseNpcCount +
                (Mathf.Max(
                     0,
                     index) *
                 NpcCountPerFloor),
                1,
                MaximumNpcCount);
        }

        /// <summary>
        /// 층이 올라갈수록 높은 등급이 섞인다.
        /// 1층은 연습용, 위층은 정기 효율이 좋지만 특성이 까다로워진다.
        /// </summary>
        public static NpcGrade[] BuildGrades(
            int index,
            int count)
        {
            int safeCount =
                Mathf.Max(
                    1,
                    count);

            NpcGrade[] grades =
                new NpcGrade[safeCount];

            int floor =
                Mathf.Max(
                    0,
                    index);

            for (int i = 0;
                 i < safeCount;
                 i++)
            {
                grades[i] =
                    PickGrade(
                        floor,
                        i);
            }

            return grades;
        }

        /// <summary>
        /// 난수를 쓰지 않고 자리 번호로 등급을 정한다.
        /// 같은 층은 항상 같은 구성이 되어, 밸런스를 읽고 조정하기 쉽다.
        /// </summary>
        private static NpcGrade PickGrade(
            int floor,
            int slot)
        {
            // 층 번호를 섞어 층마다 고급 NPC가 다른 자리에 오게 한다.
            int shifted =
                slot +
                floor;

            if (floor >= 3 &&
                shifted % 9 == 0)
            {
                return NpcGrade.Awakened;
            }

            if (floor >= 2 &&
                shifted % 7 == 0)
            {
                return NpcGrade.Special;
            }

            if (floor >= 1 &&
                shifted % 5 == 0)
            {
                return NpcGrade.Rare;
            }

            if (shifted % 3 == 0)
            {
                return NpcGrade.Skilled;
            }

            // 1층에도 희귀 한 명은 둬서 등급 차이를 처음부터 보여 준다.
            if (floor == 0 &&
                slot == 8)
            {
                return NpcGrade.Rare;
            }

            return NpcGrade.Common;
        }

        public static Color GetWallTint(
            int index)
        {
            // 1F 따뜻한 미색 → 위층으로 갈수록 푸르게.
            float t =
                Mathf.Clamp01(
                    index * 0.26f);

            return Color.Lerp(
                new Color(1.00f, 1.00f, 1.00f),
                new Color(0.78f, 0.86f, 1.00f),
                t);
        }

        public static FloorDefinition GetDefinition(
            int index,
            int floorCount)
        {
            int safe =
                ClampFloor(
                    index,
                    floorCount);

            return new FloorDefinition
            {
                Index = safe,
                Label =
                    GetLabel(
                        safe),
                NpcCount =
                    GetNpcCount(
                        safe),
                WallTint =
                    GetWallTint(
                        safe),
                HasUpStair =
                    HasUpStair(
                        safe,
                        floorCount),
                HasDownStair =
                    HasDownStair(
                        safe)
            };
        }
    }
}
