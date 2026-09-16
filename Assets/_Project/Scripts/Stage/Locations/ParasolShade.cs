using System.Collections.Generic;
using UnityEngine;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 해변가 파라솔 그늘이다 (23일차, 부록 B.3 [1]).
    /// 그늘 안의 플레이어는 라이프가드 시야에 잡히지 않는다. 탁 트인 해변에서 유일하게 숨을 곳이다.
    /// </summary>
    public sealed class ParasolShade : MonoBehaviour
    {
        public const float RadiusX = 1.6f;
        public const float RadiusY = 0.9f;

        private static readonly List<ParasolShade> Active =
            new List<ParasolShade>();

        public static int ActiveCount =>
            Active.Count;

        public void Configure(
            int floor,
            float x,
            float localY,
            Color canopy)
        {
            Vector2 center =
                FloorSpace.ToWorld(
                    floor,
                    new Vector2(x, localY));

            transform.position =
                new Vector3(center.x, center.y, 0f);

            LocationProps.Blob(
                transform,
                "Shade",
                center,
                new Vector2(RadiusX * 2f, RadiusY * 2f),
                new Color(0.05f, 0.08f, 0.20f, 0.32f),
                -55);

            LocationProps.ParasolCanopy(
                transform,
                center,
                canopy);
        }

        public bool Contains(
            Vector2 worldPosition)
        {
            Vector2 center = transform.position;

            return AreaLogic.IsInsideEllipse(
                worldPosition.x - center.x,
                worldPosition.y - center.y,
                RadiusX,
                RadiusY);
        }

        public static bool IsShaded(
            Vector2 worldPosition)
        {
            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i].Contains(worldPosition))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active.Clear();
        }
    }
}
