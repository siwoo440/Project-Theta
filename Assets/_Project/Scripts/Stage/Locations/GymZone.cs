using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>헬스장 구역 한 칸의 자리다.</summary>
    public struct GymZoneSpec
    {
        public GymZoneKind Kind;
        public int Floor;
        public float MinX;
        public float MaxX;

        public GymZoneSpec(
            GymZoneKind kind,
            int floor,
            float minX,
            float maxX)
        {
            Kind = kind;
            Floor = floor;
            MinX = minX;
            MaxX = maxX;
        }

        public float CenterX =>
            (MinX + MaxX) * 0.5f;
    }

    /// <summary>
    /// 헬스장 구역 배치다 (24일차). 방해 세력 배치 · 특수 대상 선수 자리와 맞춘다.
    ///   1F  운동 구역(러닝머신) 왼쪽, 요가실 오른쪽
    ///   2F  수영장 왼쪽, 운동 구역 오른쪽
    /// </summary>
    public static class GymLayout
    {
        public static readonly GymZoneSpec[] Zones =
        {
            new GymZoneSpec(GymZoneKind.Workout, 0, -10f, -1.5f),
            new GymZoneSpec(GymZoneKind.Yoga, 0, 2f, 9f),
            new GymZoneSpec(GymZoneKind.Pool, 1, -10f, 0.5f),
            new GymZoneSpec(GymZoneKind.Workout, 1, 3.5f, 10.5f)
        };

        /// <summary>층마다 특수 대상 선수가 서는 자리다. 그 층 운동 구역 가운데다.</summary>
        public static float GetAthleteX(
            int floor)
        {
            for (int i = 0;
                 i < Zones.Length;
                 i++)
            {
                if (Zones[i].Floor == floor &&
                    Zones[i].Kind == GymZoneKind.Workout)
                {
                    return Zones[i].CenterX;
                }
            }

            return 0f;
        }
    }

    /// <summary>
    /// 헬스장 구역이다 (24일차, 부록 B.3 [3] 심박 규칙).
    /// 서 있는 구역에 따라 최면 속도와 동행자 충동 속도가 달라진다. 단체 PT 상태도 여기서 들고 있다.
    /// </summary>
    public sealed class GymZone : MonoBehaviour
    {
        private static readonly List<GymZone> Active =
            new List<GymZone>();

        private static float _ptRemaining;

        private GymZoneSpec _spec;
        private SpriteRenderer _floorTint;

        public GymZoneKind Kind =>
            _spec.Kind;

        public int Floor =>
            _spec.Floor;

        public static int ActiveCount =>
            Active.Count;

        public static bool IsPtActive =>
            _ptRemaining > 0f;

        public static float PtRemaining =>
            Mathf.Max(0f, _ptRemaining);

        public static void BeginPt(
            float seconds)
        {
            _ptRemaining =
                Mathf.Max(
                    _ptRemaining,
                    seconds);
        }

        public static void TickPt(
            float deltaTime)
        {
            if (_ptRemaining > 0f)
            {
                _ptRemaining -= deltaTime;
            }
        }

        /// <summary>새 구역을 지을 때 지난 구역의 PT 상태를 지운다.</summary>
        public static void ResetState()
        {
            _ptRemaining = 0f;
        }

        public void Configure(
            GymZoneSpec spec)
        {
            _spec = spec;

            float top = FloorSpace.WalkMaxY;
            float bottom = FloorSpace.WalkMinY;

            Vector2 center =
                FloorSpace.ToWorld(
                    spec.Floor,
                    new Vector2(spec.CenterX, (top + bottom) * 0.5f));

            transform.position =
                new Vector3(center.x, center.y, 0f);

            _floorTint =
                LocationProps.Box(
                    transform,
                    "ZoneFloor",
                    center,
                    new Vector2(spec.MaxX - spec.MinX, top - bottom),
                    GetColor(spec.Kind, false),
                    -57);

            LocationProps.Sign(
                transform,
                spec.Floor,
                new Vector2(spec.CenterX, top + 1.1f),
                GetLabel(spec.Kind),
                GetColor(spec.Kind, true));

            BuildProps();
        }

        private void BuildProps()
        {
            float top = FloorSpace.WalkMaxY;

            switch (_spec.Kind)
            {
                case GymZoneKind.Workout:
                    // 안쪽 벽을 따라 러닝머신을 늘어놓는다.
                    for (float x = _spec.MinX + 1f; x < _spec.MaxX - 0.5f; x += 2f)
                    {
                        LocationProps.Structure(
                            transform,
                            "Treadmill",
                            FloorSpace.ToWorld(_spec.Floor, new Vector2(x, top + 0.3f)),
                            new Vector2(1.3f, 0.5f),
                            new Color(0.25f, 0.25f, 0.30f),
                            -45);
                    }
                    break;

                case GymZoneKind.Yoga:
                    for (float x = _spec.MinX + 1f; x < _spec.MaxX - 0.5f; x += 1.8f)
                    {
                        LocationProps.Box(
                            transform,
                            "YogaMat",
                            FloorSpace.ToWorld(_spec.Floor, new Vector2(x, -1.2f)),
                            new Vector2(1.2f, 0.45f),
                            new Color(0.70f, 0.55f, 0.85f, 0.55f),
                            -54);
                    }
                    break;
            }
        }

        private void Update()
        {
            if (_floorTint == null ||
                _spec.Kind != GymZoneKind.Workout)
            {
                return;
            }

            Color target =
                IsPtActive
                    ? new Color(1.00f, 0.55f, 0.20f, 0.22f)
                    : GetColor(GymZoneKind.Workout, false);

            if (_floorTint.color != target)
            {
                _floorTint.color = target;
            }
        }

        public bool Contains(
            Vector2 worldPosition)
        {
            if (FloorSpace.FloorAt(worldPosition.y) != _spec.Floor)
            {
                return false;
            }

            return worldPosition.x >= _spec.MinX &&
                   worldPosition.x <= _spec.MaxX;
        }

        public static GymZoneKind? GetZone(
            Vector2 worldPosition)
        {
            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i].Contains(worldPosition))
                {
                    return Active[i].Kind;
                }
            }

            return null;
        }

        /// <summary>그 자리 NPC의 최면 속도 배율이다. 헬스장이 아니면 1이다.</summary>
        public static float GetHypnosisMultiplier(
            Vector2 worldPosition)
        {
            if (Active.Count == 0)
            {
                return 1f;
            }

            return HeartRateLogic.GetHypnosisMultiplier(
                GetZone(worldPosition),
                IsPtActive,
                CityAbilityValues.PtWorkoutHypnosisMultiplier);
        }

        /// <summary>그 자리 동행자의 충동 속도 배율이다. 헬스장이 아니면 1이다.</summary>
        public static float GetImpulseMultiplier(
            Vector2 worldPosition)
        {
            if (Active.Count == 0)
            {
                return 1f;
            }

            return HeartRateLogic.GetImpulseMultiplier(
                GetZone(worldPosition),
                IsPtActive,
                CityAbilityValues.PtImpulseMultiplier,
                BalanceOverrides.StageOrDefault.HeartRateImpulseScale);
        }

        public static string GetLabel(
            GymZoneKind kind)
        {
            switch (kind)
            {
                case GymZoneKind.Workout:
                    return "운동 구역 · 최면↑ 충동↑";

                case GymZoneKind.Yoga:
                    return "요가실 · 충동 없음";

                default:
                    return "수영장";
            }
        }

        private static Color GetColor(
            GymZoneKind kind,
            bool text)
        {
            float alpha = text ? 0.95f : 0.12f;

            switch (kind)
            {
                case GymZoneKind.Workout:
                    return new Color(1.00f, 0.60f, 0.35f, alpha);

                case GymZoneKind.Yoga:
                    return new Color(0.75f, 0.60f, 1.00f, alpha);

                default:
                    return new Color(0.35f, 0.70f, 1.00f, text ? alpha : 0.30f);
            }
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
            _ptRemaining = 0f;
        }
    }
}
