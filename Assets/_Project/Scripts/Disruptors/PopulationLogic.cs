using System;
using System.Collections.Generic;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 방해 세력 인원 제한이다 (24일차).
    ///
    ///   한 층에 동시에 있는 방해 세력은 최대 4명이다 (사물인 안내 방송실은 세지 않는다).
    ///   판이 시작되면 0명에서 출발해, 일정 시간마다 층당 허용 인원이 1명씩 늘어난다.
    ///   배치표 순서대로 나오므로, 배치표 앞쪽이 먼저 등장한다.
    /// 열차 행인 · 비상 증원도 이 제한 안에서만 나온다.
    /// </summary>
    public static class PopulationLogic
    {
        public const int MaxPerFloor = 4;

        /// <summary>추격 중에도 넘지 않는 층당 인원이다 (34일차). 밸런스 값이 커도 여기서 막는다.</summary>
        public const int HardMaxPerFloor = 8;

        /// <summary>허용 인원이 1명 늘어나는 간격의 기본값이다. 20초면 80초에 최대가 된다.</summary>
        public const float DefaultRampSeconds = 20f;

        /// <summary>플레이어가 이 거리 안에 있으면 그 자리에 바로 나타나지 않고 기다린다.</summary>
        public const float SpawnClearDistance = 3f;

        /// <summary>지금 한 층에 허용되는 인원이다. 간격이 0 이하면 처음부터 최대다.</summary>
        public static int GetAllowed(
            float elapsed,
            float rampSeconds)
        {
            if (float.IsNaN(rampSeconds) ||
                rampSeconds <= 0f)
            {
                return MaxPerFloor;
            }

            if (float.IsNaN(elapsed) ||
                elapsed <= 0f)
            {
                return 0;
            }

            return (int)Math.Min(
                MaxPerFloor,
                Math.Floor(elapsed / rampSeconds));
        }

        /// <summary>
        /// 추격을 반영한 허용 인원이다 (34일차).
        ///   목표를 채운 뒤(탈출 가능)에는 곧바로 1명이 더 허용되고,
        ///   추격 간격마다 1명씩 늘어 <paramref name="chaseMax"/>까지 간다.
        /// </summary>
        public static int GetAllowed(
            float elapsed,
            float rampSeconds,
            bool chasing,
            float chaseElapsed,
            float chaseRampSeconds,
            int chaseMax)
        {
            int normal =
                GetAllowed(
                    elapsed,
                    rampSeconds);

            if (!chasing)
            {
                return normal;
            }

            int cap =
                GetCap(
                    true,
                    chaseMax);

            if (float.IsNaN(chaseRampSeconds) ||
                chaseRampSeconds <= 0f)
            {
                return cap;
            }

            double steps =
                float.IsNaN(chaseElapsed) ||
                chaseElapsed <= 0f
                    ? 0d
                    : Math.Floor(chaseElapsed / chaseRampSeconds);

            return (int)Math.Min(
                cap,
                normal + 1d + steps);
        }

        /// <summary>층당 최대 인원이다. 추격 중이면 밸런스 값(4~8)을 쓴다.</summary>
        public static int GetCap(
            bool chasing,
            int chaseMax)
        {
            return chasing
                ? Math.Max(
                    MaxPerFloor,
                    Math.Min(
                        HardMaxPerFloor,
                        chaseMax))
                : MaxPerFloor;
        }

        /// <summary>더 내보낼 수 있는 인원이다.</summary>
        public static int GetFreeSlots(
            int allowed,
            int alive)
        {
            int cap =
                Math.Min(
                    HardMaxPerFloor,
                    Math.Max(0, allowed));

            return Math.Max(0, cap - Math.Max(0, alive));
        }

        /// <summary>배치표에서 한 층에 세는 인원이다. 사물은 빼고 센다.</summary>
        public static int CountOnFloor(
            IReadOnlyList<DisruptorPlacement> placements,
            int floor)
        {
            int count = 0;

            if (placements == null)
            {
                return count;
            }

            for (int i = 0;
                 i < placements.Count;
                 i++)
            {
                if (placements[i].Floor == floor &&
                    !DisruptorCatalog.Get(placements[i].Kind).IsObject)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
