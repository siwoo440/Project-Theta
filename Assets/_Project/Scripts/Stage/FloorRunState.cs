using UnityEngine;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 한 판 동안 살아 있는 층 이동 기록이다.
    ///
    /// 층은 오르내릴 수 있으므로 같은 층을 여러 번 지나간다.
    /// 보상을 반복해서 받는 것을 막기 위해 "처음 도달"을 따로 기록한다.
    /// </summary>
    public sealed class FloorRunState
    {
        private readonly bool[] _visited;

        public FloorRunState(
            int floorCount)
        {
            FloorCount =
                Mathf.Max(
                    1,
                    floorCount);

            _visited =
                new bool[FloorCount];

            CurrentFloor = 0;

            _visited[0] = true;

            HighestReached = 0;

            VisitedCount = 1;
        }

        public int FloorCount { get; }

        public int CurrentFloor { get; private set; }

        /// <summary>지금까지 올라간 가장 높은 층이다. 결과 화면에 쓴다.</summary>
        public int HighestReached { get; private set; }

        public int VisitedCount { get; private set; }

        public bool HasUpStair =>
            FloorPlanLogic.HasUpStair(
                CurrentFloor,
                FloorCount);

        public bool HasDownStair =>
            FloorPlanLogic.HasDownStair(
                CurrentFloor);

        public bool IsVisited(
            int index)
        {
            return index >= 0 &&
                   index < FloorCount &&
                   _visited[index];
        }

        /// <summary>이동할 수 있는 층인지 본다.</summary>
        public bool CanMoveTo(
            int index)
        {
            return index >= 0 &&
                   index < FloorCount &&
                   index != CurrentFloor;
        }

        /// <summary>
        /// 층을 옮긴다. 그 층에 처음 도달했으면 true를 돌려준다.
        /// 호출한 쪽은 이 값으로 보상 지급 여부를 정한다.
        /// </summary>
        public bool MoveTo(
            int index)
        {
            if (!CanMoveTo(
                    index))
            {
                return false;
            }

            CurrentFloor = index;

            HighestReached =
                Mathf.Max(
                    HighestReached,
                    index);

            if (_visited[index])
            {
                return false;
            }

            _visited[index] = true;

            VisitedCount++;

            return true;
        }
    }
}
