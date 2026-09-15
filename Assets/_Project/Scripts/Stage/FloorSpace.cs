using UnityEngine;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 층을 같은 씬 안에서 세로로 쌓아 배치하는 좌표계다.
    ///
    /// 층마다 씬을 따로 두면 씬 로드 때문에 세션이 끊기고, 층을 오르내릴 때마다
    /// 동행 NPC를 다시 만들어야 한다. 그래서 모든 층을 한 씬에 미리 지어 두고,
    /// 계단은 플레이어와 동행자를 목표 층의 같은 자리로 옮기기만 한다.
    ///
    /// 층 사이 간격은 카메라 시야보다 넉넉히 넓어서 위아래 층이 겹쳐 보이지 않는다.
    /// </summary>
    public static class FloorSpace
    {
        /// <summary>층과 층 사이의 세로 간격이다. 카메라 시야(약 9.3)보다 넓게 잡았다.</summary>
        public const float FloorHeight = 16f;

        // 1층 기준 보행 구역. 위층은 여기에 층 간격을 더한 자리가 된다.
        public const float WalkMinX = -17.4f;
        public const float WalkMaxX = 17.4f;
        public const float WalkMinY = -5.2f;
        public const float WalkMaxY = 0.9f;

        /// <summary>해당 층의 원점이 1층 기준으로 얼마나 올라가 있는지다.</summary>
        public static float OriginY(
            int floorIndex)
        {
            return floorIndex *
                   FloorHeight;
        }

        public static float MinYOn(
            int floorIndex)
        {
            return WalkMinY +
                   OriginY(
                       floorIndex);
        }

        public static float MaxYOn(
            int floorIndex)
        {
            return WalkMaxY +
                   OriginY(
                       floorIndex);
        }

        /// <summary>세로 좌표만 보고 몇 층에 있는지 되돌린다.</summary>
        public static int FloorAt(
            float worldY)
        {
            float bandCenter =
                (WalkMinY + WalkMaxY) *
                0.5f;

            return Mathf.RoundToInt(
                (worldY - bandCenter) /
                FloorHeight);
        }

        /// <summary>
        /// 같은 층 안으로 세로 좌표를 가둔다.
        /// 층을 넘나드는 것은 계단으로만 가능해야 하므로, 걷기로는 못 넘어간다.
        ///
        /// 층은 반드시 <paramref name="referenceY"/>(지금 서 있는 자리)로 정한다.
        /// 가두려는 값으로 층을 정하면, 값이 크게 밀렸을 때 그 값이 위층으로 읽혀
        /// 오히려 층을 빠져나가는 통로가 된다.
        /// </summary>
        public static float ClampYNear(
            float referenceY,
            float desiredY,
            float padBottom,
            float padTop)
        {
            return ClampYOn(
                FloorAt(
                    referenceY),
                desiredY,
                padBottom,
                padTop);
        }

        /// <summary>지정한 층 안에서 세로 좌표를 가둔다.</summary>
        public static float ClampYOn(
            int floorIndex,
            float worldY,
            float padBottom,
            float padTop)
        {
            return Mathf.Clamp(
                worldY,
                MinYOn(
                    floorIndex) +
                padBottom,
                MaxYOn(
                    floorIndex) -
                padTop);
        }

        /// <summary>1층 기준 좌표를 해당 층의 세계 좌표로 옮긴다.</summary>
        public static Vector2 ToWorld(
            int floorIndex,
            Vector2 localPosition)
        {
            return new Vector2(
                localPosition.x,
                localPosition.y +
                OriginY(
                    floorIndex));
        }

        /// <summary>세계 좌표를 1층 기준 좌표로 되돌린다.</summary>
        public static Vector2 ToLocal(
            int floorIndex,
            Vector2 worldPosition)
        {
            return new Vector2(
                worldPosition.x,
                worldPosition.y -
                OriginY(
                    floorIndex));
        }
    }
}
