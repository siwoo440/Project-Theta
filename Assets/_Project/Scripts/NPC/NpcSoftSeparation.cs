using System.Collections.Generic;
using UnityEngine;

namespace ProjectTheta.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class NpcSoftSeparation : MonoBehaviour
    {
        [SerializeField] private float _desiredDistance = 0.88f;
        [SerializeField] private float _maximumPushSpeed = 1.20f;

        private static readonly List<NpcSoftSeparation> Active =
            new List<NpcSoftSeparation>();

        /// <summary>
        /// 이번 프레임에 한 번만 읽은 위치다.
        ///
        /// 밀어내기는 NPC마다 다른 NPC 전부와 거리를 잰다 (52명이면 한 프레임에 2,704쌍).
        /// 이전에는 쌍마다 상대의 transform.position을 새로 읽었다.
        /// 프레임당 한 번만 모두 읽어 두고, 그 배열로 계산한다.
        /// </summary>
        private static Vector2[] _positions =
            new Vector2[64];

        private static int _snapshotFrame = -1;

        /// <summary>스냅샷을 찍을 때의 인원이다. 그 뒤 새로 켜진 NPC는 다음 스냅샷부터 계산에 들어간다.</summary>
        private static int _snapshotCount;

        /// <summary>이 NPC가 목록에서 몇 번째인지다. 스냅샷을 찍을 때 갱신한다.</summary>
        private int _snapshotIndex = -1;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            Active.Clear();

            _snapshotFrame = -1;
            _snapshotCount = 0;
        }

        private static void EnsureSnapshot()
        {
            int frame =
                Time.frameCount;

            if (_snapshotFrame == frame)
            {
                return;
            }

            _snapshotFrame = frame;

            RemoveMissingEntries();

            _snapshotCount = Active.Count;

            if (_positions.Length < Active.Count)
            {
                _positions =
                    new Vector2[
                        Mathf.NextPowerOfTwo(
                            Active.Count)];
            }

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                NpcSoftSeparation entry =
                    Active[i];

                entry._snapshotIndex = i;

                _positions[i] =
                    entry.transform.position;
            }
        }

        private Collider2D _collider;

        private void Awake()
        {
            _collider =
                GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            RemoveMissingEntries();

            if (_collider == null)
            {
                _collider =
                    GetComponent<Collider2D>();
            }

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                NpcSoftSeparation other =
                    Active[i];

                if (other == null ||
                    other._collider == null ||
                    _collider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    _collider,
                    other._collider,
                    true);
            }

            if (!Active.Contains(this))
            {
                Active.Add(this);
            }

            // 목록이 바뀌었으므로 이번 프레임 스냅샷을 무효로 한다.
            // 그러지 않으면 새 NPC 자리에 이전 위치값이 남아 한 프레임 엉뚱하게 밀린다.
            _snapshotFrame = -1;
        }

        private void OnDisable()
        {
            Active.Remove(this);

            // 목록 순서가 바뀌었으므로 이번 프레임 스냅샷을 무효로 한다.
            _snapshotFrame = -1;
        }

        public Vector2 GetCorrectionVelocity()
        {
            EnsureSnapshot();

            Vector2 origin =
                transform.position;

            Vector2 push =
                Vector2.zero;

            float desired =
                _desiredDistance;

            float desiredSquared =
                desired *
                desired;

            for (int i = 0;
                 i < _snapshotCount;
                 i++)
            {
                if (i == _snapshotIndex)
                {
                    continue;
                }

                Vector2 delta =
                    origin -
                    _positions[i];

                // 세로가 밀어내기 거리보다 멀면 제곱근을 구하기 전에 버린다.
                // 다른 층 NPC(세로 16 이상 차이)는 전부 여기서 걸러진다.
                if (delta.y > desired ||
                    delta.y < -desired)
                {
                    continue;
                }

                float distanceSquared =
                    delta.sqrMagnitude;

                if (distanceSquared >= desiredSquared)
                {
                    continue;
                }

                NpcSoftSeparation other =
                    Active[i];

                float distance =
                    Mathf.Sqrt(
                        distanceSquared);

                float weight =
                    NpcSoftSeparationLogic.ComputeWeight(
                        distance,
                        _desiredDistance);

                if (weight <= 0f)
                {
                    continue;
                }

                Vector2 direction;

                if (distance <= 0.001f)
                {
                    direction =
                        GetFallbackDirection(
                            other);
                }
                else
                {
                    direction =
                        delta /
                        distance;
                }

                push +=
                    direction *
                    weight;
            }

            if (push.sqrMagnitude <=
                0.0001f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(
                push * _maximumPushSpeed,
                _maximumPushSpeed);
        }

        private Vector2 GetFallbackDirection(
            NpcSoftSeparation other)
        {
            int sign =
                GetInstanceID() <
                other.GetInstanceID()
                    ? -1
                    : 1;

            return new Vector2(
                sign,
                sign * 0.35f).normalized;
        }

        private static void RemoveMissingEntries()
        {
            for (int i = Active.Count - 1;
                 i >= 0;
                 i--)
            {
                if (Active[i] == null)
                {
                    Active.RemoveAt(i);
                }
            }
        }

        private void OnDestroy()
        {
            _snapshotIndex = -1;
        }
    }
}
