using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 야시장 등불과 촬영팀 조명이다 (23일차, 부록 B.3 [4]).
    ///
    /// 어둠이 켜진 장소에서는 불빛 안에서만 최면 사거리가 온전하고, 밖에서는 줄어든다.
    /// 어둠 여부는 부트스트랩이 장소마다 <see cref="SetDarkness"/>로 정한다.
    /// </summary>
    public sealed class LanternLight : MonoBehaviour
    {
        private static readonly List<LanternLight> Active =
            new List<LanternLight>();

        private static bool _darkness;

        private SpriteRenderer _glow;

        public float Radius { get; private set; }

        public static bool DarknessActive =>
            _darkness;

        public static int ActiveCount =>
            Active.Count;

        public static void SetDarkness(
            bool active)
        {
            _darkness = active;
        }

        public static LanternLight Create(
            Transform parent,
            string name,
            Vector2 worldPosition,
            float radius,
            Color color,
            bool withPost)
        {
            GameObject go = new GameObject(name);

            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);

            LanternLight light = go.AddComponent<LanternLight>();

            light.Radius = Mathf.Max(0f, radius);

            light._glow =
                LocationProps.Blob(
                    go.transform,
                    "Glow",
                    worldPosition,
                    new Vector2(
                        light.Radius * 2f,
                        light.Radius * 2f * LanternLogic.VerticalRatio),
                    color,
                    -50);

            if (withPost)
            {
                // 등불 알맹이는 빛 위쪽 허공에 매단다.
                LocationProps.Blob(
                    go.transform,
                    "Bulb",
                    worldPosition + new Vector2(0f, 2.9f),
                    new Vector2(0.45f, 0.55f),
                    new Color(1.00f, 0.60f, 0.25f),
                    -44);
            }

            return light;
        }

        public void SetColor(
            Color color)
        {
            if (_glow != null &&
                _glow.color != color)
            {
                _glow.color = color;
            }
        }

        public bool Contains(
            Vector2 worldPosition)
        {
            Vector2 center = transform.position;

            return AreaLogic.IsInsideEllipse(
                worldPosition.x - center.x,
                worldPosition.y - center.y,
                Radius,
                Radius * LanternLogic.VerticalRatio);
        }

        public static bool IsLit(
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

        /// <summary>최면 사거리 배율이다. 어둠이 없는 장소에서는 항상 1이다.</summary>
        public static float GetRangeMultiplier(
            Vector2 worldPosition)
        {
            if (!_darkness)
            {
                return 1f;
            }

            return LanternLogic.GetRangeMultiplier(
                true,
                IsLit(worldPosition),
                BalanceOverrides.StageOrDefault.DarkHypnosisRangeScale);
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
            _darkness = false;
        }
    }
}
